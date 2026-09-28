import { useId, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { get, useFormContext } from "react-hook-form";

// ---------- layout ----------

export function FormSection({
  title,
  description,
  columns = 2,
  children,
}: {
  title: string;
  description?: string;
  columns?: 1 | 2 | 3;
  children: ReactNode;
}) {
  const grid = columns === 1 ? "grid-cols-1" : columns === 3 ? "grid-cols-1 sm:grid-cols-2 lg:grid-cols-3" : "grid-cols-1 sm:grid-cols-2";
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-gray-900/40">
      <div className="mb-4 border-b border-slate-100 pb-3 dark:border-slate-700">
        <h2 className="text-base font-semibold text-slate-800 dark:text-slate-100">{title}</h2>
        {description && <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{description}</p>}
      </div>
      <div className={`grid gap-4 ${grid}`}>{children}</div>
    </section>
  );
}

/** Makes a field span the whole section row (notes, addresses). */
export const FullWidth = ({ children }: { children: ReactNode }) => <div className="sm:col-span-full">{children}</div>;

// ---------- field wiring ----------

export interface FieldControlProps {
  id: string;
  "aria-invalid": boolean;
  "aria-describedby"?: string;
  "aria-required"?: boolean;
}

export interface FormFieldProps {
  /** react-hook-form path; used to find this field's error. */
  name: string;
  label: string;
  required?: boolean;
  help?: ReactNode;
  /** Render the control with the ids/aria attributes that connect it to label, help and error. */
  children: (control: FieldControlProps) => ReactNode;
}

export function FormField({ name, label, required, help, children }: FormFieldProps) {
  const id = useId();
  const { formState } = useFormContext();
  const error = get(formState.errors, name) as { message?: string } | undefined;
  const helpId = help ? `${id}-help` : undefined;
  const errorId = error?.message ? `${id}-error` : undefined;
  const describedBy = [errorId, helpId].filter(Boolean).join(" ") || undefined;

  return (
    <div className="min-w-0">
      <label htmlFor={id} className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
        {label}
        {required && (
          <>
            <span className="ms-0.5 text-danger" aria-hidden="true">
              *
            </span>
            <span className="sr-only"> (required)</span>
          </>
        )}
      </label>
      {children({ id, "aria-invalid": !!error, "aria-describedby": describedBy, "aria-required": required || undefined })}
      {error?.message && (
        <p id={errorId} className="mt-1 text-sm text-danger">
          {error.message}
        </p>
      )}
      {help && (
        <p id={helpId} className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {help}
        </p>
      )}
    </div>
  );
}

const inputClass = (invalid: boolean) =>
  `form-input w-full ${invalid ? "border-danger focus:border-danger focus:ring-danger" : ""} disabled:bg-slate-50 disabled:text-slate-500 dark:disabled:bg-slate-800`;

type BaseFieldProps = Omit<FormFieldProps, "children"> & { disabled?: boolean; placeholder?: string };

export function TextField({ type = "text", autoComplete, ...props }: BaseFieldProps & { type?: "text" | "email" | "tel" | "url"; autoComplete?: string }) {
  const { register } = useFormContext();
  return (
    <FormField {...props}>
      {(control) => (
        <input
          {...control}
          {...register(props.name)}
          type={type}
          autoComplete={autoComplete}
          placeholder={props.placeholder}
          disabled={props.disabled}
          className={inputClass(control["aria-invalid"])}
        />
      )}
    </FormField>
  );
}

export function TextareaField({ rows = 4, ...props }: BaseFieldProps & { rows?: number }) {
  const { register } = useFormContext();
  return (
    <FormField {...props}>
      {(control) => (
        <textarea
          {...control}
          {...register(props.name)}
          rows={rows}
          placeholder={props.placeholder}
          disabled={props.disabled}
          className={`${inputClass(control["aria-invalid"])} resize-y`}
        />
      )}
    </FormField>
  );
}

export interface Option {
  value: string;
  label: string;
}

