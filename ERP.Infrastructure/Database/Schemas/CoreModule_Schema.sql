-- =============================================
-- CORE MODULE DATABASE SCHEMA
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Core Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Core')
BEGIN
    EXEC('CREATE SCHEMA [Core]')
END
GO

-- =============================================
-- TENANTS TABLE
-- =============================================
CREATE TABLE [Core].[Tenants] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [TenantName] NVARCHAR(200) NOT NULL,
    [Domain] NVARCHAR(100) NOT NULL UNIQUE,
    [Industry] NVARCHAR(100),
    [Country] NVARCHAR(100),
    [Status] NVARCHAR(50) DEFAULT 'Active',
    [DatabaseName] NVARCHAR(100) NOT NULL,
    [ConnectionString] NVARCHAR(MAX) NOT NULL,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- PLANS TABLE
-- =============================================
CREATE TABLE [Core].[Plans] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PlanName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [MonthlyPrice] DECIMAL(18,2) NOT NULL,
    [YearlyPrice] DECIMAL(18,2) NOT NULL,
    [Currency] NVARCHAR(3) DEFAULT 'USD',
    [MaxUsers] INT NOT NULL,
    [MaxStorage] INT NOT NULL,
    [FeaturesJSON] NVARCHAR(MAX),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- SUBSCRIPTIONS TABLE
-- =============================================
CREATE TABLE [Core].[Subscriptions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [PlanId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [RenewalDate] DATETIME2 NOT NULL,
    [PaymentStatus] NVARCHAR(50) DEFAULT 'Pending',
    [IsAutoRenew] BIT DEFAULT 1,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id]),
    FOREIGN KEY ([PlanId]) REFERENCES [Core].[Plans]([Id])
);
GO

-- =============================================
-- COMPANIES TABLE
-- =============================================
CREATE TABLE [Core].[Companies] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CompanyName] NVARCHAR(200) NOT NULL,
    [LegalName] NVARCHAR(200),
    [RegistrationNumber] NVARCHAR(50),
    [TaxNumber] NVARCHAR(50),
    [AddressLine1] NVARCHAR(200),
    [AddressLine2] NVARCHAR(200),
    [City] NVARCHAR(100),
    [State] NVARCHAR(100),
    [Country] NVARCHAR(100),
    [ZipCode] NVARCHAR(20),
    [Phone] NVARCHAR(20),
    [Email] NVARCHAR(200),
    [Website] NVARCHAR(200),
    [LogoUrl] NVARCHAR(500),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id])
);
GO

-- =============================================
-- USERS TABLE
-- =============================================
CREATE TABLE [Core].[Users] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL,
    [Salutation] NVARCHAR(10) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [FullName] NVARCHAR(200) NOT NULL,
    [Gender] NVARCHAR(10) NOT NULL,
    [DateOfBirth] DATETIME2,
    [JobTitle] NVARCHAR(100) NOT NULL,
    [Department] NVARCHAR(100) NOT NULL,
    [Nationality] NVARCHAR(100) NOT NULL,
    [PrimaryEmail] NVARCHAR(200) NOT NULL,
    [SecondaryEmail] NVARCHAR(200) NOT NULL,
    [PrimaryPhone] NVARCHAR(20) NOT NULL,
    [SecondaryPhone] NVARCHAR(20) NOT NULL,
    [MobilePhone] NVARCHAR(20) NOT NULL,
    [Fax] NVARCHAR(20) NOT NULL,
    [LinkedIn] NVARCHAR(200) NOT NULL,
    [Twitter] NVARCHAR(200) NOT NULL,
    [Facebook] NVARCHAR(200) NOT NULL,
    [Instagram] NVARCHAR(200) NOT NULL,
    [PreferredCommunicationMethod] NVARCHAR(20) NOT NULL,
    [LanguagePreference] NVARCHAR(10) NOT NULL,
    [TimeZone] NVARCHAR(50) NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [PasswordSalt] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [LastLoginDate] DATETIME2,
    [IsActive] BIT NOT NULL,
    [IsLocked] BIT NOT NULL,
    [FailedLoginAttempts] INT NOT NULL,
    [LockedUntil] DATETIME2,
    [EmailVerificationToken] NVARCHAR(MAX),
    [PasswordResetToken] NVARCHAR(MAX),
    [PasswordResetExpires] DATETIME2,
    [RefreshToken] NVARCHAR(MAX),
    [RefreshTokenExpires] DATETIME2,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsTenantOwner] BIT NOT NULL DEFAULT 0,
    FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id]),
    FOREIGN KEY ([CompanyId]) REFERENCES [Core].[Companies]([Id])
);
GO

