-- =============================================
-- INVENTORY MODULE STORED PROCEDURES
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Inventory Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Inventory')
BEGIN
    EXEC('CREATE SCHEMA [Inventory]')
END
GO

-- =============================================
-- WAREHOUSE MANAGEMENT PROCEDURES
-- =============================================

-- Get Warehouse by ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetWarehouseById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, WarehouseCode, WarehouseName, Address, City, State, Country,
        ZipCode, Phone, Email, ManagerId, IsActive, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[Warehouses]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get All Warehouses for Tenant
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetAllWarehouses]
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = 1,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, WarehouseCode, WarehouseName, Address, City, State, Country,
        ZipCode, Phone, Email, ManagerId, IsActive, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[Warehouses]
    WHERE TenantId = @TenantId AND IsActive = @IsActive
    ORDER BY WarehouseName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Inventory].[Warehouses]
    WHERE TenantId = @TenantId AND IsActive = @IsActive;
END
GO

-- Create Warehouse
CREATE OR ALTER PROCEDURE [Inventory].[sp_CreateWarehouse]
    @Id UNIQUEIDENTIFIER,
    @WarehouseCode NVARCHAR(50),
    @WarehouseName NVARCHAR(200),
    @Address NVARCHAR(500),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Country NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Phone NVARCHAR(20),
    @Email NVARCHAR(200),
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Inventory].[Warehouses] (
        Id, WarehouseCode, WarehouseName, Address, City, State, Country,
        ZipCode, Phone, Email, ManagerId, IsActive, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @WarehouseCode, @WarehouseName, @Address, @City, @State, @Country,
        @ZipCode, @Phone, @Email, @ManagerId, 1, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Warehouse
CREATE OR ALTER PROCEDURE [Inventory].[sp_UpdateWarehouse]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @WarehouseCode NVARCHAR(50) = NULL,
    @WarehouseName NVARCHAR(200) = NULL,
    @Address NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate warehouse exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[Warehouses] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Warehouse not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[Warehouses]
    SET 
        WarehouseCode = ISNULL(@WarehouseCode, WarehouseCode),
        WarehouseName = ISNULL(@WarehouseName, WarehouseName),
        Address = ISNULL(@Address, Address),
        City = ISNULL(@City, City),
        State = ISNULL(@State, State),
        Country = ISNULL(@Country, Country),
        ZipCode = ISNULL(@ZipCode, ZipCode),
        Phone = ISNULL(@Phone, Phone),
        Email = ISNULL(@Email, Email),
        ManagerId = ISNULL(@ManagerId, ManagerId),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Warehouse (Soft Delete)
CREATE OR ALTER PROCEDURE [Inventory].[sp_DeleteWarehouse]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate warehouse exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[Warehouses] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Warehouse not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if warehouse has stock
    IF EXISTS (SELECT 1 FROM [Inventory].[StockBalances] WHERE WarehouseId = @Id AND TenantId = @TenantId AND QuantityOnHand > 0)
    BEGIN
        RAISERROR('Cannot delete warehouse with stock. Please transfer or adjust stock first.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Inventory].[Warehouses]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- ITEM CATEGORY PROCEDURES
-- =============================================

-- Get Item Category by ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetItemCategoryById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CategoryCode, CategoryName, ParentCategoryId, Description,
        IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[ItemCategories]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get All Item Categories for Tenant
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetAllItemCategories]
    @TenantId UNIQUEIDENTIFIER,
    @ParentCategoryId UNIQUEIDENTIFIER = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CategoryCode, CategoryName, ParentCategoryId, Description,
        IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[ItemCategories]
    WHERE TenantId = @TenantId 
        AND IsActive = @IsActive
        AND (@ParentCategoryId IS NULL OR ParentCategoryId = @ParentCategoryId)
    ORDER BY CategoryCode;
END
GO

-- Create Item Category
CREATE OR ALTER PROCEDURE [Inventory].[sp_CreateItemCategory]
    @Id UNIQUEIDENTIFIER,
    @CategoryCode NVARCHAR(50),
    @CategoryName NVARCHAR(200),
    @ParentCategoryId UNIQUEIDENTIFIER = NULL,
    @Description NVARCHAR(500),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Inventory].[ItemCategories] (
        Id, CategoryCode, CategoryName, ParentCategoryId, Description,
        IsActive, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @CategoryCode, @CategoryName, @ParentCategoryId, @Description,
        1, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Item Category
CREATE OR ALTER PROCEDURE [Inventory].[sp_UpdateItemCategory]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CategoryName NVARCHAR(200) = NULL,
    @CategoryCode NVARCHAR(50) = NULL,
    @ParentCategoryId UNIQUEIDENTIFIER = NULL,
    @Description NVARCHAR(500) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate category exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[ItemCategories] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Item category not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if trying to set parent to itself
    IF @ParentCategoryId = @Id
    BEGIN
        RAISERROR('Category cannot be its own parent.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[ItemCategories]
    SET 
        CategoryName = ISNULL(@CategoryName, CategoryName),
        CategoryCode = ISNULL(@CategoryCode, CategoryCode),
        ParentCategoryId = @ParentCategoryId, -- Allow NULL explicitly
        Description = ISNULL(@Description, Description),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Item Category (Soft Delete)
CREATE OR ALTER PROCEDURE [Inventory].[sp_DeleteItemCategory]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate category exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[ItemCategories] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Item category not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if category has child categories
    IF EXISTS (SELECT 1 FROM [Inventory].[ItemCategories] WHERE ParentCategoryId = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Cannot delete category with child categories. Please delete or reassign child categories first.', 16, 1);
        RETURN;
    END
    
    -- Check if category has items
    IF EXISTS (SELECT 1 FROM [Inventory].[Items] WHERE CategoryId = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Cannot delete category with items. Please reassign items to another category first.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Inventory].[ItemCategories]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- ITEM MANAGEMENT PROCEDURES
-- =============================================

-- Get Item by ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetItemById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        i.Id, i.ItemCode, i.ItemName, i.Description, i.CategoryId, i.UnitOfMeasure,
        i.ItemType, i.BarCode, i.SKU, i.CostPrice, i.SellingPrice, i.MinStockLevel,
        i.MaxStockLevel, i.ReorderLevel, i.IsSerialized, i.IsBatchTracked, i.IsActive,
        i.TenantId, i.CreatedAt, i.UpdatedAt, i.CreatedBy, i.UpdatedBy,
        ic.CategoryName
    FROM [Inventory].[Items] i
    INNER JOIN [Inventory].[ItemCategories] ic ON i.CategoryId = ic.Id
    WHERE i.Id = @Id AND i.TenantId = @TenantId AND i.IsActive = 1;
END
GO

-- Get Item by Item Code
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetItemByCode]
    @ItemCode NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        i.Id, i.ItemCode, i.ItemName, i.Description, i.CategoryId, i.UnitOfMeasure,
        i.ItemType, i.BarCode, i.SKU, i.CostPrice, i.SellingPrice, i.MinStockLevel,
        i.MaxStockLevel, i.ReorderLevel, i.IsSerialized, i.IsBatchTracked, i.IsActive,
        i.TenantId, i.CreatedAt, i.UpdatedAt, i.CreatedBy, i.UpdatedBy,
        ic.CategoryName
    FROM [Inventory].[Items] i
    INNER JOIN [Inventory].[ItemCategories] ic ON i.CategoryId = ic.Id
    WHERE i.ItemCode = @ItemCode AND i.TenantId = @TenantId AND i.IsActive = 1;
END
GO

-- Get All Items for Tenant
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetAllItems]
    @TenantId UNIQUEIDENTIFIER,
    @CategoryId UNIQUEIDENTIFIER = NULL,
    @ItemType NVARCHAR(50) = NULL,
    @IsActive BIT = 1,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        i.Id, i.ItemCode, i.ItemName, i.Description, i.CategoryId, i.UnitOfMeasure,
        i.ItemType, i.BarCode, i.SKU, i.CostPrice, i.SellingPrice, i.MinStockLevel,
        i.MaxStockLevel, i.ReorderLevel, i.IsSerialized, i.IsBatchTracked, i.IsActive,
        i.TenantId, i.CreatedAt, i.UpdatedAt, i.CreatedBy, i.UpdatedBy,
        ic.CategoryName
    FROM [Inventory].[Items] i
    INNER JOIN [Inventory].[ItemCategories] ic ON i.CategoryId = ic.Id
    WHERE i.TenantId = @TenantId 
        AND i.IsActive = @IsActive
        AND (@CategoryId IS NULL OR i.CategoryId = @CategoryId)
        AND (@ItemType IS NULL OR i.ItemType = @ItemType)
    ORDER BY i.ItemCode
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Inventory].[Items] i
    WHERE i.TenantId = @TenantId 
        AND i.IsActive = @IsActive
        AND (@CategoryId IS NULL OR i.CategoryId = @CategoryId)
        AND (@ItemType IS NULL OR i.ItemType = @ItemType);
