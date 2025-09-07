-- =============================================
-- CRM MODULE DATABASE SCHEMA
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create CRM Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'CRM')
BEGIN
    EXEC('CREATE SCHEMA [CRM]')
END
GO

-- =============================================
-- CUSTOMERS TABLE
-- =============================================
CREATE TABLE [CRM].[Customers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CustomerCode] NVARCHAR(50) NOT NULL,
    [CustomerName] NVARCHAR(200) NOT NULL,
    [CustomerType] NVARCHAR(50) NOT NULL,
    [PrimaryEmail] NVARCHAR(200),
    [SecondaryEmail] NVARCHAR(200),
    [PrimaryPhone] NVARCHAR(20),
    [SecondaryPhone] NVARCHAR(20),
    [Website] NVARCHAR(200),
    [TaxNumber] NVARCHAR(50),
    [CreditLimit] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [PaymentTerms] NVARCHAR(100),
    [Currency] NVARCHAR(10) NOT NULL DEFAULT 'USD',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- CUSTOMER ADDRESSES TABLE
-- =============================================
CREATE TABLE [CRM].[CustomerAddresses] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [AddressType] NVARCHAR(50) NOT NULL,
    [AddressLine1] NVARCHAR(200) NOT NULL,
    [AddressLine2] NVARCHAR(200),
    [City] NVARCHAR(100) NOT NULL,
    [State] NVARCHAR(100),
    [Country] NVARCHAR(100) NOT NULL,
    [ZipCode] NVARCHAR(20),
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- CONTACTS TABLE
-- =============================================
CREATE TABLE [CRM].[Contacts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [JobTitle] NVARCHAR(100),
    [Department] NVARCHAR(100),
    [Email] NVARCHAR(200),
    [Phone] NVARCHAR(20),
    [Mobile] NVARCHAR(20),
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- LEADS TABLE
-- =============================================
CREATE TABLE [CRM].[Leads] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [LeadName] NVARCHAR(200) NOT NULL,
    [CompanyName] NVARCHAR(200),
    [Email] NVARCHAR(200),
    [Phone] NVARCHAR(20),
    [Source] NVARCHAR(100),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'New',
    [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    [EstimatedValue] DECIMAL(18,2),
    [ExpectedCloseDate] DATETIME2,
    [AssignedTo] UNIQUEIDENTIFIER,
    [Description] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- OPPORTUNITIES TABLE
-- =============================================
CREATE TABLE [CRM].[Opportunities] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [OpportunityName] NVARCHAR(200) NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER,
    [LeadId] UNIQUEIDENTIFIER,
    [Stage] NVARCHAR(50) NOT NULL DEFAULT 'Prospecting',
    [Probability] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [ExpectedCloseDate] DATETIME2,
    [ActualCloseDate] DATETIME2,
    [AssignedTo] UNIQUEIDENTIFIER,
    [Source] NVARCHAR(100),
    [Description] NVARCHAR(1000),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]),
    FOREIGN KEY ([LeadId]) REFERENCES [CRM].[Leads]([Id])
);
GO

-- =============================================
-- ACTIVITIES TABLE
-- =============================================
CREATE TABLE [CRM].[Activities] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ActivityType] NVARCHAR(50) NOT NULL,
    [Subject] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000),
    [CustomerId] UNIQUEIDENTIFIER,
    [ContactId] UNIQUEIDENTIFIER,
    [LeadId] UNIQUEIDENTIFIER,
    [OpportunityId] UNIQUEIDENTIFIER,
    [AssignedTo] UNIQUEIDENTIFIER,
    [DueDate] DATETIME2,
    [CompletedDate] DATETIME2,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Not Started',
    [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]),
    FOREIGN KEY ([ContactId]) REFERENCES [CRM].[Contacts]([Id]),
    FOREIGN KEY ([LeadId]) REFERENCES [CRM].[Leads]([Id]),
    FOREIGN KEY ([OpportunityId]) REFERENCES [CRM].[Opportunities]([Id])
);
GO

