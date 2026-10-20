-- Party harden: salesman master + customer/vendor credit days & tax IDs.
IF OBJECT_ID('dbo.Salesmen', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Salesmen (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        SalesmanCode nvarchar(50) NOT NULL,
        FullName nvarchar(200) NOT NULL,
        Phone nvarchar(50) NULL,
        Email nvarchar(200) NULL,
        Address nvarchar(500) NULL,
        Cnic nvarchar(20) NULL,
        Remarks nvarchar(500) NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_Salesmen_Status DEFAULT ('Active'),
        TenantId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NULL,
        UpdatedAt datetime2 NULL,
        CreatedBy nvarchar(100) NULL,
        UpdatedBy nvarchar(100) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Salesmen_IsActive DEFAULT (1)
    );
    CREATE INDEX IX_Salesmen_TenantId ON dbo.Salesmen(TenantId);
END
GO

IF COL_LENGTH('dbo.Customers', 'CreditDays') IS NULL AND OBJECT_ID('dbo.Customers', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Customers ADD CreditDays int NOT NULL CONSTRAINT DF_Customers_CreditDays DEFAULT (0);
    ALTER TABLE dbo.Customers ADD TaxType nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD Ntn nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD Strn nvarchar(50) NULL;
    ALTER TABLE dbo.Customers ADD Cnic nvarchar(20) NULL;
    ALTER TABLE dbo.Customers ADD SalesmanId uniqueidentifier NULL;
END
GO

IF COL_LENGTH('dbo.Vendors', 'CreditDays') IS NULL AND OBJECT_ID('dbo.Vendors', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Vendors ADD CreditDays int NOT NULL CONSTRAINT DF_Vendors_CreditDays DEFAULT (0);
    ALTER TABLE dbo.Vendors ADD TaxType nvarchar(50) NULL;
    ALTER TABLE dbo.Vendors ADD Ntn nvarchar(50) NULL;
    ALTER TABLE dbo.Vendors ADD Strn nvarchar(50) NULL;
    ALTER TABLE dbo.Vendors ADD Cnic nvarchar(20) NULL;
END
GO
