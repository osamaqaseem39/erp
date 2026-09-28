# File and Folder Naming Conventions

## ✅ Standardized Structure

All entities now follow the exact same folder and file naming conventions.

## Directory Naming Rules

### Entity Directories
- **Single word**: Use lowercase
  - ✅ `user/`
  - ✅ `role/`
  - ✅ `tenant/`
- **Multi-word**: Use kebab-case
  - ✅ `audit-log/`
  - ✅ `system-log/`
  - ✅ `system-setting/` or `settings/`

### Subdirectories (Same for all entities)
- ✅ `components/` - Entity-specific components
- ✅ `hooks/` - Entity-specific hooks
- ✅ `pages/` - Entity pages
- ✅ `services/` - Entity API services
- ✅ `types/` - Entity TypeScript types
- ✅ `utils/` - Entity-specific utilities

## File Naming Rules

### Service Files
- **Pattern**: `{entityName}Service.ts` (camelCase)
- **Location**: `entities/{entity-name}/services/{entityName}Service.ts`

**Examples:**
- ✅ `userService.ts`
- ✅ `roleService.ts`
- ✅ `tenantService.ts`
- ✅ `auditLogService.ts`
- ✅ `systemLogService.ts`
- ✅ `systemSettingService.ts`

### Component Files
- **Pattern**: `{EntityName}{Type}.tsx` (PascalCase)
- **Location**: `entities/{entity-name}/pages/{EntityName}{Type}.tsx`

**Types:**
- `List` - List/table view
- `Form` - Create/Edit form
- `Detail` - Detail view
- `{Action}Form` - Action-specific forms

**Examples:**
- ✅ `UserList.tsx`, `UserForm.tsx`, `ResetPasswordForm.tsx`
- ✅ `RoleList.tsx`, `RoleForm.tsx`, `AssignPermissionsForm.tsx`
- ✅ `TenantList.tsx`, `TenantForm.tsx`
- ✅ `AuditLogList.tsx`
- ✅ `SystemLogList.tsx`
- ✅ `SystemSettingList.tsx`, `SystemSettingForm.tsx`

### Index Files
- **Pattern**: Always `index.ts` (lowercase)
- **Location**: Every directory has an `index.ts` for exports

## Complete Entity Structure

Every entity follows this exact structure:

```
{entity-name}/                          # kebab-case or lowercase
├── components/
│   └── index.ts                       # Export components
├── hooks/
│   └── index.ts                       # Export hooks
├── pages/
│   ├── {EntityName}List.tsx          # List page
│   ├── {EntityName}Form.tsx          # Form page
│   └── index.ts                      # Export pages
├── services/
│   ├── {entityName}Service.ts        # Service file
│   └── index.ts                      # Export service + DTOs
├── types/
│   └── index.ts                      # Export types
├── utils/
│   └── index.ts                      # Export utilities
└── index.ts                          # Main entity exports
```

## Verification

### ✅ User Entity
```
user/
├── components/index.ts ✅
├── hooks/index.ts ✅
├── pages/
│   ├── UserList.tsx ✅
│   ├── UserForm.tsx ✅
│   ├── ResetPasswordForm.tsx ✅
│   └── index.ts ✅
├── services/
│   ├── userService.ts ✅
│   └── index.ts ✅
├── types/index.ts ✅
├── utils/index.ts ✅
└── index.ts ✅
```

### ✅ Role Entity
```
role/
├── components/index.ts ✅
├── hooks/index.ts ✅
├── pages/
│   ├── RoleList.tsx ✅
│   ├── RoleForm.tsx ✅
│   ├── AssignPermissionsForm.tsx ✅
│   └── index.ts ✅
├── services/
│   ├── roleService.ts ✅
│   └── index.ts ✅
├── types/index.ts ✅
├── utils/index.ts ✅
└── index.ts ✅
```

### ✅ Tenant Entity
```
tenant/
├── components/index.ts ✅
├── hooks/index.ts ✅
├── pages/
│   ├── TenantList.tsx ✅
│   ├── TenantForm.tsx ✅
│   └── index.ts ✅
├── services/
│   ├── tenantService.ts ✅
│   └── index.ts ✅
├── types/index.ts ✅
├── utils/index.ts ✅
└── index.ts ✅
```

### ✅ Audit Log Entity
```
audit-log/
├── components/index.ts ✅
├── hooks/index.ts ✅
├── pages/
│   ├── AuditLogList.tsx ✅
│   └── index.ts ✅
├── services/
│   ├── auditLogService.ts ✅
│   └── index.ts ✅
├── types/index.ts ✅
├── utils/index.ts ✅
└── index.ts ✅
```

### ✅ System Log Entity
```
system-log/
├── components/index.ts ✅
├── hooks/index.ts ✅
├── pages/
│   ├── SystemLogList.tsx ✅
│   └── index.ts ✅
├── services/
│   ├── systemLogService.ts ✅
│   └── index.ts ✅
├── types/index.ts ✅
├── utils/index.ts ✅
└── index.ts ✅
```

### ✅ Settings Entity
```
settings/
├── components/index.ts ✅
├── hooks/index.ts ✅
├── pages/
│   ├── Settings.tsx ✅
│   ├── SystemSettingList.tsx ✅
│   ├── SystemSettingForm.tsx ✅
│   └── index.ts ✅
├── services/
│   ├── settingsService.ts ✅
│   ├── systemSettingService.ts ✅
│   └── index.ts ✅
├── types/index.ts ✅
├── utils/index.ts ✅
└── index.ts ✅
```

## Summary

✅ **All directories**: Consistent structure with all subdirectories
✅ **All files**: Follow naming conventions (camelCase for services, PascalCase for components)
✅ **All index files**: Created for consistent exports
✅ **All entities**: Have main `index.ts` for entity-level exports

**Status: COMPLETE** - All file and folder naming conventions are now standardized across all entities.

