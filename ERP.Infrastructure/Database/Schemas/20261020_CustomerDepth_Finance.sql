-- Customer depth: Finance opening balances, periods, cheque books, expense claims
IF SCHEMA_ID(N'Finance') IS NULL EXEC(N'CREATE SCHEMA [Finance]');
GO

IF OBJECT_ID(N'[Finance].[OpeningBalances]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[OpeningBalances] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [FiscalYearId] UNIQUEIDENTIFIER NOT NULL,
        [ReferenceNo] NVARCHAR(50) NULL,
        [AsOfDate] DATE NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_OpenBal_Status DEFAULT (N'Draft'),
        [JournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_OpenBal_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_OpenBal_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_OpenBal_Tenant_FY ON [Finance].[OpeningBalances]([TenantId], [FiscalYearId]);
END
GO

IF OBJECT_ID(N'[Finance].[OpeningBalanceLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[OpeningBalanceLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [OpeningBalanceId] UNIQUEIDENTIFIER NOT NULL,
        [LineNumber] INT NOT NULL,
        [AccountId] UNIQUEIDENTIFIER NOT NULL,
        [Debit] DECIMAL(18,4) NOT NULL CONSTRAINT DF_OpenBalLine_Debit DEFAULT (0),
        [Credit] DECIMAL(18,4) NOT NULL CONSTRAINT DF_OpenBalLine_Credit DEFAULT (0),
        [CostCenterId] UNIQUEIDENTIFIER NULL,
        [ProjectId] UNIQUEIDENTIFIER NULL,
        [PartyType] NVARCHAR(30) NULL,
        [PartyId] UNIQUEIDENTIFIER NULL,
        [Narration] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_OpenBalLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_OpenBalLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_OpenBalLines_Header FOREIGN KEY ([OpeningBalanceId])
            REFERENCES [Finance].[OpeningBalances]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_OpenBalLines_Header ON [Finance].[OpeningBalanceLines]([OpeningBalanceId]);
END
GO

IF OBJECT_ID(N'[Finance].[AccountingPeriods]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[AccountingPeriods] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [FiscalYearId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [StartDate] DATE NOT NULL,
        [EndDate] DATE NOT NULL,
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_AcctPeriod_Status DEFAULT (N'Open'),
        [IsActive] BIT NOT NULL CONSTRAINT DF_AcctPeriod_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_AcctPeriod_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_AcctPeriod_Tenant_FY ON [Finance].[AccountingPeriods]([TenantId], [FiscalYearId]);
    CREATE INDEX IX_AcctPeriod_Tenant_Dates ON [Finance].[AccountingPeriods]([TenantId], [StartDate], [EndDate]);
END
GO

IF OBJECT_ID(N'[Finance].[ChequeBooks]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[ChequeBooks] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [BankAccountId] UNIQUEIDENTIFIER NOT NULL,
        [BookNumber] NVARCHAR(50) NOT NULL,
        [StartLeafNumber] INT NOT NULL,
        [EndLeafNumber] INT NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ChequeBook_Status DEFAULT (N'Active'),
        [IsActive] BIT NOT NULL CONSTRAINT DF_ChequeBook_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ChequeBook_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_ChequeBooks_Tenant_Book ON [Finance].[ChequeBooks]([TenantId], [BookNumber]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[Finance].[ChequeLeaves]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[ChequeLeaves] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ChequeBookId] UNIQUEIDENTIFIER NOT NULL,
        [LeafNumber] INT NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ChequeLeaf_Status DEFAULT (N'Blank'),
        [IssuedAt] DATETIME2 NULL,
        [JournalId] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ChequeLeaf_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ChequeLeaf_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_ChequeLeaves_Book FOREIGN KEY ([ChequeBookId])
            REFERENCES [Finance].[ChequeBooks]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_ChequeLeaves_Book_Leaf ON [Finance].[ChequeLeaves]([ChequeBookId], [LeafNumber]);
    CREATE INDEX IX_ChequeLeaves_Tenant_Status ON [Finance].[ChequeLeaves]([TenantId], [Status]);
END
GO

IF OBJECT_ID(N'[Finance].[ExpenseClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[ExpenseClaims] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ClaimNumber] NVARCHAR(50) NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NULL,
        [ProjectId] UNIQUEIDENTIFIER NULL,
        [ClaimDate] DATE NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ExpClaim_Status DEFAULT (N'Draft'),
        [TotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_ExpClaim_Total DEFAULT (0),
        [CurrencyCode] NVARCHAR(10) NOT NULL CONSTRAINT DF_ExpClaim_Currency DEFAULT (N'USD'),
        [Notes] NVARCHAR(1000) NULL,
        [SubmittedAt] DATETIME2 NULL,
        [ApprovedAt] DATETIME2 NULL,
        [ApprovedBy] UNIQUEIDENTIFIER NULL,
        [PaidAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ExpClaim_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ExpClaim_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_ExpClaim_Tenant_Number ON [Finance].[ExpenseClaims]([TenantId], [ClaimNumber]);
END
GO

IF OBJECT_ID(N'[Finance].[ExpenseClaimLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Finance].[ExpenseClaimLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ExpenseClaimId] UNIQUEIDENTIFIER NOT NULL,
        [LineNumber] INT NOT NULL,
        [ExpenseDate] DATE NOT NULL,
        [Category] NVARCHAR(100) NULL,
        [Description] NVARCHAR(500) NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [AccountId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ExpClaimLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ExpClaimLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_ExpClaimLines_Header FOREIGN KEY ([ExpenseClaimId])
            REFERENCES [Finance].[ExpenseClaims]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_ExpClaimLines_Header ON [Finance].[ExpenseClaimLines]([ExpenseClaimId]);
END
GO
