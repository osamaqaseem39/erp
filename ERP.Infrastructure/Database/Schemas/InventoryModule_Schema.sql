-- =============================================
-- INVENTORY MODULE DATABASE SCHEMA
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Inventory Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Inventory')
BEGIN
    EXEC('CREATE SCHEMA [Inventory]')
END
GO

-- =============================================
-- WAREHOUSES TABLE
-- =============================================
CREATE TABLE [Inventory].[Warehouses] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [WarehouseCode] NVARCHAR(50) NOT NULL,
    [WarehouseName] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [State] NVARCHAR(100),
    [Country] NVARCHAR(100),
    [ZipCode] NVARCHAR(20),
    [Phone] NVARCHAR(20),
    [Email] NVARCHAR(200),
    [ManagerId] UNIQUEIDENTIFIER,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- ITEM CATEGORIES TABLE
-- =============================================
CREATE TABLE [Inventory].[ItemCategories] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CategoryCode] NVARCHAR(50) NOT NULL,
    [CategoryName] NVARCHAR(200) NOT NULL,
    [ParentCategoryId] UNIQUEIDENTIFIER,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ParentCategoryId]) REFERENCES [Inventory].[ItemCategories]([Id])
);
GO

-- =============================================
-- ITEMS TABLE
-- =============================================
CREATE TABLE [Inventory].[Items] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemCode] NVARCHAR(50) NOT NULL,
    [ItemName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [UnitOfMeasure] NVARCHAR(50) NOT NULL,
    [ItemType] NVARCHAR(50) NOT NULL,
    [BarCode] NVARCHAR(100),
    [SKU] NVARCHAR(100),
    [CostPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [SellingPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [MinStockLevel] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [MaxStockLevel] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [ReorderLevel] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [IsSerialized] BIT NOT NULL DEFAULT 0,
    [IsBatchTracked] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([CategoryId]) REFERENCES [Inventory].[ItemCategories]([Id])
);
GO

-- =============================================
-- STOCK BALANCES TABLE
-- =============================================
CREATE TABLE [Inventory].[StockBalances] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [QuantityOnHand] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [QuantityReserved] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [QuantityAvailable] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [LastUpdated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]),
    FOREIGN KEY ([WarehouseId]) REFERENCES [Inventory].[Warehouses]([Id])
);
GO

-- =============================================
-- STOCK LEDGERS TABLE
-- =============================================
CREATE TABLE [Inventory].[StockLedgers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionType] NVARCHAR(50) NOT NULL,
    [TransactionDate] DATETIME2 NOT NULL,
    [Quantity] DECIMAL(18,2) NOT NULL,
    [UnitCost] DECIMAL(18,2) NOT NULL,
    [TotalCost] DECIMAL(18,2) NOT NULL,
    [Reference] NVARCHAR(100),
    [ReferenceId] UNIQUEIDENTIFIER,
    [BatchNumber] NVARCHAR(100),
    [SerialNumber] NVARCHAR(100),
    [ExpiryDate] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]),
    FOREIGN KEY ([WarehouseId]) REFERENCES [Inventory].[Warehouses]([Id])
);
GO

-- =============================================
-- INVENTORY ADJUSTMENTS TABLE
-- =============================================
CREATE TABLE [Inventory].[InventoryAdjustments] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [AdjustmentNumber] NVARCHAR(50) NOT NULL,
    [AdjustmentDate] DATETIME2 NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [Reason] NVARCHAR(200) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [TotalValue] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Notes] NVARCHAR(1000),
    [ApprovedBy] UNIQUEIDENTIFIER,
    [ApprovedAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([WarehouseId]) REFERENCES [Inventory].[Warehouses]([Id])
);
GO

