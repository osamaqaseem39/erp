-- HRM.Departments + optional FK from Employees.DepartmentId
IF NOT EXISTS (SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'HRM' AND t.name = N'Departments')
BEGIN
    CREATE TABLE [HRM].[Departments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CompanyId] UNIQUEIDENTIFIER NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Code] NVARCHAR(50) NULL,
        [Description] NVARCHAR(1000) NULL,
        [ManagerId] UNIQUEIDENTIFIER NULL,
        [ParentDepartmentId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_Departments_Manager FOREIGN KEY ([ManagerId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE NO ACTION,
        CONSTRAINT FK_Departments_Parent FOREIGN KEY ([ParentDepartmentId]) REFERENCES [HRM].[Departments]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX IX_Departments_TenantId ON [HRM].[Departments]([TenantId]);
    CREATE INDEX IX_Departments_ParentDepartmentId ON [HRM].[Departments]([ParentDepartmentId]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Employees_Department')
BEGIN
    ALTER TABLE [HRM].[Employees] WITH NOCHECK
    ADD CONSTRAINT FK_Employees_Department
        FOREIGN KEY ([DepartmentId]) REFERENCES [HRM].[Departments]([Id]) ON DELETE NO ACTION;
END
GO
