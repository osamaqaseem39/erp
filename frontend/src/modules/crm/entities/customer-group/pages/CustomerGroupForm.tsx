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
  requiredNumber,
} from "../../../components";
import {
  customerGroupService,
  type CreateCustomerGroupDto,
  type UpdateCustomerGroupDto,
} from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const customerGroupSchema = yup.object({
  groupName: yup.string().trim().required("Group name is required"),
  description: optionalString(),
  discountPercentage: requiredNumber(0).max(100, "Discount must be 100% or less"),
});

type CustomerGroupFormValues = yup.InferType<typeof customerGroupSchema>;

const defaultValues: CustomerGroupFormValues = {
  groupName: "",
  description: "",
  discountPercentage: 0,
};

const CustomerGroupForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  const methods = useForm<CustomerGroupFormValues>({
    defaultValues,
    resolver: yupResolver(customerGroupSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    if (isEdit && id) {
      void loadCustomerGroup(id);
    }
  }, [isEdit, id]);

  const loadCustomerGroup = async (groupId: string) => {
    setFetching(true);
    setError(null);
    try {
      const data = await customerGroupService.getById(groupId);
      reset({
        groupName: data.groupName ?? "",
        description: data.description ?? "",
        discountPercentage: data.discountPercentage ?? 0,
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error || e.response?.data?.message || "Failed to load customer group"
      );
    } finally {
      setFetching(false);
    }
  };

  const onSubmit = async (data: CustomerGroupFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateCustomerGroupDto = {
          id,
          groupName: data.groupName,
          description: data.description || undefined,
          discountPercentage: Number(data.discountPercentage),
        };
        await customerGroupService.update(id, updateData);
      } else {
        const createData: CreateCustomerGroupDto = {
          groupName: data.groupName,
          description: data.description || undefined,
          discountPercentage: Number(data.discountPercentage),
        };
        await customerGroupService.create(createData);
      }
      navigate("/crm/customer-groups");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error ||
          e.response?.data?.message ||
          `Failed to ${isEdit ? "Save changes" : "Add Customer Group"} customer group`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Customer Group" : "Add Customer Group"}
      title={isEdit ? "Edit Customer Group" : "Add Customer Group"}
      subtitle={
        isEdit
          ? "Update group name, discount, and description"
          : "Define a customer segment with a default discount"
      }
      backUrl="/crm/customer-groups"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Group details"
            description="Name and default discount for customers in this group."
          >
            <FormInput
              label="Group name"
              type="text"
              name="groupName"
              placeholder="e.g. Wholesale, VIP"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Discount percentage"
              type="number"
              name="discountPercentage"
              placeholder="0"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="0.01"
              min="0"
              max="100"
              required
            />
          </FormSection>

          <FormSection title="Description" description="Optional notes about this group." columns={1}>
            <FormInput
              label="Description"
              type="textarea"
              name="description"
              rows={4}
              placeholder="How this group is used, eligibility, etc."
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/customer-groups"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add group"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default CustomerGroupForm;
