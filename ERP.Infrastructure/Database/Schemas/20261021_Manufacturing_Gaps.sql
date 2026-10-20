-- Manufacturing light-MFG+ gaps: SO links, warehouse, operations, scrap reasons, QC, capacity, BOM/routing defaults
-- Idempotent IF NOT EXISTS / COL_LENGTH checks

IF COL_LENGTH(N'dbo.ProductionOrders', N'SalesOrderId') IS NULL
    ALTER TABLE [dbo].[ProductionOrders] ADD [SalesOrderId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProductionOrders', N'SalesOrderLineId') IS NULL
    ALTER TABLE [dbo].[ProductionOrders] ADD [SalesOrderLineId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProductionOrders', N'WarehouseId') IS NULL
    ALTER TABLE [dbo].[ProductionOrders] ADD [WarehouseId] UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductionOrders_SalesOrderLine' AND object_id = OBJECT_ID(N'dbo.ProductionOrders'))
    CREATE INDEX IX_ProductionOrders_SalesOrderLine ON [dbo].[ProductionOrders]([TenantId], [SalesOrderLineId]) WHERE [SalesOrderLineId] IS NOT NULL;
GO

IF COL_LENGTH(N'dbo.WorkCenters', N'WeeklyAvailableHours') IS NULL
    ALTER TABLE [dbo].[WorkCenters] ADD [WeeklyAvailableHours] DECIMAL(18,4) NOT NULL CONSTRAINT DF_WorkCenter_WeeklyHours DEFAULT (40);
GO

IF COL_LENGTH(N'dbo.BillOfMaterials', N'IsDefault') IS NULL
    ALTER TABLE [dbo].[BillOfMaterials] ADD [IsDefault] BIT NOT NULL CONSTRAINT DF_BOM_IsDefault DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.BillOfMaterials', N'EffectiveFrom') IS NULL
    ALTER TABLE [dbo].[BillOfMaterials] ADD [EffectiveFrom] DATETIME2 NULL;
GO
IF COL_LENGTH(N'dbo.BillOfMaterials', N'EffectiveTo') IS NULL
    ALTER TABLE [dbo].[BillOfMaterials] ADD [EffectiveTo] DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.BillOfMaterialLines', N'AlternateGroup') IS NULL
    ALTER TABLE [dbo].[BillOfMaterialLines] ADD [AlternateGroup] NVARCHAR(50) NULL;
GO

IF COL_LENGTH(N'dbo.Routings', N'IsDefault') IS NULL
    ALTER TABLE [dbo].[Routings] ADD [IsDefault] BIT NOT NULL CONSTRAINT DF_Routing_IsDefault DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ScrapReasons' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[ScrapReasons] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_ScrapReason_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_ScrapReason_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX UX_ScrapReasons_Tenant_Code ON [dbo].[ScrapReasons]([TenantId], [Code]);
END
GO

IF COL_LENGTH(N'dbo.ProductionScraps', N'ScrapReasonId') IS NULL
    ALTER TABLE [dbo].[ProductionScraps] ADD [ScrapReasonId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProductionScraps', N'WarehouseId') IS NULL
    ALTER TABLE [dbo].[ProductionScraps] ADD [WarehouseId] UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ProductionOrderOperations' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[ProductionOrderOperations] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ProductionOrderId] UNIQUEIDENTIFIER NOT NULL,
        [RoutingStepId] UNIQUEIDENTIFIER NULL,
        [StepNumber] INT NOT NULL,
        [WorkCenterId] UNIQUEIDENTIFIER NULL,
        [OperationName] NVARCHAR(200) NOT NULL,
        [StandardTimeMinutes] DECIMAL(18,4) NOT NULL CONSTRAINT DF_POOp_StdTime DEFAULT (0),
        [ActualStart] DATETIME2 NULL,
        [ActualEnd] DATETIME2 NULL,
        [ActualTimeMinutes] DECIMAL(18,4) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_POOp_Status DEFAULT (N'Pending'),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_POOp_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_POOp_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_POOp_ProductionOrder FOREIGN KEY ([ProductionOrderId])
            REFERENCES [dbo].[ProductionOrders]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_POOp_Order ON [dbo].[ProductionOrderOperations]([ProductionOrderId]);
END
GO

IF COL_LENGTH(N'dbo.ProductionRuns', N'WorkCenterId') IS NULL
    ALTER TABLE [dbo].[ProductionRuns] ADD [WorkCenterId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH(N'dbo.ProductionRuns', N'ProductionOrderOperationId') IS NULL
    ALTER TABLE [dbo].[ProductionRuns] ADD [ProductionOrderOperationId] UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'QualityChecks' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[QualityChecks] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [ProductionOrderId] UNIQUEIDENTIFIER NOT NULL,
        [CheckNumber] NVARCHAR(50) NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_QC_Status DEFAULT (N'Pending'),
        [CheckedAt] DATETIME2 NULL,
        [Notes] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_QC_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_QC_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_QC_ProductionOrder FOREIGN KEY ([ProductionOrderId])
            REFERENCES [dbo].[ProductionOrders]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_QC_Order ON [dbo].[QualityChecks]([ProductionOrderId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'QualityCheckLines' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE [dbo].[QualityCheckLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [QualityCheckId] UNIQUEIDENTIFIER NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [QuantityInspected] DECIMAL(18,4) NOT NULL CONSTRAINT DF_QCL_Inspected DEFAULT (0),
        [QuantityPassed] DECIMAL(18,4) NOT NULL CONSTRAINT DF_QCL_Passed DEFAULT (0),
        [QuantityFailed] DECIMAL(18,4) NOT NULL CONSTRAINT DF_QCL_Failed DEFAULT (0),
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT DF_QCL_IsActive DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_QCL_CreatedAt DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT FK_QCL_QualityCheck FOREIGN KEY ([QualityCheckId])
            REFERENCES [dbo].[QualityChecks]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX IX_QCL_Check ON [dbo].[QualityCheckLines]([QualityCheckId]);
END
GO
