# Stored Procedure Naming Conventions

## Overview
This document outlines the comprehensive naming conventions used for stored procedures in the Multi-tenant SaaS ERP System.

## General Naming Pattern

### Format
```
[Schema].[sp_ActionEntityName]
```

### Components
- **Schema**: Module schema name (Core, HRM, Finance, Inventory, CRM)
- **sp_**: Standard prefix for stored procedures
- **Action**: CRUD operation or business action
- **EntityName**: Target entity or business object

## Action Prefixes

### CRUD Operations
| Prefix | Action | Description | Example |
|--------|--------|-------------|---------|
| `Get` | Read | Retrieve single record | `sp_GetUserById` |
| `GetAll` | Read | Retrieve multiple records | `sp_GetAllUsers` |
| `GetBy` | Read | Retrieve by specific field | `sp_GetUserByEmail` |
| `Create` | Create | Insert new record | `sp_CreateUser` |
| `Update` | Update | Modify existing record | `sp_UpdateUser` |
| `Delete` | Delete | Remove record | `sp_DeleteUser` |

### Business Actions
| Prefix | Action | Description | Example |
|--------|--------|-------------|---------|
| `Assign` | Business Logic | Assign relationship | `sp_AssignRoleToUser` |
| `Remove` | Business Logic | Remove relationship | `sp_RemoveRoleFromUser` |
| `Approve` | Business Logic | Approve/Authorize | `sp_ApproveLeaveRequest` |
| `Reject` | Business Logic | Reject/Deny | `sp_RejectLeaveRequest` |
| `Post` | Business Logic | Post/Commit | `sp_PostJournalEntry` |
| `Cancel` | Business Logic | Cancel operation | `sp_CancelInvoice` |
| `Convert` | Business Logic | Convert between types | `sp_ConvertLeadToOpportunity` |

### Specialized Actions
| Prefix | Action | Description | Example |
|--------|--------|-------------|---------|
| `Search` | Search | Complex search operations | `sp_SearchCustomers` |
| `Report` | Reporting | Generate reports | `sp_ReportSalesSummary` |
| `Export` | Export | Export data | `sp_ExportInventory` |
| `Import` | Import | Import data | `sp_ImportItems` |
| `Validate` | Validation | Validate data | `sp_ValidateInvoice` |
| `Calculate` | Calculation | Perform calculations | `sp_CalculatePayroll` |

## Entity Naming

### Core Module Entities
- `Tenant`, `User`, `Role`, `Permission`, `Company`, `Setting`, `AuditLog`
- `Plan`, `Subscription`

### HRM Module Entities
- `Employee`, `Payroll`, `LeaveRequest`, `LeaveType`, `TimeEntry`, `Shift`
- `PerformanceReview`, `JobOpening`, `Candidate`, `Interview`, `Training`

### Finance Module Entities
- `Account` (Chart of Accounts), `Bank`, `BankAccount`, `Invoice`, `Bill`
- `Payment`, `JournalEntry`, `TaxRate`, `Currency`

### Inventory Module Entities
- `Warehouse`, `Item`, `ItemCategory`, `StockBalance`, `StockLedger`
- `InventoryAdjustment`, `StockTransfer`, `UnitOfMeasure`, `ItemPricing`

### CRM Module Entities
- `Customer`, `Contact`, `Lead`, `Opportunity`, `Activity`
- `Quote`, `SalesOrder`, `Campaign`

## Parameter Naming

### Standard Parameters
| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `@Id` | UNIQUEIDENTIFIER | Primary key | `@Id` |
| `@TenantId` | UNIQUEIDENTIFIER | Tenant identifier | `@TenantId` |
| `@PageNumber` | INT | Pagination page | `@PageNumber` |
| `@PageSize` | INT | Pagination size | `@PageSize` |
| `@StartDate` | DATETIME2 | Date range start | `@StartDate` |
| `@EndDate` | DATETIME2 | Date range end | `@EndDate` |
| `@CreatedBy` | NVARCHAR(100) | Creator identifier | `@CreatedBy` |
| `@UpdatedBy` | NVARCHAR(100) | Updater identifier | `@UpdatedBy` |

### Filter Parameters
| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `@Status` | NVARCHAR(50) | Status filter | `@Status` |
| `@Type` | NVARCHAR(50) | Type filter | `@Type` |
| `@IsActive` | BIT | Active status filter | `@IsActive` |
| `@SearchTerm` | NVARCHAR(255) | Search text | `@SearchTerm` |

## Return Value Conventions

### Standard Returns
- **Single Record**: Return the record data
- **Multiple Records**: Return recordset with optional total count
- **Count Operations**: Return `@@ROWCOUNT` or `COUNT(*)`
- **ID Operations**: Return the created/updated ID

### Pagination
- Always include total count query for paginated results
- Use `OFFSET` and `FETCH NEXT` for SQL Server pagination
- Return both data and `TotalCount`

