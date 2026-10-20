-- HRM.Benefits catalog + employee enrollments
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'HRM')
    EXEC(N'CREATE SCHEMA [HRM]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'HRM' AND t.name = N'Benefits')
BEGIN
    CREATE TABLE [HRM].[Benefits] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Type] NVARCHAR(50) NOT NULL CONSTRAINT DF_Benefits_Type DEFAULT (N'Health'),
        [Cost] DECIMAL(18,2) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_Benefits_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_Benefits_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );

    CREATE INDEX IX_Benefits_TenantId ON [HRM].[Benefits]([TenantId]);
    CREATE INDEX IX_Benefits_Tenant_Type ON [HRM].[Benefits]([TenantId], [Type]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'HRM' AND t.name = N'EmployeeBenefitEnrollments')
BEGIN
    CREATE TABLE [HRM].[EmployeeBenefitEnrollments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [BenefitId] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [EnrollmentDate] DATETIME2 NOT NULL,
        [UnenrollmentDate] DATETIME2 NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_EmpBenefit_Status DEFAULT (N'Active'),
        [Reason] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_EmpBenefit_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_EmpBenefit_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_EmpBenefit_Benefit FOREIGN KEY ([BenefitId])
            REFERENCES [HRM].[Benefits]([Id]) ON DELETE NO ACTION,
        CONSTRAINT FK_EmpBenefit_Employee FOREIGN KEY ([EmployeeId])
            REFERENCES [HRM].[Employees]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX IX_EmpBenefit_Tenant_Employee ON [HRM].[EmployeeBenefitEnrollments]([TenantId], [EmployeeId]);
    CREATE INDEX IX_EmpBenefit_Benefit ON [HRM].[EmployeeBenefitEnrollments]([BenefitId]);
END
GO
