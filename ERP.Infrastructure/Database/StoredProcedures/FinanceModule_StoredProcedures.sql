-- =============================================
-- FINANCE MODULE STORED PROCEDURES
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Finance Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Finance')
BEGIN
    EXEC('CREATE SCHEMA [Finance]')
END
GO

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

-- Update Account
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateAccount]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @AccountCode NVARCHAR(50) = NULL,
    @AccountName NVARCHAR(200) = NULL,
    @AccountType NVARCHAR(50) = NULL,
    @ParentAccountId UNIQUEIDENTIFIER = NULL,
    @Level INT = NULL,
    @Description NVARCHAR(500) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate account exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[ChartOfAccounts] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Account not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[ChartOfAccounts]
    SET 
        AccountCode = ISNULL(@AccountCode, AccountCode),
        AccountName = ISNULL(@AccountName, AccountName),
        AccountType = ISNULL(@AccountType, AccountType),
        ParentAccountId = ISNULL(@ParentAccountId, ParentAccountId),
        Level = ISNULL(@Level, Level),
        Description = ISNULL(@Description, Description),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Account (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteAccount]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate account exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[ChartOfAccounts] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Account not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if account has child accounts
    IF EXISTS (SELECT 1 FROM [Finance].[ChartOfAccounts] WHERE ParentAccountId = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Cannot delete account with child accounts. Please delete or reassign child accounts first.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Finance].[ChartOfAccounts]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
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

-- Get All Banks
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllBanks]
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, BankName, BankCode, SwiftCode, Address, Phone, Email, Website,
        IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[Banks]
    WHERE TenantId = @TenantId
    AND (@IsActive IS NULL OR IsActive = @IsActive)
    ORDER BY BankName;
END
GO

-- Get Bank Accounts by Bank ID
CREATE OR ALTER PROCEDURE [Finance].[sp_GetBankAccountsByBankId]
    @BankId UNIQUEIDENTIFIER,
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
    WHERE ba.BankId = @BankId
    AND ba.TenantId = @TenantId
    AND ba.IsActive = 1
    ORDER BY ba.AccountName;
END
GO

-- Create Bank
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateBank]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @BankName NVARCHAR(200),
    @BankCode NVARCHAR(50),
    @SwiftCode NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Website NVARCHAR(200) = NULL,
    @IsActive BIT = 1,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Finance].[Banks] (
        Id, TenantId, BankName, BankCode, SwiftCode, Address,
        Phone, Email, Website, IsActive, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @TenantId, @BankName, @BankCode, @SwiftCode, @Address,
        @Phone, @Email, @Website, @IsActive, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Bank
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateBank]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @BankName NVARCHAR(200) = NULL,
    @BankCode NVARCHAR(50) = NULL,
    @SwiftCode NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Website NVARCHAR(200) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bank exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Banks] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Bank not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[Banks]
    SET 
        BankName = ISNULL(@BankName, BankName),
        BankCode = ISNULL(@BankCode, BankCode),
        SwiftCode = ISNULL(@SwiftCode, SwiftCode),
        Address = ISNULL(@Address, Address),
        Phone = ISNULL(@Phone, Phone),
        Email = ISNULL(@Email, Email),
        Website = ISNULL(@Website, Website),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Bank (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteBank]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bank exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Banks] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Bank not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if bank has active accounts
    IF EXISTS (SELECT 1 FROM [Finance].[BankAccounts] WHERE BankId = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Cannot delete bank with active accounts. Please deactivate or delete bank accounts first.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Finance].[Banks]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Create Bank Account
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateBankAccount]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @BankId UNIQUEIDENTIFIER,
    @AccountName NVARCHAR(200),
    @AccountNumber NVARCHAR(50),
    @AccountType NVARCHAR(50),
    @Currency NVARCHAR(10),
    @OpeningBalance DECIMAL(18,2) = 0,
    @IsActive BIT = 1,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bank exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Banks] WHERE Id = @BankId AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Bank not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    INSERT INTO [Finance].[BankAccounts] (
        Id, TenantId, BankId, AccountName, AccountNumber, AccountType,
        Currency, OpeningBalance, CurrentBalance, IsActive, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @TenantId, @BankId, @AccountName, @AccountNumber, @AccountType,
        @Currency, @OpeningBalance, @OpeningBalance, @IsActive, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Bank Account
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateBankAccount]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @AccountName NVARCHAR(200) = NULL,
    @AccountNumber NVARCHAR(50) = NULL,
    @AccountType NVARCHAR(50) = NULL,
    @Currency NVARCHAR(10) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bank account exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[BankAccounts] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Bank account not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[BankAccounts]
    SET 
        AccountName = ISNULL(@AccountName, AccountName),
        AccountNumber = ISNULL(@AccountNumber, AccountNumber),
        AccountType = ISNULL(@AccountType, AccountType),
        Currency = ISNULL(@Currency, Currency),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Bank Account (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteBankAccount]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bank account exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[BankAccounts] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Bank account not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Finance].[BankAccounts]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
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

