-- Customer depth: Manufacturing routing, scrap, production order routing link
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Routings' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[Routings] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [RoutingNumber] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [Version] NVARCHAR(20) NOT NULL CONSTRAINT DF_Routing_Version DEFAULT (N'1'),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_Routing_Status DEFAULT (N'Draft'),
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_Routing_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_Routing_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_Routings_Tenant_Number ON [dbo].[Routings]([TenantId], [RoutingNumber]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'RoutingSteps' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[RoutingSteps] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [RoutingId] UNIQUEIDENTIFIER NOT NULL,
        [StepNumber] INT NOT NULL,
        [WorkCenterId] UNIQUEIDENTIFIER NULL,
        [OperationName] NVARCHAR(200) NOT NULL,
        [StandardTimeMinutes] DECIMAL(18,4) NOT NULL CONSTRAINT DF_RoutingStep_StdTime DEFAULT (0),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_RoutingStep_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_RoutingStep_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_RoutingSteps_Routing FOREIGN KEY ([RoutingId])
            REFERENCES [dbo].[Routings]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_RoutingSteps_Routing ON [dbo].[RoutingSteps]([RoutingId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ProductionScraps' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[ProductionScraps] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ProductionOrderId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [ScrapDate] DATETIME2 NOT NULL,
        [Reason] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ProdScrap_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ProdScrap_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_ProdScrap_Tenant_Order ON [dbo].[ProductionScraps]([TenantId], [ProductionOrderId]);
END
GO

IF COL_LENGTH(N'dbo.ProductionOrders', N'RoutingId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ProductionOrders] ADD [RoutingId] UNIQUEIDENTIFIER NULL;
END
GO