END
GO

-- Create Item
CREATE OR ALTER PROCEDURE [Inventory].[sp_CreateItem]
    @Id UNIQUEIDENTIFIER,
    @ItemCode NVARCHAR(50),
    @ItemName NVARCHAR(200),
    @Description NVARCHAR(500),
    @CategoryId UNIQUEIDENTIFIER,
    @UnitOfMeasure NVARCHAR(50),
    @ItemType NVARCHAR(50),
    @BarCode NVARCHAR(100),
    @SKU NVARCHAR(100),
    @CostPrice DECIMAL(18,2),
    @SellingPrice DECIMAL(18,2),
    @MinStockLevel DECIMAL(18,2),
    @MaxStockLevel DECIMAL(18,2),
    @ReorderLevel DECIMAL(18,2),
    @IsSerialized BIT,
    @IsBatchTracked BIT,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Inventory].[Items] (
        Id, ItemCode, ItemName, Description, CategoryId, UnitOfMeasure, ItemType,
        BarCode, SKU, CostPrice, SellingPrice, MinStockLevel, MaxStockLevel,
        ReorderLevel, IsSerialized, IsBatchTracked, IsActive, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @ItemCode, @ItemName, @Description, @CategoryId, @UnitOfMeasure, @ItemType,
        @BarCode, @SKU, @CostPrice, @SellingPrice, @MinStockLevel, @MaxStockLevel,
        @ReorderLevel, @IsSerialized, @IsBatchTracked, 1, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Item
CREATE OR ALTER PROCEDURE [Inventory].[sp_UpdateItem]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ItemCode NVARCHAR(50) = NULL,
    @ItemName NVARCHAR(200) = NULL,
    @Description NVARCHAR(1000) = NULL,
    @CategoryId UNIQUEIDENTIFIER = NULL,
    @UnitOfMeasure NVARCHAR(50) = NULL,
    @ItemType NVARCHAR(50) = NULL,
    @BarCode NVARCHAR(100) = NULL,
    @SKU NVARCHAR(100) = NULL,
    @CostPrice DECIMAL(18,2) = NULL,
    @SellingPrice DECIMAL(18,2) = NULL,
    @MinStockLevel DECIMAL(18,2) = NULL,
    @MaxStockLevel DECIMAL(18,2) = NULL,
    @ReorderLevel DECIMAL(18,2) = NULL,
    @IsSerialized BIT = NULL,
    @IsBatchTracked BIT = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate item exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[Items] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Item not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[Items]
    SET 
        ItemCode = ISNULL(@ItemCode, ItemCode),
        ItemName = ISNULL(@ItemName, ItemName),
        Description = ISNULL(@Description, Description),
        CategoryId = ISNULL(@CategoryId, CategoryId),
        UnitOfMeasure = ISNULL(@UnitOfMeasure, UnitOfMeasure),
        ItemType = ISNULL(@ItemType, ItemType),
        BarCode = ISNULL(@BarCode, BarCode),
        SKU = ISNULL(@SKU, SKU),
        CostPrice = ISNULL(@CostPrice, CostPrice),
        SellingPrice = ISNULL(@SellingPrice, SellingPrice),
        MinStockLevel = ISNULL(@MinStockLevel, MinStockLevel),
        MaxStockLevel = ISNULL(@MaxStockLevel, MaxStockLevel),
        ReorderLevel = ISNULL(@ReorderLevel, ReorderLevel),
        IsSerialized = ISNULL(@IsSerialized, IsSerialized),
        IsBatchTracked = ISNULL(@IsBatchTracked, IsBatchTracked),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Item (Soft Delete)
CREATE OR ALTER PROCEDURE [Inventory].[sp_DeleteItem]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate item exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[Items] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Item not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if item has stock
    IF EXISTS (SELECT 1 FROM [Inventory].[StockBalances] WHERE ItemId = @Id AND TenantId = @TenantId AND QuantityOnHand > 0)
    BEGIN
        RAISERROR('Cannot delete item with stock. Please adjust or transfer stock first.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Inventory].[Items]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- STOCK BALANCE PROCEDURES
-- =============================================

-- Get Stock Balance by Item and Warehouse
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockBalance]
    @ItemId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, ItemId, WarehouseId, QuantityOnHand, QuantityReserved, QuantityAvailable,
        LastUpdated, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockBalances]
    WHERE ItemId = @ItemId AND WarehouseId = @WarehouseId AND TenantId = @TenantId;
