import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link, useSearchParams } from "react-router-dom";
import { useFormContext, useWatch } from "react-hook-form";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { posTransactionService, type CreatePOSTransactionDto, type UpdatePOSTransactionDto, type POSTransactionDto, type POSTransactionLineDto, type CreatePOSTransactionLineDto } from "../services";
import { posSessionService } from "../../pos-session/services";
import { customerService } from "../../../../crm/entities/customer/services";
import { PAYMENT_METHOD_OPTIONS, TRANSACTION_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const POSTransactionForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const sessionIdParam = searchParams.get('sessionId');
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [sessions, setSessions] = useState<any[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [transaction, setTransaction] = useState<POSTransactionDto | null>(null);
  const [lines, setLines] = useState<CreatePOSTransactionLineDto[]>([]);

  useEffect(() => {
    loadSessions();
    loadCustomers();
    if (isEdit && id) {
      loadTransaction(id);
    } else {
      // Initialize with one empty line
      setLines([{
        itemId: "",
        description: "",
        quantity: 1,
        unitPrice: 0,
        discountRate: 0,
        taxRate: 0,
        lineNumber: 1,
      }]);
    }
  }, [isEdit, id]);

  const loadTransaction = async (transactionId: string) => {
    try {
      const transactionData = await posTransactionService.getById(transactionId);
      setTransaction(transactionData);
      if (transactionData.lines && transactionData.lines.length > 0) {
        setLines(transactionData.lines.map(line => ({
          itemId: line.itemId,
          description: line.description,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
          discountRate: line.discountRate,
          taxRate: line.taxRate,
          lineNumber: line.lineNumber,
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load POS transaction");
    }
  };

  const loadSessions = async () => {
    try {
      const data = await posSessionService.getAll(undefined, "Open");
      setSessions(data);
    } catch (err: any) {
      console.error("Failed to load POS sessions", err);
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

  const addLine = () => {
    setLines([...lines, {
      itemId: "",
      description: "",
      quantity: 1,
      unitPrice: 0,
      discountRate: 0,
      taxRate: 0,
      lineNumber: lines.length + 1,
    }]);
  };

  const removeLine = (index: number) => {
    const newLines = lines.filter((_, i) => i !== index);
    // Renumber lines
    newLines.forEach((line, i) => {
      line.lineNumber = i + 1;
    });
    setLines(newLines);
  };

  const updateLine = (index: number, field: string, value: any) => {
    const newLines = [...lines];
    (newLines[index] as any)[field] = value;
    
    // Ensure numeric fields are numbers
    if (field === "quantity" || field === "unitPrice" || field === "discountRate" || field === "taxRate") {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
    }
    
    setLines(newLines);
  };

  const calculateTotals = () => {
    let subTotal = 0;
    let totalTax = 0;
    let totalDiscount = 0;

    lines.forEach(line => {
      const lineSubTotal = line.quantity * line.unitPrice;
      const lineDiscount = lineSubTotal * (line.discountRate / 100);
      const lineAfterDiscount = lineSubTotal - lineDiscount;
      const lineTax = lineAfterDiscount * (line.taxRate / 100);
      
      subTotal += lineSubTotal;
      totalDiscount += lineDiscount;
      totalTax += lineTax;
    });

    return {
      subTotal,
      taxAmount: totalTax,
      discountAmount: totalDiscount,
      totalAmount: subTotal - totalDiscount + totalTax,
    };
  };

  const formatDateTimeForInput = (dateString?: string): string => {
    if (!dateString) return "";
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "";
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const formatDateTimeForAPI = (dateString?: string): string | undefined => {
    if (!dateString) return undefined;
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return undefined;
    return date.toISOString();
  };

  const transactionSchema = yup.object({
    transactionNumber: yup.string().required("Transaction number is required"),
    sessionId: yup.string().uuid("Invalid session").required("Session is required"),
    customerId: yup.string().uuid("Invalid customer").required("Customer is required"),
    transactionDate: yup.date().required("Transaction date is required"),
    paymentMethod: yup.string().oneOf(["Cash", "Card", "Mobile", "Other"], "Invalid payment method").required("Payment method is required"),
    amountPaid: yup.number().min(0, "Amount paid must be >= 0").required("Amount paid is required"),
    status: yup.string().oneOf(["Completed", "Pending", "Cancelled", "Refunded"], "Invalid status").required("Status is required"),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      const totals = calculateTotals();
      const changeAmount = data.amountPaid - totals.totalAmount;

      if (isEdit && id) {
        const updateData: UpdatePOSTransactionDto = {
          id,
          transactionNumber: data.transactionNumber,
          sessionId: data.sessionId,
          customerId: data.customerId,
          transactionDate: formatDateTimeForAPI(data.transactionDate) || new Date().toISOString(),
          paymentMethod: data.paymentMethod,
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          discountAmount: totals.discountAmount,
          totalAmount: totals.totalAmount,
          amountPaid: parseFloat(data.amountPaid) || 0,
          status: data.status,
          notes: data.notes || undefined,
          lines: lines.map((line, index) => ({
            id: transaction?.lines[index]?.id || "",
            itemId: line.itemId,
            description: line.description,
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            discountRate: line.discountRate,
            taxRate: line.taxRate,
            lineNumber: line.lineNumber,
          })),
        };
        await posTransactionService.update(id, updateData);
      } else {
        const createData: CreatePOSTransactionDto = {
          transactionNumber: data.transactionNumber,
          posSessionId: data.sessionId,
          sessionId: data.sessionId,
          customerId: data.customerId,
          transactionDate: formatDateTimeForAPI(data.transactionDate) || new Date().toISOString(),
          paymentMethod: data.paymentMethod,
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          discountAmount: totals.discountAmount,
          totalAmount: totals.totalAmount,
          amountPaid: parseFloat(data.amountPaid) || 0,
          status: data.status,
          notes: data.notes || undefined,
          lines: lines,
        };
        await posTransactionService.create(createData);
      }
      navigate("/pos/transactions");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add POSTransaction"} POS transaction`);
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();

  // Component to display change amount (must be inside form context)
  const ChangeAmountDisplay = () => {
    const amountPaid = useWatch({ name: "amountPaid" });
    const changeAmount = (parseFloat(amountPaid) || 0) - totals.totalAmount;
    return (
      <div className="bg-info/10 p-3 rounded">
        <div className="flex justify-between">
          <span>Change:</span>
          <span className={`font-semibold ${changeAmount < 0 ? "text-danger" : "text-success"}`}>
            {changeAmount.toFixed(2)}
          </span>
        </div>
      </div>
    );
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit POS Transaction" : "Add POS Transaction"} subName="POS" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/pos/transactions" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit POS Transaction" : "Add POS Transaction"}</h4>
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
            resolver={yupResolver(transactionSchema)}
            defaultValues={transaction || {
              transactionNumber: "",
              sessionId: sessionIdParam || "",
              customerId: "",
              transactionDate: formatDateTimeForInput(new Date().toISOString()),
              paymentMethod: "Cash",
              amountPaid: 0,
              status: "Completed",
              notes: "",
            }}
          >
            {/* Basic Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Transaction Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Transaction Number"
                  type="text"
                  name="transactionNumber"
                  placeholder="Enter transaction number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="POS Session"
                  type="select"
                  name="sessionId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                  disabled={!!sessionIdParam || isEdit}
                >
                  <option value="">Select session</option>
                  {sessions.map((session) => (
                    <option key={session.id} value={session.id}>
                      {session.sessionNumber} - {session.cashierName || ""}
                    </option>
                  ))}
                </FormInput>

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
                  label="Transaction Date"
                  type="datetime-local"
                  name="transactionDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Payment Method"
                  type="select"
                  name="paymentMethod"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  {PAYMENT_METHOD_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
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
                  required
                >
                  {TRANSACTION_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>
              </div>
            </div>

            {/* Transaction Lines */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Transaction Lines</h5>
                <button
                  type="button"
                  onClick={addLine}
                  className="btn btn-sm btn-soft-primary"
                >
                  <i className="mgc_add_line me-2"></i>Add Line
                </button>
              </div>

              <div className="table-responsive">
                <table className="table table-striped">
                  <thead>
                    <tr>
                      <th>Item ID</th>
                      <th>Description</th>
                      <th>Quantity</th>
                      <th>Unit Price</th>
                      <th>Discount %</th>
                      <th>Tax %</th>
                      <th>Line Total</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, index) => {
                      const lineSubTotal = line.quantity * line.unitPrice;
                      const lineDiscount = lineSubTotal * (line.discountRate / 100);
                      const lineAfterDiscount = lineSubTotal - lineDiscount;
                      const lineTax = lineAfterDiscount * (line.taxRate / 100);
                      const lineTotal = lineAfterDiscount + lineTax;

                      return (
                        <tr key={index}>
                          <td>
                            <input
                              type="text"
                              className="form-input form-input-sm"
                              value={line.itemId}
                              onChange={(e) => updateLine(index, "itemId", e.target.value)}
                              placeholder="Item ID (GUID)"
                            />
                          </td>
                          <td>
                            <input
                              type="text"
                              className="form-input form-input-sm"
                              value={line.description}
                              onChange={(e) => updateLine(index, "description", e.target.value)}
                              placeholder="Description"
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-input form-input-sm"
                              value={line.quantity}
                              onChange={(e) => updateLine(index, "quantity", parseFloat(e.target.value) || 0)}
                              min="0"
                              step="0.01"
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-input form-input-sm"
                              value={line.unitPrice}
                              onChange={(e) => updateLine(index, "unitPrice", parseFloat(e.target.value) || 0)}
                              min="0"
                              step="0.01"
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-input form-input-sm"
                              value={line.discountRate}
                              onChange={(e) => updateLine(index, "discountRate", parseFloat(e.target.value) || 0)}
                              min="0"
                              max="100"
                              step="0.01"
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-input form-input-sm"
                              value={line.taxRate}
                              onChange={(e) => updateLine(index, "taxRate", parseFloat(e.target.value) || 0)}
                              min="0"
                              max="100"
                              step="0.01"
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
                </table>
              </div>
            </div>

            {/* Totals */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Totals</h5>
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded">
                  <div className="flex justify-between mb-2">
                    <span>Subtotal:</span>
                    <span>{totals.subTotal.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between mb-2">
                    <span>Discount:</span>
                    <span className="text-danger">-{totals.discountAmount.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between mb-2">
                    <span>Tax:</span>
                    <span>{totals.taxAmount.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between font-bold text-lg border-t pt-2">
                    <span>Total:</span>
                    <span>{totals.totalAmount.toFixed(2)}</span>
                  </div>
                </div>

                <div>
                  <FormInput
                    label="Amount Paid"
                    type="number"
                    name="amountPaid"
                    placeholder="0.00"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    step="0.01"
                    min="0"
                    required
                  />
                  <ChangeAmountDisplay />
                </div>
              </div>
            </div>

            {/* Notes */}
            <div className="mb-6">
              <FormInput
                label="Notes"
                type="textarea"
                name="notes"
                rows={4}
                placeholder="Enter notes"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />
            </div>

            <div className="flex justify-end gap-2">
              <Link to="/pos/transactions" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add POSTransaction"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default POSTransactionForm;

