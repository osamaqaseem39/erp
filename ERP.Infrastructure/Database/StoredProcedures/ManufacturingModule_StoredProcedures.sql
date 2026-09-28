-- =============================================
-- Manufacturing Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Manufacturing Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Manufacturing')
BEGIN
    EXEC('CREATE SCHEMA [Manufacturing]')
END
GO

-- =============================================
-- WORK CENTER MANAGEMENT PROCEDURES
-- =============================================

-- Get Work Center by ID
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetWorkCenterById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        wc.Id,
        wc.CompanyId,
        wc.TenantId,
        wc.WorkCenterCode,
        wc.WorkCenterName,
        wc.Description,
        wc.WorkCenterType,
        wc.Capacity,
        wc.CapacityUnit,
        wc.IsActive,
        wc.Notes,
        wc.CreatedAt,
        wc.UpdatedAt,
        wc.CreatedBy,
        wc.UpdatedBy
    FROM [Manufacturing].WorkCenters wc
    WHERE wc.Id = @Id
    AND wc.TenantId = @TenantID;
END
GO

-- Get All Work Centers
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetAllWorkCenters]
    @TenantID UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        wc.Id,
        wc.CompanyId,
        wc.WorkCenterCode,
        wc.WorkCenterName,
        wc.Description,
        wc.WorkCenterType,
        wc.Capacity,
        wc.CapacityUnit,
        wc.IsActive,
        wc.CreatedAt,
        wc.UpdatedAt
    FROM [Manufacturing].WorkCenters wc
    WHERE wc.TenantId = @TenantID
    AND (@CompanyId IS NULL OR wc.CompanyId = @CompanyId)
    AND (@IsActive IS NULL OR wc.IsActive = @IsActive)
    ORDER BY wc.WorkCenterCode
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM [Manufacturing].WorkCenters wc
    WHERE wc.TenantId = @TenantID
    AND (@CompanyId IS NULL OR wc.CompanyId = @CompanyId)
    AND (@IsActive IS NULL OR wc.IsActive = @IsActive);
END
GO

-- Create Work Center
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_CreateWorkCenter]
    @Id UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @WorkCenterCode NVARCHAR(50),
    @WorkCenterName NVARCHAR(200),
    @Description NVARCHAR(500) = NULL,
    @WorkCenterType NVARCHAR(50),
    @Capacity DECIMAL(18,2),
    @CapacityUnit NVARCHAR(20) = 'Hours',
    @IsActive BIT = 1,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Manufacturing].WorkCenters (
        Id, CompanyId, TenantId, WorkCenterCode, WorkCenterName, Description,
        WorkCenterType, Capacity, CapacityUnit, IsActive, Notes, CreatedBy
    )
    VALUES (
        @Id, @CompanyId, @TenantID, @WorkCenterCode, @WorkCenterName, @Description,
        @WorkCenterType, @Capacity, @CapacityUnit, @IsActive, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Work Center
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_UpdateWorkCenter]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @WorkCenterCode NVARCHAR(50),
    @WorkCenterName NVARCHAR(200),
    @Description NVARCHAR(500) = NULL,
    @WorkCenterType NVARCHAR(50),
    @Capacity DECIMAL(18,2),
    @CapacityUnit NVARCHAR(20) = 'Hours',
    @IsActive BIT = 1,
    @Notes NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [Manufacturing].WorkCenters
    SET 
        WorkCenterCode = @WorkCenterCode,
        WorkCenterName = @WorkCenterName,
        Description = @Description,
        WorkCenterType = @WorkCenterType,
        Capacity = @Capacity,
        CapacityUnit = @CapacityUnit,
        IsActive = @IsActive,
        Notes = @Notes,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- Delete Work Center
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_DeleteWorkCenter]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate work center exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Manufacturing].WorkCenters WHERE Id = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Work center not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if work center is used in production orders
    IF EXISTS (SELECT 1 FROM [Manufacturing].ProductionOrders WHERE WorkCenterId = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Cannot delete work center that is used in production orders.', 16, 1);
        RETURN;
    END
    
    -- Soft delete - set IsActive to 0
    UPDATE [Manufacturing].WorkCenters
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- PRODUCTION ORDER MANAGEMENT PROCEDURES
-- =============================================

-- Get Production Order by ID
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetProductionOrderById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        po.Id,
        po.CompanyId,
        po.TenantId,
        po.ProductionOrderNumber,
        po.OrderNumber,
        po.ItemId,
        po.WorkCenterId,
        po.Quantity,
        po.StartDate,
        po.EndDate,
        po.PlannedStartDate,
        po.PlannedEndDate,
        po.ActualStartDate,
        po.ActualEndDate,
        po.Status,
        po.Priority,
        po.CompletedQuantity,
        po.Notes,
        po.CancellationReason,
        po.CreatedAt,
        po.UpdatedAt,
        po.CreatedBy,
        po.UpdatedBy,
        i.ItemName,
        i.ItemCode,
        wc.WorkCenterName,
        wc.WorkCenterCode
    FROM [Manufacturing].ProductionOrders po
    LEFT JOIN [Inventory].Items i ON po.ItemId = i.Id
    LEFT JOIN [Manufacturing].WorkCenters wc ON po.WorkCenterId = wc.Id
    WHERE po.Id = @Id
    AND po.TenantId = @TenantID;
END
GO

-- Get All Production Orders
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetAllProductionOrders]
    @TenantID UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        po.Id,
        po.ProductionOrderNumber,
        po.OrderNumber,
        po.ItemId,
        po.WorkCenterId,
        po.Quantity,
        po.CompletedQuantity,
        po.Status,
        po.Priority,
        po.PlannedStartDate,
        po.PlannedEndDate,
        po.ActualStartDate,
        po.ActualEndDate,
        po.CreatedAt,
        i.ItemName,
        i.ItemCode,
        wc.WorkCenterName
    FROM [Manufacturing].ProductionOrders po
    LEFT JOIN [Inventory].Items i ON po.ItemId = i.Id
    LEFT JOIN [Manufacturing].WorkCenters wc ON po.WorkCenterId = wc.Id
    WHERE po.TenantId = @TenantID
    AND (@CompanyId IS NULL OR po.CompanyId = @CompanyId)
    AND (@Status IS NULL OR po.Status = @Status)
    ORDER BY po.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM [Manufacturing].ProductionOrders po
    WHERE po.TenantId = @TenantID
    AND (@CompanyId IS NULL OR po.CompanyId = @CompanyId)
    AND (@Status IS NULL OR po.Status = @Status);
