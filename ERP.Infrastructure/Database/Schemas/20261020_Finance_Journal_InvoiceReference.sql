-- Typed voucher header: Invoice No + optional EntityReference to AR invoice / AP bill.
IF COL_LENGTH('Finance.Journals', 'InvoiceNumber') IS NULL
   AND COL_LENGTH('dbo.Journals', 'InvoiceNumber') IS NULL
BEGIN
    IF OBJECT_ID('Finance.Journals', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE Finance.Journals ADD InvoiceNumber nvarchar(50) NULL;
        ALTER TABLE Finance.Journals ADD EntityReferenceTable nvarchar(100) NULL;
        ALTER TABLE Finance.Journals ADD EntityReferenceId uniqueidentifier NULL;
    END
    ELSE IF OBJECT_ID('dbo.Journals', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Journals ADD InvoiceNumber nvarchar(50) NULL;
        ALTER TABLE dbo.Journals ADD EntityReferenceTable nvarchar(100) NULL;
        ALTER TABLE dbo.Journals ADD EntityReferenceId uniqueidentifier NULL;
    END
END
GO

IF OBJECT_ID('Finance.Journals', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Journals_Tenant_InvoiceNumber' AND object_id = OBJECT_ID('Finance.Journals'))
        CREATE INDEX IX_Journals_Tenant_InvoiceNumber ON Finance.Journals (TenantId, InvoiceNumber);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Journals_Tenant_EntityReference' AND object_id = OBJECT_ID('Finance.Journals'))
        CREATE INDEX IX_Journals_Tenant_EntityReference ON Finance.Journals (TenantId, EntityReferenceTable, EntityReferenceId);
END
ELSE IF OBJECT_ID('dbo.Journals', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Journals_Tenant_InvoiceNumber' AND object_id = OBJECT_ID('dbo.Journals'))
        CREATE INDEX IX_Journals_Tenant_InvoiceNumber ON dbo.Journals (TenantId, InvoiceNumber);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Journals_Tenant_EntityReference' AND object_id = OBJECT_ID('dbo.Journals'))
        CREATE INDEX IX_Journals_Tenant_EntityReference ON dbo.Journals (TenantId, EntityReferenceTable, EntityReferenceId);
END
GO
