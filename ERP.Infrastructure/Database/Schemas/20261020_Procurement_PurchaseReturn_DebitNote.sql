-- Purchase return + debit note link.
IF OBJECT_ID('dbo.PurchaseReturns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseReturns (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ReturnNumber nvarchar(50) NOT NULL,
        ReturnDate datetime2 NOT NULL,
        VendorId uniqueidentifier NOT NULL,
        PurchaseOrderId uniqueidentifier NULL,
        PurchaseInvoiceId uniqueidentifier NULL,
        WarehouseId uniqueidentifier NULL,
        DebitNoteId uniqueidentifier NULL,
        Reason nvarchar(500) NOT NULL CONSTRAINT DF_PurchaseReturns_Reason DEFAULT (''),
        Status nvarchar(50) NOT NULL CONSTRAINT DF_PurchaseReturns_Status DEFAULT ('Pending'),
        TotalAmount decimal(18,2) NOT NULL CONSTRAINT DF_PurchaseReturns_TotalAmount DEFAULT (0),
        Notes nvarchar(max) NULL,
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_PurchaseReturns_IsActive DEFAULT (1)
    );
    CREATE INDEX IX_PurchaseReturns_TenantId ON dbo.PurchaseReturns(TenantId);
    CREATE INDEX IX_PurchaseReturns_VendorId ON dbo.PurchaseReturns(VendorId);
END
GO

IF OBJECT_ID('dbo.PurchaseReturnLines', 'U') IS NULL AND OBJECT_ID('dbo.PurchaseReturns', 'U') IS NOT NULL
BEGIN
    CREATE TABLE dbo.PurchaseReturnLines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        PurchaseReturnId uniqueidentifier NOT NULL,
        ItemId uniqueidentifier NOT NULL,
        Quantity decimal(18,4) NOT NULL,
        UnitPrice decimal(18,2) NOT NULL,
        LineTotal decimal(18,2) NOT NULL,
        Reason nvarchar(500) NULL,
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_PurchaseReturnLines_IsActive DEFAULT (1),
        CONSTRAINT FK_PurchaseReturnLines_PurchaseReturns FOREIGN KEY (PurchaseReturnId)
            REFERENCES dbo.PurchaseReturns(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_PurchaseReturnLines_PurchaseReturnId ON dbo.PurchaseReturnLines(PurchaseReturnId);
END
GO
