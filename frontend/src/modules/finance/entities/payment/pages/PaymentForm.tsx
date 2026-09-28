import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { paymentService, type CreatePaymentDto, type UpdatePaymentDto, type PaymentDto } from "../services";
import { invoiceService } from "../../invoice/services";
import { billService } from "../../bill/services";
import { currencyService } from "../../currency/services";
import { PAYMENT_TYPE_OPTIONS, PAYMENT_METHOD_OPTIONS, PAYMENT_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const PaymentForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [payment, setPayment] = useState<PaymentDto | null>(null);
  const [invoices, setInvoices] = useState<any[]>([]);
  const [bills, setBills] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);

  useEffect(() => {
    loadInvoices();
    loadBills();
    loadCurrencies();
    if (isEdit && id) {
      loadPayment(id);
    }
  }, [isEdit, id]);

  const loadPayment = async (paymentId: string) => {
    try {
      const paymentData = await paymentService.getById(paymentId);
      setPayment(paymentData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load payment");
    }
  };

  const loadInvoices = async () => {
    try {
      const data = await invoiceService.getAll();
      setInvoices(data);
    } catch (err: any) {
      console.error("Failed to load invoices", err);
    }
  };

  const loadBills = async () => {
    try {
      const data = await billService.getAll();
      setBills(data);
    } catch (err: any) {
      console.error("Failed to load bills", err);
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

  const paymentSchema = yup.object({
    paymentNumber: yup.string(),
    paymentDate: yup.string().required("Payment date is required"),
    paymentType: yup.string(),
    paymentMethod: yup.string().required("Payment method is required"),
    amount: yup.number().min(0.01, "Amount must be > 0").required("Amount is required"),
    currencyCode: yup.string(),
    relatedInvoiceId: yup.string().uuid("Invalid invoice").nullable(),
    relatedBillId: yup.string().uuid("Invalid bill").nullable(),
    status: yup.string(),
    reference: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdatePaymentDto = {
          paymentId: id,
          paymentNumber: data.paymentNumber || undefined,
          paymentDate: data.paymentDate,
          paymentType: data.paymentType || undefined,
          paymentMethod: data.paymentMethod,
          amount: parseFloat(data.amount) || 0,
          currencyCode: data.currencyCode || "USD",
          relatedInvoiceId: data.relatedInvoiceId || undefined,
          relatedBillId: data.relatedBillId || undefined,
          status: data.status || "Pending",
          reference: data.reference || undefined,
          notes: data.notes || undefined,
        };
        await paymentService.update(id, updateData);
      } else {
        const createData: CreatePaymentDto = {
          paymentNumber: data.paymentNumber || undefined,
          paymentDate: data.paymentDate,
          paymentType: data.paymentType || undefined,
          paymentMethod: data.paymentMethod,
          amount: parseFloat(data.amount) || 0,
          currencyCode: data.currencyCode || "USD",
          relatedInvoiceId: data.relatedInvoiceId || undefined,
          relatedBillId: data.relatedBillId || undefined,
          status: data.status || "Pending",
          reference: data.reference || undefined,
          notes: data.notes || undefined,
        };
        await paymentService.create(createData);
      }
      navigate("/finance/payments");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save payment");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && payment
    ? {
        paymentNumber: payment.paymentNumber || "",
        paymentDate: payment.paymentDate ? payment.paymentDate.split('T')[0] : new Date().toISOString().split('T')[0],
        paymentType: payment.paymentType || "",
        paymentMethod: payment.paymentMethod || "",
        amount: payment.amount || 0,
        currencyCode: payment.currencyCode || "USD",
        relatedInvoiceId: payment.relatedInvoiceId || "",
        relatedBillId: payment.relatedBillId || "",
        status: payment.status || "Pending",
        reference: payment.reference || "",
        notes: payment.notes || "",
      }
    : {
        paymentNumber: "",
        paymentDate: new Date().toISOString().split('T')[0],
        paymentType: "",
        paymentMethod: "",
        amount: 0,
        currencyCode: "USD",
        relatedInvoiceId: "",
        relatedBillId: "",
        status: "Pending",
        reference: "",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Payment" : "Add Payment"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/payments" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(paymentSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Payment Number"
                type="text"
                name="paymentNumber"
                placeholder="Auto-generated if empty"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Payment Date"
                type="date"
                name="paymentDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Payment Type"
                type="select"
                name="paymentType"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select payment type</option>
                {PAYMENT_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Payment Method"
                type="select"
                name="paymentMethod"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select payment method</option>
                {PAYMENT_METHOD_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Amount"
                type="number"
                name="amount"
                placeholder="0.00"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.01"
                min="0.01"
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
                label="Related Invoice"
                type="select"
                name="relatedInvoiceId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">None</option>
                {invoices.map((invoice) => (
                  <option key={invoice.invoiceId || invoice.id} value={invoice.invoiceId || invoice.id}>
                    {invoice.invoiceNumber} - {invoice.customerName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Related Bill"
                type="select"
                name="relatedBillId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">None</option>
                {bills.map((bill) => (
                  <option key={bill.billId || bill.id} value={bill.billId || bill.id}>
                    {bill.billNumber} - {bill.vendorName || bill.vendorId}
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
                {PAYMENT_STATUS_OPTIONS.map((option) => (
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

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/payments" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Payment"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default PaymentForm;

