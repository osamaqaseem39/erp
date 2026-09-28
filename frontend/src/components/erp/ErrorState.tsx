import type { ApiErrorInfo } from "../../lib/erp/apiError";
import { EmptyState } from "./EmptyState";
import { PermissionDenied } from "./PermissionDenied";

export interface ErrorStateProps {
  error: ApiErrorInfo | null | undefined;
  onRetry?: () => void;
  title?: string;
  compact?: boolean;
}

/**
 * Renders a failed load. 403 shows the permission state, 501 the not-available state,
 * anything else the message plus a Retry button and the trace id for support.
 */
export function ErrorState({ error, onRetry, title = "Unable to load data", compact }: ErrorStateProps) {
  if (error?.status === 403) return <PermissionDenied compact={compact} />;
  if (error?.status === 501) return <EmptyState variant="unavailable" compact={compact} />;

  return (
    <EmptyState
      variant="error"
      compact={compact}
      title={title}
      description={
        <>
          {error?.message ?? "Something went wrong."}
          {error?.traceId && <span className="mt-1 block text-xs text-slate-400">Reference: {error.traceId}</span>}
        </>
      }
      action={
        onRetry && (
          <button type="button" className="btn btn-sm bg-primary text-white" onClick={onRetry}>
            <i className="mgc_refresh_1_line me-1" aria-hidden="true" />
            Retry
          </button>
        )
      }
    />
  );
}
