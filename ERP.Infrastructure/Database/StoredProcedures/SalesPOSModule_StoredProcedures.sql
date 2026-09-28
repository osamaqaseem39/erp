-- =============================================
-- Sales/POS Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Sales Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Sales')
BEGIN
    EXEC('CREATE SCHEMA [Sales]')
END
GO

-- =============================================
-- POS SESSION MANAGEMENT PROCEDURES
-- =============================================

-- Get POS Session by ID
CREATE OR ALTER PROCEDURE [Sales].[sp_GetPOSSessionById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ps.Id,
        ps.TenantId,
        ps.SessionNumber,
        ps.CashierId,
        ps.TerminalId,
        ps.StartTime,
        ps.EndTime,
        ps.OpeningBalance,
        ps.ClosingBalance,
        ps.ExpectedBalance,
        ps.Difference,
        ps.Status,
        ps.Notes,
        ps.CreatedAt,
        ps.UpdatedAt,
        ps.CreatedBy,
        ps.UpdatedBy,
        u.FirstName + ' ' + u.LastName as CashierName
    FROM POSSessions ps
    LEFT JOIN [Core].[Users] u ON ps.CashierId = u.Id
    WHERE ps.Id = @Id
    AND ps.TenantId = @TenantID;
END
GO

-- Get Active POS Session
CREATE OR ALTER PROCEDURE [Sales].[sp_GetActivePOSSession]
    @TenantID UNIQUEIDENTIFIER,
    @CashierId UNIQUEIDENTIFIER = NULL,
    @TerminalId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ps.Id,
        ps.SessionNumber,
        ps.CashierId,
        ps.TerminalId,
        ps.StartTime,
        ps.OpeningBalance,
        ps.ExpectedBalance,
        ps.Status,
        ps.Notes,
        u.FirstName + ' ' + u.LastName as CashierName
    FROM POSSessions ps
    LEFT JOIN [Core].[Users] u ON ps.CashierId = u.Id
    WHERE ps.TenantId = @TenantID
    AND ps.Status = 'Open'
    AND (@CashierId IS NULL OR ps.CashierId = @CashierId)
    AND (@TerminalId IS NULL OR ps.TerminalId = @TerminalId)
    ORDER BY ps.StartTime DESC;
END
GO

-- Create POS Session
CREATE OR ALTER PROCEDURE [Sales].[sp_CreatePOSSession]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @SessionNumber NVARCHAR(50),
    @CashierId UNIQUEIDENTIFIER,
    @TerminalId UNIQUEIDENTIFIER,
    @OpeningBalance DECIMAL(18,2),
    @Status NVARCHAR(50) = 'Open',
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO POSSessions (
        Id, TenantId, SessionNumber, CashierId, TerminalId,
        StartTime, OpeningBalance, ClosingBalance, ExpectedBalance,
        Difference, Status, Notes, CreatedBy
    )
    VALUES (
        @Id, @TenantID, @SessionNumber, @CashierId, @TerminalId,
        GETUTCDATE(), @OpeningBalance, 0, @OpeningBalance,
        0, @Status, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Close POS Session
CREATE OR ALTER PROCEDURE [Sales].[sp_ClosePOSSession]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ClosingBalance DECIMAL(18,2),
    @ExpectedBalance DECIMAL(18,2),
    @Difference DECIMAL(18,2),
    @Status NVARCHAR(50) = 'Closed',
    @Notes NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE POSSessions
    SET 
        EndTime = GETUTCDATE(),
        ClosingBalance = @ClosingBalance,
        ExpectedBalance = @ExpectedBalance,
        Difference = @Difference,
        Status = @Status,
        Notes = @Notes,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID
    AND Status = 'Open';
END
GO

-- =============================================
-- POS TRANSACTION MANAGEMENT PROCEDURES
-- =============================================

-- Get POS Transaction by ID
CREATE OR ALTER PROCEDURE [Sales].[sp_GetPOSTransactionById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pt.Id,
        pt.TenantId,
        pt.TransactionNumber,
        pt.POSSessionId,
        pt.SessionId,
        pt.CustomerId,
        pt.TransactionDate,
        pt.PaymentMethod,
        pt.SubTotal,
        pt.TaxAmount,
        pt.DiscountAmount,
        pt.TotalAmount,
        pt.AmountPaid,
        pt.ChangeAmount,
        pt.Status,
        pt.Notes,
        pt.CreatedAt,
        pt.UpdatedAt,
        pt.CreatedBy,
        pt.UpdatedBy,
        ps.SessionNumber,
        c.CustomerName
    FROM POSTransactions pt
    LEFT JOIN POSSessions ps ON pt.POSSessionId = ps.Id
    LEFT JOIN [CRM].Customers c ON pt.CustomerId = c.Id
    WHERE pt.Id = @Id
    AND pt.TenantId = @TenantID;
END
GO

-- Get POS Transactions by Session ID
CREATE OR ALTER PROCEDURE [Sales].[sp_GetPOSTransactionsBySessionId]
    @POSSessionId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        pt.Id,
        pt.TransactionNumber,
        pt.CustomerId,
        pt.TransactionDate,
        pt.PaymentMethod,
        pt.SubTotal,
        pt.TaxAmount,
        pt.DiscountAmount,
        pt.TotalAmount,
        pt.AmountPaid,
        pt.ChangeAmount,
        pt.Status,
        pt.CreatedAt,
        c.CustomerName
    FROM POSTransactions pt
    LEFT JOIN [CRM].Customers c ON pt.CustomerId = c.Id
    WHERE pt.POSSessionId = @POSSessionId
    AND pt.TenantId = @TenantID
    ORDER BY pt.TransactionDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM POSTransactions pt
    WHERE pt.POSSessionId = @POSSessionId
    AND pt.TenantId = @TenantID;
END
GO

-- Create POS Transaction
CREATE OR ALTER PROCEDURE [Sales].[sp_CreatePOSTransaction]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @TransactionNumber NVARCHAR(50),
    @POSSessionId UNIQUEIDENTIFIER,
    @SessionId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER,
    @TransactionDate DATETIME2,
    @PaymentMethod NVARCHAR(50),
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @AmountPaid DECIMAL(18,2),
    @ChangeAmount DECIMAL(18,2),
    @Status NVARCHAR(50) = 'Completed',
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO POSTransactions (
        Id, TenantId, TransactionNumber, POSSessionId, SessionId, CustomerId,
        TransactionDate, PaymentMethod, SubTotal, TaxAmount, DiscountAmount,
        TotalAmount, AmountPaid, ChangeAmount, Status, Notes, CreatedBy
    )
    VALUES (
        @Id, @TenantID, @TransactionNumber, @POSSessionId, @SessionId, @CustomerId,
        @TransactionDate, @PaymentMethod, @SubTotal, @TaxAmount, @DiscountAmount,
        @TotalAmount, @AmountPaid, @ChangeAmount, @Status, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Get POS Transaction Lines by Transaction ID
CREATE OR ALTER PROCEDURE [Sales].[sp_GetPOSTransactionLinesByTransactionId]
    @POSTransactionId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ptl.Id,
        ptl.POSTransactionId,
        ptl.ItemId,
        ptl.Description,
        ptl.Quantity,
        ptl.UnitPrice,
        ptl.DiscountRate,
        ptl.DiscountAmount,
        ptl.TaxRate,
        ptl.TaxAmount,
        ptl.LineTotal,
        ptl.LineNumber,
        ptl.CreatedAt,
        i.ItemName,
        i.ItemCode
    FROM POSTransactionLines ptl
    INNER JOIN POSTransactions pt ON ptl.POSTransactionId = pt.Id
    LEFT JOIN Items i ON ptl.ItemId = i.Id
    WHERE ptl.POSTransactionId = @POSTransactionId
    AND pt.TenantId = @TenantID
    ORDER BY ptl.LineNumber;
END
GO

-- Process POS Transaction
CREATE OR ALTER PROCEDURE [Sales].[sp_ProcessPOSTransaction]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @TransactionNumber NVARCHAR(50),
    @POSSessionId UNIQUEIDENTIFIER,
    @SessionId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER,
    @PaymentMethod NVARCHAR(50),
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @AmountPaid DECIMAL(18,2),
    @ChangeAmount DECIMAL(18,2),
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Create transaction
        INSERT INTO POSTransactions (
            Id, TenantId, TransactionNumber, POSSessionId, SessionId, CustomerId,
            TransactionDate, PaymentMethod, SubTotal, TaxAmount, DiscountAmount,
            TotalAmount, AmountPaid, ChangeAmount, Status, Notes, CreatedBy
        )
        VALUES (
            @Id, @TenantID, @TransactionNumber, @POSSessionId, @SessionId, @CustomerId,
            GETUTCDATE(), @PaymentMethod, @SubTotal, @TaxAmount, @DiscountAmount,
            @TotalAmount, @AmountPaid, @ChangeAmount, 'Completed', @Notes, @CreatedBy
        );
        
        -- Update POS Session expected balance
        UPDATE POSSessions
        SET 
            ExpectedBalance = ExpectedBalance + @TotalAmount,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @POSSessionId
        AND TenantId = @TenantID;
        
        COMMIT TRANSACTION;
        SELECT @Id as Id, 1 as Success;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- SALES INVOICE MANAGEMENT PROCEDURES
-- =============================================

-- Get Sales Invoice by ID
CREATE OR ALTER PROCEDURE [Sales].[sp_GetSalesInvoiceById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        si.Id,
        si.TenantId,
        si.InvoiceNumber,
        si.CustomerId,
        si.InvoiceDate,
        si.DueDate,
        si.Status,
        si.SubTotal,
        si.TaxAmount,
        si.TotalAmount,
        si.PaidAmount,
        si.BalanceAmount,
        si.Notes,
        si.Terms,
        si.SalesOrderId,
        si.PostedAt,
        si.PostedBy,
        si.CreatedAt,
        si.UpdatedAt,
        si.CreatedBy,
        si.UpdatedBy,
        si.IsActive,
        c.CustomerName
    FROM SalesInvoices si
    LEFT JOIN [CRM].Customers c ON si.CustomerId = c.Id
    WHERE si.Id = @Id
    AND si.TenantId = @TenantId
    AND si.IsActive = 1;
END
GO

-- Get All Sales Invoices
CREATE OR ALTER PROCEDURE [Sales].[sp_GetAllSalesInvoices]
    @TenantId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        si.Id,
        si.InvoiceNumber,
        si.CustomerId,
        si.InvoiceDate,
        si.DueDate,
        si.Status,
        si.SubTotal,
        si.TaxAmount,
        si.TotalAmount,
        si.PaidAmount,
        si.BalanceAmount,
        si.Notes,
        si.Terms,
        si.SalesOrderId,
        si.PostedAt,
        si.PostedBy,
        si.CreatedAt,
        si.UpdatedAt,
        c.CustomerName
    FROM SalesInvoices si
    LEFT JOIN [CRM].Customers c ON si.CustomerId = c.Id
    WHERE si.TenantId = @TenantId
    AND si.IsActive = 1
    AND (@CustomerId IS NULL OR si.CustomerId = @CustomerId)
    AND (@Status IS NULL OR si.Status = @Status)
    ORDER BY si.InvoiceDate DESC, si.InvoiceNumber DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM SalesInvoices si
    WHERE si.TenantId = @TenantId
    AND si.IsActive = 1
    AND (@CustomerId IS NULL OR si.CustomerId = @CustomerId)
    AND (@Status IS NULL OR si.Status = @Status);
END
GO

-- Get Sales Invoices by Customer
CREATE OR ALTER PROCEDURE [Sales].[sp_GetSalesInvoicesByCustomer]
    @CustomerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        si.Id,
        si.InvoiceNumber,
        si.InvoiceDate,
        si.DueDate,
        si.Status,
        si.SubTotal,
        si.TaxAmount,
        si.TotalAmount,
        si.PaidAmount,
        si.BalanceAmount,
        si.Notes,
        si.Terms,
        si.PostedAt,
        si.CreatedAt
    FROM SalesInvoices si
    WHERE si.CustomerId = @CustomerId
    AND si.TenantId = @TenantId
    AND si.IsActive = 1
    AND (@Status IS NULL OR si.Status = @Status)
    ORDER BY si.InvoiceDate DESC;
END
GO

-- Get Sales Invoices by Status
CREATE OR ALTER PROCEDURE [Sales].[sp_GetSalesInvoicesByStatus]
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        si.Id,
        si.InvoiceNumber,
        si.CustomerId,
        si.InvoiceDate,
        si.DueDate,
        si.Status,
        si.SubTotal,
        si.TaxAmount,
        si.TotalAmount,
        si.PaidAmount,
        si.BalanceAmount,
        si.Notes,
        si.PostedAt,
        c.CustomerName
    FROM SalesInvoices si
    LEFT JOIN [CRM].Customers c ON si.CustomerId = c.Id
    WHERE si.Status = @Status
    AND si.TenantId = @TenantId
    AND si.IsActive = 1
    ORDER BY si.InvoiceDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM SalesInvoices si
    WHERE si.Status = @Status
    AND si.TenantId = @TenantId
    AND si.IsActive = 1;
END
GO

-- Create Sales Invoice
CREATE OR ALTER PROCEDURE [Sales].[sp_CreateSalesInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @InvoiceNumber NVARCHAR(50),
    @CustomerId UNIQUEIDENTIFIER,
    @InvoiceDate DATETIME2,
    @DueDate DATETIME2,
    @Status NVARCHAR(50) = 'Draft',
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @PaidAmount DECIMAL(18,2) = 0,
    @BalanceAmount DECIMAL(18,2) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @Terms NVARCHAR(MAX) = NULL,
    @SalesOrderId UNIQUEIDENTIFIER = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calculate BalanceAmount if not provided
    SET @BalanceAmount = ISNULL(@BalanceAmount, @TotalAmount - @PaidAmount);
    
    INSERT INTO SalesInvoices (
        Id, TenantId, InvoiceNumber, CustomerId, InvoiceDate, DueDate,
        Status, SubTotal, TaxAmount, TotalAmount, PaidAmount, BalanceAmount,
        Notes, Terms, SalesOrderId, IsActive, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @TenantId, @InvoiceNumber, @CustomerId, @InvoiceDate, @DueDate,
        @Status, @SubTotal, @TaxAmount, @TotalAmount, @PaidAmount, @BalanceAmount,
        @Notes, @Terms, @SalesOrderId, 1, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Sales Invoice
CREATE OR ALTER PROCEDURE [Sales].[sp_UpdateSalesInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @InvoiceNumber NVARCHAR(50) = NULL,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @InvoiceDate DATETIME2 = NULL,
    @DueDate DATETIME2 = NULL,
    @Status NVARCHAR(50) = NULL,
    @SubTotal DECIMAL(18,2) = NULL,
    @TaxAmount DECIMAL(18,2) = NULL,
    @TotalAmount DECIMAL(18,2) = NULL,
    @PaidAmount DECIMAL(18,2) = NULL,
    @BalanceAmount DECIMAL(18,2) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @Terms NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate invoice exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM SalesInvoices WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Sales invoice not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Recalculate BalanceAmount if TotalAmount or PaidAmount changed
    IF @BalanceAmount IS NULL AND (@TotalAmount IS NOT NULL OR @PaidAmount IS NOT NULL)
    BEGIN
        DECLARE @CurrentTotal DECIMAL(18,2);
        DECLARE @CurrentPaid DECIMAL(18,2);
        
        SELECT @CurrentTotal = ISNULL(@TotalAmount, TotalAmount),
               @CurrentPaid = ISNULL(@PaidAmount, PaidAmount)
        FROM SalesInvoices
        WHERE Id = @Id;
        
        SET @BalanceAmount = @CurrentTotal - @CurrentPaid;
    END
    
    UPDATE SalesInvoices
    SET 
        InvoiceNumber = ISNULL(@InvoiceNumber, InvoiceNumber),
        CustomerId = ISNULL(@CustomerId, CustomerId),
        InvoiceDate = ISNULL(@InvoiceDate, InvoiceDate),
        DueDate = ISNULL(@DueDate, DueDate),
        Status = ISNULL(@Status, Status),
        SubTotal = ISNULL(@SubTotal, SubTotal),
        TaxAmount = ISNULL(@TaxAmount, TaxAmount),
        TotalAmount = ISNULL(@TotalAmount, TotalAmount),
        PaidAmount = ISNULL(@PaidAmount, PaidAmount),
        BalanceAmount = ISNULL(@BalanceAmount, BalanceAmount),
        Notes = ISNULL(@Notes, Notes),
        Terms = ISNULL(@Terms, Terms),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId
    AND IsActive = 1;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Sales Invoice (Soft Delete)
CREATE OR ALTER PROCEDURE [Sales].[sp_DeleteSalesInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate invoice exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM SalesInvoices WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Sales invoice not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE SalesInvoices
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Post Sales Invoice
CREATE OR ALTER PROCEDURE [Sales].[sp_PostSalesInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PostedBy UNIQUEIDENTIFIER,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate invoice exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM SalesInvoices WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Sales invoice not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Check if invoice is in Draft status
    IF EXISTS (SELECT 1 FROM SalesInvoices WHERE Id = @Id AND Status != 'Draft')
    BEGIN
        RAISERROR('Only Draft invoices can be posted.', 16, 1);
        RETURN;
    END
    
    UPDATE SalesInvoices
    SET 
        Status = 'Posted',
        PostedAt = GETUTCDATE(),
        PostedBy = @PostedBy,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, CAST(@PostedBy AS NVARCHAR(100)))
    WHERE Id = @Id
    AND TenantId = @TenantId
    AND IsActive = 1;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Void Sales Invoice
CREATE OR ALTER PROCEDURE [Sales].[sp_VoidSalesInvoice]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate invoice exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM SalesInvoices WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Sales invoice not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE SalesInvoices
    SET 
        Status = 'Voided',
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId
    AND IsActive = 1;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Get Sales Invoice Lines by Invoice ID
CREATE OR ALTER PROCEDURE [Sales].[sp_GetSalesInvoiceLinesByInvoiceId]
    @SalesInvoiceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sil.Id,
        sil.SalesInvoiceId,
        sil.ItemId,
        sil.Description,
        sil.Quantity,
        sil.UnitPrice,
        sil.TaxRate,
        sil.TaxAmount,
        sil.LineTotal,
        sil.LineNumber,
        sil.CreatedAt,
        sil.UpdatedAt,
        i.ItemName,
        i.ItemCode
    FROM SalesInvoiceLines sil
    INNER JOIN SalesInvoices si ON sil.SalesInvoiceId = si.Id
    LEFT JOIN Items i ON sil.ItemId = i.Id
    WHERE sil.SalesInvoiceId = @SalesInvoiceId
    AND si.TenantId = @TenantId
    AND sil.IsActive = 1
    ORDER BY sil.LineNumber;
END
GO

PRINT 'Sales/POS Module stored procedures created successfully!'
