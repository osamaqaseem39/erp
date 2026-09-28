-- =============================================
-- CRM MODULE STORED PROCEDURES
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create CRM Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'CRM')
BEGIN
    EXEC('CREATE SCHEMA [CRM]')
END
GO

-- =============================================
-- CUSTOMER MANAGEMENT PROCEDURES
-- =============================================

-- Get Customer by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetCustomerById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CustomerCode, CustomerName, CustomerType, PrimaryEmail, SecondaryEmail,
        PrimaryPhone, SecondaryPhone, Website, TaxNumber, CreditLimit, PaymentTerms,
        Currency, Status, Notes, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[Customers]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get Customer by Customer Code
CREATE OR ALTER PROCEDURE [CRM].[sp_GetCustomerByCode]
    @CustomerCode NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, CustomerCode, CustomerName, CustomerType, PrimaryEmail, SecondaryEmail,
        PrimaryPhone, SecondaryPhone, Website, TaxNumber, CreditLimit, PaymentTerms,
        Currency, Status, Notes, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[Customers]
    WHERE CustomerCode = @CustomerCode AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get All Customers for Tenant
CREATE OR ALTER PROCEDURE [CRM].[sp_GetAllCustomers]
    @TenantId UNIQUEIDENTIFIER,
    @CustomerType NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, CustomerCode, CustomerName, CustomerType, PrimaryEmail, PrimaryPhone,
        Website, CreditLimit, PaymentTerms, Currency, Status, Notes, TenantId,
        CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[Customers]
    WHERE TenantId = @TenantId 
        AND IsActive = 1
        AND (@CustomerType IS NULL OR CustomerType = @CustomerType)
        AND (@Status IS NULL OR Status = @Status)
    ORDER BY CustomerName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].[Customers]
    WHERE TenantId = @TenantId 
        AND IsActive = 1
        AND (@CustomerType IS NULL OR CustomerType = @CustomerType)
        AND (@Status IS NULL OR Status = @Status);
END
GO

