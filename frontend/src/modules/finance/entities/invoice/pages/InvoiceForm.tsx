import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { invoiceService, type CreateInvoiceDto, type UpdateInvoiceDto, type InvoiceDto, type CreateInvoiceLineDto } from "../services";
import { customerService } from "../../../../crm/entities/customer/services";
import { itemService } from "../../../../inventory/entities/item/services";
import { currencyService } from "../../currency/services";
import { INVOICE_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";

const InvoiceForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [invoice, setInvoice] = useState<InvoiceDto | null>(null);
  const [lines, setLines] = useState<CreateInvoiceLineDto[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [items, setItems] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);

  useEffect(() => {
    loadCustomers();
    loadItems();
    loadCurrencies();
    if (isEdit && id) {
      loadInvoice(id);
    } else {
      setLines([{
        productId: "",
        description: "",
        quantity: 1,
        unitPrice: 0,
        taxRate: 0,
      }]);
    }
  }, [isEdit, id]);

  const loadInvoice = async (invoiceId: string) => {
    try {
      const invoiceData = await invoiceService.getById(invoiceId);
      setInvoice(invoiceData);
      if (invoiceData.invoiceLines && invoiceData.invoiceLines.length > 0) {
        setLines(invoiceData.invoiceLines.map(line => ({
          productId: line.productId || "",
          description: line.description || "",
          quantity: line.quantity,
          unitPrice: line.unitPrice,
          taxRate: line.taxRate || 0,
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load invoice");
    }
  };

  const loadCustomers = async () => {
    try {
      const data = await customerService.getAll();
      setCustomers(data);
    } catch (err: any) {
      console.error("Failed to load customers", err);
    }
  };

  const loadItems = async () => {
    try {
      const companyId = getCompanyId();
      const data = await itemService.getAll({ companyId });
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
      productId: "",
      description: "",
      quantity: 1,
      unitPrice: 0,
      taxRate: 0,
    }]);
  };

  const removeLine = (index: number) => {
    const newLines = lines.filter((_, i) => i !== index);
    setLines(newLines);
  };

  const updateLine = (index: number, field: string, value: any) => {
    const newLines = [...lines];
    (newLines[index] as any)[field] = value;
    
    // If item is selected, update description and unit price
    if (field === "productId" && value) {
      const item = items.find(item => item.id === value);
      if (item) {
        newLines[index].description = item.itemName || "";
        newLines[index].unitPrice = item.sellingPrice || 0;
      }
    }
    
    // Ensure numeric fields are numbers
    if (field === "quantity" || field === "unitPrice" || field === "taxRate") {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
    }
    
    setLines(newLines);
  };

  const calculateTotals = () => {
    let subTotal = 0;
    let totalTax = 0;

    lines.forEach(line => {
      const lineSubTotal = line.quantity * line.unitPrice;
      const lineTax = lineSubTotal * ((line.taxRate || 0) / 100);
      
      subTotal += lineSubTotal;
      totalTax += lineTax;
    });

    return {
      subTotal,
      taxAmount: totalTax,
      totalAmount: subTotal + totalTax,
    };
  };

  const invoiceSchema = yup.object({
    invoiceNumber: yup.string(),
    customerId: yup.string().uuid("Invalid customer").required("Customer is required"),
    invoiceDate: yup.string().required("Invoice date is required"),
    dueDate: yup.string().required("Due date is required"),
    currencyCode: yup.string(),
    status: yup.string(),
    notes: yup.string(),
    terms: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      const totals = calculateTotals();

      if (isEdit && id) {
        const updateData: UpdateInvoiceDto = {
          invoiceId: id,
          invoiceNumber: data.invoiceNumber || undefined,
          customerId: data.customerId,
          invoiceDate: data.invoiceDate,
          dueDate: data.dueDate,
          currencyCode: data.currencyCode || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          notes: data.notes || undefined,
          terms: data.terms || undefined,
        };
        await invoiceService.update(id, updateData);
      } else {
        const createData: CreateInvoiceDto = {
          invoiceNumber: data.invoiceNumber || undefined,
          customerId: data.customerId,
          invoiceDate: data.invoiceDate,
          dueDate: data.dueDate,
          currencyCode: data.currencyCode || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          notes: data.notes || undefined,
          terms: data.terms || undefined,
          invoiceLines: lines,
        };
        await invoiceService.create(createData);
      }
      navigate("/finance/invoices");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save invoice");
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();
  const defaultValues = isEdit && invoice
    ? {
        invoiceNumber: invoice.invoiceNumber || "",
        customerId: invoice.customerId || "",
        invoiceDate: invoice.invoiceDate ? invoice.invoiceDate.split('T')[0] : new Date().toISOString().split('T')[0],
        dueDate: invoice.dueDate ? invoice.dueDate.split('T')[0] : new Date().toISOString().split('T')[0],
        currencyCode: invoice.currencyCode || "USD",
        status: invoice.status || "Draft",
        notes: invoice.notes || "",
        terms: invoice.terms || "",
      }
    : {
        invoiceNumber: "",
        customerId: "",
        invoiceDate: new Date().toISOString().split('T')[0],
        dueDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 30 days from now
        currencyCode: "USD",
        status: "Draft",
        notes: "",
        terms: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Invoice" : "Add Invoice"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/invoices" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(invoiceSchema)}
            defaultValues={defaultValues}
          >
            {/* Invoice Header */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Invoice Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Invoice Number"
                  type="text"
                  name="invoiceNumber"
                  placeholder="Auto-generated if empty"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Customer"
                  type="select"
                  name="customerId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select customer</option>
                  {customers.map((customer) => (
                    <option key={customer.id} value={customer.id}>
                      {customer.customerName} ({customer.companyName})
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Invoice Date"
                  type="date"
                  name="invoiceDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Due Date"
                  type="date"
                  name="dueDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Currency"
                  type="select"
                  name="currencyCode"
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
                  {INVOICE_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <div className="md:col-span-2">
                  <FormInput
                    label="Terms"
                    type="textarea"
                    name="terms"
                    rows={2}
                    placeholder="Payment terms"
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

            {/* Invoice Lines */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Invoice Lines</h5>
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
                      <th>Description</th>
                      <th>Quantity</th>
                      <th>Unit Price</th>
                      <th>Tax Rate %</th>
                      <th>Line Total</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, index) => {
                      const lineSubTotal = line.quantity * line.unitPrice;
                      const lineTax = lineSubTotal * ((line.taxRate || 0) / 100);
                      const lineTotal = lineSubTotal + lineTax;
                      
                      return (
                        <tr key={index}>
                          <td>
                            <select
                              className="form-select form-select-sm"
                              value={line.productId || ""}
                              onChange={(e) => updateLine(index, "productId", e.target.value)}
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
                      <td colSpan={5}>Subtotal</td>
                      <td>{totals.subTotal.toFixed(2)}</td>
                      <td></td>
                    </tr>
                    <tr className="font-bold">
                      <td colSpan={5}>Tax</td>
                      <td>{totals.taxAmount.toFixed(2)}</td>
                      <td></td>
                    </tr>
                    <tr className="font-bold text-lg">
                      <td colSpan={5}>Total</td>
                      <td>{totals.totalAmount.toFixed(2)}</td>
                      <td></td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/invoices" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving?" : isEdit ? "Save changes" : "Add Invoice"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default InvoiceForm;

