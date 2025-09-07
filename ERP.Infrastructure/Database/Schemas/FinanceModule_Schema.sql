-- =============================================
-- FINANCE MODULE DATABASE SCHEMA
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Finance Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Finance')
BEGIN
    EXEC('CREATE SCHEMA [Finance]')
END
GO

-- =============================================
-- CHART OF ACCOUNTS TABLE
-- =============================================
CREATE TABLE [Finance].[ChartOfAccounts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [AccountCode] NVARCHAR(50) NOT NULL,
    [AccountName] NVARCHAR(200) NOT NULL,
    [AccountType] NVARCHAR(50) NOT NULL,
    [ParentAccountId] UNIQUEIDENTIFIER,
    [Level] INT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Description] NVARCHAR(500),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- BANKS TABLE
-- =============================================
CREATE TABLE [Finance].[Banks] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BankName] NVARCHAR(200) NOT NULL,
    [BankCode] NVARCHAR(50) NOT NULL,
    [SwiftCode] NVARCHAR(20),
    [Address] NVARCHAR(500),
    [Phone] NVARCHAR(20),
    [Email] NVARCHAR(200),
    [Website] NVARCHAR(200),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- BANK ACCOUNTS TABLE
-- =============================================
CREATE TABLE [Finance].[BankAccounts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BankId] UNIQUEIDENTIFIER NOT NULL,
    [AccountName] NVARCHAR(200) NOT NULL,
    [AccountNumber] NVARCHAR(50) NOT NULL,
    [AccountType] NVARCHAR(50) NOT NULL,
    [Currency] NVARCHAR(10) NOT NULL,
    [OpeningBalance] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CurrentBalance] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BankId]) REFERENCES [Finance].[Banks]([Id])
);
GO

-- =============================================
-- BANK TRANSACTIONS TABLE
-- =============================================
CREATE TABLE [Finance].[BankTransactions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BankAccountId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionDate] DATETIME2 NOT NULL,
    [TransactionType] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Balance] DECIMAL(18,2) NOT NULL,
    [Description] NVARCHAR(500),
    [Reference] NVARCHAR(100),
    [IsReconciled] BIT NOT NULL DEFAULT 0,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BankAccountId]) REFERENCES [Finance].[BankAccounts]([Id])
);
GO

-- =============================================
-- BANK RECONCILIATIONS TABLE
-- =============================================
CREATE TABLE [Finance].[BankReconciliations] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BankAccountId] UNIQUEIDENTIFIER NOT NULL,
    [StatementDate] DATETIME2 NOT NULL,
    [StatementBalance] DECIMAL(18,2) NOT NULL,
    [BookBalance] DECIMAL(18,2) NOT NULL,
    [ReconciledBalance] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [ReconciledBy] UNIQUEIDENTIFIER,
    [ReconciledAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BankAccountId]) REFERENCES [Finance].[BankAccounts]([Id])
);
GO

-- =============================================
-- INVOICES TABLE
-- =============================================
CREATE TABLE [Finance].[Invoices] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [InvoiceNumber] NVARCHAR(50) NOT NULL,
    [InvoiceDate] DATETIME2 NOT NULL,
    [DueDate] DATETIME2 NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [SubTotal] DECIMAL(18,2) NOT NULL,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [PaidAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [BalanceAmount] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [PaymentTerms] NVARCHAR(200),
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- INVOICE ITEMS TABLE
-- =============================================
CREATE TABLE [Finance].[InvoiceItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL,
    [ItemName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [Quantity] DECIMAL(18,2) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [TaxRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [LineTotal] DECIMAL(18,2) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([InvoiceId]) REFERENCES [Finance].[Invoices]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- BILLS TABLE
-- =============================================
CREATE TABLE [Finance].[Bills] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BillNumber] NVARCHAR(50) NOT NULL,
    [BillDate] DATETIME2 NOT NULL,
    [DueDate] DATETIME2 NOT NULL,
    [VendorId] UNIQUEIDENTIFIER NOT NULL,
    [SubTotal] DECIMAL(18,2) NOT NULL,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [PaidAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [BalanceAmount] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [PaymentTerms] NVARCHAR(200),
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- BILL ITEMS TABLE
-- =============================================
CREATE TABLE [Finance].[BillItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BillId] UNIQUEIDENTIFIER NOT NULL,
    [ItemName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [Quantity] DECIMAL(18,2) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [TaxRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [LineTotal] DECIMAL(18,2) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BillId]) REFERENCES [Finance].[Bills]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- PAYMENTS TABLE
-- =============================================
CREATE TABLE [Finance].[Payments] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PaymentNumber] NVARCHAR(50) NOT NULL,
    [PaymentDate] DATETIME2 NOT NULL,
    [PaymentType] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [BankAccountId] UNIQUEIDENTIFIER,
    [Reference] NVARCHAR(100),
    [Description] NVARCHAR(500),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BankAccountId]) REFERENCES [Finance].[BankAccounts]([Id])
);
GO