-- Get All Invoices
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllInvoices]
    @TenantId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, InvoiceNumber, InvoiceDate, DueDate, CustomerId, SubTotal,
        TaxAmount, DiscountAmount, TotalAmount, PaidAmount, BalanceAmount,
        Status, PaymentTerms, Notes, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[Invoices]
    WHERE TenantId = @TenantId
        AND (@CustomerId IS NULL OR CustomerId = @CustomerId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR InvoiceDate >= @StartDate)
        AND (@EndDate IS NULL OR InvoiceDate <= @EndDate)
    ORDER BY InvoiceDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Finance].[Invoices]
    WHERE TenantId = @TenantId
        AND (@CustomerId IS NULL OR CustomerId = @CustomerId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR InvoiceDate >= @StartDate)
        AND (@EndDate IS NULL OR InvoiceDate <= @EndDate);
END
GO

-- Update Invoice
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @InvoiceNumber NVARCHAR(50) = NULL,
    @InvoiceDate DATETIME2 = NULL,
    @DueDate DATETIME2 = NULL,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @SubTotal DECIMAL(18,2) = NULL,
    @TaxAmount DECIMAL(18,2) = NULL,
    @DiscountAmount DECIMAL(18,2) = NULL,
    @TotalAmount DECIMAL(18,2) = NULL,
    @PaymentTerms NVARCHAR(200) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate invoice exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Invoices] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Invoice not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Recalculate balance if amounts are updated
    DECLARE @NewPaidAmount DECIMAL(18,2);
    DECLARE @NewBalanceAmount DECIMAL(18,2);
    
    IF @TotalAmount IS NOT NULL
    BEGIN
        SELECT @NewPaidAmount = PaidAmount FROM [Finance].[Invoices] WHERE Id = @Id;
        SET @NewBalanceAmount = @TotalAmount - @NewPaidAmount;
    END
    
    UPDATE [Finance].[Invoices]
    SET 
        InvoiceNumber = ISNULL(@InvoiceNumber, InvoiceNumber),
        InvoiceDate = ISNULL(@InvoiceDate, InvoiceDate),
        DueDate = ISNULL(@DueDate, DueDate),
        CustomerId = ISNULL(@CustomerId, CustomerId),
        SubTotal = ISNULL(@SubTotal, SubTotal),
        TaxAmount = ISNULL(@TaxAmount, TaxAmount),
        DiscountAmount = ISNULL(@DiscountAmount, DiscountAmount),
        TotalAmount = ISNULL(@TotalAmount, TotalAmount),
        BalanceAmount = ISNULL(@NewBalanceAmount, BalanceAmount),
        PaymentTerms = ISNULL(@PaymentTerms, PaymentTerms),
        Notes = ISNULL(@Notes, Notes),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Invoice (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate invoice exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Invoices] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Invoice not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if invoice is posted or paid
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Finance].[Invoices] WHERE Id = @Id;
    
    IF @Status IN ('Posted', 'Paid', 'PartiallyPaid')
    BEGIN
        RAISERROR('Cannot delete invoice that is posted or paid. Please void the invoice instead.', 16, 1);
        RETURN;
    END
    
    -- Delete invoice items first
    DELETE FROM [Finance].[InvoiceItems]
    WHERE InvoiceId = @Id AND TenantId = @TenantId;
    
    -- Delete invoice
    DELETE FROM [Finance].[Invoices]
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
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

-- Get All Bills
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllBills]
    @TenantId UNIQUEIDENTIFIER,
    @VendorId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, BillNumber, BillDate, DueDate, VendorId, SubTotal,
        TaxAmount, DiscountAmount, TotalAmount, PaidAmount, BalanceAmount,
        Status, PaymentTerms, Notes, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[Bills]
    WHERE TenantId = @TenantId
        AND (@VendorId IS NULL OR VendorId = @VendorId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR BillDate >= @StartDate)
        AND (@EndDate IS NULL OR BillDate <= @EndDate)
    ORDER BY BillDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Finance].[Bills]
    WHERE TenantId = @TenantId
        AND (@VendorId IS NULL OR VendorId = @VendorId)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR BillDate >= @StartDate)
        AND (@EndDate IS NULL OR BillDate <= @EndDate);
