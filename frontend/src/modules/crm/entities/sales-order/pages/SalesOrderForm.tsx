import React, { useEffect, useState } from "react";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";
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
} from "../../../components";
import { salesOrderService } from "../services";
import type {
  CreateSalesOrderDto,
  UpdateSalesOrderDto,
  CreateSalesOrderLineDto,
} from "../types";
import { customerService } from "../../customer/services";
import { quoteService } from "../../quote/services";
import { itemService } from "../../../../inventory/entities/item/services";
import { currencyService } from "../../../../finance/entities/currency/services";
import { userService } from "../../../../core/entities/user/services";
import { SALES_ORDER_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const salesOrderSchema = yup.object({
  orderNumber: yup.string().trim().required("Order number is required"),
  salesOrderNumber: yup.string().trim().required("Sales order number is required"),
  customerId: optionalUuid(),
  quoteId: optionalUuid(),
  orderDate: yup.string().trim().required("Order date is required"),
  requiredDate: optionalString(),
  currency: optionalString(),
  status: optionalString(),
  terms: optionalString(),
  shippingAddress: optionalString(),
  billingAddress: optionalString(),
  notes: optionalString(),
  assignedToUserId: optionalUuid(),
});

type SalesOrderFormValues = yup.InferType<typeof salesOrderSchema>;

const defaultValues: SalesOrderFormValues = {
  orderNumber: "",
  salesOrderNumber: "",
  customerId: "",
  quoteId: "",
  orderDate: new Date().toISOString().split("T")[0],
  requiredDate: "",
  currency: "USD",
  status: "Draft",
  terms: "",
  shippingAddress: "",
  billingAddress: "",
  notes: "",
  assignedToUserId: "",
};

const SalesOrderForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [lines, setLines] = useState<CreateSalesOrderLineDto[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [quotes, setQuotes] = useState<any[]>([]);
  const [items, setItems] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  const methods = useForm<SalesOrderFormValues>({
    defaultValues,
    resolver: yupResolver(salesOrderSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset } = methods;

  useEffect(() => {
    void loadCustomers();
    void loadQuotes();
    void loadItems();
    void loadCurrencies();
    void loadUsers();
    if (isEdit && id) {
      void loadSalesOrder(id);
    } else {
      setLines([
        {
          itemId: "",
          description: "",
          quantity: 1,
          unitPrice: 0,
          discountPercent: 0,
          taxRate: 0,
        },
      ]);
    }
  }, [isEdit, id]);

  const loadSalesOrder = async (salesOrderId: string) => {
    setFetching(true);
    setError(null);
    try {
      const salesOrderData = await salesOrderService.getById(salesOrderId);
      reset({
        orderNumber: salesOrderData.orderNumber ?? "",
        salesOrderNumber: salesOrderData.salesOrderNumber ?? "",
        customerId: salesOrderData.customerId ?? "",
        quoteId: salesOrderData.quoteId ?? "",
        orderDate: salesOrderData.orderDate
          ? salesOrderData.orderDate.split("T")[0]
          : new Date().toISOString().split("T")[0],
        requiredDate: salesOrderData.requiredDate
          ? salesOrderData.requiredDate.split("T")[0]
          : "",
        currency: salesOrderData.currency ?? "USD",
        status: salesOrderData.status ?? "Draft",
        terms: salesOrderData.terms ?? "",
        shippingAddress: salesOrderData.shippingAddress ?? "",
        billingAddress: salesOrderData.billingAddress ?? "",
        notes: salesOrderData.notes ?? "",
        assignedToUserId: salesOrderData.assignedToUserId ?? "",
      });
      if (salesOrderData.salesOrderLines && salesOrderData.salesOrderLines.length > 0) {
        setLines(
          salesOrderData.salesOrderLines.map((line) => ({
            itemId: line.itemId,
            description: line.description || "",
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            discountPercent: line.discountPercent || 0,
            taxRate: line.taxRate || 0,
          }))
        );
      }
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error || e.response?.data?.message || "Failed to load sales order"
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

  const loadQuotes = async () => {
    try {
      const data = await quoteService.getAll();
      setQuotes(data);
    } catch (err: unknown) {
      console.error("Failed to load quotes", err);
    }
  };

  const loadItems = async () => {
    try {
      const data = await itemService.getAll({ companyId: getCompanyId() });
      setItems(data);
    } catch (err: unknown) {
      console.error("Failed to load items", err);
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

  const loadUsers = async () => {
    try {
      const data = await userService.getAll();
      setUsers(data);
    } catch (err: unknown) {
      console.error("Failed to load users", err);
    }
  };

  const addLine = () => {
    setLines([
      ...lines,
      {
        itemId: "",
        description: "",
        quantity: 1,
        unitPrice: 0,
        discountPercent: 0,
        taxRate: 0,
      },
    ]);
  };

  const removeLine = (index: number) => {
    setLines(lines.filter((_, i) => i !== index));
  };

  const updateLine = (index: number, field: string, value: any) => {
    const newLines = [...lines];
    (newLines[index] as any)[field] = value;

    if (field === "itemId" && value) {
      const item = items.find((item) => item.id === value);
      if (item) {
        newLines[index].description = item.itemName || "";
        newLines[index].unitPrice = item.sellingPrice || 0;
      }
    }

    if (
      field === "quantity" ||
      field === "unitPrice" ||
      field === "taxRate" ||
      field === "discountPercent"
    ) {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
    }

    setLines(newLines);
  };

  const calculateTotals = () => {
    let subTotal = 0;
    let totalTax = 0;

    lines.forEach((line) => {
      const lineSubTotal = line.quantity * line.unitPrice;
      const discountAmount = lineSubTotal * ((line.discountPercent || 0) / 100);
      const lineAfterDiscount = lineSubTotal - discountAmount;
      const lineTax = lineAfterDiscount * ((line.taxRate || 0) / 100);

      subTotal += lineAfterDiscount;
      totalTax += lineTax;
    });

    return {
      subTotal,
      taxAmount: totalTax,
      totalAmount: subTotal + totalTax,
    };
  };

  const onSubmit = async (data: SalesOrderFormValues) => {
    setError(null);
    setLoading(true);
    try {
      const totals = calculateTotals();

      if (isEdit && id) {
        const updateData: UpdateSalesOrderDto = {
          id,
          orderNumber: data.orderNumber,
          salesOrderNumber: data.salesOrderNumber,
          customerId: data.customerId || undefined,
          quoteId: data.quoteId || undefined,
          orderDate: data.orderDate,
          requiredDate: data.requiredDate || undefined,
          currency: data.currency || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          terms: data.terms || undefined,
          shippingAddress: data.shippingAddress || undefined,
          billingAddress: data.billingAddress || undefined,
          notes: data.notes || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
        };
        await salesOrderService.update(id, updateData);
      } else {
        const createData: CreateSalesOrderDto = {
          companyId: getCompanyId(),
          orderNumber: data.orderNumber,
          salesOrderNumber: data.salesOrderNumber,
          customerId: data.customerId || undefined,
          quoteId: data.quoteId || undefined,
          orderDate: data.orderDate,
          requiredDate: data.requiredDate || undefined,
          currency: data.currency || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          terms: data.terms || undefined,
          shippingAddress: data.shippingAddress || undefined,
          billingAddress: data.billingAddress || undefined,
          notes: data.notes || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
          salesOrderLines: lines,
        };
        await salesOrderService.create(createData);
      }
      navigate("/crm/sales-orders");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(
        e.response?.data?.Error || e.response?.data?.message || "Failed to save sales order"
      );
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Sales Order" : "Add Sales Order"}
      title={isEdit ? "Edit Sales Order" : "Add Sales Order"}
      subtitle={
        isEdit ? "Update order details and line items" : "Record a new customer sales order"
      }
      backUrl="/crm/sales-orders"
      fetching={fetching}
      error={error}
      maxWidth="wide"
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection title="Order information" description="Header details and addresses.">
            <FormInput
              label="Order number"
              type="text"
              name="orderNumber"
              placeholder="Enter order number"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Sales order number"
              type="text"
              name="salesOrderNumber"
              placeholder="Enter sales order number"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
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
              label="Quote"
              type="select"
              name="quoteId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select quote</option>
              {quotes.map((quote) => (
                <option key={quote.id} value={quote.id}>
                  {quote.quoteNumber}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Order date"
              type="date"
              name="orderDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
              required
            />
            <FormInput
              label="Required date"
              type="date"
              name="requiredDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
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
              label="Status"
              type="select"
              name="status"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              {SALES_ORDER_STATUS_OPTIONS.map((option) => (
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
            <div className="sm:col-span-2">
              <FormInput
                label="Shipping address"
                type="textarea"
                name="shippingAddress"
                rows={2}
                placeholder="Enter shipping address"
                containerClass={CRM_FIELD_WRAP}
                className={CRM_TEXTAREA_CLASS}
                labelClassName={CRM_LABEL_CLASS}
              />
            </div>
            <div className="sm:col-span-2">
              <FormInput
                label="Billing address"
                type="textarea"
                name="billingAddress"
                rows={2}
                placeholder="Enter billing address"
                containerClass={CRM_FIELD_WRAP}
                className={CRM_TEXTAREA_CLASS}
                labelClassName={CRM_LABEL_CLASS}
              />
            </div>
          </FormSection>

          <FormSection title="Line items" description="Products and pricing." columns={1}>
            <div className="flex items-center justify-end mb-2">
              <button type="button" onClick={addLine} className="btn btn-sm btn-primary">
                <i className="mgc_add_line me-2" />
                Add line
              </button>
            </div>
            <div className="table-responsive">
              <table className="table">
                <thead>
                  <tr>
                    <th>Item</th>
                    <th>Description</th>
                    <th>Quantity</th>
                    <th>Unit price</th>
                    <th>Discount %</th>
                    <th>Tax rate %</th>
                    <th>Line total</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {lines.map((line, index) => {
                    const lineSubTotal = line.quantity * line.unitPrice;
                    const discountAmount = lineSubTotal * ((line.discountPercent || 0) / 100);
                    const lineAfterDiscount = lineSubTotal - discountAmount;
                    const lineTax = lineAfterDiscount * ((line.taxRate || 0) / 100);
                    const lineTotal = lineAfterDiscount + lineTax;

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
                            type="text"
                            className="form-control form-control-sm"
                            value={line.description || ""}
                            onChange={(e) => updateLine(index, "description", e.target.value)}
                            placeholder="Description"
                          />
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
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={line.discountPercent || 0}
                            onChange={(e) => updateLine(index, "discountPercent", e.target.value)}
                            step="0.01"
                            min="0"
                            max="100"
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={line.taxRate || 0}
                            onChange={(e) => updateLine(index, "taxRate", e.target.value)}
                            step="0.01"
                            min="0"
                            max="100"
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
                              <i className="mgc_delete_line" />
                            </button>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
                <tfoot>
                  <tr className="font-bold">
                    <td colSpan={6}>Subtotal</td>
                    <td>{totals.subTotal.toFixed(2)}</td>
                    <td />
                  </tr>
                  <tr className="font-bold">
                    <td colSpan={6}>Tax</td>
                    <td>{totals.taxAmount.toFixed(2)}</td>
                    <td />
                  </tr>
                  <tr className="font-bold text-lg">
                    <td colSpan={6}>Total</td>
                    <td>{totals.totalAmount.toFixed(2)}</td>
                    <td />
                  </tr>
                </tfoot>
              </table>
            </div>
          </FormSection>

          <FormSection title="Terms & notes" description="Payment terms and internal notes." columns={1}>
            <FormInput
              label="Terms"
              type="textarea"
              name="terms"
              rows={2}
              placeholder="Payment terms"
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
            cancelUrl="/crm/sales-orders"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add sales order"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default SalesOrderForm;
