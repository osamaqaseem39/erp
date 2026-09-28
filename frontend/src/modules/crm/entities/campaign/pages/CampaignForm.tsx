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
  optionalNumber,
} from "../../../components";
import { campaignService } from "../services";
import type { CreateCampaignDto, UpdateCampaignDto } from "../types";
import { currencyService } from "../../../../finance/entities/currency/services";
import { CAMPAIGN_TYPE_OPTIONS, CAMPAIGN_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const campaignSchema = yup.object({
  campaignName: yup.string().trim().required("Campaign name is required"),
  campaignType: optionalString(),
  status: optionalString(),
  startDate: optionalString(),
  endDate: optionalString(),
  budget: optionalNumber(0),
  actualCost: optionalNumber(0),
  expectedResponse: optionalNumber(0),
  actualResponse: optionalNumber(0),
  description: optionalString(),
  currency: optionalString(),
  targetAudience: optionalString(),
  goals: optionalString(),
});

type CampaignFormValues = yup.InferType<typeof campaignSchema>;

const defaultValues: CampaignFormValues = {
  campaignName: "",
  campaignType: "",
  status: "Draft",
  startDate: "",
  endDate: "",
  budget: 0,
  actualCost: 0,
  expectedResponse: 0,
  actualResponse: 0,
  description: "",
  currency: "USD",
  targetAudience: "",
  goals: "",
};

const CampaignForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [currencies, setCurrencies] = useState<any[]>([]);

  const methods = useForm<CampaignFormValues>({
    defaultValues,
    resolver: yupResolver(campaignSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    void loadCurrencies();
    if (isEdit && id) {
      void loadCampaign(id);
    }
  }, [isEdit, id]);

  const loadCampaign = async (campaignId: string) => {
    setFetching(true);
    setError(null);
    try {
      const campaign = await campaignService.getById(campaignId);
      reset({
        campaignName: campaign.campaignName ?? "",
        campaignType: campaign.campaignType ?? "",
        status: campaign.status ?? "Draft",
        startDate: campaign.startDate ? campaign.startDate.split("T")[0] : "",
        endDate: campaign.endDate ? campaign.endDate.split("T")[0] : "",
        budget: campaign.budget ?? 0,
        actualCost: campaign.actualCost ?? 0,
        expectedResponse: campaign.expectedResponse ?? 0,
        actualResponse: campaign.actualResponse ?? 0,
        description: campaign.description ?? "",
        currency: campaign.currency ?? "USD",
        targetAudience: campaign.targetAudience ?? "",
        goals: campaign.goals ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load campaign");
    } finally {
      setFetching(false);
    }
  };

  const loadCurrencies = async () => {
    try {
      const data = await currencyService.getActive();
      setCurrencies(data);
    } catch (err: unknown) {
      console.error("Failed to load currencies", err);
    }
  };

  const onSubmit = async (data: CampaignFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateCampaignDto = {
          id,
          campaignName: data.campaignName,
          campaignType: data.campaignType || undefined,
          status: data.status || "Draft",
          startDate: data.startDate || undefined,
          endDate: data.endDate || undefined,
          budget: data.budget ? parseFloat(String(data.budget)) : undefined,
          actualCost: data.actualCost ? parseFloat(String(data.actualCost)) : undefined,
          expectedResponse: data.expectedResponse
            ? parseInt(String(data.expectedResponse))
            : undefined,
          actualResponse: data.actualResponse ? parseInt(String(data.actualResponse)) : undefined,
          description: data.description || undefined,
          currency: data.currency || undefined,
          targetAudience: data.targetAudience || undefined,
          goals: data.goals || undefined,
        };
        await campaignService.update(id, updateData);
      } else {
        const createData: CreateCampaignDto = {
          campaignName: data.campaignName,
          campaignType: data.campaignType || undefined,
          status: data.status || "Draft",
          startDate: data.startDate || undefined,
          endDate: data.endDate || undefined,
          budget: data.budget ? parseFloat(String(data.budget)) : undefined,
          actualCost: data.actualCost ? parseFloat(String(data.actualCost)) : undefined,
          expectedResponse: data.expectedResponse
            ? parseInt(String(data.expectedResponse))
            : undefined,
          actualResponse: data.actualResponse ? parseInt(String(data.actualResponse)) : undefined,
          description: data.description || undefined,
          currency: data.currency || undefined,
          targetAudience: data.targetAudience || undefined,
          goals: data.goals || undefined,
        };
        await campaignService.create(createData);
      }
      navigate("/crm/campaigns");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to save campaign");
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Campaign" : "Add Campaign"}
      title={isEdit ? "Edit Campaign" : "Add Campaign"}
      subtitle={
        isEdit ? "Update campaign details and performance" : "Plan a new marketing campaign"
      }
      backUrl="/crm/campaigns"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection title="Campaign details" description="Name, type, and schedule.">
            <FormInput
              label="Campaign name"
              type="text"
              name="campaignName"
              placeholder="Enter campaign name"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Campaign type"
              type="select"
              name="campaignType"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select campaign type</option>
              {CAMPAIGN_TYPE_OPTIONS.map((option) => (
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
              {CAMPAIGN_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Currency"
              type="select"
              name="currency"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select currency</option>
              {currencies.map((curr) => (
                <option key={curr.id} value={curr.code}>
                  {curr.code} - {curr.name}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Start date"
              type="date"
              name="startDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="End date"
              type="date"
              name="endDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Budget & response" description="Financial targets and results.">
            <FormInput
              label="Budget"
              type="number"
              name="budget"
              placeholder="0.00"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="0.01"
              min="0"
            />
            <FormInput
              label="Actual cost"
              type="number"
              name="actualCost"
              placeholder="0.00"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="0.01"
              min="0"
            />
            <FormInput
              label="Expected response"
              type="number"
              name="expectedResponse"
              placeholder="0"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="1"
              min="0"
            />
            <FormInput
              label="Actual response"
              type="number"
              name="actualResponse"
              placeholder="0"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="1"
              min="0"
            />
          </FormSection>

          <FormSection title="Content" description="Audience, goals, and description." columns={1}>
            <FormInput
              label="Target audience"
              type="textarea"
              name="targetAudience"
              rows={2}
              placeholder="Target audience description"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Goals"
              type="textarea"
              name="goals"
              rows={2}
              placeholder="Campaign goals"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Description"
              type="textarea"
              name="description"
              rows={4}
              placeholder="Campaign description"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/campaigns"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add campaign"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default CampaignForm;
