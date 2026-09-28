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
  optionalUuid,
  requiredNumber,
} from "../../../components";
import {
  returnOrderService,
  type CreateReturnOrderDto,
  type UpdateReturnOrderDto,
} from "../services";
import { customerService } from "../../customer/services";
import type { CustomerDto } from "../../customer/types";
import { RETURN_ORDER_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const returnOrderSchema = yup.object({
  returnNumber: optionalString(),
  returnDate: yup.string().trim().required("Return date is required"),
  customerId: yup.string().uuid("Invalid customer").required("Customer is required"),
  salesOrderId: optionalUuid(),
  reason: yup.string().trim().required("Reason is required"),
  status: optionalString(),
  totalAmount: requiredNumber(0),
  notes: optionalString(),
});

type ReturnOrderFormValues = yup.InferType<typeof returnOrderSchema>;

const defaultValues: ReturnOrderFormValues = {
  returnNumber: "",
  returnDate: new Date().toISOString().split("T")[0],
  customerId: "",
  salesOrderId: "",
  reason: "",
  status: "Pending",
  totalAmount: 0,
  notes: "",
};

const ReturnOrderForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);

  const methods = useForm<ReturnOrderFormValues>({
    defaultValues,
    resolver: yupResolver(returnOrderSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    void loadCustomers();
    if (isEdit && id) {
      void loadReturnOrder(id);
    }
  }, [isEdit, id]);

  const loadReturnOrder = async (returnOrderId: string) => {
    setFetching(true);
    setError(null);
    try {
      const data = await returnOrderService.getById(returnOrderId);
      reset({
        returnNumber: data.returnNumber ?? "",
        returnDate: data.returnDate ? data.returnDate.split("T")[0] : "",
        customerId: data.customerId ?? "",
        salesOrderId: data.salesOrderId ?? "",
        reason: data.reason ?? "",
        status: data.status ?? "Pending",
        totalAmount: data.totalAmount ?? 0,
        notes: data.notes ?? "",
      });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load return order");
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

  const onSubmit = async (data: ReturnOrderFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateReturnOrderDto = {
          id,
          reason: data.reason,
          status: data.status || "Pending",
          totalAmount: Number(data.totalAmount),
          notes: data.notes || undefined,
        };
        await returnOrderService.update(id, updateData);
      } else {
        const createData: CreateReturnOrderDto = {
          returnNumber: data.returnNumber || undefined,
          returnDate: data.returnDate,
          customerId: data.customerId,
          salesOrderId: data.salesOrderId || undefined,
          reason: data.reason,
          status: data.status || "Pending",
          totalAmount: Number(data.totalAmount),
          notes: data.notes || undefined,
        };
        await returnOrderService.create(createData);
      }
      navigate("/crm/return-orders");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error ||
          e.response?.data?.message ||
          `Failed to ${isEdit ? "Save changes" : "Add Return Order"} return order`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Return Order" : "Add Return Order"}
      title={isEdit ? "Edit Return Order" : "Add Return Order"}
      subtitle={
        isEdit
          ? "Update status, amount, reason, and notes"
          : "Record a customer return against a sales order"
      }
      backUrl="/crm/return-orders"
      fetching={fetching}
      error={error}
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection
            title="Return details"
            description={
              isEdit
                ? "Header fields are read-only after creation."
                : "Return number is optional; leave blank to auto-generate."
            }
          >
            <FormInput
              label="Return number"
              type="text"
              name="returnNumber"
              placeholder="Auto-generated if empty"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              disabled={isEdit}
            />
            <FormInput
              label="Return date"
              type="date"
              name="returnDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required={!isEdit}
              disabled={isEdit}
            />
            <FormInput
              label="Customer"
              type="select"
              name="customerId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required={!isEdit}
              disabled={isEdit}
            >
              <option value="">Select customer</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.customerName} ({customer.companyName})
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Sales order ID"
              type="text"
              name="salesOrderId"
              placeholder="Linked sales order (optional)"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              disabled={isEdit}
            />
            <FormInput
              label="Status"
              type="select"
              name="status"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              {RETURN_ORDER_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Total amount"
              type="number"
              name="totalAmount"
              placeholder="0.00"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              step="0.01"
              min="0"
              required
            />
          </FormSection>

          <FormSection title="Reason" description="Why the customer is returning items." columns={1}>
            <FormInput
              label="Reason"
              type="textarea"
              name="reason"
              rows={3}
              placeholder="Describe the return reason"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
          </FormSection>

          <FormSection title="Notes" description="Internal notes about this return." columns={1}>
            <FormInput
              label="Notes"
              type="textarea"
              name="notes"
              rows={3}
              placeholder="Additional information…"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_TEXTAREA_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/return-orders"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add return order"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default ReturnOrderForm;
