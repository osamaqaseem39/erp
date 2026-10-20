-- Projects entity depth: members, milestone billing, billable timesheet lines,
-- finance dimensions, invoice ProjectId, document scoping.
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Projects')
    EXEC(N'CREATE SCHEMA [Projects]');
GO

-- ProjectMembers (team membership)
IF OBJECT_ID(N'[Projects].[ProjectMembers]', N'U') IS NULL
BEGIN
    CREATE TABLE [Projects].[ProjectMembers] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ProjectId] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [Role] NVARCHAR(100) NOT NULL CONSTRAINT DF_ProjMem_Role DEFAULT (N'Member'),
        [StartDate] DATE NULL,
        [EndDate] DATE NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ProjMem_Status DEFAULT (N'Active'),
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ProjMem_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ProjMem_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_ProjMem_Tenant_Project ON [Projects].[ProjectMembers]([TenantId], [ProjectId]);
    CREATE INDEX IX_ProjMem_Tenant_Employee ON [Projects].[ProjectMembers]([TenantId], [EmployeeId]);
END
GO

-- dbo.Projects finance dimensions
IF COL_LENGTH(N'dbo.Projects', N'CostCenterId') IS NULL
    ALTER TABLE [dbo].[Projects] ADD [CostCenterId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.Projects', N'CurrencyId') IS NULL
    ALTER TABLE [dbo].[Projects] ADD [CurrencyId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.Projects', N'BranchId') IS NULL
    ALTER TABLE [dbo].[Projects] ADD [BranchId] UNIQUEIDENTIFIER NULL;
GO

-- dbo.ProjectMilestones billing
IF COL_LENGTH(N'dbo.ProjectMilestones', N'Amount') IS NULL
    ALTER TABLE [dbo].[ProjectMilestones] ADD [Amount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_ProjMs_Amount DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.ProjectMilestones', N'IsBillable') IS NULL
    ALTER TABLE [dbo].[ProjectMilestones] ADD [IsBillable] BIT NOT NULL CONSTRAINT DF_ProjMs_IsBillable DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.ProjectMilestones', N'SalesInvoiceId') IS NULL
    ALTER TABLE [dbo].[ProjectMilestones] ADD [SalesInvoiceId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProjectMilestones', N'FinanceInvoiceId') IS NULL
    ALTER TABLE [dbo].[ProjectMilestones] ADD [FinanceInvoiceId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProjectMilestones', N'BilledAt') IS NULL
    ALTER TABLE [dbo].[ProjectMilestones] ADD [BilledAt] DATETIME2 NULL;
GO

-- Projects.ProjectTimesheetLines billable
IF COL_LENGTH(N'Projects.ProjectTimesheetLines', N'IsBillable') IS NULL
    ALTER TABLE [Projects].[ProjectTimesheetLines] ADD [IsBillable] BIT NOT NULL CONSTRAINT DF_ProjTsLine_IsBillable DEFAULT (1);
GO
IF COL_LENGTH(N'Projects.ProjectTimesheetLines', N'HourlyRate') IS NULL
    ALTER TABLE [Projects].[ProjectTimesheetLines] ADD [HourlyRate] DECIMAL(18,4) NOT NULL CONSTRAINT DF_ProjTsLine_HourlyRate DEFAULT (0);
GO

-- dbo.ProjectDocuments scoping
IF COL_LENGTH(N'dbo.ProjectDocuments', N'ProjectTaskId') IS NULL
    ALTER TABLE [dbo].[ProjectDocuments] ADD [ProjectTaskId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProjectDocuments', N'ProjectMilestoneId') IS NULL
    ALTER TABLE [dbo].[ProjectDocuments] ADD [ProjectMilestoneId] UNIQUEIDENTIFIER NULL;
GO

-- dbo.SalesInvoices project link
IF COL_LENGTH(N'dbo.SalesInvoices', N'ProjectId') IS NULL
    ALTER TABLE [dbo].[SalesInvoices] ADD [ProjectId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.SalesInvoices', N'ProjectMilestoneId') IS NULL
    ALTER TABLE [dbo].[SalesInvoices] ADD [ProjectMilestoneId] UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesInvoices_ProjectId' AND object_id = OBJECT_ID(N'dbo.SalesInvoices'))
    CREATE INDEX IX_SalesInvoices_ProjectId ON [dbo].[SalesInvoices]([ProjectId]) WHERE [ProjectId] IS NOT NULL;
GO

-- dbo.Invoices (Finance AR) project link
IF COL_LENGTH(N'dbo.Invoices', N'ProjectId') IS NULL
    ALTER TABLE [dbo].[Invoices] ADD [ProjectId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.Invoices', N'ProjectMilestoneId') IS NULL
    ALTER TABLE [dbo].[Invoices] ADD [ProjectMilestoneId] UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Invoices_ProjectId' AND object_id = OBJECT_ID(N'dbo.Invoices'))
    CREATE INDEX IX_Invoices_ProjectId ON [dbo].[Invoices]([ProjectId]) WHERE [ProjectId] IS NOT NULL;
GO

-- dbo.TimeEntries: ProjectTaskId (may already exist as shadow column)
IF COL_LENGTH(N'dbo.TimeEntries', N'ProjectTaskId') IS NULL
    ALTER TABLE [dbo].[TimeEntries] ADD [ProjectTaskId] UNIQUEIDENTIFIER NULL;
GO
