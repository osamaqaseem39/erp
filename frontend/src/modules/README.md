# ERP Frontend Module Structure

This directory contains the module-wise, entity-wise, and component-wise organization of the ERP frontend application.

## Structure Overview

```
modules/
├── core/              # Core module (Tenants, Auth, etc.)
├── admin/             # Admin module (Users, Roles, Settings, Logs)
├── hrm/               # HRM module (Employees, Payroll, Leave, etc.)
├── finance/           # Finance module (Invoices, Payments, Accounts, etc.)
├── inventory/         # Inventory module (Items, Warehouses, Stock, etc.)
├── crm/               # CRM module (Customers, Leads, Opportunities, etc.)
├── manufacturing/     # Manufacturing module
├── procurement/       # Procurement module
├── projects/          # Projects module
└── reports/           # Reports module
```

## Module Structure

Each module follows this structure:

```
module-name/
├── components/        # Module-specific shared components
├── entities/          # Entity-specific code
│   └── entity-name/
│       ├── components/    # Entity-specific components
│       ├── pages/         # Entity pages (List, Form, Detail, etc.)
│       ├── hooks/         # Entity-specific hooks
│       ├── services/      # Entity API services
│       ├── types/         # Entity TypeScript types
│       └── utils/         # Entity-specific utilities
├── hooks/             # Module-level hooks
├── services/          # Module-level services
├── types/             # Module-level types
└── utils/             # Module-level utilities
```

## Entity Structure

Each entity follows this structure:

```
entity-name/
├── components/        # Entity-specific components
│   ├── EntityList.tsx
│   ├── EntityForm.tsx
│   ├── EntityCard.tsx
│   └── EntityDetail.tsx
├── pages/             # Entity pages
│   ├── List.tsx
│   ├── Form.tsx
│   ├── Detail.tsx
│   └── index.ts
├── hooks/             # Entity-specific hooks
│   ├── useEntity.ts
│   ├── useEntityList.ts
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

## Component Organization

### Shared Components (`src/components/`)
- Common UI components used across all modules
- Layout components
- Form components
- UI primitives

### Module Components (`modules/{module}/components/`)
- Components specific to a module
- Shared within the module but not across modules

### Entity Components (`modules/{module}/entities/{entity}/components/`)
- Components specific to an entity
- Reusable within the entity context

## Naming Conventions

- **Components**: PascalCase (e.g., `UserList.tsx`, `InvoiceForm.tsx`)
- **Hooks**: camelCase with `use` prefix (e.g., `useUser.ts`, `useInvoiceList.ts`)
- **Services**: camelCase with `Service` suffix (e.g., `userService.ts`, `invoiceService.ts`)
- **Types**: camelCase with `.types.ts` suffix (e.g., `user.types.ts`, `invoice.types.ts`)
- **Pages**: PascalCase (e.g., `List.tsx`, `Form.tsx`, `Detail.tsx`)

## Import Patterns

```typescript
// Module-level imports
import { UserList } from '@/modules/core/entities/user/pages';
import { userService } from '@/modules/core/entities/user/services';
import type { UserDto } from '@/modules/core/entities/user/services';

// Shared component imports
import { Button, Card } from '@/components';
import { PageBreadcrumb } from '@/components';
```

