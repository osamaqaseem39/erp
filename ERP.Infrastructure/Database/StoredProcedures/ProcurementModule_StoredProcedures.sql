-- =============================================
-- Procurement Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

-- =============================================
-- VENDOR MANAGEMENT PROCEDURES
-- =============================================

-- Get Vendor by Code
CREATE OR ALTER PROCEDURE Procurement.sp_GetVendorByCode
    @VendorCode NVARCHAR(50),
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VendorID,
        v.CompanyID,
        v.TenantID,
        v.VendorCode,
        v.VendorName,
        v.ContactPerson,
        v.PhoneNumber,
        v.Email,
        v.AddressLine1,
        v.AddressLine2,
        v.City,
        v.State,
        v.Country,
        v.PostalCode,
        v.TaxNumber,
        v.PaymentTerms,
        v.CreditLimit,
        v.IsActive,
        v.CreatedAt,
        v.UpdatedAt,
        v.CreatedBy,
        v.UpdatedBy,
        c.CompanyName
    FROM Procurement.Vendors v
    INNER JOIN Core.Companies c ON v.CompanyID = c.Id
    WHERE v.VendorCode = @VendorCode 
    AND v.CompanyID = @CompanyID
    AND v.TenantID = @TenantID
    AND v.IsActive = 1;
END
GO

-- Get Vendor by Email
CREATE OR ALTER PROCEDURE Procurement.sp_GetVendorByEmail
    @Email NVARCHAR(255),
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VendorID,
        v.CompanyID,
        v.TenantID,
        v.VendorCode,
        v.VendorName,
        v.ContactPerson,
        v.PhoneNumber,
        v.Email,
        v.AddressLine1,
        v.AddressLine2,
        v.City,
        v.State,
        v.Country,
        v.PostalCode,
        v.TaxNumber,
        v.PaymentTerms,
        v.CreditLimit,
        v.IsActive,
        v.CreatedAt,
        v.UpdatedAt,
        v.CreatedBy,
        v.UpdatedBy,
        c.CompanyName
    FROM Procurement.Vendors v
    INNER JOIN Core.Companies c ON v.CompanyID = c.Id
    WHERE v.Email = @Email 
    AND v.TenantID = @TenantID
    AND v.IsActive = 1;
END
GO

-- Get Vendor by ID
CREATE OR ALTER PROCEDURE Procurement.sp_GetVendorById
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VendorID,
        v.CompanyID,
        v.TenantID,
        v.VendorCode,
        v.VendorName,
        v.ContactPerson,
        v.PhoneNumber,
        v.Email,
        v.AddressLine1,
        v.AddressLine2,
        v.City,
        v.State,
        v.Country,
        v.PostalCode,
        v.TaxNumber,
        v.PaymentTerms,
        v.CreditLimit,
        v.IsActive,
        v.CreatedAt,
        v.UpdatedAt,
        v.CreatedBy,
        v.UpdatedBy,
        c.CompanyName
    FROM Procurement.Vendors v
    INNER JOIN Core.Companies c ON v.CompanyID = c.Id
    WHERE v.VendorID = @VendorID 
    AND v.TenantID = @TenantID
    AND v.IsActive = 1;
END
GO

-- Get All Vendors with Pagination
CREATE OR ALTER PROCEDURE Procurement.sp_GetAllVendors
    @TenantID UNIQUEIDENTIFIER,
    @CompanyID UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(255) = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SortBy NVARCHAR(50) = 'VendorName',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredVendors AS (
        SELECT 
            v.VendorID,
            v.CompanyID,
            v.TenantID,
            v.VendorCode,
            v.VendorName,
            v.ContactPerson,
            v.PhoneNumber,
            v.Email,
            v.AddressLine1,
            v.City,
            v.State,
            v.Country,
            v.TaxNumber,
            v.PaymentTerms,
            v.CreditLimit,
            v.IsActive,
            v.CreatedAt,
            v.UpdatedAt,
            c.CompanyName,
            ROW_NUMBER() OVER (
                ORDER BY 
                    CASE WHEN @SortBy = 'VendorName' AND @SortDirection = 'ASC' THEN v.VendorName END ASC,
                    CASE WHEN @SortBy = 'VendorName' AND @SortDirection = 'DESC' THEN v.VendorName END DESC,
                    CASE WHEN @SortBy = 'VendorCode' AND @SortDirection = 'ASC' THEN v.VendorCode END ASC,
                    CASE WHEN @SortBy = 'VendorCode' AND @SortDirection = 'DESC' THEN v.VendorCode END DESC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortDirection = 'ASC' THEN v.CreatedAt END ASC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortDirection = 'DESC' THEN v.CreatedAt END DESC
            ) AS RowNum
        FROM Procurement.Vendors v
        INNER JOIN Core.Companies c ON v.CompanyID = c.Id
        WHERE v.TenantID = @TenantID
        AND (@CompanyID IS NULL OR v.CompanyID = @CompanyID)
        AND (@SearchTerm IS NULL OR 
             v.VendorName LIKE '%' + @SearchTerm + '%' OR 
             v.VendorCode LIKE '%' + @SearchTerm + '%' OR
             v.Email LIKE '%' + @SearchTerm + '%')
        AND (@IsActive IS NULL OR v.IsActive = @IsActive)
    )
    SELECT 
        VendorID,
        CompanyID,
        TenantID,
        VendorCode,
        VendorName,
        ContactPerson,
        PhoneNumber,
        Email,
        AddressLine1,
        City,
        State,
        Country,
        TaxNumber,
        PaymentTerms,
        CreditLimit,
        IsActive,
        CreatedAt,
        UpdatedAt,
        CompanyName
    FROM FilteredVendors
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM Procurement.Vendors v
    WHERE v.TenantID = @TenantID
    AND (@CompanyID IS NULL OR v.CompanyID = @CompanyID)
    AND (@SearchTerm IS NULL OR 
         v.VendorName LIKE '%' + @SearchTerm + '%' OR 
         v.VendorCode LIKE '%' + @SearchTerm + '%' OR
         v.Email LIKE '%' + @SearchTerm + '%')
    AND (@IsActive IS NULL OR v.IsActive = @IsActive);
END
GO