-- =============================================
-- QUOTES TABLE
-- =============================================
CREATE TABLE [CRM].[Quotes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [QuoteNumber] NVARCHAR(50) NOT NULL,
    [QuoteDate] DATETIME2 NOT NULL,
    [ValidUntil] DATETIME2 NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [ContactId] UNIQUEIDENTIFIER,
    [OpportunityId] UNIQUEIDENTIFIER,
    [SubTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [Notes] NVARCHAR(1000),
    [Terms] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]),
    FOREIGN KEY ([ContactId]) REFERENCES [CRM].[Contacts]([Id]),
    FOREIGN KEY ([OpportunityId]) REFERENCES [CRM].[Opportunities]([Id])
);
GO

-- =============================================
-- QUOTE ITEMS TABLE
-- =============================================
CREATE TABLE [CRM].[QuoteItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [QuoteId] UNIQUEIDENTIFIER NOT NULL,
    [ItemName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [Quantity] DECIMAL(18,2) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [DiscountRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TaxRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [LineTotal] DECIMAL(18,2) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([QuoteId]) REFERENCES [CRM].[Quotes]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- SALES ORDERS TABLE
-- =============================================
CREATE TABLE [CRM].[SalesOrders] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [OrderNumber] NVARCHAR(50) NOT NULL,
    [OrderDate] DATETIME2 NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [ContactId] UNIQUEIDENTIFIER,
    [QuoteId] UNIQUEIDENTIFIER,
    [OpportunityId] UNIQUEIDENTIFIER,
    [SubTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [ExpectedDeliveryDate] DATETIME2,
    [Notes] NVARCHAR(1000),
    [Terms] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]),
    FOREIGN KEY ([ContactId]) REFERENCES [CRM].[Contacts]([Id]),
    FOREIGN KEY ([QuoteId]) REFERENCES [CRM].[Quotes]([Id]),
    FOREIGN KEY ([OpportunityId]) REFERENCES [CRM].[Opportunities]([Id])
);
GO

-- =============================================
-- SALES ORDER ITEMS TABLE
-- =============================================
CREATE TABLE [CRM].[SalesOrderItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [ItemName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [Quantity] DECIMAL(18,2) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [DiscountRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TaxRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [LineTotal] DECIMAL(18,2) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([OrderId]) REFERENCES [CRM].[SalesOrders]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- CAMPAIGNS TABLE
-- =============================================
CREATE TABLE [CRM].[Campaigns] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CampaignName] NVARCHAR(200) NOT NULL,
    [CampaignType] NVARCHAR(50) NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2,
    [Budget] DECIMAL(18,2),
    [ExpectedRevenue] DECIMAL(18,2),
    [ActualRevenue] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Planning',
    [Description] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- CAMPAIGN MEMBERS TABLE
-- =============================================
CREATE TABLE [CRM].[CampaignMembers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CampaignId] UNIQUEIDENTIFIER NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER,
    [LeadId] UNIQUEIDENTIFIER,
    [ContactId] UNIQUEIDENTIFIER,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Sent',
    [ResponseDate] DATETIME2,
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CampaignId]) REFERENCES [CRM].[Campaigns]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]),
    FOREIGN KEY ([LeadId]) REFERENCES [CRM].[Leads]([Id]),
    FOREIGN KEY ([ContactId]) REFERENCES [CRM].[Contacts]([Id])
);
GO

