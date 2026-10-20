-- Customer depth: Project timesheets
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Projects')
    EXEC(N'CREATE SCHEMA [Projects]');
GO

IF OBJECT_ID(N'[Projects].[ProjectTimesheets]', N'U') IS NULL
BEGIN
    CREATE TABLE [Projects].[ProjectTimesheets] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ProjectId] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [PeriodStart] DATE NOT NULL,
        [PeriodEnd] DATE NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ProjTs_Status DEFAULT (N'Draft'),
        [TotalHours] DECIMAL(18,4) NOT NULL CONSTRAINT DF_ProjTs_Total DEFAULT (0),
        [SubmittedAt] DATETIME2 NULL,
        [ApprovedAt] DATETIME2 NULL,
        [ApprovedBy] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ProjTs_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ProjTs_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_ProjTs_Tenant_Project ON [Projects].[ProjectTimesheets]([TenantId], [ProjectId]);
END
GO

IF OBJECT_ID(N'[Projects].[ProjectTimesheetLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Projects].[ProjectTimesheetLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ProjectTimesheetId] UNIQUEIDENTIFIER NOT NULL,
        [LineNumber] INT NOT NULL,
        [WorkDate] DATE NOT NULL,
        [ProjectTaskId] UNIQUEIDENTIFIER NULL,
        [Hours] DECIMAL(18,4) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ProjTsLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ProjTsLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_ProjTsLines_Header FOREIGN KEY ([ProjectTimesheetId])
            REFERENCES [Projects].[ProjectTimesheets]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProjTsLines_Header ON [Projects].[ProjectTimesheetLines]([ProjectTimesheetId]);
END
GO