END
GO

-- Get All Stock Balances for Warehouse
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockBalancesByWarehouse]
    @WarehouseId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ItemId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        sb.Id, sb.ItemId, sb.WarehouseId, sb.QuantityOnHand, sb.QuantityReserved,
        sb.QuantityAvailable, sb.LastUpdated, sb.TenantId, sb.CreatedAt, sb.UpdatedAt,
        sb.CreatedBy, sb.UpdatedBy,
        i.ItemCode, i.ItemName, i.UnitOfMeasure
    FROM [Inventory].[StockBalances] sb
    INNER JOIN [Inventory].[Items] i ON sb.ItemId = i.Id
    WHERE sb.WarehouseId = @WarehouseId 
        AND sb.TenantId = @TenantId
        AND (@ItemId IS NULL OR sb.ItemId = @ItemId)
    ORDER BY i.ItemCode
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Inventory].[StockBalances] sb
    WHERE sb.WarehouseId = @WarehouseId 
        AND sb.TenantId = @TenantId
        AND (@ItemId IS NULL OR sb.ItemId = @ItemId);
END
GO

-- Update Stock Balance
CREATE OR ALTER PROCEDURE [Inventory].[sp_UpdateStockBalance]
    @ItemId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER,
    @QuantityOnHand DECIMAL(18,2),
    @QuantityReserved DECIMAL(18,2),
    @QuantityAvailable DECIMAL(18,2),
    @TenantId UNIQUEIDENTIFIER,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Inventory].[StockBalances] WHERE ItemId = @ItemId AND WarehouseId = @WarehouseId AND TenantId = @TenantId)
    BEGIN
        UPDATE [Inventory].[StockBalances]
        SET 
            QuantityOnHand = @QuantityOnHand,
            QuantityReserved = @QuantityReserved,
            QuantityAvailable = @QuantityAvailable,
            LastUpdated = GETUTCDATE(),
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @UpdatedBy
        WHERE ItemId = @ItemId AND WarehouseId = @WarehouseId AND TenantId = @TenantId;
    END
    ELSE
    BEGIN
    INSERT INTO [Inventory].[StockBalances] (
        Id, ItemId, WarehouseId, QuantityOnHand, QuantityReserved, QuantityAvailable,
        LastUpdated, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        NEWID(), @ItemId, @WarehouseId, @QuantityOnHand, @QuantityReserved, @QuantityAvailable,
        GETUTCDATE(), @TenantId, GETUTCDATE(), @UpdatedBy
    );
    END
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- STOCK LEDGER PROCEDURES
-- =============================================

-- Get Stock Ledger Entries by Item
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockLedgerEntriesByItem]
    @ItemId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, ItemId, WarehouseId, TransactionType, TransactionDate, Quantity,
        UnitCost, TotalCost, Reference, ReferenceId, BatchNumber, SerialNumber,
        ExpiryDate, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockLedgers]
    WHERE ItemId = @ItemId 
        AND TenantId = @TenantId
        AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
        AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
        AND (@EndDate IS NULL OR TransactionDate <= @EndDate)
    ORDER BY TransactionDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Inventory].[StockLedgers]
    WHERE ItemId = @ItemId 
        AND TenantId = @TenantId
        AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
        AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
        AND (@EndDate IS NULL OR TransactionDate <= @EndDate);
END
GO

