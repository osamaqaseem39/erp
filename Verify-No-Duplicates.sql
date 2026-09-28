-- Verify and remove all duplicate tables from module schemas
-- Keep only dbo schema tables since Entity Framework uses that

USE [erp]
GO

SET NOCOUNT ON;

PRINT '=== Removing duplicate tables from module schemas ===';
PRINT '';

-- Generate DROP statements for all duplicate tables in module schemas
-- that have a corresponding table in dbo schema
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @count INT = 0;

SELECT @sql = @sql + 
    'DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];' + CHAR(13) + CHAR(10)
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
    AND t1.TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
ORDER BY t1.TABLE_SCHEMA, t1.TABLE_NAME;

-- Count how many will be dropped
SELECT @count = COUNT(*)
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

PRINT 'Tables to be dropped: ' + CAST(@count AS NVARCHAR(10));
PRINT '';

-- Show the SQL that will be executed
IF @sql IS NOT NULL AND LEN(@sql) > 0
BEGIN
    -- Execute the DROP statements
    EXEC sp_executesql @sql;
    PRINT 'Successfully removed ' + CAST(@count AS NVARCHAR(10)) + ' duplicate tables.';
END
ELSE
BEGIN
    PRINT 'No duplicate tables found to remove.';
END

PRINT '';
PRINT '=== Remaining duplicate tables (if any) ===';

-- Check for any remaining duplicates
SELECT 
    TABLE_NAME,
    STRING_AGG(TABLE_SCHEMA, ', ') AS RemainingSchemas,
    COUNT(*) AS SchemaCount
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
    AND TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
GROUP BY TABLE_NAME
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC, TABLE_NAME;

GO

