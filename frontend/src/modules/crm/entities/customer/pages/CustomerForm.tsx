import React, { useEffect, useState } from "react";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";
import { useNavigate, useParams } from "react-router-dom";
import { FormInput } from "../../../../../components";
import {
  CrmFormLayout,
  CrmFormFooter,
  FormSection,
  CRM_LABEL_CLASS,
  CRM_INPUT_CLASS,
  CRM_SELECT_CLASS,
  CRM_FIELD_WRAP,
  CRM_TEXTAREA_CLASS,
  optionalString,
  optionalEmail,
  optionalUrl,
} from "../../../components";
import {
  customerService,
  type CreateCustomerDto,
  type UpdateCustomerDto,
} from "../services";
import {
  CUSTOMER_TYPE_OPTIONS,
  CUSTOMER_STATUS_OPTIONS,
  CURRENCY_OPTIONS,
  PAYMENT_TERMS_OPTIONS,
} from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const customerSchema = yup.object({
  customerCode: yup.string().trim().required("Customer code is required"),
  customerName: yup.string().trim().required("Customer name is required"),
  companyName: yup.string().trim().when("customerType", {
    is: "Company",
    then: (schema) => schema.required("Company / organization name is required"),
    otherwise: (schema) => schema.optional(),
  }),
  contactPerson: optionalString(),
  email: optionalEmail(),
  phone: optionalString(),
  address: optionalString(),
  city: optionalString(),
  state: optionalString(),
  country: optionalString(),
  postalCode: optionalString(),
  customerType: yup
    .string()
    .oneOf(["Individual", "Company"], "Invalid customer type")
    .required("Customer type is required"),
  status: yup
    .string()
    .oneOf(["Active", "Inactive", "Suspended"], "Invalid status")
    .required("Status is required"),
  industry: optionalString(),
  creditLimit: yup.number().typeError("Enter a valid amount").min(0, "Must be 0 or greater").default(0),
  outstandingBalance: yup
    .number()
    .typeError("Enter a valid amount")
    .min(0, "Must be 0 or greater")
    .default(0),
  paymentTerms: optionalString(),
  taxId: optionalString(),
  website: optionalUrl(),
  notes: optionalString(),
  currencyCode: yup.string().required("Currency is required"),
});

type CustomerFormValues = yup.InferType<typeof customerSchema>;

const defaultValues: CustomerFormValues = {
  customerCode: "",
  customerName: "",
  companyName: "",
  contactPerson: "",
  email: "",
  phone: "",
  address: "",
  city: "",
  state: "",
  country: "",
  postalCode: "",
  customerType: "Individual",
  status: "Active",
  industry: "",
  creditLimit: 0,
  outstandingBalance: 0,
  paymentTerms: "",
  taxId: "",
  website: "",
  notes: "",
  currencyCode: "USD",
};

const CustomerForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  const methods = useForm<CustomerFormValues>({
    defaultValues,
    resolver: yupResolver(customerSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset, watch } = methods;
  const customerType = watch("customerType");

  useEffect(() => {
    if (isEdit && id) {
      void loadCustomer(id);
    }
  }, [isEdit, id]);

  const loadCustomer = async (customerId: string) => {
    setFetching(true);
    setError(null);
    try {
      const data = await customerService.getById(customerId);
      reset({
        customerCode: data.customerCode ?? "",
        customerName: data.customerName ?? "",
        companyName: data.companyName ?? "",
        contactPerson: data.contactPerson ?? "",
        email: data.email ?? "",
        phone: data.phone ?? "",
        address: data.address ?? "",
        city: data.city ?? "",
        state: data.state ?? "",
        country: data.country ?? "",
        postalCode: data.postalCode ?? "",
        // Unknown values are kept so validation flags them instead of silently rewriting them.
        customerType: (data.customerType ?? "Individual") as CustomerFormValues["customerType"],
        status: (data.status ?? "Active") as CustomerFormValues["status"],
        industry: data.industry ?? "",
        creditLimit: data.creditLimit ?? 0,
        outstandingBalance: data.outstandingBalance ?? 0,
        paymentTerms: data.paymentTerms ?? "",
        taxId: data.taxId ?? "",
        website: data.website ?? "",
        notes: data.notes ?? "",
        currencyCode: data.currencyCode ?? "USD",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load customer");
    } finally {
      setFetching(false);
    }
  };

  const resolveCompanyName = (data: CustomerFormValues) => {
    if (data.customerType === "Company") {
      return data.companyName?.trim() || data.customerName;
    }
    return data.companyName?.trim() || data.customerName;
  };

  const onSubmit = async (data: CustomerFormValues) => {
    setError(null);
    setLoading(true);
    const companyName = resolveCompanyName(data);

    try {
      if (isEdit && id) {
        const updateData: UpdateCustomerDto = {
          id,
          customerCode: data.customerCode,
          customerName: data.customerName,
          companyName,
          contactPerson: data.contactPerson,
          email: data.email,
          phone: data.phone,
          address: data.address,
          city: data.city,
          state: data.state,
          country: data.country,
          postalCode: data.postalCode,
          customerType: data.customerType,
          status: data.status,
          industry: data.industry,
          creditLimit: Number(data.creditLimit) || 0,
          outstandingBalance: Number(data.outstandingBalance) || 0,
          paymentTerms: data.paymentTerms,
          taxId: data.taxId,
          website: data.website,
          notes: data.notes,
          currencyCode: data.currencyCode,
        };
        await customerService.update(id, updateData);
      } else {
        const createData: CreateCustomerDto = {
          companyId: getCompanyId(),
          customerCode: data.customerCode,
          customerName: data.customerName,
          companyName,
          contactPerson: data.contactPerson,
          email: data.email,
          phone: data.phone,
          address: data.address,
          city: data.city,
          state: data.state,
          country: data.country,
          postalCode: data.postalCode,
          customerType: data.customerType,
          status: data.status,
          industry: data.industry,
          creditLimit: Number(data.creditLimit) || 0,
          outstandingBalance: Number(data.outstandingBalance) || 0,
          paymentTerms: data.paymentTerms,
          taxId: data.taxId,
          website: data.website,
          notes: data.notes,
          currencyCode: data.currencyCode,
        };
        await customerService.create(createData);
      }
      navigate("/crm/customers");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error ||
          e.response?.data?.message ||
          `Failed to ${isEdit ? "Save changes" : "Add Customer"} customer`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Customer" : "Add Customer"}
      showBreadcrumb={false}
      title={isEdit ? "Edit Customer" : "Add Customer"}
      subtitle={
        isEdit ? "Update customer profile and billing details" : "Add a new customer to your CRM"
      }
      backUrl="/crm/customers"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
              <FormSection
                title="Basic information"
                description="Core identity and classification for this customer."
              >
                <FormInput
                  label="Customer code"
                  type="text"
                  name="customerCode"
                  placeholder="e.g. CUST-001"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  required
                />
                <FormInput
                  label="Customer name"
                  type="text"
                  name="customerName"
                  placeholder="Display name"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  required
                />
                <FormInput
                  label={customerType === "Company" ? "Organization name" : "Company name (optional)"}
                  type="text"
                  name="companyName"
                  placeholder={
                    customerType === "Company" ? "Legal company name" : "Leave blank to use customer name"
                  }
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  required={customerType === "Company"}
                />
                <FormInput
                  label="Customer type"
                  type="select"
                  name="customerType"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_SELECT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  required
                >
                  {CUSTOMER_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>
                <FormInput
                  label="Status"
                  type="select"
                  name="status"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_SELECT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  required
                >
                  {CUSTOMER_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>
                <FormInput
                  label="Industry"
                  type="text"
                  name="industry"
                  placeholder="e.g. Technology, Retail"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
              </FormSection>

              <FormSection
                title="Contact details"
                description="How to reach this customer."
              >
                <FormInput
                  label="Contact person"
                  type="text"
                  name="contactPerson"
                  placeholder="Primary contact name"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
                <FormInput
                  label="Email"
                  type="email"
                  name="email"
                  placeholder="name@company.com"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
                <FormInput
                  label="Phone"
                  type="tel"
                  name="phone"
                  placeholder="+1 555 000 0000"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
                <FormInput
                  label="Website"
                  type="url"
                  name="website"
                  placeholder="https://example.com"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
              </FormSection>

              <FormSection
                title="Address"
                description="Billing or primary business location."
              >
                <div className="sm:col-span-2">
                  <FormInput
                    label="Street address"
                    type="text"
                    name="address"
                    placeholder="Street, building, suite"
                    containerClass={CRM_FIELD_WRAP}
                    className={CRM_INPUT_CLASS}
                    labelClassName={CRM_LABEL_CLASS}
                  />
                </div>
                <FormInput
                  label="City"
                  type="text"
                  name="city"
                  placeholder="City"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
                <FormInput
                  label="State / province"
                  type="text"
                  name="state"
                  placeholder="State or region"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
                <FormInput
                  label="Country"
                  type="text"
                  name="country"
                  placeholder="Country"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
                <FormInput
                  label="Postal code"
                  type="text"
                  name="postalCode"
                  placeholder="ZIP / postal code"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
              </FormSection>

              <FormSection
                title="Financial"
                description="Credit, currency, and tax settings."
              >
                <FormInput
                  label="Currency"
                  type="select"
                  name="currencyCode"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_SELECT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  required
                >
                  {CURRENCY_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>
                <FormInput
                  label="Payment terms"
                  type="select"
                  name="paymentTerms"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_SELECT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                >
                  {PAYMENT_TERMS_OPTIONS.map((option) => (
                    <option key={option.value || "none"} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>
                <FormInput
                  label="Credit limit"
                  type="number"
                  name="creditLimit"
                  placeholder="0.00"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  step="0.01"
                  min="0"
                />
                <FormInput
                  label="Outstanding balance"
                  type="number"
                  name="outstandingBalance"
                  placeholder="0.00"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                  step="0.01"
                  min="0"
                />
                <FormInput
                  label="Tax ID / VAT"
                  type="text"
                  name="taxId"
                  placeholder="Tax registration number"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_INPUT_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
              </FormSection>

              <FormSection title="Notes" description="Internal notes about this customer." columns={1}>
                <FormInput
                  label="Notes"
                  type="textarea"
                  name="notes"
                  rows={4}
                  placeholder="Additional information, preferences, or reminders…"
                  containerClass={CRM_FIELD_WRAP}
                  className={CRM_TEXTAREA_CLASS}
                  labelClassName={CRM_LABEL_CLASS}
                />
              </FormSection>

              <CrmFormFooter
                cancelUrl="/crm/customers"
                loading={loading}
                isEdit={isEdit}
                createLabel="Add customer"
              />
            </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default CustomerForm;
