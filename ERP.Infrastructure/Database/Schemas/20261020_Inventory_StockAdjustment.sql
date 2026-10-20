-- Stock adjustment documents (header + lines); post applies qty delta via stock service.
IF OBJECT_ID(N'[dbo].[StockAdjustments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StockAdjustments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CompanyId] UNIQUEIDENTIFIER NULL,
        [AdjustmentNumber] NVARCHAR(50) NOT NULL,
        [AdjustmentDate] DATETIME2 NOT NULL,
        [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
        [AdjustmentType] NVARCHAR(50) NOT NULL CONSTRAINT [DF_StockAdjustments_Type] DEFAULT (N'Physical'),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_StockAdjustments_Status] DEFAULT (N'Draft'),
        [Reference] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(1000) NULL,
        [PostedBy] NVARCHAR(100) NULL,
        [PostedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_StockAdjustments_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_StockAdjustments_IsActive] DEFAULT (1)
    );
    CREATE INDEX [IX_StockAdjustments_TenantId] ON [dbo].[StockAdjustments]([TenantId]);
    CREATE INDEX [IX_StockAdjustments_WarehouseId] ON [dbo].[StockAdjustments]([WarehouseId]);
END
GO

IF OBJECT_ID(N'[dbo].[StockAdjustmentLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StockAdjustmentLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [StockAdjustmentId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NOT NULL,
        [QuantityOnHand] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockAdjLines_OnHand] DEFAULT (0),
        [QuantityCounted] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockAdjLines_Counted] DEFAULT (0),
        [QuantityDifference] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockAdjLines_Diff] DEFAULT (0),
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_StockAdjLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_StockAdjLines_IsActive] DEFAULT (1),
        CONSTRAINT [FK_StockAdjustmentLines_Header]
            FOREIGN KEY ([StockAdjustmentId]) REFERENCES [dbo].[StockAdjustments]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_StockAdjustmentLines_Header] ON [dbo].[StockAdjustmentLines]([StockAdjustmentId]);
    CREATE INDEX [IX_StockAdjustmentLines_ItemId] ON [dbo].[StockAdjustmentLines]([ItemId]);
END
GO
