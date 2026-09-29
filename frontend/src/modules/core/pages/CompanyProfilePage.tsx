import { useEffect, useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import * as yup from "yup";
import {
  ConfirmDialog,
  EmptyState,
  ErrorState,
  FormErrorSummary,
  FormFooter,
  FormSection,
  FullWidth,
  PageHeader,
  SelectField,
  SkeletonFields,
  TextField,
  toast,
} from "../../../components/erp";
import { parseApiError } from "../../../lib/erp/apiError";
import { applyServerErrors } from "../../../lib/erp/formErrors";
import { useResource } from "../../../lib/erp/http";
import { useUnsavedChangesGuard } from "../../../lib/erp/useUnsavedChangesGuard";
import { usePermission } from "../../../lib/erp/permissions";
import { coreApi, type CompanyProfile } from "../services/coreApi";
import { refreshTenantContext } from "../tenantContext";

const optional = (max: number, label: string) => yup.string().trim().max(max, `${label} cannot exceed ${max} characters`).default("");
const url = (label: string) =>
  optional(500, label).test("url", `${label} must be a full web address starting with https://`, (v) => !v || /^https?:\/\/\S+$/i.test(v));

const schema = yup.object({
  companyName: yup.string().trim().required("Company name is required").max(200, "Company name cannot exceed 200 characters"),
  legalName: optional(200, "Legal name"),
  registrationNumber: optional(100, "Registration number"),
  taxId: optional(100, "Tax ID"),
  addressLine1: optional(200, "Address line 1"),
  addressLine2: optional(200, "Address line 2"),
  city: optional(100, "City"),
  state: optional(100, "State"),
  postalCode: optional(20, "Postal code"),
  country: optional(100, "Country"),
  phone: optional(30, "Phone"),
  email: optional(200, "Email").email("Enter a valid email address"),
  website: url("Website"),
  logoUrl: url("Logo URL"),
  currency: yup.string().required("Currency is required"),
  timeZone: yup.string().required("Time zone is required"),
});

type FormValues = yup.InferType<typeof schema>;
const FIELDS = Object.keys(schema.fields);

// Common choices; the server accepts any ISO 4217 code and IANA time zone.
const CURRENCIES = ["PKR", "USD", "EUR", "GBP", "AED", "SAR", "INR", "CAD", "AUD", "CNY", "JPY", "CHF"].map((c) => ({ value: c, label: c }));
const browserZones = (): string[] => {
  const intl = Intl as unknown as { supportedValuesOf?: (key: string) => string[] };
  try {
    return intl.supportedValuesOf?.("timeZone") ?? [];
  } catch {
    return [];
  }
};
const TIME_ZONES = Array.from(new Set(["UTC", "Asia/Karachi", "Asia/Dubai", "Asia/Riyadh", "Europe/London", "America/New_York", ...browserZones()])).map((z) => ({
  value: z,
  label: z,
}));

const toForm = (c: CompanyProfile): FormValues => ({
  companyName: c.companyName,
  legalName: c.legalName ?? "",
  registrationNumber: c.registrationNumber ?? "",
  taxId: c.taxId ?? "",
  addressLine1: c.addressLine1 ?? "",
  addressLine2: c.addressLine2 ?? "",
  city: c.city ?? "",
  state: c.state ?? "",
  postalCode: c.postalCode ?? "",
  country: c.country ?? "",
  phone: c.phone ?? "",
  email: c.email ?? "",
  website: c.website ?? "",
  logoUrl: c.logoUrl ?? "",
  currency: c.currency,
  timeZone: c.timeZone,
});

export default function CompanyProfilePage() {
  const { data, error, isLoading, refetch } = useResource((_: null, signal) => coreApi.company(signal), null);
  const canEdit = usePermission("core.company.update");
  const methods = useForm<FormValues>({ resolver: yupResolver(schema), mode: "onBlur" });
  const { handleSubmit, reset, setError, formState } = methods;
  const [formErrors, setFormErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const guard = useUnsavedChangesGuard(formState.isDirty);

  useEffect(() => {
    if (data) reset(toForm(data));
  }, [data, reset]);

  const onSubmit = async (values: FormValues) => {
    setFormErrors([]);
    setSaving(true);
    try {
      const saved = await coreApi.updateCompany(values);
      reset(toForm(saved));
      await refreshTenantContext();
      toast.success("Company profile saved", "Currency and time zone apply across the app.");
    } catch (err) {
      setFormErrors(applyServerErrors(parseApiError(err), setError, FIELDS));
    } finally {
      setSaving(false);
    }
  };

  const breadcrumbs = [{ label: "Core / Settings", to: "/core" }, { label: "Settings", to: "/core/settings" }, { label: "Company profile" }];

  return (
    <>
      <PageHeader title="Company profile" description="How your organization appears on documents, and its currency and time zone." breadcrumbs={breadcrumbs} />
      {error && !data ? (
        <div className="card">
          {error.status === 401 ? (
            <EmptyState variant="forbidden" title="Your account isn't linked to a company" description="Sign out and in again, or ask an administrator to link your user to a company." />
          ) : (
            <ErrorState error={error} onRetry={refetch} />
          )}
        </div>
      ) : isLoading || !data ? (
        <div className="card p-5">
          <SkeletonFields fields={8} />
        </div>
      ) : (
        <FormProvider {...methods}>
          <form noValidate onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-5xl space-y-4">
            <FormErrorSummary messages={formErrors} />
            <fieldset disabled={!canEdit} className="space-y-4">
              <FormSection title="Identity">
                <TextField name="companyName" label="Company name" required autoComplete="organization" />
                <TextField name="legalName" label="Legal name" help="As registered; printed on invoices." />
                <TextField name="registrationNumber" label="Registration number" />
                <TextField name="taxId" label="Tax ID" help="For example NTN or VAT number." />
              </FormSection>
              <FormSection title="Regional" description="Used to format money and dates for everyone in your organization.">
                <SelectField name="currency" label="Base currency" required options={CURRENCIES} />
                <SelectField name="timeZone" label="Time zone" required options={TIME_ZONES} />
              </FormSection>
              <FormSection title="Address">
                <FullWidth>
                  <TextField name="addressLine1" label="Address line 1" autoComplete="address-line1" />
                </FullWidth>
                <FullWidth>
                  <TextField name="addressLine2" label="Address line 2" autoComplete="address-line2" />
                </FullWidth>
                <TextField name="city" label="City" autoComplete="address-level2" />
                <TextField name="state" label="State / province" autoComplete="address-level1" />
                <TextField name="postalCode" label="Postal code" autoComplete="postal-code" />
                <TextField name="country" label="Country" autoComplete="country-name" />
              </FormSection>
              <FormSection title="Contact">
                <TextField name="phone" label="Phone" type="tel" />
                <TextField name="email" label="Email" type="email" />
                <TextField name="website" label="Website" type="url" placeholder="https://" />
                <TextField name="logoUrl" label="Logo URL" type="url" placeholder="https://" help="File upload arrives with file storage; paste a hosted image link for now." />
              </FormSection>
            </fieldset>
            {canEdit && <FormFooter cancelTo="/core/settings" isSaving={saving} submitLabel="Save profile" />}
          </form>
        </FormProvider>
      )}
      <ConfirmDialog
        open={guard.isBlocked}
        title="Leave without saving?"
        description="Your changes to the company profile will be lost."
        confirmLabel="Leave page"
        cancelLabel="Keep editing"
        tone="danger"
        onConfirm={guard.proceed}
        onCancel={guard.stay}
      />
    </>
  );
}