-- Create Customer
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateCustomer]
    @Id UNIQUEIDENTIFIER,
    @CustomerCode NVARCHAR(50),
    @CustomerName NVARCHAR(200),
    @CustomerType NVARCHAR(50),
    @PrimaryEmail NVARCHAR(200),
    @SecondaryEmail NVARCHAR(200),
    @PrimaryPhone NVARCHAR(20),
    @SecondaryPhone NVARCHAR(20),
    @Website NVARCHAR(200),
    @TaxNumber NVARCHAR(50),
    @CreditLimit DECIMAL(18,2),
    @PaymentTerms NVARCHAR(100),
    @Currency NVARCHAR(10),
    @Status NVARCHAR(50),
    @Notes NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [CRM].[Customers] (
        Id, CustomerCode, CustomerName, CustomerType, PrimaryEmail, SecondaryEmail,
        PrimaryPhone, SecondaryPhone, Website, TaxNumber, CreditLimit, PaymentTerms,
        Currency, Status, Notes, TenantId, CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @CustomerCode, @CustomerName, @CustomerType, @PrimaryEmail, @SecondaryEmail,
        @PrimaryPhone, @SecondaryPhone, @Website, @TaxNumber, @CreditLimit, @PaymentTerms,
        @Currency, @Status, @Notes, @TenantId, GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- Update Customer
CREATE OR ALTER PROCEDURE [CRM].[sp_UpdateCustomer]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CustomerCode NVARCHAR(50) = NULL,
    @CustomerName NVARCHAR(200) = NULL,
    @CustomerType NVARCHAR(50) = NULL,
    @PrimaryEmail NVARCHAR(200) = NULL,
    @SecondaryEmail NVARCHAR(200) = NULL,
    @PrimaryPhone NVARCHAR(20) = NULL,
    @SecondaryPhone NVARCHAR(20) = NULL,
    @Website NVARCHAR(200) = NULL,
    @TaxNumber NVARCHAR(50) = NULL,
    @CreditLimit DECIMAL(18,2) = NULL,
    @PaymentTerms NVARCHAR(100) = NULL,
    @Currency NVARCHAR(10) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate customer exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [CRM].[Customers] WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1)
    BEGIN
        RAISERROR('Customer not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [CRM].[Customers]
    SET 
        CustomerCode = ISNULL(@CustomerCode, CustomerCode),
        CustomerName = ISNULL(@CustomerName, CustomerName),
        CustomerType = ISNULL(@CustomerType, CustomerType),
        PrimaryEmail = ISNULL(@PrimaryEmail, PrimaryEmail),
        SecondaryEmail = ISNULL(@SecondaryEmail, SecondaryEmail),
        PrimaryPhone = ISNULL(@PrimaryPhone, PrimaryPhone),
        SecondaryPhone = ISNULL(@SecondaryPhone, SecondaryPhone),
        Website = ISNULL(@Website, Website),
        TaxNumber = ISNULL(@TaxNumber, TaxNumber),
        CreditLimit = ISNULL(@CreditLimit, CreditLimit),
        PaymentTerms = ISNULL(@PaymentTerms, PaymentTerms),
        Currency = ISNULL(@Currency, Currency),
        Status = ISNULL(@Status, Status),
        Notes = ISNULL(@Notes, Notes),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Customer (Soft Delete)
CREATE OR ALTER PROCEDURE [CRM].[sp_DeleteCustomer]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate customer exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [CRM].[Customers] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Customer not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [CRM].[Customers]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- CONTACT MANAGEMENT PROCEDURES
-- =============================================

-- Get Contact by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetContactById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.Id, c.CustomerId, c.FirstName, c.LastName, c.JobTitle, c.Department,
        c.Email, c.Phone, c.Mobile, c.IsPrimary, c.Status, c.Notes, c.TenantId,
        c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy, c.IsActive,
        cu.CustomerName
    FROM [CRM].[Contacts] c
    INNER JOIN [CRM].[Customers] cu ON c.CustomerId = cu.Id
    WHERE c.Id = @Id AND c.TenantId = @TenantId AND c.IsActive = 1;
END
GO

-- Get Contacts by Customer ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetContactsByCustomerId]
    @CustomerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, CustomerId, FirstName, LastName, JobTitle, Department, Email, Phone,
        Mobile, IsPrimary, Status, Notes, TenantId, CreatedAt, UpdatedAt,
        CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[Contacts]
    WHERE CustomerId = @CustomerId AND TenantId = @TenantId AND IsActive = 1
    ORDER BY IsPrimary DESC, FirstName, LastName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].[Contacts]
    WHERE CustomerId = @CustomerId AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Create Contact
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateContact]
    @Id UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @JobTitle NVARCHAR(100),
    @Department NVARCHAR(100),
    @Email NVARCHAR(200),
    @Phone NVARCHAR(20),
    @Mobile NVARCHAR(20),
    @IsPrimary BIT,
    @Status NVARCHAR(50),
    @Notes NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If this is the primary contact, unset other primary contacts
    IF @IsPrimary = 1
    BEGIN
        UPDATE [CRM].[Contacts]
        SET IsPrimary = 0
        WHERE CustomerId = @CustomerId AND TenantId = @TenantId AND IsActive = 1;
    END
    
    INSERT INTO [CRM].[Contacts] (
        Id, CustomerId, FirstName, LastName, JobTitle, Department, Email, Phone,
        Mobile, IsPrimary, Status, Notes, TenantId, CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @CustomerId, @FirstName, @LastName, @JobTitle, @Department, @Email, @Phone,
        @Mobile, @IsPrimary, @Status, @Notes, @TenantId, GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- LEAD MANAGEMENT PROCEDURES
-- =============================================

-- Get Lead by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetLeadById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, LeadName, CompanyName, Email, Phone, Source, Status, Priority,
        EstimatedValue, ExpectedCloseDate, AssignedTo, Description, TenantId,
        CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[Leads]
    WHERE Id = @Id AND TenantId = @TenantId AND IsActive = 1;
END
GO

-- Get All Leads for Tenant
CREATE OR ALTER PROCEDURE [CRM].[sp_GetAllLeads]
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @Priority NVARCHAR(50) = NULL,
    @AssignedTo UNIQUEIDENTIFIER = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, LeadName, CompanyName, Email, Phone, Source, Status, Priority,
        EstimatedValue, ExpectedCloseDate, AssignedTo, Description, TenantId,
        CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[Leads]
    WHERE TenantId = @TenantId 
        AND IsActive = 1
        AND (@Status IS NULL OR Status = @Status)
        AND (@Priority IS NULL OR Priority = @Priority)
        AND (@AssignedTo IS NULL OR AssignedTo = @AssignedTo)
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].[Leads]
    WHERE TenantId = @TenantId 
        AND IsActive = 1
        AND (@Status IS NULL OR Status = @Status)
        AND (@Priority IS NULL OR Priority = @Priority)
        AND (@AssignedTo IS NULL OR AssignedTo = @AssignedTo);
END
GO

