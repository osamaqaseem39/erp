# Module Index

This document provides a quick reference for all modules and entities in the ERP frontend.

## Module Structure

### Core Module (`modules/core/`)
Core system functionality and multi-tenancy.

**Entities**:
- `tenant` - Tenant management
- `user` - Core user management

**Usage**:
```typescript
import { TenantList } from '@/modules/core/entities/tenant/pages';
import { tenantService } from '@/modules/core/entities/tenant/services';
```

### Admin Module (`modules/admin/`)
System administration and configuration.

**Note**: Admin module uses Core module entities. Import from Core module directly.

**Entities** (from Core module):
- `user` - User administration
- `role` - Role and permission management
- `system-setting` - System settings
- `audit-log` - Audit logs
- `system-log` - System logs

**Usage** (Use Core module):
```typescript
import { UserList, UserForm } from '@/modules/core/entities/user/pages';
import { RoleList, RoleForm } from '@/modules/core/entities/role/pages';
import { userService } from '@/modules/core/entities/user/services';
import { roleService } from '@/modules/core/entities/role/services';
```

### HRM Module (`modules/hrm/`)
Human Resource Management.

**Entities** (to be implemented):
- `employee` - Employee management
- `payroll` - Payroll management
- `leave-request` - Leave management
- `performance-review` - Performance reviews
- `job-opening` - Job openings
- `candidate` - Candidate management
- `training` - Training management

### Finance Module (`modules/finance/`)
Financial management and accounting.

**Entities** (to be implemented):
- `chart-of-account` - Chart of accounts
- `bank` - Bank management
- `invoice` - Invoice management
- `bill` - Bill management
- `payment` - Payment management
- `journal-entry` - Journal entries
- `tax-rate` - Tax rates

### Inventory Module (`modules/inventory/`)
Inventory and warehouse management.

**Entities** (to be implemented):
- `warehouse` - Warehouse management
- `item` - Item management
- `stock-balance` - Stock balances
- `stock-ledger` - Stock ledgers
- `inventory-adjustment` - Inventory adjustments

### CRM Module (`modules/crm/`)
Customer Relationship Management.

**Entities** (to be implemented):
- `customer` - Customer management
- `lead` - Lead management
- `opportunity` - Opportunity management
- `quote` - Quote management
- `sales-order` - Sales orders
- `activity` - Activity management
- `campaign` - Campaign management

## Quick Import Reference

### Admin Module (Uses Core Entities)
```typescript
// Users (from Core module)
import { UserList, UserForm } from '@/modules/core/entities/user/pages';
import { userService } from '@/modules/core/entities/user/services';
import type { UserDto, CreateUserDto } from '@/modules/core/entities/user/services';

// Roles (from Core module)
import { RoleList, RoleForm } from '@/modules/core/entities/role/pages';
import { roleService } from '@/modules/core/entities/role/services';
import type { RoleDto, CreateRoleDto } from '@/modules/core/entities/role/services';

// System Settings (from Core module)
import { SystemSettingList, SystemSettingForm } from '@/modules/core/entities/settings/pages';
import { systemSettingService } from '@/modules/core/entities/settings/services';
import type { SystemSettingDto } from '@/modules/core/entities/settings/services';

// Audit Logs (from Core module)
import { AuditLogList } from '@/modules/core/entities/audit-log/pages';
import { auditLogService } from '@/modules/core/entities/audit-log/services';

// System Logs (from Core module)
import { SystemLogList } from '@/modules/core/entities/system-log/pages';
import { systemLogService } from '@/modules/core/entities/system-log/services';
```

### Core Module
```typescript
// Tenants
import { TenantList, TenantForm } from '@/modules/core/entities/tenant/pages';
import { tenantService } from '@/modules/core/entities/tenant/services';
import type { TenantDto, CreateTenantDto } from '@/modules/core/entities/tenant/services';

// Users
import { UserList, UserForm, ResetPasswordForm } from '@/modules/core/entities/user/pages';
import { userService } from '@/modules/core/entities/user/services';
import type { UserDto, CreateUserDto, UpdateUserDto } from '@/modules/core/entities/user/services';

// Roles
import { RoleList, RoleForm, AssignPermissionsForm } from '@/modules/core/entities/role/pages';
import { roleService } from '@/modules/core/entities/role/services';
import type { RoleDto, CreateRoleDto, PermissionDto } from '@/modules/core/entities/role/services';

// System Settings
import { SystemSettingList, SystemSettingForm } from '@/modules/core/entities/settings/pages';
import { systemSettingService } from '@/modules/core/entities/settings/services';
import type { SystemSettingDto } from '@/modules/core/entities/settings/services';

// Audit Logs
import { AuditLogList } from '@/modules/core/entities/audit-log/pages';
import { auditLogService } from '@/modules/core/entities/audit-log/services';
import type { AuditLogDto } from '@/modules/core/entities/audit-log/services';

// System Logs
import { SystemLogList } from '@/modules/core/entities/system-log/pages';
import { systemLogService } from '@/modules/core/entities/system-log/services';
import type { SystemLogDto } from '@/modules/core/entities/system-log/services';
```

## Path Aliases

All path aliases are configured in `tsconfig.json` and `vite.config.ts`:

- `@/*` → `src/*`
- `@/modules/*` → `src/modules/*`
- `@/components/*` → `src/components/*`
- `@/services/*` → `src/services/*`
- `@/types/*` → `src/types/*`
- `@/hooks/*` → `src/hooks/*`
- `@/utils/*` → `src/utils/*`
- `@/helpers/*` → `src/helpers/*`
- `@/redux/*` → `src/redux/*`
- `@/routes/*` → `src/routes/*`
- `@/layouts/*` → `src/layouts/*`
- `@/pages/*` → `src/pages/*`

## Adding New Entities

When adding a new entity:

1. Create entity directory: `modules/{module}/entities/{entity-name}/`
2. Create subdirectories: `components/`, `pages/`, `hooks/`, `services/`, `types/`, `utils/`
3. Create index files in each subdirectory
4. Create main entity index file
5. Update module index file
6. Add types, services, and pages following the existing patterns

See `STRUCTURE.md` for detailed guidelines.