-- =============================================
-- INVENTORY ADJUSTMENT ITEMS TABLE
-- =============================================
CREATE TABLE [Inventory].[InventoryAdjustmentItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [AdjustmentId] UNIQUEIDENTIFIER NOT NULL,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [QuantityOnHand] DECIMAL(18,2) NOT NULL,
    [QuantityCounted] DECIMAL(18,2) NOT NULL,
    [QuantityDifference] DECIMAL(18,2) NOT NULL,
    [UnitCost] DECIMAL(18,2) NOT NULL,
    [TotalValue] DECIMAL(18,2) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([AdjustmentId]) REFERENCES [Inventory].[InventoryAdjustments]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id])
);
GO

-- =============================================
-- BATCH SERIAL TRACKING TABLE
-- =============================================
CREATE TABLE [Inventory].[BatchSerialTracking] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [BatchNumber] NVARCHAR(100),
    [SerialNumber] NVARCHAR(100),
    [Quantity] DECIMAL(18,2) NOT NULL,
    [ExpiryDate] DATETIME2,
    [ManufacturingDate] DATETIME2,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Available',
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]),
    FOREIGN KEY ([WarehouseId]) REFERENCES [Inventory].[Warehouses]([Id])
);
GO

-- =============================================
-- STOCK TRANSFERS TABLE
-- =============================================
CREATE TABLE [Inventory].[StockTransfers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TransferNumber] NVARCHAR(50) NOT NULL,
    [TransferDate] DATETIME2 NOT NULL,
    [FromWarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [ToWarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [TotalValue] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Notes] NVARCHAR(1000),
    [ApprovedBy] UNIQUEIDENTIFIER,
    [ApprovedAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([FromWarehouseId]) REFERENCES [Inventory].[Warehouses]([Id]),
    FOREIGN KEY ([ToWarehouseId]) REFERENCES [Inventory].[Warehouses]([Id])
);
GO

-- =============================================
-- STOCK TRANSFER ITEMS TABLE
-- =============================================
CREATE TABLE [Inventory].[StockTransferItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TransferId] UNIQUEIDENTIFIER NOT NULL,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] DECIMAL(18,2) NOT NULL,
    [UnitCost] DECIMAL(18,2) NOT NULL,
    [TotalCost] DECIMAL(18,2) NOT NULL,
    [BatchNumber] NVARCHAR(100),
    [SerialNumber] NVARCHAR(100),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([TransferId]) REFERENCES [Inventory].[StockTransfers]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id])
);
GO

-- =============================================
-- UNITS OF MEASURE TABLE
-- =============================================
CREATE TABLE [Inventory].[UnitsOfMeasure] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [UnitCode] NVARCHAR(50) NOT NULL,
    [UnitName] NVARCHAR(100) NOT NULL,
    [UnitType] NVARCHAR(50) NOT NULL,
    [BaseUnitId] UNIQUEIDENTIFIER,
    [ConversionFactor] DECIMAL(18,6) NOT NULL DEFAULT 1,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BaseUnitId]) REFERENCES [Inventory].[UnitsOfMeasure]([Id])
);
GO

-- =============================================
-- ITEM PRICING TABLE
-- =============================================
CREATE TABLE [Inventory].[ItemPricing] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [PriceListName] NVARCHAR(100) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [MinQuantity] DECIMAL(18,2) NOT NULL DEFAULT 1,
    [MaxQuantity] DECIMAL(18,2),
    [ValidFrom] DATETIME2 NOT NULL,
    [ValidTo] DATETIME2,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id])
);
GO

