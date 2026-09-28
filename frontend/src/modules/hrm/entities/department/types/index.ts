// Department Entity Types
// Based on ERP.Shared.DTOs.HRM.DepartmentDto

export interface DepartmentDto {
  id?: string;
  tenantId?: string;
  name: string;
  description?: string;
  managerId?: string;
  parentDepartmentId?: string;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateDepartmentDto {
  name: string;
  description?: string;
  managerId?: string;
  parentDepartmentId?: string;
  isActive: boolean;
}

export interface UpdateDepartmentDto {
  id: string;
  name: string;
  description?: string;
  managerId?: string;
  parentDepartmentId?: string;
  isActive: boolean;
}

