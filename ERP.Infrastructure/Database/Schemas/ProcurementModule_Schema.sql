-- =============================================
-- Procurement Module Schema
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Procurement schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Procurement')
BEGIN
    EXEC('CREATE SCHEMA Procurement')
END
GO

-- =============================================
-- 1. Vendors Table
-- =============================================
CREATE TABLE Procurement.Vendors (
    VendorID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    VendorCode NVARCHAR(50) NOT NULL,
    VendorName NVARCHAR(255) NOT NULL,
    ContactPerson NVARCHAR(255),
    PhoneNumber NVARCHAR(50),
    Email NVARCHAR(255),
    AddressLine1 NVARCHAR(255),
    AddressLine2 NVARCHAR(255),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Country NVARCHAR(100),
    PostalCode NVARCHAR(20),
    TaxNumber NVARCHAR(50), -- VAT/GST/NTN
    PaymentTerms NVARCHAR(100), -- Net 30, Net 60, etc.
    CreditLimit DECIMAL(18,2) DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_Vendors_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_Vendors_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT UQ_Vendors_VendorCode UNIQUE (CompanyID, VendorCode),
    CONSTRAINT UQ_Vendors_Email UNIQUE (TenantID, Email)
);

-- =============================================
-- 2. VendorBankAccounts Table
-- =============================================
CREATE TABLE Procurement.VendorBankAccounts (
    VendorBankAccountID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VendorID UNIQUEIDENTIFIER NOT NULL,
    BankName NVARCHAR(255) NOT NULL,
    AccountNumber NVARCHAR(50) NOT NULL,
    IBAN NVARCHAR(50),
    SWIFTCode NVARCHAR(20),
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    IsPrimary BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_VendorBankAccounts_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID)
);

-- =============================================
-- 3. PurchaseRequisitions Table
-- =============================================
CREATE TABLE Procurement.PurchaseRequisitions (
    PRID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    RequestedByUserID UNIQUEIDENTIFIER NOT NULL,
    DepartmentID UNIQUEIDENTIFIER, -- FK to Departments table (if exists)
    RequestDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RequiredByDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Submitted, Approved, Rejected, ConvertedToPO
    Remarks NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PurchaseRequisitions_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_PurchaseRequisitions_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT FK_PurchaseRequisitions_User FOREIGN KEY (RequestedByUserID) REFERENCES Core.Users(Id),
    CONSTRAINT CHK_PurchaseRequisitions_Status CHECK (Status IN ('Draft', 'Submitted', 'Approved', 'Rejected', 'ConvertedToPO'))
);

-- =============================================
-- 4. PurchaseRequisitionDetails Table
-- =============================================
CREATE TABLE Procurement.PurchaseRequisitionDetails (
    PRDetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PRID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    QuantityRequested DECIMAL(18,4) NOT NULL,
    UnitOfMeasureID UNIQUEIDENTIFIER NOT NULL,
    EstimatedUnitPrice DECIMAL(18,4) DEFAULT 0,
    Remarks NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PRDetails_PR FOREIGN KEY (PRID) REFERENCES Procurement.PurchaseRequisitions(PRID) ON DELETE CASCADE,
    CONSTRAINT FK_PRDetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    -- CONSTRAINT FK_PRDetails_UOM FOREIGN KEY (UnitOfMeasureID) REFERENCES Inventory.UnitOfMeasures(UnitOfMeasureID), -- TODO: Create UnitOfMeasures table
    CONSTRAINT CHK_PRDetails_Quantity CHECK (QuantityRequested > 0)
);

-- =============================================
-- 5. RequestForQuotation Table
-- =============================================
CREATE TABLE Procurement.RequestForQuotation (
    RFQID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    PRID UNIQUEIDENTIFIER, -- nullable, if RFQ generated from PR
    IssuedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DueDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open', -- Open, Closed, Awarded, Cancelled
    CreatedBy NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Constraints
    CONSTRAINT FK_RFQ_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_RFQ_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT FK_RFQ_PR FOREIGN KEY (PRID) REFERENCES Procurement.PurchaseRequisitions(PRID),
    CONSTRAINT CHK_RFQ_Status CHECK (Status IN ('Open', 'Closed', 'Awarded', 'Cancelled'))
);

