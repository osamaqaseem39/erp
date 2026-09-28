# Stored Procedures Migration

This directory contains migration scripts to update stored procedures to follow the new naming conventions.

## Migration Overview

This migration updates all stored procedures to:
1. Use module schemas (Core, CRM, HRM, Finance, Inventory, Admin) instead of `dbo`
2. Ensure all procedures start with `sp_` prefix
3. Update all table references to use module schemas

## Files

- `UpdateStoredProceduresNamingConvention.sql` - Main migration script
- `DeployStoredProceduresMigration.ps1` - PowerShell deployment script
- `README.md` - This file

## Prerequisites

- SQL Server 2019 or later
- Appropriate permissions to create/modify stored procedures
- PowerShell 5.1 or later (for PowerShell script)

## Deployment Options

### Option 1: Using PowerShell Script (Recommended)

```powershell
# Using Integrated Security
.\DeployStoredProceduresMigration.ps1 -ServerInstance "localhost" -Database "ERP" -UseIntegratedSecurity

# Using SQL Authentication
.\DeployStoredProceduresMigration.ps1 -ServerInstance "localhost" -Database "ERP" -Username "sa" -Password "YourPassword" -UseIntegratedSecurity:$false
```

### Option 2: Manual SQL Execution

1. Execute the migration script:
   ```sql
   :r "UpdateStoredProceduresNamingConvention.sql"
   ```

2. Execute all stored procedure files in order:
   ```sql
   :r "../StoredProcedures/CoreModule_StoredProcedures.sql"
   :r "../StoredProcedures/CRMModule_StoredProcedures.sql"
   :r "../StoredProcedures/HRMModule_StoredProcedures.sql"
   :r "../StoredProcedures/FinanceModule_StoredProcedures.sql"
   :r "../StoredProcedures/InventoryModule_StoredProcedures.sql"
   :r "../StoredProcedures/AdminModule_StoredProcedures.sql"
   ```

### Option 3: Using SQL Server Management Studio (SSMS)

1. Open `UpdateStoredProceduresNamingConvention.sql` in SSMS
2. Execute the script
3. Open and execute each stored procedure file in the order listed above

## What the Migration Does

1. **Creates Module Schemas**: Ensures all required schemas (Core, CRM, HRM, Finance, Inventory, Admin) exist
2. **Drops Old Procedures**: Removes old procedures from `dbo` schema that don't follow naming conventions
3. **Creates New Procedures**: Executes updated stored procedure scripts with proper schema and naming
4. **Verification**: Checks for any remaining procedures that violate naming conventions

## Verification

After migration, verify the changes:

```sql
-- Check procedures by schema
SELECT 
    s.name AS SchemaName,
    p.name AS ProcedureName
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name IN ('Core', 'CRM', 'HRM', 'Finance', 'Inventory', 'Admin')
ORDER BY s.name, p.name;

-- Verify no procedures in dbo (except system procedures)
SELECT 
    p.name AS ProcedureName
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name = 'dbo'
AND p.name NOT LIKE 'sp_%'
AND p.name NOT IN ('sp_helpdiagrams', 'sp_upgraddiagrams', 'sp_creatediagram');
```

## Rollback

If you need to rollback, you would need to:
1. Restore the previous version of stored procedure files
2. Re-execute them

**Note**: This migration does not modify table structures or data, only stored procedures.

## Troubleshooting

### Error: Schema already exists
This is normal if schemas were already created. The script checks for existence before creating.

### Error: Procedure does not exist
This is normal if the old procedures were already removed. The script checks for existence before dropping.

### Permission Errors
Ensure the SQL user has:
- `CREATE SCHEMA` permission
- `CREATE PROCEDURE` permission
- `ALTER` permission on the database

## Support

For issues or questions, please refer to the main project documentation or contact the development team.

