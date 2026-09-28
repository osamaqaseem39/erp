-- Complete script to remove ALL duplicate tables
-- Handles foreign key constraints properly

USE [erp]
GO

SET NOCOUNT ON;

PRINT 'Starting duplicate table removal...';
PRINT '';

DECLARE @SQL NVARCHAR(MAX);
DECLARE @TableName NVARCHAR(128);
DECLARE @SchemaName NVARCHAR(128);
DECLARE @FKName NVARCHAR(128);
DECLARE @Count INT = 0;

-- Step 1: Drop ALL foreign key constraints that reference module schema tables
-- This includes:
--   1. FKs defined ON module schema tables (pointing anywhere)
--   2. FKs in ANY schema that point TO module schema tables we want to drop
PRINT 'Step 1: Dropping foreign key constraints...';

-- First, drop FKs defined ON module schema tables
DECLARE fk_cursor CURSOR FOR
SELECT DISTINCT
    SCHEMA_NAME(parent_object_id) AS FK_Schema,
    OBJECT_NAME(parent_object_id) AS FK_Table,
    name AS FK_Name
FROM sys.foreign_keys
WHERE SCHEMA_NAME(parent_object_id) IN ('Core', 'CRM', 'Finance', 'HRM', 'Inventory', 'Procurement')
    AND EXISTS (
        -- The table must have a duplicate in dbo
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLES t
        WHERE t.TABLE_NAME = OBJECT_NAME(parent_object_id)
        AND t.TABLE_SCHEMA = SCHEMA_NAME(parent_object_id)
        AND t.TABLE_TYPE = 'BASE TABLE'
        AND EXISTS (
            SELECT 1 
            FROM INFORMATION_SCHEMA.TABLES t_dbo 
            WHERE t_dbo.TABLE_NAME = t.TABLE_NAME 
            AND t_dbo.TABLE_SCHEMA = 'dbo'
            AND t_dbo.TABLE_TYPE = 'BASE TABLE'
        )
    );

OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @SchemaName, @TableName, @FKName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] DROP CONSTRAINT [' + @FKName + '];';
    BEGIN TRY
        EXEC sp_executesql @SQL;
        SET @Count = @Count + 1;
        PRINT 'Dropped FK: [' + @SchemaName + '].[' + @TableName + '].[' + @FKName + ']';
    END TRY
    BEGIN CATCH
        PRINT 'Failed to drop FK: [' + @SchemaName + '].[' + @TableName + '].[' + @FKName + '] - ' + ERROR_MESSAGE();
    END CATCH
    FETCH NEXT FROM fk_cursor INTO @SchemaName, @TableName, @FKName;
END

CLOSE fk_cursor;
DEALLOCATE fk_cursor;

-- Second, drop FKs in ANY schema that point TO module schema tables we want to drop
DECLARE fk_cursor2 CURSOR FOR
SELECT DISTINCT
    SCHEMA_NAME(parent_object_id) AS FK_Schema,
    OBJECT_NAME(parent_object_id) AS FK_Table,
    name AS FK_Name
FROM sys.foreign_keys fk
WHERE SCHEMA_NAME(referenced_object_id) IN ('Core', 'CRM', 'Finance', 'HRM', 'Inventory', 'Procurement')
    AND EXISTS (
        -- The referenced table must have a duplicate in dbo
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLES t
        WHERE t.TABLE_NAME = OBJECT_NAME(referenced_object_id)
        AND t.TABLE_SCHEMA = SCHEMA_NAME(referenced_object_id)
        AND t.TABLE_TYPE = 'BASE TABLE'
        AND EXISTS (
            SELECT 1 
            FROM INFORMATION_SCHEMA.TABLES t_dbo 
            WHERE t_dbo.TABLE_NAME = t.TABLE_NAME 
            AND t_dbo.TABLE_SCHEMA = 'dbo'
            AND t_dbo.TABLE_TYPE = 'BASE TABLE'
        )
    );

OPEN fk_cursor2;
FETCH NEXT FROM fk_cursor2 INTO @SchemaName, @TableName, @FKName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] DROP CONSTRAINT [' + @FKName + '];';
    BEGIN TRY
        EXEC sp_executesql @SQL;
        SET @Count = @Count + 1;
        PRINT 'Dropped FK: [' + @SchemaName + '].[' + @TableName + '].[' + @FKName + ']';
    END TRY
    BEGIN CATCH
        PRINT 'Failed to drop FK: [' + @SchemaName + '].[' + @TableName + '].[' + @FKName + '] - ' + ERROR_MESSAGE();
    END CATCH
    FETCH NEXT FROM fk_cursor2 INTO @SchemaName, @TableName, @FKName;
END

CLOSE fk_cursor2;
DEALLOCATE fk_cursor2;

PRINT 'Dropped ' + CAST(@Count AS NVARCHAR(10)) + ' foreign key constraints.';
PRINT '';

-- Step 2: Drop all duplicate tables from module schemas
PRINT 'Step 2: Dropping duplicate tables...';
SET @Count = 0;

DECLARE table_cursor CURSOR FOR
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
    AND t1.TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
ORDER BY t1.TABLE_SCHEMA, t1.TABLE_NAME;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @SchemaName, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'DROP TABLE [' + @SchemaName + '].[' + @TableName + '];';
    BEGIN TRY
        EXEC sp_executesql @SQL;
        SET @Count = @Count + 1;
        PRINT 'Dropped: [' + @SchemaName + '].[' + @TableName + ']';
    END TRY
    BEGIN CATCH
        PRINT 'Failed: [' + @SchemaName + '].[' + @TableName + '] - ' + ERROR_MESSAGE();
    END CATCH
    FETCH NEXT FROM table_cursor INTO @SchemaName, @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

PRINT '';
PRINT 'Dropped ' + CAST(@Count AS NVARCHAR(10)) + ' duplicate tables.';
PRINT '';

-- Step 3: Verify
PRINT 'Step 3: Verification...';
DECLARE @Remaining INT;
SELECT @Remaining = COUNT(*)
FROM (
    SELECT TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME NOT LIKE '__EFMigrationsHistory%'
    GROUP BY TABLE_NAME
    HAVING COUNT(*) > 1
) AS duplicates;

IF @Remaining = 0
BEGIN
    PRINT 'SUCCESS: All duplicate tables removed!';
END
ELSE
BEGIN
    PRINT 'WARNING: ' + CAST(@Remaining AS NVARCHAR(10)) + ' duplicate tables still remain.';
END

GO


