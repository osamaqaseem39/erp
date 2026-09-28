-- Script to remove duplicate tables, keeping only dbo schema tables
-- Entity Framework uses dbo schema, so we'll keep those and remove module schema duplicates

USE [erp]
GO

SET NOCOUNT ON;

DECLARE @sql NVARCHAR(MAX);
DECLARE @TableName NVARCHAR(128);
DECLARE @SchemaName NVARCHAR(128);
DECLARE @RowCount INT;

-- Create a cursor to iterate through duplicate tables
DECLARE duplicate_cursor CURSOR FOR
SELECT 
    t.TABLE_SCHEMA,
    t.TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES t
WHERE t.TABLE_TYPE = 'BASE TABLE'
    AND t.TABLE_SCHEMA != 'dbo'
    AND t.TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
    AND EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLES t2 
        WHERE t2.TABLE_NAME = t.TABLE_NAME 
        AND t2.TABLE_SCHEMA = 'dbo'
        AND t2.TABLE_TYPE = 'BASE TABLE'
    )
ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME;

OPEN duplicate_cursor;
FETCH NEXT FROM duplicate_cursor INTO @SchemaName, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check row count in module schema table
    SET @sql = N'SELECT @Count = COUNT(*) FROM [' + @SchemaName + '].[' + @TableName + ']';
    EXEC sp_executesql @sql, N'@Count INT OUTPUT', @Count = @RowCount OUTPUT;
    
    IF @RowCount = 0
    BEGIN
        -- Table is empty, safe to drop
        SET @sql = N'DROP TABLE [' + @SchemaName + '].[' + @TableName + ']';
        PRINT 'Dropping empty table: [' + @SchemaName + '].[' + @TableName + ']';
        EXEC sp_executesql @sql;
    END
    ELSE
    BEGIN
        -- Table has data - report it but don't drop
        PRINT 'WARNING: Table [' + @SchemaName + '].[' + @TableName + '] has ' + CAST(@RowCount AS NVARCHAR(10)) + ' rows. Skipping.';
    END
    
    FETCH NEXT FROM duplicate_cursor INTO @SchemaName, @TableName;
END

CLOSE duplicate_cursor;
DEALLOCATE duplicate_cursor;

PRINT '';
PRINT 'Duplicate table cleanup completed.';
PRINT 'Check the output above for any tables with data that were skipped.';
GO

-- Show remaining duplicate tables
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