-- =============================================
-- 6. RFQDetails Table
-- =============================================
CREATE TABLE Procurement.RFQDetails (
    RFQDetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RFQID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    QuantityRequested DECIMAL(18,4) NOT NULL,
    UnitOfMeasureID UNIQUEIDENTIFIER NOT NULL,
    Remarks NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_RFQDetails_RFQ FOREIGN KEY (RFQID) REFERENCES Procurement.RequestForQuotation(RFQID) ON DELETE CASCADE,
    CONSTRAINT FK_RFQDetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    -- CONSTRAINT FK_RFQDetails_UOM FOREIGN KEY (UnitOfMeasureID) REFERENCES Inventory.UnitOfMeasures(UnitOfMeasureID), -- TODO: Create UnitOfMeasures table
    CONSTRAINT CHK_RFQDetails_Quantity CHECK (QuantityRequested > 0)
);

-- =============================================
-- 7. RFQVendorResponses Table
-- =============================================
CREATE TABLE Procurement.RFQVendorResponses (
    RFQResponseID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RFQID UNIQUEIDENTIFIER NOT NULL,
    VendorID UNIQUEIDENTIFIER NOT NULL,
    ResponseDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TotalQuotedAmount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    PaymentTerms NVARCHAR(100),
    DeliveryTime INT, -- in days
    Status NVARCHAR(50) NOT NULL DEFAULT 'Submitted', -- Submitted, Shortlisted, Rejected, Accepted
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_RFQResponses_RFQ FOREIGN KEY (RFQID) REFERENCES Procurement.RequestForQuotation(RFQID) ON DELETE CASCADE,
    CONSTRAINT FK_RFQResponses_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID),
    CONSTRAINT CHK_RFQResponses_Status CHECK (Status IN ('Submitted', 'Shortlisted', 'Rejected', 'Accepted')),
    CONSTRAINT CHK_RFQResponses_Amount CHECK (TotalQuotedAmount >= 0)
);

-- =============================================
-- 8. RFQVendorResponseDetails Table
-- =============================================
CREATE TABLE Procurement.RFQVendorResponseDetails (
    ResponseDetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RFQResponseID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    QuotedUnitPrice DECIMAL(18,4) NOT NULL,
    Discount DECIMAL(5,2) DEFAULT 0, -- percentage
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    FinalUnitPrice DECIMAL(18,4) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_RFQResponseDetails_Response FOREIGN KEY (RFQResponseID) REFERENCES Procurement.RFQVendorResponses(RFQResponseID) ON DELETE CASCADE,
    CONSTRAINT FK_RFQResponseDetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    CONSTRAINT CHK_RFQResponseDetails_Price CHECK (QuotedUnitPrice >= 0),
    CONSTRAINT CHK_RFQResponseDetails_Discount CHECK (Discount >= 0 AND Discount <= 100),
    CONSTRAINT CHK_RFQResponseDetails_Tax CHECK (TaxAmount >= 0)
);

-- =============================================
-- 9. PurchaseOrders Table
-- =============================================
CREATE TABLE Procurement.PurchaseOrders (
    POID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    RFQID UNIQUEIDENTIFIER, -- nullable, if derived from RFQ
    VendorID UNIQUEIDENTIFIER NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpectedDeliveryDate DATETIME2 NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentTerms NVARCHAR(100),
    DeliveryTerms NVARCHAR(255),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Issued, PartiallyReceived, Completed, Cancelled
    CreatedBy NVARCHAR(255),
    ApprovedBy NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PO_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_PO_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT FK_PO_RFQ FOREIGN KEY (RFQID) REFERENCES Procurement.RequestForQuotation(RFQID),
    CONSTRAINT FK_PO_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID),
    CONSTRAINT CHK_PO_Status CHECK (Status IN ('Draft', 'Issued', 'PartiallyReceived', 'Completed', 'Cancelled')),
    CONSTRAINT CHK_PO_Amount CHECK (TotalAmount >= 0)
);