-- =============================================
-- ROLES TABLE
-- =============================================
CREATE TABLE [Core].[Roles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [RoleName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL,
    [IsSystemRole] BIT NOT NULL,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id])
);
GO

-- =============================================
-- USER ROLES TABLE
-- =============================================
CREATE TABLE [Core].[UserRoles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedDate] DATETIME2 DEFAULT GETUTCDATE(),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([UserId]) REFERENCES [Core].[Users]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([RoleId]) REFERENCES [Core].[Roles]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- PERMISSIONS TABLE
-- =============================================
CREATE TABLE [Core].[Permissions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ModuleName] NVARCHAR(100) NOT NULL,
    [ActionName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- ROLE PERMISSIONS TABLE
-- =============================================
CREATE TABLE [Core].[RolePermissions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [PermissionId] UNIQUEIDENTIFIER NOT NULL,
    [IsAllowed] BIT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([RoleId]) REFERENCES [Core].[Roles]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([PermissionId]) REFERENCES [Core].[Permissions]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- AUDIT LOGS TABLE
-- =============================================
CREATE TABLE [Core].[AuditLogs] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Action] NVARCHAR(100) NOT NULL,
    [EntityName] NVARCHAR(100) NOT NULL,
    [EntityId] NVARCHAR(50) NOT NULL,
    [OldValues] NVARCHAR(4000) NOT NULL,
    [NewValues] NVARCHAR(4000) NOT NULL,
    [IpAddress] NVARCHAR(50) NOT NULL,
    [UserAgent] NVARCHAR(500) NOT NULL,
    [Timestamp] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([UserId]) REFERENCES [Core].[Users]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- SETTINGS TABLE
-- =============================================
CREATE TABLE [Core].[Settings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [SettingKey] NVARCHAR(100) NOT NULL,
    [SettingValue] NVARCHAR(1000) NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- INDEXES FOR CORE MODULE
-- =============================================
CREATE INDEX IX_Tenants_Domain ON [Core].[Tenants]([Domain]);
CREATE INDEX IX_Tenants_Status ON [Core].[Tenants]([Status]);
CREATE INDEX IX_Users_TenantId ON [Core].[Users]([TenantId]);
CREATE INDEX IX_Users_PrimaryEmail ON [Core].[Users]([PrimaryEmail]);
CREATE INDEX IX_Users_CompanyId ON [Core].[Users]([CompanyId]);
CREATE INDEX IX_Roles_TenantId ON [Core].[Roles]([TenantId]);
CREATE INDEX IX_UserRoles_UserId ON [Core].[UserRoles]([UserId]);
CREATE INDEX IX_UserRoles_RoleId ON [Core].[UserRoles]([RoleId]);
CREATE INDEX IX_AuditLogs_TenantId ON [Core].[AuditLogs]([TenantId]);
CREATE INDEX IX_AuditLogs_UserId ON [Core].[AuditLogs]([UserId]);
CREATE INDEX IX_AuditLogs_Entity ON [Core].[AuditLogs]([EntityName], [EntityId]);
CREATE INDEX IX_Settings_TenantId ON [Core].[Settings]([TenantId]);
CREATE INDEX IX_Settings_Key ON [Core].[Settings]([SettingKey]);

-- =============================================
-- CURRENCIES TABLE (Global Master Data)
-- =============================================
CREATE TABLE [Core].[Currencies] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CurrencyCode] NVARCHAR(10) NOT NULL,
    [CurrencyName] NVARCHAR(100) NOT NULL,
    [Symbol] NVARCHAR(10),
    [IsBaseCurrency] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- EXCHANGE RATES TABLE
-- =============================================
CREATE TABLE [Core].[ExchangeRates] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [FromCurrencyId] UNIQUEIDENTIFIER NOT NULL,
    [ToCurrencyId] UNIQUEIDENTIFIER NOT NULL,
    [ExchangeRate] DECIMAL(18,6) NOT NULL,
    [RateDate] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([FromCurrencyId]) REFERENCES [Core].[Currencies]([Id]),
    FOREIGN KEY ([ToCurrencyId]) REFERENCES [Core].[Currencies]([Id])
);
GO

-- =============================================
-- COUNTRIES TABLE
-- =============================================
CREATE TABLE [Core].[Countries] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CountryCode] NVARCHAR(3) NOT NULL,
    [CountryName] NVARCHAR(100) NOT NULL,
    [ISOCode] NVARCHAR(2) NOT NULL,
    [PhoneCode] NVARCHAR(10),
    [CurrencyId] UNIQUEIDENTIFIER,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([CurrencyId]) REFERENCES [Core].[Currencies]([Id])
);
GO

