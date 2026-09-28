# Missing Stored Procedures Modules

## Overview
This document lists the modules that currently do not have stored procedures implemented.

## Missing Modules

### 1. Manufacturing Module
**Status**: ❌ Missing  
**Entities**:
- `WorkCenter`
- `ProductionOrder`
- `ProductionOrderLine`
- `ProductionRun`

**Suggested Procedures**:
- `sp_GetWorkCenterById`
- `sp_GetAllWorkCenters`
- `sp_CreateWorkCenter`
- `sp_GetProductionOrderById`
- `sp_GetAllProductionOrders`
- `sp_CreateProductionOrder`
- `sp_UpdateProductionOrderStatus`
- `sp_GetProductionOrderLinesByOrderId`
- `sp_GetProductionRunById`
- `sp_CreateProductionRun`
- `sp_StartProductionRun`
- `sp_CompleteProductionRun`

**File**: `ManufacturingModule_StoredProcedures.sql` (to be created)

---

### 2. Projects Module
**Status**: ❌ Missing  
**Entities**:
- `Project`
- `ProjectTask`
- `ProjectResource`
- `ProjectDocument`
- `ProjectMilestone`

**Suggested Procedures**:
- `sp_GetProjectById`
- `sp_GetAllProjects`
- `sp_CreateProject`
- `sp_UpdateProject`
- `sp_GetProjectTasksByProjectId`
- `sp_CreateProjectTask`
- `sp_UpdateProjectTaskStatus`
- `sp_GetProjectResourcesByProjectId`
- `sp_AssignResourceToProject`
- `sp_GetProjectMilestonesByProjectId`
- `sp_CreateProjectMilestone`
- `sp_GetProjectDocumentsByProjectId`

**File**: `ProjectsModule_StoredProcedures.sql` (to be created)

---

### 3. Sales/POS Module
**Status**: ❌ Missing  
**Entities**:
- `POSSession`
- `POSTransaction`
- `POSTransactionLine`

**Note**: `SalesInvoice` and `SalesOrder` are covered in CRM Module.

**Suggested Procedures**:
- `sp_GetPOSSessionById`
- `sp_GetActivePOSSession`
- `sp_CreatePOSSession`
- `sp_ClosePOSSession`
- `sp_GetPOSTransactionById`
- `sp_GetPOSTransactionsBySessionId`
- `sp_CreatePOSTransaction`
- `sp_GetPOSTransactionLinesByTransactionId`
- `sp_ProcessPOSTransaction`

**File**: `SalesPOSModule_StoredProcedures.sql` (to be created)

---

### 4. Reports Module
**Status**: ❌ Missing  
**Entities**:
- `Report`
- `ReportExecution`
- `ReportSchedule`

**Suggested Procedures**:
- `sp_GetReportById`
- `sp_GetAllReports`
- `sp_CreateReport`
- `sp_UpdateReport`
- `sp_GetReportExecutionsByReportId`
- `sp_CreateReportExecution`
- `sp_GetReportSchedulesByReportId`
- `sp_CreateReportSchedule`
- `sp_UpdateReportSchedule`
- `sp_GetScheduledReports`

**File**: `ReportsModule_StoredProcedures.sql` (to be created)

---

### 5. Admin Module
**Status**: ❌ Missing  
**Entities**:
- `SystemLog`
- `SystemSetting`

**Suggested Procedures**:
- `sp_GetSystemLogById`
- `sp_GetSystemLogs`
- `sp_CreateSystemLog`
- `sp_GetSystemSettingByKey`
- `sp_GetAllSystemSettings`
- `sp_UpsertSystemSetting`
- `sp_DeleteSystemSetting`

**File**: `AdminModule_StoredProcedures.sql` (to be created)

---

## Implementation Priority

### High Priority
1. **Sales/POS Module** - Critical for point-of-sale operations
2. **Projects Module** - Important for project management functionality

### Medium Priority
3. **Manufacturing Module** - Needed for production management
4. **Reports Module** - Important for reporting and analytics

### Low Priority
5. **Admin Module** - System administration and logging

## Next Steps

1. Create stored procedure files for each missing module
2. Follow the naming conventions defined in `NAMING_CONVENTIONS.md`
3. Ensure multi-tenancy support (include `@TenantID` parameter)
4. Add error handling and validation
5. Update `Deploy-StoredProcedures.ps1` to include new modules
6. Update `STORED_PROCEDURES_SUMMARY.md` with new procedures

## Notes

- All procedures should follow the existing naming convention: `sp_[Action][Entity]`
- All procedures must include `@TenantID` parameter for multi-tenancy
- All procedures should include proper error handling
- Consider pagination for list procedures
- Include proper indexes in queries for performance