-- Create Stock Ledger Entry
CREATE OR ALTER PROCEDURE [Inventory].[sp_CreateStockLedgerEntry]
    @Id UNIQUEIDENTIFIER,
    @ItemId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER,
    @TransactionType NVARCHAR(50),
    @TransactionDate DATETIME2,
    @Quantity DECIMAL(18,2),
    @UnitCost DECIMAL(18,2),
    @TotalCost DECIMAL(18,2),
    @Reference NVARCHAR(100),
    @ReferenceId UNIQUEIDENTIFIER = NULL,
    @BatchNumber NVARCHAR(100) = NULL,
    @SerialNumber NVARCHAR(100) = NULL,
    @ExpiryDate DATETIME2 = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Inventory].[StockLedgers] (
        Id, ItemId, WarehouseId, TransactionType, TransactionDate, Quantity,
        UnitCost, TotalCost, Reference, ReferenceId, BatchNumber, SerialNumber,
        ExpiryDate, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @WarehouseId, @TransactionType, @TransactionDate, @Quantity,
        @UnitCost, @TotalCost, @Reference, @ReferenceId, @BatchNumber, @SerialNumber,
        @ExpiryDate, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- INVENTORY ADJUSTMENT PROCEDURES
-- =============================================

-- Get Inventory Adjustment by ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetInventoryAdjustmentById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, AdjustmentNumber, AdjustmentDate, WarehouseId, Reason, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[InventoryAdjustments]
    WHERE Id = @Id AND TenantId = @TenantId;
END
GO

-- Get Inventory Adjustment Items by Adjustment ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetInventoryAdjustmentItemsByAdjustmentId]
    @AdjustmentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        iai.Id, iai.AdjustmentId, iai.ItemId, iai.QuantityOnHand, iai.QuantityCounted,
        iai.QuantityDifference, iai.UnitCost, iai.TotalValue, iai.TenantId, iai.CreatedAt,
        iai.UpdatedAt, iai.CreatedBy, iai.UpdatedBy,
        i.ItemCode, i.ItemName, i.UnitOfMeasure
    FROM [Inventory].[InventoryAdjustmentItems] iai
    INNER JOIN [Inventory].[Items] i ON iai.ItemId = i.Id
    WHERE iai.AdjustmentId = @AdjustmentId AND iai.TenantId = @TenantId
    ORDER BY i.ItemCode;
END
GO

-- Create Inventory Adjustment
CREATE OR ALTER PROCEDURE [Inventory].[sp_CreateInventoryAdjustment]
    @Id UNIQUEIDENTIFIER,
    @AdjustmentNumber NVARCHAR(50),
    @AdjustmentDate DATETIME2,
    @WarehouseId UNIQUEIDENTIFIER,
    @Reason NVARCHAR(200),
    @Notes NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Inventory].[InventoryAdjustments] (
        Id, AdjustmentNumber, AdjustmentDate, WarehouseId, Reason, Status,
        TotalValue, Notes, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @AdjustmentNumber, @AdjustmentDate, @WarehouseId, @Reason, 'Draft',
        0, @Notes, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Get All Inventory Adjustments
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetAllInventoryAdjustments]
    @TenantId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, AdjustmentNumber, AdjustmentDate, WarehouseId, Reason, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[InventoryAdjustments]
    WHERE TenantId = @TenantId
        AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR AdjustmentDate >= @StartDate)
        AND (@EndDate IS NULL OR AdjustmentDate <= @EndDate)
    ORDER BY AdjustmentDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Inventory].[InventoryAdjustments]
    WHERE TenantId = @TenantId
        AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR AdjustmentDate >= @StartDate)
        AND (@EndDate IS NULL OR AdjustmentDate <= @EndDate);
END
GO

-- Update Inventory Adjustment
CREATE OR ALTER PROCEDURE [Inventory].[sp_UpdateInventoryAdjustment]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @AdjustmentNumber NVARCHAR(50) = NULL,
    @AdjustmentDate DATETIME2 = NULL,
    @WarehouseId UNIQUEIDENTIFIER = NULL,
    @Reason NVARCHAR(200) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate adjustment exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[InventoryAdjustments] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Inventory adjustment not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if adjustment is approved
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Inventory].[InventoryAdjustments] WHERE Id = @Id;
    
    IF @Status = 'Approved'
    BEGIN
        RAISERROR('Cannot update approved adjustment.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[InventoryAdjustments]
    SET 
        AdjustmentNumber = ISNULL(@AdjustmentNumber, AdjustmentNumber),
        AdjustmentDate = ISNULL(@AdjustmentDate, AdjustmentDate),
        WarehouseId = ISNULL(@WarehouseId, WarehouseId),
        Reason = ISNULL(@Reason, Reason),
        Notes = ISNULL(@Notes, Notes),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Inventory Adjustment
CREATE OR ALTER PROCEDURE [Inventory].[sp_DeleteInventoryAdjustment]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate adjustment exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[InventoryAdjustments] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Inventory adjustment not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if adjustment is approved
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Inventory].[InventoryAdjustments] WHERE Id = @Id;
    
    IF @Status = 'Approved'
    BEGIN
        RAISERROR('Cannot delete approved adjustment.', 16, 1);
        RETURN;
    END
    
    -- Delete adjustment items first
    DELETE FROM [Inventory].[InventoryAdjustmentItems]
    WHERE AdjustmentId = @Id AND TenantId = @TenantId;
    
    -- Delete adjustment
    DELETE FROM [Inventory].[InventoryAdjustments]
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- STOCK TRANSFER PROCEDURES
-- =============================================

-- Get Stock Transfer by ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockTransferById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, TransferNumber, TransferDate, FromWarehouseId, ToWarehouseId, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockTransfers]
    WHERE Id = @Id AND TenantId = @TenantId;
END
GO

-- Get Stock Transfer Items by Transfer ID
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockTransferItemsByTransferId]
    @TransferId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sti.Id, sti.TransferId, sti.ItemId, sti.Quantity, sti.UnitCost, sti.TotalCost,
        sti.BatchNumber, sti.SerialNumber, sti.TenantId, sti.CreatedAt, sti.UpdatedAt,
        sti.CreatedBy, sti.UpdatedBy,
        i.ItemCode, i.ItemName, i.UnitOfMeasure
    FROM [Inventory].[StockTransferItems] sti
    INNER JOIN [Inventory].[Items] i ON sti.ItemId = i.Id
    WHERE sti.TransferId = @TransferId AND sti.TenantId = @TenantId
    ORDER BY i.ItemCode;
