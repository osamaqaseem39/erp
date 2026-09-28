import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { FormProvider, useForm } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import * as yup from "yup";
import {
  ConfirmDialog,
  CurrencyField,
  EmptyState,
  ErrorState,
  FormErrorSummary,
  FormFooter,
  FormSection,
  FullWidth,
  PageHeader,
  SelectField,
  SkeletonFields,
  TextareaField,
  TextField,
  toast,
} from "../../../../../components/erp";
import { parseApiError } from "../../../../../lib/erp/apiError";
import { applyServerErrors } from "../../../../../lib/erp/formErrors";
import { formatMoney } from "../../../../../lib/erp/format";
import { useResource } from "../../../../../lib/erp/http";
import { useUnsavedChangesGuard } from "../../../../../lib/erp/useUnsavedChangesGuard";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";
import { customerService } from "../services";
import { CURRENCY_OPTIONS, CUSTOMER_STATUS_OPTIONS, CUSTOMER_TYPE_OPTIONS, PAYMENT_TERMS_OPTIONS, type CustomerDto } from "../types";
import { toUpdateDto } from "../customerUtils";

const optional = (max: number, label: string) =>
  yup.string().trim().max(max, `${label} cannot exceed ${max} characters`).default("");

// Mirrors ERP.Api/Validators/CRM/CreateCustomerValidator so users see limits before submitting.
const schema = yup.object({
  customerCode: yup.string().trim().required("Customer code is required").max(50, "Customer code cannot exceed 50 characters"),
  customerName: yup.string().trim().required("Customer name is required").max(200, "Customer name cannot exceed 200 characters"),
  customerType: yup.string().oneOf(["Individual", "Company"], "Choose a customer type").required("Customer type is required"),
  companyName: optional(200, "Company name").when("customerType", {
    is: "Company",
    then: (s) => s.required("Company name is required for company customers"),
  }),
  status: yup.string().oneOf(["Active", "Inactive", "Suspended"], "Choose a status").required("Status is required"),
  contactPerson: optional(100, "Contact person"),
  email: optional(100, "Email").email("Enter a valid email address"),
  phone: optional(20, "Phone"),
  website: optional(200, "Website").url("Enter a full web address, including https://"),
  address: optional(500, "Address"),
  city: optional(100, "City"),
  state: optional(100, "State"),
  postalCode: optional(20, "Postal code"),
  country: optional(100, "Country"),
  currencyCode: yup.string().required("Currency is required"),
  creditLimit: yup.number().typeError("Enter an amount").min(0, "Credit limit can't be negative").default(0),
  paymentTerms: optional(50, "Payment terms"),
  taxId: optional(50, "Tax ID"),
  industry: optional(100, "Industry"),
  notes: optional(2000, "Notes"),
});

type FormValues = yup.InferType<typeof schema>;
const FIELD_NAMES = Object.keys(schema.fields);

const EMPTY: FormValues = {
  customerCode: "",
  customerName: "",
  customerType: "Individual",
  companyName: "",
  status: "Active",
  contactPerson: "",
  email: "",
  phone: "",
  website: "",
  address: "",
  city: "",
  state: "",
  postalCode: "",
  country: "",
  currencyCode: "USD",
  creditLimit: 0,
  paymentTerms: "",
  taxId: "",
  industry: "",
  notes: "",
};

const toFormValues = (c: CustomerDto): FormValues => ({
  customerCode: c.customerCode ?? "",
  customerName: c.customerName ?? "",
  // Unknown values are kept so validation flags them instead of silently rewriting data.
  customerType: (c.customerType ?? "Individual") as FormValues["customerType"],
  companyName: c.companyName ?? "",
  status: (c.status ?? "Active") as FormValues["status"],
  contactPerson: c.contactPerson ?? "",
  email: c.email ?? "",
  phone: c.phone ?? "",
  website: c.website ?? "",
  address: c.address ?? "",
  city: c.city ?? "",
  state: c.state ?? "",
  postalCode: c.postalCode ?? "",
  country: c.country ?? "",
  currencyCode: c.currencyCode ?? "USD",
  creditLimit: c.creditLimit ?? 0,
  paymentTerms: c.paymentTerms ?? "",
  taxId: c.taxId ?? "",
  industry: c.industry ?? "",
  notes: c.notes ?? "",
});

const blankToUndefined = (value: string) => (value.trim() ? value.trim() : undefined);

