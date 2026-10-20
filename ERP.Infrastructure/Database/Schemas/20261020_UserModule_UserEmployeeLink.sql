-- User module U5: optional User → Employee link
-- Idempotent for SQL Server.

IF COL_LENGTH('Core.Users', 'EmployeeId') IS NULL
    ALTER TABLE [Core].[Users] ADD [EmployeeId] UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Users_Employees_EmployeeId'
)
BEGIN
    -- Employees live in dbo (default EF table); only add FK when that table exists.
    IF OBJECT_ID(N'[dbo].[Employees]', N'U') IS NOT NULL
    BEGIN
        ALTER TABLE [Core].[Users]
            ADD CONSTRAINT [FK_Users_Employees_EmployeeId]
            FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]);
    END
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_Users_EmployeeId' AND object_id = OBJECT_ID(N'[Core].[Users]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_Users_EmployeeId]
        ON [Core].[Users]([EmployeeId])
        WHERE [EmployeeId] IS NOT NULL;
END
GO