-- =============================================
-- 10. PurchaseOrderDetails Table
-- =============================================
CREATE TABLE Procurement.PurchaseOrderDetails (
    PODetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    POID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    QuantityOrdered DECIMAL(18,4) NOT NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    Discount DECIMAL(5,2) DEFAULT 0, -- percentage
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL,
    ReceivedQuantity DECIMAL(18,4) DEFAULT 0, -- updated on GRN
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PODetails_PO FOREIGN KEY (POID) REFERENCES Procurement.PurchaseOrders(POID) ON DELETE CASCADE,
    CONSTRAINT FK_PODetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    CONSTRAINT CHK_PODetails_Quantity CHECK (QuantityOrdered > 0),
    CONSTRAINT CHK_PODetails_Price CHECK (UnitPrice >= 0),
    CONSTRAINT CHK_PODetails_Discount CHECK (Discount >= 0 AND Discount <= 100),
    CONSTRAINT CHK_PODetails_Tax CHECK (TaxAmount >= 0),
    CONSTRAINT CHK_PODetails_NetAmount CHECK (NetAmount >= 0),
    CONSTRAINT CHK_PODetails_Received CHECK (ReceivedQuantity >= 0 AND ReceivedQuantity <= QuantityOrdered)
);

-- =============================================
-- 11. GoodsReceiptNote Table
-- =============================================
CREATE TABLE Procurement.GoodsReceiptNote (
    GRNID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    POID UNIQUEIDENTIFIER NOT NULL,
    ReceivedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    WarehouseID UNIQUEIDENTIFIER NOT NULL,
    ReceivedByUserID UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Verified, Posted
    Remarks NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_GRN_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_GRN_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT FK_GRN_PO FOREIGN KEY (POID) REFERENCES Procurement.PurchaseOrders(POID),
    CONSTRAINT FK_GRN_Warehouse FOREIGN KEY (WarehouseID) REFERENCES Inventory.Warehouses(Id),
    CONSTRAINT FK_GRN_User FOREIGN KEY (ReceivedByUserID) REFERENCES Core.Users(Id),
    CONSTRAINT CHK_GRN_Status CHECK (Status IN ('Draft', 'Verified', 'Posted'))
);

-- =============================================
-- 12. GoodsReceiptNoteDetails Table
-- =============================================
CREATE TABLE Procurement.GoodsReceiptNoteDetails (
    GRNDetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GRNID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    QuantityReceived DECIMAL(18,4) NOT NULL,
    BatchSerialID UNIQUEIDENTIFIER, -- nullable, if batch/serial tracked
    UnitPrice DECIMAL(18,4) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_GRNDetails_GRN FOREIGN KEY (GRNID) REFERENCES Procurement.GoodsReceiptNote(GRNID) ON DELETE CASCADE,
    CONSTRAINT FK_GRNDetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    CONSTRAINT FK_GRNDetails_BatchSerial FOREIGN KEY (BatchSerialID) REFERENCES Inventory.BatchSerialTracking(Id),
    CONSTRAINT CHK_GRNDetails_Quantity CHECK (QuantityReceived > 0),
    CONSTRAINT CHK_GRNDetails_Price CHECK (UnitPrice >= 0),
    CONSTRAINT CHK_GRNDetails_Tax CHECK (TaxAmount >= 0),
    CONSTRAINT CHK_GRNDetails_Total CHECK (TotalAmount >= 0)
);

-- =============================================
-- 13. PurchaseInvoices Table
-- =============================================
CREATE TABLE Procurement.PurchaseInvoices (
    InvoiceID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    POID UNIQUEIDENTIFIER, -- nullable, if directly invoiced
    VendorID UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber NVARCHAR(100) NOT NULL,
    InvoiceDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    TotalAmount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    NetPayable DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, PartiallyPaid, Cancelled
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PurchaseInvoices_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_PurchaseInvoices_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT FK_PurchaseInvoices_PO FOREIGN KEY (POID) REFERENCES Procurement.PurchaseOrders(POID),
    CONSTRAINT FK_PurchaseInvoices_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID),
    CONSTRAINT UQ_PurchaseInvoices_Number UNIQUE (TenantID, InvoiceNumber),
    CONSTRAINT CHK_PurchaseInvoices_Status CHECK (Status IN ('Pending', 'Paid', 'PartiallyPaid', 'Cancelled')),
    CONSTRAINT CHK_PurchaseInvoices_Amount CHECK (TotalAmount >= 0),
    CONSTRAINT CHK_PurchaseInvoices_Tax CHECK (TaxAmount >= 0),
    CONSTRAINT CHK_PurchaseInvoices_Net CHECK (NetPayable >= 0)
);

