import { useEffect, useRef, useState } from "react";
import AsyncSelect from "react-select/async";
import { Controller, useFormContext } from "react-hook-form";
import { FormField } from "./Form";

export interface EntityOption {
  /** The id stored in the form and sent to the API. Never shown to users. */
  value: string;
  /** What users recognize: "Acme Ltd". */
  label: string;
  /** Secondary text such as a code: "C-001". */
  hint?: string;
}

export interface EntitySelectProps {
  name: string;
  label: string;
  required?: boolean;
  help?: string;
  placeholder?: string;
  disabled?: boolean;
  /** Search by name/code. Called with "" when the menu opens. */
  loadOptions: (search: string) => Promise<EntityOption[]>;
  /** Looks up the label for an id already in the form (edit mode). */
  resolveOption: (id: string) => Promise<EntityOption | null>;
}

/** Relationship picker: stores an id, shows and searches by name. */
export function EntitySelect({ name, label, required, help, placeholder = "Search…", disabled, loadOptions, resolveOption }: EntitySelectProps) {
  const { control, watch } = useFormContext();
  const value = watch(name) as string | undefined;
  const [selected, setSelected] = useState<EntityOption | null>(null);
  const resolveRef = useRef(resolveOption);
  resolveRef.current = resolveOption;

  // Resolve the label for an id that arrived without one (loaded record, URL prefill).
  useEffect(() => {
    let cancelled = false;
    if (!value) {
      setSelected(null);
    } else if (selected?.value !== value) {
      resolveRef
        .current(value)
        .then((option) => !cancelled && setSelected(option ?? { value, label: "Unknown record" }))
        .catch(() => !cancelled && setSelected({ value, label: "Unavailable record" }));
    }
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- only react to the stored id changing
  }, [value]);

  return (
    <FormField name={name} label={label} required={required} help={help}>
      {(fieldProps) => (
        <Controller
          name={name}
          control={control}
          render={({ field }) => (
            <AsyncSelect<EntityOption>
              inputId={fieldProps.id}
              aria-invalid={fieldProps["aria-invalid"]}
              aria-describedby={fieldProps["aria-describedby"]}
              aria-required={fieldProps["aria-required"]}
              name={field.name}
              value={selected}
              onChange={(option) => {
                setSelected(option);
                field.onChange(option?.value ?? "");
              }}
              onBlur={field.onBlur}
              loadOptions={loadOptions}
              defaultOptions
              cacheOptions
              isClearable={!required}
              isDisabled={disabled}
              placeholder={placeholder}
              noOptionsMessage={({ inputValue }) => (inputValue ? `No matches for "${inputValue}"` : "Start typing to search")}
              loadingMessage={() => "Searching…"}
              formatOptionLabel={(option) => (
                <span className="flex items-baseline gap-2">
                  <span>{option.label}</span>
                  {option.hint && <span className="text-xs text-slate-500">{option.hint}</span>}
                </span>
              )}
              classNamePrefix="erp-select"
              classNames={{
                control: (state) =>
                  `${fieldProps["aria-invalid"] ? "!border-danger" : ""} ${state.isFocused ? "!border-primary !shadow-[0_0_0_1px_#3073F1]" : ""}`,
                menu: () => "!z-40",
              }}
            />
          )}
        />
      )}
    </FormField>
  );
}