-- =============================================
-- INDEXES FOR CRM MODULE
-- =============================================
CREATE INDEX IX_Customers_TenantId ON [CRM].[Customers]([TenantId]);
CREATE INDEX IX_Customers_CustomerCode ON [CRM].[Customers]([CustomerCode]);
CREATE INDEX IX_CustomerAddresses_CustomerId ON [CRM].[CustomerAddresses]([CustomerId]);
CREATE INDEX IX_Contacts_CustomerId ON [CRM].[Contacts]([CustomerId]);
CREATE INDEX IX_Leads_TenantId ON [CRM].[Leads]([TenantId]);
CREATE INDEX IX_Leads_AssignedTo ON [CRM].[Leads]([AssignedTo]);
CREATE INDEX IX_Opportunities_CustomerId ON [CRM].[Opportunities]([CustomerId]);
CREATE INDEX IX_Opportunities_LeadId ON [CRM].[Opportunities]([LeadId]);
CREATE INDEX IX_Opportunities_AssignedTo ON [CRM].[Opportunities]([AssignedTo]);
CREATE INDEX IX_Activities_CustomerId ON [CRM].[Activities]([CustomerId]);
CREATE INDEX IX_Activities_ContactId ON [CRM].[Activities]([ContactId]);
CREATE INDEX IX_Activities_LeadId ON [CRM].[Activities]([LeadId]);
CREATE INDEX IX_Activities_OpportunityId ON [CRM].[Activities]([OpportunityId]);
CREATE INDEX IX_Activities_AssignedTo ON [CRM].[Activities]([AssignedTo]);
CREATE INDEX IX_Quotes_CustomerId ON [CRM].[Quotes]([CustomerId]);
CREATE INDEX IX_Quotes_QuoteNumber ON [CRM].[Quotes]([QuoteNumber]);
CREATE INDEX IX_QuoteItems_QuoteId ON [CRM].[QuoteItems]([QuoteId]);
CREATE INDEX IX_SalesOrders_CustomerId ON [CRM].[SalesOrders]([CustomerId]);
CREATE INDEX IX_SalesOrders_OrderNumber ON [CRM].[SalesOrders]([OrderNumber]);
CREATE INDEX IX_SalesOrderItems_OrderId ON [CRM].[SalesOrderItems]([OrderId]);
CREATE INDEX IX_Campaigns_TenantId ON [CRM].[Campaigns]([TenantId]);
CREATE INDEX IX_CampaignMembers_CampaignId ON [CRM].[CampaignMembers]([CampaignId]);
CREATE INDEX IX_CampaignMembers_CustomerId ON [CRM].[CampaignMembers]([CustomerId]);
CREATE INDEX IX_CampaignMembers_LeadId ON [CRM].[CampaignMembers]([LeadId]);
CREATE INDEX IX_CampaignMembers_ContactId ON [CRM].[CampaignMembers]([ContactId]);

-- =============================================
-- CUSTOMER GROUPS TABLE
-- =============================================
CREATE TABLE [CRM].[CustomerGroups] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [GroupName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [DiscountPercentage] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- CONTACT PERSONS TABLE
-- =============================================
CREATE TABLE [CRM].[ContactPersons] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [JobTitle] NVARCHAR(100),
    [Department] NVARCHAR(100),
    [Email] NVARCHAR(200),
    [Phone] NVARCHAR(20),
    [Mobile] NVARCHAR(20),
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- SALES TERRITORIES TABLE
-- =============================================
CREATE TABLE [CRM].[SalesTerritories] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TerritoryName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [ManagerId] UNIQUEIDENTIFIER,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- DISCOUNT POLICIES TABLE
-- =============================================
CREATE TABLE [CRM].[DiscountPolicies] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PolicyName] NVARCHAR(100) NOT NULL,
    [CustomerGroupId] UNIQUEIDENTIFIER,
    [CustomerId] UNIQUEIDENTIFIER,
    [ItemGroupId] UNIQUEIDENTIFIER,
    [ItemId] UNIQUEIDENTIFIER,
    [DiscountType] NVARCHAR(50) NOT NULL, -- Percentage, Fixed Amount
    [DiscountValue] DECIMAL(18,2) NOT NULL,
    [MinQuantity] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [MinAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [ValidFrom] DATETIME2 NOT NULL,
    [ValidTo] DATETIME2,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([CustomerGroupId]) REFERENCES [CRM].[CustomerGroups]([Id]),
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id])
);
GO

