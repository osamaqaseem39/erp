-- WMS MVP: bin-level stock, location types, putaway tasks, pick lists
-- Idempotent IF NOT EXISTS / COL_LENGTH checks

IF COL_LENGTH(N'dbo.WarehouseLocations', N'LocationType') IS NULL
    ALTER TABLE [dbo].[WarehouseLocations] ADD [LocationType] NVARCHAR(30) NOT NULL CONSTRAINT [DF_WarehouseLocations_LocationType] DEFAULT (N'Pick');
GO
IF COL_LENGTH(N'dbo.WarehouseLocations', N'Zone') IS NULL
    ALTER TABLE [dbo].[WarehouseLocations] ADD [Zone] NVARCHAR(50) NULL;
GO

IF COL_LENGTH(N'dbo.StockBalances', N'WarehouseLocationId') IS NULL
    ALTER TABLE [dbo].[StockBalances] ADD [WarehouseLocationId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.StockLedgers', N'WarehouseLocationId') IS NULL
    ALTER TABLE [dbo].[StockLedgers] ADD [WarehouseLocationId] UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockBalances_Item_Warehouse_Location' AND object_id = OBJECT_ID(N'dbo.StockBalances'))
    CREATE UNIQUE INDEX [IX_StockBalances_Item_Warehouse_Location]
        ON [dbo].[StockBalances]([TenantId], [ItemId], [WarehouseId], [WarehouseLocationId])
        WHERE [WarehouseLocationId] IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockBalances_Item_Warehouse_Default' AND object_id = OBJECT_ID(N'dbo.StockBalances'))
    CREATE UNIQUE INDEX [IX_StockBalances_Item_Warehouse_Default]
        ON [dbo].[StockBalances]([TenantId], [ItemId], [WarehouseId])
        WHERE [WarehouseLocationId] IS NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockLedgers_Location' AND object_id = OBJECT_ID(N'dbo.StockLedgers'))
    CREATE INDEX [IX_StockLedgers_Location] ON [dbo].[StockLedgers]([WarehouseLocationId]) WHERE [WarehouseLocationId] IS NOT NULL;
GO

IF OBJECT_ID(N'[dbo].[PutawayTasks]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PutawayTasks] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [TaskNumber] NVARCHAR(50) NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [ReferenceDocType] NVARCHAR(50) NOT NULL CONSTRAINT [DF_PutawayTasks_RefType] DEFAULT (N'GRN'),
        [ReferenceDocId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_PutawayTasks_Status] DEFAULT (N'Open'),
        [Notes] NVARCHAR(500) NULL,
        [CompletedAt] DATETIME2 NULL,
        [CompletedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PutawayTasks_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PutawayTasks_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_PutawayTasks_Tenant] ON [dbo].[PutawayTasks]([TenantId]);
    CREATE INDEX [IX_PutawayTasks_Warehouse] ON [dbo].[PutawayTasks]([WarehouseId]);
    CREATE INDEX [IX_PutawayTasks_Reference] ON [dbo].[PutawayTasks]([ReferenceDocType], [ReferenceDocId]);
END
GO

IF OBJECT_ID(N'[dbo].[PutawayTaskLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PutawayTaskLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PutawayTaskId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [SuggestedLocationId] UNIQUEIDENTIFIER NULL,
        [ActualLocationId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PutawayTaskLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PutawayTaskLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_PutawayTaskLines_Header]
            FOREIGN KEY ([PutawayTaskId]) REFERENCES [dbo].[PutawayTasks]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_PutawayTaskLines_Header] ON [dbo].[PutawayTaskLines]([PutawayTaskId]);
    CREATE INDEX [IX_PutawayTaskLines_Item] ON [dbo].[PutawayTaskLines]([ItemId]);
END
GO

IF OBJECT_ID(N'[dbo].[PickLists]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PickLists] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PickNumber] NVARCHAR(50) NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [ReferenceDocType] NVARCHAR(50) NOT NULL CONSTRAINT [DF_PickLists_RefType] DEFAULT (N'DispatchAdvice'),
        [ReferenceDocId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_PickLists_Status] DEFAULT (N'Open'),
        [Notes] NVARCHAR(500) NULL,
        [CompletedAt] DATETIME2 NULL,
        [CompletedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PickLists_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PickLists_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_PickLists_Tenant] ON [dbo].[PickLists]([TenantId]);
    CREATE INDEX [IX_PickLists_Warehouse] ON [dbo].[PickLists]([WarehouseId]);
    CREATE INDEX [IX_PickLists_Reference] ON [dbo].[PickLists]([ReferenceDocType], [ReferenceDocId]);
END
GO

IF OBJECT_ID(N'[dbo].[PickListLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PickListLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PickListId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [QuantityRequested] DECIMAL(18,4) NOT NULL,
        [QuantityPicked] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_PickListLines_Picked] DEFAULT (0),
        [FromLocationId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PickListLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PickListLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_PickListLines_Header]
            FOREIGN KEY ([PickListId]) REFERENCES [dbo].[PickLists]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_PickListLines_Header] ON [dbo].[PickListLines]([PickListId]);
    CREATE INDEX [IX_PickListLines_Item] ON [dbo].[PickListLines]([ItemId]);
END
GO