END
GO

-- Create Production Order
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_CreateProductionOrder]
    @Id UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @ProductionOrderNumber NVARCHAR(50),
    @OrderNumber NVARCHAR(50) = NULL,
    @ItemId UNIQUEIDENTIFIER = NULL,
    @WorkCenterId UNIQUEIDENTIFIER = NULL,
    @Quantity DECIMAL(18,2),
    @PlannedStartDate DATETIME2 = NULL,
    @PlannedEndDate DATETIME2 = NULL,
    @Status NVARCHAR(50) = 'Planned',
    @Priority NVARCHAR(50) = 'Medium',
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Manufacturing].ProductionOrders (
        Id, CompanyId, TenantId, ProductionOrderNumber, OrderNumber, ItemId,
        WorkCenterId, Quantity, PlannedStartDate, PlannedEndDate, Status,
        Priority, CompletedQuantity, Notes, CreatedBy
    )
    VALUES (
        @Id, @CompanyId, @TenantID, @ProductionOrderNumber, @OrderNumber, @ItemId,
        @WorkCenterId, @Quantity, @PlannedStartDate, @PlannedEndDate, @Status,
        @Priority, 0, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Production Order Status
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_UpdateProductionOrderStatus]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ActualStartDate DATETIME2 = NULL,
    @ActualEndDate DATETIME2 = NULL,
    @CompletedQuantity DECIMAL(18,2) = NULL,
    @CancellationReason NVARCHAR(500) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [Manufacturing].ProductionOrders
    SET 
        Status = @Status,
        ActualStartDate = ISNULL(@ActualStartDate, ActualStartDate),
        ActualEndDate = ISNULL(@ActualEndDate, ActualEndDate),
        CompletedQuantity = ISNULL(@CompletedQuantity, CompletedQuantity),
        CancellationReason = @CancellationReason,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- Update Production Order
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_UpdateProductionOrder]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ItemId UNIQUEIDENTIFIER = NULL,
    @WorkCenterId UNIQUEIDENTIFIER = NULL,
    @Quantity DECIMAL(18,2) = NULL,
    @PlannedStartDate DATETIME2 = NULL,
    @PlannedEndDate DATETIME2 = NULL,
    @Priority NVARCHAR(50) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate production order exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Manufacturing].ProductionOrders WHERE Id = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Production order not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if production order is started or completed
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Manufacturing].ProductionOrders WHERE Id = @Id;
    
    IF @Status IN ('Started', 'Completed', 'Cancelled')
    BEGIN
        RAISERROR('Cannot update production order that is started, completed, or cancelled.', 16, 1);
        RETURN;
    END
    
    UPDATE [Manufacturing].ProductionOrders
    SET 
        ItemId = ISNULL(@ItemId, ItemId),
        WorkCenterId = ISNULL(@WorkCenterId, WorkCenterId),
        Quantity = ISNULL(@Quantity, Quantity),
        PlannedStartDate = ISNULL(@PlannedStartDate, PlannedStartDate),
        PlannedEndDate = ISNULL(@PlannedEndDate, PlannedEndDate),
        Priority = ISNULL(@Priority, Priority),
        Notes = ISNULL(@Notes, Notes),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Production Order
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_DeleteProductionOrder]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate production order exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Manufacturing].ProductionOrders WHERE Id = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Production order not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if production order is started or completed
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Manufacturing].ProductionOrders WHERE Id = @Id;
    
    IF @Status IN ('Started', 'Completed')
    BEGIN
        RAISERROR('Cannot delete production order that is started or completed.', 16, 1);
        RETURN;
    END
    
    -- Delete production order lines first
    DELETE FROM [Manufacturing].ProductionOrderLines
    WHERE ProductionOrderId = @Id;
    
    -- Delete production order
    DELETE FROM [Manufacturing].ProductionOrders
    WHERE Id = @Id AND TenantId = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Start Production Order
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_StartProductionOrder]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @StartedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate production order exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Manufacturing].ProductionOrders WHERE Id = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Production order not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if production order is already started or completed
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Manufacturing].ProductionOrders WHERE Id = @Id;
    
    IF @Status = 'Started'
    BEGIN
        RAISERROR('Production order is already started.', 16, 1);
        RETURN;
    END
    
    IF @Status = 'Completed'
    BEGIN
        RAISERROR('Cannot start a completed production order.', 16, 1);
        RETURN;
    END
    
    UPDATE [Manufacturing].ProductionOrders
    SET 
        Status = 'Started',
        ActualStartDate = GETUTCDATE(),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @StartedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Complete Production Order
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_CompleteProductionOrder]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @CompletedQuantity DECIMAL(18,2) = NULL,
    @CompletedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate production order exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Manufacturing].ProductionOrders WHERE Id = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Production order not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if production order is started
    DECLARE @Status NVARCHAR(50);
    DECLARE @OrderQuantity DECIMAL(18,2);
    SELECT @Status = Status, @OrderQuantity = Quantity FROM [Manufacturing].ProductionOrders WHERE Id = @Id;
    
    IF @Status != 'Started'
    BEGIN
        RAISERROR('Production order must be started before it can be completed.', 16, 1);
        RETURN;
    END
    
    -- Use provided completed quantity or default to order quantity
    DECLARE @FinalCompletedQuantity DECIMAL(18,2) = ISNULL(@CompletedQuantity, @OrderQuantity);
    
    UPDATE [Manufacturing].ProductionOrders
    SET 
        Status = 'Completed',
        ActualEndDate = GETUTCDATE(),
        CompletedQuantity = @FinalCompletedQuantity,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @CompletedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Cancel Production Order
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_CancelProductionOrder]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @CancellationReason NVARCHAR(500),
    @CancelledBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate production order exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Manufacturing].ProductionOrders WHERE Id = @Id AND TenantId = @TenantID)
    BEGIN
        RAISERROR('Production order not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if production order is already completed
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Manufacturing].ProductionOrders WHERE Id = @Id;
    
    IF @Status = 'Completed'
    BEGIN
        RAISERROR('Cannot cancel a completed production order.', 16, 1);
        RETURN;
    END
    
    UPDATE [Manufacturing].ProductionOrders
    SET 
        Status = 'Cancelled',
        CancellationReason = @CancellationReason,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @CancelledBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Get Production Orders by Work Center
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetProductionOrdersByWorkCenter]
    @WorkCenterId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        po.Id,
        po.ProductionOrderNumber,
        po.OrderNumber,
        po.ItemId,
        po.WorkCenterId,
        po.Quantity,
        po.CompletedQuantity,
        po.Status,
        po.Priority,
        po.PlannedStartDate,
        po.PlannedEndDate,
        po.ActualStartDate,
        po.ActualEndDate,
        po.CreatedAt,
        i.ItemName,
        i.ItemCode,
        wc.WorkCenterName
    FROM [Manufacturing].ProductionOrders po
    LEFT JOIN [Inventory].Items i ON po.ItemId = i.Id
    LEFT JOIN [Manufacturing].WorkCenters wc ON po.WorkCenterId = wc.Id
    WHERE po.WorkCenterId = @WorkCenterId
        AND po.TenantId = @TenantID
        AND (@Status IS NULL OR po.Status = @Status)
    ORDER BY po.CreatedAt DESC;