END
GO

-- Create Stock Transfer
CREATE OR ALTER PROCEDURE [Inventory].[sp_CreateStockTransfer]
    @Id UNIQUEIDENTIFIER,
    @TransferNumber NVARCHAR(50),
    @TransferDate DATETIME2,
    @FromWarehouseId UNIQUEIDENTIFIER,
    @ToWarehouseId UNIQUEIDENTIFIER,
    @Notes NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Inventory].[StockTransfers] (
        Id, TransferNumber, TransferDate, FromWarehouseId, ToWarehouseId, Status,
        TotalValue, Notes, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @TransferNumber, @TransferDate, @FromWarehouseId, @ToWarehouseId, 'Draft',
        0, @Notes, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Get All Stock Transfers
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetAllStockTransfers]
    @TenantId UNIQUEIDENTIFIER,
    @FromWarehouseId UNIQUEIDENTIFIER = NULL,
    @ToWarehouseId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, TransferNumber, TransferDate, FromWarehouseId, ToWarehouseId, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockTransfers]
    WHERE TenantId = @TenantId
        AND (@FromWarehouseId IS NULL OR FromWarehouseId = @FromWarehouseId)
        AND (@ToWarehouseId IS NULL OR ToWarehouseId = @ToWarehouseId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR TransferDate >= @StartDate)
        AND (@EndDate IS NULL OR TransferDate <= @EndDate)
    ORDER BY TransferDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Inventory].[StockTransfers]
    WHERE TenantId = @TenantId
        AND (@FromWarehouseId IS NULL OR FromWarehouseId = @FromWarehouseId)
        AND (@ToWarehouseId IS NULL OR ToWarehouseId = @ToWarehouseId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR TransferDate >= @StartDate)
        AND (@EndDate IS NULL OR TransferDate <= @EndDate);
END
GO

-- Get Stock Transfers by From Warehouse
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockTransfersByFromWarehouse]
    @FromWarehouseId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, TransferNumber, TransferDate, FromWarehouseId, ToWarehouseId, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockTransfers]
    WHERE FromWarehouseId = @FromWarehouseId
        AND TenantId = @TenantId
        AND (@Status IS NULL OR Status = @Status)
    ORDER BY TransferDate DESC;
END
GO

-- Get Stock Transfers by To Warehouse
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockTransfersByToWarehouse]
    @ToWarehouseId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, TransferNumber, TransferDate, FromWarehouseId, ToWarehouseId, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockTransfers]
    WHERE ToWarehouseId = @ToWarehouseId
        AND TenantId = @TenantId
        AND (@Status IS NULL OR Status = @Status)
    ORDER BY TransferDate DESC;
END
GO

-- Get Stock Transfers by Status
CREATE OR ALTER PROCEDURE [Inventory].[sp_GetStockTransfersByStatus]
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, TransferNumber, TransferDate, FromWarehouseId, ToWarehouseId, Status,
        TotalValue, Notes, ApprovedBy, ApprovedAt, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy
    FROM [Inventory].[StockTransfers]
    WHERE Status = @Status
        AND TenantId = @TenantId
    ORDER BY TransferDate DESC;
END
GO

-- Update Stock Transfer
CREATE OR ALTER PROCEDURE [Inventory].[sp_UpdateStockTransfer]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @TransferNumber NVARCHAR(50) = NULL,
    @TransferDate DATETIME2 = NULL,
    @FromWarehouseId UNIQUEIDENTIFIER = NULL,
    @ToWarehouseId UNIQUEIDENTIFIER = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate transfer exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[StockTransfers] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Stock transfer not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if transfer is completed
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Inventory].[StockTransfers] WHERE Id = @Id;
    
    IF @Status = 'Completed'
    BEGIN
        RAISERROR('Cannot update completed transfer.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[StockTransfers]
    SET 
        TransferNumber = ISNULL(@TransferNumber, TransferNumber),
        TransferDate = ISNULL(@TransferDate, TransferDate),
        FromWarehouseId = ISNULL(@FromWarehouseId, FromWarehouseId),
        ToWarehouseId = ISNULL(@ToWarehouseId, ToWarehouseId),
        Notes = ISNULL(@Notes, Notes),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Stock Transfer
CREATE OR ALTER PROCEDURE [Inventory].[sp_DeleteStockTransfer]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate transfer exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[StockTransfers] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Stock transfer not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if transfer is completed
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Inventory].[StockTransfers] WHERE Id = @Id;
    
    IF @Status = 'Completed'
    BEGIN
        RAISERROR('Cannot delete completed transfer.', 16, 1);
        RETURN;
    END
    
    -- Delete transfer items first
    DELETE FROM [Inventory].[StockTransferItems]
    WHERE TransferId = @Id AND TenantId = @TenantId;
    
    -- Delete transfer
    DELETE FROM [Inventory].[StockTransfers]
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Approve Stock Transfer
CREATE OR ALTER PROCEDURE [Inventory].[sp_ApproveStockTransfer]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ApprovedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate transfer exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[StockTransfers] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Stock transfer not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[StockTransfers]
    SET 
        Status = 'Approved',
        ApprovedBy = @ApprovedBy,
        ApprovedAt = GETUTCDATE(),
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Complete Stock Transfer
CREATE OR ALTER PROCEDURE [Inventory].[sp_CompleteStockTransfer]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate transfer exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Inventory].[StockTransfers] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Stock transfer not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if transfer is approved
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Inventory].[StockTransfers] WHERE Id = @Id;
    
    IF @Status != 'Approved'
    BEGIN
        RAISERROR('Only approved transfers can be completed.', 16, 1);
        RETURN;
    END
    
    UPDATE [Inventory].[StockTransfers]
    SET 
        Status = 'Completed',
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- UNITS OF MEASURE PROCEDURES
-- =============================================

