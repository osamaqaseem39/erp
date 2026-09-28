-- =============================================
-- Reports Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Reports')
BEGIN
    EXEC('CREATE SCHEMA [Reports]')
END
GO

-- =============================================
-- REPORT MANAGEMENT PROCEDURES
-- =============================================

-- Get Report by ID
CREATE OR ALTER PROCEDURE [Reports].[sp_GetReportById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.Id,
        r.CompanyId,
        r.TenantId,
        r.Name,
        r.ReportName,
        r.ReportCode,
        r.Description,
        r.ReportType,
        r.Category,
        r.Query,
        r.Parameters,
        r.OutputFormat,
        r.IsActive,
        r.IsPublic,
        r.Notes,
        r.CreatedAt,
        r.UpdatedAt,
        r.CreatedBy,
        r.UpdatedBy
    FROM dbo.Reports r
    WHERE r.Id = @Id
    AND r.TenantId = @TenantID;
END
GO

-- Get All Reports
CREATE OR ALTER PROCEDURE [Reports].[sp_GetAllReports]
    @TenantID UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @ReportType NVARCHAR(50) = NULL,
    @Category NVARCHAR(50) = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        r.Id,
        r.ReportName,
        r.ReportCode,
        r.Description,
        r.ReportType,
        r.Category,
        r.OutputFormat,
        r.IsActive,
        r.IsPublic,
        r.CreatedAt,
        r.UpdatedAt
    FROM dbo.Reports r
    WHERE r.TenantId = @TenantID
    AND (@CompanyId IS NULL OR r.CompanyId = @CompanyId)
    AND (@ReportType IS NULL OR r.ReportType = @ReportType)
    AND (@Category IS NULL OR r.Category = @Category)
    AND (@IsActive IS NULL OR r.IsActive = @IsActive)
    ORDER BY r.Category, r.ReportName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM dbo.Reports r
    WHERE r.TenantId = @TenantID
    AND (@CompanyId IS NULL OR r.CompanyId = @CompanyId)
    AND (@ReportType IS NULL OR r.ReportType = @ReportType)
    AND (@Category IS NULL OR r.Category = @Category)
    AND (@IsActive IS NULL OR r.IsActive = @IsActive);
END
GO

-- Create Report
CREATE OR ALTER PROCEDURE [Reports].[sp_CreateReport]
    @Id UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @ReportName NVARCHAR(200),
    @ReportCode NVARCHAR(50),
    @Description NVARCHAR(MAX) = NULL,
    @ReportType NVARCHAR(50),
    @Category NVARCHAR(50),
    @Query NVARCHAR(MAX),
    @Parameters NVARCHAR(MAX) = NULL,
    @OutputFormat NVARCHAR(50) = 'PDF',
    @IsActive BIT = 1,
    @IsPublic BIT = 0,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.Reports (
        Id, CompanyId, TenantId, Name, ReportName, ReportCode, Description,
        ReportType, Category, Query, Parameters, OutputFormat, IsActive,
        IsPublic, Notes, CreatedBy
    )
    VALUES (
        @Id, @CompanyId, @TenantID, @Name, @ReportName, @ReportCode, @Description,
        @ReportType, @Category, @Query, @Parameters, @OutputFormat, @IsActive,
        @IsPublic, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Report
CREATE OR ALTER PROCEDURE [Reports].[sp_UpdateReport]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @ReportName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @ReportType NVARCHAR(50),
    @Category NVARCHAR(50),
    @Query NVARCHAR(MAX),
    @Parameters NVARCHAR(MAX) = NULL,
    @OutputFormat NVARCHAR(50),
    @IsActive BIT,
    @IsPublic BIT,
    @Notes NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Reports
    SET 
        Name = @Name,
        ReportName = @ReportName,
        Description = @Description,
        ReportType = @ReportType,
        Category = @Category,
        Query = @Query,
        Parameters = @Parameters,
        OutputFormat = @OutputFormat,
        IsActive = @IsActive,
        IsPublic = @IsPublic,
        Notes = @Notes,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- =============================================
-- REPORT EXECUTION MANAGEMENT PROCEDURES
-- =============================================

-- Get Report Executions by Report ID
CREATE OR ALTER PROCEDURE [Reports].[sp_GetReportExecutionsByReportId]
    @ReportId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        re.Id,
        re.ReportId,
        re.StartedAt as ExecutionDate,
        re.Status,
        re.Parameters,
        re.OutputPath as OutputFilePath,
        re.ExecutionTimeMs as ExecutionTime,
        re.ErrorMessage,
        re.ExecutedBy as CreatedBy,
        re.CreatedAt
    FROM dbo.ReportExecutions re
    INNER JOIN dbo.Reports r ON re.ReportId = r.Id
    WHERE re.ReportId = @ReportId
    AND r.TenantId = @TenantID
    ORDER BY re.StartedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM dbo.ReportExecutions re
    INNER JOIN dbo.Reports r ON re.ReportId = r.Id
    WHERE re.ReportId = @ReportId
    AND r.TenantId = @TenantID;
END
GO

-- Create Report Execution
CREATE OR ALTER PROCEDURE [Reports].[sp_CreateReportExecution]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ReportId UNIQUEIDENTIFIER,
    @ExecutionDate DATETIME2,
    @Status NVARCHAR(50),
    @Parameters NVARCHAR(MAX) = NULL,
    @OutputFilePath NVARCHAR(500) = NULL,
    @ExecutionTime INT = NULL,
    @ErrorMessage NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.ReportExecutions (
        Id, ReportId, StartedAt, Status, Parameters, OutputPath,
        ExecutionTimeMs, ErrorMessage, ExecutedBy
    )
    VALUES (
        @Id, @ReportId, @ExecutionDate, @Status, @Parameters, @OutputFilePath,
        @ExecutionTime, @ErrorMessage, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- REPORT SCHEDULE MANAGEMENT PROCEDURES
-- =============================================

-- Get Report Schedules by Report ID
CREATE OR ALTER PROCEDURE [Reports].[sp_GetReportSchedulesByReportId]
    @ReportId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rs.Id,
        rs.ReportId,
        rs.ScheduleName,
        rs.ScheduleType,
        rs.CronExpression as ScheduleExpression,
        rs.NextRunTime as NextRunDate,
        rs.LastRunTime as LastRunDate,
        rs.IsActive,
        rs.EmailRecipients as Recipients,
        rs.Notes,
        rs.CreatedAt,
        rs.UpdatedAt
    FROM dbo.ReportSchedules rs
    INNER JOIN dbo.Reports r ON rs.ReportId = r.Id
    WHERE rs.ReportId = @ReportId
    AND r.TenantId = @TenantID
    ORDER BY rs.NextRunTime;
END
GO

-- Create Report Schedule
CREATE OR ALTER PROCEDURE [Reports].[sp_CreateReportSchedule]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ReportId UNIQUEIDENTIFIER,
    @ScheduleName NVARCHAR(200),
    @ScheduleType NVARCHAR(50),
    @ScheduleExpression NVARCHAR(200),
    @NextRunDate DATETIME2,
    @IsActive BIT = 1,
    @Recipients NVARCHAR(MAX) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.ReportSchedules (
        Id, ReportId, ScheduleName, ScheduleType, CronExpression,
        NextRunTime, IsActive, EmailRecipients, Notes, CreatedBy
    )
    VALUES (
        @Id, @ReportId, @ScheduleName, @ScheduleType, @ScheduleExpression,
        @NextRunDate, @IsActive, @Recipients, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Report Schedule
CREATE OR ALTER PROCEDURE [Reports].[sp_UpdateReportSchedule]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ScheduleName NVARCHAR(200),
    @ScheduleType NVARCHAR(50),
    @ScheduleExpression NVARCHAR(200),
    @NextRunDate DATETIME2,
    @LastRunDate DATETIME2 = NULL,
    @IsActive BIT,
    @Recipients NVARCHAR(MAX) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.ReportSchedules
    SET 
        ScheduleName = @ScheduleName,
        ScheduleType = @ScheduleType,
        CronExpression = @ScheduleExpression,
        NextRunTime = @NextRunDate,
        LastRunTime = ISNULL(@LastRunDate, LastRunTime),
        IsActive = @IsActive,
        EmailRecipients = @Recipients,
        Notes = @Notes,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND EXISTS (
        SELECT 1 FROM dbo.Reports r
        WHERE r.Id = dbo.ReportSchedules.ReportId
        AND r.TenantId = @TenantID
    );
END
GO

-- Get Scheduled Reports
CREATE OR ALTER PROCEDURE [Reports].[sp_GetScheduledReports]
    @TenantID UNIQUEIDENTIFIER,
    @NextRunDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rs.Id,
        rs.ReportId,
        rs.ScheduleName,
        rs.ScheduleType,
        rs.CronExpression as ScheduleExpression,
        rs.NextRunTime as NextRunDate,
        rs.LastRunTime as LastRunDate,
        rs.IsActive,
        rs.EmailRecipients as Recipients,
        r.ReportName,
        r.ReportCode,
        r.ReportType,
        r.Category
    FROM dbo.ReportSchedules rs
    INNER JOIN dbo.Reports r ON rs.ReportId = r.Id
    WHERE r.TenantId = @TenantID
    AND rs.IsActive = 1
    AND (@NextRunDate IS NULL OR rs.NextRunTime <= @NextRunDate)
    ORDER BY rs.NextRunTime;
END
GO

