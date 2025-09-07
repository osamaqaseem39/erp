-- =============================================
-- FINANCE MODULE STORED PROCEDURES
-- Multi-tenant SaaS ERP System
-- =============================================

-- =============================================
-- CHART OF ACCOUNTS PROCEDURES
-- =============================================

-- Get Account by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAccountById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, AccountCode, AccountName, AccountType, ParentAccountId, Level,
        IsActive, Description, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[ChartOfAccounts]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get All Accounts for Tenant
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllAccounts]
    @TenantId UNIQUEIDENTIFIER,
    @AccountType NVARCHAR(50) = NULL,
    @ParentAccountId UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, AccountCode, AccountName, AccountType, ParentAccountId, Level,
        IsActive, Description, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[ChartOfAccounts]
    WHERE TenantId = @TenantId 
        AND IsActive = 1
        AND (@AccountType IS NULL OR AccountType = @AccountType)
        AND (@ParentAccountId IS NULL OR ParentAccountId = @ParentAccountId)
    ORDER BY AccountCode
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Finance].[ChartOfAccounts]
    WHERE TenantId = @TenantId 
        AND IsActive = 1
        AND (@AccountType IS NULL OR AccountType = @AccountType)
        AND (@ParentAccountId IS NULL OR ParentAccountId = @ParentAccountId);
END
GO

-- Create Account
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateAccount]
    @Id UNIQUEIDENTIFIER,
    @AccountCode NVARCHAR(50),
    @AccountName NVARCHAR(200),
    @AccountType NVARCHAR(50),
    @ParentAccountId UNIQUEIDENTIFIER = NULL,
    @Level INT,
    @Description NVARCHAR(500),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[ChartOfAccounts] (
        Id, AccountCode, AccountName, AccountType, ParentAccountId, Level,
        IsActive, Description, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @AccountCode, @AccountName, @AccountType, @ParentAccountId, @Level,
        1, @Description, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- BANK MANAGEMENT PROCEDURES
-- =============================================

-- Get Bank by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetBankById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, BankName, BankCode, SwiftCode, Address, Phone, Email, Website,
        IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[Banks]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get Bank Account by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetBankAccountById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ba.Id, ba.BankId, ba.AccountName, ba.AccountNumber, ba.AccountType,
        ba.Currency, ba.OpeningBalance, ba.CurrentBalance, ba.IsActive,
        ba.TenantId, ba.CreatedAt, ba.UpdatedAt, ba.CreatedBy, ba.UpdatedBy,
        b.BankName, b.BankCode
    FROM [Finance].[BankAccounts] ba
    INNER JOIN [Finance].[Banks] b ON ba.BankId = b.Id
    WHERE ba.Id = @Id AND ba.TenantId = @TenantId AND ba.IsActive = 1;
END
GO

-- Get Bank Transactions by Account
CREATE OR ALTER PROCEDURE [Finance].[sp_GetBankTransactionsByAccount]
    @BankAccountId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, BankAccountId, TransactionDate, TransactionType, Amount, Balance,
        Description, Reference, IsReconciled, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[BankTransactions]
    WHERE BankAccountId = @BankAccountId 
        AND TenantId = @TenantId
        AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
        AND (@EndDate IS NULL OR TransactionDate <= @EndDate)
    ORDER BY TransactionDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Finance].[BankTransactions]
    WHERE BankAccountId = @BankAccountId 
        AND TenantId = @TenantId
        AND (@StartDate IS NULL OR TransactionDate >= @StartDate)
        AND (@EndDate IS NULL OR TransactionDate <= @EndDate);
END
GO

-- =============================================
-- INVOICE MANAGEMENT PROCEDURES
-- =============================================

-- Get Invoice by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetInvoiceById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, InvoiceNumber, InvoiceDate, DueDate, CustomerId, SubTotal,
        TaxAmount, DiscountAmount, TotalAmount, PaidAmount, BalanceAmount,
        Status, PaymentTerms, Notes, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[Invoices]
    WHERE Id = @Id AND TenantId = @TenantId;
END
GO

-- Get Invoice Items by Invoice ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetInvoiceItemsByInvoiceId]
    @InvoiceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, InvoiceId, ItemName, Description, Quantity, UnitPrice,
        TaxRate, TaxAmount, DiscountRate, DiscountAmount, LineTotal,
        TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[InvoiceItems]
    WHERE InvoiceId = @InvoiceId AND TenantId = @TenantId
    ORDER BY Id;
END
GO

