import { useFieldArray, useFormContext, useWatch, get } from "react-hook-form";
import { formatMoney } from "../../lib/erp/format";
import { lineTotals, type LineInput } from "../../lib/erp/lineTotals";

export interface LineItemRow extends LineInput {
  description: string;
}

export interface LineItemsTableProps {
  /** Field-array path in the form, e.g. "lines". */
  name: string;
  currency: string;
  /** Default values for a new row. */
  newRow?: Partial<LineItemRow>;
  showDiscount?: boolean;
  showTax?: boolean;
  disabled?: boolean;
}

/**
 * Editable document lines (quote, order, invoice, bill) with live totals.
 * Totals are a preview; the server recalculates and its figures are what get saved.
 */
export function LineItemsTable({ name, currency, newRow, showDiscount = true, showTax = true, disabled }: LineItemsTableProps) {
  const { control, register, formState } = useFormContext();
  const { fields, append, remove } = useFieldArray({ control, name });
  const rows = (useWatch({ control, name }) as LineItemRow[] | undefined) ?? [];
  const totals = lineTotals(rows);
  const numeric = { setValueAs: (v: unknown) => (v === "" || v === null || v === undefined ? undefined : Number(v)) };

  const cellInput = (path: string, label: string, extra = "") => {
    const error = get(formState.errors, path) as { message?: string } | undefined;
    return (
      <>
        <input
          {...register(path, numeric)}
          type="number"
          inputMode="decimal"
          step="any"
          min={0}
          disabled={disabled}
          aria-label={label}
          aria-invalid={!!error}
          title={error?.message}
          className={`form-input w-full text-right tabular-nums ${error ? "border-danger" : ""} ${extra}`}
        />
        {error?.message && <span className="sr-only">{error.message}</span>}
      </>
    );
  };

  return (
    <div>
      <div className="overflow-x-auto rounded-lg border border-slate-200 dark:border-slate-700">
        <table className="min-w-full text-sm">
          <caption className="sr-only">Line items</caption>
          <thead className="bg-slate-50 dark:bg-slate-800">
            <tr className="text-slate-600 dark:text-slate-300 [&>th]:normal-case [&>th]:tracking-normal [&>th]:font-semibold">
              <th scope="col" className="w-8 px-2 py-2 text-left">#</th>
              <th scope="col" className="min-w-56 px-2 py-2 text-left">Description</th>
              <th scope="col" className="w-24 px-2 py-2 text-right">Qty</th>
              <th scope="col" className="w-32 px-2 py-2 text-right">Unit price</th>
              {showDiscount && <th scope="col" className="w-24 px-2 py-2 text-right">Disc. %</th>}
              {showTax && <th scope="col" className="w-24 px-2 py-2 text-right">Tax %</th>}
              <th scope="col" className="w-32 px-2 py-2 text-right">Total</th>
              <th scope="col" className="w-10 px-2 py-2">
                <span className="sr-only">Remove</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {fields.length === 0 && (
              <tr>
                <td colSpan={8} className="px-3 py-6 text-center text-slate-500">
                  No lines yet. Add the first item below.
                </td>
              </tr>
            )}
            {fields.map((field, index) => {
              const path = `${name}.${index}`;
              const descriptionError = get(formState.errors, `${path}.description`) as { message?: string } | undefined;
              return (
                <tr key={field.id} className="border-t border-slate-100 align-top dark:border-slate-700">
                  <td className="px-2 py-2 text-slate-500">{index + 1}</td>
                  <td className="px-2 py-2">
                    <input
                      {...register(`${path}.description`)}
                      disabled={disabled}
                      aria-label={`Line ${index + 1} description`}
                      aria-invalid={!!descriptionError}
                      className={`form-input w-full ${descriptionError ? "border-danger" : ""}`}
                    />
                    {descriptionError?.message && <p className="mt-1 text-xs text-danger">{descriptionError.message}</p>}
                  </td>
                  <td className="px-2 py-2">{cellInput(`${path}.quantity`, `Line ${index + 1} quantity`)}</td>
                  <td className="px-2 py-2">{cellInput(`${path}.unitPrice`, `Line ${index + 1} unit price`)}</td>
                  {showDiscount && <td className="px-2 py-2">{cellInput(`${path}.discountPercent`, `Line ${index + 1} discount percent`)}</td>}
                  {showTax && <td className="px-2 py-2">{cellInput(`${path}.taxPercent`, `Line ${index + 1} tax percent`)}</td>}
                  <td className="px-2 py-2 pt-4 text-right font-medium tabular-nums">{formatMoney(totals.lines[index]?.total ?? 0, currency)}</td>
                  <td className="px-2 py-2">
                    {!disabled && (
                      <button
                        type="button"
                        onClick={() => remove(index)}
                        className="mt-1 rounded p-1.5 text-slate-400 hover:bg-danger/10 hover:text-danger"
                        aria-label={`Remove line ${index + 1}`}
                      >
                        <i className="mgc_delete_2_line" aria-hidden="true" />
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <div className="mt-3 flex flex-wrap items-start justify-between gap-4">
        {!disabled ? (
          <button
            type="button"
            className="btn border border-dashed border-slate-300 text-primary dark:border-slate-600"
            onClick={() => append({ description: "", quantity: 1, unitPrice: 0, discountPercent: 0, taxPercent: 0, ...newRow })}
          >
            <i className="mgc_add_line me-1" aria-hidden="true" />
            Add line
          </button>
        ) : (
          <span />
        )}
        <dl className="w-full max-w-xs space-y-1 text-sm tabular-nums" aria-label="Totals">
          <TotalRow label="Subtotal" value={formatMoney(totals.subtotal, currency)} />
          {showDiscount && <TotalRow label="Discount" value={`− ${formatMoney(totals.discountTotal, currency)}`} />}
          {showTax && <TotalRow label="Tax" value={formatMoney(totals.taxTotal, currency)} />}
          <div className="flex justify-between border-t border-slate-200 pt-1.5 text-base font-semibold dark:border-slate-700">
            <dt>Total</dt>
            <dd>{formatMoney(totals.grandTotal, currency)}</dd>
          </div>
        </dl>
      </div>
    </div>
  );
}

function TotalRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between text-slate-600 dark:text-slate-300">
      <dt>{label}</dt>
      <dd>{value}</dd>
    </div>
  );
}
