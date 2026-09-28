import { isRouteErrorResponse, Link, useRouteError } from "react-router-dom";
import { EmptyState } from "./EmptyState";

/** Shown by the router when a page throws while rendering or its code chunk fails to load. */
export function RouteErrorBoundary() {
  const error = useRouteError();
  if (import.meta.env.DEV) console.error(error);

  const notFound = isRouteErrorResponse(error) && error.status === 404;
  const chunkFailed = error instanceof Error && /dynamically imported module|Loading chunk/i.test(error.message);

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 p-6 dark:bg-gray-900">
      <div className="card w-full max-w-lg">
        <EmptyState
          variant="error"
          title={notFound ? "Page not found" : chunkFailed ? "A new version is available" : "This page stopped working"}
          description={
            chunkFailed
              ? "Reload to get the latest version of the app."
              : "The problem has been logged. You can reload the page or go back to the module list."
          }
          action={
            <div className="flex items-center justify-center gap-2">
              <button type="button" className="btn btn-sm bg-primary text-white" onClick={() => window.location.reload()}>
                Reload
              </button>
              <Link to="/modules" className="btn btn-sm border border-slate-300 dark:border-slate-600">
                Go to modules
              </Link>
            </div>
          }
        />
      </div>
    </div>
  );
}
