-- Customer depth: Sales invoice commercial fields + dispatch advice
IF COL_LENGTH('dbo.SalesInvoices', 'ReferenceNo') IS NULL AND OBJECT_ID('dbo.SalesInvoices', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesInvoices ADD ReferenceNo nvarchar(100) NULL;
    ALTER TABLE dbo.SalesInvoices ADD ReferenceDate datetime2 NULL;
    ALTER TABLE dbo.SalesInvoices ADD CreditDays int NOT NULL CONSTRAINT DF_SalesInvoices_CreditDays DEFAULT (0);
    ALTER TABLE dbo.SalesInvoices ADD PaymentMode nvarchar(50) NULL;
    ALTER TABLE dbo.SalesInvoices ADD PaymentTermId uniqueidentifier NULL;
    ALTER TABLE dbo.SalesInvoices ADD Freight decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoices_Freight DEFAULT (0);
    ALTER TABLE dbo.SalesInvoices ADD Overhead decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoices_Overhead DEFAULT (0);
    ALTER TABLE dbo.SalesInvoices ADD DiscountType nvarchar(20) NULL;
    ALTER TABLE dbo.SalesInvoices ADD DiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoices_DiscountAmount DEFAULT (0);
    ALTER TABLE dbo.SalesInvoices ADD DispatchAdviceId uniqueidentifier NULL;
END
GO

IF OBJECT_ID(N'[dbo].[DispatchAdvices]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DispatchAdvices] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [AdviceNumber] NVARCHAR(50) NOT NULL,
        [SalesOrderId] UNIQUEIDENTIFIER NULL,
        [CustomerId] UNIQUEIDENTIFIER NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NULL,
        [AdviceDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_DispatchAdvices_Status] DEFAULT ('Draft'),
        [DispatchedAt] DATETIME2 NULL,
        [Notes] NVARCHAR(2000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_DispatchAdvices_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_DispatchAdvices_Tenant_Status] ON [dbo].[DispatchAdvices]([TenantId], [Status]);
END
GO

IF OBJECT_ID(N'[dbo].[DispatchAdviceLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DispatchAdviceLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [DispatchAdviceId] UNIQUEIDENTIFIER NOT NULL,
        [SalesOrderLineId] UNIQUEIDENTIFIER NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_DispatchAdviceLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_DispatchAdviceLines_Advice] FOREIGN KEY ([DispatchAdviceId]) REFERENCES [dbo].[DispatchAdvices]([Id]) ON DELETE CASCADE
    );
END
GO
