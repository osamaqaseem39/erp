# Module-wise Database Schema Structure

This directory contains the module-wise database schemas for the Multi-tenant SaaS ERP System.

## Schema Organization

The database is organized into the following modules:

### 1. Core Module (`Core` Schema)
- **Purpose**: Core system functionality and multi-tenancy
- **Tables**: Tenants, Users, Roles, Permissions, Companies, Settings, AuditLogs
- **File**: `CoreModule_Schema.sql`

### 2. HRM Module (`HRM` Schema)
- **Purpose**: Human Resource Management
- **Tables**: Employees, Payrolls, LeaveRequests, PerformanceReviews, JobOpenings, Candidates, Trainings
- **File**: `HRMModule_Schema.sql`

### 3. Finance Module (`Finance` Schema)
- **Purpose**: Financial management and accounting
- **Tables**: ChartOfAccounts, Banks, Invoices, Bills, Payments, JournalEntries, TaxRates
- **File**: `FinanceModule_Schema.sql`

### 4. Inventory Module (`Inventory` Schema)
- **Purpose**: Inventory and warehouse management
- **Tables**: Warehouses, Items, StockBalances, StockLedgers, InventoryAdjustments
- **File**: `InventoryModule_Schema.sql`

### 5. CRM Module (`CRM` Schema)
- **Purpose**: Customer Relationship Management
- **Tables**: Customers, Leads, Opportunities, Activities, Quotes, SalesOrders, Campaigns
- **File**: `CRMModule_Schema.sql`

### 6. Manufacturing Module (`Manufacturing` Schema)
- **Purpose**: Manufacturing and production management
- **Tables**: (To be implemented)
- **File**: `ManufacturingModule_Schema.sql` (Future)

### 7. Procurement Module (`Procurement` Schema)
- **Purpose**: Procurement and vendor management
- **Tables**: (To be implemented)
- **File**: `ProcurementModule_Schema.sql` (Future)

### 8. Projects Module (`Projects` Schema)
- **Purpose**: Project management
- **Tables**: (To be implemented)
- **File**: `ProjectsModule_Schema.sql` (Future)

### 9. Reports Module (`Reports` Schema)
- **Purpose**: Reporting and analytics
- **Tables**: (To be implemented)
- **File**: `ReportsModule_Schema.sql` (Future)

### 10. Admin Module (`Admin` Schema)
- **Purpose**: System administration
- **Tables**: (To be implemented)
- **File**: `AdminModule_Schema.sql` (Future)

## Deployment Instructions

### Option 1: Deploy All Modules
```sql
-- Run the master deployment script
:r "DeployModuleSchemas.sql"
```

### Option 2: Deploy Individual Modules
```sql
-- 1. Create schemas first
:r "CreateAllModuleSchemas.sql"

-- 2. Deploy specific modules
:r "Schemas\CoreModule_Schema.sql"
:r "Schemas\HRMModule_Schema.sql"
:r "Schemas\FinanceModule_Schema.sql"
:r "Schemas\InventoryModule_Schema.sql"
:r "Schemas\CRMModule_Schema.sql"
```

### Option 3: Using SQL Server Management Studio
1. Open each schema file in SSMS
2. Execute them in the following order:
   - `CreateAllModuleSchemas.sql`
   - `CoreModule_Schema.sql`
   - `HRMModule_Schema.sql`
   - `FinanceModule_Schema.sql`
   - `InventoryModule_Schema.sql`
   - `CRMModule_Schema.sql`

## Schema Benefits

### 1. **Modularity**
- Each module is self-contained
- Easy to maintain and update
- Clear separation of concerns

### 2. **Scalability**
- Modules can be deployed independently
- Easy to add new modules
- Supports microservices architecture

### 3. **Security**
- Schema-level permissions
- Module-specific access control
- Data isolation

### 4. **Performance**
- Module-specific indexes
- Optimized queries per module
- Reduced cross-module dependencies

### 5. **Multi-tenancy**
- All tables include `TenantId`
- Tenant-specific data isolation
- Scalable for multiple tenants

## Table Naming Convention

- **Schema Prefix**: Each table is prefixed with its schema name
- **Pascal Case**: Table names use PascalCase
- **Descriptive**: Names clearly indicate the table purpose
- **Consistent**: Similar tables across modules follow the same pattern

## Indexing Strategy

Each module includes optimized indexes for:
- Primary keys
- Foreign keys
- Frequently queried columns
- Tenant-specific queries
- Performance-critical lookups

## Future Enhancements

1. **Additional Modules**: Manufacturing, Procurement, Projects, Reports, Admin
2. **Cross-Module Views**: Views that span multiple modules
3. **Module Dependencies**: Define inter-module relationships
4. **Data Archiving**: Archive strategies per module
5. **Audit Trails**: Module-specific audit logging

## Maintenance

- **Regular Updates**: Keep schemas updated with application changes
- **Performance Monitoring**: Monitor query performance per module
- **Backup Strategy**: Module-specific backup strategies
- **Documentation**: Keep this README updated with changes