-- =============================================
-- 14. PurchaseInvoiceDetails Table
-- =============================================
CREATE TABLE Procurement.PurchaseInvoiceDetails (
    InvoiceDetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    InvoiceID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PurchaseInvoiceDetails_Invoice FOREIGN KEY (InvoiceID) REFERENCES Procurement.PurchaseInvoices(InvoiceID) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseInvoiceDetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    CONSTRAINT CHK_PurchaseInvoiceDetails_Quantity CHECK (Quantity > 0),
    CONSTRAINT CHK_PurchaseInvoiceDetails_Price CHECK (UnitPrice >= 0),
    CONSTRAINT CHK_PurchaseInvoiceDetails_Tax CHECK (TaxAmount >= 0),
    CONSTRAINT CHK_PurchaseInvoiceDetails_Net CHECK (NetAmount >= 0)
);

-- =============================================
-- Indexes for Performance
-- =============================================

-- Vendors indexes
CREATE INDEX IX_Vendors_TenantID_CompanyID ON Procurement.Vendors(TenantID, CompanyID);
CREATE INDEX IX_Vendors_VendorCode ON Procurement.Vendors(CompanyID, VendorCode);
CREATE INDEX IX_Vendors_IsActive ON Procurement.Vendors(IsActive);

-- Purchase Requisitions indexes
CREATE INDEX IX_PurchaseRequisitions_TenantID_CompanyID ON Procurement.PurchaseRequisitions(TenantID, CompanyID);
CREATE INDEX IX_PurchaseRequisitions_Status ON Procurement.PurchaseRequisitions(Status);
CREATE INDEX IX_PurchaseRequisitions_RequestedBy ON Procurement.PurchaseRequisitions(RequestedByUserID);

-- RFQ indexes
CREATE INDEX IX_RFQ_TenantID_CompanyID ON Procurement.RequestForQuotation(TenantID, CompanyID);
CREATE INDEX IX_RFQ_Status ON Procurement.RequestForQuotation(Status);
CREATE INDEX IX_RFQ_DueDate ON Procurement.RequestForQuotation(DueDate);

-- Purchase Orders indexes
CREATE INDEX IX_PurchaseOrders_TenantID_CompanyID ON Procurement.PurchaseOrders(TenantID, CompanyID);
CREATE INDEX IX_PurchaseOrders_VendorID ON Procurement.PurchaseOrders(VendorID);
CREATE INDEX IX_PurchaseOrders_Status ON Procurement.PurchaseOrders(Status);
CREATE INDEX IX_PurchaseOrders_OrderDate ON Procurement.PurchaseOrders(OrderDate);

-- GRN indexes
CREATE INDEX IX_GRN_TenantID_CompanyID ON Procurement.GoodsReceiptNote(TenantID, CompanyID);
CREATE INDEX IX_GRN_POID ON Procurement.GoodsReceiptNote(POID);
CREATE INDEX IX_GRN_Status ON Procurement.GoodsReceiptNote(Status);

-- Purchase Invoices indexes
CREATE INDEX IX_PurchaseInvoices_TenantID_CompanyID ON Procurement.PurchaseInvoices(TenantID, CompanyID);
CREATE INDEX IX_PurchaseInvoices_VendorID ON Procurement.PurchaseInvoices(VendorID);
CREATE INDEX IX_PurchaseInvoices_Status ON Procurement.PurchaseInvoices(Status);
CREATE INDEX IX_PurchaseInvoices_DueDate ON Procurement.PurchaseInvoices(DueDate);

-- =============================================
-- VENDOR GROUPS TABLE
-- =============================================
CREATE TABLE Procurement.VendorGroups (
    VendorGroupID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GroupName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_VendorGroups_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id)
);
GO

-- =============================================
-- VENDOR CONTACTS TABLE
-- =============================================
CREATE TABLE Procurement.VendorContacts (
    VendorContactID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VendorID UNIQUEIDENTIFIER NOT NULL,
    ContactPerson NVARCHAR(255) NOT NULL,
    JobTitle NVARCHAR(100),
    Department NVARCHAR(100),
    Email NVARCHAR(255),
    PhoneNumber NVARCHAR(50),
    MobileNumber NVARCHAR(50),
    IsPrimary BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_VendorContacts_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID) ON DELETE CASCADE,
    CONSTRAINT FK_VendorContacts_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id)
);
GO