END
GO

-- Update Bill
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateBill]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @BillNumber NVARCHAR(50) = NULL,
    @BillDate DATETIME2 = NULL,
    @DueDate DATETIME2 = NULL,
    @VendorId UNIQUEIDENTIFIER = NULL,
    @SubTotal DECIMAL(18,2) = NULL,
    @TaxAmount DECIMAL(18,2) = NULL,
    @DiscountAmount DECIMAL(18,2) = NULL,
    @TotalAmount DECIMAL(18,2) = NULL,
    @PaymentTerms NVARCHAR(200) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bill exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Bills] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Bill not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Recalculate balance if amounts are updated
    DECLARE @NewPaidAmount DECIMAL(18,2);
    DECLARE @NewBalanceAmount DECIMAL(18,2);
    
    IF @TotalAmount IS NOT NULL
    BEGIN
        SELECT @NewPaidAmount = PaidAmount FROM [Finance].[Bills] WHERE Id = @Id;
        SET @NewBalanceAmount = @TotalAmount - @NewPaidAmount;
    END
    
    UPDATE [Finance].[Bills]
    SET 
        BillNumber = ISNULL(@BillNumber, BillNumber),
        BillDate = ISNULL(@BillDate, BillDate),
        DueDate = ISNULL(@DueDate, DueDate),
        VendorId = ISNULL(@VendorId, VendorId),
        SubTotal = ISNULL(@SubTotal, SubTotal),
        TaxAmount = ISNULL(@TaxAmount, TaxAmount),
        DiscountAmount = ISNULL(@DiscountAmount, DiscountAmount),
        TotalAmount = ISNULL(@TotalAmount, TotalAmount),
        BalanceAmount = ISNULL(@NewBalanceAmount, BalanceAmount),
        PaymentTerms = ISNULL(@PaymentTerms, PaymentTerms),
        Notes = ISNULL(@Notes, Notes),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Bill (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteBill]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate bill exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Bills] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Bill not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if bill is posted or paid
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Finance].[Bills] WHERE Id = @Id;
    
    IF @Status IN ('Posted', 'Paid', 'PartiallyPaid')
    BEGIN
        RAISERROR('Cannot delete bill that is posted or paid. Please void the bill instead.', 16, 1);
        RETURN;
    END
    
    -- Delete bill items first
    DELETE FROM [Finance].[BillItems]
    WHERE BillId = @Id AND TenantId = @TenantId;
    
    -- Delete bill
    DELETE FROM [Finance].[Bills]
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
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

-- Get All Payments
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllPayments]
    @TenantId UNIQUEIDENTIFIER,
    @PaymentType NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, PaymentNumber, PaymentDate, PaymentType, Amount, BankAccountId,
        Reference, Description, Status, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[Payments]
    WHERE TenantId = @TenantId
        AND (@PaymentType IS NULL OR PaymentType = @PaymentType)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR PaymentDate >= @StartDate)
        AND (@EndDate IS NULL OR PaymentDate <= @EndDate)
    ORDER BY PaymentDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Finance].[Payments]
    WHERE TenantId = @TenantId
        AND (@PaymentType IS NULL OR PaymentType = @PaymentType)
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR PaymentDate >= @StartDate)
        AND (@EndDate IS NULL OR PaymentDate <= @EndDate);
END
GO

-- Update Payment
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdatePayment]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PaymentNumber NVARCHAR(50) = NULL,
    @PaymentDate DATETIME2 = NULL,
    @PaymentType NVARCHAR(50) = NULL,
    @Amount DECIMAL(18,2) = NULL,
    @BankAccountId UNIQUEIDENTIFIER = NULL,
    @Reference NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @Status NVARCHAR(50) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate payment exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Payments] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Payment not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[Payments]
    SET 
        PaymentNumber = ISNULL(@PaymentNumber, PaymentNumber),
        PaymentDate = ISNULL(@PaymentDate, PaymentDate),
        PaymentType = ISNULL(@PaymentType, PaymentType),
        Amount = ISNULL(@Amount, Amount),
        BankAccountId = ISNULL(@BankAccountId, BankAccountId),
        Reference = ISNULL(@Reference, Reference),
        Description = ISNULL(@Description, Description),
        Status = ISNULL(@Status, Status),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Payment