-- Create Lead
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateLead]
    @Id UNIQUEIDENTIFIER,
    @LeadName NVARCHAR(200),
    @CompanyName NVARCHAR(200),
    @Email NVARCHAR(200),
    @Phone NVARCHAR(20),
    @Source NVARCHAR(100),
    @Status NVARCHAR(50),
    @Priority NVARCHAR(50),
    @EstimatedValue DECIMAL(18,2),
    @ExpectedCloseDate DATETIME2,
    @AssignedTo UNIQUEIDENTIFIER,
    @Description NVARCHAR(1000),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [CRM].[Leads] (
        Id, LeadName, CompanyName, Email, Phone, Source, Status, Priority,
        EstimatedValue, ExpectedCloseDate, AssignedTo, Description, TenantId,
        CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @LeadName, @CompanyName, @Email, @Phone, @Source, @Status, @Priority,
        @EstimatedValue, @ExpectedCloseDate, @AssignedTo, @Description, @TenantId,
        GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- OPPORTUNITY MANAGEMENT PROCEDURES
-- =============================================

-- Get Opportunity by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetOpportunityById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.Id, o.OpportunityName, o.CustomerId, o.LeadId, o.Stage, o.Probability,
        o.Amount, o.ExpectedCloseDate, o.ActualCloseDate, o.AssignedTo, o.Source,
        o.Description, o.Status, o.TenantId, o.CreatedAt, o.UpdatedAt, o.CreatedBy, o.UpdatedBy, o.IsActive,
        c.CustomerName,
        l.LeadName
    FROM [CRM].[Opportunities] o
    LEFT JOIN [CRM].[Customers] c ON o.CustomerId = c.Id
    LEFT JOIN [CRM].[Leads] l ON o.LeadId = l.Id
    WHERE o.Id = @Id AND o.TenantId = @TenantId AND o.IsActive = 1;
END
GO

-- Get Opportunities by Customer ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetOpportunitiesByCustomerId]
    @CustomerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Stage NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        o.Id, o.OpportunityName, o.CustomerId, o.LeadId, o.Stage, o.Probability,
        o.Amount, o.ExpectedCloseDate, o.ActualCloseDate, o.AssignedTo, o.Source,
        o.Description, o.Status, o.TenantId, o.CreatedAt, o.UpdatedAt, o.CreatedBy, o.UpdatedBy, o.IsActive,
        c.CustomerName,
        l.LeadName
    FROM [CRM].[Opportunities] o
    LEFT JOIN [CRM].[Customers] c ON o.CustomerId = c.Id
    LEFT JOIN [CRM].[Leads] l ON o.LeadId = l.Id
    WHERE o.CustomerId = @CustomerId 
        AND o.TenantId = @TenantId 
        AND o.IsActive = 1
        AND (@Stage IS NULL OR o.Stage = @Stage)
        AND (@Status IS NULL OR o.Status = @Status)
    ORDER BY o.ExpectedCloseDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].[Opportunities] o
    WHERE o.CustomerId = @CustomerId 
        AND o.TenantId = @TenantId 
        AND o.IsActive = 1
        AND (@Stage IS NULL OR o.Stage = @Stage)
        AND (@Status IS NULL OR o.Status = @Status);
END
GO

-- Create Opportunity
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateOpportunity]
    @Id UNIQUEIDENTIFIER,
    @OpportunityName NVARCHAR(200),
    @CustomerId UNIQUEIDENTIFIER,
    @LeadId UNIQUEIDENTIFIER,
    @Stage NVARCHAR(50),
    @Probability DECIMAL(5,2),
    @Amount DECIMAL(18,2),
    @ExpectedCloseDate DATETIME2,
    @AssignedTo UNIQUEIDENTIFIER,
    @Source NVARCHAR(100),
    @Description NVARCHAR(1000),
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [CRM].[Opportunities] (
        Id, OpportunityName, CustomerId, LeadId, Stage, Probability, Amount,
        ExpectedCloseDate, AssignedTo, Source, Description, Status, TenantId,
        CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @OpportunityName, @CustomerId, @LeadId, @Stage, @Probability, @Amount,
        @ExpectedCloseDate, @AssignedTo, @Source, @Description, @Status, @TenantId,
        GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- QUOTE MANAGEMENT PROCEDURES
-- =============================================

-- Get Quote by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetQuoteById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        q.Id, q.QuoteNumber, q.QuoteDate, q.ValidUntil, q.CustomerId, q.ContactId,
        q.OpportunityId, q.SubTotal, q.TaxAmount, q.DiscountAmount, q.TotalAmount,
        q.Status, q.Notes, q.Terms, q.TenantId, q.CreatedAt, q.UpdatedAt, q.CreatedBy, q.UpdatedBy, q.IsActive,
        c.CustomerName,
        co.FirstName + ' ' + co.LastName as ContactName,
        o.OpportunityName
    FROM [CRM].[Quotes] q
    LEFT JOIN [CRM].[Customers] c ON q.CustomerId = c.Id
    LEFT JOIN [CRM].[Contacts] co ON q.ContactId = co.Id
    LEFT JOIN [CRM].[Opportunities] o ON q.OpportunityId = o.Id
    WHERE q.Id = @Id AND q.TenantId = @TenantId;
END
GO

-- Get Quote Items by Quote ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetQuoteItemsByQuoteId]
    @QuoteId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, QuoteId, ItemName, Description, Quantity, UnitPrice, DiscountRate,
        DiscountAmount, TaxRate, TaxAmount, LineTotal, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[QuoteItems]
    WHERE QuoteId = @QuoteId AND TenantId = @TenantId
    ORDER BY Id;
END
GO

