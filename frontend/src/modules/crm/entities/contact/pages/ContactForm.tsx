import React, { useEffect, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
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
} from "../../../components";
import { contactService } from "../services";
import type { CreateContactDto, UpdateContactDto } from "../types";
import { customerService, type CustomerDto } from "../../customer/services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const contactSchema = yup.object({
  customerId: yup.string().uuid("Invalid customer").required("Customer is required"),
  firstName: yup.string().trim().required("First name is required"),
  lastName: yup.string().trim().required("Last name is required"),
  jobTitle: optionalString(),
  department: optionalString(),
  email: optionalEmail(),
  phone: optionalString(),
  mobile: optionalString(),
  address: optionalString(),
  city: optionalString(),
  state: optionalString(),
  country: optionalString(),
  postalCode: optionalString(),
  isPrimary: yup.boolean().default(false),
  notes: optionalString(),
});

type ContactFormValues = yup.InferType<typeof contactSchema>;

const ContactForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const customerIdParam = searchParams.get("customerId");
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);

  const defaultValues: ContactFormValues = {
    customerId: customerIdParam || "",
    firstName: "",
    lastName: "",
    jobTitle: "",
    department: "",
    email: "",
    phone: "",
    mobile: "",
    address: "",
    city: "",
    state: "",
    country: "",
    postalCode: "",
    isPrimary: false,
    notes: "",
  };

  const methods = useForm<ContactFormValues>({
    defaultValues,
    resolver: yupResolver(contactSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    void loadCustomers();
    if (isEdit && id) {
      void loadContact(id);
    }
  }, [isEdit, id]);

  const loadContact = async (contactId: string) => {
    setFetching(true);
    setError(null);
    try {
      const data = await contactService.getById(contactId);
      reset({
        customerId: data.customerId ?? "",
        firstName: data.firstName ?? "",
        lastName: data.lastName ?? "",
        jobTitle: data.jobTitle ?? "",
        department: data.department ?? "",
        email: data.email ?? "",
        phone: data.phone ?? "",
        mobile: data.mobile ?? "",
        address: data.address ?? "",
        city: data.city ?? "",
        state: data.state ?? "",
        country: data.country ?? "",
        postalCode: data.postalCode ?? "",
        isPrimary: data.isPrimary ?? false,
        notes: data.notes ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load contact");
    } finally {
      setFetching(false);
    }
  };

  const loadCustomers = async () => {
    try {
      const data = await customerService.getAll();
      setCustomers(data);
    } catch (err: unknown) {
      console.error("Failed to load customers", err);
    }
  };

  const onSubmit = async (data: ContactFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateContactDto = {
          id,
          firstName: data.firstName,
          lastName: data.lastName,
          jobTitle: data.jobTitle || undefined,
          department: data.department || undefined,
          email: data.email || undefined,
          phone: data.phone || undefined,
          mobile: data.mobile || undefined,
          address: data.address || undefined,
          city: data.city || undefined,
          state: data.state || undefined,
          country: data.country || undefined,
          postalCode: data.postalCode || undefined,
          isPrimary: data.isPrimary ?? false,
          notes: data.notes || undefined,
        };
        await contactService.update(id, updateData);
      } else {
        const createData: CreateContactDto = {
          customerId: data.customerId,
          firstName: data.firstName,
          lastName: data.lastName,
          jobTitle: data.jobTitle || undefined,
          department: data.department || undefined,
          email: data.email || undefined,
          phone: data.phone || undefined,
          mobile: data.mobile || undefined,
          address: data.address || undefined,
          city: data.city || undefined,
          state: data.state || undefined,
          country: data.country || undefined,
          postalCode: data.postalCode || undefined,
          isPrimary: data.isPrimary ?? false,
          notes: data.notes || undefined,
        };
        await contactService.create(createData);
      }
      navigate("/crm/contacts");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error ||
          e.response?.data?.message ||
          `Failed to ${isEdit ? "Save changes" : "Add Contact"} contact`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Contact" : "Add Contact"}
      title={isEdit ? "Edit Contact" : "Add Contact"}
      subtitle={
        isEdit
          ? "Update contact person details for this customer"
          : "Add a contact person linked to a customer account"
      }
      backUrl="/crm/contacts"
      fetching={fetching}
      error={error}
      maxWidth="wide"
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Basic information"
            description="Customer account and primary contact flag."
          >
            <FormInput
              label="Customer"
              type="select"
              name="customerId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
              disabled={!!customerIdParam || isEdit}
            >
              <option value="">Select customer</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.customerName} ({customer.companyName})
                </option>
              ))}
            </FormInput>
            <div className={CRM_FIELD_WRAP}>
              <FormInput
                label="Primary contact"
                type="checkbox"
                name="isPrimary"
                containerClass="flex items-center gap-2"
                labelClassName={`${CRM_LABEL_CLASS} mb-0`}
                className="form-checkbox rounded"
              />
            </div>
            <FormInput
              label="First name"
              type="text"
              name="firstName"
              placeholder="First name"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Last name"
              type="text"
              name="lastName"
              placeholder="Last name"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Job title"
              type="text"
              name="jobTitle"
              placeholder="e.g. Purchasing Manager"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Department"
              type="text"
              name="department"
              placeholder="e.g. Finance, Operations"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Contact details" description="How to reach this person.">
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
              placeholder="Office phone"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Mobile"
              type="tel"
              name="mobile"
              placeholder="Mobile number"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Address" description="Contact location (optional).">
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

          <FormSection title="Notes" description="Internal notes about this contact." columns={1}>
            <FormInput
              label="Notes"
              type="textarea"
              name="notes"
              rows={4}
              placeholder="Preferences, role context, or reminders…"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/contacts"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add contact"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default ContactForm;