-- =============================================
-- INDEXES FOR INVENTORY MODULE
-- =============================================
CREATE INDEX IX_Warehouses_TenantId ON [Inventory].[Warehouses]([TenantId]);
CREATE INDEX IX_Warehouses_WarehouseCode ON [Inventory].[Warehouses]([WarehouseCode]);
CREATE INDEX IX_ItemCategories_TenantId ON [Inventory].[ItemCategories]([TenantId]);
CREATE INDEX IX_ItemCategories_ParentCategoryId ON [Inventory].[ItemCategories]([ParentCategoryId]);
CREATE INDEX IX_Items_TenantId ON [Inventory].[Items]([TenantId]);
CREATE INDEX IX_Items_CategoryId ON [Inventory].[Items]([CategoryId]);
CREATE INDEX IX_Items_ItemCode ON [Inventory].[Items]([ItemCode]);
CREATE INDEX IX_Items_BarCode ON [Inventory].[Items]([BarCode]);
CREATE INDEX IX_StockBalances_ItemId ON [Inventory].[StockBalances]([ItemId]);
CREATE INDEX IX_StockBalances_WarehouseId ON [Inventory].[StockBalances]([WarehouseId]);
CREATE INDEX IX_StockLedgers_ItemId ON [Inventory].[StockLedgers]([ItemId]);
CREATE INDEX IX_StockLedgers_WarehouseId ON [Inventory].[StockLedgers]([WarehouseId]);
CREATE INDEX IX_StockLedgers_TransactionDate ON [Inventory].[StockLedgers]([TransactionDate]);
CREATE INDEX IX_InventoryAdjustments_WarehouseId ON [Inventory].[InventoryAdjustments]([WarehouseId]);
CREATE INDEX IX_InventoryAdjustmentItems_AdjustmentId ON [Inventory].[InventoryAdjustmentItems]([AdjustmentId]);
CREATE INDEX IX_InventoryAdjustmentItems_ItemId ON [Inventory].[InventoryAdjustmentItems]([ItemId]);
CREATE INDEX IX_BatchSerialTracking_ItemId ON [Inventory].[BatchSerialTracking]([ItemId]);
CREATE INDEX IX_BatchSerialTracking_WarehouseId ON [Inventory].[BatchSerialTracking]([WarehouseId]);
CREATE INDEX IX_StockTransfers_FromWarehouseId ON [Inventory].[StockTransfers]([FromWarehouseId]);
CREATE INDEX IX_StockTransfers_ToWarehouseId ON [Inventory].[StockTransfers]([ToWarehouseId]);
CREATE INDEX IX_StockTransferItems_TransferId ON [Inventory].[StockTransferItems]([TransferId]);
CREATE INDEX IX_StockTransferItems_ItemId ON [Inventory].[StockTransferItems]([ItemId]);
CREATE INDEX IX_UnitsOfMeasure_TenantId ON [Inventory].[UnitsOfMeasure]([TenantId]);
CREATE INDEX IX_ItemPricing_ItemId ON [Inventory].[ItemPricing]([ItemId]);

-- =============================================
-- ITEM GROUPS TABLE
-- =============================================
CREATE TABLE [Inventory].[ItemGroups] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [GroupCode] NVARCHAR(50) NOT NULL,
    [GroupName] NVARCHAR(200) NOT NULL,
    [ParentGroupId] UNIQUEIDENTIFIER,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ParentGroupId]) REFERENCES [Inventory].[ItemGroups]([Id])
);
GO

