-- Sales Credit Note: operational CN that posts by reducing Finance AR balance on the linked SI.
IF OBJECT_ID('dbo.SalesCreditNotes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesCreditNotes (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        CreditNoteNumber nvarchar(50) NOT NULL,
        CreditNoteDate datetime2 NOT NULL,
        CustomerId uniqueidentifier NOT NULL,
        OriginalSalesInvoiceId uniqueidentifier NOT NULL,
        ReturnOrderId uniqueidentifier NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_SalesCreditNotes_Status DEFAULT ('Draft'),
        Reason nvarchar(500) NULL,
        SubTotal decimal(18,2) NOT NULL CONSTRAINT DF_SalesCreditNotes_SubTotal DEFAULT (0),
        TaxAmount decimal(18,2) NOT NULL CONSTRAINT DF_SalesCreditNotes_TaxAmount DEFAULT (0),
        TotalAmount decimal(18,2) NOT NULL CONSTRAINT DF_SalesCreditNotes_TotalAmount DEFAULT (0),
        Notes nvarchar(max) NULL,
        PostedAt datetime2 NULL,
        PostedBy uniqueidentifier NULL,
        AppliedFinanceInvoiceId uniqueidentifier NULL,
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_SalesCreditNotes_IsActive DEFAULT (1)
    );
    CREATE INDEX IX_SalesCreditNotes_TenantId ON dbo.SalesCreditNotes(TenantId);
    CREATE INDEX IX_SalesCreditNotes_CustomerId ON dbo.SalesCreditNotes(CustomerId);
    CREATE INDEX IX_SalesCreditNotes_OriginalSalesInvoiceId ON dbo.SalesCreditNotes(OriginalSalesInvoiceId);
    CREATE UNIQUE INDEX UX_SalesCreditNotes_Tenant_Number ON dbo.SalesCreditNotes(TenantId, CreditNoteNumber);
END
GO

IF OBJECT_ID('dbo.SalesCreditNoteLines', 'U') IS NULL AND OBJECT_ID('dbo.SalesCreditNotes', 'U') IS NOT NULL
BEGIN
    CREATE TABLE dbo.SalesCreditNoteLines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        CreditNoteId uniqueidentifier NOT NULL,
        LineNumber int NOT NULL CONSTRAINT DF_SalesCreditNoteLines_LineNumber DEFAULT (1),
        OriginalSalesInvoiceLineId uniqueidentifier NULL,
        ItemId uniqueidentifier NULL,
        Description nvarchar(500) NOT NULL CONSTRAINT DF_SalesCreditNoteLines_Description DEFAULT (''),
        Quantity decimal(18,4) NOT NULL,
        UnitPrice decimal(18,2) NOT NULL,
        TaxRate decimal(18,4) NOT NULL CONSTRAINT DF_SalesCreditNoteLines_TaxRate DEFAULT (0),
        LineTotal decimal(18,2) NOT NULL,
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_SalesCreditNoteLines_IsActive DEFAULT (1),
        CONSTRAINT FK_SalesCreditNoteLines_SalesCreditNotes FOREIGN KEY (CreditNoteId)
            REFERENCES dbo.SalesCreditNotes(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_SalesCreditNoteLines_CreditNoteId ON dbo.SalesCreditNoteLines(CreditNoteId);
END
GO
