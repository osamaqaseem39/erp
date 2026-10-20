-- Customer depth: SCM (inventory) — brands, bins, reservations, kits, cycle count
IF COL_LENGTH('dbo.Items', 'BrandId') IS NULL AND OBJECT_ID('dbo.Items', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Items ADD BrandId UNIQUEIDENTIFIER NULL;
END
GO

IF OBJECT_ID(N'[dbo].[Brands]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Brands] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Brands_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Brands_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_Brands_Tenant_Code] ON [dbo].[Brands]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[dbo].[WarehouseLocations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WarehouseLocations] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_WarehouseLocations_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_WarehouseLocations_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_WarehouseLocations_Tenant_Warehouse_Code]
        ON [dbo].[WarehouseLocations]([TenantId], [WarehouseId], [Code]) WHERE [IsActive] = 1;
    CREATE INDEX [IX_WarehouseLocations_WarehouseId] ON [dbo].[WarehouseLocations]([WarehouseId]);
END
GO

IF OBJECT_ID(N'[dbo].[StockReservations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StockReservations] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ReservationNumber] NVARCHAR(50) NOT NULL,
        [SalesOrderId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_StockReservations_Status] DEFAULT (N'Active'),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_StockReservations_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_StockReservations_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_StockReservations_TenantId] ON [dbo].[StockReservations]([TenantId]);
    CREATE INDEX [IX_StockReservations_SalesOrderId] ON [dbo].[StockReservations]([SalesOrderId]);
END
GO

IF OBJECT_ID(N'[dbo].[StockReservationLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StockReservationLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [StockReservationId] UNIQUEIDENTIFIER NOT NULL,
        [SalesOrderId] UNIQUEIDENTIFIER NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_StockReservationLines_Status] DEFAULT (N'Active'),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_StockReservationLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_StockReservationLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_StockReservationLines_Header]
            FOREIGN KEY ([StockReservationId]) REFERENCES [dbo].[StockReservations]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_StockReservationLines_Header] ON [dbo].[StockReservationLines]([StockReservationId]);
    CREATE INDEX [IX_StockReservationLines_Item_Warehouse] ON [dbo].[StockReservationLines]([ItemId], [WarehouseId]);
END
GO

IF OBJECT_ID(N'[dbo].[ItemKits]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ItemKits] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [KitItemId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_ItemKits_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ItemKits_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_ItemKits_Tenant_Code] ON [dbo].[ItemKits]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[dbo].[ItemKitComponents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ItemKitComponents] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ItemKitId] UNIQUEIDENTIFIER NOT NULL,
        [ComponentItemId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_ItemKitComponents_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ItemKitComponents_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_ItemKitComponents_Kit]
            FOREIGN KEY ([ItemKitId]) REFERENCES [dbo].[ItemKits]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ItemKitComponents_KitId] ON [dbo].[ItemKitComponents]([ItemKitId]);
END
GO

IF OBJECT_ID(N'[dbo].[CycleCounts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CycleCounts] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CountNumber] NVARCHAR(50) NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [CountDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_CycleCounts_Status] DEFAULT (N'Draft'),
        [Notes] NVARCHAR(1000) NULL,
        [PostedStockAdjustmentId] UNIQUEIDENTIFIER NULL,
        [PostedAt] DATETIME2 NULL,
        [PostedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CycleCounts_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_CycleCounts_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_CycleCounts_TenantId] ON [dbo].[CycleCounts]([TenantId]);
    CREATE INDEX [IX_CycleCounts_WarehouseId] ON [dbo].[CycleCounts]([WarehouseId]);
END
GO

IF OBJECT_ID(N'[dbo].[CycleCountLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CycleCountLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CycleCountId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [WarehouseLocationId] UNIQUEIDENTIFIER NULL,
        [QuantityOnHand] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_CycleCountLines_OnHand] DEFAULT (0),
        [QuantityCounted] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_CycleCountLines_Counted] DEFAULT (0),
        [QuantityVariance] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_CycleCountLines_Variance] DEFAULT (0),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CycleCountLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_CycleCountLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_CycleCountLines_Header]
            FOREIGN KEY ([CycleCountId]) REFERENCES [dbo].[CycleCounts]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CycleCountLines_Header] ON [dbo].[CycleCountLines]([CycleCountId]);
END
GO
