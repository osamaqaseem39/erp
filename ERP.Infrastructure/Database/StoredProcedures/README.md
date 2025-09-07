# ERP Stored Procedures Documentation

## Overview
This directory contains all stored procedures for the Multi-tenant SaaS ERP System, organized by module for better maintainability and clarity.

## Module Structure

### 1. Core Module (`CoreModule_StoredProcedures.sql`)
**Purpose**: Core system functionality including tenant management, user management, and audit logging.

**Key Procedures**:
- `sp_GetTenantById` - Retrieve tenant information by ID
- `sp_GetAllTenants` - Get all active tenants
- `sp_GetUserById` - Get user details by ID
- `sp_GetUserByEmail` - Get user details by email
- `sp_CreateUser` - Create new user
- `sp_GetCompanyById` - Get company information
- `sp_CreateCompany` - Create new company
- `sp_CreateAuditLog` - Log system activities
- `sp_GetAuditLogs` - Retrieve audit logs with filtering

### 2. HRM Module (`HRMModule_StoredProcedures.sql`)
**Purpose**: Human Resource Management including employee management, payroll, leave management, and time tracking.

**Key Procedures**:
- `sp_GetEmployeeById` - Get employee details
- `sp_CreateEmployee` - Create new employee
- `sp_GetAllEmployees` - List all employees with pagination
- `sp_CreatePayroll` - Create payroll run
- `sp_AddPayrollLine` - Add employee to payroll
- `sp_GetLeaveRequests` - Get leave requests with filtering
- `sp_CreateLeaveRequest` - Create new leave request
- `sp_ApproveLeaveRequest` - Approve leave request
- `sp_ClockIn` - Employee clock in
- `sp_ClockOut` - Employee clock out

### 3. Inventory Module (`InventoryModule_StoredProcedures.sql`)
**Purpose**: Inventory management including warehouse management, item management, stock tracking, and transfers.

**Key Procedures**:
- `sp_GetWarehouseById` - Get warehouse details
- `sp_CreateWarehouse` - Create new warehouse
- `sp_GetItemById` - Get item details
- `sp_CreateItem` - Create new item
- `sp_GetStockBalance` - Get current stock balance
- `sp_UpdateStockBalance` - Update stock quantities
- `sp_CreateStockTransfer` - Create stock transfer
- `sp_ProcessStockTransfer` - Process stock transfer
- `sp_CreateInventoryAdjustment` - Create inventory adjustment

### 4. Finance Module (`FinanceModule_StoredProcedures.sql`)
**Purpose**: Financial management including chart of accounts, journal entries, general ledger, and invoicing.

**Key Procedures**:
- `sp_GetChartOfAccounts` - Get chart of accounts
- `sp_CreateChartOfAccount` - Create new account
- `sp_CreateJournalEntry` - Create journal entry
- `sp_AddJournalEntryLine` - Add line to journal entry
- `sp_PostJournalEntry` - Post journal entry
- `sp_GetGeneralLedger` - Get general ledger entries
- `sp_GetTrialBalance` - Generate trial balance
- `sp_CreateInvoice` - Create invoice
- `sp_AddInvoiceLine` - Add line to invoice
- `sp_PostInvoice` - Post invoice
- `sp_RecordPayment` - Record payment

### 5. CRM Module (`CRMModule_StoredProcedures.sql`)
**Purpose**: Customer Relationship Management including customer management, opportunity tracking, and quote management.

**Key Procedures**:
- `sp_GetCustomerById` - Get customer details
- `sp_CreateCustomer` - Create new customer
- `sp_GetAllCustomers` - List all customers
- `sp_GetOpportunityById` - Get opportunity details
- `sp_CreateOpportunity` - Create new opportunity
- `sp_UpdateOpportunityStage` - Update opportunity stage
- `sp_CloseOpportunity` - Close opportunity
- `sp_GetActivities` - Get activities/tasks
- `sp_CreateActivity` - Create new activity
- `sp_CreateQuote` - Create quote
- `sp_AddQuoteLine` - Add line to quote
- `sp_ConvertQuoteToInvoice` - Convert quote to invoice