-- Create Vendor
CREATE OR ALTER PROCEDURE Procurement.sp_CreateVendor
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @VendorCode NVARCHAR(50),
    @VendorName NVARCHAR(255),
    @ContactPerson NVARCHAR(255) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @Email NVARCHAR(255) = NULL,
    @AddressLine1 NVARCHAR(255) = NULL,
    @AddressLine2 NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL,
    @PostalCode NVARCHAR(20) = NULL,
    @TaxNumber NVARCHAR(50) = NULL,
    @PaymentTerms NVARCHAR(100) = NULL,
    @CreditLimit DECIMAL(18,2) = 0,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @VendorID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.Vendors (
        VendorID, CompanyID, TenantID, VendorCode, VendorName, ContactPerson,
        PhoneNumber, Email, AddressLine1, AddressLine2, City, State, Country,
        PostalCode, TaxNumber, PaymentTerms, CreditLimit, CreatedBy
    )
    VALUES (
        @VendorID, @CompanyID, @TenantID, @VendorCode, @VendorName, @ContactPerson,
        @PhoneNumber, @Email, @AddressLine1, @AddressLine2, @City, @State, @Country,
        @PostalCode, @TaxNumber, @PaymentTerms, @CreditLimit, @CreatedBy
    );
    
    SELECT @VendorID as VendorID;
END
GO

-- Update Vendor
CREATE OR ALTER PROCEDURE Procurement.sp_UpdateVendor
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @VendorCode NVARCHAR(50) = NULL,
    @VendorName NVARCHAR(255) = NULL,
    @ContactPerson NVARCHAR(255) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @Email NVARCHAR(255) = NULL,
    @AddressLine1 NVARCHAR(255) = NULL,
    @AddressLine2 NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL,
    @PostalCode NVARCHAR(20) = NULL,
    @TaxNumber NVARCHAR(50) = NULL,
    @PaymentTerms NVARCHAR(100) = NULL,
    @CreditLimit DECIMAL(18,2) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Procurement.Vendors
    SET 
        VendorCode = ISNULL(@VendorCode, VendorCode),
        VendorName = ISNULL(@VendorName, VendorName),
        ContactPerson = ISNULL(@ContactPerson, ContactPerson),
        PhoneNumber = ISNULL(@PhoneNumber, PhoneNumber),
        Email = ISNULL(@Email, Email),
        AddressLine1 = ISNULL(@AddressLine1, AddressLine1),
        AddressLine2 = ISNULL(@AddressLine2, AddressLine2),
        City = ISNULL(@City, City),
        State = ISNULL(@State, State),
        Country = ISNULL(@Country, Country),
        PostalCode = ISNULL(@PostalCode, PostalCode),
        TaxNumber = ISNULL(@TaxNumber, TaxNumber),
        PaymentTerms = ISNULL(@PaymentTerms, PaymentTerms),
        CreditLimit = ISNULL(@CreditLimit, CreditLimit),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE VendorID = @VendorID 
    AND TenantID = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Vendor
CREATE OR ALTER PROCEDURE Procurement.sp_DeleteVendor
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Soft delete - set IsActive to 0
    UPDATE Procurement.Vendors
    SET IsActive = 0, UpdatedAt = GETUTCDATE()
    WHERE VendorID = @VendorID 
    AND TenantID = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- PURCHASE REQUISITION PROCEDURES
-- =============================================

-- Get Purchase Requisition by ID
CREATE OR ALTER PROCEDURE Procurement.sp_GetPurchaseRequisitionById
    @PRID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pr.PRID,
        pr.CompanyID,
        pr.TenantID,
        pr.RequestedByUserID,
        pr.DepartmentID,
        pr.RequestDate,
        pr.RequiredByDate,
        pr.Status,
        pr.Remarks,
        pr.CreatedAt,
        pr.UpdatedAt,
        pr.CreatedBy,
        pr.UpdatedBy,
        u.FirstName + ' ' + u.LastName as RequestedByName,
        c.CompanyName
    FROM Procurement.PurchaseRequisitions pr
    INNER JOIN Core.Users u ON pr.RequestedByUserID = u.Id
    INNER JOIN Core.Companies c ON pr.CompanyID = c.Id
    WHERE pr.PRID = @PRID 
    AND pr.TenantID = @TenantID;
END
GO

-- Get Purchase Requisition Details
CREATE OR ALTER PROCEDURE Procurement.sp_GetPurchaseRequisitionDetails
    @PRID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        prd.PRDetailID,
        prd.PRID,
        prd.ItemID,
        prd.QuantityRequested,
        prd.UnitOfMeasureID,
        prd.EstimatedUnitPrice,
        prd.Remarks,
        i.ItemName,
        i.ItemCode
        -- uom.UnitName, -- TODO: Create UnitOfMeasures table
        -- uom.UnitSymbol -- TODO: Create UnitOfMeasures table
    FROM Procurement.PurchaseRequisitionDetails prd
    INNER JOIN Inventory.Items i ON prd.ItemID = i.Id
    -- INNER JOIN Inventory.UnitOfMeasures uom ON prd.UnitOfMeasureID = uom.UnitOfMeasureID -- TODO: Create UnitOfMeasures table
    WHERE prd.PRID = @PRID;
END
GO

-- Create Purchase Requisition
CREATE OR ALTER PROCEDURE Procurement.sp_CreatePurchaseRequisition
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @RequestedByUserID UNIQUEIDENTIFIER,
    @DepartmentID UNIQUEIDENTIFIER = NULL,
    @RequiredByDate DATETIME2,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PRID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.PurchaseRequisitions (
        PRID, CompanyID, TenantID, RequestedByUserID, DepartmentID,
        RequiredByDate, Remarks, CreatedBy
    )
    VALUES (
        @PRID, @CompanyID, @TenantID, @RequestedByUserID, @DepartmentID,
        @RequiredByDate, @Remarks, @CreatedBy
    );
    
    SELECT @PRID as PRID;
END
GO

-- Add Purchase Requisition Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddPurchaseRequisitionDetail
    @PRID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @QuantityRequested DECIMAL(18,4),
    @UnitOfMeasureID UNIQUEIDENTIFIER,
    @EstimatedUnitPrice DECIMAL(18,4) = 0,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PRDetailID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.PurchaseRequisitionDetails (
        PRDetailID, PRID, ItemID, QuantityRequested, UnitOfMeasureID,
        EstimatedUnitPrice, Remarks, CreatedBy
    )
    VALUES (
        @PRDetailID, @PRID, @ItemID, @QuantityRequested, @UnitOfMeasureID,
        @EstimatedUnitPrice, @Remarks, @CreatedBy
    );
    
    SELECT @PRDetailID as PRDetailID;
END
GO

-- Update Purchase Requisition Status
CREATE OR ALTER PROCEDURE Procurement.sp_UpdatePurchaseRequisitionStatus
    @PRID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @UpdatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Procurement.PurchaseRequisitions
    SET 
        Status = @Status,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE PRID = @PRID 
    AND TenantID = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- PURCHASE ORDER PROCEDURES
-- =============================================

