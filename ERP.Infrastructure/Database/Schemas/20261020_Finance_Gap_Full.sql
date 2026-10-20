-- Full Finance gap set: glue columns + depth tables (Finance schema)
IF SCHEMA_ID(N'Finance') IS NULL EXEC(N'CREATE SCHEMA [Finance]');
GO

-- ===== Phase 1: glue / orphans =====
IF COL_LENGTH(N'dbo.Invoices', N'SourceSalesInvoiceId') IS NULL
    ALTER TABLE [dbo].[Invoices] ADD [SourceSalesInvoiceId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.Invoices', N'Notes') IS NULL
    ALTER TABLE [dbo].[Invoices] ADD [Notes] NVARCHAR(2000) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Invoices_SourceSalesInvoiceId' AND object_id = OBJECT_ID(N'dbo.Invoices'))
    CREATE UNIQUE INDEX UX_Invoices_SourceSalesInvoiceId ON [dbo].[Invoices]([SourceSalesInvoiceId]) WHERE [SourceSalesInvoiceId] IS NOT NULL;
GO

IF COL_LENGTH(N'dbo.Bills', N'SourcePurchaseInvoiceId') IS NULL
    ALTER TABLE [dbo].[Bills] ADD [SourcePurchaseInvoiceId] UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Bills_SourcePurchaseInvoiceId' AND object_id = OBJECT_ID(N'dbo.Bills'))
    CREATE UNIQUE INDEX UX_Bills_SourcePurchaseInvoiceId ON [dbo].[Bills]([SourcePurchaseInvoiceId]) WHERE [SourcePurchaseInvoiceId] IS NOT NULL;
GO

IF COL_LENGTH(N'dbo.PaymentApplications', N'BillId') IS NULL
    ALTER TABLE [dbo].[PaymentApplications] ADD [BillId] UNIQUEIDENTIFIER NULL;
GO
-- Allow InvoiceId null for AP-only applications (idempotent if already nullable)
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PaymentApplications') AND name = N'InvoiceId' AND is_nullable = 0)
    ALTER TABLE [dbo].[PaymentApplications] ALTER COLUMN [InvoiceId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.Journals', N'ReversedFromJournalId') IS NULL
    ALTER TABLE [dbo].[Journals] ADD [ReversedFromJournalId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'Finance.AccountingPeriods', N'IsLocked') IS NULL
    ALTER TABLE [Finance].[AccountingPeriods] ADD [IsLocked] BIT NOT NULL CONSTRAINT DF_AcctPeriod_IsLocked DEFAULT (0);
GO
IF COL_LENGTH(N'Finance.AccountingPeriods', N'ClosedAt') IS NULL
    ALTER TABLE [Finance].[AccountingPeriods] ADD [ClosedAt] DATETIME2 NULL;
GO
IF COL_LENGTH(N'Finance.AccountingPeriods', N'ClosedBy') IS NULL
    ALTER TABLE [Finance].[AccountingPeriods] ADD [ClosedBy] UNIQUEIDENTIFIER NULL;
GO

IF OBJECT_ID(N'[Finance].[TaxTransactions]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[TaxTransactions] (
        [TaxTransactionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [TaxCodeId] UNIQUEIDENTIFIER NOT NULL,
        [EntityReferenceTable] NVARCHAR(100) NOT NULL,
        [EntityReferenceId] UNIQUEIDENTIFIER NOT NULL,
        [TaxAmount] DECIMAL(28,8) NOT NULL,
        [TaxableAmount] DECIMAL(28,8) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_TaxTx_CreatedAt DEFAULT (GETUTCDATE()),
        [CreatedBy] UNIQUEIDENTIFIER NULL
    );
    CREATE INDEX IX_TaxTx_Tenant_Entity ON [Finance].[TaxTransactions]([TenantId], [EntityReferenceTable], [EntityReferenceId]);
END
GO

-- ===== Phase 2: Budget lines + Tax groups =====
IF OBJECT_ID(N'[Finance].[BudgetLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[BudgetLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [BudgetId] UNIQUEIDENTIFIER NOT NULL,
        [PeriodStart] DATE NOT NULL,
        [PeriodEnd] DATE NOT NULL,
        [Amount] DECIMAL(18,4) NOT NULL CONSTRAINT DF_BudgetLine_Amount DEFAULT (0),
        [IsActive] BIT NOT NULL CONSTRAINT DF_BudgetLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_BudgetLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_BudgetLines_Budget ON [Finance].[BudgetLines]([BudgetId]);
END
GO

IF OBJECT_ID(N'[Finance].[TaxGroups]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[TaxGroups] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [TaxType] NVARCHAR(50) NOT NULL CONSTRAINT DF_TaxGroup_TaxType DEFAULT (N'Sales'),
        [IsActive] BIT NOT NULL CONSTRAINT DF_TaxGroup_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_TaxGroup_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_TaxGroups_Tenant_Code ON [Finance].[TaxGroups]([TenantId], [Code]);
END
GO

IF OBJECT_ID(N'[Finance].[TaxGroupLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[TaxGroupLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [TaxGroupId] UNIQUEIDENTIFIER NOT NULL,
        [TaxCodeId] UNIQUEIDENTIFIER NOT NULL,
        [Sequence] INT NOT NULL CONSTRAINT DF_TaxGroupLine_Seq DEFAULT (1),
        [IsCompound] BIT NOT NULL CONSTRAINT DF_TaxGroupLine_Compound DEFAULT (0),
        [IsActive] BIT NOT NULL CONSTRAINT DF_TaxGroupLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_TaxGroupLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_TaxGroupLines_Group FOREIGN KEY ([TaxGroupId]) REFERENCES [Finance].[TaxGroups]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_TaxGroupLines_Group ON [Finance].[TaxGroupLines]([TaxGroupId]);
END
GO

IF COL_LENGTH(N'dbo.TaxCodes', N'TaxGroupId') IS NULL
    ALTER TABLE [dbo].[TaxCodes] ADD [TaxGroupId] UNIQUEIDENTIFIER NULL;
GO

-- ===== Phase 3: Fixed assets =====
IF OBJECT_ID(N'[Finance].[AssetCategories]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[AssetCategories] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [UsefulLifeMonths] INT NOT NULL CONSTRAINT DF_AssetCat_Life DEFAULT (60),
        [DepreciationMethod] NVARCHAR(50) NOT NULL CONSTRAINT DF_AssetCat_Method DEFAULT (N'StraightLine'),
        [AssetAccountId] UNIQUEIDENTIFIER NULL,
        [AccumDepAccountId] UNIQUEIDENTIFIER NULL,
        [DepExpenseAccountId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_AssetCat_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_AssetCat_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_AssetCategories_Tenant_Code ON [Finance].[AssetCategories]([TenantId], [Code]);
END
GO

IF COL_LENGTH(N'dbo.FixedAssets', N'AssetCategoryId') IS NULL
    ALTER TABLE [dbo].[FixedAssets] ADD [AssetCategoryId] UNIQUEIDENTIFIER NULL;
GO

IF OBJECT_ID(N'[Finance].[AssetDisposals]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[AssetDisposals] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [AssetId] UNIQUEIDENTIFIER NOT NULL,
        [DisposalDate] DATE NOT NULL,
        [Proceeds] DECIMAL(18,4) NOT NULL CONSTRAINT DF_AssetDisp_Proceeds DEFAULT (0),
        [BookValue] DECIMAL(18,4) NOT NULL CONSTRAINT DF_AssetDisp_Book DEFAULT (0),
        [GainLoss] DECIMAL(18,4) NOT NULL CONSTRAINT DF_AssetDisp_GL DEFAULT (0),
        [PostedJournalId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_AssetDisp_Status DEFAULT (N'Draft'),
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_AssetDisp_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_AssetDisp_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_AssetDisposals_Asset ON [Finance].[AssetDisposals]([AssetId]);
END
GO

IF OBJECT_ID(N'[Finance].[AssetTransfers]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[AssetTransfers] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [AssetId] UNIQUEIDENTIFIER NOT NULL,
        [TransferDate] DATE NOT NULL,
        [FromCostCenterId] UNIQUEIDENTIFIER NULL,
        [ToCostCenterId] UNIQUEIDENTIFIER NULL,
        [FromBranchId] UNIQUEIDENTIFIER NULL,
        [ToBranchId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_AssetXfer_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_AssetXfer_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_AssetTransfers_Asset ON [Finance].[AssetTransfers]([AssetId]);
END
GO

-- ===== Phase 4: Banks =====
IF OBJECT_ID(N'[Finance].[BankReconciliationLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[BankReconciliationLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ReconciliationId] UNIQUEIDENTIFIER NOT NULL,
        [BankTransactionId] UNIQUEIDENTIFIER NOT NULL,
        [MatchedAmount] DECIMAL(18,4) NOT NULL CONSTRAINT DF_BankReconLine_Amt DEFAULT (0),
        [IsMatched] BIT NOT NULL CONSTRAINT DF_BankReconLine_Matched DEFAULT (1),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_BankReconLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_BankReconLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_BankReconLines_Recon ON [Finance].[BankReconciliationLines]([ReconciliationId]);
    CREATE UNIQUE INDEX UX_BankReconLines_Tx ON [Finance].[BankReconciliationLines]([ReconciliationId], [BankTransactionId]);
END
GO

IF OBJECT_ID(N'[Finance].[FundTransfers]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[FundTransfers] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [TransferNumber] NVARCHAR(50) NOT NULL,
        [TransferDate] DATE NOT NULL,
        [FromBankAccountId] UNIQUEIDENTIFIER NULL,
        [ToBankAccountId] UNIQUEIDENTIFIER NULL,
        [FromAccountId] UNIQUEIDENTIFIER NULL,
        [ToAccountId] UNIQUEIDENTIFIER NULL,
        [Amount] DECIMAL(18,4) NOT NULL,
        [CurrencyCode] NVARCHAR(10) NOT NULL CONSTRAINT DF_FundXfer_Ccy DEFAULT (N'USD'),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_FundXfer_Status DEFAULT (N'Draft'),
        [PostedJournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_FundXfer_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_FundXfer_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_FundTransfers_Tenant_No ON [Finance].[FundTransfers]([TenantId], [TransferNumber]);
END
GO

-- ===== Phase 5: AR/AP memos, advances, WHT =====
IF OBJECT_ID(N'[Finance].[CreditMemos]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[CreditMemos] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [MemoNumber] NVARCHAR(50) NOT NULL,
        [CustomerId] UNIQUEIDENTIFIER NOT NULL,
        [MemoDate] DATE NOT NULL,
        [CurrencyCode] NVARCHAR(10) NOT NULL CONSTRAINT DF_CreditMemo_Ccy DEFAULT (N'USD'),
        [SubTotal] DECIMAL(18,4) NOT NULL CONSTRAINT DF_CreditMemo_Sub DEFAULT (0),
        [TaxTotal] DECIMAL(18,4) NOT NULL CONSTRAINT DF_CreditMemo_Tax DEFAULT (0),
        [TotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT DF_CreditMemo_Tot DEFAULT (0),
        [Balance] DECIMAL(18,4) NOT NULL CONSTRAINT DF_CreditMemo_Bal DEFAULT (0),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_CreditMemo_Status DEFAULT (N'Draft'),
        [PostedJournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_CreditMemo_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_CreditMemo_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_CreditMemos_Tenant_No ON [Finance].[CreditMemos]([TenantId], [MemoNumber]);
END
GO

IF OBJECT_ID(N'[Finance].[CreditMemoLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[CreditMemoLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CreditMemoId] UNIQUEIDENTIFIER NOT NULL,
        [LineNumber] INT NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Amount] DECIMAL(18,4) NOT NULL,
        [TaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT DF_CreditMemoLine_Tax DEFAULT (0),
        [AccountId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_CreditMemoLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_CreditMemoLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_CreditMemoLines_Header FOREIGN KEY ([CreditMemoId]) REFERENCES [Finance].[CreditMemos]([Id]) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[Finance].[DebitMemos]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[DebitMemos] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [MemoNumber] NVARCHAR(50) NOT NULL,
        [VendorId] UNIQUEIDENTIFIER NOT NULL,
        [MemoDate] DATE NOT NULL,
        [CurrencyCode] NVARCHAR(10) NOT NULL CONSTRAINT DF_DebitMemo_Ccy DEFAULT (N'USD'),
        [SubTotal] DECIMAL(18,4) NOT NULL CONSTRAINT DF_DebitMemo_Sub DEFAULT (0),
        [TaxTotal] DECIMAL(18,4) NOT NULL CONSTRAINT DF_DebitMemo_Tax DEFAULT (0),
        [TotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT DF_DebitMemo_Tot DEFAULT (0),
        [Balance] DECIMAL(18,4) NOT NULL CONSTRAINT DF_DebitMemo_Bal DEFAULT (0),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_DebitMemo_Status DEFAULT (N'Draft'),
        [PostedJournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_DebitMemo_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_DebitMemo_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_DebitMemos_Tenant_No ON [Finance].[DebitMemos]([TenantId], [MemoNumber]);
END
GO

IF OBJECT_ID(N'[Finance].[DebitMemoLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[DebitMemoLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [DebitMemoId] UNIQUEIDENTIFIER NOT NULL,
        [LineNumber] INT NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Amount] DECIMAL(18,4) NOT NULL,
        [TaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT DF_DebitMemoLine_Tax DEFAULT (0),
        [AccountId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_DebitMemoLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_DebitMemoLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_DebitMemoLines_Header FOREIGN KEY ([DebitMemoId]) REFERENCES [Finance].[DebitMemos]([Id]) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[Finance].[CustomerAdvances]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[CustomerAdvances] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [AdvanceNumber] NVARCHAR(50) NOT NULL,
        [CustomerId] UNIQUEIDENTIFIER NOT NULL,
        [AdvanceDate] DATE NOT NULL,
        [Amount] DECIMAL(18,4) NOT NULL,
        [Balance] DECIMAL(18,4) NOT NULL,
        [CurrencyCode] NVARCHAR(10) NOT NULL CONSTRAINT DF_CustAdv_Ccy DEFAULT (N'USD'),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_CustAdv_Status DEFAULT (N'Open'),
        [PaymentId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_CustAdv_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_CustAdv_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_CustomerAdvances_Tenant_No ON [Finance].[CustomerAdvances]([TenantId], [AdvanceNumber]);
END
GO

IF OBJECT_ID(N'[Finance].[VendorAdvances]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[VendorAdvances] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [AdvanceNumber] NVARCHAR(50) NOT NULL,
        [VendorId] UNIQUEIDENTIFIER NOT NULL,
        [AdvanceDate] DATE NOT NULL,
        [Amount] DECIMAL(18,4) NOT NULL,
        [Balance] DECIMAL(18,4) NOT NULL,
        [CurrencyCode] NVARCHAR(10) NOT NULL CONSTRAINT DF_VendAdv_Ccy DEFAULT (N'USD'),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_VendAdv_Status DEFAULT (N'Open'),
        [PaymentId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_VendAdv_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_VendAdv_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_VendorAdvances_Tenant_No ON [Finance].[VendorAdvances]([TenantId], [AdvanceNumber]);
END
GO

IF OBJECT_ID(N'[Finance].[AdvanceApplications]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[AdvanceApplications] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CustomerAdvanceId] UNIQUEIDENTIFIER NULL,
        [VendorAdvanceId] UNIQUEIDENTIFIER NULL,
        [InvoiceId] UNIQUEIDENTIFIER NULL,
        [BillId] UNIQUEIDENTIFIER NULL,
        [AppliedAmount] DECIMAL(18,4) NOT NULL,
        [AppliedDate] DATE NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_AdvApp_Status DEFAULT (N'Applied'),
        [IsActive] BIT NOT NULL CONSTRAINT DF_AdvApp_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_AdvApp_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
END
GO

IF OBJECT_ID(N'[Finance].[WithholdingTaxLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[WithholdingTaxLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PaymentId] UNIQUEIDENTIFIER NOT NULL,
        [TaxCodeId] UNIQUEIDENTIFIER NOT NULL,
        [BaseAmount] DECIMAL(18,4) NOT NULL,
        [WithholdingAmount] DECIMAL(18,4) NOT NULL,
        [PayableAccountId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_WhtLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_WhtLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_WhtLines_Payment ON [Finance].[WithholdingTaxLines]([PaymentId]);
END
GO

-- ===== Phase 6: Recurring, FX, IC =====
IF OBJECT_ID(N'[Finance].[RecurringJournals]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[RecurringJournals] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [JournalType] NVARCHAR(50) NOT NULL CONSTRAINT DF_RecurJ_Type DEFAULT (N'Journal'),
        [Frequency] NVARCHAR(30) NOT NULL CONSTRAINT DF_RecurJ_Freq DEFAULT (N'Monthly'),
        [NextRunDate] DATE NOT NULL,
        [EndDate] DATE NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_RecurJ_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_RecurJ_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
END
GO

IF OBJECT_ID(N'[Finance].[RecurringJournalLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[RecurringJournalLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [RecurringJournalId] UNIQUEIDENTIFIER NOT NULL,
        [AccountId] UNIQUEIDENTIFIER NOT NULL,
        [CostCenterId] UNIQUEIDENTIFIER NULL,
        [Debit] DECIMAL(18,4) NOT NULL CONSTRAINT DF_RecurJL_Dr DEFAULT (0),
        [Credit] DECIMAL(18,4) NOT NULL CONSTRAINT DF_RecurJL_Cr DEFAULT (0),
        [Narration] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_RecurJL_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_RecurJL_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_RecurJL_Header FOREIGN KEY ([RecurringJournalId]) REFERENCES [Finance].[RecurringJournals]([Id]) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[Finance].[FxRevaluationRuns]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[FxRevaluationRuns] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [RunDate] DATE NOT NULL,
        [RateSource] NVARCHAR(100) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_FxRun_Status DEFAULT (N'Draft'),
        [PostedJournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_FxRun_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_FxRun_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
END
GO

IF OBJECT_ID(N'[Finance].[FxRevaluationLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[FxRevaluationLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [FxRevaluationRunId] UNIQUEIDENTIFIER NOT NULL,
        [AccountId] UNIQUEIDENTIFIER NULL,
        [PartyType] NVARCHAR(30) NULL,
        [PartyId] UNIQUEIDENTIFIER NULL,
        [CurrencyCode] NVARCHAR(10) NOT NULL,
        [ForeignBalance] DECIMAL(18,4) NOT NULL CONSTRAINT DF_FxLine_FBal DEFAULT (0),
        [BookBalance] DECIMAL(18,4) NOT NULL CONSTRAINT DF_FxLine_BBal DEFAULT (0),
        [RevaluedBalance] DECIMAL(18,4) NOT NULL CONSTRAINT DF_FxLine_RBal DEFAULT (0),
        [UnrealizedGainLoss] DECIMAL(18,4) NOT NULL CONSTRAINT DF_FxLine_UGL DEFAULT (0),
        [IsActive] BIT NOT NULL CONSTRAINT DF_FxLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_FxLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_FxLine_Run FOREIGN KEY ([FxRevaluationRunId]) REFERENCES [Finance].[FxRevaluationRuns]([Id]) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[Finance].[IntercompanySettlements]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[IntercompanySettlements] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SettlementNumber] NVARCHAR(50) NOT NULL,
        [SettlementDate] DATE NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_IcSettle_Status DEFAULT (N'Draft'),
        [PostedJournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_IcSettle_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_IcSettle_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_IcSettlements_Tenant_No ON [Finance].[IntercompanySettlements]([TenantId], [SettlementNumber]);
END
GO

IF COL_LENGTH(N'dbo.IntercompanyTransactions', N'SettlementId') IS NULL
    ALTER TABLE [dbo].[IntercompanyTransactions] ADD [SettlementId] UNIQUEIDENTIFIER NULL;
GO