### 6. All Modules (`AllModules_StoredProcedures.sql`)
**Purpose**: Contains all procedures from all modules in a single file for easy deployment.

### 7. Deployment Script (`DeployAllStoredProcedures.sql`)
**Purpose**: Master deployment script that executes all module-specific stored procedures in the correct order.

## Naming Convention

All stored procedures follow the pattern: `sp_[Action][Entity]`

**Examples**:
- `sp_GetUserById` - Get user by ID
- `sp_CreateCustomer` - Create new customer
- `sp_UpdateStockBalance` - Update stock balance
- `sp_DeleteEmployee` - Delete employee (soft delete)

## Common Parameters

Most procedures include these standard parameters:
- `@TenantId` - Multi-tenant isolation
- `@CompanyId` - Company-specific data
- `@CreatedBy` - Audit trail for creation
- `@UpdatedBy` - Audit trail for updates

## Pagination Support

List procedures support pagination with:
- `@PageNumber` - Page number (default: 1)
- `@PageSize` - Records per page (default: 50)

## Error Handling

All procedures include:
- `SET NOCOUNT ON` for performance
- Proper error handling with try-catch blocks
- Return codes for success/failure indication

## Performance Considerations

- All procedures use parameterized queries to prevent SQL injection
- Proper indexing on frequently queried columns
- Pagination for large result sets
- Efficient joins and WHERE clauses

## Security Features

- Multi-tenant isolation enforced in all procedures
- Audit logging for all data modifications
- Soft deletes to maintain data integrity
- Role-based access control integration

## Usage Examples

### Get User by Email
```sql
EXEC sp_GetUserByEmail @Email = 'user@example.com', @TenantId = '12345678-1234-1234-1234-123456789012'
```

### Create New Customer
```sql
EXEC sp_CreateCustomer 
    @Id = NEWID(),
    @TenantId = '12345678-1234-1234-1234-123456789012',
    @CompanyId = '87654321-4321-4321-4321-210987654321',
    @CustomerCode = 'CUST001',
    @CustomerName = 'ABC Company',
    @CustomerType = 'Business',
    @Email = 'contact@abccompany.com',
    @CreatedBy = 'admin@system.com'
```

### Update Stock Balance
```sql
EXEC sp_UpdateStockBalance 
    @ItemId = 'item-guid',
    @WarehouseId = 'warehouse-guid',
    @TenantId = 'tenant-guid',
    @QuantityChange = 100,
    @TransactionType = 'Receipt',
    @ReferenceDocType = 'PurchaseOrder',
    @ReferenceDocId = 'po-guid',
    @CreatedBy = 'user@system.com'
```

## Maintenance

### Regular Maintenance Tasks
1. **Cleanup Expired Tokens**: Run `sp_CleanupExpiredTokens` daily
2. **Archive Old Audit Logs**: Run `sp_ArchiveOldAuditLogs` monthly
3. **Update Statistics**: Run `sp_UpdateStatistics` weekly
4. **Backup Database**: Run `sp_BackupDatabase` as needed

### Monitoring
- Use `sp_GetSystemHealth` to check system status
- Use `sp_GetDatabaseInfo` to get database statistics
- Use `sp_GetSystemStatistics` to get business metrics

## Deployment

1. **Single Module**: Execute the specific module file
2. **All Modules**: Execute `DeployAllStoredProcedures.sql`
3. **Verification**: Check the deployment output for any errors

## Troubleshooting

### Common Issues
1. **Missing Procedures**: Check if all module files were executed
2. **Permission Errors**: Ensure the database user has CREATE PROCEDURE permissions
3. **Syntax Errors**: Check SQL Server version compatibility
4. **Performance Issues**: Verify indexes are created properly

### Support
For issues or questions regarding stored procedures, contact the development team or refer to the system documentation.