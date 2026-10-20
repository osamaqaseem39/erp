-- Customer depth: HRM employee advances and teams
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'HRM')
    EXEC(N'CREATE SCHEMA [HRM]');
GO

IF OBJECT_ID(N'[HRM].[EmployeeAdvances]', N'U') IS NULL
BEGIN
    CREATE TABLE [HRM].[EmployeeAdvances] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [ReferenceNo] NVARCHAR(50) NOT NULL,
        [AdvanceDate] DATE NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [RecoveredAmount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_EmpAdv_Recovered DEFAULT (0),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_EmpAdv_Status DEFAULT (N'Disbursed'),
        [Notes] NVARCHAR(1000) NULL,
        [DisbursedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_EmpAdv_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_EmpAdv_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_EmpAdv_Tenant_Ref ON [HRM].[EmployeeAdvances]([TenantId], [ReferenceNo]);
    CREATE INDEX IX_EmpAdv_Tenant_Employee ON [HRM].[EmployeeAdvances]([TenantId], [EmployeeId]);
END
GO

IF OBJECT_ID(N'[HRM].[Teams]', N'U') IS NULL
BEGIN
    CREATE TABLE [HRM].[Teams] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [ManagerEmployeeId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_Teams_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_Teams_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_Teams_Tenant ON [HRM].[Teams]([TenantId]);
END
GO