-- =============================================
-- VENDOR RATINGS TABLE
-- =============================================
CREATE TABLE Procurement.VendorRatings (
    VendorRatingID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VendorID UNIQUEIDENTIFIER NOT NULL,
    RatingDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    QualityRating DECIMAL(3,2) NOT NULL, -- 1.00 to 5.00
    DeliveryRating DECIMAL(3,2) NOT NULL, -- 1.00 to 5.00
    ServiceRating DECIMAL(3,2) NOT NULL, -- 1.00 to 5.00
    PriceRating DECIMAL(3,2) NOT NULL, -- 1.00 to 5.00
    OverallRating DECIMAL(3,2) NOT NULL, -- 1.00 to 5.00
    Comments NVARCHAR(1000),
    RatedByUserID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_VendorRatings_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID) ON DELETE CASCADE,
    CONSTRAINT FK_VendorRatings_User FOREIGN KEY (RatedByUserID) REFERENCES Core.Users(Id),
    CONSTRAINT FK_VendorRatings_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT CHK_VendorRatings_Quality CHECK (QualityRating >= 1.00 AND QualityRating <= 5.00),
    CONSTRAINT CHK_VendorRatings_Delivery CHECK (DeliveryRating >= 1.00 AND DeliveryRating <= 5.00),
    CONSTRAINT CHK_VendorRatings_Service CHECK (ServiceRating >= 1.00 AND ServiceRating <= 5.00),
    CONSTRAINT CHK_VendorRatings_Price CHECK (PriceRating >= 1.00 AND PriceRating <= 5.00),
    CONSTRAINT CHK_VendorRatings_Overall CHECK (OverallRating >= 1.00 AND OverallRating <= 5.00)
);
GO

-- =============================================
-- CONTRACT AGREEMENTS TABLE
-- =============================================
CREATE TABLE Procurement.ContractAgreements (
    ContractID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VendorID UNIQUEIDENTIFIER NOT NULL,
    ContractNumber NVARCHAR(50) NOT NULL,
    ContractName NVARCHAR(255) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    ContractValue DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    ContractType NVARCHAR(50) NOT NULL, -- Fixed Price, Time & Material, Cost Plus
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Active, Expired, Terminated
    Terms NVARCHAR(MAX),
    Notes NVARCHAR(1000),
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_ContractAgreements_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID),
    CONSTRAINT FK_ContractAgreements_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT CHK_ContractAgreements_Status CHECK (Status IN ('Draft', 'Active', 'Expired', 'Terminated'))
);
GO

-- =============================================
-- PURCHASE RETURN ORDERS TABLE
-- =============================================
CREATE TABLE Procurement.PurchaseReturnOrders (
    ReturnOrderID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyID UNIQUEIDENTIFIER NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    VendorID UNIQUEIDENTIFIER NOT NULL,
    POID UNIQUEIDENTIFIER, -- Reference to original PO
    ReturnNumber NVARCHAR(50) NOT NULL,
    ReturnDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Reason NVARCHAR(500) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Submitted, Approved, Processed
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ApprovedByUserID UNIQUEIDENTIFIER,
    ApprovedAt DATETIME2,
    Notes NVARCHAR(1000),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PurchaseReturnOrders_Company FOREIGN KEY (CompanyID) REFERENCES Core.Companies(Id),
    CONSTRAINT FK_PurchaseReturnOrders_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT FK_PurchaseReturnOrders_Vendor FOREIGN KEY (VendorID) REFERENCES Procurement.Vendors(VendorID),
    CONSTRAINT FK_PurchaseReturnOrders_PO FOREIGN KEY (POID) REFERENCES Procurement.PurchaseOrders(POID),
    CONSTRAINT FK_PurchaseReturnOrders_User FOREIGN KEY (ApprovedByUserID) REFERENCES Core.Users(Id),
    CONSTRAINT CHK_PurchaseReturnOrders_Status CHECK (Status IN ('Draft', 'Submitted', 'Approved', 'Processed'))
);
GO

-- =============================================
-- PURCHASE RETURN ORDER DETAILS TABLE
-- =============================================
CREATE TABLE Procurement.PurchaseReturnOrderDetails (
    ReturnDetailID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReturnOrderID UNIQUEIDENTIFIER NOT NULL,
    ItemID UNIQUEIDENTIFIER NOT NULL,
    QuantityReturned DECIMAL(18,4) NOT NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(500),
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_PurchaseReturnOrderDetails_ReturnOrder FOREIGN KEY (ReturnOrderID) REFERENCES Procurement.PurchaseReturnOrders(ReturnOrderID) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseReturnOrderDetails_Item FOREIGN KEY (ItemID) REFERENCES Inventory.Items(Id),
    CONSTRAINT FK_PurchaseReturnOrderDetails_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT CHK_PurchaseReturnOrderDetails_Quantity CHECK (QuantityReturned > 0),
    CONSTRAINT CHK_PurchaseReturnOrderDetails_Price CHECK (UnitPrice >= 0)
);
GO

