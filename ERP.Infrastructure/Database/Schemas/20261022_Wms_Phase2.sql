-- WMS Phase 2: transfer locations, replenishment, pack, shipment
-- Idempotent IF NOT EXISTS / COL_LENGTH checks

IF COL_LENGTH(N'dbo.StockTransferDetails', N'FromLocationId') IS NULL
    ALTER TABLE [dbo].[StockTransferDetails] ADD [FromLocationId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.StockTransferDetails', N'ToLocationId') IS NULL
    ALTER TABLE [dbo].[StockTransferDetails] ADD [ToLocationId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.StockAdjustmentLines', N'WarehouseLocationId') IS NULL
    ALTER TABLE [dbo].[StockAdjustmentLines] ADD [WarehouseLocationId] UNIQUEIDENTIFIER NULL;
GO

IF OBJECT_ID(N'[dbo].[ReplenishmentTasks]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReplenishmentTasks] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [TaskNumber] NVARCHAR(50) NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_ReplenishmentTasks_Status] DEFAULT (N'Open'),
        [Notes] NVARCHAR(500) NULL,
        [CompletedAt] DATETIME2 NULL,
        [CompletedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_ReplenishmentTasks_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ReplenishmentTasks_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_ReplenishmentTasks_Tenant] ON [dbo].[ReplenishmentTasks]([TenantId]);
    CREATE INDEX [IX_ReplenishmentTasks_Warehouse] ON [dbo].[ReplenishmentTasks]([WarehouseId]);
END
GO

IF OBJECT_ID(N'[dbo].[ReplenishmentTaskLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReplenishmentTaskLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ReplenishmentTaskId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [FromLocationId] UNIQUEIDENTIFIER NULL,
        [ToLocationId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_ReplenishmentTaskLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ReplenishmentTaskLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_ReplenishmentTaskLines_Header]
            FOREIGN KEY ([ReplenishmentTaskId]) REFERENCES [dbo].[ReplenishmentTasks]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ReplenishmentTaskLines_Header] ON [dbo].[ReplenishmentTaskLines]([ReplenishmentTaskId]);
END
GO

IF OBJECT_ID(N'[dbo].[PackOrders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PackOrders] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PackNumber] NVARCHAR(50) NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [PickListId] UNIQUEIDENTIFIER NULL,
        [DispatchAdviceId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_PackOrders_Status] DEFAULT (N'Open'),
        [Notes] NVARCHAR(500) NULL,
        [PackedAt] DATETIME2 NULL,
        [PackedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PackOrders_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PackOrders_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_PackOrders_Tenant] ON [dbo].[PackOrders]([TenantId]);
    CREATE INDEX [IX_PackOrders_Dispatch] ON [dbo].[PackOrders]([DispatchAdviceId]);
    CREATE INDEX [IX_PackOrders_Pick] ON [dbo].[PackOrders]([PickListId]);
END
GO

IF OBJECT_ID(N'[dbo].[PackOrderLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PackOrderLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PackOrderId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [PackageCode] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PackOrderLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PackOrderLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_PackOrderLines_Header]
            FOREIGN KEY ([PackOrderId]) REFERENCES [dbo].[PackOrders]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_PackOrderLines_Header] ON [dbo].[PackOrderLines]([PackOrderId]);
END
GO

IF OBJECT_ID(N'[dbo].[Shipments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Shipments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ShipmentNumber] NVARCHAR(50) NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [PackOrderId] UNIQUEIDENTIFIER NULL,
        [DispatchAdviceId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_Shipments_Status] DEFAULT (N'Draft'),
        [Carrier] NVARCHAR(100) NULL,
        [TrackingNumber] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,
        [ShippedAt] DATETIME2 NULL,
        [ShippedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Shipments_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Shipments_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_Shipments_Tenant] ON [dbo].[Shipments]([TenantId]);
    CREATE INDEX [IX_Shipments_Dispatch] ON [dbo].[Shipments]([DispatchAdviceId]);
    CREATE INDEX [IX_Shipments_Pack] ON [dbo].[Shipments]([PackOrderId]);
END
GO

IF OBJECT_ID(N'[dbo].[ShipmentLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ShipmentLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ShipmentId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_ShipmentLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ShipmentLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_ShipmentLines_Header]
            FOREIGN KEY ([ShipmentId]) REFERENCES [dbo].[Shipments]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ShipmentLines_Header] ON [dbo].[ShipmentLines]([ShipmentId]);
END
GO
