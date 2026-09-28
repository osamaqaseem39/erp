# Frontend Module Structure Guide

## Overview

The frontend is organized using a **component-wise, module-wise, entity-wise** structure for better maintainability, scalability, and code organization.

## Directory Structure

```
src/
├── modules/                    # Module-based organization
│   ├── core/                  # Core module (Tenants, Auth)
│   │   ├── components/        # Core module components
│   │   ├── entities/          # Core entities
│   │   │   ├── tenant/
│   │   │   │   ├── components/
│   │   │   │   ├── pages/
│   │   │   │   ├── hooks/
│   │   │   │   ├── services/
│   │   │   │   ├── types/
│   │   │   │   └── utils/
│   │   │   └── user/
│   │   ├── hooks/             # Module-level hooks
│   │   ├── services/          # Module-level services
│   │   ├── types/             # Module-level types
│   │   └── utils/             # Module-level utilities
│   ├── admin/                 # Admin module
│   │   ├── index.ts           # Re-exports Core entities
│   │   ├── components/        # Admin-specific components (if any)
│   │   ├── hooks/             # Admin-specific hooks (if any)
│   │   ├── services/          # Admin-specific services (if any)
│   │   ├── types/             # Admin-specific types (if any)
│   │   └── utils/             # Admin-specific utilities (if any)
│   ├── hrm/                   # HRM module
│   ├── finance/               # Finance module
│   ├── inventory/             # Inventory module
│   ├── crm/                   # CRM module
│   └── ...
├── components/                # Shared components (used across modules)
├── services/                  # Shared services
├── types/                     # Shared types
├── hooks/                     # Shared hooks
└── ...
```

## Module Organization

### 1. Core Module
**Purpose**: Core system functionality and multi-tenancy

**Entities**:
- `tenant` - Tenant management
- `user` - User management (core)

**Location**: `modules/core/`

### 2. Admin Module
**Purpose**: System administration

**Note**: Admin module uses Core module entities. All entities are in Core module.

**Entities** (from Core module):
- `user` - User administration
- `role` - Role management
- `system-setting` - System settings
- `audit-log` - Audit logs
- `system-log` - System logs

**Location**: `modules/admin/` (re-exports from `modules/core/`)

### 3. HRM Module
**Purpose**: Human Resource Management

**Entities** (to be implemented):
- `employee` - Employee management
- `payroll` - Payroll management
- `leave-request` - Leave management
- `performance-review` - Performance reviews
- `job-opening` - Job openings
- `candidate` - Candidate management
- `training` - Training management

**Location**: `modules/hrm/`

### 4. Finance Module
**Purpose**: Financial management and accounting

**Entities** (to be implemented):
- `chart-of-account` - Chart of accounts
- `bank` - Bank management
- `invoice` - Invoice management
- `bill` - Bill management
- `payment` - Payment management
- `journal-entry` - Journal entries
- `tax-rate` - Tax rates

**Location**: `modules/finance/`

### 5. Inventory Module
**Purpose**: Inventory and warehouse management

**Entities** (to be implemented):
- `warehouse` - Warehouse management
- `item` - Item management
- `stock-balance` - Stock balances
- `stock-ledger` - Stock ledgers
- `inventory-adjustment` - Inventory adjustments

**Location**: `modules/inventory/`

### 6. CRM Module
**Purpose**: Customer Relationship Management

**Entities** (to be implemented):
- `customer` - Customer management
- `lead` - Lead management
- `opportunity` - Opportunity management
- `quote` - Quote management
- `sales-order` - Sales orders
- `activity` - Activity management
- `campaign` - Campaign management

**Location**: `modules/crm/`

## Entity Structure

Each entity follows this structure:

```
entity-name/
├── components/        # Entity-specific components
│   ├── EntityList.tsx
│   ├── EntityForm.tsx
│   ├── EntityCard.tsx
│   ├── EntityDetail.tsx
│   └── index.ts
├── pages/             # Entity pages
│   ├── List.tsx       # List/Index page
│   ├── Form.tsx       # Create/Edit form
│   ├── Detail.tsx     # Detail/View page
│   └── index.ts
├── hooks/             # Entity-specific hooks
│   ├── useEntity.ts
│   ├── useEntityList.ts
│   ├── useEntityForm.ts
│   └── index.ts
├── services/          # Entity API services
│   ├── entityService.ts
│   └── index.ts
├── types/             # Entity types
│   ├── entity.types.ts
│   └── index.ts
└── utils/             # Entity utilities
    └── index.ts
```

## Import Patterns

### Using Path Aliases

```typescript
// Entity imports (from Core module)
import { UserList, UserForm } from '@/modules/core/entities/user/pages';
import { userService } from '@/modules/core/entities/user/services';
import type { UserDto, CreateUserDto } from '@/modules/core/entities/user/services';

// Module imports
import { AdminComponent } from '@/modules/admin/components';
import type { AdminType } from '@/modules/admin/types';

// Shared imports
import { Button, Card } from '@/components';
import { PageBreadcrumb } from '@/components';
import { APICore } from '@/helpers/api/apiCore';
```

### Example: User Entity

```typescript
// pages/List.tsx
import { UserList } from '@/modules/core/entities/user/pages';
import { userService } from '@/modules/core/entities/user/services';
import type { UserDto } from '@/modules/core/entities/user/services';

// Or using index exports
import { UserList } from '@/modules/core/entities/user';
```

## Naming Conventions

### Files
- **Components**: PascalCase (e.g., `UserList.tsx`, `InvoiceForm.tsx`)
- **Hooks**: camelCase with `use` prefix (e.g., `useUser.ts`, `useInvoiceList.ts`)
- **Services**: camelCase with `Service` suffix (e.g., `userService.ts`)
- **Types**: camelCase with `.types.ts` suffix (e.g., `user.types.ts`)
- **Pages**: PascalCase (e.g., `List.tsx`, `Form.tsx`, `Detail.tsx`)
- **Utils**: camelCase (e.g., `formatDate.ts`, `validateEmail.ts`)

### Directories
- **Modules**: lowercase (e.g., `admin/`, `hrm/`, `finance/`)
- **Entities**: kebab-case (e.g., `user/`, `system-setting/`, `audit-log/`)
- **Subdirectories**: lowercase (e.g., `components/`, `pages/`, `hooks/`)

## Best Practices

1. **Keep entities self-contained**: Each entity should have all its related code (components, hooks, services, types) in its own directory.

2. **Use index files**: Create `index.ts` files for easy imports and cleaner code.

3. **Shared vs Module vs Entity**: 
   - **Shared**: Used across multiple modules → `src/components/`
   - **Module**: Used within a module → `modules/{module}/components/`
   - **Entity**: Used for a specific entity → `modules/{module}/entities/{entity}/components/`

4. **Type organization**: 
   - Entity-specific types → `modules/{module}/entities/{entity}/types/`
   - Module-level types → `modules/{module}/types/`
   - Shared types → `src/types/`

5. **Service organization**:
   - Entity-specific services → `modules/{module}/entities/{entity}/services/`
   - Module-level services → `modules/{module}/services/`
   - Shared services → `src/services/`

## Migration Guide

When migrating existing code:

1. **Identify the module** (Core, Admin, HRM, etc.)
2. **Identify the entity** (User, Role, Tenant, etc.)
3. **Move files** to appropriate entity directory
4. **Update imports** to use path aliases
5. **Create index files** for easy exports
6. **Update routes** if necessary

## Example: Complete User Entity

```
modules/core/entities/user/
├── components/
│   └── index.ts
├── pages/
│   ├── UserList.tsx
│   ├── UserForm.tsx
│   ├── ResetPasswordForm.tsx
│   └── index.ts
├── hooks/
│   └── index.ts
├── services/
│   ├── userService.ts
│   └── index.ts
├── types/
│   └── index.ts
└── utils/
    └── index.ts
```

This structure provides:
- ✅ Clear separation of concerns
- ✅ Easy to find and maintain code
- ✅ Scalable for large applications
- ✅ Reusable components and hooks
- ✅ Type safety with organized types
- ✅ Consistent patterns across modules