-- =============================================
-- ITEM ATTRIBUTES TABLE
-- =============================================
CREATE TABLE [Inventory].[ItemAttributes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [AttributeName] NVARCHAR(100) NOT NULL,
    [AttributeType] NVARCHAR(50) NOT NULL, -- Text, Number, Date, List
    [IsRequired] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- ITEM VARIANTS TABLE
-- =============================================
CREATE TABLE [Inventory].[ItemVariants] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [VariantCode] NVARCHAR(50) NOT NULL,
    [VariantName] NVARCHAR(200) NOT NULL,
    [SKU] NVARCHAR(100),
    [CostPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [SellingPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- ITEM VARIANT ATTRIBUTES TABLE
-- =============================================
CREATE TABLE [Inventory].[ItemVariantAttributes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [VariantId] UNIQUEIDENTIFIER NOT NULL,
    [AttributeId] UNIQUEIDENTIFIER NOT NULL,
    [AttributeValue] NVARCHAR(500) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([VariantId]) REFERENCES [Inventory].[ItemVariants]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([AttributeId]) REFERENCES [Inventory].[ItemAttributes]([Id])
);
GO

-- =============================================
-- BARCODES TABLE
-- =============================================
CREATE TABLE [Inventory].[Barcodes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER,
    [VariantId] UNIQUEIDENTIFIER,
    [Barcode] NVARCHAR(100) NOT NULL,
    [BarcodeType] NVARCHAR(50) NOT NULL, -- UPC, EAN, Code128, QR
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([VariantId]) REFERENCES [Inventory].[ItemVariants]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- BATCH NUMBERS TABLE
-- =============================================
CREATE TABLE [Inventory].[BatchNumbers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [BatchNumber] NVARCHAR(100) NOT NULL,
    [ManufacturingDate] DATETIME2,
    [ExpiryDate] DATETIME2,
    [Quantity] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- SERIAL NUMBERS TABLE
-- =============================================
CREATE TABLE [Inventory].[SerialNumbers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [SerialNumber] NVARCHAR(100) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Available', -- Available, Sold, Reserved, Damaged
    [Location] NVARCHAR(200),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- REORDER RULES TABLE
-- =============================================
CREATE TABLE [Inventory].[ReorderRules] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [MinStockLevel] DECIMAL(18,4) NOT NULL,
    [MaxStockLevel] DECIMAL(18,4) NOT NULL,
    [ReorderPoint] DECIMAL(18,4) NOT NULL,
    [ReorderQuantity] DECIMAL(18,4) NOT NULL,
    [LeadTimeDays] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ItemId]) REFERENCES [Inventory].[Items]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([WarehouseId]) REFERENCES [Inventory].[Warehouses]([Id])
);
GO

-- =============================================
-- INVENTORY VALUATION METHODS TABLE
-- =============================================
CREATE TABLE [Inventory].[InventoryValuationMethods] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [MethodName] NVARCHAR(100) NOT NULL,
    [MethodCode] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- ADDITIONAL INDEXES FOR INVENTORY MODULE
-- =============================================
CREATE INDEX IX_ItemGroups_TenantId ON [Inventory].[ItemGroups]([TenantId]);
CREATE INDEX IX_ItemGroups_GroupCode ON [Inventory].[ItemGroups]([GroupCode]);
CREATE INDEX IX_ItemAttributes_TenantId ON [Inventory].[ItemAttributes]([TenantId]);
CREATE INDEX IX_ItemVariants_ItemId ON [Inventory].[ItemVariants]([ItemId]);
CREATE INDEX IX_ItemVariants_VariantCode ON [Inventory].[ItemVariants]([VariantCode]);
CREATE INDEX IX_ItemVariantAttributes_VariantId ON [Inventory].[ItemVariantAttributes]([VariantId]);
CREATE INDEX IX_ItemVariantAttributes_AttributeId ON [Inventory].[ItemVariantAttributes]([AttributeId]);
CREATE INDEX IX_Barcodes_ItemId ON [Inventory].[Barcodes]([ItemId]);
CREATE INDEX IX_Barcodes_VariantId ON [Inventory].[Barcodes]([VariantId]);
CREATE INDEX IX_Barcodes_Barcode ON [Inventory].[Barcodes]([Barcode]);
CREATE INDEX IX_BatchNumbers_ItemId ON [Inventory].[BatchNumbers]([ItemId]);
CREATE INDEX IX_BatchNumbers_BatchNumber ON [Inventory].[BatchNumbers]([BatchNumber]);
CREATE INDEX IX_SerialNumbers_ItemId ON [Inventory].[SerialNumbers]([ItemId]);
CREATE INDEX IX_SerialNumbers_SerialNumber ON [Inventory].[SerialNumbers]([SerialNumber]);
CREATE INDEX IX_ReorderRules_ItemId ON [Inventory].[ReorderRules]([ItemId]);
CREATE INDEX IX_ReorderRules_WarehouseId ON [Inventory].[ReorderRules]([WarehouseId]);
CREATE INDEX IX_InventoryValuationMethods_TenantId ON [Inventory].[InventoryValuationMethods]([TenantId]);

PRINT 'Inventory Module schema created successfully!'