-- Get Purchase Order by ID
CREATE OR ALTER PROCEDURE Procurement.sp_GetPurchaseOrderById
    @POID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        po.POID,
        po.CompanyID,
        po.TenantID,
        po.RFQID,
        po.VendorID,
        po.OrderDate,
        po.ExpectedDeliveryDate,
        po.Currency,
        po.TotalAmount,
        po.PaymentTerms,
        po.DeliveryTerms,
        po.Status,
        po.CreatedBy,
        po.ApprovedBy,
        po.CreatedAt,
        po.UpdatedAt,
        v.VendorName,
        v.VendorCode,
        c.CompanyName
    FROM Procurement.PurchaseOrders po
    INNER JOIN Procurement.Vendors v ON po.VendorID = v.VendorID
    INNER JOIN Core.Companies c ON po.CompanyID = c.Id
    WHERE po.POID = @POID 
    AND po.TenantID = @TenantID;
END
GO

-- Get Purchase Order Details
CREATE OR ALTER PROCEDURE Procurement.sp_GetPurchaseOrderDetails
    @POID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pod.PODetailID,
        pod.POID,
        pod.ItemID,
        pod.QuantityOrdered,
        pod.UnitPrice,
        pod.Discount,
        pod.TaxAmount,
        pod.NetAmount,
        pod.ReceivedQuantity,
        i.ItemName,
        i.ItemCode,
        -- uom.UnitName, -- TODO: Create UnitOfMeasures table
        -- uom.UnitSymbol -- TODO: Create UnitOfMeasures table
    FROM Procurement.PurchaseOrderDetails pod
    INNER JOIN Inventory.Items i ON pod.ItemID = i.Id
    -- INNER JOIN Inventory.UnitOfMeasures uom ON i.UnitOfMeasureID = uom.UnitOfMeasureID -- TODO: Create UnitOfMeasures table
    WHERE pod.POID = @POID;
END
GO

-- Create Purchase Order
CREATE OR ALTER PROCEDURE Procurement.sp_CreatePurchaseOrder
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @RFQID UNIQUEIDENTIFIER = NULL,
    @VendorID UNIQUEIDENTIFIER,
    @ExpectedDeliveryDate DATETIME2,
    @Currency NVARCHAR(3) = 'USD',
    @PaymentTerms NVARCHAR(100) = NULL,
    @DeliveryTerms NVARCHAR(255) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @POID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.PurchaseOrders (
        POID, CompanyID, TenantID, RFQID, VendorID,
        ExpectedDeliveryDate, Currency, PaymentTerms, DeliveryTerms, CreatedBy
    )
    VALUES (
        @POID, @CompanyID, @TenantID, @RFQID, @VendorID,
        @ExpectedDeliveryDate, @Currency, @PaymentTerms, @DeliveryTerms, @CreatedBy
    );
    
    SELECT @POID as POID;
END
GO

-- Add Purchase Order Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddPurchaseOrderDetail
    @POID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @QuantityOrdered DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @Discount DECIMAL(5,2) = 0,
    @TaxAmount DECIMAL(18,2) = 0,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PODetailID UNIQUEIDENTIFIER = NEWID();
    DECLARE @NetAmount DECIMAL(18,2);
    
    -- Calculate net amount
    SET @NetAmount = (@QuantityOrdered * @UnitPrice) - ((@QuantityOrdered * @UnitPrice * @Discount) / 100) + @TaxAmount;
    
    INSERT INTO Procurement.PurchaseOrderDetails (
        PODetailID, POID, ItemID, QuantityOrdered, UnitPrice,
        Discount, TaxAmount, NetAmount, CreatedBy
    )
    VALUES (
        @PODetailID, @POID, @ItemID, @QuantityOrdered, @UnitPrice,
        @Discount, @TaxAmount, @NetAmount, @CreatedBy
    );
    
    -- Update PO total amount
    UPDATE Procurement.PurchaseOrders
    SET TotalAmount = (
        SELECT SUM(NetAmount) 
        FROM Procurement.PurchaseOrderDetails 
        WHERE POID = @POID
    )
    WHERE POID = @POID;
    
    SELECT @PODetailID as PODetailID;
END
GO

-- Update Purchase Order Status
CREATE OR ALTER PROCEDURE Procurement.sp_UpdatePurchaseOrderStatus
    @POID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ApprovedBy NVARCHAR(255) = NULL,
    @UpdatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Procurement.PurchaseOrders
    SET 
        Status = @Status,
        ApprovedBy = ISNULL(@ApprovedBy, ApprovedBy),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE POID = @POID 
    AND TenantID = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- GOODS RECEIPT NOTE PROCEDURES
-- =============================================

-- Get GRN by ID
CREATE OR ALTER PROCEDURE Procurement.sp_GetGRNById
    @GRNID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        grn.GRNID,
        grn.CompanyID,
        grn.TenantID,
        grn.POID,
        grn.ReceivedDate,
        grn.WarehouseID,
        grn.ReceivedByUserID,
        grn.Status,
        grn.Remarks,
        grn.CreatedAt,
        grn.UpdatedAt,
        po.POID as PurchaseOrderID,
        v.VendorName,
        w.WarehouseName,
        u.FirstName + ' ' + u.LastName as ReceivedByName
    FROM Procurement.GoodsReceiptNote grn
    INNER JOIN Procurement.PurchaseOrders po ON grn.POID = po.POID
    INNER JOIN Procurement.Vendors v ON po.VendorID = v.VendorID
    INNER JOIN Inventory.Warehouses w ON grn.WarehouseID = w.WarehouseID
    INNER JOIN Core.Users u ON grn.ReceivedByUserID = u.Id
    WHERE grn.GRNID = @GRNID 
    AND grn.TenantID = @TenantID;
END
GO

-- Create GRN
CREATE OR ALTER PROCEDURE Procurement.sp_CreateGRN
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @POID UNIQUEIDENTIFIER,
    @WarehouseID UNIQUEIDENTIFIER,
    @ReceivedByUserID UNIQUEIDENTIFIER,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GRNID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.GoodsReceiptNote (
        GRNID, CompanyID, TenantID, POID, WarehouseID,
        ReceivedByUserID, Remarks, CreatedBy
    )
    VALUES (
        @GRNID, @CompanyID, @TenantID, @POID, @WarehouseID,
        @ReceivedByUserID, @Remarks, @CreatedBy
    );
    
    SELECT @GRNID as GRNID;
END
GO

