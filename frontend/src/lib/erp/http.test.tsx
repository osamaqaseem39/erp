import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { stableKey, useMutation, useResource } from "./http";

const deferred = <T,>() => {
  let resolve!: (value: T) => void;
  let reject!: (reason: unknown) => void;
  const promise = new Promise<T>((res, rej) => {
    resolve = res;
    reject = rej;
  });
  return { promise, resolve, reject };
};

describe("useResource", () => {
  it("loads, then exposes data", async () => {
    const { result } = renderHook(() => useResource(async (id: number) => `row ${id}`, 1));
    expect(result.current.isLoading).toBe(true);
    await waitFor(() => expect(result.current.status).toBe("success"));
    expect(result.current.data).toBe("row 1");
  });

  it("drops a slower, older response when arguments change (newest wins)", async () => {
    const first = deferred<string>();
    const second = deferred<string>();
    const fetcher = vi.fn((page: number) => (page === 1 ? first.promise : second.promise));
    const { result, rerender } = renderHook(({ page }) => useResource(fetcher, page), { initialProps: { page: 1 } });

    rerender({ page: 2 });
    await act(async () => second.resolve("page 2"));
    await act(async () => first.resolve("page 1 (stale)"));

    expect(result.current.data).toBe("page 2");
  });

  it("aborts the previous request's signal", async () => {
    const signals: AbortSignal[] = [];
    const fetcher = (_: number, signal: AbortSignal) => {
      signals.push(signal);
      return new Promise<string>(() => {});
    };
    const { rerender, unmount } = renderHook(({ page }) => useResource(fetcher, page), { initialProps: { page: 1 } });
    rerender({ page: 2 });
    expect(signals[0].aborted).toBe(true);
    unmount();
    expect(signals[1].aborted).toBe(true);
  });

  it("does not refetch for an equal-but-new arguments object", async () => {
    const fetcher = vi.fn(async () => "ok");
    const { rerender } = renderHook(({ q }) => useResource(fetcher, q), { initialProps: { q: { a: 1, b: 2 } } });
    await waitFor(() => expect(fetcher).toHaveBeenCalledTimes(1));
    rerender({ q: { b: 2, a: 1 } });
    await Promise.resolve();
    expect(fetcher).toHaveBeenCalledTimes(1);
  });

  it("keeps previous data while refetching", async () => {
    let call = 0;
    const next = deferred<string>();
    const fetcher = () => (++call === 1 ? Promise.resolve("v1") : next.promise);
    const { result } = renderHook(() => useResource(fetcher, "x"));
    await waitFor(() => expect(result.current.data).toBe("v1"));

    act(() => result.current.refetch());
    expect(result.current.isRefreshing).toBe(true);
    expect(result.current.data).toBe("v1");
    await act(async () => next.resolve("v2"));
    expect(result.current.data).toBe("v2");
  });

  it("maps failures through parseApiError", async () => {
    const fetcher = () => Promise.reject(Object.assign(new Error("x"), { response: { status: 404, data: null } }));
    const { result } = renderHook(() => useResource(fetcher, 1));
    await waitFor(() => expect(result.current.status).toBe("error"));
    expect(result.current.error?.status).toBe(404);
  });

  it("does nothing while disabled", () => {
    const fetcher = vi.fn(async () => "x");
    const { result } = renderHook(() => useResource(fetcher, 1, { enabled: false }));
    expect(fetcher).not.toHaveBeenCalled();
    expect(result.current.status).toBe("idle");
  });
});

describe("useMutation", () => {
  it("ignores a second submit while the first is in flight", async () => {
    const pending = deferred<string>();
    const fn = vi.fn(() => pending.promise);
    const { result } = renderHook(() => useMutation(fn));

    let first!: Promise<unknown>;
    act(() => {
      first = result.current.mutate(undefined);
    });
    const second = await result.current.mutate(undefined);
    expect(second.ok).toBe(false);
    expect(fn).toHaveBeenCalledTimes(1);

    await act(async () => {
      pending.resolve("saved");
      await first;
    });
    expect(result.current.isPending).toBe(false);
  });

  it("returns the parsed error on failure", async () => {
    const fn = () => Promise.reject(Object.assign(new Error("x"), { response: { status: 409, data: { message: "Code already exists" } } }));
    const { result } = renderHook(() => useMutation(fn));
    let outcome!: Awaited<ReturnType<typeof result.current.mutate>>;
    await act(async () => {
      outcome = await result.current.mutate(undefined);
    });
    expect(outcome.ok).toBe(false);
    expect(result.current.error?.message).toBe("Code already exists");
  });
});

describe("stableKey", () => {
  it("ignores object key order", () => {
    expect(stableKey({ a: 1, b: { d: 1, c: 2 } })).toBe(stableKey({ b: { c: 2, d: 1 }, a: 1 }));
  });
});
