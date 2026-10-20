import { useEffect, useMemo, useState } from "react";
import { FormProvider, useForm, useFormContext } from "react-hook-form";
import {
  ConfirmDialog,
  ErrorState,
  FormErrorSummary,
  FormField,
  FormFooter,
  FormSection,
  PageHeader,
  SkeletonFields,
  toast,
} from "../../../components/erp";
import { parseApiError } from "../../../lib/erp/apiError";
import { useResource } from "../../../lib/erp/http";
import { useUnsavedChangesGuard } from "../../../lib/erp/useUnsavedChangesGuard";
import { coreApi, type SettingValue } from "../services/coreApi";
import { refreshTenantContext } from "../tenantContext";

export interface SettingsCategoryPageProps {
  category: "preferences" | "security" | "email" | "finance";
  title: string;
  description: string;
  /** Honest note about what the settings do today (e.g. "stored now, enforced in Checkpoint 2"). */
  notice?: string;
}

/** RHF treats dots as nested paths, so setting keys are stored under a dot-free field name. */
const fieldName = (key: string) => key.replace(/\./g, "__");

type SecretAction = "keep" | "replace" | "clear";

/** Renders a settings category from the server's catalog definitions: types, options, limits and defaults. */
export default function SettingsCategoryPage({ category, title, description, notice }: SettingsCategoryPageProps) {
  const { data, error, isLoading, refetch } = useResource((c: string, signal) => coreApi.settings(c, signal), category);
  const methods = useForm<Record<string, string>>({ mode: "onBlur" });
  const { handleSubmit, reset, setError, formState } = methods;
  const [secretActions, setSecretActions] = useState<Record<string, SecretAction>>({});
  const [formErrors, setFormErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const secretsDirty = Object.values(secretActions).some((a) => a !== "keep");
  const guard = useUnsavedChangesGuard(formState.isDirty || secretsDirty);

  const defaults = useMemo(
    () => Object.fromEntries((data?.settings ?? []).filter((s) => s.type !== "secret").map((s) => [fieldName(s.key), s.value ?? ""])),
    [data]
  );
  useEffect(() => {
    if (!data) return;
    reset(defaults);
    setSecretActions({});
  }, [data, defaults, reset]);

  const onSubmit = async (form: Record<string, string>) => {
    if (!data) return;
    setFormErrors([]);
    setSaving(true);
    const values: Record<string, string | null> = {};
    for (const s of data.settings) {
      if (s.readOnly) continue;
      if (s.type === "secret") {
        const action = secretActions[s.key] ?? "keep";
        if (action === "clear") values[s.key] = "";
        if (action === "replace") values[s.key] = form[fieldName(s.key)] ?? "";
      } else {
        values[s.key] = form[fieldName(s.key)] ?? "";
      }
    }
    try {
      await coreApi.updateSettings(category, values);
      if (category === "preferences") await refreshTenantContext();
      toast.success(`${title} saved`);
      refetch();
    } catch (err) {
      const info = parseApiError(err);
      // Catalog validation messages come back as "setting.key: message".
      const unassigned: string[] = [];
      for (const message of Object.values(info.fieldErrors).flat()) {
        const match = /^([\w.]+): (.+)$/.exec(message);
        if (match && data.settings.some((s) => s.key === match[1])) {
          setError(fieldName(match[1]), { type: "server", message: match[2] });
        } else unassigned.push(message);
      }
      setFormErrors(unassigned.length ? unassigned : Object.keys(info.fieldErrors).length ? [] : [info.message]);
    } finally {
      setSaving(false);
    }
  };

  const breadcrumbs = [{ label: "Core / Settings", to: "/core" }, { label: "Settings", to: "/core/settings" }, { label: title }];

  return (
    <>
      <PageHeader title={title} description={description} breadcrumbs={breadcrumbs} />
      {notice && (
        <div role="note" className="mb-4 flex items-start gap-3 rounded-lg border border-info/30 bg-info/5 px-4 py-3 text-sm">
          <i className="mgc_information_line mt-0.5 text-lg text-info" aria-hidden="true" />
          <p className="mb-0">{notice}</p>
        </div>
      )}
      {error && !data ? (
        <div className="card">
          <ErrorState error={error} onRetry={refetch} />
        </div>
      ) : isLoading || !data ? (
        <div className="card p-5">
          <SkeletonFields fields={4} />
        </div>
      ) : (
        <FormProvider {...methods}>
          <form noValidate onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-4xl space-y-4">
            <FormErrorSummary messages={formErrors} />
            <FormSection title={title} columns={2}>
              {data.settings.map((s) =>
                s.type === "secret" ? (
                  <SecretSetting
                    key={s.key}
                    setting={s}
                    action={secretActions[s.key] ?? "keep"}
                    onAction={(a) => setSecretActions((prev) => ({ ...prev, [s.key]: a }))}
                  />
                ) : (
                  <SettingInput key={s.key} setting={s} />
                )
              )}
            </FormSection>
            <FormFooter cancelTo="/core/settings" isSaving={saving} submitLabel="Save settings" />
          </form>
        </FormProvider>
      )}
      <ConfirmDialog
        open={guard.isBlocked}
        title="Leave without saving?"
        description="Your changes to these settings will be lost."
        confirmLabel="Leave page"
        cancelLabel="Keep editing"
        tone="danger"
        onConfirm={guard.proceed}
        onCancel={guard.stay}
      />
    </>
  );
}

function helpFor(s: SettingValue) {
  const parts = [s.description, s.isDefault && s.value ? "Using the default." : null, s.readOnly ? "Read-only." : null].filter(Boolean);
  return parts.length ? parts.join(" ") : undefined;
}

function SettingInput({ setting: s }: { setting: SettingValue }) {
  const { register } = useFormContext();
  const name = fieldName(s.key);
  const rules = {
    required: s.required ? `${s.label} is required.` : false,
    ...(s.type === "number"
      ? {
          validate: (v: string) => {
            if (v === "" && !s.required) return true;
            const n = Number(v);
            if (!Number.isInteger(n)) return `${s.label} must be a whole number.`;
            if (s.min != null && n < s.min) return `${s.label} must be at least ${s.min}.`;
            if (s.max != null && n > s.max) return `${s.label} must be at most ${s.max}.`;
            return true;
          },
        }
      : {}),
    ...(s.type === "email" ? { pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email address." } } : {}),
  };

  return (
    <FormField name={name} label={s.label} required={s.required} help={helpFor(s)}>
      {(control) =>
        s.type === "choice" ? (
          <select {...control} {...register(name, rules)} disabled={s.readOnly} className="form-select w-full">
            {!s.required && <option value="">Not set</option>}
            {s.options?.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        ) : s.type === "boolean" ? (
          <select {...control} {...register(name, rules)} disabled={s.readOnly} className="form-select w-full">
            <option value="true">Yes</option>
            <option value="false">No</option>
          </select>
        ) : (
          <input
            {...control}
            {...register(name, rules)}
            type={s.type === "number" ? "number" : s.type === "email" ? "email" : "text"}
            min={s.min ?? undefined}
            max={s.type === "number" ? s.max ?? undefined : undefined}
            maxLength={s.type === "text" ? s.max ?? undefined : undefined}
            disabled={s.readOnly}
            className={`form-input w-full ${control["aria-invalid"] ? "border-danger" : ""}`}
          />
        )
      }
    </FormField>
  );
}

/** Write-only: shows whether a value is stored, and lets the user replace or clear it. */
function SecretSetting({ setting: s, action, onAction }: { setting: SettingValue; action: SecretAction; onAction: (a: SecretAction) => void }) {
  const { register } = useFormContext();
  const name = fieldName(s.key);
  return (
    <FormField name={name} label={s.label} help={s.description ?? undefined}>
      {(control) =>
        action === "replace" ? (
          <div className="flex gap-2">
            <input
              {...control}
              {...register(name, { required: `Enter the new ${s.label.toLowerCase()}, or cancel.` })}
              type="password"
              autoComplete="new-password"
              className="form-input w-full"
            />
            <button type="button" className="btn border border-slate-300 dark:border-slate-600" onClick={() => onAction("keep")}>
              Cancel
            </button>
          </div>
        ) : (
          <div id={control.id} className="flex flex-wrap items-center gap-2 py-1.5 text-sm">
            <span className={action === "clear" ? "text-danger" : s.hasValue ? "text-success" : "text-slate-500"}>
              <i className={action === "clear" ? "mgc_delete_2_line" : s.hasValue ? "mgc_lock_line" : "mgc_unlock_line"} aria-hidden="true" />{" "}
              {action === "clear" ? "Will be cleared when you save" : s.hasValue ? "Stored (hidden)" : "Not set"}
            </span>
            <button type="button" className="text-primary underline" onClick={() => onAction("replace")}>
              {s.hasValue ? "Replace" : "Set"}
            </button>
            {s.hasValue && action !== "clear" && (
              <button type="button" className="text-danger underline" onClick={() => onAction("clear")}>
                Clear
              </button>
            )}
            {action === "clear" && (
              <button type="button" className="text-slate-600 underline" onClick={() => onAction("keep")}>
                Undo
              </button>
            )}
          </div>
        )
      }
    </FormField>
  );
}
