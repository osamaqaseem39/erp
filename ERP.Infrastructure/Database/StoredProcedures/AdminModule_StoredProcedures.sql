-- =============================================
-- Admin Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create Admin Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Admin')
BEGIN
    EXEC('CREATE SCHEMA [Admin]')
END
GO

-- =============================================
-- SYSTEM LOG MANAGEMENT PROCEDURES
-- =============================================

-- Get System Log by ID
CREATE OR ALTER PROCEDURE [Admin].[sp_GetSystemLogById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sl.Id,
        sl.TenantId,
        sl.LogLevel,
        sl.Message,
        sl.Exception,
        sl.Source,
        sl.Method,
        sl.UserId,
        sl.IPAddress,
        sl.Timestamp,
        sl.Properties,
        sl.CreatedAt,
        u.FirstName + ' ' + u.LastName as UserName
    FROM [Admin].SystemLogs sl
    LEFT JOIN [Core].Users u ON sl.UserId = u.Id
    WHERE sl.Id = @Id
    AND sl.TenantId = @TenantID;
END
GO

-- Get System Logs
CREATE OR ALTER PROCEDURE [Admin].[sp_GetSystemLogs]
    @TenantID UNIQUEIDENTIFIER,
    @LogLevel NVARCHAR(50) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @Source NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        sl.Id,
        sl.LogLevel,
        sl.Message,
        sl.Exception,
        sl.Source,
        sl.Method,
        sl.UserId,
        sl.IPAddress,
        sl.Timestamp,
        sl.Properties,
        u.FirstName + ' ' + u.LastName as UserName
    FROM [Admin].SystemLogs sl
    LEFT JOIN [Core].Users u ON sl.UserId = u.Id
    WHERE sl.TenantId = @TenantID
    AND (@LogLevel IS NULL OR sl.LogLevel = @LogLevel)
    AND (@StartDate IS NULL OR sl.Timestamp >= @StartDate)
    AND (@EndDate IS NULL OR sl.Timestamp <= @EndDate)
    AND (@UserId IS NULL OR sl.UserId = @UserId)
    AND (@Source IS NULL OR sl.Source LIKE '%' + @Source + '%')
    ORDER BY sl.Timestamp DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM SystemLogs sl
    WHERE sl.TenantId = @TenantID
    AND (@LogLevel IS NULL OR sl.LogLevel = @LogLevel)
    AND (@StartDate IS NULL OR sl.Timestamp >= @StartDate)
    AND (@EndDate IS NULL OR sl.Timestamp <= @EndDate)
    AND (@UserId IS NULL OR sl.UserId = @UserId)
    AND (@Source IS NULL OR sl.Source LIKE '%' + @Source + '%');
END
GO

-- Create System Log
CREATE OR ALTER PROCEDURE [Admin].[sp_CreateSystemLog]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @LogLevel NVARCHAR(50),
    @Message NVARCHAR(MAX),
    @Exception NVARCHAR(MAX) = NULL,
    @Source NVARCHAR(200) = NULL,
    @Method NVARCHAR(200) = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @IPAddress NVARCHAR(50) = NULL,
    @Timestamp DATETIME2 = NULL,
    @Properties NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Admin].SystemLogs (
        Id, TenantId, LogLevel, Message, Exception, Source, Method,
        UserId, IPAddress, Timestamp, Properties
    )
    VALUES (
        @Id, @TenantID, @LogLevel, @Message, @Exception, @Source, @Method,
        @UserId, @IPAddress, ISNULL(@Timestamp, GETUTCDATE()), @Properties
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- SYSTEM SETTING MANAGEMENT PROCEDURES
-- =============================================

-- Get System Setting by Key
CREATE OR ALTER PROCEDURE [Admin].[sp_GetSystemSettingByKey]
    @SettingKey NVARCHAR(200),
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ss.Id,
        ss.TenantId,
        ss.SettingKey,
        ss.SettingValue,
        ss.Description,
        ss.SettingType,
        ss.Category,
        ss.IsEncrypted,
        ss.IsReadOnly,
        ss.ValidationRule,
        ss.Notes,
        ss.CreatedAt,
        ss.UpdatedAt,
        ss.CreatedBy,
        ss.UpdatedBy
    FROM [Admin].SystemSettings ss
    WHERE ss.SettingKey = @SettingKey
    AND ss.TenantId = @TenantID;
END
GO

-- Get All System Settings
CREATE OR ALTER PROCEDURE [Admin].[sp_GetAllSystemSettings]
    @TenantID UNIQUEIDENTIFIER,
    @Category NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        ss.Id,
        ss.SettingKey,
        ss.SettingValue,
        ss.Description,
        ss.SettingType,
        ss.Category,
        ss.IsEncrypted,
        ss.IsReadOnly,
        ss.ValidationRule,
        ss.Notes,
        ss.CreatedAt,
        ss.UpdatedAt
    FROM [Admin].SystemSettings ss
    WHERE ss.TenantId = @TenantID
    AND (@Category IS NULL OR ss.Category = @Category)
    ORDER BY ss.Category, ss.SettingKey
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM [Admin].SystemSettings ss
    WHERE ss.TenantId = @TenantID
    AND (@Category IS NULL OR ss.Category = @Category);
END
GO

-- Upsert System Setting
CREATE OR ALTER PROCEDURE [Admin].[sp_UpsertSystemSetting]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @SettingKey NVARCHAR(200),
    @SettingValue NVARCHAR(MAX),
    @Description NVARCHAR(500) = NULL,
    @SettingType NVARCHAR(50) = 'String',
    @Category NVARCHAR(50) = 'General',
    @IsEncrypted BIT = 0,
    @IsReadOnly BIT = 0,
    @ValidationRule NVARCHAR(500) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Admin].SystemSettings WHERE SettingKey = @SettingKey AND TenantId = @TenantID)
    BEGIN
        UPDATE [Admin].SystemSettings
        SET 
            SettingValue = @SettingValue,
            Description = @Description,
            SettingType = @SettingType,
            Category = @Category,
            IsEncrypted = @IsEncrypted,
            IsReadOnly = @IsReadOnly,
            ValidationRule = @ValidationRule,
            Notes = @Notes,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @CreatedBy
        WHERE SettingKey = @SettingKey
        AND TenantId = @TenantID;
        
        SELECT Id FROM [Admin].SystemSettings WHERE SettingKey = @SettingKey AND TenantId = @TenantID;
    END
    ELSE
    BEGIN
        INSERT INTO [Admin].SystemSettings (
            Id, TenantId, SettingKey, SettingValue, Description, SettingType,
            Category, IsEncrypted, IsReadOnly, ValidationRule, Notes, CreatedBy
        )
        VALUES (
            @Id, @TenantID, @SettingKey, @SettingValue, @Description, @SettingType,
            @Category, @IsEncrypted, @IsReadOnly, @ValidationRule, @Notes, @CreatedBy
        );
        
        SELECT @Id as Id;
    END
END
GO

-- Delete System Setting
CREATE OR ALTER PROCEDURE [Admin].[sp_DeleteSystemSetting]
    @SettingKey NVARCHAR(200),
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [Admin].SystemSettings
    WHERE SettingKey = @SettingKey
    AND TenantId = @TenantID
    AND IsReadOnly = 0; -- Prevent deletion of read-only settings
END
GO

