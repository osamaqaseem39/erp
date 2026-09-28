import React, { useEffect, useState } from "react";
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
  optionalUuid,
  optionalNumber,
} from "../../../components";
import { leadService } from "../services";
import type { CreateLeadDto, UpdateLeadDto } from "../types";
import { userService } from "../../../../core/entities/user/services";
import { LEAD_SOURCE_OPTIONS, LEAD_STATUS_OPTIONS, LEAD_PRIORITY_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const leadSchema = yup.object({
  leadCode: yup.string().trim().required("Lead code is required"),
  firstName: yup.string().trim().required("First name is required"),
  lastName: yup.string().trim().required("Last name is required"),
  companyName: optionalString(),
  email: optionalEmail(),
  phone: optionalString(),
  address: optionalString(),
  city: optionalString(),
  state: optionalString(),
  country: optionalString(),
  leadSource: optionalString(),
  source: optionalString(),
  status: optionalString(),
  priority: optionalString(),
  estimatedValue: optionalNumber(0),
  lastContactDate: optionalString(),
  assignedToUserId: optionalUuid(),
  contactPerson: optionalString(),
  postalCode: optionalString(),
  industry: optionalString(),
  leadScore: optionalNumber(0, 100),
  notes: optionalString(),
});

type LeadFormValues = yup.InferType<typeof leadSchema>;

const defaultValues: LeadFormValues = {
  leadCode: "",
  firstName: "",
  lastName: "",
  companyName: "",
  email: "",
  phone: "",
  address: "",
  city: "",
  state: "",
  country: "",
  leadSource: "",
  source: "",
  status: "New",
  priority: "Medium",
  estimatedValue: 0,
  lastContactDate: "",
  assignedToUserId: "",
  contactPerson: "",
  postalCode: "",
  industry: "",
  leadScore: 0,
  notes: "",
};

const LeadForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [users, setUsers] = useState<any[]>([]);

  const methods = useForm<LeadFormValues>({
    defaultValues,
    resolver: yupResolver(leadSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    void loadUsers();
    if (isEdit && id) {
      void loadLead(id);
    }
  }, [isEdit, id]);

  const loadLead = async (leadId: string) => {
    setFetching(true);
    setError(null);
    try {
      const lead = await leadService.getById(leadId);
      reset({
        leadCode: lead.leadCode ?? "",
        firstName: lead.firstName ?? "",
        lastName: lead.lastName ?? "",
        companyName: lead.companyName ?? "",
        email: lead.email ?? "",
        phone: lead.phone ?? "",
        address: lead.address ?? "",
        city: lead.city ?? "",
        state: lead.state ?? "",
        country: lead.country ?? "",
        leadSource: lead.leadSource ?? "",
        source: lead.source ?? "",
        status: lead.status ?? "New",
        priority: lead.priority ?? "Medium",
        estimatedValue: lead.estimatedValue ?? 0,
        lastContactDate: lead.lastContactDate ? lead.lastContactDate.split("T")[0] : "",
        assignedToUserId: lead.assignedToUserId ?? "",
        contactPerson: lead.contactPerson ?? "",
        postalCode: lead.postalCode ?? "",
        industry: lead.industry ?? "",
        leadScore: lead.leadScore ?? 0,
        notes: lead.notes ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load lead");
    } finally {
      setFetching(false);
    }
  };

  const loadUsers = async () => {
    try {
      const data = await userService.getAll();
      setUsers(data);
    } catch (err: unknown) {
      console.error("Failed to load users", err);
    }
  };

  const onSubmit = async (data: LeadFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateLeadDto = {
          id,
          leadCode: data.leadCode,
          firstName: data.firstName,
          lastName: data.lastName,
          companyName: data.companyName || undefined,
          email: data.email || undefined,
          phone: data.phone || undefined,
          address: data.address || undefined,
          city: data.city || undefined,
          state: data.state || undefined,
          country: data.country || undefined,
          leadSource: data.leadSource || "",
          source: data.source || "",
          status: data.status || "New",
          priority: data.priority || "Medium",
          estimatedValue: data.estimatedValue ? parseFloat(String(data.estimatedValue)) : undefined,
          lastContactDate: data.lastContactDate || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
          contactPerson: data.contactPerson || undefined,
          postalCode: data.postalCode || undefined,
          industry: data.industry || undefined,
          leadScore: data.leadScore ? parseInt(String(data.leadScore)) : undefined,
          notes: data.notes || undefined,
        };
        await leadService.update(id, updateData);
      } else {
        const createData: CreateLeadDto = {
          leadCode: data.leadCode,
          firstName: data.firstName,
          lastName: data.lastName,
          companyName: data.companyName || undefined,
          email: data.email || undefined,
          phone: data.phone || undefined,
          address: data.address || undefined,
          city: data.city || undefined,
          state: data.state || undefined,
          country: data.country || undefined,
          leadSource: data.leadSource || "",
          source: data.source || "",
          status: data.status || "New",
          priority: data.priority || "Medium",
          estimatedValue: data.estimatedValue ? parseFloat(String(data.estimatedValue)) : undefined,
          lastContactDate: data.lastContactDate || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
          contactPerson: data.contactPerson || undefined,
          postalCode: data.postalCode || undefined,
          industry: data.industry || undefined,
          leadScore: data.leadScore ? parseInt(String(data.leadScore)) : undefined,
          notes: data.notes || undefined,
        };
        await leadService.create(createData);
      }
      navigate("/crm/leads");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to save lead");
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Lead" : "Add Lead"}
      title={isEdit ? "Edit Lead" : "Add Lead"}
      subtitle={isEdit ? "Update lead details and status" : "Add a new sales lead to your pipeline"}
      backUrl="/crm/leads"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Basic information"
            description="Contact identity and company details."
          >
            <FormInput
              label="Lead code"
              type="text"
              name="leadCode"
              placeholder="Enter lead code"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="First name"
              type="text"
              name="firstName"
              placeholder="Enter first name"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Last name"
              type="text"
              name="lastName"
              placeholder="Enter last name"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Company name"
              type="text"
              name="companyName"
              placeholder="Enter company name"
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
              placeholder="Enter phone number"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Contact person"
              type="text"
              name="contactPerson"
              placeholder="Enter contact person"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Industry"
              type="text"
              name="industry"
              placeholder="Enter industry"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Address" description="Lead location details.">
            <div className="sm:col-span-2">
              <FormInput
                label="Address"
                type="text"
                name="address"
                placeholder="Enter address"
                containerClass={CRM_FIELD_WRAP}
                className={CRM_INPUT_CLASS}
                labelClassName={CRM_LABEL_CLASS}
              />
            </div>
            <FormInput
              label="City"
              type="text"
              name="city"
              placeholder="Enter city"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="State"
              type="text"
              name="state"
              placeholder="Enter state"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Country"
              type="text"
              name="country"
              placeholder="Enter country"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Postal code"
              type="text"
              name="postalCode"
              placeholder="Enter postal code"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Lead details" description="Source, status, and assignment.">
            <FormInput
              label="Lead source"
              type="select"
              name="leadSource"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select source</option>
              {LEAD_SOURCE_OPTIONS.map((option) => (
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
            >
              {LEAD_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Priority"
              type="select"
              name="priority"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              {LEAD_PRIORITY_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Assigned to"
              type="select"
              name="assignedToUserId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select user</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username || user.email}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Estimated value"
              type="number"
              name="estimatedValue"
              placeholder="0.00"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="0.01"
              min="0"
            />
            <FormInput
              label="Last contact date"
              type="date"
              name="lastContactDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Lead score"
              type="number"
              name="leadScore"
              placeholder="0–100"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="1"
              min="0"
              max="100"
            />
          </FormSection>

          <FormSection title="Notes" description="Additional information about this lead." columns={1}>
            <FormInput
              label="Notes"
              type="textarea"
              name="notes"
              rows={4}
              placeholder="Additional notes"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/leads"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add lead"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default LeadForm;
