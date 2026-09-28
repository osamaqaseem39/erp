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
  optionalUuid,
} from "../../../components";
import { activityService } from "../services";
import type { CreateActivityDto, UpdateActivityDto } from "../types";
import { customerService } from "../../customer/services";
import { leadService } from "../../lead/services";
import { leadDisplayName } from "../../lead/leadHubUtils";
import { opportunityService } from "../../opportunity/services";
import { userService } from "../../../../core/entities/user/services";
import {
  ACTIVITY_TYPE_OPTIONS,
  ACTIVITY_STATUS_OPTIONS,
  ACTIVITY_PRIORITY_OPTIONS,
} from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const activitySchema = yup.object({
  activityType: yup.string().trim().required("Activity type is required"),
  subject: yup.string().trim().required("Subject is required"),
  description: optionalString(),
  startDate: yup.string().trim().required("Start date is required"),
  endDate: optionalString(),
  status: optionalString(),
  priority: optionalString(),
  assignedToUserId: yup.string().trim().required("Assigned to is required").uuid("Invalid user"),
  location: optionalString(),
  notes: optionalString(),
  customerId: optionalUuid(),
  leadId: optionalUuid(),
  opportunityId: optionalUuid(),
});

type ActivityFormValues = yup.InferType<typeof activitySchema>;

const defaultValues: ActivityFormValues = {
  activityType: "",
  subject: "",
  description: "",
  startDate: new Date().toISOString().slice(0, 16),
  endDate: "",
  status: "Open",
  priority: "Medium",
  assignedToUserId: "",
  location: "",
  notes: "",
  customerId: "",
  leadId: "",
  opportunityId: "",
};

const formatDateTimeForInput = (dateString?: string): string => {
  if (!dateString) return "";
  const date = new Date(dateString);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
};

const ActivityForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const leadIdParam = searchParams.get("leadId");
  const opportunityIdParam = searchParams.get("opportunityId");
  const customerIdParam = searchParams.get("customerId");
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [customers, setCustomers] = useState<any[]>([]);
  const [leads, setLeads] = useState<any[]>([]);
  const [opportunities, setOpportunities] = useState<any[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  const methods = useForm<ActivityFormValues>({
    defaultValues: {
      ...defaultValues,
      leadId: leadIdParam || "",
      opportunityId: opportunityIdParam || "",
      customerId: customerIdParam || "",
    },
    resolver: yupResolver(activitySchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset, setValue } = methods;

  useEffect(() => {
    void loadCustomers();
    void loadLeads();
    void loadOpportunities();
    void loadUsers();
    if (isEdit && id) {
      void loadActivity(id);
    } else if (leadIdParam) {
      void prefillFromLead(leadIdParam);
    }
  }, [isEdit, id, leadIdParam]);

  const prefillFromLead = async (leadId: string) => {
    try {
      const lead = await leadService.getById(leadId);
      setValue("leadId", leadId);
      if (lead.assignedToUserId) setValue("assignedToUserId", lead.assignedToUserId);
      if (lead.priority) setValue("priority", lead.priority);
      const name = leadDisplayName(lead);
      setValue("subject", name ? `Follow up: ${name}` : "Follow up");
      if (lead.companyName) {
        setValue("description", `Company: ${lead.companyName}${lead.email ? `\nEmail: ${lead.email}` : ""}`);
      }
    } catch {
      setValue("leadId", leadId);
    }
  };

  const loadActivity = async (activityId: string) => {
    setFetching(true);
    setError(null);
    try {
      const activity = await activityService.getById(activityId);
      reset({
        activityType: activity.activityType ?? "",
        subject: activity.subject ?? "",
        description: activity.description ?? "",
        startDate: formatDateTimeForInput(activity.startDate),
        endDate: formatDateTimeForInput(activity.endDate),
        status: activity.status ?? "Open",
        priority: activity.priority ?? "Medium",
        assignedToUserId: activity.assignedToUserId ?? "",
        location: activity.location ?? "",
        notes: activity.notes ?? "",
        customerId: activity.customerId ?? "",
        leadId: activity.leadId ?? "",
        opportunityId: activity.opportunityId ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load activity");
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

  const loadLeads = async () => {
    try {
      const data = await leadService.getAll();
      setLeads(data);
    } catch (err: unknown) {
      console.error("Failed to load leads", err);
    }
  };

  const loadOpportunities = async () => {
    try {
      const data = await opportunityService.getAll();
      setOpportunities(data);
    } catch (err: unknown) {
      console.error("Failed to load opportunities", err);
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

  const onSubmit = async (data: ActivityFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateActivityDto = {
          id,
          activityType: data.activityType,
          subject: data.subject,
          description: data.description || undefined,
          startDate: data.startDate,
          endDate: data.endDate || undefined,
          status: data.status || "Open",
          priority: data.priority || "Medium",
          assignedToUserId: data.assignedToUserId,
          location: data.location || undefined,
          notes: data.notes || undefined,
          customerId: data.customerId || undefined,
          leadId: data.leadId || undefined,
          opportunityId: data.opportunityId || undefined,
        };
        await activityService.update(id, updateData);
      } else {
        const createData: CreateActivityDto = {
          activityType: data.activityType,
          subject: data.subject,
          description: data.description || undefined,
          startDate: data.startDate,
          endDate: data.endDate || undefined,
          status: data.status || "Open",
          priority: data.priority || "Medium",
          assignedToUserId: data.assignedToUserId,
          location: data.location || undefined,
          notes: data.notes || undefined,
          customerId: data.customerId || undefined,
          leadId: data.leadId || undefined,
          opportunityId: data.opportunityId || undefined,
        };
        await activityService.create(createData);
      }
      navigate("/crm/activities");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to save activity");
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Activity" : "Add Activity"}
      title={isEdit ? "Edit Activity" : "Add Activity"}
      subtitle={
        isEdit ? "Update activity details and schedule" : "Log a call, meeting, or task"
      }
      backUrl="/crm/activities"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection title="Activity details" description="Type, subject, and schedule.">
            <FormInput
              label="Activity type"
              type="select"
              name="activityType"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            >
              <option value="">Select activity type</option>
              {ACTIVITY_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Subject"
              type="text"
              name="subject"
              placeholder="Enter subject"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Start date & time"
              type="datetime-local"
              name="startDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="End date & time"
              type="datetime-local"
              name="endDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Status"
              type="select"
              name="status"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              {ACTIVITY_STATUS_OPTIONS.map((option) => (
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
              {ACTIVITY_PRIORITY_OPTIONS.map((option) => (
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
              required
            >
              <option value="">Select user</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username || user.email}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Location"
              type="text"
              name="location"
              placeholder="Enter location"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Related records" description="Link to customer, lead, or opportunity.">
            <FormInput
              label="Customer"
              type="select"
              name="customerId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select customer</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.customerName} ({customer.companyName})
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Lead"
              type="select"
              name="leadId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select lead</option>
              {leads.map((lead) => (
                <option key={lead.id} value={lead.id}>
                  {lead.leadCode} - {lead.firstName} {lead.lastName}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Opportunity"
              type="select"
              name="opportunityId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select opportunity</option>
              {opportunities.map((opp) => (
                <option key={opp.id} value={opp.id}>
                  {opp.opportunityCode || opp.title || opp.opportunityName}
                </option>
              ))}
            </FormInput>
          </FormSection>

          <FormSection title="Notes" description="Description and additional notes." columns={1}>
            <FormInput
              label="Description"
              type="textarea"
              name="description"
              rows={4}
              placeholder="Activity description"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Notes"
              type="textarea"
              name="notes"
              rows={3}
              placeholder="Additional notes"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/activities"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add activity"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default ActivityForm;