-- Create Invoice
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateInvoice]
    @Id UNIQUEIDENTIFIER,
    @InvoiceNumber NVARCHAR(50),
    @InvoiceDate DATETIME2,
    @DueDate DATETIME2,
    @CustomerId UNIQUEIDENTIFIER,
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @PaymentTerms NVARCHAR(200),
    @Notes NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[Invoices] (
        Id, InvoiceNumber, InvoiceDate, DueDate, CustomerId, SubTotal,
        TaxAmount, DiscountAmount, TotalAmount, PaidAmount, BalanceAmount,
        Status, PaymentTerms, Notes, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @InvoiceNumber, @InvoiceDate, @DueDate, @CustomerId, @SubTotal,
        @TaxAmount, @DiscountAmount, @TotalAmount, 0, @TotalAmount,
        'Draft', @PaymentTerms, @Notes, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Invoice Status
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateInvoiceStatus]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @PaidAmount DECIMAL(18,2) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [Finance].[Invoices]
    SET 
        Status = @Status,
        PaidAmount = ISNULL(@PaidAmount, PaidAmount),
        BalanceAmount = TotalAmount - ISNULL(@PaidAmount, PaidAmount),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- BILL MANAGEMENT PROCEDURES
-- =============================================

-- Get Bill by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetBillById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, BillNumber, BillDate, DueDate, VendorId, SubTotal,
        TaxAmount, DiscountAmount, TotalAmount, PaidAmount, BalanceAmount,
        Status, PaymentTerms, Notes, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[Bills]
    WHERE Id = @Id AND TenantId = @TenantId;
END
GO

-- Get Bill Items by Bill ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetBillItemsByBillId]
    @BillId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, BillId, ItemName, Description, Quantity, UnitPrice,
        TaxRate, TaxAmount, DiscountRate, DiscountAmount, LineTotal,
        TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[BillItems]
    WHERE BillId = @BillId AND TenantId = @TenantId
    ORDER BY Id;
END
GO

-- Create Bill
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateBill]
    @Id UNIQUEIDENTIFIER,
    @BillNumber NVARCHAR(50),
    @BillDate DATETIME2,
    @DueDate DATETIME2,
    @VendorId UNIQUEIDENTIFIER,
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @PaymentTerms NVARCHAR(200),
    @Notes NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[Bills] (
        Id, BillNumber, BillDate, DueDate, VendorId, SubTotal,
        TaxAmount, DiscountAmount, TotalAmount, PaidAmount, BalanceAmount,
        Status, PaymentTerms, Notes, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @BillNumber, @BillDate, @DueDate, @VendorId, @SubTotal,
        @TaxAmount, @DiscountAmount, @TotalAmount, 0, @TotalAmount,
        'Draft', @PaymentTerms, @Notes, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- PAYMENT MANAGEMENT PROCEDURES
-- =============================================

-- Get Payment by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetPaymentById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, PaymentNumber, PaymentDate, PaymentType, Amount, BankAccountId,
        Reference, Description, Status, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[Payments]
    WHERE Id = @Id AND TenantId = @TenantId;
END
GO

-- Create Payment
CREATE OR ALTER PROCEDURE [Finance].[sp_CreatePayment]
    @Id UNIQUEIDENTIFIER,
    @PaymentNumber NVARCHAR(50),
    @PaymentDate DATETIME2,
    @PaymentType NVARCHAR(50),
    @Amount DECIMAL(18,2),
    @BankAccountId UNIQUEIDENTIFIER = NULL,
    @Reference NVARCHAR(100),
    @Description NVARCHAR(500),
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[Payments] (
        Id, PaymentNumber, PaymentDate, PaymentType, Amount, BankAccountId,
        Reference, Description, Status, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @PaymentNumber, @PaymentDate, @PaymentType, @Amount, @BankAccountId,
        @Reference, @Description, @Status, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- JOURNAL ENTRY PROCEDURES
-- =============================================

-- Get Journal Entry by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetJournalEntryById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, EntryNumber, EntryDate, Reference, Description, TotalDebit,
        TotalCredit, Status, PostedBy, PostedAt, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[JournalEntries]
    WHERE Id = @Id AND TenantId = @TenantId;
END
GO

-- Get Journal Entry Lines by Entry ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetJournalEntryLinesByEntryId]
    @JournalEntryId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        jel.Id, jel.JournalEntryId, jel.AccountId, jel.DebitAmount, jel.CreditAmount,
        jel.Description, jel.TenantId, jel.CreatedAt, jel.UpdatedAt, jel.CreatedBy, jel.UpdatedBy,
        coa.AccountCode, coa.AccountName
    FROM [Finance].[JournalEntryLines] jel
    INNER JOIN [Finance].[ChartOfAccounts] coa ON jel.AccountId = coa.Id
    WHERE jel.JournalEntryId = @JournalEntryId AND jel.TenantId = @TenantId
    ORDER BY jel.Id;
END
GO

-- Create Journal Entry
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateJournalEntry]
    @Id UNIQUEIDENTIFIER,
    @EntryNumber NVARCHAR(50),
    @EntryDate DATETIME2,
    @Reference NVARCHAR(100),
    @Description NVARCHAR(500),
    @TotalDebit DECIMAL(18,2),
    @TotalCredit DECIMAL(18,2),
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[JournalEntries] (
        Id, EntryNumber, EntryDate, Reference, Description, TotalDebit,
        TotalCredit, Status, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @EntryNumber, @EntryDate, @Reference, @Description, @TotalDebit,
        @TotalCredit, @Status, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Post Journal Entry
CREATE OR ALTER PROCEDURE [Finance].[sp_PostJournalEntry]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PostedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [Finance].[JournalEntries]
    SET 
        Status = 'Posted',
        PostedBy = @PostedBy,
        PostedAt = GETUTCDATE(),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @PostedBy
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- TAX RATE PROCEDURES
-- =============================================

-- Get Tax Rate by ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetTaxRateById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, TaxName, TaxRate, TaxType, IsActive, Description,
        TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[TaxRates]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get All Tax Rates for Tenant
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllTaxRates]
    @TenantId UNIQUEIDENTIFIER,
    @TaxType NVARCHAR(50) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, TaxName, TaxRate, TaxType, IsActive, Description,
        TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[TaxRates]
    WHERE TenantId = @TenantId 
        AND IsActive = @IsActive
        AND (@TaxType IS NULL OR TaxType = @TaxType)
    ORDER BY TaxName;
