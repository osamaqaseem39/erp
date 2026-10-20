-- Customer depth: Procurement — vendor party fields, landed cost voucher, vendor contract
IF COL_LENGTH('dbo.Vendors', 'LocalName') IS NULL AND OBJECT_ID('dbo.Vendors', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Vendors ADD LocalName NVARCHAR(255) NULL;
    ALTER TABLE dbo.Vendors ADD Phone2 NVARCHAR(50) NULL;
    ALTER TABLE dbo.Vendors ADD Cell NVARCHAR(50) NULL;
    ALTER TABLE dbo.Vendors ADD Cell2 NVARCHAR(50) NULL;
    ALTER TABLE dbo.Vendors ADD Fax NVARCHAR(50) NULL;
    ALTER TABLE dbo.Vendors ADD CityId UNIQUEIDENTIFIER NULL;
    ALTER TABLE dbo.Vendors ADD PaymentTermId UNIQUEIDENTIFIER NULL;
    ALTER TABLE dbo.Vendors ADD PayableAccountId UNIQUEIDENTIFIER NULL;
    ALTER TABLE dbo.Vendors ADD Rating INT NULL;
END
GO

IF OBJECT_ID(N'[dbo].[LandedCostVouchers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LandedCostVouchers] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [VoucherNumber] NVARCHAR(50) NOT NULL,
        [VendorId] UNIQUEIDENTIFIER NULL,
        [VoucherDate] DATETIME2 NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [TotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_LandedCostVouchers_Total] DEFAULT (0),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_LandedCostVouchers_Status] DEFAULT (N'Draft'),
        [PostedAt] DATETIME2 NULL,
        [PostedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_LandedCostVouchers_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_LandedCostVouchers_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_LandedCostVouchers_Tenant_Number] ON [dbo].[LandedCostVouchers]([TenantId], [VoucherNumber]);
END
GO

IF OBJECT_ID(N'[dbo].[LandedCostVoucherLines]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LandedCostVoucherLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [LandedCostVoucherId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [GrnDetailId] UNIQUEIDENTIFIER NULL,
        [PurchaseInvoiceDetailId] UNIQUEIDENTIFIER NULL,
        [AllocatedAmount] DECIMAL(18,4) NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_LandedCostVoucherLines_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_LandedCostVoucherLines_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_LandedCostVoucherLines_Header]
            FOREIGN KEY ([LandedCostVoucherId]) REFERENCES [dbo].[LandedCostVouchers]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_LandedCostVoucherLines_Header] ON [dbo].[LandedCostVoucherLines]([LandedCostVoucherId]);
END
GO

IF OBJECT_ID(N'[dbo].[VendorContracts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[VendorContracts] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [VendorId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [UnitPrice] DECIMAL(18,4) NOT NULL,
        [ValidFrom] DATETIME2 NOT NULL,
        [ValidTo] DATETIME2 NULL,
        [PaymentTermId] UNIQUEIDENTIFIER NULL,
        [ContractNumber] NVARCHAR(50) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_VendorContracts_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_VendorContracts_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX [IX_VendorContracts_Tenant_Vendor] ON [dbo].[VendorContracts]([TenantId], [VendorId]);
    CREATE INDEX [IX_VendorContracts_ItemId] ON [dbo].[VendorContracts]([ItemId]);
END
GO
