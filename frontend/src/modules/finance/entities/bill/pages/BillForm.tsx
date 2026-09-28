import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { billService, type CreateBillDto, type UpdateBillDto, type BillDto, type CreateBillLineDto } from "../services";
import { itemService } from "../../../../inventory/entities/item/services";
import { currencyService } from "../../currency/services";
import { chartOfAccountService } from "../../chart-of-account/services";
import { BILL_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";

const BillForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [bill, setBill] = useState<BillDto | null>(null);
  const [lines, setLines] = useState<CreateBillLineDto[]>([]);
  const [items, setItems] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);
  const [accounts, setAccounts] = useState<any[]>([]);

  useEffect(() => {
    loadItems();
    loadCurrencies();
    loadAccounts();
    if (isEdit && id) {
      loadBill(id);
    } else {
      setLines([{
        itemId: "",
        description: "",
        quantity: 1,
        unitPrice: 0,
        taxRate: 0,
      }]);
    }
  }, [isEdit, id]);

  const loadBill = async (billId: string) => {
    try {
      const billData = await billService.getById(billId);
      setBill(billData);
      if (billData.billLines && billData.billLines.length > 0) {
        setLines(billData.billLines.map(line => ({
          itemId: line.itemId || "",
          description: line.description || "",
          quantity: line.quantity,
          unitPrice: line.unitPrice,
          taxRate: line.taxRate || 0,
          glAccountId: line.glAccountId,
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load bill");
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

  const loadAccounts = async () => {
    try {
      const data = await chartOfAccountService.getAll({ isActive: true });
      setAccounts(data);
    } catch (err: any) {
      console.error("Failed to load accounts", err);
    }
  };

  const addLine = () => {
    setLines([...lines, {
      itemId: "",
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
    if (field === "itemId" && value) {
      const item = items.find(item => item.id === value);
      if (item) {
        newLines[index].description = item.itemName || "";
        newLines[index].unitPrice = item.costPrice || 0;
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

  const billSchema = yup.object({
    billNumber: yup.string(),
    vendorId: yup.string().required("Vendor is required"),
    billDate: yup.string().required("Bill date is required"),
    dueDate: yup.string().required("Due date is required"),
    currencyCode: yup.string(),
    status: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      const totals = calculateTotals();

      if (isEdit && id) {
        const updateData: UpdateBillDto = {
          billId: id,
          billNumber: data.billNumber || undefined,
          vendorId: data.vendorId,
          billDate: data.billDate,
          dueDate: data.dueDate,
          currencyCode: data.currencyCode || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          notes: data.notes || undefined,
          reference: data.reference || undefined,
        };
        await billService.update(id, updateData);
      } else {
        const createData: CreateBillDto = {
          companyId: data.companyId || "", // TODO: Get from context/state
          billNumber: data.billNumber || undefined,
          vendorId: data.vendorId,
          billDate: data.billDate,
          dueDate: data.dueDate,
          currencyCode: data.currencyCode || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          notes: data.notes || undefined,
          reference: data.reference || undefined,
          billLines: lines,
        };
        await billService.create(createData);
      }
      navigate("/finance/bills");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save bill");
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();
  const defaultValues = isEdit && bill
    ? {
        billNumber: bill.billNumber || "",
        vendorId: bill.vendorId || "",
        billDate: bill.billDate ? bill.billDate.split('T')[0] : new Date().toISOString().split('T')[0],
        dueDate: bill.dueDate ? bill.dueDate.split('T')[0] : new Date().toISOString().split('T')[0],
        currencyCode: bill.currencyCode || "USD",
        status: bill.status || "Draft",
        notes: bill.notes || "",
        reference: bill.reference || "",
      }
    : {
        billNumber: "",
        vendorId: "",
        billDate: new Date().toISOString().split('T')[0],
        dueDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        currencyCode: "USD",
        status: "Draft",
        notes: "",
        reference: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Bill" : "Add Bill"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/bills" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(billSchema)}
            defaultValues={defaultValues}
          >
            {/* Bill Header */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Bill Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Bill Number"
                  type="text"
                  name="billNumber"
                  placeholder="Auto-generated if empty"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Vendor ID"
                  type="text"
                  name="vendorId"
                  placeholder="Enter vendor ID (UUID)"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Bill Date"
                  type="date"
                  name="billDate"
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
                  {BILL_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Reference"
                  type="text"
                  name="reference"
                  placeholder="Reference number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

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

            {/* Bill Lines */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Bill Lines</h5>
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
                      <th>GL Account</th>
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
                              value={line.itemId || ""}
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
                              value={line.taxRate || 0}
                              onChange={(e) => updateLine(index, "taxRate", e.target.value)}
                              step="0.01"
                              min="0"
                              max="100"
                            />
                          </td>
                          <td>
                            <select
                              className="form-select form-select-sm"
                              value={line.glAccountId || ""}
                              onChange={(e) => updateLine(index, "glAccountId", e.target.value)}
                            >
                              <option value="">Select account</option>
                              {accounts.map((acc) => (
                                <option key={acc.accountId || acc.id} value={acc.accountId || acc.id}>
                                  {acc.accountCode} - {acc.accountName}
                                </option>
                              ))}
                            </select>
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
                      <td colSpan={6}>Subtotal</td>
                      <td>{totals.subTotal.toFixed(2)}</td>
                      <td></td>
                    </tr>
                    <tr className="font-bold">
                      <td colSpan={6}>Tax</td>
                      <td>{totals.taxAmount.toFixed(2)}</td>
                      <td></td>
                    </tr>
                    <tr className="font-bold text-lg">
                      <td colSpan={6}>Total</td>
                      <td>{totals.totalAmount.toFixed(2)}</td>
                      <td></td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/bills" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Bill"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default BillForm;

