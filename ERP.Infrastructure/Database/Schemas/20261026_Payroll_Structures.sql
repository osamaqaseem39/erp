-- Payroll: salary components, employee salary structures, payslip breakdown
-- Idempotent IF NOT EXISTS / COL_LENGTH checks

IF COL_LENGTH(N'dbo.PayrollComponents', N'Code') IS NULL
    ALTER TABLE [dbo].[PayrollComponents] ADD [Code] NVARCHAR(30) NULL;
GO
IF COL_LENGTH(N'dbo.PayrollComponents', N'DisplayOrder') IS NULL
    ALTER TABLE [dbo].[PayrollComponents] ADD [DisplayOrder] INT NOT NULL CONSTRAINT [DF_PayrollComponents_DisplayOrder] DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.PayrollComponents', N'Description') IS NULL
    ALTER TABLE [dbo].[PayrollComponents] ADD [Description] NVARCHAR(500) NULL;
GO

IF COL_LENGTH(N'dbo.PayrollLines', N'BreakdownJson') IS NULL
    ALTER TABLE [dbo].[PayrollLines] ADD [BreakdownJson] NVARCHAR(MAX) NULL;
GO

IF OBJECT_ID(N'[dbo].[EmployeeSalaryComponents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EmployeeSalaryComponents] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [PayrollComponentId] UNIQUEIDENTIFIER NOT NULL,
        [Value] DECIMAL(18,6) NULL,
        [EffectiveFrom] DATETIME2 NULL,
        [EffectiveTo] DATETIME2 NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_EmployeeSalaryComponents_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_EmployeeSalaryComponents_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(MAX) NULL,
        [UpdatedBy] NVARCHAR(MAX) NULL,
        CONSTRAINT [FK_EmployeeSalaryComponents_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EmployeeSalaryComponents_PayrollComponents_PayrollComponentId] FOREIGN KEY ([PayrollComponentId]) REFERENCES [dbo].[PayrollComponents]([Id])
    );
    CREATE INDEX [IX_EmployeeSalaryComponents_TenantId_EmployeeId] ON [dbo].[EmployeeSalaryComponents]([TenantId], [EmployeeId]);
    CREATE INDEX [IX_EmployeeSalaryComponents_PayrollComponentId] ON [dbo].[EmployeeSalaryComponents]([PayrollComponentId]);
END
GO