## Examples by Module

### Core Module Examples
```sql
-- User Management
[Core].[sp_GetUserById]
[Core].[sp_GetUserByEmail]
[Core].[sp_GetAllUsers]
[Core].[sp_CreateUser]
[Core].[sp_UpdateUser]
[Core].[sp_UpdateUserLastLogin]

-- Role Management
[Core].[sp_GetUserRoles]
[Core].[sp_AssignRoleToUser]
[Core].[sp_RemoveRoleFromUser]

-- Audit Management
[Core].[sp_CreateAuditLog]
[Core].[sp_GetAuditLogs]
```

### HRM Module Examples
```sql
-- Employee Management
[HRM].[sp_GetEmployeeById]
[HRM].[sp_GetEmployeeByCode]
[HRM].[sp_GetAllEmployees]
[HRM].[sp_CreateEmployee]
[HRM].[sp_UpdateEmployee]

-- Leave Management
[HRM].[sp_GetLeaveRequestById]
[HRM].[sp_CreateLeaveRequest]
[HRM].[sp_UpdateLeaveRequestStatus]

-- Payroll Management
[HRM].[sp_GetPayrollById]
[HRM].[sp_CreatePayroll]
[HRM].[sp_GetPayrollLinesByPayrollId]
```

### Finance Module Examples
```sql
-- Account Management
[Finance].[sp_GetAccountById]
[Finance].[sp_GetAllAccounts]
[Finance].[sp_CreateAccount]

-- Invoice Management
[Finance].[sp_GetInvoiceById]
[Finance].[sp_CreateInvoice]
[Finance].[sp_UpdateInvoiceStatus]
[Finance].[sp_GetInvoiceItemsByInvoiceId]

-- Journal Entry Management
[Finance].[sp_GetJournalEntryById]
[Finance].[sp_CreateJournalEntry]
[Finance].[sp_PostJournalEntry]
```

### Inventory Module Examples
```sql
-- Item Management
[Inventory].[sp_GetItemById]
[Inventory].[sp_GetItemByCode]
[Inventory].[sp_GetAllItems]
[Inventory].[sp_CreateItem]

-- Stock Management
[Inventory].[sp_GetStockBalance]
[Inventory].[sp_UpdateStockBalance]
[Inventory].[sp_GetStockLedgerEntriesByItem]

-- Warehouse Management
[Inventory].[sp_GetWarehouseById]
[Inventory].[sp_GetAllWarehouses]
[Inventory].[sp_CreateWarehouse]
```

### CRM Module Examples
```sql
-- Customer Management
[CRM].[sp_GetCustomerById]
[CRM].[sp_GetCustomerByCode]
[CRM].[sp_GetAllCustomers]
[CRM].[sp_CreateCustomer]
[CRM].[sp_UpdateCustomer]

-- Lead Management
[CRM].[sp_GetLeadById]
[CRM].[sp_GetAllLeads]
[CRM].[sp_CreateLead]

-- Opportunity Management
[CRM].[sp_GetOpportunityById]
[CRM].[sp_CreateOpportunity]
[CRM].[sp_GetOpportunitiesByCustomerId]

-- Quote Management
[CRM].[sp_GetQuoteById]
[CRM].[sp_CreateQuote]
[CRM].[sp_GetQuoteItemsByQuoteId]
```

## Best Practices

### 1. Consistency
- Always use the same naming pattern across all modules
- Maintain consistent parameter naming
- Use standard return value patterns

### 2. Clarity
- Names should clearly indicate the procedure's purpose
- Use descriptive action verbs
- Include entity names for clarity

### 3. Maintainability
- Follow the established patterns for easy maintenance
- Use consistent parameter ordering
- Include proper error handling

### 4. Performance
- Include appropriate indexes in WHERE clauses
- Use pagination for large result sets
- Optimize queries for performance

### 5. Security
- Always include `@TenantId` for multi-tenant procedures
- Validate input parameters
- Use parameterized queries

## Module-Specific Conventions

### Core Module
- Focus on system-wide operations
- Include authentication and authorization procedures
- Handle multi-tenancy and audit logging

### HRM Module
- Focus on employee lifecycle management
- Include payroll and time tracking procedures
- Handle leave and performance management

### Finance Module
- Focus on financial transactions and reporting
- Include accounting and banking procedures
- Handle invoicing and payment processing

### Inventory Module
- Focus on stock and warehouse management
- Include item and category procedures
- Handle stock movements and adjustments

### CRM Module
- Focus on customer relationship management
- Include sales and marketing procedures
- Handle leads, opportunities, and activities

## Summary

This naming convention ensures:
- **Consistency** across all modules
- **Clarity** in procedure purposes
- **Maintainability** for future development
- **Scalability** for additional modules
- **Professional** database design standards

All stored procedures follow these conventions to maintain a clean, organized, and professional database structure.