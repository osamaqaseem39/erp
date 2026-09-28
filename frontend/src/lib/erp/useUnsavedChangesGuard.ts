import { useCallback, useEffect } from "react";
import { useBlocker, type BlockerFunction } from "react-router-dom";

export interface UnsavedChangesGuard {
  /** True while an in-app navigation is waiting for the user's decision. */
  isBlocked: boolean;
  /** Leave anyway (discard changes). */
  proceed: () => void;
  /** Stay on the page. */
  stay: () => void;
}

/**
 * Warns before leaving a page with unsaved changes: in-app navigation is paused until the user
 * decides (render a ConfirmDialog from `isBlocked`), and closing/reloading the tab triggers the
 * browser's own prompt. Navigation that only changes the query string (e.g. switching tabs) is allowed.
 */
export function useUnsavedChangesGuard(isDirty: boolean): UnsavedChangesGuard {
  const shouldBlock = useCallback<BlockerFunction>(
    ({ currentLocation, nextLocation }) => isDirty && currentLocation.pathname !== nextLocation.pathname,
    [isDirty]
  );
  const blocker = useBlocker(shouldBlock);

  useEffect(() => {
    if (!isDirty) return;
    const onBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      // Required by some browsers to show the prompt.
      event.returnValue = "";
    };
    window.addEventListener("beforeunload", onBeforeUnload);
    return () => window.removeEventListener("beforeunload", onBeforeUnload);
  }, [isDirty]);

  // If the form becomes clean while blocked (e.g. saved), let the pending navigation through.
  useEffect(() => {
    if (blocker.state === "blocked" && !isDirty) blocker.proceed();
  }, [blocker, isDirty]);

  return {
    isBlocked: blocker.state === "blocked",
    proceed: () => blocker.proceed?.(),
    stay: () => blocker.reset?.(),
  };
}
