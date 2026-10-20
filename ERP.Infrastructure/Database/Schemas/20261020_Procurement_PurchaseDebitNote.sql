-- Purchase Debit Note: operational DN that posts by reducing Finance AP balance on the linked PI bill.
IF OBJECT_ID('dbo.PurchaseDebitNotes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseDebitNotes (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        DebitNoteNumber nvarchar(50) NOT NULL,
        DebitNoteDate datetime2 NOT NULL,
        VendorId uniqueidentifier NOT NULL,
        OriginalPurchaseInvoiceId uniqueidentifier NOT NULL,
        PurchaseReturnId uniqueidentifier NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_PurchaseDebitNotes_Status DEFAULT ('Draft'),
        Reason nvarchar(500) NULL,
        SubTotal decimal(18,2) NOT NULL CONSTRAINT DF_PurchaseDebitNotes_SubTotal DEFAULT (0),
        TaxAmount decimal(18,2) NOT NULL CONSTRAINT DF_PurchaseDebitNotes_TaxAmount DEFAULT (0),
        TotalAmount decimal(18,2) NOT NULL CONSTRAINT DF_PurchaseDebitNotes_TotalAmount DEFAULT (0),
        Notes nvarchar(max) NULL,
        PostedAt datetime2 NULL,
        PostedBy uniqueidentifier NULL,
        AppliedFinanceBillId uniqueidentifier NULL,
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_PurchaseDebitNotes_IsActive DEFAULT (1)
    );
    CREATE INDEX IX_PurchaseDebitNotes_TenantId ON dbo.PurchaseDebitNotes(TenantId);
    CREATE INDEX IX_PurchaseDebitNotes_VendorId ON dbo.PurchaseDebitNotes(VendorId);
    CREATE INDEX IX_PurchaseDebitNotes_OriginalPurchaseInvoiceId ON dbo.PurchaseDebitNotes(OriginalPurchaseInvoiceId);
    CREATE UNIQUE INDEX UX_PurchaseDebitNotes_Tenant_Number ON dbo.PurchaseDebitNotes(TenantId, DebitNoteNumber);
END
GO

IF OBJECT_ID('dbo.PurchaseDebitNoteLines', 'U') IS NULL AND OBJECT_ID('dbo.PurchaseDebitNotes', 'U') IS NOT NULL
BEGIN
    CREATE TABLE dbo.PurchaseDebitNoteLines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        DebitNoteId uniqueidentifier NOT NULL,
        LineNumber int NOT NULL CONSTRAINT DF_PurchaseDebitNoteLines_LineNumber DEFAULT (1),
        OriginalPurchaseInvoiceLineId uniqueidentifier NULL,
        ItemId uniqueidentifier NULL,
        Description nvarchar(500) NOT NULL CONSTRAINT DF_PurchaseDebitNoteLines_Description DEFAULT (''),
        Quantity decimal(18,4) NOT NULL,
        UnitPrice decimal(18,2) NOT NULL,
        TaxRate decimal(18,4) NOT NULL CONSTRAINT DF_PurchaseDebitNoteLines_TaxRate DEFAULT (0),
        LineTotal decimal(18,2) NOT NULL,
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_PurchaseDebitNoteLines_IsActive DEFAULT (1),
        CONSTRAINT FK_PurchaseDebitNoteLines_PurchaseDebitNotes FOREIGN KEY (DebitNoteId)
            REFERENCES dbo.PurchaseDebitNotes(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_PurchaseDebitNoteLines_DebitNoteId ON dbo.PurchaseDebitNoteLines(DebitNoteId);
END
GO