END
GO

-- Get Production Order Lines by Order ID
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetProductionOrderLinesByOrderId]
    @ProductionOrderId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pol.Id,
        pol.ProductionOrderId,
        pol.ItemId,
        pol.Quantity,
        pol.ConsumedQuantity,
        pol.LineType,
        pol.Notes,
        pol.CreatedAt,
        pol.UpdatedAt,
        i.ItemName,
        i.ItemCode
    FROM [Manufacturing].ProductionOrderLines pol
    INNER JOIN [Manufacturing].ProductionOrders po ON pol.ProductionOrderId = po.Id
    LEFT JOIN [Inventory].Items i ON pol.ItemId = i.Id
    WHERE pol.ProductionOrderId = @ProductionOrderId
    AND po.TenantId = @TenantID
    ORDER BY pol.LineType, pol.CreatedAt;
END
GO

-- =============================================
-- PRODUCTION RUN MANAGEMENT PROCEDURES
-- =============================================

-- Get Production Run by ID
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_GetProductionRunById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pr.Id,
        pr.ProductionOrderId,
        pr.RunNumber,
        pr.StartTime,
        pr.EndTime,
        pr.QuantityProduced,
        pr.QuantityRejected,
        pr.Status,
        pr.Notes,
        pr.CreatedAt,
        pr.UpdatedAt,
        pr.CreatedBy,
        pr.UpdatedBy,
        po.ProductionOrderNumber
    FROM [Manufacturing].ProductionRuns pr
    INNER JOIN [Manufacturing].ProductionOrders po ON pr.ProductionOrderId = po.Id
    WHERE pr.Id = @Id
    AND po.TenantId = @TenantID;
