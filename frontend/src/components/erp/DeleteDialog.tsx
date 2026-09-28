import { useEffect, useId, useState } from "react";
import { parseApiError, type ApiErrorInfo } from "../../lib/erp/apiError";
import { ConfirmDialog } from "./ConfirmDialog";

export interface DeleteDialogProps {
  open: boolean;
  /** Singular entity name for the title and button, e.g. "customer". */
  entityName: string;
  /** What is being deleted, as users know it: "Acme Ltd (C-001)". */
  recordLabel: string;
  /** Extra consequence text, e.g. "Its contacts will remain but lose their company link." */
  consequences?: string;
  /** Require typing the record label before deleting (high-risk entities). */
  requireTypedConfirmation?: boolean;
  onDelete: () => Promise<unknown>;
  /** When provided, offers archiving as the safer alternative (entities with history). */
  onArchive?: () => Promise<unknown>;
  /** Called after a successful delete/archive; show a toast and refresh there. */
  onDone: (action: "deleted" | "archived") => void;
  onCancel: () => void;
}

/**
 * The one delete confirmation in the ERP. Backend refusals (e.g. 409 "still in use") are shown
 * inside the dialog so the user sees why, instead of a toast that disappears.
 */
export function DeleteDialog({
  open,
  entityName,
  recordLabel,
  consequences,
  requireTypedConfirmation = false,
  onDelete,
  onArchive,
  onDone,
  onCancel,
}: DeleteDialogProps) {
  const [pending, setPending] = useState<"delete" | "archive" | null>(null);
  const [error, setError] = useState<ApiErrorInfo | null>(null);
  const [typed, setTyped] = useState("");
  const inputId = useId();

  useEffect(() => {
    if (open) {
      setError(null);
      setTyped("");
    }
  }, [open]);

  const run = async (action: "delete" | "archive") => {
    const fn = action === "delete" ? onDelete : onArchive;
    if (!fn) return;
    setPending(action);
    setError(null);
    try {
      await fn();
      onDone(action === "delete" ? "deleted" : "archived");
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setPending(null);
    }
  };

  const typedOk = !requireTypedConfirmation || typed.trim() === recordLabel.trim();
  const Entity = entityName.charAt(0).toUpperCase() + entityName.slice(1);

  return (
    <ConfirmDialog
      open={open}
      tone="danger"
      title={`Delete ${entityName}?`}
      confirmLabel={`Delete ${entityName}`}
      isPending={pending !== null}
      confirmDisabled={!typedOk}
      onConfirm={() => void run("delete")}
      onCancel={onCancel}
      description={
        <>
          You are about to permanently delete <strong className="text-slate-900 dark:text-slate-100">{recordLabel}</strong>.
          {consequences ? ` ${consequences}` : ""} This cannot be undone.
        </>
      }
    >
      {onArchive && (
        <p className="text-sm text-slate-600 dark:text-slate-300">
          Archiving keeps its history and hides it from everyday lists.{" "}
          <button
            type="button"
            className="font-medium text-primary underline disabled:opacity-60"
            onClick={() => void run("archive")}
            disabled={pending !== null}
          >
            {pending === "archive" ? "Archiving…" : `Archive ${entityName} instead`}
          </button>
        </p>
      )}
      {requireTypedConfirmation && (
        <div className="mt-3">
          <label htmlFor={inputId} className="mb-1 block text-sm text-slate-700 dark:text-slate-300">
            Type <strong>{recordLabel}</strong> to confirm
          </label>
          <input
            id={inputId}
            className="form-input w-full"
            value={typed}
            onChange={(e) => setTyped(e.target.value)}
            autoComplete="off"
          />
        </div>
      )}
      {error && (
        <div role="alert" className="mt-3 rounded-md bg-danger/10 px-3 py-2 text-sm text-danger">
          {error.status === 409 ? `${Entity} can't be deleted: ${error.message}` : error.message}
        </div>
      )}
    </ConfirmDialog>
  );
}
