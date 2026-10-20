-- Dual tax (Tax1 + Tax2) on sales orders / invoices.
IF COL_LENGTH('dbo.SalesOrderLines', 'Tax1Rate') IS NULL AND OBJECT_ID('dbo.SalesOrderLines', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesOrderLines ADD Tax1Rate decimal(18,4) NOT NULL CONSTRAINT DF_SalesOrderLines_Tax1Rate DEFAULT (0);
    ALTER TABLE dbo.SalesOrderLines ADD Tax1Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrderLines_Tax1Amount DEFAULT (0);
    ALTER TABLE dbo.SalesOrderLines ADD Tax2Rate decimal(18,4) NOT NULL CONSTRAINT DF_SalesOrderLines_Tax2Rate DEFAULT (0);
    ALTER TABLE dbo.SalesOrderLines ADD Tax2Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrderLines_Tax2Amount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.SalesOrders', 'Tax1Amount') IS NULL AND OBJECT_ID('dbo.SalesOrders', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesOrders ADD Tax1Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrders_Tax1Amount DEFAULT (0);
    ALTER TABLE dbo.SalesOrders ADD Tax2Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesOrders_Tax2Amount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.SalesInvoiceLines', 'Tax1Rate') IS NULL AND OBJECT_ID('dbo.SalesInvoiceLines', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesInvoiceLines ADD Tax1Rate decimal(18,4) NOT NULL CONSTRAINT DF_SalesInvoiceLines_Tax1Rate DEFAULT (0);
    ALTER TABLE dbo.SalesInvoiceLines ADD Tax1Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoiceLines_Tax1Amount DEFAULT (0);
    ALTER TABLE dbo.SalesInvoiceLines ADD Tax2Rate decimal(18,4) NOT NULL CONSTRAINT DF_SalesInvoiceLines_Tax2Rate DEFAULT (0);
    ALTER TABLE dbo.SalesInvoiceLines ADD Tax2Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoiceLines_Tax2Amount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.SalesInvoices', 'Tax1Amount') IS NULL AND OBJECT_ID('dbo.SalesInvoices', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesInvoices ADD Tax1Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoices_Tax1Amount DEFAULT (0);
    ALTER TABLE dbo.SalesInvoices ADD Tax2Amount decimal(18,2) NOT NULL CONSTRAINT DF_SalesInvoices_Tax2Amount DEFAULT (0);
END
GO
