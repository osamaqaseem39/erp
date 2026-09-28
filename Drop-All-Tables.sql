-- Script to drop all tables in the database
-- This will drop all foreign keys first, then all tables

USE [erpnew]
GO

SET NOCOUNT ON;

PRINT 'Starting to drop all tables...';
PRINT '';

DECLARE @SQL NVARCHAR(MAX) = '';
DECLARE @TableName NVARCHAR(128);
DECLARE @SchemaName NVARCHAR(128);
DECLARE @FKName NVARCHAR(128);
DECLARE @Count INT = 0;

-- Step 1: Drop all foreign key constraints
PRINT 'Step 1: Dropping all foreign key constraints...';

DECLARE fk_cursor CURSOR FOR
SELECT DISTINCT
    SCHEMA_NAME(parent_object_id) AS FK_Schema,
    OBJECT_NAME(parent_object_id) AS FK_Table,
    name AS FK_Name
FROM sys.foreign_keys;

OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @SchemaName, @TableName, @FKName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] DROP CONSTRAINT [' + @FKName + '];';
    BEGIN TRY
        EXEC sp_executesql @SQL;
        SET @Count = @Count + 1;
    END TRY
    BEGIN CATCH
        PRINT 'Failed to drop FK: [' + @SchemaName + '].[' + @TableName + '].[' + @FKName + '] - ' + ERROR_MESSAGE();
    END CATCH
    FETCH NEXT FROM fk_cursor INTO @SchemaName, @TableName, @FKName;
END

CLOSE fk_cursor;
DEALLOCATE fk_cursor;

PRINT 'Dropped ' + CAST(@Count AS NVARCHAR(10)) + ' foreign key constraints.';
PRINT '';

-- Step 2: Drop all tables
PRINT 'Step 2: Dropping all tables...';
SET @Count = 0;

DECLARE table_cursor CURSOR FOR
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;

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
PRINT 'Dropped ' + CAST(@Count AS NVARCHAR(10)) + ' tables.';
PRINT '';
PRINT 'All tables dropped successfully!';

GO

