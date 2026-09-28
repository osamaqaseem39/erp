-- =============================================
-- Core Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Core Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Core')
BEGIN
    EXEC('CREATE SCHEMA [Core]')
END
GO

-- =============================================
-- CURRENCY MANAGEMENT PROCEDURES
-- =============================================

-- Get All Currencies
CREATE OR ALTER PROCEDURE [Core].sp_GetAllCurrencies
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        CurrencyCode,
        CurrencyName,
        Symbol,
        IsBaseCurrency,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM [Core].Currencies
    WHERE TenantId = @TenantID
    AND IsActive = 1
    ORDER BY CurrencyCode;
END
GO

-- Get Currency by Code
CREATE OR ALTER PROCEDURE [Core].sp_GetCurrencyByCode
    @CurrencyCode NVARCHAR(10),
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        CurrencyCode,
        CurrencyName,
        Symbol,
        IsBaseCurrency,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM [Core].Currencies
    WHERE CurrencyCode = @CurrencyCode
    AND TenantId = @TenantID
    AND IsActive = 1;
END
GO

-- Create Currency
CREATE OR ALTER PROCEDURE [Core].sp_CreateCurrency
    @Id UNIQUEIDENTIFIER,
    @CurrencyCode NVARCHAR(10),
    @CurrencyName NVARCHAR(100),
    @Symbol NVARCHAR(10) = NULL,
    @IsBaseCurrency BIT = 0,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].Currencies (
        Id, CurrencyCode, CurrencyName, Symbol, IsBaseCurrency, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CurrencyCode, @CurrencyName, @Symbol, @IsBaseCurrency, 1, @TenantID, @CreatedBy
    );
END
GO

-- Update Currency
CREATE OR ALTER PROCEDURE [Core].sp_UpdateCurrency
    @Id UNIQUEIDENTIFIER,
    @CurrencyCode NVARCHAR(10),
    @CurrencyName NVARCHAR(100),
    @Symbol NVARCHAR(10) = NULL,
    @IsBaseCurrency BIT = 0,
    @IsActive BIT = 1,
    @TenantID UNIQUEIDENTIFIER,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [Core].Currencies
    SET 
        CurrencyCode = @CurrencyCode,
        CurrencyName = @CurrencyName,
        Symbol = @Symbol,
        IsBaseCurrency = @IsBaseCurrency,
        IsActive = @IsActive,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- Delete Currency
CREATE OR ALTER PROCEDURE [Core].sp_DeleteCurrency
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [Core].Currencies
    SET IsActive = 0, UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- =============================================
-- EXCHANGE RATE MANAGEMENT PROCEDURES
-- =============================================

-- Get Exchange Rate
CREATE OR ALTER PROCEDURE [Core].sp_GetExchangeRate
    @FromCurrencyId UNIQUEIDENTIFIER,
    @ToCurrencyId UNIQUEIDENTIFIER,
    @RateDate DATETIME2,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        er.Id,
        er.FromCurrencyId,
        er.ToCurrencyId,
        er.ExchangeRate,
        er.RateDate,
        fc.CurrencyCode AS FromCurrencyCode,
        tc.CurrencyCode AS ToCurrencyCode
    FROM [Core].ExchangeRates er
    INNER JOIN [Core].Currencies fc ON er.FromCurrencyId = fc.Id
    INNER JOIN [Core].Currencies tc ON er.ToCurrencyId = tc.Id
    WHERE er.FromCurrencyId = @FromCurrencyId
    AND er.ToCurrencyId = @ToCurrencyId
    AND er.RateDate = @RateDate
    AND er.TenantId = @TenantID
    AND er.IsActive = 1;
END
GO

