-- Final script to remove ALL duplicate tables from module schemas
-- This will keep only dbo schema tables (used by Entity Framework)

USE [erp]
GO

SET NOCOUNT ON;
SET ANSI_WARNINGS OFF;

PRINT '=== Removing ALL duplicate tables from module schemas ===';
PRINT 'Keeping only dbo schema tables (Entity Framework compatible)';
PRINT '';

DECLARE @SQL NVARCHAR(MAX) = '';
DECLARE @TableName NVARCHAR(128);
DECLARE @SchemaName NVARCHAR(128);
DECLARE @DropCount INT = 0;

-- First, disable all foreign keys temporarily (we'll re-enable later if needed)
-- But first, let's just drop the tables that exist in both schemas

-- Create a table to store tables to drop
IF OBJECT_ID('tempdb..#TablesToDrop') IS NOT NULL DROP TABLE #TablesToDrop;
CREATE TABLE #TablesToDrop (
    SchemaName NVARCHAR(128),
    TableName NVARCHAR(128)
);

-- Find all duplicate tables in module schemas
INSERT INTO #TablesToDrop (SchemaName, TableName)
SELECT DISTINCT
    t1.TABLE_SCHEMA,
    t1.TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES t1
WHERE t1.TABLE_TYPE = 'BASE TABLE'
    AND t1.TABLE_SCHEMA IN ('Core', 'CRM', 'Finance', 'HRM', 'Inventory', 'Procurement')
    AND EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLES t2 
        WHERE t2.TABLE_NAME = t1.TABLE_NAME 
        AND t2.TABLE_SCHEMA = 'dbo'
        AND t2.TABLE_TYPE = 'BASE TABLE'
    )
    AND t1.TABLE_NAME NOT LIKE '__EFMigrationsHistory%';

SELECT @DropCount = COUNT(*) FROM #TablesToDrop;
PRINT 'Found ' + CAST(@DropCount AS NVARCHAR(10)) + ' duplicate tables to remove';
PRINT '';

-- Drop each table
DECLARE drop_cursor CURSOR FOR
SELECT SchemaName, TableName FROM #TablesToDrop ORDER BY SchemaName, TableName;

OPEN drop_cursor;
FETCH NEXT FROM drop_cursor INTO @SchemaName, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'DROP TABLE IF EXISTS [' + @SchemaName + '].[' + @TableName + '];';
    
    BEGIN TRY
        EXEC sp_executesql @SQL;
        PRINT 'Dropped: [' + @SchemaName + '].[' + @TableName + ']';
    END TRY
    BEGIN CATCH
        PRINT 'ERROR dropping [' + @SchemaName + '].[' + @TableName + ']: ' + ERROR_MESSAGE();
    END CATCH
    
    FETCH NEXT FROM drop_cursor INTO @SchemaName, @TableName;
END

CLOSE drop_cursor;
DEALLOCATE drop_cursor;

DROP TABLE #TablesToDrop;

PRINT '';
PRINT '=== Cleanup completed ===';
PRINT '';

-- Verify no duplicates remain
DECLARE @RemainingCount INT;
SELECT @RemainingCount = COUNT(*)
FROM (
    SELECT TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
    GROUP BY TABLE_NAME
    HAVING COUNT(*) > 1
) AS duplicates;

IF @RemainingCount = 0
BEGIN
    PRINT 'SUCCESS: No duplicate tables remaining!';
END
ELSE
BEGIN
    PRINT 'WARNING: ' + CAST(@RemainingCount AS NVARCHAR(10)) + ' duplicate tables still remain.';
    PRINT 'Remaining duplicates:';
    
    SELECT 
        TABLE_NAME,
        STRING_AGG(TABLE_SCHEMA, ', ') AS Schemas
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
    GROUP BY TABLE_NAME
    HAVING COUNT(*) > 1
    ORDER BY TABLE_NAME;
END

GO


