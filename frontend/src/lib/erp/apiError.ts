/**
 * Normalizes any failure from the API layer into one shape the UI can render.
 *
 * Understands the current backend contract (ApiError: { status, message, errorCode, errors, traceId })
 * and the legacy shapes still returned by some endpoints ({ Error }, { message }, FluentValidation
 * failure arrays, ASP.NET ProblemDetails, plain strings).
 */

export interface ApiErrorInfo {
  /** HTTP status, or null for network failures and cancellations. */
  status: number | null;
  /** A sentence safe to show to the user. */
  message: string;
  /** Machine code from the backend (e.g. VALIDATION_ERROR, BUSINESS_RULE), when provided. */
  code: string | null;
  /** camelCase field path -> messages. "" holds errors not tied to a field. */
  fieldErrors: Record<string, string[]>;
  traceId: string | null;
  isNetworkError: boolean;
  isCanceled: boolean;
}

export const DEFAULT_ERROR_MESSAGE = "Something went wrong. Please try again.";

const STATUS_MESSAGES: Record<number, string> = {
  400: "Some of the information is not valid. Please check the highlighted fields.",
  401: "Your session has expired. Please sign in again.",
  403: "You don't have permission to do this.",
  404: "This record could not be found. It may have been deleted.",
  409: "This change conflicts with the current data. Refresh and try again.",
  422: "Some of the information is not valid. Please check the highlighted fields.",
  429: "Too many requests. Please wait a moment and try again.",
  501: "This feature is not available yet.",
};

const NETWORK_MESSAGE = "Can't reach the server. Check your connection and try again.";

type UnknownRecord = Record<string, unknown>;

const isRecord = (value: unknown): value is UnknownRecord =>
  typeof value === "object" && value !== null && !Array.isArray(value);

const asString = (value: unknown): string | null =>
  typeof value === "string" && value.trim() ? value.trim() : null;

/** "CustomerCode" -> "customerCode", "Lines[0].UnitPrice" -> "lines[0].unitPrice", "$.email" -> "email". */
export function toFieldPath(key: string): string {
  return key
    .replace(/^\$\.?/, "")
    .split(".")
    .map((segment) => (segment ? segment[0].toLowerCase() + segment.slice(1) : segment))
    .join(".");
}

function addFieldError(target: Record<string, string[]>, field: string, message: string) {
  const key = toFieldPath(field);
  const existing = target[key] ?? (target[key] = []);
  if (!existing.includes(message)) existing.push(message);
}

function readFieldErrors(errors: unknown): Record<string, string[]> {
  const result: Record<string, string[]> = {};

  // Current contract / ProblemDetails: { field: string[] }
  if (isRecord(errors)) {
    for (const [field, value] of Object.entries(errors)) {
      const messages = Array.isArray(value) ? value : [value];
      for (const message of messages) {
        const text = asString(message);
        if (text) addFieldError(result, field, text);
      }
    }
    return result;
  }

  // Legacy: FluentValidation ValidationFailure[] or string[]
  if (Array.isArray(errors)) {
    for (const item of errors) {
      if (isRecord(item)) {
        const text = asString(item.errorMessage) ?? asString(item.ErrorMessage);
        const field = asString(item.propertyName) ?? asString(item.PropertyName) ?? "";
        if (text) addFieldError(result, field, text);
      } else {
        const text = asString(item);
        if (text) addFieldError(result, "", text);
      }
    }
  }
  return result;
}

interface ErrorLike {
  response?: { status?: number; data?: unknown };
  status?: number;
  message?: string;
  name?: string;
  code?: string;
}

export function parseApiError(error: unknown, fallback = DEFAULT_ERROR_MESSAGE): ApiErrorInfo {
  const err = (isRecord(error) || error instanceof Error ? error : {}) as ErrorLike;

  const isCanceled = err.name === "CanceledError" || err.name === "AbortError" || err.code === "ERR_CANCELED";
  if (isCanceled) {
    return { status: null, message: "The request was canceled.", code: null, fieldErrors: {}, traceId: null, isNetworkError: false, isCanceled: true };
  }

  const status = err.response?.status ?? (typeof err.status === "number" ? err.status : null);
  if (status === null) {
    // No HTTP response: either the server was unreachable or the failure happened in client code.
    // Client-side exceptions are bugs, so their text is not shown to the user.
    const isNetwork = err.code === "ERR_NETWORK" || /network/i.test(err.message ?? "");
    return {
      status: null,
      message: isNetwork ? NETWORK_MESSAGE : fallback,
      code: null,
      fieldErrors: {},
      traceId: null,
      isNetworkError: isNetwork,
      isCanceled: false,
    };
  }

  const body = err.response?.data;
  let message: string | null = null;
  let code: string | null = null;
  let traceId: string | null = null;
  let fieldErrors: Record<string, string[]> = {};

  if (typeof body === "string") {
    message = asString(body);
  } else if (isRecord(body)) {
    message =
      asString(body.message) ?? asString(body.Message) ?? asString(body.Error) ?? asString(body.error) ?? asString(body.title);
    code = asString(body.errorCode) ?? asString(body.ErrorCode);
    traceId = asString(body.traceId) ?? asString(body.TraceId);
    fieldErrors = readFieldErrors(body.errors ?? body.Errors);
  }

  // Server 5xx text is never shown to users; the trace id is what support needs.
  if (status >= 500 && status !== 501) {
    message = fallback;
  }

  return {
    status,
    message: message ?? STATUS_MESSAGES[status] ?? fallback,
    code,
    fieldErrors,
    traceId,
    isNetworkError: false,
    isCanceled: false,
  };
}

/** Messages not attached to a known form field, for a form-level alert. */
export function unassignedErrors(info: ApiErrorInfo, knownFields: readonly string[]): string[] {
  return Object.entries(info.fieldErrors)
    .filter(([field]) => !knownFields.includes(field))
    .flatMap(([, messages]) => messages);
}
