import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { productionOrderService, type CreateProductionOrderDto, type UpdateProductionOrderDto, type ProductionOrderDto } from "../services";
import { itemService } from "../../../../inventory/entities/item/services";
import { PRODUCTION_ORDER_STATUS_OPTIONS, PRODUCTION_ORDER_PRIORITY_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const ProductionOrderForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [productionOrder, setProductionOrder] = useState<ProductionOrderDto | null>(null);
  const [items, setItems] = useState<any[]>([]);

  useEffect(() => {
    loadItems();
    if (isEdit && id) {
      loadProductionOrder(id);
    }
  }, [isEdit, id]);

  const loadProductionOrder = async (productionOrderId: string) => {
    try {
      const productionOrderData = await productionOrderService.getById(productionOrderId);
      setProductionOrder(productionOrderData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load production order");
    }
  };

  const loadItems = async () => {
    try {
      const data = await itemService.getAll({ companyId: "" });
      setItems(data);
    } catch (err: any) {
      console.error("Failed to load items", err);
    }
  };

  const productionOrderSchema = yup.object({
    productionOrderNumber: yup.string(),
    orderNumber: yup.string(),
    itemId: yup.string().uuid("Invalid item").nullable(),
    workCenterId: yup.string().uuid("Invalid work center").nullable(),
    quantity: yup.number().min(0.01, "Quantity must be greater than 0").required("Quantity is required"),
    plannedStartDate: yup.string(),
    plannedEndDate: yup.string(),
    status: yup.string(),
    priority: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateProductionOrderDto = {
          id,
          productionOrderNumber: data.productionOrderNumber || undefined,
          orderNumber: data.orderNumber || undefined,
          itemId: data.itemId || undefined,
          workCenterId: data.workCenterId || undefined,
          quantity: parseFloat(data.quantity),
          plannedStartDate: data.plannedStartDate || undefined,
          plannedEndDate: data.plannedEndDate || undefined,
          status: data.status || "Planned",
          priority: data.priority || "Medium",
          notes: data.notes || undefined,
        };
        await productionOrderService.update(id, updateData);
      } else {
        const createData: CreateProductionOrderDto = {
          productionOrderNumber: data.productionOrderNumber || undefined,
          orderNumber: data.orderNumber || undefined,
          itemId: data.itemId || undefined,
          workCenterId: data.workCenterId || undefined,
          quantity: parseFloat(data.quantity),
          plannedStartDate: data.plannedStartDate || undefined,
          plannedEndDate: data.plannedEndDate || undefined,
          status: data.status || "Planned",
          priority: data.priority || "Medium",
          notes: data.notes || undefined,
        };
        await productionOrderService.create(createData);
      }
      navigate("/manufacturing/production-orders");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save production order");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && productionOrder
    ? {
        productionOrderNumber: productionOrder.productionOrderNumber || "",
        orderNumber: productionOrder.orderNumber || "",
        itemId: productionOrder.itemId || "",
        workCenterId: productionOrder.workCenterId || "",
        quantity: productionOrder.quantity || 1,
        plannedStartDate: productionOrder.plannedStartDate ? productionOrder.plannedStartDate.split('T')[0] : "",
        plannedEndDate: productionOrder.plannedEndDate ? productionOrder.plannedEndDate.split('T')[0] : "",
        status: productionOrder.status || "Planned",
        priority: productionOrder.priority || "Medium",
        notes: productionOrder.notes || "",
      }
    : {
        productionOrderNumber: "",
        orderNumber: "",
        itemId: "",
        workCenterId: "",
        quantity: 1,
        plannedStartDate: "",
        plannedEndDate: "",
        status: "Planned",
        priority: "Medium",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Production Order" : "Add Production Order"} subName="Manufacturing" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/manufacturing/production-orders" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <VerticalForm
            onSubmit={onSubmit}
            resolver={yupResolver(productionOrderSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Production Order Number"
                type="text"
                name="productionOrderNumber"
                placeholder="Auto-generated if empty"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Order Number"
                type="text"
                name="orderNumber"
                placeholder="Related order number"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Item"
                type="select"
                name="itemId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select item</option>
                {items.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.itemCode} - {item.itemName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Work Center"
                type="text"
                name="workCenterId"
                placeholder="Work center ID"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Quantity"
                type="number"
                name="quantity"
                placeholder="1"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.01"
                min="0.01"
                required
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {PRODUCTION_ORDER_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Priority"
                type="select"
                name="priority"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {PRODUCTION_ORDER_PRIORITY_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Planned Start Date"
                type="date"
                name="plannedStartDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Planned End Date"
                type="date"
                name="plannedEndDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <div className="md:col-span-2">
                <FormInput
                  label="Notes"
                  type="textarea"
                  name="notes"
                  rows={4}
                  placeholder="Additional notes"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              {isEdit && productionOrder && (
                <>
                  <FormInput
                    label="Completed Quantity"
                    type="number"
                    name="completedQuantity"
                    value={productionOrder.completedQuantity}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />

                  <FormInput
                    label="Actual Start Date"
                    type="text"
                    name="actualStartDate"
                    value={productionOrder.actualStartDate ? new Date(productionOrder.actualStartDate).toLocaleString() : "-"}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />

                  <FormInput
                    label="Actual End Date"
                    type="text"
                    name="actualEndDate"
                    value={productionOrder.actualEndDate ? new Date(productionOrder.actualEndDate).toLocaleString() : "-"}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />
                </>
              )}
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/manufacturing/production-orders" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Production Order"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default ProductionOrderForm;