END
GO

-- Create Tax Rate
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateTaxRate]
    @Id UNIQUEIDENTIFIER,
    @TaxName NVARCHAR(100),
    @TaxRate DECIMAL(5,2),
    @TaxType NVARCHAR(50),
    @Description NVARCHAR(500),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[TaxRates] (
        Id, TaxName, TaxRate, TaxType, IsActive, Description,
        TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @TaxName, @TaxRate, @TaxType, 1, @Description,
        @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- FISCAL YEARS PROCEDURES
-- =============================================

-- Get All Fiscal Years
CREATE OR ALTER PROCEDURE Finance.sp_GetAllFiscalYears
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fy.Id,
        fy.YearName,
        fy.StartDate,
        fy.EndDate,
        fy.IsActive,
        fy.CreatedAt,
        fy.UpdatedAt,
        fy.CreatedBy,
        fy.UpdatedBy
    FROM Finance.FiscalYears fy
    WHERE fy.TenantId = @TenantId
    AND (@IsActive IS NULL OR fy.IsActive = @IsActive)
    ORDER BY fy.StartDate DESC;
END
GO

-- Create Fiscal Year
CREATE OR ALTER PROCEDURE Finance.sp_CreateFiscalYear
    @YearName NVARCHAR(100),
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Finance.FiscalYears (
        Id, YearName, StartDate, EndDate, TenantId, CreatedBy
    )
    VALUES (
        @Id, @YearName, @StartDate, @EndDate, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- PERIODS PROCEDURES
-- =============================================

-- Get Fiscal Year Periods
CREATE OR ALTER PROCEDURE Finance.sp_GetFiscalYearPeriods
    @FiscalYearId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        p.FiscalYearId,
        p.PeriodName,
        p.StartDate,
        p.EndDate,
        p.IsClosed,
        p.CreatedAt,
        p.UpdatedAt,
        p.CreatedBy,
        p.UpdatedBy
    FROM Finance.Periods p
    INNER JOIN Finance.FiscalYears fy ON p.FiscalYearId = fy.Id
    WHERE fy.Id = @FiscalYearId 
    AND fy.TenantId = @TenantId
    ORDER BY p.StartDate;
END
GO

-- Create Period
CREATE OR ALTER PROCEDURE Finance.sp_CreatePeriod
    @FiscalYearId UNIQUEIDENTIFIER,
    @PeriodName NVARCHAR(100),
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Finance.Periods (
        Id, FiscalYearId, PeriodName, StartDate, EndDate, TenantId, CreatedBy
    )
    VALUES (
        @Id, @FiscalYearId, @PeriodName, @StartDate, @EndDate, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- PAYMENT TERMS PROCEDURES
-- =============================================

-- Get All Payment Terms
CREATE OR ALTER PROCEDURE Finance.sp_GetAllPaymentTerms
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pt.Id,
        pt.TermName,
        pt.Days,
        pt.Description,
        pt.IsActive,
        pt.CreatedAt,
        pt.UpdatedAt,
        pt.CreatedBy,
        pt.UpdatedBy
    FROM Finance.PaymentTerms pt
    WHERE pt.TenantId = @TenantId
    AND (@IsActive IS NULL OR pt.IsActive = @IsActive)
    ORDER BY pt.Days;
END
GO

-- Create Payment Term
CREATE OR ALTER PROCEDURE Finance.sp_CreatePaymentTerm
    @TermName NVARCHAR(100),
    @Days INT,
    @Description NVARCHAR(500) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Finance.PaymentTerms (
        Id, TermName, Days, Description, TenantId, CreatedBy
    )
    VALUES (
        @Id, @TermName, @Days, @Description, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- BUDGET HEADERS PROCEDURES
-- =============================================

-- Get Budget Headers
CREATE OR ALTER PROCEDURE Finance.sp_GetBudgetHeaders
    @TenantId UNIQUEIDENTIFIER,
    @FiscalYearId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredBudgets AS (
        SELECT 
            bh.Id,
            bh.BudgetName,
            bh.FiscalYearId,
            bh.BudgetType,
            bh.Status,
            bh.TotalBudget,
            bh.ApprovedBy,
            bh.ApprovedAt,
            bh.CreatedAt,
            bh.UpdatedAt,
            bh.CreatedBy,
            bh.UpdatedBy,
            fy.YearName as FiscalYearName,
            u.FirstName + ' ' + u.LastName as ApprovedByName,
            ROW_NUMBER() OVER (ORDER BY bh.CreatedAt DESC) AS RowNum
        FROM Finance.BudgetHeaders bh
        INNER JOIN Finance.FiscalYears fy ON bh.FiscalYearId = fy.Id
        LEFT JOIN Core.Users u ON bh.ApprovedBy = u.Id
        WHERE fy.TenantId = @TenantId
        AND (@FiscalYearId IS NULL OR bh.FiscalYearId = @FiscalYearId)
        AND (@Status IS NULL OR bh.Status = @Status)
    )
    SELECT 
        Id,
        BudgetName,
        FiscalYearId,
        BudgetType,
        Status,
        TotalBudget,
        ApprovedBy,
        ApprovedAt,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        FiscalYearName,
        ApprovedByName
    FROM FilteredBudgets
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM Finance.BudgetHeaders bh
    INNER JOIN Finance.FiscalYears fy ON bh.FiscalYearId = fy.Id
    WHERE fy.TenantId = @TenantId
    AND (@FiscalYearId IS NULL OR bh.FiscalYearId = @FiscalYearId)
    AND (@Status IS NULL OR bh.Status = @Status);
END
GO

-- Create Budget Header
CREATE OR ALTER PROCEDURE Finance.sp_CreateBudgetHeader
    @BudgetName NVARCHAR(200),
    @FiscalYearId UNIQUEIDENTIFIER,
    @BudgetType NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Finance.BudgetHeaders (
        Id, BudgetName, FiscalYearId, BudgetType, TenantId, CreatedBy
    )
    VALUES (
        @Id, @BudgetName, @FiscalYearId, @BudgetType, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- BUDGET LINES PROCEDURES
-- =============================================

-- Get Budget Lines
CREATE OR ALTER PROCEDURE Finance.sp_GetBudgetLines
    @BudgetHeaderId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        bl.Id,
        bl.BudgetHeaderId,
        bl.AccountId,
        bl.DepartmentId,
        bl.BudgetAmount,
        bl.ActualAmount,
        bl.Variance,
        bl.CreatedAt,
        bl.UpdatedAt,
        bl.CreatedBy,
        bl.UpdatedBy,
        coa.AccountName,
        coa.AccountCode
    FROM Finance.BudgetLines bl
    INNER JOIN Finance.ChartOfAccounts coa ON bl.AccountId = coa.Id
    INNER JOIN Finance.BudgetHeaders bh ON bl.BudgetHeaderId = bh.Id
    INNER JOIN Finance.FiscalYears fy ON bh.FiscalYearId = fy.Id
    WHERE fy.Id = @BudgetHeaderId 
    AND fy.TenantId = @TenantId
    ORDER BY coa.AccountCode;
END
GO

-- Add Budget Line
CREATE OR ALTER PROCEDURE Finance.sp_AddBudgetLine
    @BudgetHeaderId UNIQUEIDENTIFIER,
    @AccountId UNIQUEIDENTIFIER,
    @DepartmentId UNIQUEIDENTIFIER = NULL,
    @BudgetAmount DECIMAL(18,2),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Finance.BudgetLines (
        Id, BudgetHeaderId, AccountId, DepartmentId, BudgetAmount, TenantId, CreatedBy
    )
    VALUES (
        @Id, @BudgetHeaderId, @AccountId, @DepartmentId, @BudgetAmount, @TenantId, @CreatedBy
    );
    
    -- Update budget header total
    UPDATE Finance.BudgetHeaders
    SET TotalBudget = (
        SELECT SUM(BudgetAmount) 
        FROM Finance.BudgetLines 
        WHERE BudgetHeaderId = @BudgetHeaderId
    )
    WHERE Id = @BudgetHeaderId;
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- FIXED ASSETS PROCEDURES
-- =============================================

-- Get Fixed Assets
CREATE OR ALTER PROCEDURE Finance.sp_GetFixedAssets
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @AssetCategory NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredAssets AS (
        SELECT 
            fa.Id,
            fa.AssetCode,
            fa.AssetName,
            fa.AssetCategory,
            fa.PurchaseDate,
            fa.PurchaseCost,
            fa.SalvageValue,
            fa.UsefulLifeYears,
            fa.DepreciationMethod,
            fa.CurrentValue,
            fa.Location,
            fa.Status,
            fa.CreatedAt,
            fa.UpdatedAt,
            fa.CreatedBy,
            fa.UpdatedBy,
            ROW_NUMBER() OVER (ORDER BY fa.AssetCode) AS RowNum
        FROM Finance.FixedAssets fa
        WHERE fa.TenantId = @TenantId
        AND (@Status IS NULL OR fa.Status = @Status)
        AND (@AssetCategory IS NULL OR fa.AssetCategory = @AssetCategory)
    )
    SELECT 
        Id,
        AssetCode,
        AssetName,
        AssetCategory,
        PurchaseDate,
        PurchaseCost,
        SalvageValue,
        UsefulLifeYears,
        DepreciationMethod,
        CurrentValue,
        Location,
        Status,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy
    FROM FilteredAssets
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM Finance.FixedAssets fa
    WHERE fa.TenantId = @TenantId
    AND (@Status IS NULL OR fa.Status = @Status)
    AND (@AssetCategory IS NULL OR fa.AssetCategory = @AssetCategory);
END
GO

-- Create Fixed Asset
CREATE OR ALTER PROCEDURE Finance.sp_CreateFixedAsset
    @AssetCode NVARCHAR(50),
    @AssetName NVARCHAR(200),
    @AssetCategory NVARCHAR(100),
    @PurchaseDate DATETIME2,
    @PurchaseCost DECIMAL(18,2),
    @SalvageValue DECIMAL(18,2) = 0,
    @UsefulLifeYears INT,
    @DepreciationMethod NVARCHAR(50),
    @Location NVARCHAR(200) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @CurrentValue DECIMAL(18,2);
    
    -- Set current value to purchase cost initially
    SET @CurrentValue = @PurchaseCost;
    
    INSERT INTO Finance.FixedAssets (
        Id, AssetCode, AssetName, AssetCategory, PurchaseDate, PurchaseCost,
        SalvageValue, UsefulLifeYears, DepreciationMethod, CurrentValue,
        Location, TenantId, CreatedBy
    )
    VALUES (
        @Id, @AssetCode, @AssetName, @AssetCategory, @PurchaseDate, @PurchaseCost,
        @SalvageValue, @UsefulLifeYears, @DepreciationMethod, @CurrentValue,
        @Location, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- DEPRECIATION SCHEDULES PROCEDURES
-- =============================================

-- Get Asset Depreciation Schedule
CREATE OR ALTER PROCEDURE Finance.sp_GetAssetDepreciationSchedule
    @AssetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ds.Id,
        ds.AssetId,
        ds.FiscalYearId,
        ds.PeriodId,
        ds.DepreciationAmount,
        ds.AccumulatedDepreciation,
        ds.BookValue,
        ds.IsPosted,
        ds.PostedAt,
        ds.CreatedAt,
        ds.UpdatedAt,
        ds.CreatedBy,
        ds.UpdatedBy,
        fy.YearName as FiscalYearName,
        p.PeriodName
    FROM Finance.DepreciationSchedules ds
    INNER JOIN Finance.FixedAssets fa ON ds.AssetId = fa.Id
    INNER JOIN Finance.FiscalYears fy ON ds.FiscalYearId = fy.Id
    INNER JOIN Finance.Periods p ON ds.PeriodId = p.Id
    WHERE fa.Id = @AssetId 
    AND fa.TenantId = @TenantId
    ORDER BY fy.StartDate, p.StartDate;
END
GO

-- Calculate Depreciation
CREATE OR ALTER PROCEDURE Finance.sp_CalculateDepreciation
    @AssetId UNIQUEIDENTIFIER,
    @FiscalYearId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CalculatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @PurchaseCost DECIMAL(18,2);
        DECLARE @SalvageValue DECIMAL(18,2);
        DECLARE @UsefulLifeYears INT;
        DECLARE @DepreciationMethod NVARCHAR(50);
        DECLARE @CurrentValue DECIMAL(18,2);
        DECLARE @AnnualDepreciation DECIMAL(18,2);
        DECLARE @MonthlyDepreciation DECIMAL(18,2);
        
        -- Get asset details
        SELECT 
            @PurchaseCost = PurchaseCost,
            @SalvageValue = SalvageValue,
            @UsefulLifeYears = UsefulLifeYears,
            @DepreciationMethod = DepreciationMethod,
            @CurrentValue = CurrentValue
        FROM Finance.FixedAssets
        WHERE Id = @AssetId AND TenantId = @TenantId;
        
        -- Calculate annual depreciation based on method
        IF @DepreciationMethod = 'Straight Line'
        BEGIN
            SET @AnnualDepreciation = (@PurchaseCost - @SalvageValue) / @UsefulLifeYears;
        END
        ELSE IF @DepreciationMethod = 'Double Declining Balance'
        BEGIN
            SET @AnnualDepreciation = (@CurrentValue * 2) / @UsefulLifeYears;
        END
        
        SET @MonthlyDepreciation = @AnnualDepreciation / 12;
        
        -- Get periods for the fiscal year
        DECLARE period_cursor CURSOR FOR
        SELECT Id FROM Finance.Periods 
        WHERE FiscalYearId = @FiscalYearId 
        ORDER BY StartDate;
        
        DECLARE @PeriodId UNIQUEIDENTIFIER;
        DECLARE @AccumulatedDepreciation DECIMAL(18,2) = 0;
        DECLARE @BookValue DECIMAL(18,2) = @CurrentValue;
        
        OPEN period_cursor;
        FETCH NEXT FROM period_cursor INTO @PeriodId;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Check if depreciation already calculated for this period
            IF NOT EXISTS (SELECT 1 FROM Finance.DepreciationSchedules 
                          WHERE AssetId = @AssetId AND PeriodId = @PeriodId)
            BEGIN
                SET @AccumulatedDepreciation = @AccumulatedDepreciation + @MonthlyDepreciation;
                SET @BookValue = @PurchaseCost - @AccumulatedDepreciation;
                
                -- Ensure book value doesn't go below salvage value
                IF @BookValue < @SalvageValue
                BEGIN
                    SET @BookValue = @SalvageValue;
                    SET @MonthlyDepreciation = @BookValue - (@PurchaseCost - (@AccumulatedDepreciation - @MonthlyDepreciation));
                    SET @AccumulatedDepreciation = @PurchaseCost - @SalvageValue;
                END
                
                INSERT INTO Finance.DepreciationSchedules (
                    Id, AssetId, FiscalYearId, PeriodId, DepreciationAmount,
                    AccumulatedDepreciation, BookValue, TenantId, CreatedBy
                )
                VALUES (
                    NEWID(), @AssetId, @FiscalYearId, @PeriodId, @MonthlyDepreciation,
                    @AccumulatedDepreciation, @BookValue, @TenantId, @CalculatedBy
                );
            END
            
            FETCH NEXT FROM period_cursor INTO @PeriodId;
        END
        
        CLOSE period_cursor;
        DEALLOCATE period_cursor;
        
        -- Update asset current value
        UPDATE Finance.FixedAssets
        SET CurrentValue = @BookValue
        WHERE Id = @AssetId;
        
        COMMIT TRANSACTION;
        SELECT 1 as Success;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 as Success, ERROR_MESSAGE() as ErrorMessage;
    END CATCH
END
GO

PRINT 'Finance Module stored procedures created successfully!'