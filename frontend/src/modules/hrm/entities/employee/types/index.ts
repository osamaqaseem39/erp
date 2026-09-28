// Employee Entity Types
// Based on ERP.Shared.DTOs.HRM.EmployeeDto

export interface EmployeeDto {
  id?: string;
  employeeId?: string;
  tenantId?: string;
  companyId?: string;
  employeeCode: string;
  salutation?: string;
  firstName: string;
  lastName: string;
  preferredName?: string;
  gender?: string;
  dateOfBirth?: string; // ISO date string
  nationality?: string;
  maritalStatus?: string;
  jobTitle?: string;
  departmentId?: string;
  managerId?: string;
  employmentType: string; // "FullTime" | "PartTime" | "Contract"
  workEmail?: string;
  personalEmail?: string;
  workPhone?: string;
  mobilePhone?: string;
  workLocationId?: string;
  dateHired?: string; // ISO date string
  dateTerminated?: string; // ISO date string
  status: string; // "Active" | "Inactive" | "Terminated"
  isRemote: boolean;
  employmentContractId?: string;
  isDeleted: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateEmployeeDto {
  companyId?: string;
  employeeCode: string;
  salutation?: string;
  firstName: string;
  lastName: string;
  preferredName?: string;
  gender?: string;
  dateOfBirth?: string; // ISO date string
  nationality?: string;
  maritalStatus?: string;
  jobTitle?: string;
  departmentId?: string;
  managerId?: string;
  employmentType: string; // "FullTime" | "PartTime" | "Contract"
  workEmail?: string;
  personalEmail?: string;
  workPhone?: string;
  mobilePhone?: string;
  workLocationId?: string;
  dateHired?: string; // ISO date string
  status: string; // "Active" | "Inactive" | "Terminated"
  isRemote: boolean;
  employmentContractId?: string;
}

export interface UpdateEmployeeDto {
  id: string;
  companyId?: string;
  salutation?: string;
  preferredName?: string;
  gender?: string;
  dateOfBirth?: string; // ISO date string
  nationality?: string;
  maritalStatus?: string;
  jobTitle?: string;
  departmentId?: string;
  managerId?: string;
  employmentType?: string;
  workEmail?: string;
  personalEmail?: string;
  workPhone?: string;
  mobilePhone?: string;
  workLocationId?: string;
  dateHired?: string; // ISO date string
  dateTerminated?: string; // ISO date string
  status?: string;
  isRemote?: boolean;
  employmentContractId?: string;
}

export interface EmployeeListDto {
  id: string;
  employeeId?: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  jobTitle?: string;
  departmentId?: string;
  department?: string;
  managerId?: string;
  workEmail?: string;
  workPhone?: string;
  status: string;
  dateHired?: string;
  isActive: boolean;
}

export interface EmployeeQuery {
  searchTerm?: string;
  departmentId?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
  companyId?: string;
  tenantId?: string;
  page?: number;
}

// Enum values for dropdowns
export const SALUTATION_OPTIONS = [
  { value: "Mr", label: "Mr" },
  { value: "Mrs", label: "Mrs" },
  { value: "Ms", label: "Ms" },
  { value: "Dr", label: "Dr" },
];

export const GENDER_OPTIONS = [
  { value: "Male", label: "Male" },
  { value: "Female", label: "Female" },
  { value: "Other", label: "Other" },
];

export const MARITAL_STATUS_OPTIONS = [
  { value: "Single", label: "Single" },
  { value: "Married", label: "Married" },
  { value: "Divorced", label: "Divorced" },
  { value: "Widowed", label: "Widowed" },
];

export const EMPLOYMENT_TYPE_OPTIONS = [
  { value: "FullTime", label: "Full Time" },
  { value: "PartTime", label: "Part Time" },
  { value: "Contract", label: "Contract" },
];

export const EMPLOYEE_STATUS_OPTIONS = [
  { value: "Active", label: "Active" },
  { value: "Inactive", label: "Inactive" },
  { value: "Terminated", label: "Terminated" },
];