-- Add GRN Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddGRNDetail
    @GRNID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @QuantityReceived DECIMAL(18,4),
    @BatchSerialID UNIQUEIDENTIFIER = NULL,
    @UnitPrice DECIMAL(18,4),
    @TaxAmount DECIMAL(18,2) = 0,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GRNDetailID UNIQUEIDENTIFIER = NEWID();
    DECLARE @TotalAmount DECIMAL(18,2);
    
    -- Calculate total amount
    SET @TotalAmount = (@QuantityReceived * @UnitPrice) + @TaxAmount;
    
    INSERT INTO Procurement.GoodsReceiptNoteDetails (
        GRNDetailID, GRNID, ItemID, QuantityReceived, BatchSerialID,
        UnitPrice, TaxAmount, TotalAmount, CreatedBy
    )
    VALUES (
        @GRNDetailID, @GRNID, @ItemID, @QuantityReceived, @BatchSerialID,
        @UnitPrice, @TaxAmount, @TotalAmount, @CreatedBy
    );
    
    -- Update received quantity in PO details
    UPDATE pod
    SET ReceivedQuantity = pod.ReceivedQuantity + @QuantityReceived
    FROM Procurement.PurchaseOrderDetails pod
    INNER JOIN Procurement.GoodsReceiptNote grn ON pod.POID = grn.POID
    WHERE grn.GRNID = @GRNID 
    AND pod.ItemID = @ItemID;
    
    SELECT @GRNDetailID as GRNDetailID;
END
GO

-- Post GRN (Update inventory)
CREATE OR ALTER PROCEDURE Procurement.sp_PostGRN
    @GRNID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @PostedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Update GRN status
        UPDATE Procurement.GoodsReceiptNote
        SET 
            Status = 'Posted',
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @PostedBy
        WHERE GRNID = @GRNID 
        AND TenantID = @TenantID;
        
        -- Update inventory stock balances
        INSERT INTO Inventory.StockLedger (
            StockLedgerID, ItemID, WarehouseID, TransactionType, 
            Quantity, UnitPrice, ReferenceID, ReferenceType, 
            TransactionDate, TenantID, CreatedBy
        )
        SELECT 
            NEWID(),
            grnd.ItemID,
            grn.WarehouseID,
            'GRN',
            grnd.QuantityReceived,
            grnd.UnitPrice,
            @GRNID,
            'GRN',
            grn.ReceivedDate,
            @TenantID,
            @PostedBy
        FROM Procurement.GoodsReceiptNoteDetails grnd
        INNER JOIN Procurement.GoodsReceiptNote grn ON grnd.GRNID = grn.GRNID
        WHERE grn.GRNID = @GRNID;
        
        -- Update stock balances
        MERGE Inventory.StockBalances AS target
        USING (
            SELECT 
                grnd.ItemID,
                grn.WarehouseID,
                SUM(grnd.QuantityReceived) as Quantity,
                AVG(grnd.UnitPrice) as UnitPrice
            FROM Procurement.GoodsReceiptNoteDetails grnd
            INNER JOIN Procurement.GoodsReceiptNote grn ON grnd.GRNID = grn.GRNID
            WHERE grn.GRNID = @GRNID
            GROUP BY grnd.ItemID, grn.WarehouseID
        ) AS source ON target.ItemID = source.ItemID AND target.WarehouseID = source.WarehouseID
        WHEN MATCHED THEN
            UPDATE SET 
                QuantityOnHand = target.QuantityOnHand + source.Quantity,
                UpdatedAt = GETUTCDATE(),
                UpdatedBy = @PostedBy
        WHEN NOT MATCHED THEN
            INSERT (StockBalanceID, ItemID, WarehouseID, QuantityOnHand, UnitPrice, TenantID, CreatedBy)
            VALUES (NEWID(), source.ItemID, source.WarehouseID, source.Quantity, source.UnitPrice, @TenantID, @PostedBy);
        
        COMMIT TRANSACTION;
        SELECT 1 as Success;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 as Success, ERROR_MESSAGE() as ErrorMessage;
    END CATCH
END
GO

-- =============================================
-- PURCHASE INVOICE PROCEDURES
-- =============================================

-- Get Purchase Invoice by ID
CREATE OR ALTER PROCEDURE Procurement.sp_GetPurchaseInvoiceById
    @InvoiceID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pi.InvoiceID,
        pi.CompanyID,
        pi.TenantID,
        pi.POID,
        pi.VendorID,
        pi.InvoiceNumber,
        pi.InvoiceDate,
        pi.DueDate,
        pi.Currency,
        pi.TotalAmount,
        pi.TaxAmount,
        pi.NetPayable,
        pi.Status,
        pi.CreatedAt,
        pi.UpdatedAt,
        v.VendorName,
        v.VendorCode,
        c.CompanyName
    FROM Procurement.PurchaseInvoices pi
    INNER JOIN Procurement.Vendors v ON pi.VendorID = v.VendorID
    INNER JOIN Core.Companies c ON pi.CompanyID = c.Id
    WHERE pi.InvoiceID = @InvoiceID 
    AND pi.TenantID = @TenantID;
END
GO

-- Create Purchase Invoice
CREATE OR ALTER PROCEDURE Procurement.sp_CreatePurchaseInvoice
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @POID UNIQUEIDENTIFIER = NULL,
    @VendorID UNIQUEIDENTIFIER,
    @InvoiceNumber NVARCHAR(100),
    @InvoiceDate DATETIME2,
    @DueDate DATETIME2,
    @Currency NVARCHAR(3) = 'USD',
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @InvoiceID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.PurchaseInvoices (
        InvoiceID, CompanyID, TenantID, POID, VendorID,
        InvoiceNumber, InvoiceDate, DueDate, Currency, CreatedBy
    )
    VALUES (
        @InvoiceID, @CompanyID, @TenantID, @POID, @VendorID,
        @InvoiceNumber, @InvoiceDate, @DueDate, @Currency, @CreatedBy
    );
    
    SELECT @InvoiceID as InvoiceID;
END
GO

-- Add Purchase Invoice Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddPurchaseInvoiceDetail
    @InvoiceID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @Quantity DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @TaxAmount DECIMAL(18,2) = 0,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @InvoiceDetailID UNIQUEIDENTIFIER = NEWID();
    DECLARE @NetAmount DECIMAL(18,2);
    
    -- Calculate net amount
    SET @NetAmount = (@Quantity * @UnitPrice) + @TaxAmount;
    
    INSERT INTO Procurement.PurchaseInvoiceDetails (
        InvoiceDetailID, InvoiceID, ItemID, Quantity, UnitPrice,
        TaxAmount, NetAmount, CreatedBy
    )
    VALUES (
        @InvoiceDetailID, @InvoiceID, @ItemID, @Quantity, @UnitPrice,
        @TaxAmount, @NetAmount, @CreatedBy
    );
    
    -- Update invoice totals
    UPDATE Procurement.PurchaseInvoices
    SET 
        TotalAmount = (
            SELECT SUM(NetAmount) 
            FROM Procurement.PurchaseInvoiceDetails 
            WHERE InvoiceID = @InvoiceID
        ),
        TaxAmount = (
            SELECT SUM(TaxAmount) 
            FROM Procurement.PurchaseInvoiceDetails 
            WHERE InvoiceID = @InvoiceID
        ),
        NetPayable = (
            SELECT SUM(NetAmount) 
            FROM Procurement.PurchaseInvoiceDetails 
            WHERE InvoiceID = @InvoiceID
        )
    WHERE InvoiceID = @InvoiceID;
    
    SELECT @InvoiceDetailID as InvoiceDetailID;