-- =============================================
-- RETURN ORDERS TABLE
-- =============================================
CREATE TABLE [CRM].[ReturnOrders] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ReturnNumber] NVARCHAR(50) NOT NULL,
    [ReturnDate] DATETIME2 NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [SalesOrderId] UNIQUEIDENTIFIER,
    [Reason] NVARCHAR(500) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [ApprovedBy] UNIQUEIDENTIFIER,
    [ApprovedAt] DATETIME2,
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id]),
    FOREIGN KEY ([SalesOrderId]) REFERENCES [CRM].[SalesOrders]([Id])
);
GO

-- =============================================
-- RETURN ORDER ITEMS TABLE
-- =============================================
CREATE TABLE [CRM].[ReturnOrderItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ReturnOrderId] UNIQUEIDENTIFIER NOT NULL,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [Reason] NVARCHAR(500),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([ReturnOrderId]) REFERENCES [CRM].[ReturnOrders]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- PRICE LISTS TABLE
-- =============================================
CREATE TABLE [CRM].[PriceLists] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PriceListName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [ValidFrom] DATETIME2 NOT NULL,
    [ValidTo] DATETIME2,
    [IsDefault] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- PRICE LIST ITEMS TABLE
-- =============================================
CREATE TABLE [CRM].[PriceListItems] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PriceListId] UNIQUEIDENTIFIER NOT NULL,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [MinQuantity] DECIMAL(18,4) NOT NULL DEFAULT 0,
    [MaxQuantity] DECIMAL(18,4),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([PriceListId]) REFERENCES [CRM].[PriceLists]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- SERVICE CONTRACTS TABLE
-- =============================================
CREATE TABLE [CRM].[ServiceContracts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ContractNumber] NVARCHAR(50) NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [ServiceType] NVARCHAR(100) NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [ContractValue] DECIMAL(18,2) NOT NULL,
    [BillingFrequency] NVARCHAR(50) NOT NULL, -- Monthly, Quarterly, Annually
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [Description] NVARCHAR(1000),
    [Terms] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([CustomerId]) REFERENCES [CRM].[Customers]([Id])
);
GO

-- =============================================
-- ADDITIONAL INDEXES FOR CRM MODULE
-- =============================================
CREATE INDEX IX_CustomerGroups_TenantId ON [CRM].[CustomerGroups]([TenantId]);
CREATE INDEX IX_ContactPersons_CustomerId ON [CRM].[ContactPersons]([CustomerId]);
CREATE INDEX IX_SalesTerritories_TenantId ON [CRM].[SalesTerritories]([TenantId]);
CREATE INDEX IX_DiscountPolicies_TenantId ON [CRM].[DiscountPolicies]([TenantId]);
CREATE INDEX IX_DiscountPolicies_CustomerGroupId ON [CRM].[DiscountPolicies]([CustomerGroupId]);
CREATE INDEX IX_DiscountPolicies_CustomerId ON [CRM].[DiscountPolicies]([CustomerId]);
CREATE INDEX IX_ReturnOrders_CustomerId ON [CRM].[ReturnOrders]([CustomerId]);
CREATE INDEX IX_ReturnOrders_SalesOrderId ON [CRM].[ReturnOrders]([SalesOrderId]);
CREATE INDEX IX_ReturnOrderItems_ReturnOrderId ON [CRM].[ReturnOrderItems]([ReturnOrderId]);
CREATE INDEX IX_PriceLists_TenantId ON [CRM].[PriceLists]([TenantId]);
CREATE INDEX IX_PriceListItems_PriceListId ON [CRM].[PriceListItems]([PriceListId]);
CREATE INDEX IX_PriceListItems_ItemId ON [CRM].[PriceListItems]([ItemId]);
CREATE INDEX IX_ServiceContracts_CustomerId ON [CRM].[ServiceContracts]([CustomerId]);
CREATE INDEX IX_ServiceContracts_Status ON [CRM].[ServiceContracts]([Status]);

PRINT 'CRM Module schema created successfully!'