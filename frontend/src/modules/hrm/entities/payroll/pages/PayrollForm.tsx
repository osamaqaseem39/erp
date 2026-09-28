import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { payrollService, type CreatePayrollDto, type UpdatePayrollDto, type PayrollDto } from "../services";
import { PAYROLL_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const PayrollForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [payroll, setPayroll] = useState<PayrollDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadPayroll(id);
    }
  }, [isEdit, id]);

  const loadPayroll = async (payrollId: string) => {
    try {
      const payrollData = await payrollService.getById(payrollId);
      setPayroll(payrollData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load payroll");
    }
  };

  const payrollSchema = yup.object({
    payrollNumber: yup.string(),
    payPeriodStart: yup.string().required("Pay period start date is required"),
    payPeriodEnd: yup.string().required("Pay period end date is required"),
    payDate: yup.string().required("Pay date is required"),
    status: yup.string(),
    currency: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdatePayrollDto = {
          id,
          payrollNumber: data.payrollNumber || undefined,
          payPeriodStart: data.payPeriodStart,
          payPeriodEnd: data.payPeriodEnd,
          payDate: data.payDate,
          status: data.status || "Draft",
          currency: data.currency || "USD",
          notes: data.notes || undefined,
        };
        await payrollService.update(id, updateData);
      } else {
        const createData: CreatePayrollDto = {
          payrollNumber: data.payrollNumber || undefined,
          payPeriodStart: data.payPeriodStart,
          payPeriodEnd: data.payPeriodEnd,
          payDate: data.payDate,
          status: data.status || "Draft",
          currency: data.currency || "USD",
          notes: data.notes || undefined,
        };
        await payrollService.create(createData);
      }
      navigate("/hrm/payrolls");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save payroll");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && payroll
    ? {
        payrollNumber: payroll.payrollNumber || "",
        payPeriodStart: payroll.payPeriodStart ? payroll.payPeriodStart.split('T')[0] : new Date().toISOString().split('T')[0],
        payPeriodEnd: payroll.payPeriodEnd ? payroll.payPeriodEnd.split('T')[0] : new Date().toISOString().split('T')[0],
        payDate: payroll.payDate ? payroll.payDate.split('T')[0] : new Date().toISOString().split('T')[0],
        status: payroll.status || "Draft",
        currency: payroll.currency || "USD",
        notes: payroll.notes || "",
      }
    : {
        payrollNumber: "",
        payPeriodStart: new Date().toISOString().split('T')[0],
        payPeriodEnd: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 14 days from now
        payDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        status: "Draft",
        currency: "USD",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Payroll" : "Add Payroll"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/payrolls" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(payrollSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Payroll Number"
                type="text"
                name="payrollNumber"
                placeholder="Auto-generated if empty"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {PAYROLL_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Pay Period Start"
                type="date"
                name="payPeriodStart"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Pay Period End"
                type="date"
                name="payPeriodEnd"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Pay Date"
                type="date"
                name="payDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Currency"
                type="text"
                name="currency"
                placeholder="USD"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              {isEdit && payroll && (
                <>
                  <div className="md:col-span-2 grid grid-cols-1 gap-4 md:grid-cols-3">
                    <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded">
                      <div className="text-sm text-gray-600 dark:text-gray-400">Total Gross</div>
                      <div className="text-lg font-semibold">{payroll.totalGross.toFixed(2)}</div>
                    </div>
                    <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded">
                      <div className="text-sm text-gray-600 dark:text-gray-400">Total Deductions</div>
                      <div className="text-lg font-semibold">{payroll.totalDeductions.toFixed(2)}</div>
                    </div>
                    <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded">
                      <div className="text-sm text-gray-600 dark:text-gray-400">Total Net</div>
                      <div className="text-lg font-semibold text-green-600">{payroll.totalNet.toFixed(2)}</div>
                    </div>
                  </div>
                  <div className="md:col-span-2">
                    <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded">
                      <div className="text-sm text-gray-600 dark:text-gray-400">Employee Count</div>
                      <div className="text-lg font-semibold">{payroll.employeeCount}</div>
                    </div>
                  </div>
                </>
              )}

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
              <Link to="/hrm/payrolls" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Payroll"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default PayrollForm;

