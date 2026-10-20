-- Accounts module: CoA remarks, AccountDefaults, Journal CPV/approve columns
-- Apply on shared host DB before deploying API that uses these columns.

IF COL_LENGTH('Finance.ChartOfAccounts', 'Remarks') IS NULL
    AND COL_LENGTH('dbo.ChartOfAccounts', 'Remarks') IS NULL
BEGIN
    IF OBJECT_ID('Finance.ChartOfAccounts', 'U') IS NOT NULL
        ALTER TABLE Finance.ChartOfAccounts ADD Remarks nvarchar(500) NULL;
    ELSE IF OBJECT_ID('dbo.ChartOfAccounts', 'U') IS NOT NULL
        ALTER TABLE dbo.ChartOfAccounts ADD Remarks nvarchar(500) NULL;
END
GO

IF OBJECT_ID('Finance.AccountDefaults', 'U') IS NULL AND OBJECT_ID('dbo.AccountDefaults', 'U') IS NULL
BEGIN
    IF SCHEMA_ID('Finance') IS NOT NULL
    BEGIN
        CREATE TABLE Finance.AccountDefaults (
            AccountDefaultId uniqueidentifier NOT NULL PRIMARY KEY,
            Id uniqueidentifier NOT NULL,
            TenantId uniqueidentifier NOT NULL,
            Purpose nvarchar(50) NOT NULL,
            AccountId uniqueidentifier NOT NULL,
            IsActive bit NOT NULL DEFAULT 1,
            CreatedAt datetime2 NOT NULL,
            UpdatedAt datetime2 NULL,
            CreatedBy nvarchar(100) NULL,
            UpdatedBy nvarchar(100) NULL
        );
        CREATE UNIQUE INDEX IX_AccountDefaults_Tenant_Purpose ON Finance.AccountDefaults (TenantId, Purpose);
    END
    ELSE
    BEGIN
        CREATE TABLE dbo.AccountDefaults (
            AccountDefaultId uniqueidentifier NOT NULL PRIMARY KEY,
            Id uniqueidentifier NOT NULL,
            TenantId uniqueidentifier NOT NULL,
            Purpose nvarchar(50) NOT NULL,
            AccountId uniqueidentifier NOT NULL,
            IsActive bit NOT NULL DEFAULT 1,
            CreatedAt datetime2 NOT NULL,
            UpdatedAt datetime2 NULL,
            CreatedBy nvarchar(100) NULL,
            UpdatedBy nvarchar(100) NULL
        );
        CREATE UNIQUE INDEX IX_AccountDefaults_Tenant_Purpose ON dbo.AccountDefaults (TenantId, Purpose);
    END
END
GO

DECLARE @JournalTable sysname =
    CASE WHEN OBJECT_ID('Finance.Journals', 'U') IS NOT NULL THEN 'Finance.Journals'
         WHEN OBJECT_ID('dbo.Journals', 'U') IS NOT NULL THEN 'dbo.Journals'
         ELSE NULL END;

IF @JournalTable IS NOT NULL
BEGIN
    IF COL_LENGTH(@JournalTable, 'ReferenceDate') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ReferenceDate datetime2 NULL');
    IF COL_LENGTH(@JournalTable, 'ChequeNumber') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ChequeNumber nvarchar(50) NULL');
    IF COL_LENGTH(@JournalTable, 'PaidBy') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD PaidBy nvarchar(200) NULL');
    IF COL_LENGTH(@JournalTable, 'ReceivedBy') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ReceivedBy nvarchar(200) NULL');
    IF COL_LENGTH(@JournalTable, 'CashOrBankAccountId') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD CashOrBankAccountId uniqueidentifier NULL');
    IF COL_LENGTH(@JournalTable, 'ApprovedAt') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ApprovedAt datetime2 NULL');
    IF COL_LENGTH(@JournalTable, 'ApprovedBy') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ApprovedBy uniqueidentifier NULL');
END
GO