END
GO

-- Update Purchase Invoice Status
CREATE OR ALTER PROCEDURE Procurement.sp_UpdatePurchaseInvoiceStatus
    @InvoiceID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @UpdatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Procurement.PurchaseInvoices
    SET 
        Status = @Status,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE InvoiceID = @InvoiceID 
    AND TenantID = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- REQUEST FOR QUOTATION PROCEDURES
-- =============================================

-- Get RFQ by ID
CREATE OR ALTER PROCEDURE Procurement.sp_GetRFQById
    @RFQID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rfq.RFQID,
        rfq.CompanyID,
        rfq.TenantID,
        rfq.PRID,
        rfq.RFQNumber,
        rfq.RFQDate,
        rfq.DueDate,
        rfq.Status,
        rfq.Remarks,
        rfq.CreatedAt,
        rfq.UpdatedAt,
        rfq.CreatedBy,
        rfq.UpdatedBy,
        c.CompanyName
    FROM Procurement.RequestForQuotation rfq
    INNER JOIN Core.Companies c ON rfq.CompanyID = c.Id
    WHERE rfq.RFQID = @RFQID 
    AND rfq.TenantID = @TenantID;
END
GO

-- Get RFQ Details
CREATE OR ALTER PROCEDURE Procurement.sp_GetRFQDetails
    @RFQID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rfqd.RFQDetailID,
        rfqd.RFQID,
        rfqd.ItemID,
        rfqd.QuantityRequested,
        rfqd.UnitOfMeasureID,
        rfqd.Remarks,
        i.ItemName,
        i.ItemCode
        -- uom.UnitName, -- TODO: Create UnitOfMeasures table
        -- uom.UnitSymbol -- TODO: Create UnitOfMeasures table
    FROM Procurement.RFQDetails rfqd
    INNER JOIN Inventory.Items i ON rfqd.ItemID = i.Id
    -- INNER JOIN Inventory.UnitOfMeasures uom ON rfqd.UnitOfMeasureID = uom.UnitOfMeasureID -- TODO: Create UnitOfMeasures table
    WHERE rfqd.RFQID = @RFQID;
END
GO

-- Create RFQ
CREATE OR ALTER PROCEDURE Procurement.sp_CreateRFQ
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @PRID UNIQUEIDENTIFIER = NULL,
    @RFQNumber NVARCHAR(50),
    @DueDate DATETIME2,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RFQID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.RequestForQuotation (
        RFQID, CompanyID, TenantID, PRID, RFQNumber, DueDate, Remarks, CreatedBy
    )
    VALUES (
        @RFQID, @CompanyID, @TenantID, @PRID, @RFQNumber, @DueDate, @Remarks, @CreatedBy
    );
    
    SELECT @RFQID as RFQID;
END
GO

-- Add RFQ Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddRFQDetail
    @RFQID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @QuantityRequested DECIMAL(18,4),
    @UnitOfMeasureID UNIQUEIDENTIFIER,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RFQDetailID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.RFQDetails (
        RFQDetailID, RFQID, ItemID, QuantityRequested, UnitOfMeasureID, Remarks, CreatedBy
    )
    VALUES (
        @RFQDetailID, @RFQID, @ItemID, @QuantityRequested, @UnitOfMeasureID, @Remarks, @CreatedBy
    );
    
    SELECT @RFQDetailID as RFQDetailID;
END
GO

-- Get RFQ Vendor Responses
CREATE OR ALTER PROCEDURE Procurement.sp_GetRFQVendorResponses
    @RFQID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rvr.RFQResponseID,
        rvr.RFQID,
        rvr.VendorID,
        rvr.ResponseDate,
        rvr.Status,
        rvr.TotalQuotedAmount,
        rvr.ValidityDays,
        rvr.Remarks,
        rvr.CreatedAt,
        rvr.UpdatedAt,
        v.VendorName,
        v.VendorCode
    FROM Procurement.RFQVendorResponses rvr
    INNER JOIN Procurement.Vendors v ON rvr.VendorID = v.VendorID
    INNER JOIN Procurement.RequestForQuotation rfq ON rvr.RFQID = rfq.RFQID
    WHERE rfq.RFQID = @RFQID 
    AND rfq.TenantID = @TenantID
    ORDER BY rvr.ResponseDate DESC;
END
GO

-- Add RFQ Vendor Response
CREATE OR ALTER PROCEDURE Procurement.sp_AddRFQVendorResponse
    @RFQID UNIQUEIDENTIFIER,
    @VendorID UNIQUEIDENTIFIER,
    @ValidityDays INT = 30,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RFQResponseID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.RFQVendorResponses (
        RFQResponseID, RFQID, VendorID, ValidityDays, Remarks, CreatedBy
    )
    VALUES (
        @RFQResponseID, @RFQID, @VendorID, @ValidityDays, @Remarks, @CreatedBy
    );
    
    SELECT @RFQResponseID as RFQResponseID;
END
GO

-- Add RFQ Vendor Response Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddRFQVendorResponseDetail
    @RFQResponseID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @QuotedUnitPrice DECIMAL(18,4),
    @Discount DECIMAL(5,2) = 0,
    @TaxAmount DECIMAL(18,2) = 0,
    @Remarks NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RFQResponseDetailID UNIQUEIDENTIFIER = NEWID();
    DECLARE @NetAmount DECIMAL(18,2);
    
    -- Calculate net amount
    SET @NetAmount = @QuotedUnitPrice - ((@QuotedUnitPrice * @Discount) / 100) + @TaxAmount;
    
    INSERT INTO Procurement.RFQVendorResponseDetails (
        RFQResponseDetailID, RFQResponseID, ItemID, QuotedUnitPrice,
        Discount, TaxAmount, NetAmount, Remarks, CreatedBy
    )
    VALUES (
        @RFQResponseDetailID, @RFQResponseID, @ItemID, @QuotedUnitPrice,
        @Discount, @TaxAmount, @NetAmount, @Remarks, @CreatedBy
    );
    
    -- Update response total amount
    UPDATE Procurement.RFQVendorResponses
    SET TotalQuotedAmount = (
        SELECT SUM(NetAmount) 
        FROM Procurement.RFQVendorResponseDetails 
        WHERE RFQResponseID = @RFQResponseID
    )
    WHERE RFQResponseID = @RFQResponseID;
    
    SELECT @RFQResponseDetailID as RFQResponseDetailID;