-- Get All Units of Measure
CREATE OR ALTER PROCEDURE Inventory.sp_GetAllUnitsOfMeasure
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        uom.Id,
        uom.UnitCode,
        uom.UnitName,
        uom.UnitType,
        uom.BaseUnitId,
        uom.ConversionFactor,
        uom.IsActive,
        uom.CreatedAt,
        uom.UpdatedAt,
        uom.CreatedBy,
        uom.UpdatedBy,
        baseUnit.UnitName as BaseUnitName
    FROM Inventory.UnitsOfMeasure uom
    LEFT JOIN Inventory.UnitsOfMeasure baseUnit ON uom.BaseUnitId = baseUnit.Id
    WHERE uom.TenantId = @TenantId
    AND (@IsActive IS NULL OR uom.IsActive = @IsActive)
    ORDER BY uom.UnitName;
END
GO

-- Create Unit of Measure
CREATE OR ALTER PROCEDURE Inventory.sp_CreateUnitOfMeasure
    @UnitCode NVARCHAR(50),
    @UnitName NVARCHAR(100),
    @UnitType NVARCHAR(50),
    @BaseUnitId UNIQUEIDENTIFIER = NULL,
    @ConversionFactor DECIMAL(18,6) = 1,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.UnitsOfMeasure (
        Id, UnitCode, UnitName, UnitType, BaseUnitId, ConversionFactor, TenantId, CreatedBy
    )
    VALUES (
        @Id, @UnitCode, @UnitName, @UnitType, @BaseUnitId, @ConversionFactor, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Get Unit of Measure by ID
CREATE OR ALTER PROCEDURE Inventory.sp_GetUnitOfMeasureById
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        uom.Id,
        uom.UnitCode,
        uom.UnitName,
        uom.UnitType,
        uom.BaseUnitId,
        uom.ConversionFactor,
        uom.IsActive,
        uom.TenantId,
        uom.CreatedAt,
        uom.UpdatedAt,
        uom.CreatedBy,
        uom.UpdatedBy,
        base.UnitName as BaseUnitName
    FROM Inventory.UnitsOfMeasure uom
    LEFT JOIN Inventory.UnitsOfMeasure base ON uom.BaseUnitId = base.Id
    WHERE uom.Id = @Id 
    AND uom.TenantId = @TenantId
    AND uom.IsActive = 1;
END
GO

-- Update Unit of Measure
CREATE OR ALTER PROCEDURE Inventory.sp_UpdateUnitOfMeasure
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @UnitCode NVARCHAR(50) = NULL,
    @UnitName NVARCHAR(100) = NULL,
    @UnitType NVARCHAR(50) = NULL,
    @BaseUnitId UNIQUEIDENTIFIER = NULL,
    @ConversionFactor DECIMAL(18,6) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate unit exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM Inventory.UnitsOfMeasure WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Unit of measure not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE Inventory.UnitsOfMeasure
    SET 
        UnitCode = ISNULL(@UnitCode, UnitCode),
        UnitName = ISNULL(@UnitName, UnitName),
        UnitType = ISNULL(@UnitType, UnitType),
        BaseUnitId = @BaseUnitId, -- Allow NULL explicitly
        ConversionFactor = ISNULL(@ConversionFactor, ConversionFactor),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Unit of Measure (Soft Delete)
CREATE OR ALTER PROCEDURE Inventory.sp_DeleteUnitOfMeasure
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate unit exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM Inventory.UnitsOfMeasure WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Unit of measure not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if unit is used by items
    IF EXISTS (SELECT 1 FROM Inventory.Items WHERE UnitOfMeasure = (SELECT UnitCode FROM Inventory.UnitsOfMeasure WHERE Id = @Id) AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Cannot delete unit of measure that is used by items.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE Inventory.UnitsOfMeasure
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- ITEM PRICING PROCEDURES
-- =============================================

-- Get Item Pricing
CREATE OR ALTER PROCEDURE Inventory.sp_GetItemPricing
    @ItemId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PriceListName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ip.Id,
        ip.ItemId,
        ip.PriceListName,
        ip.UnitPrice,
        ip.MinQuantity,
        ip.MaxQuantity,
        ip.ValidFrom,
        ip.ValidTo,
        ip.IsActive,
        ip.CreatedAt,
        ip.UpdatedAt,
        ip.CreatedBy,
        ip.UpdatedBy
    FROM Inventory.ItemPricing ip
    INNER JOIN Inventory.Items i ON ip.ItemId = i.Id
    WHERE i.Id = @ItemId 
    AND i.TenantId = @TenantId
    AND (@PriceListName IS NULL OR ip.PriceListName = @PriceListName)
    AND ip.IsActive = 1
    ORDER BY ip.ValidFrom DESC;
END
GO

-- Add Item Pricing
CREATE OR ALTER PROCEDURE Inventory.sp_AddItemPricing
    @ItemId UNIQUEIDENTIFIER,
    @PriceListName NVARCHAR(100),
    @UnitPrice DECIMAL(18,2),
    @MinQuantity DECIMAL(18,2) = 1,
    @MaxQuantity DECIMAL(18,2) = NULL,
    @ValidFrom DATETIME2,
    @ValidTo DATETIME2 = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.ItemPricing (
        Id, ItemId, PriceListName, UnitPrice, MinQuantity, MaxQuantity,
        ValidFrom, ValidTo, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @PriceListName, @UnitPrice, @MinQuantity, @MaxQuantity,
        @ValidFrom, @ValidTo, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- ITEM GROUPS PROCEDURES
-- =============================================

-- Get All Item Groups
CREATE OR ALTER PROCEDURE Inventory.sp_GetAllItemGroups
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ig.Id,
        ig.GroupCode,
        ig.GroupName,
        ig.ParentGroupId,
        ig.Description,
        ig.IsActive,
        ig.CreatedAt,
        ig.UpdatedAt,
        ig.CreatedBy,
        ig.UpdatedBy,
        parentGroup.GroupName as ParentGroupName
    FROM Inventory.ItemGroups ig
    LEFT JOIN Inventory.ItemGroups parentGroup ON ig.ParentGroupId = parentGroup.Id
    WHERE ig.TenantId = @TenantId
    AND (@IsActive IS NULL OR ig.IsActive = @IsActive)
    ORDER BY ig.GroupName;
END
GO

-- Create Item Group
CREATE OR ALTER PROCEDURE Inventory.sp_CreateItemGroup
    @GroupCode NVARCHAR(50),
    @GroupName NVARCHAR(200),
    @ParentGroupId UNIQUEIDENTIFIER = NULL,
    @Description NVARCHAR(500) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.ItemGroups (
        Id, GroupCode, GroupName, ParentGroupId, Description, TenantId, CreatedBy
    )
    VALUES (
        @Id, @GroupCode, @GroupName, @ParentGroupId, @Description, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- ITEM ATTRIBUTES PROCEDURES
-- =============================================

-- Get All Item Attributes
CREATE OR ALTER PROCEDURE Inventory.sp_GetAllItemAttributes
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ia.Id,
        ia.AttributeName,
        ia.AttributeType,
        ia.IsRequired,
        ia.IsActive,
        ia.CreatedAt,
        ia.UpdatedAt,
        ia.CreatedBy,
        ia.UpdatedBy
    FROM Inventory.ItemAttributes ia
    WHERE ia.TenantId = @TenantId
    AND (@IsActive IS NULL OR ia.IsActive = @IsActive)
    ORDER BY ia.AttributeName;
END
GO

-- Create Item Attribute
CREATE OR ALTER PROCEDURE Inventory.sp_CreateItemAttribute
    @AttributeName NVARCHAR(100),
    @AttributeType NVARCHAR(50),
    @IsRequired BIT = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.ItemAttributes (
        Id, AttributeName, AttributeType, IsRequired, TenantId, CreatedBy
    )
    VALUES (
        @Id, @AttributeName, @AttributeType, @IsRequired, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- ITEM VARIANTS PROCEDURES
-- =============================================

-- Get Item Variants
CREATE OR ALTER PROCEDURE Inventory.sp_GetItemVariants
    @ItemId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        iv.Id,
        iv.ItemId,
        iv.VariantCode,
        iv.VariantName,
        iv.SKU,
        iv.CostPrice,
        iv.SellingPrice,
        iv.IsActive,
        iv.CreatedAt,
        iv.UpdatedAt,
        iv.CreatedBy,
        iv.UpdatedBy
    FROM Inventory.ItemVariants iv
    INNER JOIN Inventory.Items i ON iv.ItemId = i.Id
    WHERE i.Id = @ItemId 
    AND i.TenantId = @TenantId
    AND iv.IsActive = 1
    ORDER BY iv.VariantName;
END
GO

-- Create Item Variant
CREATE OR ALTER PROCEDURE Inventory.sp_CreateItemVariant
    @ItemId UNIQUEIDENTIFIER,
    @VariantCode NVARCHAR(50),
    @VariantName NVARCHAR(200),
    @SKU NVARCHAR(100) = NULL,
    @CostPrice DECIMAL(18,2) = 0,
    @SellingPrice DECIMAL(18,2) = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.ItemVariants (
        Id, ItemId, VariantCode, VariantName, SKU, CostPrice, SellingPrice, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @VariantCode, @VariantName, @SKU, @CostPrice, @SellingPrice, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- BARCODES PROCEDURES
-- =============================================

-- Get Item Barcodes
CREATE OR ALTER PROCEDURE Inventory.sp_GetItemBarcodes
    @ItemId UNIQUEIDENTIFIER = NULL,
    @VariantId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.Id,
        b.ItemId,
        b.VariantId,
        b.Barcode,
        b.BarcodeType,
        b.IsPrimary,
        b.IsActive,
        b.CreatedAt,
        b.UpdatedAt,
        b.CreatedBy,
        b.UpdatedBy
    FROM Inventory.Barcodes b
    INNER JOIN Inventory.Items i ON b.ItemId = i.Id
    WHERE i.TenantId = @TenantId
    AND (@ItemId IS NULL OR b.ItemId = @ItemId)
    AND (@VariantId IS NULL OR b.VariantId = @VariantId)
    AND b.IsActive = 1
    ORDER BY b.IsPrimary DESC, b.Barcode;
END
GO

-- Add Barcode
CREATE OR ALTER PROCEDURE Inventory.sp_AddBarcode
    @ItemId UNIQUEIDENTIFIER = NULL,
    @VariantId UNIQUEIDENTIFIER = NULL,
    @Barcode NVARCHAR(100),
    @BarcodeType NVARCHAR(50),
    @IsPrimary BIT = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    -- If this is set as primary, unset other primary barcodes for the same item/variant
    IF @IsPrimary = 1
    BEGIN
        IF @ItemId IS NOT NULL
        BEGIN
            UPDATE Inventory.Barcodes
            SET IsPrimary = 0
            WHERE ItemId = @ItemId;
        END
        ELSE IF @VariantId IS NOT NULL
        BEGIN
            UPDATE Inventory.Barcodes
            SET IsPrimary = 0
            WHERE VariantId = @VariantId;
        END
    END
    
    INSERT INTO Inventory.Barcodes (
        Id, ItemId, VariantId, Barcode, BarcodeType, IsPrimary, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @VariantId, @Barcode, @BarcodeType, @IsPrimary, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- BATCH NUMBERS PROCEDURES
-- =============================================

-- Get Item Batch Numbers
CREATE OR ALTER PROCEDURE Inventory.sp_GetItemBatchNumbers
    @ItemId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        bn.Id,
        bn.ItemId,
        bn.BatchNumber,
        bn.ManufacturingDate,
        bn.ExpiryDate,
        bn.Quantity,
        bn.IsActive,
        bn.CreatedAt,
        bn.UpdatedAt,
        bn.CreatedBy,
        bn.UpdatedBy
    FROM Inventory.BatchNumbers bn
    INNER JOIN Inventory.Items i ON bn.ItemId = i.Id
    WHERE i.Id = @ItemId 
    AND i.TenantId = @TenantId
    AND bn.IsActive = 1
    ORDER BY bn.BatchNumber;
END
GO

-- Add Batch Number
CREATE OR ALTER PROCEDURE Inventory.sp_AddBatchNumber
    @ItemId UNIQUEIDENTIFIER,
    @BatchNumber NVARCHAR(100),
    @ManufacturingDate DATETIME2 = NULL,
    @ExpiryDate DATETIME2 = NULL,
    @Quantity DECIMAL(18,4) = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.BatchNumbers (
        Id, ItemId, BatchNumber, ManufacturingDate, ExpiryDate, Quantity, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @BatchNumber, @ManufacturingDate, @ExpiryDate, @Quantity, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- SERIAL NUMBERS PROCEDURES
-- =============================================

-- Get Item Serial Numbers
CREATE OR ALTER PROCEDURE Inventory.sp_GetItemSerialNumbers
    @ItemId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sn.Id,
        sn.ItemId,
        sn.SerialNumber,
        sn.Status,
        sn.Location,
        sn.IsActive,
        sn.CreatedAt,
        sn.UpdatedAt,
        sn.CreatedBy,
        sn.UpdatedBy
    FROM Inventory.SerialNumbers sn
    INNER JOIN Inventory.Items i ON sn.ItemId = i.Id
    WHERE i.Id = @ItemId 
    AND i.TenantId = @TenantId
    AND (@Status IS NULL OR sn.Status = @Status)
    AND sn.IsActive = 1
    ORDER BY sn.SerialNumber;
END
GO

-- Add Serial Number
CREATE OR ALTER PROCEDURE Inventory.sp_AddSerialNumber
    @ItemId UNIQUEIDENTIFIER,
    @SerialNumber NVARCHAR(100),
    @Status NVARCHAR(50) = 'Available',
    @Location NVARCHAR(200) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.SerialNumbers (
        Id, ItemId, SerialNumber, Status, Location, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @SerialNumber, @Status, @Location, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- REORDER RULES PROCEDURES
-- =============================================

-- Get Item Reorder Rules
CREATE OR ALTER PROCEDURE Inventory.sp_GetItemReorderRules
    @ItemId UNIQUEIDENTIFIER = NULL,
    @WarehouseId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rr.Id,
        rr.ItemId,
        rr.WarehouseId,
        rr.MinStockLevel,
        rr.MaxStockLevel,
        rr.ReorderPoint,
        rr.ReorderQuantity,
        rr.LeadTimeDays,
        rr.IsActive,
        rr.CreatedAt,
        rr.UpdatedAt,
        rr.CreatedBy,
        rr.UpdatedBy,
        i.ItemName,
        w.WarehouseName
    FROM Inventory.ReorderRules rr
    INNER JOIN Inventory.Items i ON rr.ItemId = i.Id
    INNER JOIN Inventory.Warehouses w ON rr.WarehouseId = w.Id
    WHERE i.TenantId = @TenantId
    AND (@ItemId IS NULL OR rr.ItemId = @ItemId)
    AND (@WarehouseId IS NULL OR rr.WarehouseId = @WarehouseId)
    AND rr.IsActive = 1
    ORDER BY i.ItemName, w.WarehouseName;
END
GO

-- Create Reorder Rule
CREATE OR ALTER PROCEDURE Inventory.sp_CreateReorderRule
    @ItemId UNIQUEIDENTIFIER,
    @WarehouseId UNIQUEIDENTIFIER,
    @MinStockLevel DECIMAL(18,4),
    @MaxStockLevel DECIMAL(18,4),
    @ReorderPoint DECIMAL(18,4),
    @ReorderQuantity DECIMAL(18,4),
    @LeadTimeDays INT = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.ReorderRules (
        Id, ItemId, WarehouseId, MinStockLevel, MaxStockLevel,
        ReorderPoint, ReorderQuantity, LeadTimeDays, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ItemId, @WarehouseId, @MinStockLevel, @MaxStockLevel,
        @ReorderPoint, @ReorderQuantity, @LeadTimeDays, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- INVENTORY VALUATION METHODS PROCEDURES
-- =============================================

-- Get All Inventory Valuation Methods
CREATE OR ALTER PROCEDURE Inventory.sp_GetAllInventoryValuationMethods
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ivm.Id,
        ivm.MethodName,
        ivm.MethodCode,
        ivm.Description,
        ivm.IsActive,
        ivm.CreatedAt,
        ivm.UpdatedAt,
        ivm.CreatedBy,
        ivm.UpdatedBy
    FROM Inventory.InventoryValuationMethods ivm
    WHERE ivm.TenantId = @TenantId
    AND (@IsActive IS NULL OR ivm.IsActive = @IsActive)
    ORDER BY ivm.MethodName;
END
GO

-- Create Inventory Valuation Method
CREATE OR ALTER PROCEDURE Inventory.sp_CreateInventoryValuationMethod
    @MethodName NVARCHAR(100),
    @MethodCode NVARCHAR(50),
    @Description NVARCHAR(500) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Inventory.InventoryValuationMethods (
        Id, MethodName, MethodCode, Description, TenantId, CreatedBy
    )
    VALUES (
        @Id, @MethodName, @MethodCode, @Description, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

PRINT 'Inventory Module stored procedures created successfully!'