-- =============================================
-- JOURNAL ENTRIES TABLE
-- =============================================
CREATE TABLE [Finance].[JournalEntries] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EntryNumber] NVARCHAR(50) NOT NULL,
    [EntryDate] DATETIME2 NOT NULL,
    [Reference] NVARCHAR(100),
    [Description] NVARCHAR(500),
    [TotalDebit] DECIMAL(18,2) NOT NULL,
    [TotalCredit] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [PostedBy] UNIQUEIDENTIFIER,
    [PostedAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- JOURNAL ENTRY LINES TABLE
-- =============================================
CREATE TABLE [Finance].[JournalEntryLines] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [JournalEntryId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [DebitAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CreditAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Description] NVARCHAR(500),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([JournalEntryId]) REFERENCES [Finance].[JournalEntries]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([AccountId]) REFERENCES [Finance].[ChartOfAccounts]([Id])
);
GO

-- =============================================
-- TAX RATES TABLE
-- =============================================
CREATE TABLE [Finance].[TaxRates] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TaxName] NVARCHAR(100) NOT NULL,
    [TaxRate] DECIMAL(5,2) NOT NULL,
    [TaxType] NVARCHAR(50) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Description] NVARCHAR(500),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- CURRENCIES TABLE
-- =============================================
CREATE TABLE [Finance].[Currencies] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CurrencyCode] NVARCHAR(10) NOT NULL,
    [CurrencyName] NVARCHAR(100) NOT NULL,
    [Symbol] NVARCHAR(10),
    [ExchangeRate] DECIMAL(18,6) NOT NULL DEFAULT 1,
    [IsBaseCurrency] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- INDEXES FOR FINANCE MODULE
-- =============================================
CREATE INDEX IX_ChartOfAccounts_TenantId ON [Finance].[ChartOfAccounts]([TenantId]);
CREATE INDEX IX_ChartOfAccounts_AccountCode ON [Finance].[ChartOfAccounts]([AccountCode]);
CREATE INDEX IX_Banks_TenantId ON [Finance].[Banks]([TenantId]);
CREATE INDEX IX_BankAccounts_BankId ON [Finance].[BankAccounts]([BankId]);
CREATE INDEX IX_BankTransactions_BankAccountId ON [Finance].[BankTransactions]([BankAccountId]);
CREATE INDEX IX_BankTransactions_TransactionDate ON [Finance].[BankTransactions]([TransactionDate]);
CREATE INDEX IX_BankReconciliations_BankAccountId ON [Finance].[BankReconciliations]([BankAccountId]);
CREATE INDEX IX_Invoices_TenantId ON [Finance].[Invoices]([TenantId]);
CREATE INDEX IX_Invoices_CustomerId ON [Finance].[Invoices]([CustomerId]);
CREATE INDEX IX_Invoices_InvoiceNumber ON [Finance].[Invoices]([InvoiceNumber]);
CREATE INDEX IX_InvoiceItems_InvoiceId ON [Finance].[InvoiceItems]([InvoiceId]);
CREATE INDEX IX_Bills_TenantId ON [Finance].[Bills]([TenantId]);
CREATE INDEX IX_Bills_VendorId ON [Finance].[Bills]([VendorId]);
CREATE INDEX IX_Bills_BillNumber ON [Finance].[Bills]([BillNumber]);
CREATE INDEX IX_BillItems_BillId ON [Finance].[BillItems]([BillId]);
CREATE INDEX IX_Payments_TenantId ON [Finance].[Payments]([TenantId]);
CREATE INDEX IX_Payments_BankAccountId ON [Finance].[Payments]([BankAccountId]);
CREATE INDEX IX_JournalEntries_TenantId ON [Finance].[JournalEntries]([TenantId]);
CREATE INDEX IX_JournalEntries_EntryNumber ON [Finance].[JournalEntries]([EntryNumber]);
CREATE INDEX IX_JournalEntryLines_JournalEntryId ON [Finance].[JournalEntryLines]([JournalEntryId]);
CREATE INDEX IX_JournalEntryLines_AccountId ON [Finance].[JournalEntryLines]([AccountId]);
CREATE INDEX IX_TaxRates_TenantId ON [Finance].[TaxRates]([TenantId]);
CREATE INDEX IX_Currencies_TenantId ON [Finance].[Currencies]([TenantId]);

-- =============================================
-- FISCAL YEARS TABLE
-- =============================================
CREATE TABLE [Finance].[FiscalYears] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [FiscalYear] NVARCHAR(10) NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 0,
    [IsClosed] BIT NOT NULL DEFAULT 0,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- PERIODS TABLE