END
GO

-- Update RFQ Status
CREATE OR ALTER PROCEDURE Procurement.sp_UpdateRFQStatus
    @RFQID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @UpdatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Procurement.RequestForQuotation
    SET 
        Status = @Status,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE RFQID = @RFQID 
    AND TenantID = @TenantID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- VENDOR BANK ACCOUNTS PROCEDURES
-- =============================================

-- Get Vendor Bank Accounts
CREATE OR ALTER PROCEDURE Procurement.sp_GetVendorBankAccounts
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        vba.VendorBankAccountID,
        vba.VendorID,
        vba.BankName,
        vba.AccountNumber,
        vba.IBAN,
        vba.SWIFTCode,
        vba.Currency,
        vba.IsPrimary,
        vba.CreatedAt,
        vba.CreatedBy
    FROM Procurement.VendorBankAccounts vba
    INNER JOIN Procurement.Vendors v ON vba.VendorID = v.VendorID
    WHERE v.VendorID = @VendorID 
    AND v.TenantID = @TenantID
    ORDER BY vba.IsPrimary DESC, vba.CreatedAt DESC;
END
GO

-- Add Vendor Bank Account
CREATE OR ALTER PROCEDURE Procurement.sp_AddVendorBankAccount
    @VendorID UNIQUEIDENTIFIER,
    @BankName NVARCHAR(255),
    @AccountNumber NVARCHAR(50),
    @IBAN NVARCHAR(50) = NULL,
    @SWIFTCode NVARCHAR(20) = NULL,
    @Currency NVARCHAR(3) = 'USD',
    @IsPrimary BIT = 0,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @VendorBankAccountID UNIQUEIDENTIFIER = NEWID();
    
    -- If this is set as primary, unset other primary accounts
    IF @IsPrimary = 1
    BEGIN
        UPDATE Procurement.VendorBankAccounts
        SET IsPrimary = 0
        WHERE VendorID = @VendorID;
    END
    
    INSERT INTO Procurement.VendorBankAccounts (
        VendorBankAccountID, VendorID, BankName, AccountNumber,
        IBAN, SWIFTCode, Currency, IsPrimary, CreatedBy
    )
    VALUES (
        @VendorBankAccountID, @VendorID, @BankName, @AccountNumber,
        @IBAN, @SWIFTCode, @Currency, @IsPrimary, @CreatedBy
    );
    
    SELECT @VendorBankAccountID as VendorBankAccountID;
END
GO

-- =============================================
-- VENDOR GROUPS PROCEDURES
-- =============================================

-- Get All Vendor Groups
CREATE OR ALTER PROCEDURE Procurement.sp_GetAllVendorGroups
    @TenantID UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        vg.VendorGroupID,
        vg.GroupName,
        vg.Description,
        vg.IsActive,
        vg.CreatedAt,
        vg.UpdatedAt,
        vg.CreatedBy,
        vg.UpdatedBy
    FROM Procurement.VendorGroups vg
    WHERE vg.TenantID = @TenantID
    AND (@IsActive IS NULL OR vg.IsActive = @IsActive)
    ORDER BY vg.GroupName;
END
GO

-- Create Vendor Group
CREATE OR ALTER PROCEDURE Procurement.sp_CreateVendorGroup
    @GroupName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @VendorGroupID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.VendorGroups (
        VendorGroupID, GroupName, Description, TenantID, CreatedBy
    )
    VALUES (
        @VendorGroupID, @GroupName, @Description, @TenantID, @CreatedBy
    );
    
    SELECT @VendorGroupID as VendorGroupID;
END
GO

-- =============================================
-- VENDOR CONTACTS PROCEDURES
-- =============================================

-- Get Vendor Contacts
CREATE OR ALTER PROCEDURE Procurement.sp_GetVendorContacts
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        vc.VendorContactID,
        vc.VendorID,
        vc.ContactPerson,
        vc.JobTitle,
        vc.Department,
        vc.Email,
        vc.PhoneNumber,
        vc.MobileNumber,
        vc.IsPrimary,
        vc.IsActive,
        vc.CreatedAt,
        vc.UpdatedAt,
        vc.CreatedBy,
        vc.UpdatedBy
    FROM Procurement.VendorContacts vc
    INNER JOIN Procurement.Vendors v ON vc.VendorID = v.VendorID
    WHERE v.VendorID = @VendorID 
    AND v.TenantID = @TenantID
    AND vc.IsActive = 1
    ORDER BY vc.IsPrimary DESC, vc.ContactPerson;
END
GO

-- Add Vendor Contact
CREATE OR ALTER PROCEDURE Procurement.sp_AddVendorContact
    @VendorID UNIQUEIDENTIFIER,
    @ContactPerson NVARCHAR(255),
    @JobTitle NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
    @Email NVARCHAR(255) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @MobileNumber NVARCHAR(50) = NULL,
    @IsPrimary BIT = 0,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @VendorContactID UNIQUEIDENTIFIER = NEWID();
    
    -- If this is set as primary, unset other primary contacts
    IF @IsPrimary = 1
    BEGIN
        UPDATE Procurement.VendorContacts
        SET IsPrimary = 0
        WHERE VendorID = @VendorID;
    END
    
    INSERT INTO Procurement.VendorContacts (
        VendorContactID, VendorID, ContactPerson, JobTitle, Department,
        Email, PhoneNumber, MobileNumber, IsPrimary, TenantID, CreatedBy
    )
    VALUES (
        @VendorContactID, @VendorID, @ContactPerson, @JobTitle, @Department,
        @Email, @PhoneNumber, @MobileNumber, @IsPrimary, @TenantID, @CreatedBy
    );
    
    SELECT @VendorContactID as VendorContactID;
END
GO

-- =============================================
-- VENDOR RATINGS PROCEDURES
-- =============================================

