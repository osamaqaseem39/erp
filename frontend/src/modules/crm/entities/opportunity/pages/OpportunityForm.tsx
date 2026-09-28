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
  optionalNumber,
} from "../../../components";
import { opportunityService } from "../services";
import type { CreateOpportunityDto, UpdateOpportunityDto } from "../types";
import { customerService } from "../../customer/services";
import { leadService } from "../../lead/services";
import { leadDisplayName } from "../../lead/leadHubUtils";
import { userService } from "../../../../core/entities/user/services";
import { OPPORTUNITY_STAGE_OPTIONS, OPPORTUNITY_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const opportunitySchema = yup.object({
  opportunityCode: optionalString(),
  opportunityName: optionalString(),
  title: optionalString(),
  customerId: optionalUuid(),
  leadId: optionalUuid(),
  stage: optionalString(),
  status: optionalString(),
  estimatedValue: optionalNumber(0),
  actualValue: optionalNumber(0),
  amount: optionalNumber(0),
  currency: optionalString(),
  probability: optionalNumber(0, 100),
  expectedCloseDate: optionalString(),
  description: optionalString(),
  notes: optionalString(),
  source: optionalString(),
  assignedToUserId: optionalUuid(),
});

type OpportunityFormValues = yup.InferType<typeof opportunitySchema>;

const defaultValues: OpportunityFormValues = {
  opportunityCode: "",
  opportunityName: "",
  title: "",
  customerId: "",
  leadId: "",
  stage: "Prospecting",
  status: "Open",
  estimatedValue: 0,
  actualValue: 0,
  amount: 0,
  currency: "USD",
  probability: 0,
  expectedCloseDate: "",
  description: "",
  notes: "",
  source: "",
  assignedToUserId: "",
};

const OpportunityForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const leadIdParam = searchParams.get("leadId");
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [customers, setCustomers] = useState<any[]>([]);
  const [leads, setLeads] = useState<any[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  const methods = useForm<OpportunityFormValues>({
    defaultValues: {
      ...defaultValues,
      leadId: leadIdParam || "",
    },
    resolver: yupResolver(opportunitySchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset, setValue } = methods;

  useEffect(() => {
    void loadCustomers();
    void loadLeads();
    void loadUsers();
    if (isEdit && id) {
      void loadOpportunity(id);
    } else if (leadIdParam) {
      void prefillFromLead(leadIdParam);
    }
  }, [isEdit, id, leadIdParam]);

  const prefillFromLead = async (leadId: string) => {
    try {
      const lead = await leadService.getById(leadId);
      setValue("leadId", leadId);
      const name = leadDisplayName(lead);
      if (name) {
        setValue("title", name);
        setValue("opportunityName", lead.companyName ? `${name} — ${lead.companyName}` : name);
      }
      if (lead.estimatedValue != null) setValue("estimatedValue", lead.estimatedValue);
      if (lead.estimatedValue != null) setValue("amount", lead.estimatedValue);
      if (lead.leadSource || lead.source) setValue("source", lead.leadSource || lead.source);
      if (lead.assignedToUserId) setValue("assignedToUserId", lead.assignedToUserId);
      if (lead.notes) setValue("description", lead.notes);
    } catch {
      setValue("leadId", leadId);
    }
  };

  const loadOpportunity = async (opportunityId: string) => {
    setFetching(true);
    setError(null);
    try {
      const opportunity = await opportunityService.getById(opportunityId);
      reset({
        opportunityCode: opportunity.opportunityCode ?? "",
        opportunityName: opportunity.opportunityName ?? "",
        title: opportunity.title ?? "",
        customerId: opportunity.customerId ?? "",
        leadId: opportunity.leadId ?? "",
        stage: opportunity.stage ?? "Prospecting",
        status: opportunity.status ?? "Open",
        estimatedValue: opportunity.estimatedValue ?? 0,
        actualValue: opportunity.actualValue ?? 0,
        amount: opportunity.amount ?? 0,
        currency: opportunity.currency ?? "USD",
        probability: opportunity.probability ?? 0,
        expectedCloseDate: opportunity.expectedCloseDate
          ? opportunity.expectedCloseDate.split("T")[0]
          : "",
        description: opportunity.description ?? "",
        notes: opportunity.notes ?? "",
        source: opportunity.source ?? "",
        assignedToUserId: opportunity.assignedToUserId ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error || e.response?.data?.message || "Failed to load opportunity"
      );
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

  const loadUsers = async () => {
    try {
      const data = await userService.getAll();
      setUsers(data);
    } catch (err: unknown) {
      console.error("Failed to load users", err);
    }
  };

  const onSubmit = async (data: OpportunityFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateOpportunityDto = {
          id,
          opportunityCode: data.opportunityCode || undefined,
          opportunityName: data.opportunityName || undefined,
          title: data.title || undefined,
          customerId: data.customerId || undefined,
          leadId: data.leadId || undefined,
          stage: data.stage || "Prospecting",
          status: data.status || "Open",
          estimatedValue: data.estimatedValue ? parseFloat(String(data.estimatedValue)) : undefined,
          actualValue: data.actualValue ? parseFloat(String(data.actualValue)) : undefined,
          amount: data.amount ? parseFloat(String(data.amount)) : undefined,
          currency: data.currency || "USD",
          probability: data.probability ? parseFloat(String(data.probability)) : undefined,
          expectedCloseDate: data.expectedCloseDate || undefined,
          description: data.description || undefined,
          notes: data.notes || undefined,
          source: data.source || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
        };
        await opportunityService.update(id, updateData);
      } else {
        const createData: CreateOpportunityDto = {
          opportunityCode: data.opportunityCode || undefined,
          opportunityName: data.opportunityName || undefined,
          title: data.title || undefined,
          customerId: data.customerId || undefined,
          leadId: data.leadId || undefined,
          stage: data.stage || "Prospecting",
          status: data.status || "Open",
          estimatedValue: data.estimatedValue ? parseFloat(String(data.estimatedValue)) : undefined,
          actualValue: data.actualValue ? parseFloat(String(data.actualValue)) : undefined,
          amount: data.amount ? parseFloat(String(data.amount)) : undefined,
          currency: data.currency || "USD",
          probability: data.probability ? parseFloat(String(data.probability)) : undefined,
          expectedCloseDate: data.expectedCloseDate || undefined,
          description: data.description || undefined,
          notes: data.notes || undefined,
          source: data.source || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
        };
        await opportunityService.create(createData);
      }
      navigate("/crm/opportunities");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error || e.response?.data?.message || "Failed to save opportunity"
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Opportunity" : "Add Opportunity"}
      title={isEdit ? "Edit Opportunity" : "Add Opportunity"}
      subtitle={
        isEdit
          ? "Update opportunity details and pipeline stage"
          : "Track a new sales opportunity in your pipeline"
      }
      backUrl="/crm/opportunities"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Opportunity details"
            description="Core information and relationships."
          >
            <FormInput
              label="Opportunity code"
              type="text"
              name="opportunityCode"
              placeholder="Auto-generated if empty"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Title"
              type="text"
              name="title"
              placeholder="Enter opportunity title"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
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
              label="Stage"
              type="select"
              name="stage"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              {OPPORTUNITY_STAGE_OPTIONS.map((option) => (
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
              {OPPORTUNITY_STATUS_OPTIONS.map((option) => (
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
          </FormSection>

          <FormSection title="Financial" description="Value, currency, and close date.">
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
              label="Amount"
              type="number"
              name="amount"
              placeholder="0.00"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="0.01"
              min="0"
            />
            <FormInput
              label="Currency"
              type="text"
              name="currency"
              placeholder="USD"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Probability (%)"
              type="number"
              name="probability"
              placeholder="0–100"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="1"
              min="0"
              max="100"
            />
            <FormInput
              label="Expected close date"
              type="date"
              name="expectedCloseDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Notes" description="Description and internal notes." columns={1}>
            <FormInput
              label="Description"
              type="textarea"
              name="description"
              rows={3}
              placeholder="Opportunity description"
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
            cancelUrl="/crm/opportunities"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add opportunity"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default OpportunityForm;
