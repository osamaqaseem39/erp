// Core Types
export interface Tenant {
  id: string;
  name: string;
  tenantName?: string;
  domain: string;
  industry: string;
  country: string;
  status: string;
  databaseName: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateTenantDto {
  name: string;
  tenantName?: string;
  domain: string;
  industry: string;
  country: string;
  status?: string;
  isActive?: boolean;
}

export interface UpdateTenantDto {
  id: string;
  name?: string;
  tenantName?: string;
  domain?: string;
  industry?: string;
  country?: string;
  status?: string;
  isActive?: boolean;
}

export interface LoginDto {
  primaryEmail: string;
  password: string;
  tenantId: string; // Will be converted to Guid on the backend
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  permissions: string[];
  isSuperAdmin: boolean;
}

// API response might use different casing
export interface AuthResponseDto {
  Token?: string;
  RefreshToken?: string;
  ExpiresAt?: string;
  User?: User;
  Permissions?: string[];
  IsSuperAdmin?: boolean;
  // Also support lowercase for compatibility
  token?: string;
  refreshToken?: string;
  expiresAt?: string;
  user?: User;
  permissions?: string[];
  isSuperAdmin?: boolean;
}

export interface User {
  id: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  phoneNumber?: string;
  isActive: boolean;
  isEmailVerified: boolean;
  profilePicture?: string;
  companyId?: string;
  roleNames: string[];
  /** Effective permission codes from login /auth/me (module.resource.action). */
  permissions?: string[];
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

// Admin Types
export interface UserDto {
  id: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  phoneNumber?: string;
  isActive: boolean;
  isEmailVerified: boolean;
  profilePicture?: string;
  companyId?: string;
  roleNames: string[];
  createdAt?: string;
  updatedAt?: string;
}

// Admin.CreateUserDto structure (matches ERP.Shared.DTOs.Admin.CreateUserDto)
export interface CreateUserDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
  isActive?: boolean;
  roleIds?: string[];
}

export interface UpdateUserDto {
  id: string;
  username?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  companyId?: string;
  roleIds?: string[];
  isActive?: boolean;
}

export interface ResetUserPasswordDto {
  userId: string;
  newPassword: string;
  confirmPassword: string;
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  isSystemRole: boolean;
  permissions?: PermissionDto[];
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateRoleDto {
  name: string;
  description?: string;
  isActive?: boolean;
  permissionIds?: string[];
}

export interface UpdateRoleDto {
  id: string;
  name?: string;
  description?: string;
  isActive?: boolean;
  permissionIds?: string[];
}

export interface AssignPermissionsDto {
  permissionIds: string[];
}

export interface PermissionDto {
  id: string;
  name: string;
  description?: string;
  resource: string;
  action: string;
}

export interface SystemSettingDto {
  id: string;
  key: string;
  value: string;
  description?: string;
  category?: string;
  dataType: string;
  isEncrypted: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateSystemSettingDto {
  key: string;
  value: string;
  description?: string;
  category?: string;
  dataType?: string;
  isEncrypted?: boolean;
}

export interface UpdateSystemSettingDto {
  id: string;
  key?: string;
  value?: string;
  description?: string;
  category?: string;
  dataType?: string;
  isEncrypted?: boolean;
}

export interface UpsertSystemSettingDto {
  key: string;
  value: string;
  description?: string;
  category?: string;
  dataType?: string;
  isEncrypted?: boolean;
}

export interface AuditLogDto {
  id: string;
  entityName: string;
  entityId: string;
  action: string;
  oldValues?: string;
  newValues?: string;
  userId?: string;
  userName?: string;
  ipAddress?: string;
  userAgent?: string;
  timestamp: string;
}

export interface CreateAuditLogDto {
  entityName: string;
  entityId: string;
  action: string;
  oldValues?: string;
  newValues?: string;
  userId?: string;
  ipAddress?: string;
  userAgent?: string;
}

export interface SystemLogDto {
  id: string;
  level: string;
  message: string;
  exception?: string;
  userId?: string;
  userName?: string;
  source?: string;
  timestamp: string;
}

export interface CreateSystemLogDto {
  level: string;
  message: string;
  exception?: string;
  userId?: string;
  source?: string;
}

export interface UpdateSystemLogDto {
  id: string;
  level?: string;
  message?: string;
  exception?: string;
}

