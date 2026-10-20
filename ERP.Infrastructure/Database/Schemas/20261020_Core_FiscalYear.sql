-- Fiscal year master + optional FK on journals for post enforce.
IF OBJECT_ID('Core.FiscalYears', 'U') IS NULL AND OBJECT_ID('dbo.FiscalYears', 'U') IS NULL
BEGIN
    IF SCHEMA_ID('Core') IS NULL EXEC('CREATE SCHEMA Core');
    CREATE TABLE Core.FiscalYears
    (
        Id uniqueidentifier NOT NULL,
        FiscalYearId uniqueidentifier NOT NULL,
        TenantId uniqueidentifier NOT NULL,
        Name nvarchar(200) NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(20) NOT NULL CONSTRAINT DF_FiscalYears_Status DEFAULT ('Active'),
        IsActive bit NOT NULL CONSTRAINT DF_FiscalYears_IsActive DEFAULT (1),
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        CONSTRAINT PK_FiscalYears PRIMARY KEY (FiscalYearId)
    );
    CREATE UNIQUE INDEX IX_FiscalYears_Tenant_Id ON Core.FiscalYears (TenantId, Id);
    CREATE INDEX IX_FiscalYears_Tenant_Status_Dates ON Core.FiscalYears (TenantId, Status, StartDate, EndDate);
END
GO

DECLARE @JournalTable sysname =
    CASE WHEN OBJECT_ID('Finance.Journals', 'U') IS NOT NULL THEN 'Finance.Journals'
         WHEN OBJECT_ID('dbo.Journals', 'U') IS NOT NULL THEN 'dbo.Journals'
         ELSE NULL END;

IF @JournalTable IS NOT NULL AND COL_LENGTH(@JournalTable, 'FinancialYearId') IS NULL
    EXEC('ALTER TABLE ' + @JournalTable + ' ADD FinancialYearId uniqueidentifier NULL');
GO
