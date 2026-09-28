// Payroll Entity Types
// Based on ERP.Shared.DTOs.HRM.PayrollDto

export interface PayrollLineDto {
  id?: string;
  payrollId: string;
  employeeId: string;
  employeeName?: string;
  employeeCode?: string;
  baseSalary: number;
  earnings: number;
  allowances: number;
  deductions: number;
  taxAmount: number;
  netPay: number;
  workingDays: number;
  presentDays: number;
  absentDays: number;
  overtimeHours: number;
  overtimeAmount: number;
}

export interface PayrollDto {
  id?: string;
  tenantId?: string;
  payrollNumber: string;
  payPeriodStart: string;
  payPeriodEnd: string;
  payDate: string;
  status: string;
  currency: string;
  totalGross: number;
  totalDeductions: number;
  totalNet: number;
  employeeCount: number;
  processedAt?: string;
  processedBy?: string;
  notes?: string;
  companyId?: string;
  employeeId?: string;
  basicSalary: number;
  allowances: number;
  deductions: number;
  grossSalary: number;
  netSalary: number;
  payrollLines?: PayrollLineDto[];
}

export interface CreatePayrollDto {
  payrollNumber?: string;
  payPeriodStart: string;
  payPeriodEnd: string;
  payDate: string;
  status?: string;
  currency?: string;
  notes?: string;
}

export interface UpdatePayrollDto {
  id: string;
  payrollNumber?: string;
  payPeriodStart?: string;
  payPeriodEnd?: string;
  payDate?: string;
  status?: string;
  currency?: string;
  notes?: string;
}

// Payroll Status Options
export const PAYROLL_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Processing", label: "Processing" },
  { value: "Processed", label: "Processed" },
  { value: "Approved", label: "Approved" },
  { value: "Paid", label: "Paid" },
  { value: "Cancelled", label: "Cancelled" },
];

