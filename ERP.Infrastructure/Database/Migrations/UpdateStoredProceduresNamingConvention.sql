-- =============================================
-- MIGRATION: Update Stored Procedures Naming Convention
-- Description: Updates all stored procedures to use module schemas and sp_ prefix
-- Date: 2025-12-01
-- =============================================

PRINT 'Starting migration: Update Stored Procedures Naming Convention...';
GO

-- =============================================
-- STEP 1: Ensure all module schemas exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Core')
BEGIN
    EXEC('CREATE SCHEMA [Core]');
    PRINT 'Created Core schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'CRM')
BEGIN
    EXEC('CREATE SCHEMA [CRM]');
    PRINT 'Created CRM schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'HRM')
BEGIN
    EXEC('CREATE SCHEMA [HRM]');
    PRINT 'Created HRM schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Finance')
BEGIN
    EXEC('CREATE SCHEMA [Finance]');
    PRINT 'Created Finance schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Inventory')
BEGIN
    EXEC('CREATE SCHEMA [Inventory]');
    PRINT 'Created Inventory schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Admin')
BEGIN
    EXEC('CREATE SCHEMA [Admin]');
    PRINT 'Created Admin schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Procurement')
BEGIN
    EXEC('CREATE SCHEMA [Procurement]');
    PRINT 'Created Procurement schema';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Manufacturing')
BEGIN
    EXEC('CREATE SCHEMA [Manufacturing]');
    PRINT 'Created Manufacturing schema';
END
GO

-- =============================================
-- STEP 2: Drop old stored procedures if they exist in dbo schema
-- =============================================
PRINT 'Dropping old stored procedures from dbo schema...';
GO

-- Core Module Procedures
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getUserByEmail]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[getUserByEmail];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getUserByEmailAndTenant]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[getUserByEmailAndTenant];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getUserById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[getUserById];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getAllUsers]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[getAllUsers];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[addUser]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[addUser];
GO

-- Admin Module Procedures
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_GetSystemLogById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_GetSystemLogById];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_GetSystemLogs]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_GetSystemLogs];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_CreateSystemLog]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_CreateSystemLog];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_GetSystemSettingByKey]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_GetSystemSettingByKey];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_GetAllSystemSettings]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_GetAllSystemSettings];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_UpsertSystemSetting]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_UpsertSystemSetting];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Admin_DeleteSystemSetting]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Admin_DeleteSystemSetting];
GO

PRINT 'Old stored procedures dropped successfully.';
GO

-- =============================================
-- STEP 3: Execute updated stored procedure scripts
-- =============================================
PRINT 'Executing updated stored procedure scripts...';
GO

-- Note: The actual stored procedure definitions are in the separate .sql files
-- This migration script assumes those files will be executed separately
-- or you can use SQLCMD :r command to include them here

PRINT 'Migration completed successfully!';
PRINT 'Please execute the following stored procedure files to complete the migration:';
PRINT '  1. CoreModule_StoredProcedures.sql';
PRINT '  2. CRMModule_StoredProcedures.sql';
PRINT '  3. HRMModule_StoredProcedures.sql';
PRINT '  4. FinanceModule_StoredProcedures.sql';
PRINT '  5. InventoryModule_StoredProcedures.sql';
PRINT '  6. AdminModule_StoredProcedures.sql';
PRINT '  7. ProcurementModule_StoredProcedures.sql';
PRINT '  8. ManufacturingModule_StoredProcedures.sql';
GO

-- =============================================
-- STEP 4: Verification - Check for any remaining dbo schema procedures
-- =============================================
PRINT 'Verifying migration...';
GO

DECLARE @RemainingProcs INT;
SELECT @RemainingProcs = COUNT(*)
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name = 'dbo'
AND p.name NOT LIKE 'sp_%'
AND p.name NOT IN ('sp_helpdiagrams', 'sp_upgraddiagrams', 'sp_creatediagram');

IF @RemainingProcs > 0
BEGIN
    PRINT 'WARNING: Found ' + CAST(@RemainingProcs AS VARCHAR(10)) + ' procedures in dbo schema that do not follow naming convention.';
    SELECT 
        'dbo.' + p.name AS ProcedureName,
        'Consider migrating to appropriate module schema' AS Recommendation
    FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'dbo'
    AND p.name NOT LIKE 'sp_%'
    AND p.name NOT IN ('sp_helpdiagrams', 'sp_upgraddiagrams', 'sp_creatediagram');
END
ELSE
BEGIN
    PRINT 'Verification passed: No procedures in dbo schema that violate naming convention.';
END
GO

-- =============================================
-- STEP 5: Summary Report
-- =============================================
PRINT '========================================';
PRINT 'MIGRATION SUMMARY';
PRINT '========================================';

SELECT 
    s.name AS SchemaName,
    COUNT(*) AS ProcedureCount
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name IN ('Core', 'CRM', 'HRM', 'Finance', 'Inventory', 'Admin', 'Procurement', 'Manufacturing')
GROUP BY s.name
ORDER BY s.name;

PRINT '========================================';
PRINT 'Migration script execution completed.';
PRINT '========================================';
GO

