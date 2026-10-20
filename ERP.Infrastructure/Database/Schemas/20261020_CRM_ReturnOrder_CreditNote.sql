-- Sale return: warehouse / invoice link + credit note id; line table for EF if missing on dbo.
IF COL_LENGTH('dbo.ReturnOrders', 'WarehouseId') IS NULL
   AND OBJECT_ID('dbo.ReturnOrders', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.ReturnOrders ADD WarehouseId uniqueidentifier NULL;
    ALTER TABLE dbo.ReturnOrders ADD SalesInvoiceId uniqueidentifier NULL;
    ALTER TABLE dbo.ReturnOrders ADD CreditNoteId uniqueidentifier NULL;
END
GO

IF OBJECT_ID('dbo.ReturnOrderLines', 'U') IS NULL AND OBJECT_ID('dbo.ReturnOrders', 'U') IS NOT NULL
BEGIN
    CREATE TABLE dbo.ReturnOrderLines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        ReturnOrderId uniqueidentifier NOT NULL,
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
        IsActive bit NOT NULL CONSTRAINT DF_ReturnOrderLines_IsActive DEFAULT (1),
        CONSTRAINT FK_ReturnOrderLines_ReturnOrders FOREIGN KEY (ReturnOrderId) REFERENCES dbo.ReturnOrders(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ReturnOrderLines_ReturnOrderId ON dbo.ReturnOrderLines(ReturnOrderId);
END
GO
