-- Manufacturing BOM header/lines + optional ProductionOrders.BomId
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'BillOfMaterials' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[BillOfMaterials] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [BomNumber] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [FinishedGoodItemId] UNIQUEIDENTIFIER NOT NULL,
        [Version] NVARCHAR(20) NOT NULL CONSTRAINT DF_BOM_Version DEFAULT (N'1'),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_BOM_Status DEFAULT (N'Draft'),
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_BOM_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_BOM_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );

    CREATE UNIQUE INDEX UX_BillOfMaterials_Tenant_BomNumber
        ON [dbo].[BillOfMaterials]([TenantId], [BomNumber]);
    CREATE INDEX IX_BillOfMaterials_Tenant_FG
        ON [dbo].[BillOfMaterials]([TenantId], [FinishedGoodItemId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'BillOfMaterialLines' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[BillOfMaterialLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [BillOfMaterialId] UNIQUEIDENTIFIER NOT NULL,
        [LineNumber] INT NOT NULL,
        [ComponentItemId] UNIQUEIDENTIFIER NOT NULL,
        [QuantityPer] DECIMAL(18,4) NOT NULL,
        [ScrapPercent] DECIMAL(9,4) NOT NULL CONSTRAINT DF_BOMLine_Scrap DEFAULT (0),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_BOMLine_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_BOMLine_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_BOMLines_BOM FOREIGN KEY ([BillOfMaterialId])
            REFERENCES [dbo].[BillOfMaterials]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX IX_BOMLines_BOM ON [dbo].[BillOfMaterialLines]([BillOfMaterialId]);
END
GO

IF COL_LENGTH(N'dbo.ProductionOrders', N'BomId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ProductionOrders]
        ADD [BomId] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductionOrders_BOM')
   AND COL_LENGTH(N'dbo.ProductionOrders', N'BomId') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.tables WHERE name = N'BillOfMaterials')
BEGIN
    ALTER TABLE [dbo].[ProductionOrders] WITH NOCHECK
        ADD CONSTRAINT FK_ProductionOrders_BOM
            FOREIGN KEY ([BomId]) REFERENCES [dbo].[BillOfMaterials]([Id]) ON DELETE NO ACTION;
END
GO