export function SelectField({ options, emptyOption, ...props }: BaseFieldProps & { options: readonly Option[]; emptyOption?: string }) {
  const { register } = useFormContext();
  return (
    <FormField {...props}>
      {(control) => (
        <select {...control} {...register(props.name)} disabled={props.disabled} className={`form-select w-full ${control["aria-invalid"] ? "border-danger" : ""}`}>
          {emptyOption !== undefined && <option value="">{emptyOption}</option>}
          {options.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      )}
    </FormField>
  );
}

/** Numbers stay numbers: empty input becomes undefined, not "" or NaN. */
export function NumberField({ min, max, step = "any", ...props }: BaseFieldProps & { min?: number; max?: number; step?: number | "any" }) {
  const { register } = useFormContext();
  return (
    <FormField {...props}>
      {(control) => (
        <input
          {...control}
          {...register(props.name, { setValueAs: (v: unknown) => (v === "" || v === null || v === undefined ? undefined : Number(v)) })}
          type="number"
          inputMode="decimal"
          min={min}
          max={max}
          step={step}
          disabled={props.disabled}
          className={`${inputClass(control["aria-invalid"])} text-right tabular-nums`}
        />
      )}
    </FormField>
  );
}

/** Amount with its currency shown alongside. The currency itself is a separate field. */
export function CurrencyField({ currency, ...props }: BaseFieldProps & { currency: string }) {
  const { register } = useFormContext();
  return (
    <FormField {...props}>
      {(control) => (
        <div className="flex">
          <span className="inline-flex items-center rounded-s border border-e-0 border-slate-300 bg-slate-50 px-3 text-sm text-slate-600 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-300">
            {currency || "—"}
          </span>
          <input
            {...control}
            {...register(props.name, { setValueAs: (v: unknown) => (v === "" || v === null || v === undefined ? undefined : Number(v)) })}
            type="number"
            inputMode="decimal"
            step="0.01"
            min={0}
            disabled={props.disabled}
            className={`${inputClass(control["aria-invalid"])} rounded-s-none text-right tabular-nums`}
          />
        </div>
      )}
    </FormField>
  );
}

/** Native date input: keyboard and screen-reader friendly; value is "YYYY-MM-DD". */
export function DateField({ min, max, ...props }: BaseFieldProps & { min?: string; max?: string }) {
  const { register } = useFormContext();
  return (
    <FormField {...props}>
      {(control) => (
        <input {...control} {...register(props.name)} type="date" min={min} max={max} disabled={props.disabled} className={inputClass(control["aria-invalid"])} />
      )}
    </FormField>
  );
}

export function CheckboxField({ name, label, help, disabled }: { name: string; label: string; help?: string; disabled?: boolean }) {
  const { register } = useFormContext();
  const id = useId();
  return (
    <div className="flex items-start gap-2 pt-6">
      <input id={id} type="checkbox" className="form-checkbox mt-0.5 rounded" disabled={disabled} {...register(name)} aria-describedby={help ? `${id}-help` : undefined} />
      <div>
        <label htmlFor={id} className="text-sm font-medium text-slate-700 dark:text-slate-300">
          {label}
        </label>
        {help && (
          <p id={`${id}-help`} className="text-xs text-slate-500">
            {help}
          </p>
        )}
      </div>
    </div>
  );
}

export function FormErrorSummary({ messages, title = "The form could not be saved" }: { messages: string[]; title?: string }) {
  if (messages.length === 0) return null;
  return (
    <div role="alert" className="rounded-md border border-danger/30 bg-danger/5 px-4 py-3 text-sm text-danger">
      <p className="font-medium">{title}</p>
      {messages.length === 1 ? (
        <p className="mt-0.5">{messages[0]}</p>
      ) : (
        <ul className="mt-1 list-disc ps-5">
          {messages.map((m) => (
            <li key={m}>{m}</li>
          ))}
        </ul>
      )}
    </div>
  );
}

export function FormFooter({
  cancelTo,
  onCancel,
  isSaving,
  submitLabel = "Save",
  savingLabel = "Saving…",
}: {
  cancelTo?: string;
  onCancel?: () => void;
  isSaving: boolean;
  submitLabel?: string;
  savingLabel?: string;
}) {
  const cancelClass = "btn border border-slate-300 dark:border-slate-600";
  return (
    <div className="sticky bottom-0 z-10 -mx-1 flex items-center justify-end gap-2 border-t border-slate-200 bg-white/95 px-1 py-3 backdrop-blur dark:border-slate-700 dark:bg-gray-800/95">
      {cancelTo ? (
        <Link to={cancelTo} className={cancelClass}>
          Cancel
        </Link>
      ) : (
        <button type="button" className={cancelClass} onClick={onCancel}>
          Cancel
        </button>
      )}
      <button type="submit" className="btn min-w-32 bg-primary text-white disabled:opacity-60" disabled={isSaving} aria-busy={isSaving}>
        {isSaving ? savingLabel : submitLabel}
      </button>
    </div>
  );
}