-- =============================================
CREATE TABLE [Finance].[Periods] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [FiscalYearId] UNIQUEIDENTIFIER NOT NULL,
    [PeriodName] NVARCHAR(50) NOT NULL,
    [PeriodNumber] INT NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [IsClosed] BIT NOT NULL DEFAULT 0,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([FiscalYearId]) REFERENCES [Finance].[FiscalYears]([Id])
);
GO

-- =============================================
-- PAYMENT TERMS TABLE
-- =============================================
CREATE TABLE [Finance].[PaymentTerms] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TermName] NVARCHAR(100) NOT NULL,
    [Days] INT NOT NULL,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- BUDGET HEADERS TABLE
-- =============================================
CREATE TABLE [Finance].[BudgetHeaders] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BudgetName] NVARCHAR(200) NOT NULL,
    [FiscalYearId] UNIQUEIDENTIFIER NOT NULL,
    [BudgetType] NVARCHAR(50) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [TotalBudget] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [ApprovedBy] UNIQUEIDENTIFIER,
    [ApprovedAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([FiscalYearId]) REFERENCES [Finance].[FiscalYears]([Id])
);
GO

-- =============================================
-- BUDGET LINES TABLE
-- =============================================
CREATE TABLE [Finance].[BudgetLines] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [BudgetHeaderId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [DepartmentId] UNIQUEIDENTIFIER,
    [BudgetAmount] DECIMAL(18,2) NOT NULL,
    [ActualAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Variance] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([BudgetHeaderId]) REFERENCES [Finance].[BudgetHeaders]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([AccountId]) REFERENCES [Finance].[ChartOfAccounts]([Id])
);
GO

-- =============================================
-- FIXED ASSETS TABLE
-- =============================================
CREATE TABLE [Finance].[FixedAssets] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [AssetCode] NVARCHAR(50) NOT NULL,
    [AssetName] NVARCHAR(200) NOT NULL,
    [AssetCategory] NVARCHAR(100) NOT NULL,
    [PurchaseDate] DATETIME2 NOT NULL,
    [PurchaseCost] DECIMAL(18,2) NOT NULL,
    [SalvageValue] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [UsefulLifeYears] INT NOT NULL,
    [DepreciationMethod] NVARCHAR(50) NOT NULL,
    [CurrentValue] DECIMAL(18,2) NOT NULL,
    [Location] NVARCHAR(200),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- DEPRECIATION SCHEDULES TABLE
-- =============================================
CREATE TABLE [Finance].[DepreciationSchedules] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [AssetId] UNIQUEIDENTIFIER NOT NULL,
    [FiscalYearId] UNIQUEIDENTIFIER NOT NULL,
    [PeriodId] UNIQUEIDENTIFIER NOT NULL,
    [DepreciationAmount] DECIMAL(18,2) NOT NULL,
    [AccumulatedDepreciation] DECIMAL(18,2) NOT NULL,
    [BookValue] DECIMAL(18,2) NOT NULL,
    [IsPosted] BIT NOT NULL DEFAULT 0,
    [PostedAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([AssetId]) REFERENCES [Finance].[FixedAssets]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([FiscalYearId]) REFERENCES [Finance].[FiscalYears]([Id]),
    FOREIGN KEY ([PeriodId]) REFERENCES [Finance].[Periods]([Id])
);
GO

-- =============================================
-- ADDITIONAL INDEXES FOR FINANCE MODULE
-- =============================================
CREATE INDEX IX_FiscalYears_TenantId ON [Finance].[FiscalYears]([TenantId]);
CREATE INDEX IX_FiscalYears_IsActive ON [Finance].[FiscalYears]([IsActive]);
CREATE INDEX IX_Periods_FiscalYearId ON [Finance].[Periods]([FiscalYearId]);
CREATE INDEX IX_Periods_TenantId ON [Finance].[Periods]([TenantId]);
CREATE INDEX IX_PaymentTerms_TenantId ON [Finance].[PaymentTerms]([TenantId]);
CREATE INDEX IX_BudgetHeaders_TenantId ON [Finance].[BudgetHeaders]([TenantId]);
CREATE INDEX IX_BudgetHeaders_FiscalYearId ON [Finance].[BudgetHeaders]([FiscalYearId]);
CREATE INDEX IX_BudgetLines_BudgetHeaderId ON [Finance].[BudgetLines]([BudgetHeaderId]);
CREATE INDEX IX_BudgetLines_AccountId ON [Finance].[BudgetLines]([AccountId]);
CREATE INDEX IX_FixedAssets_TenantId ON [Finance].[FixedAssets]([TenantId]);
CREATE INDEX IX_FixedAssets_AssetCode ON [Finance].[FixedAssets]([AssetCode]);
CREATE INDEX IX_DepreciationSchedules_AssetId ON [Finance].[DepreciationSchedules]([AssetId]);
CREATE INDEX IX_DepreciationSchedules_FiscalYearId ON [Finance].[DepreciationSchedules]([FiscalYearId]);

PRINT 'Finance Module schema created successfully!'