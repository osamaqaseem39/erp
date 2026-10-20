# Business Modules — ERP SaaS

Module inventory from domain folders, controllers, frontend routes, and DI registration.

| Module | Domain folder | Controllers | Frontend | DI in `Program.cs` |
|--------|---------------|-------------|----------|--------------------|
| **Core / Tenancy** | `ERP.Domain/Core` | Tenants, Setup, Auth | `/setup`, `/core`, `/erp/tenants` | **Yes** |
| **Admin** | Core + Admin entities | Users, Roles, Permissions, Settings, Audit/System logs | `/core/settings/*`, `/erp/admin/*` | **Yes** |
| **CRM** | `ERP.Domain/CRM` | Full CRM set | `/crm/*` | **Yes** |
| **Sales / POS** | `ERP.Domain/Sales` | Sales invoices, POS sessions/transactions | `/pos/*`, finance invoice UI separate | **Yes** |
| **Finance** | `ERP.Domain/Finance` | Accounting, CoA, currency, bank, tax, AR/AP, assets, reports | `/finance/*` | **No** |
| **HRM** | `ERP.Domain/HRM` | Employees, leave, payroll, recruitment, etc. | `/hrm/*` | **No** |
| **Inventory / SCM** | `ERP.Domain/SCM` | Large inventory controller set | `/inventory/*` | **No** |
| **Procurement** | `ERP.Domain/Procurement` | Vendors, purchase orders | `/procurement/*` | **No** |
| **Manufacturing** | `ERP.Domain/Manufacturing` | Production orders | `/manufacturing/*` | **No** |
| **Projects** | `ERP.Domain/Projects` | Projects | `/projects` | **No** |
| **Reports** | `ERP.Domain/Reports` | Reports | Hub `/reports` + finance accounts reports | **Yes** |

## CRM pipeline (wired)

```text
Lead → Customer / Opportunity → Quote (+ lines) → Sales Order (+ lines)
         ↑
      Contact, Activity, Campaign
```

Extended: customer groups, price lists, sales territories, return orders.

Living completion tracker: [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md).

## Cross-module integration status (honest)

- CRM sales-order **invoice** action is present on the API; full Finance posting depends on Finance DI (called out in CRM status doc).
- Inventory reservation / procurement fulfillment from CRM SO is not documented as complete in source reviews.
- Separate standalone HRM GraphQL product under `D:\Projects\hrm` is **not** this ERP HRM module.

## Template modules

Calendar, tickets, file manager, kanban, and demo pages are UI template remnants — not ERP business modules.