CREATE OR ALTER PROCEDURE [Finance].[sp_DeletePayment]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate payment exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Payments] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Payment not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if payment is posted
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Finance].[Payments] WHERE Id = @Id;
    
    IF @Status = 'Posted'
    BEGIN
        RAISERROR('Cannot delete payment that is posted.', 16, 1);
        RETURN;
    END
    
    -- Delete payment
    DELETE FROM [Finance].[Payments]
    WHERE Id = @Id AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
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

-- Get All Journal Entries
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllJournalEntries]
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, EntryNumber, EntryDate, Reference, Description, TotalDebit,
        TotalCredit, Status, PostedBy, PostedAt, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy
    FROM [Finance].[JournalEntries]
    WHERE TenantId = @TenantId
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR EntryDate >= @StartDate)
        AND (@EndDate IS NULL OR EntryDate <= @EndDate)
    ORDER BY EntryDate DESC, EntryNumber DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [Finance].[JournalEntries]
    WHERE TenantId = @TenantId
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR EntryDate >= @StartDate)
        AND (@EndDate IS NULL OR EntryDate <= @EndDate);
END
GO

-- Update Journal Entry
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateJournalEntry]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @EntryNumber NVARCHAR(50) = NULL,
    @EntryDate DATETIME2 = NULL,
    @Reference NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @TotalDebit DECIMAL(18,2) = NULL,
    @TotalCredit DECIMAL(18,2) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate journal entry exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[JournalEntries] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Journal entry not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if entry is posted
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Finance].[JournalEntries] WHERE Id = @Id;
    
    IF @Status = 'Posted'
    BEGIN
        RAISERROR('Cannot update journal entry that is posted.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[JournalEntries]
    SET 
        EntryNumber = ISNULL(@EntryNumber, EntryNumber),
        EntryDate = ISNULL(@EntryDate, EntryDate),
        Reference = ISNULL(@Reference, Reference),
        Description = ISNULL(@Description, Description),
        TotalDebit = ISNULL(@TotalDebit, TotalDebit),
        TotalCredit = ISNULL(@TotalCredit, TotalCredit),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Journal Entry
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteJournalEntry]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate journal entry exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[JournalEntries] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Journal entry not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if entry is posted
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [Finance].[JournalEntries] WHERE Id = @Id;
    
    IF @Status = 'Posted'
    BEGIN
        RAISERROR('Cannot delete journal entry that is posted.', 16, 1);
        RETURN;
    END
    
    -- Delete journal entry lines first
    DELETE FROM [Finance].[JournalEntryLines]
    WHERE JournalEntryId = @Id AND TenantId = @TenantId;
    
    -- Delete journal entry
    DELETE FROM [Finance].[JournalEntries]
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