-- Create Quote
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateQuote]
    @Id UNIQUEIDENTIFIER,
    @QuoteNumber NVARCHAR(50),
    @QuoteDate DATETIME2,
    @ValidUntil DATETIME2,
    @CustomerId UNIQUEIDENTIFIER,
    @ContactId UNIQUEIDENTIFIER,
    @OpportunityId UNIQUEIDENTIFIER,
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @Notes NVARCHAR(1000),
    @Terms NVARCHAR(1000),
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [CRM].[Quotes] (
        Id, QuoteNumber, QuoteDate, ValidUntil, CustomerId, ContactId, OpportunityId,
        SubTotal, TaxAmount, DiscountAmount, TotalAmount, Status, Notes, Terms,
        TenantId, CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @QuoteNumber, @QuoteDate, @ValidUntil, @CustomerId, @ContactId, @OpportunityId,
        @SubTotal, @TaxAmount, @DiscountAmount, @TotalAmount, @Status, @Notes, @Terms,
        @TenantId, GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- SALES ORDER PROCEDURES
-- =============================================

-- Get Sales Order by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetSalesOrderById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        so.Id, so.OrderNumber, so.OrderDate, so.CustomerId, so.ContactId, so.QuoteId,
        so.OpportunityId, so.SubTotal, so.TaxAmount, so.DiscountAmount, so.TotalAmount,
        so.Status, so.ExpectedDeliveryDate, so.Notes, so.Terms, so.TenantId, so.CreatedAt,
        so.UpdatedAt, so.CreatedBy, so.UpdatedBy, so.IsActive,
        c.CustomerName,
        co.FirstName + ' ' + co.LastName as ContactName,
        q.QuoteNumber,
        o.OpportunityName
    FROM [CRM].[SalesOrders] so
    LEFT JOIN [CRM].[Customers] c ON so.CustomerId = c.Id
    LEFT JOIN [CRM].[Contacts] co ON so.ContactId = co.Id
    LEFT JOIN [CRM].[Quotes] q ON so.QuoteId = q.Id
    LEFT JOIN [CRM].[Opportunities] o ON so.OpportunityId = o.Id
    WHERE so.Id = @Id AND so.TenantId = @TenantId;
END
GO

-- Get Sales Order Items by Order ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetSalesOrderItemsByOrderId]
    @OrderId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, OrderId, ItemName, Description, Quantity, UnitPrice, DiscountRate,
        DiscountAmount, TaxRate, TaxAmount, LineTotal, TenantId, CreatedAt,
        UpdatedAt, CreatedBy, UpdatedBy, IsActive
    FROM [CRM].[SalesOrderItems]
    WHERE OrderId = @OrderId AND TenantId = @TenantId
    ORDER BY Id;
END
GO

