-- Customer depth: Core geo + branch + payment terms
IF SCHEMA_ID('Core') IS NULL EXEC('CREATE SCHEMA Core');
GO

IF OBJECT_ID(N'[Core].[Countries]', N'U') IS NULL
BEGIN
    CREATE TABLE [Core].[Countries] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(10) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Countries_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_Countries_Tenant_Code] ON [Core].[Countries]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[Core].[Cities]', N'U') IS NULL
BEGIN
    CREATE TABLE [Core].[Cities] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [CountryId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Cities_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_Cities_Countries] FOREIGN KEY ([CountryId]) REFERENCES [Core].[Countries]([Id])
    );
    CREATE INDEX [IX_Cities_Tenant_Country] ON [Core].[Cities]([TenantId], [CountryId]);
END
GO

IF OBJECT_ID(N'[Core].[Branches]', N'U') IS NULL
BEGIN
    CREATE TABLE [Core].[Branches] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Address] NVARCHAR(500) NULL,
        [CityId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Branches_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_Branches_Tenant_Code] ON [Core].[Branches]([TenantId], [Code]) WHERE [IsActive] = 1;
END
GO

IF OBJECT_ID(N'[Core].[PaymentTerms]', N'U') IS NULL
BEGIN
    CREATE TABLE [Core].[PaymentTerms] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Days] INT NOT NULL CONSTRAINT [DF_PaymentTerms_Days] DEFAULT (0),
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_PaymentTerms_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    CREATE UNIQUE INDEX [IX_PaymentTerms_Tenant_Name] ON [Core].[PaymentTerms]([TenantId], [Name]) WHERE [IsActive] = 1;
END
GO