-- Update Tax Rate
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateTaxRate]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @TaxName NVARCHAR(100) = NULL,
    @TaxRate DECIMAL(5,2) = NULL,
    @TaxType NVARCHAR(50) = NULL,
    @Description NVARCHAR(500) = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate tax rate exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[TaxRates] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Tax rate not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[TaxRates]
    SET 
        TaxName = ISNULL(@TaxName, TaxName),
        TaxRate = ISNULL(@TaxRate, TaxRate),
        TaxType = ISNULL(@TaxType, TaxType),
        Description = ISNULL(@Description, Description),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Tax Rate (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteTaxRate]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate tax rate exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Finance].[TaxRates] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Tax rate not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Finance].[TaxRates]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
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
        fy.FiscalYear,
        fy.StartDate,
        fy.EndDate,
        fy.IsActive,
        fy.IsClosed,
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
    @FiscalYear NVARCHAR(10),
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Finance.FiscalYears (
        Id, FiscalYear, StartDate, EndDate, TenantId, CreatedBy
    )
    VALUES (
        @Id, @FiscalYear, @StartDate, @EndDate, @TenantId, @CreatedBy
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
            fy.FiscalYear as FiscalYearName,
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
        fy.FiscalYear as FiscalYearName,
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

-- =============================================
-- CURRENCY PROCEDURES
-- =============================================

-- Get Currency by Code
CREATE OR ALTER PROCEDURE [Finance].[sp_GetCurrencyByCode]
    @CurrencyCode NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CurrencyCode, CurrencyName, Symbol, ExchangeRate,
        IsBaseCurrency, IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[Currencies]
    WHERE CurrencyCode = @CurrencyCode;
END
GO

-- Get All Currencies
CREATE OR ALTER PROCEDURE [Finance].[sp_GetAllCurrencies]
    @TenantId UNIQUEIDENTIFIER = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CurrencyCode, CurrencyName, Symbol, ExchangeRate,
        IsBaseCurrency, IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[Currencies]
    WHERE (@TenantId IS NULL OR TenantId = @TenantId)
        AND (@IsActive IS NULL OR IsActive = @IsActive)
    ORDER BY CurrencyCode;
END
GO

-- Get Active Currencies
CREATE OR ALTER PROCEDURE [Finance].[sp_GetActiveCurrencies]
    @TenantId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CurrencyCode, CurrencyName, Symbol, ExchangeRate,
        IsBaseCurrency, IsActive, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    FROM [Finance].[Currencies]
    WHERE IsActive = 1
        AND (@TenantId IS NULL OR TenantId = @TenantId)
    ORDER BY CurrencyCode;
END
GO

-- Create Currency
CREATE OR ALTER PROCEDURE [Finance].[sp_CreateCurrency]
    @Id UNIQUEIDENTIFIER,
    @CurrencyCode NVARCHAR(10),
    @CurrencyName NVARCHAR(100),
    @Symbol NVARCHAR(10) = NULL,
    @ExchangeRate DECIMAL(18,6) = 1,
    @IsBaseCurrency BIT = 0,
    @IsActive BIT = 1,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if currency code already exists for this tenant
    IF EXISTS (SELECT 1 FROM [Finance].[Currencies] WHERE CurrencyCode = @CurrencyCode AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Currency code already exists for this tenant.', 16, 1);
        RETURN;
    END
    
    INSERT INTO [Finance].[Currencies] (
        Id, CurrencyCode, CurrencyName, Symbol, ExchangeRate,
        IsBaseCurrency, IsActive, TenantId, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @CurrencyCode, @CurrencyName, @Symbol, @ExchangeRate,
        @IsBaseCurrency, @IsActive, @TenantId, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id, @CurrencyCode as CurrencyCode;
END
GO

-- Update Currency
CREATE OR ALTER PROCEDURE [Finance].[sp_UpdateCurrency]
    @CurrencyCode NVARCHAR(10),
    @TenantId UNIQUEIDENTIFIER,
    @CurrencyName NVARCHAR(100) = NULL,
    @Symbol NVARCHAR(10) = NULL,
    @ExchangeRate DECIMAL(18,6) = NULL,
    @IsBaseCurrency BIT = NULL,
    @IsActive BIT = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate currency exists
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Currencies] WHERE CurrencyCode = @CurrencyCode AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Currency not found.', 16, 1);
        RETURN;
    END
    
    UPDATE [Finance].[Currencies]
    SET 
        CurrencyName = ISNULL(@CurrencyName, CurrencyName),
        Symbol = ISNULL(@Symbol, Symbol),
        ExchangeRate = ISNULL(@ExchangeRate, ExchangeRate),
        IsBaseCurrency = ISNULL(@IsBaseCurrency, IsBaseCurrency),
        IsActive = ISNULL(@IsActive, IsActive),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE CurrencyCode = @CurrencyCode AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Currency (Soft Delete)
CREATE OR ALTER PROCEDURE [Finance].[sp_DeleteCurrency]
    @CurrencyCode NVARCHAR(10),
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate currency exists
    IF NOT EXISTS (SELECT 1 FROM [Finance].[Currencies] WHERE CurrencyCode = @CurrencyCode AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Currency not found.', 16, 1);
        RETURN;
    END
    
    -- Check if currency is base currency
    IF EXISTS (SELECT 1 FROM [Finance].[Currencies] WHERE CurrencyCode = @CurrencyCode AND TenantId = @TenantId AND IsBaseCurrency = 1)
    BEGIN
        RAISERROR('Cannot delete base currency.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Finance].[Currencies]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE CurrencyCode = @CurrencyCode AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Check if Currency Exists
CREATE OR ALTER PROCEDURE [Finance].[sp_CurrencyExists]
    @CurrencyCode NVARCHAR(10),
    @TenantId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE WHEN EXISTS (SELECT 1 FROM [Finance].[Currencies] 
                             WHERE CurrencyCode = @CurrencyCode 
                             AND (@TenantId IS NULL OR TenantId = @TenantId)) 
                THEN 1 ELSE 0 END as CurrencyExists;
END
GO

-- Note: Exchange Rates are managed in Core schema, not Finance schema
-- Exchange Rate stored procedures should be in CoreModule_StoredProcedures.sql

PRINT 'Finance Module stored procedures created successfully!'