-- Create Sales Order
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateSalesOrder]
    @Id UNIQUEIDENTIFIER,
    @OrderNumber NVARCHAR(50),
    @OrderDate DATETIME2,
    @CustomerId UNIQUEIDENTIFIER,
    @ContactId UNIQUEIDENTIFIER,
    @QuoteId UNIQUEIDENTIFIER,
    @OpportunityId UNIQUEIDENTIFIER,
    @SubTotal DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @ExpectedDeliveryDate DATETIME2,
    @Notes NVARCHAR(1000),
    @Terms NVARCHAR(1000),
    @Status NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [CRM].[SalesOrders] (
        Id, OrderNumber, OrderDate, CustomerId, ContactId, QuoteId, OpportunityId,
        SubTotal, TaxAmount, DiscountAmount, TotalAmount, Status, ExpectedDeliveryDate,
        Notes, Terms, TenantId, CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @OrderNumber, @OrderDate, @CustomerId, @ContactId, @QuoteId, @OpportunityId,
        @SubTotal, @TaxAmount, @DiscountAmount, @TotalAmount, @Status, @ExpectedDeliveryDate,
        @Notes, @Terms, @TenantId, GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- ACTIVITY MANAGEMENT PROCEDURES
-- =============================================

-- Get Activity by ID
CREATE OR ALTER PROCEDURE [CRM].[sp_GetActivityById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.Id, a.ActivityType, a.Subject, a.Description, a.CustomerId, a.ContactId,
        a.LeadId, a.OpportunityId, a.AssignedTo, a.DueDate, a.CompletedDate,
        a.Status, a.Priority, a.TenantId, a.CreatedAt, a.UpdatedAt, a.CreatedBy, a.UpdatedBy, a.IsActive,
        c.CustomerName,
        co.FirstName + ' ' + co.LastName as ContactName,
        l.LeadName,
        o.OpportunityName
    FROM [CRM].[Activities] a
    LEFT JOIN [CRM].[Customers] c ON a.CustomerId = c.Id
    LEFT JOIN [CRM].[Contacts] co ON a.ContactId = co.Id
    LEFT JOIN [CRM].[Leads] l ON a.LeadId = l.Id
    LEFT JOIN [CRM].[Opportunities] o ON a.OpportunityId = o.Id
    WHERE a.Id = @Id AND a.TenantId = @TenantId AND a.IsActive = 1;
END
GO

-- Get Activities by Entity
CREATE OR ALTER PROCEDURE [CRM].[sp_GetActivitiesByEntity]
    @EntityType NVARCHAR(50),
    @EntityId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ActivityType NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        a.Id, a.ActivityType, a.Subject, a.Description, a.CustomerId, a.ContactId,
        a.LeadId, a.OpportunityId, a.AssignedTo, a.DueDate, a.CompletedDate,
        a.Status, a.Priority, a.TenantId, a.CreatedAt, a.UpdatedAt, a.CreatedBy, a.UpdatedBy, a.IsActive
    FROM [CRM].[Activities] a
    WHERE a.TenantId = @TenantId 
        AND a.IsActive = 1
        AND (
            (@EntityType = 'Customer' AND a.CustomerId = @EntityId) OR
            (@EntityType = 'Contact' AND a.ContactId = @EntityId) OR
            (@EntityType = 'Lead' AND a.LeadId = @EntityId) OR
            (@EntityType = 'Opportunity' AND a.OpportunityId = @EntityId)
        )
        AND (@ActivityType IS NULL OR a.ActivityType = @ActivityType)
        AND (@Status IS NULL OR a.Status = @Status)
    ORDER BY a.DueDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].[Activities] a
    WHERE a.TenantId = @TenantId 
        AND a.IsActive = 1
        AND (
            (@EntityType = 'Customer' AND a.CustomerId = @EntityId) OR
            (@EntityType = 'Contact' AND a.ContactId = @EntityId) OR
            (@EntityType = 'Lead' AND a.LeadId = @EntityId) OR
            (@EntityType = 'Opportunity' AND a.OpportunityId = @EntityId)
        )
        AND (@ActivityType IS NULL OR a.ActivityType = @ActivityType)
        AND (@Status IS NULL OR a.Status = @Status);
END
GO

-- Create Activity
CREATE OR ALTER PROCEDURE [CRM].[sp_CreateActivity]
    @Id UNIQUEIDENTIFIER,
    @ActivityType NVARCHAR(50),
    @Subject NVARCHAR(200),
    @Description NVARCHAR(1000),
    @CustomerId UNIQUEIDENTIFIER,
    @ContactId UNIQUEIDENTIFIER,
    @LeadId UNIQUEIDENTIFIER,
    @OpportunityId UNIQUEIDENTIFIER,
    @AssignedTo UNIQUEIDENTIFIER,
    @DueDate DATETIME2,
    @Status NVARCHAR(50),
    @Priority NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [CRM].[Activities] (
        Id, ActivityType, Subject, Description, CustomerId, ContactId, LeadId,
        OpportunityId, AssignedTo, DueDate, Status, Priority, TenantId,
        CreatedAt, CreatedBy, IsActive
    )
    VALUES (
        @Id, @ActivityType, @Subject, @Description, @CustomerId, @ContactId, @LeadId,
        @OpportunityId, @AssignedTo, @DueDate, @Status, @Priority, @TenantId,
        GETUTCDATE(), @CreatedBy, 1
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- CAMPAIGNS PROCEDURES
-- =============================================

-- Get All Campaigns
CREATE OR ALTER PROCEDURE [CRM].sp_GetAllCampaigns
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @CampaignType NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredCampaigns AS (
        SELECT 
            c.Id,
            c.CampaignName,
            c.CampaignType,
            c.StartDate,
            c.EndDate,
            c.Budget,
            c.ExpectedRevenue,
            c.ActualRevenue,
            c.Status,
            c.Description,
            c.CreatedAt,
            c.UpdatedAt,
            c.CreatedBy,
            c.UpdatedBy,
            ROW_NUMBER() OVER (ORDER BY c.CreatedAt DESC) AS RowNum
        FROM [CRM].Campaigns c
        WHERE c.TenantId = @TenantId
        AND (@Status IS NULL OR c.Status = @Status)
        AND (@CampaignType IS NULL OR c.CampaignType = @CampaignType)
        AND c.IsActive = 1
    )
    SELECT 
        Id,
        CampaignName,
        CampaignType,
        StartDate,
        EndDate,
        Budget,
        ExpectedRevenue,
        ActualRevenue,
        Status,
        Description,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy
    FROM FilteredCampaigns
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].Campaigns c
    WHERE c.TenantId = @TenantId
    AND (@Status IS NULL OR c.Status = @Status)
    AND (@CampaignType IS NULL OR c.CampaignType = @CampaignType)
    AND c.IsActive = 1;
END
GO

-- Create Campaign
CREATE OR ALTER PROCEDURE [CRM].sp_CreateCampaign
    @CampaignName NVARCHAR(200),
    @CampaignType NVARCHAR(50),
    @StartDate DATETIME2,
    @EndDate DATETIME2 = NULL,
    @Budget DECIMAL(18,2) = NULL,
    @ExpectedRevenue DECIMAL(18,2) = NULL,
    @Description NVARCHAR(1000) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [CRM].Campaigns (
        Id, CampaignName, CampaignType, StartDate, EndDate, Budget,
        ExpectedRevenue, Description, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CampaignName, @CampaignType, @StartDate, @EndDate, @Budget,
        @ExpectedRevenue, @Description, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- CUSTOMER GROUPS PROCEDURES
-- =============================================

-- Get All Customer Groups
CREATE OR ALTER PROCEDURE [CRM].sp_GetAllCustomerGroups
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        cg.Id,
        cg.GroupName,
        cg.Description,
        cg.DiscountPercentage,
        cg.IsActive,
        cg.CreatedAt,
        cg.UpdatedAt,
        cg.CreatedBy,
        cg.UpdatedBy
    FROM [CRM].CustomerGroups cg
    WHERE cg.TenantId = @TenantId
    AND (@IsActive IS NULL OR cg.IsActive = @IsActive)
    ORDER BY cg.GroupName;
END
GO

-- Create Customer Group
CREATE OR ALTER PROCEDURE [CRM].sp_CreateCustomerGroup
    @GroupName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @DiscountPercentage DECIMAL(5,2) = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [CRM].CustomerGroups (
        Id, GroupName, Description, DiscountPercentage, TenantId, CreatedBy
    )
    VALUES (
        @Id, @GroupName, @Description, @DiscountPercentage, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- CONTACT PERSONS PROCEDURES
-- =============================================

-- Get Customer Contact Persons
CREATE OR ALTER PROCEDURE [CRM].sp_GetCustomerContactPersons
    @CustomerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        cp.Id,
        cp.CustomerId,
        cp.FirstName,
        cp.LastName,
        cp.JobTitle,
        cp.Department,
        cp.Email,
        cp.Phone,
        cp.Mobile,
        cp.IsPrimary,
        cp.IsActive,
        cp.CreatedAt,
        cp.UpdatedAt,
        cp.CreatedBy,
        cp.UpdatedBy
    FROM [CRM].ContactPersons cp
    INNER JOIN [CRM].Customers c ON cp.CustomerId = c.Id
    WHERE c.Id = @CustomerId 
    AND c.TenantId = @TenantId
    AND cp.IsActive = 1
    ORDER BY cp.IsPrimary DESC, cp.FirstName, cp.LastName;
END
GO

-- Add Contact Person
CREATE OR ALTER PROCEDURE [CRM].sp_AddContactPerson
    @CustomerId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @JobTitle NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Mobile NVARCHAR(20) = NULL,
    @IsPrimary BIT = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    -- If this is set as primary, unset other primary contacts
    IF @IsPrimary = 1
    BEGIN
            UPDATE [CRM].ContactPersons
        SET IsPrimary = 0
        WHERE CustomerId = @CustomerId;
    END
    
    INSERT INTO [CRM].ContactPersons (
        Id, CustomerId, FirstName, LastName, JobTitle, Department,
        Email, Phone, Mobile, IsPrimary, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CustomerId, @FirstName, @LastName, @JobTitle, @Department,
        @Email, @Phone, @Mobile, @IsPrimary, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- SALES TERRITORIES PROCEDURES
-- =============================================

-- Get All Sales Territories
CREATE OR ALTER PROCEDURE [CRM].sp_GetAllSalesTerritories
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        st.Id,
        st.TerritoryName,
        st.Description,
        st.ManagerId,
        st.IsActive,
        st.CreatedAt,
        st.UpdatedAt,
        st.CreatedBy,
        st.UpdatedBy,
        u.FirstName + ' ' + u.LastName as ManagerName
    FROM [CRM].SalesTerritories st
    LEFT JOIN [Core].Users u ON st.ManagerId = u.Id
    WHERE st.TenantId = @TenantId
    AND (@IsActive IS NULL OR st.IsActive = @IsActive)
    ORDER BY st.TerritoryName;
END
GO

-- Create Sales Territory
CREATE OR ALTER PROCEDURE [CRM].sp_CreateSalesTerritory
    @TerritoryName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [CRM].SalesTerritories (
        Id, TerritoryName, Description, ManagerId, TenantId, CreatedBy
    )
    VALUES (
        @Id, @TerritoryName, @Description, @ManagerId, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- DISCOUNT POLICIES PROCEDURES
-- =============================================

-- Get Discount Policies
CREATE OR ALTER PROCEDURE [CRM].sp_GetDiscountPolicies
    @TenantId UNIQUEIDENTIFIER,
    @CustomerGroupId UNIQUEIDENTIFIER = NULL,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredPolicies AS (
        SELECT 
            dp.Id,
            dp.PolicyName,
            dp.CustomerGroupId,
            dp.CustomerId,
            dp.ItemGroupId,
            dp.ItemId,
            dp.DiscountType,
            dp.DiscountValue,
            dp.MinQuantity,
            dp.MinAmount,
            dp.ValidFrom,
            dp.ValidTo,
            dp.IsActive,
            dp.CreatedAt,
            dp.UpdatedAt,
            dp.CreatedBy,
            dp.UpdatedBy,
            cg.GroupName as CustomerGroupName,
            c.CustomerName,
            ROW_NUMBER() OVER (ORDER BY dp.CreatedAt DESC) AS RowNum
        FROM [CRM].DiscountPolicies dp
        LEFT JOIN [CRM].CustomerGroups cg ON dp.CustomerGroupId = cg.Id
        LEFT JOIN [CRM].Customers c ON dp.CustomerId = c.Id
        WHERE dp.TenantId = @TenantId
        AND (@CustomerGroupId IS NULL OR dp.CustomerGroupId = @CustomerGroupId)
        AND (@CustomerId IS NULL OR dp.CustomerId = @CustomerId)
        AND (@IsActive IS NULL OR dp.IsActive = @IsActive)
    )
    SELECT 
        Id,
        PolicyName,
        CustomerGroupId,
        CustomerId,
        ItemGroupId,
        ItemId,
        DiscountType,
        DiscountValue,
        MinQuantity,
        MinAmount,
        ValidFrom,
        ValidTo,
        IsActive,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        CustomerGroupName,
        CustomerName
    FROM FilteredPolicies
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].DiscountPolicies dp
    WHERE dp.TenantId = @TenantId
    AND (@CustomerGroupId IS NULL OR dp.CustomerGroupId = @CustomerGroupId)
    AND (@CustomerId IS NULL OR dp.CustomerId = @CustomerId)
    AND (@IsActive IS NULL OR dp.IsActive = @IsActive);
END
GO

-- Create Discount Policy
CREATE OR ALTER PROCEDURE [CRM].sp_CreateDiscountPolicy
    @PolicyName NVARCHAR(100),
    @CustomerGroupId UNIQUEIDENTIFIER = NULL,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @ItemGroupId UNIQUEIDENTIFIER = NULL,
    @ItemId UNIQUEIDENTIFIER = NULL,
    @DiscountType NVARCHAR(50),
    @DiscountValue DECIMAL(18,2),
    @MinQuantity DECIMAL(18,4) = 0,
    @MinAmount DECIMAL(18,2) = 0,
    @ValidFrom DATETIME2,
    @ValidTo DATETIME2 = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [CRM].DiscountPolicies (
        Id, PolicyName, CustomerGroupId, CustomerId, ItemGroupId, ItemId,
        DiscountType, DiscountValue, MinQuantity, MinAmount, ValidFrom, ValidTo,
        TenantId, CreatedBy
    )
    VALUES (
        @Id, @PolicyName, @CustomerGroupId, @CustomerId, @ItemGroupId, @ItemId,
        @DiscountType, @DiscountValue, @MinQuantity, @MinAmount, @ValidFrom, @ValidTo,
        @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- RETURN ORDERS PROCEDURES
-- =============================================

-- Get Return Orders
CREATE OR ALTER PROCEDURE [CRM].sp_GetReturnOrders
    @TenantId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredReturns AS (
        SELECT 
            ro.Id,
            ro.ReturnNumber,
            ro.ReturnDate,
            ro.CustomerId,
            ro.SalesOrderId,
            ro.Reason,
            ro.Status,
            ro.TotalAmount,
            ro.ApprovedBy,
            ro.ApprovedAt,
            ro.Notes,
            ro.CreatedAt,
            ro.UpdatedAt,
            ro.CreatedBy,
            ro.UpdatedBy,
            c.CustomerName,
            so.OrderNumber as SalesOrderNumber,
            u.FirstName + ' ' + u.LastName as ApprovedByName,
            ROW_NUMBER() OVER (ORDER BY ro.CreatedAt DESC) AS RowNum
        FROM [CRM].ReturnOrders ro
        INNER JOIN [CRM].Customers c ON ro.CustomerId = c.Id
        LEFT JOIN [CRM].SalesOrders so ON ro.SalesOrderId = so.Id
        LEFT JOIN [Core].Users u ON ro.ApprovedBy = u.Id
        WHERE c.TenantId = @TenantId
        AND (@CustomerId IS NULL OR ro.CustomerId = @CustomerId)
        AND (@Status IS NULL OR ro.Status = @Status)
        AND ro.IsActive = 1
    )
    SELECT 
        Id,
        ReturnNumber,
        ReturnDate,
        CustomerId,
        SalesOrderId,
        Reason,
        Status,
        TotalAmount,
        ApprovedBy,
        ApprovedAt,
        Notes,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        CustomerName,
        SalesOrderNumber,
        ApprovedByName
    FROM FilteredReturns
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].ReturnOrders ro
    INNER JOIN [CRM].Customers c ON ro.CustomerId = c.Id
    WHERE c.TenantId = @TenantId
    AND (@CustomerId IS NULL OR ro.CustomerId = @CustomerId)
    AND (@Status IS NULL OR ro.Status = @Status)
    AND ro.IsActive = 1;
END
GO

-- Create Return Order
CREATE OR ALTER PROCEDURE [CRM].sp_CreateReturnOrder
    @ReturnNumber NVARCHAR(50),
    @ReturnDate DATETIME2,
    @CustomerId UNIQUEIDENTIFIER,
    @SalesOrderId UNIQUEIDENTIFIER = NULL,
    @Reason NVARCHAR(500),
    @Notes NVARCHAR(1000) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [CRM].ReturnOrders (
        Id, ReturnNumber, ReturnDate, CustomerId, SalesOrderId, Reason, Notes, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ReturnNumber, @ReturnDate, @CustomerId, @SalesOrderId, @Reason, @Notes, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- PRICE LISTS PROCEDURES
-- =============================================

-- Get All Price Lists
CREATE OR ALTER PROCEDURE [CRM].sp_GetAllPriceLists
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pl.Id,
        pl.PriceListName,
        pl.Description,
        pl.ValidFrom,
        pl.ValidTo,
        pl.IsDefault,
        pl.IsActive,
        pl.CreatedAt,
        pl.UpdatedAt,
        pl.CreatedBy,
        pl.UpdatedBy
    FROM [CRM].PriceLists pl
    WHERE pl.TenantId = @TenantId
    AND (@IsActive IS NULL OR pl.IsActive = @IsActive)
    ORDER BY pl.IsDefault DESC, pl.PriceListName;
END
GO

-- Create Price List
CREATE OR ALTER PROCEDURE [CRM].sp_CreatePriceList
    @PriceListName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @ValidFrom DATETIME2,
    @ValidTo DATETIME2 = NULL,
    @IsDefault BIT = 0,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    -- If this is set as default, unset other default price lists
    IF @IsDefault = 1
    BEGIN
        UPDATE [CRM].PriceLists
        SET IsDefault = 0
        WHERE TenantId = @TenantId;
    END
    
    INSERT INTO [CRM].PriceLists (
        Id, PriceListName, Description, ValidFrom, ValidTo, IsDefault, TenantId, CreatedBy
    )
    VALUES (
        @Id, @PriceListName, @Description, @ValidFrom, @ValidTo, @IsDefault, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- SERVICE CONTRACTS PROCEDURES
-- =============================================

-- Get Service Contracts
CREATE OR ALTER PROCEDURE [CRM].sp_GetServiceContracts
    @TenantId UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredContracts AS (
        SELECT 
            sc.Id,
            sc.ContractNumber,
            sc.CustomerId,
            sc.ServiceType,
            sc.StartDate,
            sc.EndDate,
            sc.ContractValue,
            sc.BillingFrequency,
            sc.Status,
            sc.Description,
            sc.Terms,
            sc.CreatedAt,
            sc.UpdatedAt,
            sc.CreatedBy,
            sc.UpdatedBy,
            c.CustomerName,
            ROW_NUMBER() OVER (ORDER BY sc.CreatedAt DESC) AS RowNum
        FROM [CRM].ServiceContracts sc
        INNER JOIN [CRM].Customers c ON sc.CustomerId = c.Id
        WHERE c.TenantId = @TenantId
        AND (@CustomerId IS NULL OR sc.CustomerId = @CustomerId)
        AND (@Status IS NULL OR sc.Status = @Status)
        AND sc.IsActive = 1
    )
    SELECT 
        Id,
        ContractNumber,
        CustomerId,
        ServiceType,
        StartDate,
        EndDate,
        ContractValue,
        BillingFrequency,
        Status,
        Description,
        Terms,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        CustomerName
    FROM FilteredContracts
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [CRM].ServiceContracts sc
    INNER JOIN [CRM].Customers c ON sc.CustomerId = c.Id
    WHERE c.TenantId = @TenantId
    AND (@CustomerId IS NULL OR sc.CustomerId = @CustomerId)
    AND (@Status IS NULL OR sc.Status = @Status)
    AND sc.IsActive = 1;
END
GO

-- Create Service Contract
CREATE OR ALTER PROCEDURE [CRM].sp_CreateServiceContract
    @ContractNumber NVARCHAR(50),
    @CustomerId UNIQUEIDENTIFIER,
    @ServiceType NVARCHAR(100),
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @ContractValue DECIMAL(18,2),
    @BillingFrequency NVARCHAR(50),
    @Description NVARCHAR(1000) = NULL,
    @Terms NVARCHAR(1000) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [CRM].ServiceContracts (
        Id, ContractNumber, CustomerId, ServiceType, StartDate, EndDate,
        ContractValue, BillingFrequency, Description, Terms, TenantId, CreatedBy
    )
    VALUES (
        @Id, @ContractNumber, @CustomerId, @ServiceType, @StartDate, @EndDate,
        @ContractValue, @BillingFrequency, @Description, @Terms, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

PRINT 'CRM Module stored procedures created successfully!'