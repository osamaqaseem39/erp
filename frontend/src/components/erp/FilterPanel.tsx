import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import type { ReactNode } from "react";

export interface FilterPanelProps {
  open: boolean;
  onClose: () => void;
  onApply: () => void;
  onClear: () => void;
  /** Filter inputs. Keep them as draft state and commit in onApply. */
  children: ReactNode;
  title?: string;
}

/** Right-hand drawer for advanced filters. */
export function FilterPanel({ open, onClose, onApply, onClear, children, title = "Filters" }: FilterPanelProps) {
  return (
    <Dialog open={open} onClose={onClose} className="relative z-[60]">
      <div className="fixed inset-0 bg-slate-900/40" aria-hidden="true" />
      <div className="fixed inset-y-0 end-0 flex w-full max-w-sm">
        <DialogPanel className="flex w-full flex-col bg-white shadow-xl dark:bg-gray-800">
          <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4 dark:border-slate-700">
            <DialogTitle className="text-base font-semibold">{title}</DialogTitle>
            <button type="button" onClick={onClose} className="rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-700" aria-label="Close filters">
              <i className="mgc_close_line text-lg" aria-hidden="true" />
            </button>
          </div>
          <form
            className="flex min-h-0 flex-1 flex-col"
            onSubmit={(e) => {
              e.preventDefault();
              onApply();
            }}
          >
            <div className="flex-1 space-y-4 overflow-y-auto px-5 py-4">{children}</div>
            <div className="flex justify-between gap-2 border-t border-slate-200 px-5 py-3 dark:border-slate-700">
              <button type="button" className="btn border border-slate-300 dark:border-slate-600" onClick={onClear}>
                Clear all
              </button>
              <button type="submit" className="btn bg-primary text-white">
                Apply filters
              </button>
            </div>
          </form>
        </DialogPanel>
      </div>
    </Dialog>
  );
}

export interface FilterChip {
  key: string;
  label: string;
}

/** Applied filters shown under the toolbar, each removable. */
export function FilterChips({ chips, onRemove, onClearAll }: { chips: FilterChip[]; onRemove: (key: string) => void; onClearAll: () => void }) {
  if (chips.length === 0) return null;
  return (
    <div className="flex flex-wrap items-center gap-2 px-4 pb-3" aria-label="Applied filters">
      {chips.map((chip) => (
        <span key={chip.key} className="inline-flex items-center gap-1 rounded-full bg-primary/10 py-0.5 ps-3 pe-1 text-xs text-primary">
          {chip.label}
          <button type="button" onClick={() => onRemove(chip.key)} className="rounded-full p-0.5 hover:bg-primary/20" aria-label={`Remove filter ${chip.label}`}>
            <i className="mgc_close_line" aria-hidden="true" />
          </button>
        </span>
      ))}
      <button type="button" onClick={onClearAll} className="text-xs text-slate-500 underline hover:text-slate-700">
        Clear all
      </button>
    </div>
  );
}
