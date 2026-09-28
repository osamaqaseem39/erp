import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { purchaseOrderService, type CreatePurchaseOrderDto, type UpdatePurchaseOrderDto, type PurchaseOrderDto, type CreatePurchaseOrderDetailDto } from "../services";
import { vendorService } from "../../vendor/services";
import { itemService } from "../../../../inventory/entities/item/services";
import { currencyService } from "../../../../finance/entities/currency/services";
import { PURCHASE_ORDER_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const PurchaseOrderForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [purchaseOrder, setPurchaseOrder] = useState<PurchaseOrderDto | null>(null);
  const [lines, setLines] = useState<CreatePurchaseOrderDetailDto[]>([]);
  const [vendors, setVendors] = useState<any[]>([]);
  const [items, setItems] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);

  useEffect(() => {
    loadVendors();
    loadItems();
    loadCurrencies();
    if (isEdit && id) {
      loadPurchaseOrder(id);
    } else {
      setLines([{
        itemId: "",
        quantity: 1,
        unitPrice: 0,
      }]);
    }
  }, [isEdit, id]);

  const loadPurchaseOrder = async (purchaseOrderId: string) => {
    try {
      const purchaseOrderData = await purchaseOrderService.getById(purchaseOrderId);
      setPurchaseOrder(purchaseOrderData);
      if (purchaseOrderData.details && purchaseOrderData.details.length > 0) {
        setLines(purchaseOrderData.details.map(line => ({
          itemId: line.itemId,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
        })));
      } else if (purchaseOrderData.purchaseOrderDetails && purchaseOrderData.purchaseOrderDetails.length > 0) {
        setLines(purchaseOrderData.purchaseOrderDetails.map(line => ({
          itemId: line.itemId,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load purchase order");
    }
  };

  const loadVendors = async () => {
    try {
      const data = await vendorService.getAll();
      setVendors(data);
    } catch (err: any) {
      console.error("Failed to load vendors", err);
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

  const loadCurrencies = async () => {
    try {
      const data = await currencyService.getActive();
      setCurrencies(data);
    } catch (err: any) {
      console.error("Failed to load currencies", err);
    }
  };

  const addLine = () => {
    setLines([...lines, {
      itemId: "",
      quantity: 1,
      unitPrice: 0,
    }]);
  };

  const removeLine = (index: number) => {
    const newLines = lines.filter((_, i) => i !== index);
    setLines(newLines);
  };

  const updateLine = (index: number, field: string, value: any) => {
    const newLines = [...lines];
    (newLines[index] as any)[field] = value;
    
    if (field === "itemId" && value) {
      const item = items.find(item => item.id === value);
      if (item) {
        newLines[index].unitPrice = item.purchasePrice || item.costPrice || 0;
      }
    }
    
    if (field === "quantity" || field === "unitPrice") {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
    }
    
    setLines(newLines);
  };

  const calculateTotals = () => {
    let subTotal = 0;

    lines.forEach(line => {
      const lineTotal = line.quantity * line.unitPrice;
      subTotal += lineTotal;
    });

    // Assume 0% tax for now, can be added later
    const taxAmount = 0;
    const totalAmount = subTotal + taxAmount;

    return {
      subTotal,
      taxAmount,
      totalAmount,
    };
  };

  const purchaseOrderSchema = yup.object({
    purchaseOrderNumber: yup.string(),
    vendorId: yup.string().uuid("Invalid vendor").required("Vendor is required"),
    orderDate: yup.string().required("Order date is required"),
    expectedDeliveryDate: yup.string(),
    currency: yup.string(),
    status: yup.string(),
    terms: yup.string(),
    shippingAddress: yup.string(),
    billingAddress: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      const totals = calculateTotals();

      if (isEdit && id) {
        const updateData: UpdatePurchaseOrderDto = {
          id,
          purchaseOrderNumber: data.purchaseOrderNumber || undefined,
          vendorId: data.vendorId,
          orderDate: data.orderDate,
          expectedDeliveryDate: data.expectedDeliveryDate || undefined,
          currency: data.currency || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          terms: data.terms || undefined,
          shippingAddress: data.shippingAddress || undefined,
          billingAddress: data.billingAddress || undefined,
          notes: data.notes || undefined,
          details: lines.filter(line => line.itemId).map(line => ({
            itemId: line.itemId,
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            notes: line.notes,
          })),
        };
        await purchaseOrderService.update(id, updateData);
      } else {
        const createData: CreatePurchaseOrderDto = {
          purchaseOrderNumber: data.purchaseOrderNumber || undefined,
          vendorId: data.vendorId,
          orderDate: data.orderDate,
          expectedDeliveryDate: data.expectedDeliveryDate || undefined,
          currency: data.currency || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          terms: data.terms || undefined,
          shippingAddress: data.shippingAddress || undefined,
          billingAddress: data.billingAddress || undefined,
          notes: data.notes || undefined,
          details: lines.filter(line => line.itemId).map(line => ({
            itemId: line.itemId,
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            notes: line.notes,
          })),
        };
        await purchaseOrderService.create(createData);
      }
      navigate("/procurement/purchase-orders");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save purchase order");
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();
  const defaultValues = isEdit && purchaseOrder
    ? {
        purchaseOrderNumber: purchaseOrder.purchaseOrderNumber || "",
        vendorId: purchaseOrder.vendorId || "",
        orderDate: purchaseOrder.orderDate ? purchaseOrder.orderDate.split('T')[0] : new Date().toISOString().split('T')[0],
        expectedDeliveryDate: purchaseOrder.expectedDeliveryDate ? purchaseOrder.expectedDeliveryDate.split('T')[0] : "",
        currency: purchaseOrder.currency || "USD",
        status: purchaseOrder.status || "Draft",
        terms: purchaseOrder.terms || "",
        shippingAddress: purchaseOrder.shippingAddress || "",
        billingAddress: purchaseOrder.billingAddress || "",
        notes: purchaseOrder.notes || "",
      }
    : {
        purchaseOrderNumber: "",
        vendorId: "",
        orderDate: new Date().toISOString().split('T')[0],
        expectedDeliveryDate: "",
        currency: "USD",
        status: "Draft",
        terms: "",
        shippingAddress: "",
        billingAddress: "",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Purchase Order" : "Add Purchase Order"} subName="Procurement" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/procurement/purchase-orders" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(purchaseOrderSchema)}
            defaultValues={defaultValues}
          >
            {/* Purchase Order Header */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Purchase Order Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Purchase Order Number"
                  type="text"
                  name="purchaseOrderNumber"
                  placeholder="Auto-generated if empty"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Vendor"
                  type="select"
                  name="vendorId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select vendor</option>
                  {vendors.map((vendor) => (
                    <option key={vendor.id} value={vendor.id}>
                      {vendor.vendorCode} - {vendor.vendorName}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Order Date"
                  type="date"
                  name="orderDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Expected Delivery Date"
                  type="date"
                  name="expectedDeliveryDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Currency"
                  type="select"
                  name="currency"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select currency</option>
                  {currencies.map((curr) => (
                    <option key={curr.id} value={curr.code}>
                      {curr.code} - {curr.name}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Status"
                  type="select"
                  name="status"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  {PURCHASE_ORDER_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <div className="md:col-span-2">
                  <FormInput
                    label="Shipping Address"
                    type="textarea"
                    name="shippingAddress"
                    rows={2}
                    placeholder="Enter shipping address"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>

                <div className="md:col-span-2">
                  <FormInput
                    label="Billing Address"
                    type="textarea"
                    name="billingAddress"
                    rows={2}
                    placeholder="Enter billing address"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>

                <div className="md:col-span-2">
                  <FormInput
                    label="Terms"
                    type="textarea"
                    name="terms"
                    rows={2}
                    placeholder="Payment and delivery terms"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>

                <div className="md:col-span-2">
                  <FormInput
                    label="Notes"
                    type="textarea"
                    name="notes"
                    rows={3}
                    placeholder="Additional notes"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>
              </div>
            </div>

            {/* Purchase Order Lines */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Order Lines</h5>
                <button
                  type="button"
                  onClick={addLine}
                  className="btn btn-sm btn-primary"
                >
                  <i className="mgc_add_line me-2"></i>Add Line
                </button>
              </div>

              <div className="table-responsive">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Item</th>
                      <th>Quantity</th>
                      <th>Unit Price</th>
                      <th>Line Total</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, index) => {
                      const lineTotal = line.quantity * line.unitPrice;
                      
                      return (
                        <tr key={index}>
                          <td>
                            <select
                              className="form-select form-select-sm"
                              value={line.itemId}
                              onChange={(e) => updateLine(index, "itemId", e.target.value)}
                            >
                              <option value="">Select item</option>
                              {items.map((item) => (
                                <option key={item.id} value={item.id}>
                                  {item.itemCode} - {item.itemName}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              value={line.quantity}
                              onChange={(e) => updateLine(index, "quantity", e.target.value)}
                              step="0.01"
                              min="0"
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              value={line.unitPrice}
                              onChange={(e) => updateLine(index, "unitPrice", e.target.value)}
                              step="0.01"
                              min="0"
                            />
                          </td>
                          <td>{lineTotal.toFixed(2)}</td>
                          <td>
                            {lines.length > 1 && (
                              <button
                                type="button"
                                onClick={() => removeLine(index)}
                                className="btn btn-sm btn-soft-danger"
                              >
                                <i className="mgc_delete_line"></i>
                              </button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                  <tfoot>
                    <tr className="font-bold">
                      <td colSpan={3}>Subtotal</td>
                      <td>{totals.subTotal.toFixed(2)}</td>
                      <td></td>
                    </tr>
                    <tr className="font-bold">
                      <td colSpan={3}>Tax</td>
                      <td>{totals.taxAmount.toFixed(2)}</td>
                      <td></td>
                    </tr>
                    <tr className="font-bold text-lg">
                      <td colSpan={3}>Total</td>
                      <td>{totals.totalAmount.toFixed(2)}</td>
                      <td></td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/procurement/purchase-orders" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Purchase Order"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default PurchaseOrderForm;