export default function CustomerForm() {
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const navigate = useNavigate();
  const existing = useResource((customerId: string, signal) => customerService.get(customerId, signal), id ?? "", { enabled: isEdit });

  const methods = useForm<FormValues>({ resolver: yupResolver(schema), defaultValues: EMPTY, mode: "onBlur" });
  const { handleSubmit, reset, watch, setError, formState } = methods;
  const [formErrors, setFormErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const guard = useUnsavedChangesGuard(formState.isDirty && !formState.isSubmitSuccessful);

  useEffect(() => {
    if (existing.data) reset(toFormValues(existing.data));
  }, [existing.data, reset]);

  const customerType = watch("customerType");
  const currency = watch("currencyCode");
  const title = isEdit ? `Edit ${existing.data?.customerName ?? "customer"}` : "Add customer";
  const backTo = isEdit ? `/crm/customers/${id}` : "/crm/customers";
  const breadcrumbs = useMemo(
    () => [
      { label: "CRM", to: "/crm/dashboard" },
      { label: "Customers", to: "/crm/customers" },
      ...(isEdit && existing.data ? [{ label: existing.data.customerName, to: backTo }] : []),
      { label: isEdit ? "Edit" : "New" },
    ],
    [isEdit, existing.data, backTo]
  );

  const onSubmit = async (values: FormValues) => {
    setFormErrors([]);
    setSaving(true);
    const payload = {
      customerCode: values.customerCode.trim(),
      customerName: values.customerName.trim(),
      // Individuals have no separate company; the API requires the field, so it mirrors the name.
      companyName: blankToUndefined(values.companyName) ?? values.customerName.trim(),
      customerType: values.customerType,
      status: values.status,
      contactPerson: blankToUndefined(values.contactPerson),
      email: blankToUndefined(values.email),
      phone: blankToUndefined(values.phone),
      website: blankToUndefined(values.website),
      address: blankToUndefined(values.address),
      city: blankToUndefined(values.city),
      state: blankToUndefined(values.state),
      postalCode: blankToUndefined(values.postalCode),
      country: blankToUndefined(values.country),
      currencyCode: values.currencyCode,
      creditLimit: values.creditLimit ?? 0,
      paymentTerms: blankToUndefined(values.paymentTerms),
      taxId: blankToUndefined(values.taxId),
      industry: blankToUndefined(values.industry),
      notes: blankToUndefined(values.notes),
    };
    try {
      if (isEdit && existing.data) {
        // Outstanding balance is maintained by invoices and payments; send back what the server has.
        await customerService.update(id!, { ...toUpdateDto(existing.data), ...payload });
        reset(values);
        toast.success("Customer saved");
        navigate(backTo, { replace: true });
      } else {
        const newId = await customerService.create({ ...payload, companyId: getCompanyId(), outstandingBalance: 0 });
        reset(values);
        toast.success("Customer added");
        navigate(`/crm/customers/${newId}`, { replace: true });
      }
    } catch (err) {
      setFormErrors(applyServerErrors(parseApiError(err), setError, FIELD_NAMES));
      window.scrollTo({ top: 0, behavior: "smooth" });
    } finally {
      setSaving(false);
    }
  };

  if (isEdit && existing.error) {
    return (
      <>
        <PageHeader title="Edit customer" breadcrumbs={breadcrumbs.slice(0, 2)} />
        <div className="card">
          {existing.error.status === 404 ? (
            <EmptyState variant="no-results" title="Customer not found" description="It may have been deleted." />
          ) : (
            <ErrorState error={existing.error} onRetry={existing.refetch} />
          )}
        </div>
      </>
    );
  }

  return (
    <>
      <PageHeader title={title} breadcrumbs={breadcrumbs} description={isEdit ? undefined : "Required fields are marked with an asterisk."} />
      {isEdit && existing.isLoading ? (
        <div className="card p-5">
          <SkeletonFields fields={8} />
        </div>
      ) : (
        <FormProvider {...methods}>
          <form noValidate onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-5xl space-y-4">
            <FormErrorSummary messages={formErrors} />

            <FormSection title="Customer" description="How this customer is identified across quotes, orders and invoices.">
              <TextField name="customerName" label="Customer name" required autoComplete="organization" />
              <TextField name="customerCode" label="Customer code" required help="Your own reference, e.g. C-001. Must be unique." />
              <SelectField name="customerType" label="Customer type" required options={CUSTOMER_TYPE_OPTIONS} />
              <TextField
                name="companyName"
                label="Company name"
                required={customerType === "Company"}
                help={customerType === "Company" ? "Legal or trading name." : "Optional for individuals."}
              />
              <SelectField name="status" label="Status" required options={CUSTOMER_STATUS_OPTIONS} help="Inactive and suspended customers can't be used on new orders." />
              <TextField name="industry" label="Industry" />
            </FormSection>

            <FormSection title="Contact">
              <TextField name="contactPerson" label="Primary contact" autoComplete="name" />
              <TextField name="email" label="Email" type="email" autoComplete="email" />
              <TextField name="phone" label="Phone" type="tel" autoComplete="tel" />
              <TextField name="website" label="Website" type="url" placeholder="https://" />
            </FormSection>

            <FormSection title="Address">
              <FullWidth>
                <TextareaField name="address" label="Street address" rows={2} />
              </FullWidth>
              <TextField name="city" label="City" autoComplete="address-level2" />
              <TextField name="state" label="State / province" autoComplete="address-level1" />
              <TextField name="postalCode" label="Postal code" autoComplete="postal-code" />
              <TextField name="country" label="Country" autoComplete="country-name" />
            </FormSection>

            <FormSection title="Billing" description="Defaults used on this customer's quotes, orders and invoices.">
              <SelectField name="currencyCode" label="Currency" required options={CURRENCY_OPTIONS} />
              <CurrencyField name="creditLimit" label="Credit limit" currency={currency} help="Set 0 for no credit." />
              <SelectField name="paymentTerms" label="Payment terms" options={PAYMENT_TERMS_OPTIONS.filter((o) => o.value)} emptyOption="Not set" />
              <TextField name="taxId" label="Tax ID" />
              {isEdit && existing.data && (
                <div>
                  <p className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Outstanding balance</p>
                  <p className="py-2 text-sm tabular-nums">{formatMoney(existing.data.outstandingBalance, existing.data.currencyCode)}</p>
                  <p className="text-xs text-slate-500">Calculated from invoices and payments.</p>
                </div>
              )}
            </FormSection>

            <FormSection title="Notes" columns={1}>
              <TextareaField name="notes" label="Internal notes" rows={3} help="Not shown to the customer." />
            </FormSection>

            <FormFooter cancelTo={backTo} isSaving={saving} submitLabel={isEdit ? "Save changes" : "Add customer"} />
          </form>
        </FormProvider>
      )}

      <ConfirmDialog
        open={guard.isBlocked}
        title="Leave without saving?"
        description="Your changes to this customer will be lost."
        confirmLabel="Leave page"
        cancelLabel="Keep editing"
        tone="danger"
        onConfirm={guard.proceed}
        onCancel={guard.stay}
      />
    </>
  );
}