END
GO

-- Create Production Run
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_CreateProductionRun]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ProductionOrderId UNIQUEIDENTIFIER,
    @RunNumber NVARCHAR(50),
    @StartTime DATETIME2,
    @Status NVARCHAR(50) = 'InProgress',
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO ProductionRuns (
        Id, ProductionOrderId, RunNumber, StartTime, Status,
        QuantityProduced, QuantityRejected, Notes, CreatedBy
    )
    VALUES (
        @Id, @ProductionOrderId, @RunNumber, @StartTime, @Status,
        0, 0, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Start Production Run
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_StartProductionRun]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @StartTime DATETIME2,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ProductionRuns
    SET 
        StartTime = @StartTime,
        Status = 'InProgress',
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND EXISTS (
        SELECT 1 FROM [Manufacturing].ProductionOrders po
        WHERE po.Id = ProductionRuns.ProductionOrderId
        AND po.TenantId = @TenantID
    );
END
GO

-- Complete Production Run
CREATE OR ALTER PROCEDURE [Manufacturing].[sp_CompleteProductionRun]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @EndTime DATETIME2,
    @QuantityProduced DECIMAL(18,2),
    @QuantityRejected DECIMAL(18,2) = 0,
    @Status NVARCHAR(50) = 'Completed',
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        UPDATE ProductionRuns
        SET 
            EndTime = @EndTime,
            QuantityProduced = @QuantityProduced,
            QuantityRejected = @QuantityRejected,
            Status = @Status,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @UpdatedBy
        WHERE Id = @Id
        AND EXISTS (
            SELECT 1 FROM [Manufacturing].ProductionOrders po
            WHERE po.Id = ProductionRuns.ProductionOrderId
            AND po.TenantId = @TenantID
        );
        
        -- Update Production Order completed quantity
        UPDATE [Manufacturing].ProductionOrders
        SET 
            CompletedQuantity = CompletedQuantity + @QuantityProduced,
            UpdatedAt = GETUTCDATE()
        WHERE Id = (SELECT ProductionOrderId FROM [Manufacturing].ProductionRuns WHERE Id = @Id)
        AND TenantId = @TenantID;
        
        COMMIT TRANSACTION;
        SELECT 1 as Success;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