-- Get Vendor Ratings
CREATE OR ALTER PROCEDURE Procurement.sp_GetVendorRatings
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredRatings AS (
        SELECT 
            vr.VendorRatingID,
            vr.VendorID,
            vr.RatingDate,
            vr.QualityRating,
            vr.DeliveryRating,
            vr.ServiceRating,
            vr.PriceRating,
            vr.OverallRating,
            vr.Comments,
            vr.RatedByUserID,
            u.FirstName + ' ' + u.LastName as RatedByName,
            ROW_NUMBER() OVER (ORDER BY vr.RatingDate DESC) AS RowNum
        FROM Procurement.VendorRatings vr
        INNER JOIN Procurement.Vendors v ON vr.VendorID = v.VendorID
        INNER JOIN Core.Users u ON vr.RatedByUserID = u.Id
        WHERE v.VendorID = @VendorID 
        AND v.TenantID = @TenantID
    )
    SELECT 
        VendorRatingID,
        VendorID,
        RatingDate,
        QualityRating,
        DeliveryRating,
        ServiceRating,
        PriceRating,
        OverallRating,
        Comments,
        RatedByUserID,
        RatedByName
    FROM FilteredRatings
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM Procurement.VendorRatings vr
    INNER JOIN Procurement.Vendors v ON vr.VendorID = v.VendorID
    WHERE v.VendorID = @VendorID 
    AND v.TenantID = @TenantID;
END
GO

-- Add Vendor Rating
CREATE OR ALTER PROCEDURE Procurement.sp_AddVendorRating
    @VendorID UNIQUEIDENTIFIER,
    @QualityRating DECIMAL(3,2),
    @DeliveryRating DECIMAL(3,2),
    @ServiceRating DECIMAL(3,2),
    @PriceRating DECIMAL(3,2),
    @OverallRating DECIMAL(3,2),
    @Comments NVARCHAR(1000) = NULL,
    @RatedByUserID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @VendorRatingID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.VendorRatings (
        VendorRatingID, VendorID, QualityRating, DeliveryRating, ServiceRating,
        PriceRating, OverallRating, Comments, RatedByUserID, TenantID, CreatedBy
    )
    VALUES (
        @VendorRatingID, @VendorID, @QualityRating, @DeliveryRating, @ServiceRating,
        @PriceRating, @OverallRating, @Comments, @RatedByUserID, @TenantID, @CreatedBy
    );
    
    SELECT @VendorRatingID as VendorRatingID;
END
GO

-- =============================================
-- CONTRACT AGREEMENTS PROCEDURES
-- =============================================

-- Get Contract Agreements
CREATE OR ALTER PROCEDURE Procurement.sp_GetContractAgreements
    @VendorID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ca.ContractID,
        ca.VendorID,
        ca.ContractNumber,
        ca.ContractName,
        ca.StartDate,
        ca.EndDate,
        ca.ContractValue,
        ca.Currency,
        ca.ContractType,
        ca.Status,
        ca.Terms,
        ca.Notes,
        ca.CreatedAt,
        ca.UpdatedAt,
        ca.CreatedBy,
        ca.UpdatedBy
    FROM Procurement.ContractAgreements ca
    INNER JOIN Procurement.Vendors v ON ca.VendorID = v.VendorID
    WHERE v.VendorID = @VendorID 
    AND v.TenantID = @TenantID
    AND (@Status IS NULL OR ca.Status = @Status)
    ORDER BY ca.StartDate DESC;
END
GO

-- Create Contract Agreement
CREATE OR ALTER PROCEDURE Procurement.sp_CreateContractAgreement
    @VendorID UNIQUEIDENTIFIER,
    @ContractNumber NVARCHAR(50),
    @ContractName NVARCHAR(255),
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @ContractValue DECIMAL(18,2),
    @Currency NVARCHAR(3) = 'USD',
    @ContractType NVARCHAR(50),
    @Terms NVARCHAR(MAX) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ContractID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.ContractAgreements (
        ContractID, VendorID, ContractNumber, ContractName, StartDate, EndDate,
        ContractValue, Currency, ContractType, Terms, Notes, TenantID, CreatedBy
    )
    VALUES (
        @ContractID, @VendorID, @ContractNumber, @ContractName, @StartDate, @EndDate,
        @ContractValue, @Currency, @ContractType, @Terms, @Notes, @TenantID, @CreatedBy
    );
    
    SELECT @ContractID as ContractID;
END
GO

-- =============================================
-- PURCHASE RETURN ORDERS PROCEDURES
-- =============================================

-- Get Purchase Return Orders
CREATE OR ALTER PROCEDURE Procurement.sp_GetPurchaseReturnOrders
    @TenantID UNIQUEIDENTIFIER,
    @CompanyID UNIQUEIDENTIFIER = NULL,
    @VendorID UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredReturns AS (
        SELECT 
            pro.ReturnOrderID,
            pro.CompanyID,
            pro.TenantID,
            pro.VendorID,
            pro.POID,
            pro.ReturnNumber,
            pro.ReturnDate,
            pro.Reason,
            pro.Status,
            pro.TotalAmount,
            pro.ApprovedByUserID,
            pro.ApprovedAt,
            pro.Notes,
            pro.CreatedAt,
            pro.UpdatedAt,
            v.VendorName,
            v.VendorCode,
            c.CompanyName,
            u.FirstName + ' ' + u.LastName as ApprovedByName,
            ROW_NUMBER() OVER (ORDER BY pro.ReturnDate DESC) AS RowNum
        FROM Procurement.PurchaseReturnOrders pro
        INNER JOIN Procurement.Vendors v ON pro.VendorID = v.VendorID
        INNER JOIN Core.Companies c ON pro.CompanyID = c.Id
        LEFT JOIN Core.Users u ON pro.ApprovedByUserID = u.Id
        WHERE pro.TenantID = @TenantID
        AND (@CompanyID IS NULL OR pro.CompanyID = @CompanyID)
        AND (@VendorID IS NULL OR pro.VendorID = @VendorID)
        AND (@Status IS NULL OR pro.Status = @Status)
    )
    SELECT 
        ReturnOrderID,
        CompanyID,
        TenantID,
        VendorID,
        POID,
        ReturnNumber,
        ReturnDate,
        Reason,
        Status,
        TotalAmount,
        ApprovedByUserID,
        ApprovedAt,
        Notes,
        CreatedAt,
        UpdatedAt,
        VendorName,
        VendorCode,
        CompanyName,
        ApprovedByName
    FROM FilteredReturns
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM Procurement.PurchaseReturnOrders pro
    WHERE pro.TenantID = @TenantID
    AND (@CompanyID IS NULL OR pro.CompanyID = @CompanyID)
    AND (@VendorID IS NULL OR pro.VendorID = @VendorID)
    AND (@Status IS NULL OR pro.Status = @Status);
END
GO

