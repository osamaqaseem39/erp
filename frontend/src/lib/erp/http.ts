import { useCallback, useEffect, useRef, useState } from "react";
import { parseApiError, type ApiErrorInfo } from "./apiError";
import type { PagedQuery, PagedResult } from "./paging";

/**
 * Small data hooks for entity pages. Entity data lives in component state, not Redux.
 *
 * - Requests are keyed by their arguments (compared by value), so passing a new object with the
 *   same content does not refetch.
 * - A newer request always wins: older responses are dropped and their requests aborted.
 * - While refetching, the previous data stays on screen (`isRefreshing`) to avoid table flicker.
 */

export type Fetcher<TArgs, TData> = (args: TArgs, signal: AbortSignal) => Promise<TData>;

export type ResourceStatus = "idle" | "loading" | "success" | "error";

export interface ResourceState<TData> {
  data: TData | undefined;
  error: ApiErrorInfo | null;
  status: ResourceStatus;
  /** First load with nothing to show yet. */
  isLoading: boolean;
  /** Reloading while previous data is still shown. */
  isRefreshing: boolean;
  refetch: () => void;
}

export interface ResourceOptions {
  /** Skip fetching (e.g. no id yet). */
  enabled?: boolean;
}

export function useResource<TArgs, TData>(
  fetcher: Fetcher<TArgs, TData>,
  args: TArgs,
  { enabled = true }: ResourceOptions = {}
): ResourceState<TData> {
  const [data, setData] = useState<TData | undefined>(undefined);
  const [error, setError] = useState<ApiErrorInfo | null>(null);
  const [status, setStatus] = useState<ResourceStatus>(enabled ? "loading" : "idle");
  const [reloadToken, setReloadToken] = useState(0);

  // The latest fetcher/args are read through refs so callers may pass inline functions.
  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;
  const argsRef = useRef(args);
  argsRef.current = args;
  const argsKey = stableKey(args);

  useEffect(() => {
    if (!enabled) {
      setStatus("idle");
      return;
    }
    const controller = new AbortController();
    setStatus("loading");
    setError(null);

    fetcherRef
      .current(argsRef.current, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return;
        setData(result);
        setStatus("success");
      })
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        const info = parseApiError(err);
        if (info.isCanceled) return;
        setError(info);
        setStatus("error");
      });

    return () => controller.abort();
  }, [argsKey, enabled, reloadToken]);

  const refetch = useCallback(() => setReloadToken((t) => t + 1), []);

  return {
    data,
    error,
    status,
    isLoading: status === "loading" && data === undefined,
    isRefreshing: status === "loading" && data !== undefined,
    refetch,
  };
}

export function usePagedQuery<TItem, TFilters extends object>(
  fetcher: Fetcher<PagedQuery<TFilters>, PagedResult<TItem>>,
  query: PagedQuery<TFilters>,
  options?: ResourceOptions
) {
  return useResource(fetcher, query, options);
}

export type MutationResult<TData> = { ok: true; data: TData } | { ok: false; error: ApiErrorInfo };

export interface MutationState<TArgs, TData> {
  /** Runs the mutation. Calls made while one is in flight are ignored (double-submit guard). */
  mutate: (args: TArgs) => Promise<MutationResult<TData>>;
  isPending: boolean;
  error: ApiErrorInfo | null;
  reset: () => void;
}

export function useMutation<TArgs, TData>(fn: (args: TArgs) => Promise<TData>): MutationState<TArgs, TData> {
  const [isPending, setIsPending] = useState(false);
  const [error, setError] = useState<ApiErrorInfo | null>(null);
  const inFlight = useRef(false);
  const mounted = useRef(true);
  const fnRef = useRef(fn);
  fnRef.current = fn;

  useEffect(() => {
    mounted.current = true;
    return () => {
      mounted.current = false;
    };
  }, []);

  const mutate = useCallback(async (args: TArgs): Promise<MutationResult<TData>> => {
    if (inFlight.current) {
      return { ok: false, error: { ...parseApiError(null), message: "A save is already in progress.", isCanceled: true } };
    }
    inFlight.current = true;
    setIsPending(true);
    setError(null);
    try {
      const data = await fnRef.current(args);
      return { ok: true, data };
    } catch (err) {
      const info = parseApiError(err);
      if (mounted.current) setError(info);
      return { ok: false, error: info };
    } finally {
      inFlight.current = false;
      if (mounted.current) setIsPending(false);
    }
  }, []);

  const reset = useCallback(() => setError(null), []);

  return { mutate, isPending, error, reset };
}

/** Value-based key with sorted object keys, so {a,b} and {b,a} are the same request. */
export function stableKey(value: unknown): string {
  return JSON.stringify(value, (_key, v: unknown) =>
    v && typeof v === "object" && !Array.isArray(v)
      ? Object.fromEntries(Object.entries(v as Record<string, unknown>).sort(([a], [b]) => a.localeCompare(b)))
      : v
  ) ?? "undefined";
}
