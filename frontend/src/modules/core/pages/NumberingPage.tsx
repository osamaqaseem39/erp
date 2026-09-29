import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useEffect, useId, useState } from "react";
import { DataTable, ErrorState, PageHeader, toast, type Column } from "../../../components/erp";
import { parseApiError } from "../../../lib/erp/apiError";
import { useResource } from "../../../lib/erp/http";
import { coreApi, type DocumentSequence } from "../services/coreApi";
import { formatDocumentNumber } from "../utils/coreFormat";

const columns: Column<DocumentSequence>[] = [
  { key: "label", header: "Document", cell: (s) => s.label, hideable: false },
  { key: "preview", header: "Next number", cell: (s) => <code className="text-sm">{s.preview}</code> },
  { key: "prefix", header: "Prefix", cell: (s) => s.prefix },
  { key: "reset", header: "Restarts", cell: (s) => (s.resetPolicy === "Yearly" ? "Every January" : "Never") },
];

export default function NumberingPage() {
  const { data, error, isLoading, refetch } = useResource((_: null, signal) => coreApi.sequences(signal), null);
  const [editing, setEditing] = useState<DocumentSequence | null>(null);

  return (
    <>
      <PageHeader
        title="Document numbering"
        description="How quotes, orders and other documents are numbered. Each organization has its own sequences."
        breadcrumbs={[{ label: "Core / Settings", to: "/core" }, { label: "Settings", to: "/core/settings" }, { label: "Numbering" }]}
      />
      <div className="card">
        {error && !data ? (
          <ErrorState error={error} onRetry={refetch} />
        ) : (
          <DataTable<DocumentSequence>
            caption="Document sequences"
            columns={columns}
            rows={data}
            getRowId={(s) => s.key}
            isLoading={isLoading}
            error={error}
            onRetry={refetch}
            rowActions={[{ label: "Edit numbering", icon: "mgc_edit_line", onSelect: setEditing }]}
            emptyState={null}
          />
        )}
      </div>
      <p className="mt-3 text-sm text-slate-500">
        Numbers already issued never change. A number taken by a document that then fails to save is skipped rather than reused.
      </p>
      <SequenceDialog
        sequence={editing}
        onClose={() => setEditing(null)}
        onSaved={() => {
          setEditing(null);
          refetch();
        }}
      />
    </>
  );
}

function SequenceDialog({ sequence, onClose, onSaved }: { sequence: DocumentSequence | null; onClose: () => void; onSaved: () => void }) {
  const ids = { prefix: useId(), padding: useId(), reset: useId(), next: useId() };
  const [form, setForm] = useState({ prefix: "", padding: 5, resetPolicy: "Yearly" as DocumentSequence["resetPolicy"], nextNumber: 1 });
  const [errors, setErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (sequence) {
      setForm({ prefix: sequence.prefix, padding: sequence.padding, resetPolicy: sequence.resetPolicy, nextNumber: sequence.nextNumber });
      setErrors([]);
    }
  }, [sequence]);

  const preview = formatDocumentNumber(form.prefix.toUpperCase() || "?", form.resetPolicy, new Date().getFullYear(), form.nextNumber, form.padding);

  const save = async () => {
    if (!sequence) return;
    setSaving(true);
    setErrors([]);
    try {
      await coreApi.updateSequence(sequence.key, { ...form, prefix: form.prefix.trim() });
      toast.success(`${sequence.label} numbering saved`);
      onSaved();
    } catch (err) {
      const info = parseApiError(err);
      const messages = Object.values(info.fieldErrors).flat();
      setErrors(messages.length ? messages : [info.message]);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={!!sequence} onClose={() => !saving && onClose()} className="relative z-[70]">
      <div className="fixed inset-0 bg-slate-900/50" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="w-full max-w-md rounded-lg bg-white shadow-xl dark:bg-gray-800">
          <form
            onSubmit={(e) => {
              e.preventDefault();
              void save();
            }}
          >
            <div className="space-y-4 p-5">
              <DialogTitle className="text-base font-semibold">{sequence?.label} numbering</DialogTitle>
              <div className="rounded-md bg-slate-50 px-3 py-2 text-sm dark:bg-slate-700/50" aria-live="polite">
                Next document: <code className="font-semibold">{preview}</code>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label htmlFor={ids.prefix} className="mb-1 block text-sm font-medium">
                    Prefix
                  </label>
                  <input id={ids.prefix} className="form-input w-full uppercase" maxLength={20} value={form.prefix} onChange={(e) => setForm({ ...form, prefix: e.target.value.replace(/[^a-z0-9]/gi, "") })} required />
                </div>
                <div>
                  <label htmlFor={ids.padding} className="mb-1 block text-sm font-medium">
                    Digits
                  </label>
                  <input id={ids.padding} type="number" min={1} max={10} className="form-input w-full" value={form.padding} onChange={(e) => setForm({ ...form, padding: Number(e.target.value) })} />
                </div>
                <div>
                  <label htmlFor={ids.reset} className="mb-1 block text-sm font-medium">
                    Restart numbering
                  </label>
                  <select id={ids.reset} className="form-select w-full" value={form.resetPolicy} onChange={(e) => setForm({ ...form, resetPolicy: e.target.value as DocumentSequence["resetPolicy"] })}>
                    <option value="Yearly">Every January (includes the year)</option>
                    <option value="Never">Never</option>
                  </select>
                </div>
                <div>
                  <label htmlFor={ids.next} className="mb-1 block text-sm font-medium">
                    Next number
                  </label>
                  <input
                    id={ids.next}
                    type="number"
                    min={sequence?.nextNumber ?? 1}
                    className="form-input w-full"
                    value={form.nextNumber}
                    onChange={(e) => setForm({ ...form, nextNumber: Number(e.target.value) })}
                    aria-describedby={`${ids.next}-help`}
                  />
                  <p id={`${ids.next}-help`} className="mt-1 text-xs text-slate-500">
                    Can only go up, from {sequence?.nextNumber}.
                  </p>
                </div>
              </div>
              {errors.length > 0 && (
                <ul role="alert" className="list-disc rounded-md bg-danger/10 py-2 pe-3 ps-7 text-sm text-danger">
                  {errors.map((m) => (
                    <li key={m}>{m}</li>
                  ))}
                </ul>
              )}
            </div>
            <div className="flex justify-end gap-2 border-t border-slate-100 px-5 py-3 dark:border-slate-700">
              <button type="button" className="btn border border-slate-300 dark:border-slate-600" onClick={onClose} disabled={saving}>
                Cancel
              </button>
              <button type="submit" className="btn bg-primary text-white disabled:opacity-60" disabled={saving}>
                {saving ? "Saving…" : "Save numbering"}
              </button>
            </div>
          </form>
        </DialogPanel>
      </div>
    </Dialog>
  );
}
