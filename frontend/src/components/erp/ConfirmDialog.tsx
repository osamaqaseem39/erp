import { Dialog, DialogPanel, DialogTitle, Description } from "@headlessui/react";
import { useRef, type ReactNode } from "react";

export interface ConfirmDialogProps {
  open: boolean;
  title: string;
  description?: ReactNode;
  children?: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  tone?: "primary" | "danger";
  isPending?: boolean;
  /** Disable the confirm button (e.g. until a confirmation phrase is typed). */
  confirmDisabled?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

/**
 * Accessible modal confirmation (focus trap, Escape closes, focus returns to the trigger).
 * Initial focus goes to Cancel so Enter never confirms a destructive action by accident.
 */
export function ConfirmDialog({
  open,
  title,
  description,
  children,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  tone = "primary",
  isPending = false,
  confirmDisabled = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  const cancelRef = useRef<HTMLButtonElement>(null);
  const confirmClass = tone === "danger" ? "bg-danger text-white" : "bg-primary text-white";

  return (
    <Dialog open={open} onClose={() => !isPending && onCancel()} initialFocus={cancelRef} className="relative z-[70]">
      <div className="fixed inset-0 bg-slate-900/50" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="w-full max-w-md rounded-lg bg-white shadow-xl dark:bg-gray-800">
          <div className="flex gap-4 p-5">
            {tone === "danger" && (
              <span
                className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-danger/10 text-xl text-danger"
                aria-hidden="true"
              >
                <i className="mgc_warning_line" />
              </span>
            )}
            <div className="min-w-0 flex-1">
              <DialogTitle className="text-base font-semibold text-slate-900 dark:text-slate-100">{title}</DialogTitle>
              {description && (
                <Description as="div" className="mt-1 text-sm text-slate-600 dark:text-slate-300">
                  {description}
                </Description>
              )}
              {children && <div className="mt-3">{children}</div>}
            </div>
          </div>
          <div className="flex justify-end gap-2 border-t border-slate-100 px-5 py-3 dark:border-slate-700">
            <button
              ref={cancelRef}
              type="button"
              className="btn border border-slate-300 dark:border-slate-600"
              onClick={onCancel}
              disabled={isPending}
            >
              {cancelLabel}
            </button>
            <button
              type="button"
              className={`btn ${confirmClass} disabled:opacity-60`}
              onClick={onConfirm}
              disabled={isPending || confirmDisabled}
              aria-busy={isPending}
            >
              {isPending ? "Working…" : confirmLabel}
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
}
