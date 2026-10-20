# Business Modules — ERP SaaS

Module inventory from domain folders, controllers, frontend routes, and DI registration.

| Module | Domain folder | Controllers | Frontend | DI in `Program.cs` |
|--------|---------------|-------------|----------|--------------------|
| **Core / Tenancy** | `ERP.Domain/Core` | Tenants, Setup, Auth | `/setup`, `/core`, `/erp/tenants` | **Yes** |
| **Admin** | Core + Admin entities | Users, Roles, Permissions, Settings, Audit/System logs | `/core/settings/*`, `/erp/admin/*` | **Yes** |
| **CRM** | `ERP.Domain/CRM` | Full CRM set | `/crm/*` | **Yes** |
| **Sales / POS** | `ERP.Domain/Sales` | Sales invoices, POS sessions/transactions | `/pos/*`, finance invoice UI separate | **Yes** |
| **Finance** | `ERP.Domain/Finance` | Accounting, CoA, currency, bank, tax, AR/AP, assets, reports | `/finance/*` | **Yes** (`AddErpFinanceModule`) |
| **HRM** | `ERP.Domain/HRM` | Employees, leave, payroll, recruitment, etc. | `/hrm/*` | **Yes** (`AddErpHrmModule`) |
| **Inventory / SCM** | `ERP.Domain/SCM` | Large inventory controller set | `/inventory/*` | **Yes** (`AddErpInventoryModule`) |
| **Procurement** | `ERP.Domain/Procurement` | Vendors, purchase orders | `/procurement/*` | **Yes** (`AddErpProcurementModule`) |
| **Manufacturing** | `ERP.Domain/Manufacturing` | Production orders | `/manufacturing/*` | **Yes** (`AddErpManufacturingModule`) |
| **Projects** | `ERP.Domain/Projects` | Projects | `/projects` | **Yes** (`AddErpProjectsModule`) |
| **Reports** | `ERP.Domain/Reports` | Reports | Hub `/reports` + `frontend-new` catalog/history/schedules | **Yes** (`AddErpReportsModule`) |

## CRM pipeline (wired)

```text
Lead → Customer / Opportunity → Quote (+ lines) → Sales Order (+ lines)
         ↑
      Contact, Activity, Campaign
```

Extended: customer groups, price lists, sales territories, return orders.

Living completion tracker: [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md).

## Cross-module integration status (honest)

- CRM sales-order **invoice** action is present; Finance **is** DI-wired and SO→AR works. Remaining honesty gap: FinanceGap fund/credit/debit **post without GL**.
- Inventory reservation / procurement fulfillment from CRM SO needs staging E2E confirmation (local Wave2 O2C/P2P tests exist).
- Separate standalone HRM GraphQL product under `D:\Projects\hrm` is **not** this ERP HRM module.

## Template modules

Calendar, tickets, file manager, kanban, and demo pages are UI template remnants — not ERP business modules.