-- Create Exchange Rate
CREATE OR ALTER PROCEDURE [Core].sp_CreateExchangeRate
    @Id UNIQUEIDENTIFIER,
    @FromCurrencyId UNIQUEIDENTIFIER,
    @ToCurrencyId UNIQUEIDENTIFIER,
    @ExchangeRate DECIMAL(18,6),
    @RateDate DATETIME2,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].ExchangeRates (
        Id, FromCurrencyId, ToCurrencyId, ExchangeRate, RateDate, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @FromCurrencyId, @ToCurrencyId, @ExchangeRate, @RateDate, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- COUNTRY MANAGEMENT PROCEDURES
-- =============================================

-- Get All Countries
CREATE OR ALTER PROCEDURE [Core].sp_GetAllCountries
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.Id,
        c.CountryCode,
        c.CountryName,
        c.ISOCode,
        c.PhoneCode,
        c.IsActive,
        curr.CurrencyCode,
        curr.CurrencyName
    FROM [Core].Countries c
    LEFT JOIN [Core].Currencies curr ON c.CurrencyId = curr.Id
    WHERE c.TenantId = @TenantID
    AND c.IsActive = 1
    ORDER BY c.CountryName;
END
GO

-- Get Country by Code
CREATE OR ALTER PROCEDURE [Core].sp_GetCountryByCode
    @CountryCode NVARCHAR(3),
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.Id,
        c.CountryCode,
        c.CountryName,
        c.ISOCode,
        c.PhoneCode,
        c.IsActive,
        curr.CurrencyCode,
        curr.CurrencyName
    FROM [Core].Countries c
    LEFT JOIN [Core].Currencies curr ON c.CurrencyId = curr.Id
    WHERE c.CountryCode = @CountryCode
    AND c.TenantId = @TenantID
    AND c.IsActive = 1;
END
GO

-- Create Country
CREATE OR ALTER PROCEDURE [Core].sp_CreateCountry
    @Id UNIQUEIDENTIFIER,
    @CountryCode NVARCHAR(3),
    @CountryName NVARCHAR(100),
    @ISOCode NVARCHAR(2),
    @PhoneCode NVARCHAR(10) = NULL,
    @CurrencyId UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].Countries (
        Id, CountryCode, CountryName, ISOCode, PhoneCode, CurrencyId, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CountryCode, @CountryName, @ISOCode, @PhoneCode, @CurrencyId, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- STATE MANAGEMENT PROCEDURES
-- =============================================

-- Get States by Country
CREATE OR ALTER PROCEDURE [Core].sp_GetStatesByCountry
    @CountryId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.Id,
        s.CountryId,
        s.StateCode,
        s.StateName,
        s.IsActive,
        c.CountryName
    FROM [Core].States s
    INNER JOIN [Core].Countries c ON s.CountryId = c.Id
    WHERE s.CountryId = @CountryId
    AND s.TenantId = @TenantID
    AND s.IsActive = 1
    ORDER BY s.StateName;
END
GO

-- Create State
CREATE OR ALTER PROCEDURE [Core].sp_CreateState
    @Id UNIQUEIDENTIFIER,
    @CountryId UNIQUEIDENTIFIER,
    @StateCode NVARCHAR(10),
    @StateName NVARCHAR(100),
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].States (
        Id, CountryId, StateCode, StateName, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CountryId, @StateCode, @StateName, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- CITY MANAGEMENT PROCEDURES
-- =============================================

-- Get Cities by State
CREATE OR ALTER PROCEDURE [Core].sp_GetCitiesByState
    @StateId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.Id,
        c.StateId,
        c.CityName,
        c.PostalCode,
        c.IsActive,
        s.StateName,
        s.CountryId,
        co.CountryName
    FROM [Core].Cities c
    INNER JOIN [Core].States s ON c.StateId = s.Id
    INNER JOIN [Core].Countries co ON s.CountryId = co.Id
    WHERE c.StateId = @StateId
    AND c.TenantId = @TenantID
    AND c.IsActive = 1
    ORDER BY c.CityName;
END
GO

-- Create City
CREATE OR ALTER PROCEDURE [Core].sp_CreateCity
    @Id UNIQUEIDENTIFIER,
    @StateId UNIQUEIDENTIFIER,
    @CityName NVARCHAR(100),
    @PostalCode NVARCHAR(20) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].Cities (
        Id, StateId, CityName, PostalCode, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @StateId, @CityName, @PostalCode, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- ADDRESS BOOK MANAGEMENT PROCEDURES
-- =============================================

-- Get Addresses by Entity
CREATE OR ALTER PROCEDURE [Core].sp_GetAddressesByEntity
    @EntityType NVARCHAR(50),
    @EntityId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ab.Id,
        ab.EntityType,
        ab.EntityId,
        ab.AddressType,
        ab.AddressLine1,
        ab.AddressLine2,
        ab.PostalCode,
        ab.IsPrimary,
        ab.IsActive,
        c.CityName,
        s.StateName,
        co.CountryName
    FROM [Core].AddressBook ab
    LEFT JOIN [Core].Cities c ON ab.CityId = c.Id
    LEFT JOIN [Core].States s ON ab.StateId = s.Id
    LEFT JOIN [Core].Countries co ON ab.CountryId = co.Id
    WHERE ab.EntityType = @EntityType
    AND ab.EntityId = @EntityId
    AND ab.TenantId = @TenantID
    AND ab.IsActive = 1
    ORDER BY ab.IsPrimary DESC, ab.AddressType;
END
GO

-- Create Address
CREATE OR ALTER PROCEDURE [Core].sp_CreateAddress
    @Id UNIQUEIDENTIFIER,
    @EntityType NVARCHAR(50),
    @EntityId UNIQUEIDENTIFIER,
    @AddressType NVARCHAR(50),
    @AddressLine1 NVARCHAR(200),
    @AddressLine2 NVARCHAR(200) = NULL,
    @CityId UNIQUEIDENTIFIER = NULL,
    @StateId UNIQUEIDENTIFIER = NULL,
    @CountryId UNIQUEIDENTIFIER = NULL,
    @PostalCode NVARCHAR(20) = NULL,
    @IsPrimary BIT = 0,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If this is primary, unset other primary addresses for this entity
    IF @IsPrimary = 1
    BEGIN
        UPDATE [Core].AddressBook
        SET IsPrimary = 0, UpdatedAt = GETUTCDATE()
        WHERE EntityType = @EntityType
        AND EntityId = @EntityId
        AND TenantId = @TenantID;
    END
    
    INSERT INTO [Core].AddressBook (
        Id, EntityType, EntityId, AddressType, AddressLine1, AddressLine2,
        CityId, StateId, CountryId, PostalCode, IsPrimary, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @EntityType, @EntityId, @AddressType, @AddressLine1, @AddressLine2,
        @CityId, @StateId, @CountryId, @PostalCode, @IsPrimary, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- DOCUMENT TYPE MANAGEMENT PROCEDURES
-- =============================================

-- Get Document Types by Module
CREATE OR ALTER PROCEDURE [Core].sp_GetDocumentTypesByModule
    @Module NVARCHAR(50),
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        dt.Id,
        dt.DocumentType,
        dt.Module,
        dt.Prefix,
        dt.Suffix,
        dt.NumberLength,
        dt.IsActive,
        ns.SeriesName,
        ns.CurrentNumber
    FROM [Core].DocumentTypes dt
    LEFT JOIN [Core].NumberSeries ns ON dt.Id = ns.DocumentTypeId
    WHERE dt.Module = @Module
    AND dt.TenantId = @TenantID
    AND dt.IsActive = 1
    ORDER BY dt.DocumentType;
END
GO

-- Create Document Type
CREATE OR ALTER PROCEDURE [Core].sp_CreateDocumentType
    @Id UNIQUEIDENTIFIER,
    @DocumentType NVARCHAR(50),
    @Module NVARCHAR(50),
    @Prefix NVARCHAR(10),
    @Suffix NVARCHAR(10) = NULL,
    @NumberLength INT = 6,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].DocumentTypes (
        Id, DocumentType, Module, Prefix, Suffix, NumberLength, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @DocumentType, @Module, @Prefix, @Suffix, @NumberLength, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- NUMBER SERIES MANAGEMENT PROCEDURES
-- =============================================

-- Get Next Number
CREATE OR ALTER PROCEDURE [Core].sp_GetNextNumber
    @DocumentTypeId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @NextNumber INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentNumber INT;
    
    -- Get current number and increment
    SELECT @CurrentNumber = CurrentNumber + 1
    FROM [Core].NumberSeries
    WHERE DocumentTypeId = @DocumentTypeId
    AND TenantId = @TenantID
    AND IsActive = 1;
    
    -- Update the current number
    UPDATE [Core].NumberSeries
    SET CurrentNumber = @CurrentNumber,
        UpdatedAt = GETUTCDATE()
    WHERE DocumentTypeId = @DocumentTypeId
    AND TenantId = @TenantID
    AND IsActive = 1;
    
    SET @NextNumber = @CurrentNumber;
END
GO

-- Create Number Series
CREATE OR ALTER PROCEDURE [Core].sp_CreateNumberSeries
    @Id UNIQUEIDENTIFIER,
    @DocumentTypeId UNIQUEIDENTIFIER,
    @SeriesName NVARCHAR(100),
    @StartNumber INT = 1,
    @EndNumber INT = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Core].NumberSeries (
        Id, DocumentTypeId, SeriesName, CurrentNumber, StartNumber, EndNumber, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @DocumentTypeId, @SeriesName, @StartNumber - 1, @StartNumber, @EndNumber, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- USER MANAGEMENT PROCEDURES
-- =============================================

-- Get User by Email
-- Uses only basic columns that exist in the Users table
CREATE OR ALTER PROCEDURE [Core].[sp_GetUserByEmail]
    @PrimaryEmail NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1
        u.Id,
        u.TenantId,
        u.CompanyId,
        u.Salutation,
        u.FirstName,
        u.LastName,
        u.FullName,
        u.Gender,
        u.DateOfBirth,
        u.JobTitle,
        u.Department,
        u.Nationality,
        u.PrimaryEmail,
        u.SecondaryEmail,
        u.PrimaryPhone,
        u.SecondaryPhone,
        u.MobilePhone,
        u.Fax,
        u.LinkedIn,
        u.Twitter,
        u.Facebook,
        u.Instagram,
        u.PreferredCommunicationMethod,
        u.LanguagePreference,
        u.TimeZone,
        u.PasswordHash,
        u.PasswordSalt,
        u.Status,
        u.LastLoginDate,
        u.IsActive,
        u.IsLocked,
        u.FailedLoginAttempts,
        u.LockedUntil,
        u.EmailVerificationToken,
        u.PasswordResetToken,
        u.PasswordResetExpires,
        u.RefreshToken,
        u.RefreshTokenExpires,
        u.CreatedAt,
        u.UpdatedAt,
        u.CreatedBy,
        u.UpdatedBy
    FROM [Core].[Users] u
    WHERE u.PrimaryEmail = @PrimaryEmail
    AND u.IsActive = 1;
END
GO

-- Get User by Email and Tenant
CREATE OR ALTER PROCEDURE [Core].[sp_GetUserByEmailAndTenant]
    @PrimaryEmail NVARCHAR(255),
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1
        u.Id,
        u.TenantId,
        u.CompanyId,
        u.Salutation,
        u.FirstName,
        u.LastName,
        u.FullName,
        u.Gender,
        u.DateOfBirth,
        u.JobTitle,
        u.Department,
        u.Nationality,
        u.PrimaryEmail,
        u.SecondaryEmail,
        u.PrimaryPhone,
        u.SecondaryPhone,
        u.MobilePhone,
        u.Fax,
        u.LinkedIn,
        u.Twitter,
        u.Facebook,
        u.Instagram,
        u.PreferredCommunicationMethod,
        u.LanguagePreference,
        u.TimeZone,
        u.PasswordHash,
        u.PasswordSalt,
        u.Status,
        u.LastLoginDate,
        u.IsActive,
        u.IsLocked,
        u.FailedLoginAttempts,
        u.LockedUntil,
        u.EmailVerificationToken,
        u.PasswordResetToken,
        u.PasswordResetExpires,
        u.RefreshToken,
        u.RefreshTokenExpires,
        u.CreatedAt,
        u.UpdatedAt,
        u.CreatedBy,
        u.UpdatedBy
    FROM [Core].[Users] u
    WHERE u.PrimaryEmail = @PrimaryEmail
    AND u.TenantId = @TenantId
    AND u.IsActive = 1;
END
GO

-- Get User by ID
CREATE OR ALTER PROCEDURE [Core].[sp_GetUserById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1
        u.Id,
        u.TenantId,
        u.CompanyId,
        u.Salutation,
        u.FirstName,
        u.LastName,
        u.FullName,
        u.Gender,
        u.DateOfBirth,
        u.JobTitle,
        u.Department,
        u.Nationality,
        u.PrimaryEmail,
        u.SecondaryEmail,
        u.PrimaryPhone,
        u.SecondaryPhone,
        u.MobilePhone,
        u.Fax,
        u.LinkedIn,
        u.Twitter,
        u.Facebook,
        u.Instagram,
        u.PreferredCommunicationMethod,
        u.LanguagePreference,
        u.TimeZone,
        u.PasswordHash,
        u.PasswordSalt,
        u.Status,
        u.LastLoginDate,
        u.IsActive,
        u.IsLocked,
        u.FailedLoginAttempts,
        u.LockedUntil,
        u.EmailVerificationToken,
        u.PasswordResetToken,
        u.PasswordResetExpires,
        u.RefreshToken,
        u.RefreshTokenExpires,
        u.CreatedAt,
        u.UpdatedAt,
        u.CreatedBy,
        u.UpdatedBy
    FROM [Core].[Users] u
    WHERE u.Id = @Id;
END
GO

-- Get All Users
CREATE OR ALTER PROCEDURE [Core].[sp_GetAllUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.TenantId,
        u.CompanyId,
        u.Salutation,
        u.FirstName,
        u.LastName,
        u.FullName,
        u.Gender,
        u.DateOfBirth,
        u.JobTitle,
        u.Department,
        u.Nationality,
        u.PrimaryEmail,
        u.SecondaryEmail,
        u.PrimaryPhone,
        u.SecondaryPhone,
        u.MobilePhone,
        u.Fax,
        u.LinkedIn,
        u.Twitter,
        u.Facebook,
        u.Instagram,
        u.PreferredCommunicationMethod,
        u.LanguagePreference,
        u.TimeZone,
        u.PasswordHash,
        u.PasswordSalt,
        u.Status,
        u.LastLoginDate,
        u.IsActive,
        u.IsLocked,
        u.FailedLoginAttempts,
        u.LockedUntil,
        u.EmailVerificationToken,
        u.PasswordResetToken,
        u.PasswordResetExpires,
        u.RefreshToken,
        u.RefreshTokenExpires,
        u.CreatedAt,
        u.UpdatedAt,
        u.CreatedBy,
        u.UpdatedBy
    FROM [Core].[Users] u
    WHERE u.IsActive = 1;
END
GO

-- Create User
CREATE OR ALTER PROCEDURE [Core].[sp_CreateUser]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @Salutation NVARCHAR(10) = NULL,
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @FullName NVARCHAR(200) = NULL,
    @Gender NVARCHAR(10) = NULL,
    @DateOfBirth DATETIME2 = NULL,
    @JobTitle NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
    @Nationality NVARCHAR(100) = NULL,
    @PrimaryEmail NVARCHAR(200) = NULL,
    @SecondaryEmail NVARCHAR(200) = NULL,
    @PrimaryPhone NVARCHAR(20) = NULL,
    @SecondaryPhone NVARCHAR(20) = NULL,
    @MobilePhone NVARCHAR(20) = NULL,
    @Fax NVARCHAR(20) = NULL,
    @LinkedIn NVARCHAR(200) = NULL,
    @Twitter NVARCHAR(200) = NULL,
    @Facebook NVARCHAR(200) = NULL,
    @Instagram NVARCHAR(200) = NULL,
    @PreferredCommunicationMethod NVARCHAR(20) = NULL,
    @LanguagePreference NVARCHAR(10) = NULL,
    @TimeZone NVARCHAR(50) = NULL,
    @PasswordHash NVARCHAR(MAX),
    @PasswordSalt NVARCHAR(MAX) = NULL,
    @Status NVARCHAR(50) = NULL,
    @LastLoginDate DATETIME2 = NULL,
    @IsActive BIT = 1,
    @IsLocked BIT = 0,
    @FailedLoginAttempts INT = 0,
    @LockedUntil DATETIME2 = NULL,
    @EmailVerificationToken NVARCHAR(MAX) = NULL,
    @PasswordResetToken NVARCHAR(MAX) = NULL,
    @PasswordResetExpires DATETIME2 = NULL,
    @RefreshToken NVARCHAR(MAX) = NULL,
    @RefreshTokenExpires DATETIME2 = NULL,
    @CreatedAt DATETIME2 = NULL,
    @CreatedBy NVARCHAR(100) = NULL,
    @IsTenantOwner BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set defaults for NOT NULL fields if not provided
    SET @Salutation = ISNULL(@Salutation, '');
    SET @FirstName = ISNULL(@FirstName, '');
    SET @LastName = ISNULL(@LastName, '');
    SET @FullName = ISNULL(@FullName, CONCAT(@FirstName, ' ', @LastName));
    SET @Gender = ISNULL(@Gender, '');
    SET @JobTitle = ISNULL(@JobTitle, '');
    SET @Department = ISNULL(@Department, '');
    SET @Nationality = ISNULL(@Nationality, '');
    SET @PrimaryEmail = ISNULL(@PrimaryEmail, '');
    SET @SecondaryEmail = ISNULL(@SecondaryEmail, '');
    SET @PrimaryPhone = ISNULL(@PrimaryPhone, '');
    SET @SecondaryPhone = ISNULL(@SecondaryPhone, '');
    SET @MobilePhone = ISNULL(@MobilePhone, '');
    SET @Fax = ISNULL(@Fax, '');
    SET @LinkedIn = ISNULL(@LinkedIn, '');
    SET @Twitter = ISNULL(@Twitter, '');
    SET @Facebook = ISNULL(@Facebook, '');
    SET @Instagram = ISNULL(@Instagram, '');
    SET @PreferredCommunicationMethod = ISNULL(@PreferredCommunicationMethod, '');
    SET @LanguagePreference = ISNULL(@LanguagePreference, '');
    SET @TimeZone = ISNULL(@TimeZone, '');
    SET @PasswordSalt = ISNULL(@PasswordSalt, '');
    SET @Status = ISNULL(@Status, 'Active');
    SET @CreatedAt = ISNULL(@CreatedAt, GETUTCDATE());
    SET @IsTenantOwner = ISNULL(@IsTenantOwner, 0);
    
    INSERT INTO [Core].[Users] (
        Id, TenantId, CompanyId, Salutation, FirstName, LastName, FullName,
        Gender, DateOfBirth, JobTitle, Department, Nationality,
        PrimaryEmail, SecondaryEmail, PrimaryPhone, SecondaryPhone,
        MobilePhone, Fax, LinkedIn, Twitter, Facebook, Instagram,
        PreferredCommunicationMethod, LanguagePreference, TimeZone,
        PasswordHash, PasswordSalt, Status, LastLoginDate,
        IsActive, IsLocked, FailedLoginAttempts, LockedUntil,
        EmailVerificationToken, PasswordResetToken, PasswordResetExpires,
        RefreshToken, RefreshTokenExpires,
        CreatedAt, CreatedBy,
        IsTenantOwner
    )
    VALUES (
        @Id, @TenantId, @CompanyId, @Salutation, @FirstName, @LastName, @FullName,
        @Gender, @DateOfBirth, @JobTitle, @Department, @Nationality,
        @PrimaryEmail, @SecondaryEmail, @PrimaryPhone, @SecondaryPhone,
        @MobilePhone, @Fax, @LinkedIn, @Twitter, @Facebook, @Instagram,
        @PreferredCommunicationMethod, @LanguagePreference, @TimeZone,
        @PasswordHash, @PasswordSalt, @Status, @LastLoginDate,
        @IsActive, @IsLocked, @FailedLoginAttempts, @LockedUntil,
        @EmailVerificationToken, @PasswordResetToken, @PasswordResetExpires,
        @RefreshToken, @RefreshTokenExpires,
        @CreatedAt, @CreatedBy,
        @IsTenantOwner
    );
    
    SELECT @Id AS Id;
END
GO

-- Update User
CREATE OR ALTER PROCEDURE [Core].[sp_UpdateUser]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @Salutation NVARCHAR(10) = NULL,
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @FullName NVARCHAR(200) = NULL,
    @Gender NVARCHAR(10) = NULL,
    @DateOfBirth DATETIME2 = NULL,
    @JobTitle NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
    @Nationality NVARCHAR(100) = NULL,
    @PrimaryEmail NVARCHAR(200) = NULL,
    @SecondaryEmail NVARCHAR(200) = NULL,
    @PrimaryPhone NVARCHAR(20) = NULL,
    @SecondaryPhone NVARCHAR(20) = NULL,
    @MobilePhone NVARCHAR(20) = NULL,
    @Fax NVARCHAR(20) = NULL,
    @LinkedIn NVARCHAR(200) = NULL,
    @Twitter NVARCHAR(200) = NULL,
    @Facebook NVARCHAR(200) = NULL,
    @Instagram NVARCHAR(200) = NULL,
    @PreferredCommunicationMethod NVARCHAR(20) = NULL,
    @LanguagePreference NVARCHAR(10) = NULL,
    @TimeZone NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IsActive BIT = NULL,
    @IsLocked BIT = NULL,
    @FailedLoginAttempts INT = NULL,
    @LockedUntil DATETIME2 = NULL,
    @EmailVerificationToken NVARCHAR(MAX) = NULL,
    @PasswordResetToken NVARCHAR(MAX) = NULL,
    @PasswordResetExpires DATETIME2 = NULL,
    @UpdatedAt DATETIME2 = NULL,
    @UpdatedBy NVARCHAR(100) = NULL,
    @IsTenantOwner BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate user exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Core].[Users] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('User not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Update only provided fields
    UPDATE [Core].[Users]
    SET 
        CompanyId = ISNULL(@CompanyId, CompanyId),
        Salutation = ISNULL(@Salutation, Salutation),
        FirstName = ISNULL(@FirstName, FirstName),
        LastName = ISNULL(@LastName, LastName),
        FullName = ISNULL(@FullName, ISNULL(@FullName, CONCAT(ISNULL(@FirstName, FirstName), ' ', ISNULL(@LastName, LastName)))),
        Gender = ISNULL(@Gender, Gender),
        DateOfBirth = ISNULL(@DateOfBirth, DateOfBirth),
        JobTitle = ISNULL(@JobTitle, JobTitle),
        Department = ISNULL(@Department, Department),
        Nationality = ISNULL(@Nationality, Nationality),
        PrimaryEmail = ISNULL(@PrimaryEmail, PrimaryEmail),
        SecondaryEmail = ISNULL(@SecondaryEmail, SecondaryEmail),
        PrimaryPhone = ISNULL(@PrimaryPhone, PrimaryPhone),
        SecondaryPhone = ISNULL(@SecondaryPhone, SecondaryPhone),
        MobilePhone = ISNULL(@MobilePhone, MobilePhone),
        Fax = ISNULL(@Fax, Fax),
        LinkedIn = ISNULL(@LinkedIn, LinkedIn),
        Twitter = ISNULL(@Twitter, Twitter),
        Facebook = ISNULL(@Facebook, Facebook),
        Instagram = ISNULL(@Instagram, Instagram),
        PreferredCommunicationMethod = ISNULL(@PreferredCommunicationMethod, PreferredCommunicationMethod),
        LanguagePreference = ISNULL(@LanguagePreference, LanguagePreference),
        TimeZone = ISNULL(@TimeZone, TimeZone),
        Status = ISNULL(@Status, Status),
        IsActive = ISNULL(@IsActive, IsActive),
        IsLocked = ISNULL(@IsLocked, IsLocked),
        FailedLoginAttempts = ISNULL(@FailedLoginAttempts, FailedLoginAttempts),
        LockedUntil = ISNULL(@LockedUntil, LockedUntil),
        EmailVerificationToken = ISNULL(@EmailVerificationToken, EmailVerificationToken),
        PasswordResetToken = ISNULL(@PasswordResetToken, PasswordResetToken),
        PasswordResetExpires = ISNULL(@PasswordResetExpires, PasswordResetExpires),
        UpdatedAt = ISNULL(@UpdatedAt, GETUTCDATE()),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy),
        IsTenantOwner = ISNULL(@IsTenantOwner, IsTenantOwner)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete User (Soft Delete)
CREATE OR ALTER PROCEDURE [Core].[sp_DeleteUser]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate user exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Core].[Users] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('User not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsActive = 0
    UPDATE [Core].[Users]
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Update User Password
CREATE OR ALTER PROCEDURE [Core].[sp_UpdateUserPassword]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(MAX),
    @PasswordSalt NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate user exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [Core].[Users] WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('User not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Update password
    UPDATE [Core].[Users]
    SET 
        PasswordHash = @PasswordHash,
        PasswordSalt = ISNULL(@PasswordSalt, PasswordSalt),
        PasswordResetToken = NULL,
        PasswordResetExpires = NULL,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

PRINT 'Core Module stored procedures created successfully!'