-- Create Purchase Return Order
CREATE OR ALTER PROCEDURE Procurement.sp_CreatePurchaseReturnOrder
    @CompanyID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @VendorID UNIQUEIDENTIFIER,
    @POID UNIQUEIDENTIFIER = NULL,
    @ReturnNumber NVARCHAR(50),
    @Reason NVARCHAR(500),
    @Notes NVARCHAR(1000) = NULL,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ReturnOrderID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.PurchaseReturnOrders (
        ReturnOrderID, CompanyID, TenantID, VendorID, POID,
        ReturnNumber, Reason, Notes, CreatedBy
    )
    VALUES (
        @ReturnOrderID, @CompanyID, @TenantID, @VendorID, @POID,
        @ReturnNumber, @Reason, @Notes, @CreatedBy
    );
    
    SELECT @ReturnOrderID as ReturnOrderID;
END
GO

-- Add Purchase Return Order Detail
CREATE OR ALTER PROCEDURE Procurement.sp_AddPurchaseReturnOrderDetail
    @ReturnOrderID UNIQUEIDENTIFIER,
    @ItemID UNIQUEIDENTIFIER,
    @QuantityReturned DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @Reason NVARCHAR(500) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ReturnDetailID UNIQUEIDENTIFIER = NEWID();
    DECLARE @TotalAmount DECIMAL(18,2);
    
    -- Calculate total amount
    SET @TotalAmount = @QuantityReturned * @UnitPrice;
    
    INSERT INTO Procurement.PurchaseReturnOrderDetails (
        ReturnDetailID, ReturnOrderID, ItemID, QuantityReturned,
        UnitPrice, TotalAmount, Reason, TenantID, CreatedBy
    )
    VALUES (
        @ReturnDetailID, @ReturnOrderID, @ItemID, @QuantityReturned,
        @UnitPrice, @TotalAmount, @Reason, @TenantID, @CreatedBy
    );
    
    -- Update return order total
    UPDATE Procurement.PurchaseReturnOrders
    SET TotalAmount = (
        SELECT SUM(TotalAmount) 
        FROM Procurement.PurchaseReturnOrderDetails 
        WHERE ReturnOrderID = @ReturnOrderID
    )
    WHERE ReturnOrderID = @ReturnOrderID;
    
    SELECT @ReturnDetailID as ReturnDetailID;
END
GO

-- =============================================
-- LANDED COST PROCEDURES
-- =============================================

-- Get Landed Costs
CREATE OR ALTER PROCEDURE Procurement.sp_GetLandedCosts
    @POID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lc.LandedCostID,
        lc.POID,
        lc.CostType,
        lc.CostDescription,
        lc.CostAmount,
        lc.Currency,
        lc.AllocationMethod,
        lc.IsAllocated,
        lc.AllocatedAt,
        lc.CreatedAt,
        lc.UpdatedAt,
        lc.CreatedBy,
        lc.UpdatedBy
    FROM Procurement.LandedCost lc
    INNER JOIN Procurement.PurchaseOrders po ON lc.POID = po.POID
    WHERE po.POID = @POID 
    AND po.TenantID = @TenantID
    ORDER BY lc.CreatedAt DESC;
END
GO

-- Add Landed Cost
CREATE OR ALTER PROCEDURE Procurement.sp_AddLandedCost
    @POID UNIQUEIDENTIFIER,
    @CostType NVARCHAR(50),
    @CostDescription NVARCHAR(255),
    @CostAmount DECIMAL(18,2),
    @Currency NVARCHAR(3) = 'USD',
    @AllocationMethod NVARCHAR(50),
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LandedCostID UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Procurement.LandedCost (
        LandedCostID, POID, CostType, CostDescription, CostAmount,
        Currency, AllocationMethod, TenantID, CreatedBy
    )
    VALUES (
        @LandedCostID, @POID, @CostType, @CostDescription, @CostAmount,
        @Currency, @AllocationMethod, @TenantID, @CreatedBy
    );
    
    SELECT @LandedCostID as LandedCostID;
END
GO

-- Allocate Landed Cost
CREATE OR ALTER PROCEDURE Procurement.sp_AllocateLandedCost
    @LandedCostID UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @AllocatedBy NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @POID UNIQUEIDENTIFIER;
        DECLARE @AllocationMethod NVARCHAR(50);
        DECLARE @TotalCost DECIMAL(18,2);
        
        -- Get landed cost details
        SELECT @POID = POID, @AllocationMethod = AllocationMethod, @TotalCost = CostAmount
        FROM Procurement.LandedCost
        WHERE LandedCostID = @LandedCostID AND TenantID = @TenantID;
        
        -- Delete existing allocations
        DELETE FROM Procurement.LandedCostAllocations
        WHERE LandedCostID = @LandedCostID;
        
        -- Allocate based on method
        IF @AllocationMethod = 'Equal'
        BEGIN
            -- Equal allocation across all PO details
            INSERT INTO Procurement.LandedCostAllocations (
                AllocationID, LandedCostID, PODetailID, AllocatedAmount, AllocationPercentage, TenantID, CreatedBy
            )
            SELECT 
                NEWID(),
                @LandedCostID,
                pod.PODetailID,
                @TotalCost / COUNT(*) OVER(),
                100.0 / COUNT(*) OVER(),
                @TenantID,
                @AllocatedBy
            FROM Procurement.PurchaseOrderDetails pod
            WHERE pod.POID = @POID;
        END
        ELSE IF @AllocationMethod = 'Value'
        BEGIN
            -- Allocate based on line value
            INSERT INTO Procurement.LandedCostAllocations (
                AllocationID, LandedCostID, PODetailID, AllocatedAmount, AllocationPercentage, TenantID, CreatedBy
            )
            SELECT 
                NEWID(),
                @LandedCostID,
                pod.PODetailID,
                (@TotalCost * pod.NetAmount) / (SELECT SUM(NetAmount) FROM Procurement.PurchaseOrderDetails WHERE POID = @POID),
                (pod.NetAmount * 100.0) / (SELECT SUM(NetAmount) FROM Procurement.PurchaseOrderDetails WHERE POID = @POID),
                @TenantID,
                @AllocatedBy
            FROM Procurement.PurchaseOrderDetails pod
            WHERE pod.POID = @POID;
        END
        
        -- Update landed cost as allocated
        UPDATE Procurement.LandedCost
        SET 
            IsAllocated = 1,
            AllocatedAt = GETUTCDATE(),
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @AllocatedBy
        WHERE LandedCostID = @LandedCostID;
        
        COMMIT TRANSACTION;
        SELECT 1 as Success;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 as Success, ERROR_MESSAGE() as ErrorMessage;
    END CATCH
END
GO

PRINT 'Procurement Module Stored Procedures created successfully!'