-- =============================================
-- STATES/REGIONS TABLE
-- =============================================
CREATE TABLE [Core].[States] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CountryId] UNIQUEIDENTIFIER NOT NULL,
    [StateCode] NVARCHAR(10) NOT NULL,
    [StateName] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([CountryId]) REFERENCES [Core].[Countries]([Id])
);
GO

-- =============================================
-- CITIES TABLE
-- =============================================
CREATE TABLE [Core].[Cities] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [StateId] UNIQUEIDENTIFIER NOT NULL,
    [CityName] NVARCHAR(100) NOT NULL,
    [PostalCode] NVARCHAR(20),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([StateId]) REFERENCES [Core].[States]([Id])
);
GO

-- =============================================
-- ADDRESS BOOK TABLE
-- =============================================
CREATE TABLE [Core].[AddressBook] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EntityType] NVARCHAR(50) NOT NULL, -- Customer, Vendor, Employee, Warehouse
    [EntityId] UNIQUEIDENTIFIER NOT NULL,
    [AddressType] NVARCHAR(50) NOT NULL, -- Billing, Shipping, Office, Home
    [AddressLine1] NVARCHAR(200) NOT NULL,
    [AddressLine2] NVARCHAR(200),
    [CityId] UNIQUEIDENTIFIER,
    [StateId] UNIQUEIDENTIFIER,
    [CountryId] UNIQUEIDENTIFIER,
    [PostalCode] NVARCHAR(20),
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([CityId]) REFERENCES [Core].[Cities]([Id]),
    FOREIGN KEY ([StateId]) REFERENCES [Core].[States]([Id]),
    FOREIGN KEY ([CountryId]) REFERENCES [Core].[Countries]([Id])
);
GO

-- =============================================
-- DOCUMENT TYPES TABLE
-- =============================================
CREATE TABLE [Core].[DocumentTypes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [DocumentType] NVARCHAR(50) NOT NULL,
    [Module] NVARCHAR(50) NOT NULL,
    [Prefix] NVARCHAR(10) NOT NULL,
    [Suffix] NVARCHAR(10),
    [NumberLength] INT NOT NULL DEFAULT 6,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- NUMBER SERIES TABLE
-- =============================================
CREATE TABLE [Core].[NumberSeries] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [DocumentTypeId] UNIQUEIDENTIFIER NOT NULL,
    [SeriesName] NVARCHAR(100) NOT NULL,
    [CurrentNumber] INT NOT NULL DEFAULT 0,
    [StartNumber] INT NOT NULL DEFAULT 1,
    [EndNumber] INT,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    FOREIGN KEY ([DocumentTypeId]) REFERENCES [Core].[DocumentTypes]([Id])
);
GO

-- =============================================
-- ADDITIONAL INDEXES FOR CORE MODULE
-- =============================================
CREATE INDEX IX_Currencies_TenantId ON [Core].[Currencies]([TenantId]);
CREATE INDEX IX_Currencies_CurrencyCode ON [Core].[Currencies]([CurrencyCode]);
CREATE INDEX IX_ExchangeRates_TenantId ON [Core].[ExchangeRates]([TenantId]);
CREATE INDEX IX_ExchangeRates_FromCurrency ON [Core].[ExchangeRates]([FromCurrencyId]);
CREATE INDEX IX_ExchangeRates_ToCurrency ON [Core].[ExchangeRates]([ToCurrencyId]);
CREATE INDEX IX_ExchangeRates_RateDate ON [Core].[ExchangeRates]([RateDate]);
CREATE INDEX IX_Countries_TenantId ON [Core].[Countries]([TenantId]);
CREATE INDEX IX_Countries_CountryCode ON [Core].[Countries]([CountryCode]);
CREATE INDEX IX_States_TenantId ON [Core].[States]([TenantId]);
CREATE INDEX IX_States_CountryId ON [Core].[States]([CountryId]);
CREATE INDEX IX_Cities_TenantId ON [Core].[Cities]([TenantId]);
CREATE INDEX IX_Cities_StateId ON [Core].[Cities]([StateId]);
CREATE INDEX IX_AddressBook_TenantId ON [Core].[AddressBook]([TenantId]);
CREATE INDEX IX_AddressBook_Entity ON [Core].[AddressBook]([EntityType], [EntityId]);
CREATE INDEX IX_DocumentTypes_TenantId ON [Core].[DocumentTypes]([TenantId]);
CREATE INDEX IX_DocumentTypes_Module ON [Core].[DocumentTypes]([Module]);
CREATE INDEX IX_NumberSeries_TenantId ON [Core].[NumberSeries]([TenantId]);
CREATE INDEX IX_NumberSeries_DocumentType ON [Core].[NumberSeries]([DocumentTypeId]);

PRINT 'Core Module schema created successfully!'