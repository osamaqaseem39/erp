import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { FormInput } from "../../../../../components";
import {
  CrmFormLayout,
  CrmFormFooter,
  FormSection,
  CRM_LABEL_CLASS,
  CRM_INPUT_CLASS,
  CRM_FIELD_WRAP,
  CRM_TEXTAREA_CLASS,
  optionalString,
  optionalUuid,
} from "../../../components";
import {
  salesTerritoryService,
  type CreateSalesTerritoryDto,
  type UpdateSalesTerritoryDto,
} from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const salesTerritorySchema = yup.object({
  territoryName: yup.string().trim().required("Territory name is required"),
  description: optionalString(),
  managerId: optionalUuid(),
});

type SalesTerritoryFormValues = yup.InferType<typeof salesTerritorySchema>;

const defaultValues: SalesTerritoryFormValues = {
  territoryName: "",
  description: "",
  managerId: "",
};

const SalesTerritoryForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  const methods = useForm<SalesTerritoryFormValues>({
    defaultValues,
    resolver: yupResolver(salesTerritorySchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    if (isEdit && id) {
      void loadSalesTerritory(id);
    }
  }, [isEdit, id]);

  const loadSalesTerritory = async (territoryId: string) => {
    setFetching(true);
    setError(null);
    try {
      const data = await salesTerritoryService.getById(territoryId);
      reset({
        territoryName: data.territoryName ?? "",
        description: data.description ?? "",
        managerId: data.managerId ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error || e.response?.data?.message || "Failed to load sales territory"
      );
    } finally {
      setFetching(false);
    }
  };

  const onSubmit = async (data: SalesTerritoryFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateSalesTerritoryDto = {
          id,
          territoryName: data.territoryName,
          description: data.description || undefined,
          managerId: data.managerId || undefined,
        };
        await salesTerritoryService.update(id, updateData);
      } else {
        const createData: CreateSalesTerritoryDto = {
          territoryName: data.territoryName,
          description: data.description || undefined,
          managerId: data.managerId || undefined,
        };
        await salesTerritoryService.create(createData);
      }
      navigate("/crm/sales-territories");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error ||
          e.response?.data?.message ||
          `Failed to ${isEdit ? "Save changes" : "Add Sales Territory"} sales territory`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Sales Territory" : "Add Sales Territory"}
      title={isEdit ? "Edit Sales Territory" : "Add Sales Territory"}
      subtitle={
        isEdit
          ? "Update territory name, manager, and description"
          : "Define a sales region and optional territory manager"
      }
      backUrl="/crm/sales-territories"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Territory details"
            description="Name and optional manager for this sales region."
          >
            <FormInput
              label="Territory name"
              type="text"
              name="territoryName"
              placeholder="e.g. North America, EMEA West"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Manager ID"
              type="text"
              name="managerId"
              placeholder="User UUID (optional)"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Description" description="Optional notes about this territory." columns={1}>
            <FormInput
              label="Description"
              type="textarea"
              name="description"
              rows={4}
              placeholder="Coverage area, accounts, or guidelines…"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/sales-territories"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add territory"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default SalesTerritoryForm;