-- =============================================
-- LANDED COST TABLE
-- =============================================
CREATE TABLE Procurement.LandedCost (
    LandedCostID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    POID UNIQUEIDENTIFIER NOT NULL,
    CostType NVARCHAR(50) NOT NULL, -- Freight, Customs, Insurance, Handling, Other
    CostDescription NVARCHAR(255) NOT NULL,
    CostAmount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    AllocationMethod NVARCHAR(50) NOT NULL, -- Equal, Weight, Value, Manual
    IsAllocated BIT DEFAULT 0,
    AllocatedAt DATETIME2,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_LandedCost_PO FOREIGN KEY (POID) REFERENCES Procurement.PurchaseOrders(POID),
    CONSTRAINT FK_LandedCost_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT CHK_LandedCost_Amount CHECK (CostAmount >= 0),
    CONSTRAINT CHK_LandedCost_Type CHECK (CostType IN ('Freight', 'Customs', 'Insurance', 'Handling', 'Other')),
    CONSTRAINT CHK_LandedCost_Allocation CHECK (AllocationMethod IN ('Equal', 'Weight', 'Value', 'Manual'))
);
GO

-- =============================================
-- LANDED COST ALLOCATIONS TABLE
-- =============================================
CREATE TABLE Procurement.LandedCostAllocations (
    AllocationID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LandedCostID UNIQUEIDENTIFIER NOT NULL,
    PODetailID UNIQUEIDENTIFIER NOT NULL,
    AllocatedAmount DECIMAL(18,2) NOT NULL,
    AllocationPercentage DECIMAL(5,2) NOT NULL,
    TenantID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    
    -- Constraints
    CONSTRAINT FK_LandedCostAllocations_LandedCost FOREIGN KEY (LandedCostID) REFERENCES Procurement.LandedCost(LandedCostID) ON DELETE CASCADE,
    CONSTRAINT FK_LandedCostAllocations_PODetail FOREIGN KEY (PODetailID) REFERENCES Procurement.PurchaseOrderDetails(PODetailID),
    CONSTRAINT FK_LandedCostAllocations_Tenant FOREIGN KEY (TenantID) REFERENCES Core.Tenants(Id),
    CONSTRAINT CHK_LandedCostAllocations_Amount CHECK (AllocatedAmount >= 0),
    CONSTRAINT CHK_LandedCostAllocations_Percentage CHECK (AllocationPercentage >= 0 AND AllocationPercentage <= 100)
);
GO

-- =============================================
-- ADDITIONAL INDEXES FOR PROCUREMENT MODULE
-- =============================================
CREATE INDEX IX_VendorGroups_TenantID ON Procurement.VendorGroups(TenantID);
CREATE INDEX IX_VendorContacts_VendorID ON Procurement.VendorContacts(VendorID);
CREATE INDEX IX_VendorRatings_VendorID ON Procurement.VendorRatings(VendorID);
CREATE INDEX IX_VendorRatings_RatingDate ON Procurement.VendorRatings(RatingDate);
CREATE INDEX IX_ContractAgreements_VendorID ON Procurement.ContractAgreements(VendorID);
CREATE INDEX IX_ContractAgreements_Status ON Procurement.ContractAgreements(Status);
CREATE INDEX IX_PurchaseReturnOrders_VendorID ON Procurement.PurchaseReturnOrders(VendorID);
CREATE INDEX IX_PurchaseReturnOrders_POID ON Procurement.PurchaseReturnOrders(POID);
CREATE INDEX IX_PurchaseReturnOrderDetails_ReturnOrderID ON Procurement.PurchaseReturnOrderDetails(ReturnOrderID);
CREATE INDEX IX_LandedCost_POID ON Procurement.LandedCost(POID);
CREATE INDEX IX_LandedCostAllocations_LandedCostID ON Procurement.LandedCostAllocations(LandedCostID);
CREATE INDEX IX_LandedCostAllocations_PODetailID ON Procurement.LandedCostAllocations(PODetailID);

-- =============================================
-- Audit Trail Triggers
-- =============================================

-- Note: Audit triggers will be created separately using the audit trigger creation procedure
-- when it becomes available. For now, tables are created with audit-ready structure.

PRINT 'Procurement Module Schema created successfully!'