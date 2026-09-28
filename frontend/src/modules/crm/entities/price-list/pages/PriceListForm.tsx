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
} from "../../../components";
import {
  priceListService,
  type CreatePriceListDto,
  type UpdatePriceListDto,
} from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const priceListSchema = yup.object({
  priceListName: yup.string().trim().required("Price list name is required"),
  description: optionalString(),
  validFrom: yup.string().trim().required("Valid from date is required"),
  validTo: optionalString(),
  isDefault: yup.boolean().default(false),
});

type PriceListFormValues = yup.InferType<typeof priceListSchema>;

const defaultValues: PriceListFormValues = {
  priceListName: "",
  description: "",
  validFrom: "",
  validTo: "",
  isDefault: false,
};

const PriceListForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  const methods = useForm<PriceListFormValues>({
    defaultValues,
    resolver: yupResolver(priceListSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    if (isEdit && id) {
      void loadPriceList(id);
    }
  }, [isEdit, id]);

  const loadPriceList = async (priceListId: string) => {
    setFetching(true);
    setError(null);
    try {
      const data = await priceListService.getById(priceListId);
      reset({
        priceListName: data.priceListName ?? "",
        description: data.description ?? "",
        validFrom: data.validFrom ? data.validFrom.split("T")[0] : "",
        validTo: data.validTo ? data.validTo.split("T")[0] : "",
        isDefault: data.isDefault ?? false,
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load price list");
    } finally {
      setFetching(false);
    }
  };

  const onSubmit = async (data: PriceListFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdatePriceListDto = {
          id,
          priceListName: data.priceListName,
          description: data.description || undefined,
          validFrom: data.validFrom || undefined,
          validTo: data.validTo || undefined,
          isDefault: data.isDefault ?? false,
        };
        await priceListService.update(id, updateData);
      } else {
        const createData: CreatePriceListDto = {
          priceListName: data.priceListName,
          description: data.description || undefined,
          validFrom: data.validFrom,
          validTo: data.validTo || undefined,
          isDefault: data.isDefault ?? false,
        };
        await priceListService.create(createData);
      }
      navigate("/crm/price-lists");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error ||
          e.response?.data?.message ||
          `Failed to ${isEdit ? "Save changes" : "Add Price List"} price list`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Price List" : "Add Price List"}
      title={isEdit ? "Edit Price List" : "Add Price List"}
      subtitle={
        isEdit
          ? "Update pricing catalog name, validity, and default flag"
          : "Add a named price list with effective dates"
      }
      backUrl="/crm/price-lists"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Price list"
            description="Name and whether this list is the company default."
          >
            <FormInput
              label="Price list name"
              type="text"
              name="priceListName"
              placeholder="e.g. Standard 2025, Partner pricing"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <div className={CRM_FIELD_WRAP}>
              <FormInput
                label="Default price list"
                type="checkbox"
                name="isDefault"
                containerClass="flex items-center gap-2"
                labelClassName={`${CRM_LABEL_CLASS} mb-0`}
                className="form-checkbox rounded"
              />
            </div>
          </FormSection>

          <FormSection
            title="Validity period"
            description="When this price list applies. Leave end date empty for open-ended."
          >
            <FormInput
              label="Valid from"
              type="date"
              name="validFrom"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Valid to"
              type="date"
              name="validTo"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <FormSection title="Description" description="Optional notes about this price list." columns={1}>
            <FormInput
              label="Description"
              type="textarea"
              name="description"
              rows={4}
              placeholder="Products, channels, or conditions covered…"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/price-lists"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add price list"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default PriceListForm;
