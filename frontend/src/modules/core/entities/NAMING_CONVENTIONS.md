# Entity Naming Conventions

This document defines the standard naming conventions for all entities in the Core module.

## Standard Naming Pattern

### 1. Service Files
- **File Name**: `{entityName}Service.ts` (camelCase)
- **Location**: `entities/{entity-name}/services/{entityName}Service.ts`

**Examples:**
- `userService.ts`
- `roleService.ts`
- `tenantService.ts`
- `auditLogService.ts`
- `systemLogService.ts`
- `systemSettingService.ts`

### 2. Service Classes
- **Class Name**: `{EntityName}Service` (PascalCase)
- **Pattern**: First letter of each word capitalized

**Examples:**
- `UserService`
- `RoleService`
- `TenantService`
- `AuditLogService`
- `SystemLogService`
- `SystemSettingService`

### 3. Service Instances
- **Instance Name**: `{entityName}Service` (camelCase)
- **Export**: `export const {entityName}Service = new {EntityName}Service();`

**Examples:**
- `userService`
- `roleService`
- `tenantService`
- `auditLogService`
- `systemLogService`
- `systemSettingService`

### 4. DTO Interfaces
- **Base DTO**: `{EntityName}Dto` (PascalCase)
- **Create DTO**: `Create{EntityName}Dto` (PascalCase)
- **Update DTO**: `Update{EntityName}Dto` (PascalCase)
- **Action DTOs**: `{Action}{EntityName}Dto` (PascalCase)

**Examples:**
- `UserDto`, `CreateUserDto`, `UpdateUserDto`, `ResetUserPasswordDto`
- `RoleDto`, `CreateRoleDto`, `UpdateRoleDto`, `AssignPermissionsDto`
- `TenantDto`, `CreateTenantDto`, `UpdateTenantDto`
- `AuditLogDto`
- `SystemLogDto`
- `SystemSettingDto`, `CreateSystemSettingDto`, `UpdateSystemSettingDto`, `UpsertSystemSettingDto`

### 5. Page Components
- **List Component**: `{EntityName}List.tsx` (PascalCase)
- **Form Component**: `{EntityName}Form.tsx` (PascalCase)
- **Action Components**: `{Action}{EntityName}Form.tsx` (PascalCase)

**Examples:**
- `UserList.tsx`, `UserForm.tsx`, `ResetPasswordForm.tsx`
- `RoleList.tsx`, `RoleForm.tsx`, `AssignPermissionsForm.tsx`
- `TenantList.tsx`, `TenantForm.tsx`
- `AuditLogList.tsx`
- `SystemLogList.tsx`
- `SystemSettingList.tsx`, `SystemSettingForm.tsx`

### 6. Page Exports
- **Export Pattern**: Named exports matching component names
- **File**: `entities/{entity-name}/pages/index.ts`

**Examples:**
```typescript
export { default as UserList } from './UserList';
export { default as UserForm } from './UserForm';
export { default as ResetPasswordForm } from './ResetPasswordForm';
```

### 7. Service Exports
- **Export Pattern**: Export service instance and all DTO types
- **File**: `entities/{entity-name}/services/index.ts`

**Examples:**
```typescript
export { userService, type UserDto, type CreateUserDto, type UpdateUserDto, type ResetUserPasswordDto } from './userService';
```

### 8. Directory Structure
- **Entity Directory**: `{entity-name}` (kebab-case)
- **Subdirectories**: `components/`, `pages/`, `services/`, `types/`, `hooks/`, `utils/`

**Examples:**
- `user/`
- `role/`
- `tenant/`
- `audit-log/`
- `system-log/`
- `system-setting/` or `settings/`

## Entity Name Mapping

| Entity | Directory | Service File | Service Class | Service Instance | DTO Base |
|--------|-----------|--------------|---------------|------------------|----------|
| User | `user/` | `userService.ts` | `UserService` | `userService` | `UserDto` |
| Role | `role/` | `roleService.ts` | `RoleService` | `roleService` | `RoleDto` |
| Tenant | `tenant/` | `tenantService.ts` | `TenantService` | `tenantService` | `TenantDto` |
| Audit Log | `audit-log/` | `auditLogService.ts` | `AuditLogService` | `auditLogService` | `AuditLogDto` |
| System Log | `system-log/` | `systemLogService.ts` | `SystemLogService` | `systemLogService` | `SystemLogDto` |
| System Setting | `settings/` | `systemSettingService.ts` | `SystemSettingService` | `systemSettingService` | `SystemSettingDto` |

## Service Method Naming

All services should implement these standard methods:

- `getAll(params?)` - Get all entities
- `getById(id)` - Get entity by ID
- `create(dto)` - Create new entity
- `update(id, dto)` - Update existing entity
- `delete(id)` - Delete entity

Additional methods follow the pattern:
- `getBy{Field}(value)` - Get by specific field
- `{Action}(id, dto?)` - Custom actions

## Consistency Rules

1. ✅ **Service files**: Always camelCase
2. ✅ **Service classes**: Always PascalCase
3. ✅ **Service instances**: Always camelCase
4. ✅ **DTO interfaces**: Always PascalCase with "Dto" suffix
5. ✅ **Page components**: Always PascalCase
6. ✅ **Directory names**: Always kebab-case
7. ✅ **File names**: Match the component/class name exactly

