-- Customer depth: party enrich, commercial columns, packaging masters & documents
IF SCHEMA_ID('CRM') IS NULL EXEC('CREATE SCHEMA CRM');
GO

-- Customers (dbo)
IF COL_LENGTH('dbo.Customers', 'LocalName') IS NULL AND OBJECT_ID('dbo.Customers', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Customers ADD LocalName nvarchar(200) NULL;
    ALTER TABLE dbo.Customers ADD Phone2 nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD Cell nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD Cell2 nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD Fax nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD CityId uniqueidentifier NULL;
    ALTER TABLE dbo.Customers ADD PaymentTermId uniqueidentifier NULL;
    ALTER TABLE dbo.Customers ADD RegistryNo nvarchar(100) NULL;
    ALTER TABLE dbo.Customers ADD Rating int NULL;
    ALTER TABLE dbo.Customers ADD ReceivableAccountId uniqueidentifier NULL;
    ALTER TABLE dbo.Customers ADD PortalUserId uniqueidentifier NULL;
END
GO

-- Quotes / SalesOrders commercial headers
IF COL_LENGTH('dbo.Quotes', 'ReferenceNo') IS NULL AND OBJECT_ID('dbo.Quotes', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Quotes ADD ReferenceNo nvarchar(100) NULL;
    ALTER TABLE dbo.Quotes ADD ReferenceDate datetime2 NULL;
    ALTER TABLE dbo.Quotes ADD CreditDays int NOT NULL CONSTRAINT DF_Quotes_CreditDays DEFAULT (0);
    ALTER TABLE dbo.Quotes ADD PaymentMode nvarchar(50) NULL;
    ALTER TABLE dbo.Quotes ADD PaymentTermId uniqueidentifier NULL;
    ALTER TABLE dbo.Quotes ADD Freight decimal(18,2) NOT NULL CONSTRAINT DF_Quotes_Freight DEFAULT (0);
    ALTER TABLE dbo.Quotes ADD Overhead decimal(18,2) NOT NULL CONSTRAINT DF_Quotes_Overhead DEFAULT (0);
    ALTER TABLE dbo.Quotes ADD DiscountType nvarchar(20) NULL;
    ALTER TABLE dbo.Quotes ADD DiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_Quotes_DiscountAmount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.QuoteLines', 'PackQty') IS NULL AND OBJECT_ID('dbo.QuoteLines', 'U') IS NOT NULL
    ALTER TABLE dbo.QuoteLines ADD PackQty decimal(18,4) NOT NULL CONSTRAINT DF_QuoteLines_PackQty DEFAULT (0);
GO

IF COL_LENGTH('dbo.SalesOrders', 'ReferenceNo') IS NULL AND OBJECT_ID('dbo.SalesOrders', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesOrders ADD ReferenceNo nvarchar(100) NULL;
    ALTER TABLE dbo.SalesOrders ADD ReferenceDate datetime2 NULL;
    ALTER TABLE dbo.SalesOrders ADD CreditDays int NOT NULL CONSTRAINT DF_SalesOrders_CreditDays DEFAULT (0);
    ALTER TABLE dbo.SalesOrders ADD PaymentMode nvarchar(50) NULL;
    ALTER TABLE dbo.SalesOrders ADD PaymentTermId uniqueidentifier NULL;
    ALTER TABLE dbo.SalesOrders ADD Freight decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrders_Freight DEFAULT (0);
    ALTER TABLE dbo.SalesOrders ADD Overhead decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrders_Overhead DEFAULT (0);
    ALTER TABLE dbo.SalesOrders ADD DiscountType nvarchar(20) NULL;
    ALTER TABLE dbo.SalesOrders ADD DiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrders_DiscountAmount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.SalesOrderLines', 'PackQty') IS NULL AND OBJECT_ID('dbo.SalesOrderLines', 'U') IS NOT NULL
    ALTER TABLE dbo.SalesOrderLines ADD PackQty decimal(18,4) NOT NULL CONSTRAINT DF_SalesOrderLines_PackQty DEFAULT (0);
GO

-- Packaging setup masters
IF OBJECT_ID(N'[CRM].[PouchTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[PouchTypes] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PouchTypes_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_PouchTypes_Tenant_Code] ON [CRM].[PouchTypes]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[CRM].[ProductionProcesses]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[ProductionProcesses] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_ProductionProcesses_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_ProductionProcesses_Tenant_Code] ON [CRM].[ProductionProcesses]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[CRM].[FOHTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[FOHTypes] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Basis] NVARCHAR(50) NULL,
        [Rate] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_FOHTypes_Rate] DEFAULT (0),
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_FOHTypes_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_FOHTypes_Tenant_Code] ON [CRM].[FOHTypes]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[CRM].[SaleEnquiries]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[SaleEnquiries] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [EnquiryNumber] NVARCHAR(50) NOT NULL,
        [CustomerId] UNIQUEIDENTIFIER NULL,
        [EnquiryDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_SaleEnquiries_Status] DEFAULT ('Draft'),
        [ReferenceNo] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(2000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_SaleEnquiries_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_SaleEnquiries_Tenant_Status] ON [CRM].[SaleEnquiries]([TenantId], [Status]);
END
GO

IF OBJECT_ID(N'[CRM].[SaleEnquiryLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[SaleEnquiryLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SaleEnquiryId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [ItemDescription] NVARCHAR(500) NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [Size] NVARCHAR(100) NULL,
        [MaterialType] NVARCHAR(100) NULL,
        [PET] DECIMAL(18,4) NULL,
        [MPET] DECIMAL(18,4) NULL,
        [LaminationMicron] DECIMAL(18,4) NULL,
        [CutOffLength] DECIMAL(18,4) NULL,
        [FolderSize] DECIMAL(18,4) NULL,
        [Ups] INT NULL,
        [PouchTypeId] UNIQUEIDENTIFIER NULL,
        [ProcessId] UNIQUEIDENTIFIER NULL,
        [FOHTypeId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_SaleEnquiryLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_SaleEnquiryLines_Enquiry] FOREIGN KEY ([SaleEnquiryId]) REFERENCES [CRM].[SaleEnquiries]([Id]) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[CRM].[SalePreCosts]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[SalePreCosts] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PreCostNumber] NVARCHAR(50) NOT NULL,
        [SaleEnquiryId] UNIQUEIDENTIFIER NULL,
        [QuoteId] UNIQUEIDENTIFIER NULL,
        [CustomerId] UNIQUEIDENTIFIER NULL,
        [PreCostDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_SalePreCosts_Status] DEFAULT ('Draft'),
        [Notes] NVARCHAR(2000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_SalePreCosts_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_SalePreCosts_Tenant_Status] ON [CRM].[SalePreCosts]([TenantId], [Status]);
END
GO

IF OBJECT_ID(N'[CRM].[SalePreCostLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [CRM].[SalePreCostLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SalePreCostId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [ItemDescription] NVARCHAR(500) NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [MaterialCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalePreCostLines_MaterialCost] DEFAULT (0),
        [ProcessCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalePreCostLines_ProcessCost] DEFAULT (0),
        [FohCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalePreCostLines_FohCost] DEFAULT (0),
        [MarginPercent] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalePreCostLines_MarginPercent] DEFAULT (0),
        [UnitRate] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalePreCostLines_UnitRate] DEFAULT (0),
        [Size] NVARCHAR(100) NULL,
        [MaterialType] NVARCHAR(100) NULL,
        [PET] DECIMAL(18,4) NULL,
        [MPET] DECIMAL(18,4) NULL,
        [LaminationMicron] DECIMAL(18,4) NULL,
        [CutOffLength] DECIMAL(18,4) NULL,
        [FolderSize] DECIMAL(18,4) NULL,
        [Ups] INT NULL,
        [PouchTypeId] UNIQUEIDENTIFIER NULL,
        [ProcessId] UNIQUEIDENTIFIER NULL,
        [FOHTypeId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_SalePreCostLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_SalePreCostLines_PreCost] FOREIGN KEY ([SalePreCostId]) REFERENCES [CRM].[SalePreCosts]([Id]) ON DELETE CASCADE
    );
END
GO
