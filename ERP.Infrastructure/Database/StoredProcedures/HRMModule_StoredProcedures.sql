-- =============================================
-- HRM Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create HRM Schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'HRM')
BEGIN
    EXEC('CREATE SCHEMA [HRM]')
END
GO

-- =============================================
-- EMPLOYEE TYPE MANAGEMENT PROCEDURES
-- =============================================

-- Get All Employee Types
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllEmployeeTypes
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        TypeName,
        Description,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM [HRM].EmployeeTypes
    WHERE TenantId = @TenantID
    AND IsActive = 1
    ORDER BY TypeName;
END
GO

-- Create Employee Type
CREATE OR ALTER PROCEDURE [HRM].sp_CreateEmployeeType
    @Id UNIQUEIDENTIFIER,
    @TypeName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [HRM].EmployeeTypes (
        Id, TypeName, Description, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @TypeName, @Description, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- JOB CATEGORY MANAGEMENT PROCEDURES
-- =============================================

-- Get All Job Categories
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllJobCategories
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        CategoryName,
        Description,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM [HRM].JobCategories
    WHERE TenantId = @TenantID
    AND IsActive = 1
    ORDER BY CategoryName;
END
GO

-- Create Job Category
CREATE OR ALTER PROCEDURE [HRM].sp_CreateJobCategory
    @Id UNIQUEIDENTIFIER,
    @CategoryName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [HRM].JobCategories (
        Id, CategoryName, Description, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CategoryName, @Description, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- WORK LOCATION MANAGEMENT PROCEDURES
-- =============================================

-- Get All Work Locations
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllWorkLocations
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        wl.Id,
        wl.LocationName,
        wl.Address,
        wl.City,
        wl.State,
        wl.Country,
        wl.ZipCode,
        wl.Phone,
        wl.Email,
        wl.ManagerId,
        wl.IsActive,
        e.FirstName + ' ' + e.LastName AS ManagerName
    FROM [HRM].WorkLocations wl
    LEFT JOIN [HRM].Employees e ON wl.ManagerId = e.Id
    WHERE wl.TenantId = @TenantID
    AND wl.IsActive = 1
    ORDER BY wl.LocationName;
END
GO

-- Create Work Location
CREATE OR ALTER PROCEDURE [HRM].sp_CreateWorkLocation
    @Id UNIQUEIDENTIFIER,
    @LocationName NVARCHAR(200),
    @Address NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [HRM].WorkLocations (
        Id, LocationName, Address, City, State, Country, ZipCode, 
        Phone, Email, ManagerId, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @LocationName, @Address, @City, @State, @Country, @ZipCode,
        @Phone, @Email, @ManagerId, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- OVERTIME POLICY MANAGEMENT PROCEDURES
-- =============================================

-- Get All Overtime Policies
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllOvertimePolicies
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        PolicyName,
        RegularHoursPerDay,
        RegularHoursPerWeek,
        OvertimeMultiplier,
        DoubleTimeMultiplier,
        WeekendMultiplier,
        HolidayMultiplier,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM [HRM].OvertimePolicies
    WHERE TenantId = @TenantID
    AND IsActive = 1
    ORDER BY PolicyName;
END
GO

-- Create Overtime Policy
CREATE OR ALTER PROCEDURE [HRM].sp_CreateOvertimePolicy
    @Id UNIQUEIDENTIFIER,
    @PolicyName NVARCHAR(100),
    @RegularHoursPerDay DECIMAL(5,2) = 8.00,
    @RegularHoursPerWeek DECIMAL(5,2) = 40.00,
    @OvertimeMultiplier DECIMAL(5,2) = 1.50,
    @DoubleTimeMultiplier DECIMAL(5,2) = 2.00,
    @WeekendMultiplier DECIMAL(5,2) = 1.50,
    @HolidayMultiplier DECIMAL(5,2) = 2.00,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [HRM].OvertimePolicies (
        Id, PolicyName, RegularHoursPerDay, RegularHoursPerWeek,
        OvertimeMultiplier, DoubleTimeMultiplier, WeekendMultiplier,
        HolidayMultiplier, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @PolicyName, @RegularHoursPerDay, @RegularHoursPerWeek,
        @OvertimeMultiplier, @DoubleTimeMultiplier, @WeekendMultiplier,
        @HolidayMultiplier, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- GRIEVANCE MANAGEMENT PROCEDURES
-- =============================================

-- Get Grievances by Employee
CREATE OR ALTER PROCEDURE [HRM].sp_GetGrievancesByEmployee
    @EmployeeId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        g.Id,
        g.EmployeeId,
        g.GrievanceType,
        g.Subject,
        g.Description,
        g.Status,
        g.Priority,
        g.AssignedTo,
        g.Resolution,
        g.ResolvedAt,
        g.ResolvedBy,
        g.CreatedAt,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        a.FirstName + ' ' + a.LastName AS AssignedToName,
        r.FirstName + ' ' + r.LastName AS ResolvedByName
    FROM [HRM].Grievances g
    INNER JOIN [HRM].Employees e ON g.EmployeeId = e.Id
    LEFT JOIN [HRM].Employees a ON g.AssignedTo = a.Id
    LEFT JOIN [HRM].Employees r ON g.ResolvedBy = r.Id
    WHERE g.EmployeeId = @EmployeeId
    AND g.TenantId = @TenantID
    AND g.IsActive = 1
    ORDER BY g.CreatedAt DESC;
END
GO

-- Create Grievance
CREATE OR ALTER PROCEDURE [HRM].sp_CreateGrievance
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @GrievanceType NVARCHAR(100),
    @Subject NVARCHAR(200),
    @Description NVARCHAR(4000),
    @Status NVARCHAR(50) = 'Open',
    @Priority NVARCHAR(50) = 'Medium',
    @AssignedTo UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [HRM].Grievances (
        Id, EmployeeId, GrievanceType, Subject, Description, Status,
        Priority, AssignedTo, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @EmployeeId, @GrievanceType, @Subject, @Description, @Status,
        @Priority, @AssignedTo, 1, @TenantID, @CreatedBy
    );
END
GO

-- Update Grievance Status
CREATE OR ALTER PROCEDURE [HRM].sp_UpdateGrievanceStatus
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Resolution NVARCHAR(4000) = NULL,
    @ResolvedBy UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [HRM].Grievances
    SET 
        Status = @Status,
        Resolution = @Resolution,
        ResolvedAt = CASE WHEN @Status = 'Resolved' THEN GETUTCDATE() ELSE ResolvedAt END,
        ResolvedBy = @ResolvedBy,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- =============================================
-- DISCIPLINARY ACTION MANAGEMENT PROCEDURES
-- =============================================

-- Get Disciplinary Actions by Employee
CREATE OR ALTER PROCEDURE [HRM].sp_GetDisciplinaryActionsByEmployee
    @EmployeeId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        da.Id,
        da.EmployeeId,
        da.ActionType,
        da.Subject,
        da.Description,
        da.ActionDate,
        da.Status,
        da.Severity,
        da.IssuedBy,
        da.ValidUntil,
        da.CreatedAt,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        i.FirstName + ' ' + i.LastName AS IssuedByName
    FROM [HRM].DisciplinaryActions da
    INNER JOIN [HRM].Employees e ON da.EmployeeId = e.Id
    INNER JOIN [HRM].Employees i ON da.IssuedBy = i.Id
    WHERE da.EmployeeId = @EmployeeId
    AND da.TenantId = @TenantID
    AND da.IsActive = 1
    ORDER BY da.ActionDate DESC;
END
GO

-- Create Disciplinary Action
CREATE OR ALTER PROCEDURE [HRM].sp_CreateDisciplinaryAction
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @ActionType NVARCHAR(100),
    @Subject NVARCHAR(200),
    @Description NVARCHAR(4000),
    @ActionDate DATETIME2,
    @Status NVARCHAR(50) = 'Active',
    @Severity NVARCHAR(50) = 'Medium',
    @IssuedBy UNIQUEIDENTIFIER,
    @ValidUntil DATETIME2 = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [HRM].DisciplinaryActions (
        Id, EmployeeId, ActionType, Subject, Description, ActionDate,
        Status, Severity, IssuedBy, ValidUntil, IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @EmployeeId, @ActionType, @Subject, @Description, @ActionDate,
        @Status, @Severity, @IssuedBy, @ValidUntil, 1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- EXIT INTERVIEW MANAGEMENT PROCEDURES
-- =============================================

-- Get Exit Interviews by Employee
CREATE OR ALTER PROCEDURE [HRM].sp_GetExitInterviewsByEmployee
    @EmployeeId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ei.Id,
        ei.EmployeeId,
        ei.InterviewDate,
        ei.InterviewerId,
        ei.ReasonForLeaving,
        ei.SatisfactionRating,
        ei.RecommendationRating,
        ei.Comments,
        ei.Suggestions,
        ei.CreatedAt,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        i.FirstName + ' ' + i.LastName AS InterviewerName
    FROM [HRM].ExitInterviews ei
    INNER JOIN [HRM].Employees e ON ei.EmployeeId = e.Id
    INNER JOIN [HRM].Employees i ON ei.InterviewerId = i.Id
    WHERE ei.EmployeeId = @EmployeeId
    AND ei.TenantId = @TenantID
    AND ei.IsActive = 1
    ORDER BY ei.InterviewDate DESC;
END
GO

-- Create Exit Interview
CREATE OR ALTER PROCEDURE [HRM].sp_CreateExitInterview
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @InterviewDate DATETIME2,
    @InterviewerId UNIQUEIDENTIFIER,
    @ReasonForLeaving NVARCHAR(100),
    @SatisfactionRating INT,
    @RecommendationRating INT,
    @Comments NVARCHAR(4000) = NULL,
    @Suggestions NVARCHAR(4000) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate ratings
    IF @SatisfactionRating < 1 OR @SatisfactionRating > 5
    BEGIN
        RAISERROR('Satisfaction rating must be between 1 and 5', 16, 1);
        RETURN;
    END
    
    IF @RecommendationRating < 1 OR @RecommendationRating > 5
    BEGIN
        RAISERROR('Recommendation rating must be between 1 and 5', 16, 1);
        RETURN;
    END
    
    INSERT INTO [HRM].ExitInterviews (
        Id, EmployeeId, InterviewDate, InterviewerId, ReasonForLeaving,
        SatisfactionRating, RecommendationRating, Comments, Suggestions,
        IsActive, TenantId, CreatedBy
    )
    VALUES (
        @Id, @EmployeeId, @InterviewDate, @InterviewerId, @ReasonForLeaving,
        @SatisfactionRating, @RecommendationRating, @Comments, @Suggestions,
        1, @TenantID, @CreatedBy
    );
END
GO

-- =============================================
-- EMPLOYEE MANAGEMENT PROCEDURES (Enhanced)
-- =============================================

-- Get Employee with Details
CREATE OR ALTER PROCEDURE [HRM].sp_GetEmployeeWithDetails
    @EmployeeId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.Id,
        e.CompanyId,
        e.EmployeeCode,
        e.Salutation,
        e.FirstName,
        e.LastName,
        e.PreferredName,
        e.Gender,
        e.DateOfBirth,
        e.NationalId,
        e.Nationality,
        e.MaritalStatus,
        e.JobTitle,
        e.DepartmentId,
        e.ManagerId,
        e.EmploymentType,
        e.WorkEmail,
        e.PersonalEmail,
        e.WorkPhone,
        e.MobilePhone,
        e.WorkLocationId,
        e.DateHired,
        e.DateTerminated,
        e.Status,
        e.IsRemote,
        e.EmploymentContractId,
        e.IsDeleted,
        e.CreatedAt,
        e.UpdatedAt,
        m.FirstName + ' ' + m.LastName AS ManagerName,
        wl.LocationName AS WorkLocationName,
        et.TypeName AS EmployeeTypeName,
        jc.CategoryName AS JobCategoryName
    FROM [HRM].Employees e
    LEFT JOIN [HRM].Employees m ON e.ManagerId = m.Id
    LEFT JOIN [HRM].WorkLocations wl ON e.WorkLocationId = wl.Id
    LEFT JOIN [HRM].EmployeeTypes et ON e.EmploymentType = et.TypeName
    LEFT JOIN [HRM].JobCategories jc ON e.JobTitle = jc.CategoryName
    WHERE e.Id = @EmployeeId
    AND e.TenantId = @TenantID
    AND e.IsActive = 1;
END
GO

-- =============================================
-- EMPLOYEE TYPES PROCEDURES
-- =============================================

-- Get All Employee Types
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllEmployeeTypes
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        et.Id,
        et.TypeName,
        et.Description,
        et.IsActive,
        et.CreatedAt,
        et.UpdatedAt,
        et.CreatedBy,
        et.UpdatedBy
    FROM [HRM].EmployeeTypes et
    WHERE et.TenantId = @TenantId
    AND (@IsActive IS NULL OR et.IsActive = @IsActive)
    ORDER BY et.TypeName;
END
GO

-- Create Employee Type
CREATE OR ALTER PROCEDURE [HRM].sp_CreateEmployeeType
    @TypeName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].EmployeeTypes (
        Id, TypeName, Description, TenantId, CreatedBy
    )
    VALUES (
        @Id, @TypeName, @Description, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- JOB CATEGORIES PROCEDURES
-- =============================================

-- Get All Job Categories
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllJobCategories
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        jc.Id,
        jc.CategoryName,
        jc.Description,
        jc.IsActive,
        jc.CreatedAt,
        jc.UpdatedAt,
        jc.CreatedBy,
        jc.UpdatedBy
    FROM [HRM].JobCategories jc
    WHERE jc.TenantId = @TenantId
    AND (@IsActive IS NULL OR jc.IsActive = @IsActive)
    ORDER BY jc.CategoryName;
END
GO

-- Create Job Category
CREATE OR ALTER PROCEDURE [HRM].sp_CreateJobCategory
    @CategoryName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].JobCategories (
        Id, CategoryName, Description, TenantId, CreatedBy
    )
    VALUES (
        @Id, @CategoryName, @Description, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- WORK LOCATIONS PROCEDURES
-- =============================================

-- Get All Work Locations
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllWorkLocations
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        wl.Id,
        wl.LocationName,
        wl.Address,
        wl.City,
        wl.State,
        wl.Country,
        wl.ZipCode,
        wl.Phone,
        wl.Email,
        wl.ManagerId,
        wl.IsActive,
        wl.CreatedAt,
        wl.UpdatedAt,
        wl.CreatedBy,
        wl.UpdatedBy,
        m.FirstName + ' ' + m.LastName as ManagerName
    FROM [HRM].WorkLocations wl
    LEFT JOIN [HRM].Employees m ON wl.ManagerId = m.Id
    WHERE wl.TenantId = @TenantId
    AND (@IsActive IS NULL OR wl.IsActive = @IsActive)
    ORDER BY wl.LocationName;
END
GO

-- Create Work Location
CREATE OR ALTER PROCEDURE [HRM].sp_CreateWorkLocation
    @LocationName NVARCHAR(200),
    @Address NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].WorkLocations (
        Id, LocationName, Address, City, State, Country, ZipCode,
        Phone, Email, ManagerId, TenantId, CreatedBy
    )
    VALUES (
        @Id, @LocationName, @Address, @City, @State, @Country, @ZipCode,
        @Phone, @Email, @ManagerId, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- OVERTIME POLICIES PROCEDURES
-- =============================================

-- Get All Overtime Policies
CREATE OR ALTER PROCEDURE [HRM].sp_GetAllOvertimePolicies
    @TenantId UNIQUEIDENTIFIER,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        op.Id,
        op.PolicyName,
        op.RegularHoursPerDay,
        op.RegularHoursPerWeek,
        op.OvertimeMultiplier,
        op.DoubleTimeMultiplier,
        op.WeekendMultiplier,
        op.HolidayMultiplier,
        op.IsActive,
        op.CreatedAt,
        op.UpdatedAt,
        op.CreatedBy,
        op.UpdatedBy
    FROM [HRM].OvertimePolicies op
    WHERE op.TenantId = @TenantId
    AND (@IsActive IS NULL OR op.IsActive = @IsActive)
    ORDER BY op.PolicyName;
END
GO

-- Create Overtime Policy
CREATE OR ALTER PROCEDURE [HRM].sp_CreateOvertimePolicy
    @PolicyName NVARCHAR(100),
    @RegularHoursPerDay DECIMAL(5,2) = 8.00,
    @RegularHoursPerWeek DECIMAL(5,2) = 40.00,
    @OvertimeMultiplier DECIMAL(5,2) = 1.50,
    @DoubleTimeMultiplier DECIMAL(5,2) = 2.00,
    @WeekendMultiplier DECIMAL(5,2) = 1.50,
    @HolidayMultiplier DECIMAL(5,2) = 2.00,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].OvertimePolicies (
        Id, PolicyName, RegularHoursPerDay, RegularHoursPerWeek,
        OvertimeMultiplier, DoubleTimeMultiplier, WeekendMultiplier,
        HolidayMultiplier, TenantId, CreatedBy
    )
    VALUES (
        @Id, @PolicyName, @RegularHoursPerDay, @RegularHoursPerWeek,
        @OvertimeMultiplier, @DoubleTimeMultiplier, @WeekendMultiplier,
        @HolidayMultiplier, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- GRIEVANCES PROCEDURES
-- =============================================

-- Get Employee Grievances
CREATE OR ALTER PROCEDURE [HRM].sp_GetEmployeeGrievances
    @EmployeeId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @Priority NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredGrievances AS (
        SELECT 
            g.Id,
            g.EmployeeId,
            g.GrievanceType,
            g.Subject,
            g.Description,
            g.Status,
            g.Priority,
            g.AssignedTo,
            g.Resolution,
            g.ResolvedAt,
            g.ResolvedBy,
            g.CreatedAt,
            g.UpdatedAt,
            g.CreatedBy,
            g.UpdatedBy,
            e.FirstName + ' ' + e.LastName as EmployeeName,
            a.FirstName + ' ' + a.LastName as AssignedToName,
            r.FirstName + ' ' + r.LastName as ResolvedByName,
            ROW_NUMBER() OVER (ORDER BY g.CreatedAt DESC) AS RowNum
        FROM [HRM].Grievances g
        INNER JOIN [HRM].Employees e ON g.EmployeeId = e.Id
        LEFT JOIN [HRM].Employees a ON g.AssignedTo = a.Id
        LEFT JOIN [HRM].Employees r ON g.ResolvedBy = r.Id
        WHERE e.TenantId = @TenantId
        AND (@EmployeeId IS NULL OR g.EmployeeId = @EmployeeId)
        AND (@Status IS NULL OR g.Status = @Status)
        AND (@Priority IS NULL OR g.Priority = @Priority)
        AND g.IsActive = 1
    )
    SELECT 
        Id,
        EmployeeId,
        GrievanceType,
        Subject,
        Description,
        Status,
        Priority,
        AssignedTo,
        Resolution,
        ResolvedAt,
        ResolvedBy,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        EmployeeName,
        AssignedToName,
        ResolvedByName
    FROM FilteredGrievances
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [HRM].Grievances g
    INNER JOIN [HRM].Employees e ON g.EmployeeId = e.Id
    WHERE e.TenantId = @TenantId
    AND (@EmployeeId IS NULL OR g.EmployeeId = @EmployeeId)
    AND (@Status IS NULL OR g.Status = @Status)
    AND (@Priority IS NULL OR g.Priority = @Priority)
    AND g.IsActive = 1;
END
GO

-- Create Grievance
CREATE OR ALTER PROCEDURE [HRM].sp_CreateGrievance
    @EmployeeId UNIQUEIDENTIFIER,
    @GrievanceType NVARCHAR(100),
    @Subject NVARCHAR(200),
    @Description NVARCHAR(4000),
    @Priority NVARCHAR(50) = 'Medium',
    @AssignedTo UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].Grievances (
        Id, EmployeeId, GrievanceType, Subject, Description, Priority,
        AssignedTo, TenantId, CreatedBy
    )
    VALUES (
        @Id, @EmployeeId, @GrievanceType, @Subject, @Description, @Priority,
        @AssignedTo, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Grievance Status
CREATE OR ALTER PROCEDURE [HRM].sp_UpdateGrievanceStatus
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Resolution NVARCHAR(4000) = NULL,
    @ResolvedBy UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [HRM].Grievances
    SET 
        Status = @Status,
        Resolution = ISNULL(@Resolution, Resolution),
        ResolvedAt = CASE WHEN @Status = 'Resolved' THEN GETUTCDATE() ELSE ResolvedAt END,
        ResolvedBy = ISNULL(@ResolvedBy, ResolvedBy),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id 
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- DISCIPLINARY ACTIONS PROCEDURES
-- =============================================

-- Get Employee Disciplinary Actions
CREATE OR ALTER PROCEDURE [HRM].sp_GetEmployeeDisciplinaryActions
    @EmployeeId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @Severity NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredActions AS (
        SELECT 
            da.Id,
            da.EmployeeId,
            da.ActionType,
            da.Subject,
            da.Description,
            da.ActionDate,
            da.Status,
            da.Severity,
            da.IssuedBy,
            da.ValidUntil,
            da.CreatedAt,
            da.UpdatedAt,
            da.CreatedBy,
            da.UpdatedBy,
            e.FirstName + ' ' + e.LastName as EmployeeName,
            i.FirstName + ' ' + i.LastName as IssuedByName,
            ROW_NUMBER() OVER (ORDER BY da.ActionDate DESC) AS RowNum
        FROM [HRM].DisciplinaryActions da
        INNER JOIN [HRM].Employees e ON da.EmployeeId = e.Id
        INNER JOIN [HRM].Employees i ON da.IssuedBy = i.Id
        WHERE e.TenantId = @TenantId
        AND (@EmployeeId IS NULL OR da.EmployeeId = @EmployeeId)
        AND (@Status IS NULL OR da.Status = @Status)
        AND (@Severity IS NULL OR da.Severity = @Severity)
        AND da.IsActive = 1
    )
    SELECT 
        Id,
        EmployeeId,
        ActionType,
        Subject,
        Description,
        ActionDate,
        Status,
        Severity,
        IssuedBy,
        ValidUntil,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        EmployeeName,
        IssuedByName
    FROM FilteredActions
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [HRM].DisciplinaryActions da
    INNER JOIN [HRM].Employees e ON da.EmployeeId = e.Id
    WHERE e.TenantId = @TenantId
    AND (@EmployeeId IS NULL OR da.EmployeeId = @EmployeeId)
    AND (@Status IS NULL OR da.Status = @Status)
    AND (@Severity IS NULL OR da.Severity = @Severity)
    AND da.IsActive = 1;
END
GO

-- Create Disciplinary Action
CREATE OR ALTER PROCEDURE [HRM].sp_CreateDisciplinaryAction
    @EmployeeId UNIQUEIDENTIFIER,
    @ActionType NVARCHAR(100),
    @Subject NVARCHAR(200),
    @Description NVARCHAR(4000),
    @ActionDate DATETIME2,
    @Severity NVARCHAR(50) = 'Medium',
    @IssuedBy UNIQUEIDENTIFIER,
    @ValidUntil DATETIME2 = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].DisciplinaryActions (
        Id, EmployeeId, ActionType, Subject, Description, ActionDate,
        Severity, IssuedBy, ValidUntil, TenantId, CreatedBy
    )
    VALUES (
        @Id, @EmployeeId, @ActionType, @Subject, @Description, @ActionDate,
        @Severity, @IssuedBy, @ValidUntil, @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- EXIT INTERVIEWS PROCEDURES
-- =============================================

-- Get Employee Exit Interviews
CREATE OR ALTER PROCEDURE [HRM].sp_GetEmployeeExitInterviews
    @EmployeeId UNIQUEIDENTIFIER = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH FilteredInterviews AS (
        SELECT 
            ei.Id,
            ei.EmployeeId,
            ei.InterviewDate,
            ei.InterviewerId,
            ei.ReasonForLeaving,
            ei.SatisfactionRating,
            ei.RecommendationRating,
            ei.Comments,
            ei.Suggestions,
            ei.CreatedAt,
            ei.UpdatedAt,
            ei.CreatedBy,
            ei.UpdatedBy,
            e.FirstName + ' ' + e.LastName as EmployeeName,
            i.FirstName + ' ' + i.LastName as InterviewerName,
            ROW_NUMBER() OVER (ORDER BY ei.InterviewDate DESC) AS RowNum
        FROM [HRM].ExitInterviews ei
        INNER JOIN [HRM].Employees e ON ei.EmployeeId = e.Id
        INNER JOIN [HRM].Employees i ON ei.InterviewerId = i.Id
        WHERE e.TenantId = @TenantId
        AND (@EmployeeId IS NULL OR ei.EmployeeId = @EmployeeId)
        AND ei.IsActive = 1
    )
    SELECT 
        Id,
        EmployeeId,
        InterviewDate,
        InterviewerId,
        ReasonForLeaving,
        SatisfactionRating,
        RecommendationRating,
        Comments,
        Suggestions,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy,
        EmployeeName,
        InterviewerName
    FROM FilteredInterviews
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize;
    
    -- Return total count
    SELECT COUNT(*) as TotalCount
    FROM [HRM].ExitInterviews ei
    INNER JOIN [HRM].Employees e ON ei.EmployeeId = e.Id
    WHERE e.TenantId = @TenantId
    AND (@EmployeeId IS NULL OR ei.EmployeeId = @EmployeeId)
    AND ei.IsActive = 1;
END
GO

-- Create Exit Interview
CREATE OR ALTER PROCEDURE [HRM].sp_CreateExitInterview
    @EmployeeId UNIQUEIDENTIFIER,
    @InterviewDate DATETIME2,
    @InterviewerId UNIQUEIDENTIFIER,
    @ReasonForLeaving NVARCHAR(100),
    @SatisfactionRating INT,
    @RecommendationRating INT,
    @Comments NVARCHAR(4000) = NULL,
    @Suggestions NVARCHAR(4000) = NULL,
    @TenantId UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [HRM].ExitInterviews (
        Id, EmployeeId, InterviewDate, InterviewerId, ReasonForLeaving,
        SatisfactionRating, RecommendationRating, Comments, Suggestions,
        TenantId, CreatedBy
    )
    VALUES (
        @Id, @EmployeeId, @InterviewDate, @InterviewerId, @ReasonForLeaving,
        @SatisfactionRating, @RecommendationRating, @Comments, @Suggestions,
        @TenantId, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- EMPLOYEE CRUD MANAGEMENT PROCEDURES
-- =============================================

-- Get Employee by ID
CREATE OR ALTER PROCEDURE [HRM].[sp_GetEmployeeById]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.Id,
        e.TenantId,
        e.CompanyId,
        e.EmployeeCode,
        e.Salutation,
        e.FirstName,
        e.LastName,
        e.PreferredName,
        e.Gender,
        e.DateOfBirth,
        e.Nationality,
        e.MaritalStatus,
        e.JobTitle,
        e.DepartmentId,
        e.ManagerId,
        e.EmploymentType,
        e.WorkEmail,
        e.PersonalEmail,
        e.WorkPhone,
        e.MobilePhone,
        e.WorkLocationId,
        e.DateHired,
        e.DateTerminated,
        e.Status,
        e.IsRemote,
        e.EmploymentContractId,
        e.IsDeleted,
        e.CreatedAt,
        e.UpdatedAt,
        e.CreatedBy,
        e.UpdatedBy,
        e.IsActive,
        m.FirstName + ' ' + m.LastName AS ManagerName
    FROM [HRM].Employees e
    LEFT JOIN [HRM].Employees m ON e.ManagerId = m.Id
    WHERE e.Id = @Id
    AND e.TenantId = @TenantId
    AND e.IsDeleted = 0;
END
GO

-- Get All Employees
CREATE OR ALTER PROCEDURE [HRM].[sp_GetAllEmployees]
    @TenantId UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @SearchPattern NVARCHAR(202) = '%' + ISNULL(@SearchTerm, '') + '%';
    
    SELECT 
        e.Id,
        e.CompanyId,
        e.EmployeeCode,
        e.Salutation,
        e.FirstName,
        e.LastName,
        e.PreferredName,
        e.Gender,
        e.DateOfBirth,
        e.Nationality,
        e.MaritalStatus,
        e.JobTitle,
        e.DepartmentId,
        e.ManagerId,
        e.EmploymentType,
        e.WorkEmail,
        e.PersonalEmail,
        e.WorkPhone,
        e.MobilePhone,
        e.WorkLocationId,
        e.DateHired,
        e.DateTerminated,
        e.Status,
        e.IsRemote,
        e.EmploymentContractId,
        e.CreatedAt,
        e.UpdatedAt,
        m.FirstName + ' ' + m.LastName AS ManagerName
    FROM [HRM].Employees e
    LEFT JOIN [HRM].Employees m ON e.ManagerId = m.Id
    WHERE e.TenantId = @TenantId
    AND e.IsDeleted = 0
    AND (@CompanyId IS NULL OR e.CompanyId = @CompanyId)
    AND (@Status IS NULL OR e.Status = @Status)
    AND (@SearchTerm IS NULL OR 
         e.FirstName LIKE @SearchPattern OR 
         e.LastName LIKE @SearchPattern OR 
         e.EmployeeCode LIKE @SearchPattern OR
         e.WorkEmail LIKE @SearchPattern)
    ORDER BY e.LastName, e.FirstName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM [HRM].Employees e
    WHERE e.TenantId = @TenantId
    AND e.IsDeleted = 0
    AND (@CompanyId IS NULL OR e.CompanyId = @CompanyId)
    AND (@Status IS NULL OR e.Status = @Status)
    AND (@SearchTerm IS NULL OR 
         e.FirstName LIKE @SearchPattern OR 
         e.LastName LIKE @SearchPattern OR 
         e.EmployeeCode LIKE @SearchPattern OR
         e.WorkEmail LIKE @SearchPattern);
END
GO

-- Get Employee by Code
CREATE OR ALTER PROCEDURE [HRM].[sp_GetEmployeeByCode]
    @EmployeeCode NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.Id,
        e.TenantId,
        e.CompanyId,
        e.EmployeeCode,
        e.Salutation,
        e.FirstName,
        e.LastName,
        e.PreferredName,
        e.Gender,
        e.DateOfBirth,
        e.Nationality,
        e.MaritalStatus,
        e.JobTitle,
        e.DepartmentId,
        e.ManagerId,
        e.EmploymentType,
        e.WorkEmail,
        e.PersonalEmail,
        e.WorkPhone,
        e.MobilePhone,
        e.WorkLocationId,
        e.DateHired,
        e.DateTerminated,
        e.Status,
        e.IsRemote,
        e.EmploymentContractId,
        e.IsDeleted,
        e.CreatedAt,
        e.UpdatedAt,
        e.CreatedBy,
        e.UpdatedBy,
        e.IsActive
    FROM [HRM].Employees e
    WHERE e.EmployeeCode = @EmployeeCode
    AND e.TenantId = @TenantId
    AND e.IsDeleted = 0;
END
GO

-- Check if Employee Code is Unique
CREATE OR ALTER PROCEDURE [HRM].[sp_IsEmployeeCodeUnique]
    @EmployeeCode NVARCHAR(50),
    @TenantId UNIQUEIDENTIFIER,
    @ExcludeEmployeeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(*)
    FROM [HRM].Employees
    WHERE EmployeeCode = @EmployeeCode
    AND TenantId = @TenantId
    AND IsDeleted = 0
    AND (@ExcludeEmployeeId IS NULL OR Id != @ExcludeEmployeeId);
    
    SELECT CASE WHEN @Count = 0 THEN 1 ELSE 0 END AS IsUnique;
END
GO

-- Create Employee
CREATE OR ALTER PROCEDURE [HRM].[sp_CreateEmployee]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @EmployeeCode NVARCHAR(50),
    @Salutation NVARCHAR(10) = NULL,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PreferredName NVARCHAR(100) = NULL,
    @Gender NVARCHAR(10) = NULL,
    @DateOfBirth DATETIME2 = NULL,
    @Nationality NVARCHAR(100) = NULL,
    @MaritalStatus NVARCHAR(50) = NULL,
    @JobTitle NVARCHAR(100) = NULL,
    @DepartmentId UNIQUEIDENTIFIER = NULL,
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @EmploymentType NVARCHAR(50) = 'FullTime',
    @WorkEmail NVARCHAR(200) = NULL,
    @PersonalEmail NVARCHAR(200) = NULL,
    @WorkPhone NVARCHAR(20) = NULL,
    @MobilePhone NVARCHAR(20) = NULL,
    @WorkLocationId UNIQUEIDENTIFIER = NULL,
    @DateHired DATETIME2 = NULL,
    @Status NVARCHAR(50) = 'Active',
    @IsRemote BIT = 0,
    @EmploymentContractId UNIQUEIDENTIFIER = NULL,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if employee code is unique
    IF EXISTS (SELECT 1 FROM [HRM].Employees WHERE EmployeeCode = @EmployeeCode AND TenantId = @TenantId AND IsDeleted = 0)
    BEGIN
        RAISERROR('Employee code already exists.', 16, 1);
        RETURN;
    END
    
    INSERT INTO [HRM].Employees (
        Id, TenantId, CompanyId, EmployeeCode, Salutation, FirstName, LastName,
        PreferredName, Gender, DateOfBirth, Nationality, MaritalStatus,
        JobTitle, DepartmentId, ManagerId, EmploymentType,
        WorkEmail, PersonalEmail, WorkPhone, MobilePhone,
        WorkLocationId, DateHired, Status, IsRemote, EmploymentContractId,
        IsDeleted, IsActive, CreatedAt, CreatedBy
    )
    VALUES (
        @Id, @TenantId, @CompanyId, @EmployeeCode, @Salutation, @FirstName, @LastName,
        @PreferredName, @Gender, @DateOfBirth, @Nationality, @MaritalStatus,
        @JobTitle, @DepartmentId, @ManagerId, @EmploymentType,
        @WorkEmail, @PersonalEmail, @WorkPhone, @MobilePhone,
        @WorkLocationId, @DateHired, @Status, @IsRemote, @EmploymentContractId,
        0, 1, GETUTCDATE(), @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Employee
CREATE OR ALTER PROCEDURE [HRM].[sp_UpdateEmployee]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @Salutation NVARCHAR(10) = NULL,
    @PreferredName NVARCHAR(100) = NULL,
    @Gender NVARCHAR(10) = NULL,
    @DateOfBirth DATETIME2 = NULL,
    @Nationality NVARCHAR(100) = NULL,
    @MaritalStatus NVARCHAR(50) = NULL,
    @JobTitle NVARCHAR(100) = NULL,
    @DepartmentId UNIQUEIDENTIFIER = NULL,
    @ManagerId UNIQUEIDENTIFIER = NULL,
    @EmploymentType NVARCHAR(50) = NULL,
    @WorkEmail NVARCHAR(200) = NULL,
    @PersonalEmail NVARCHAR(200) = NULL,
    @WorkPhone NVARCHAR(20) = NULL,
    @MobilePhone NVARCHAR(20) = NULL,
    @WorkLocationId UNIQUEIDENTIFIER = NULL,
    @DateHired DATETIME2 = NULL,
    @DateTerminated DATETIME2 = NULL,
    @Status NVARCHAR(50) = NULL,
    @IsRemote BIT = NULL,
    @EmploymentContractId UNIQUEIDENTIFIER = NULL,
    @UpdatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate employee exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [HRM].Employees WHERE Id = @Id AND TenantId = @TenantId AND IsDeleted = 0)
    BEGIN
        RAISERROR('Employee not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    UPDATE [HRM].Employees
    SET 
        CompanyId = ISNULL(@CompanyId, CompanyId),
        Salutation = ISNULL(@Salutation, Salutation),
        PreferredName = ISNULL(@PreferredName, PreferredName),
        Gender = ISNULL(@Gender, Gender),
        DateOfBirth = ISNULL(@DateOfBirth, DateOfBirth),
        Nationality = ISNULL(@Nationality, Nationality),
        MaritalStatus = ISNULL(@MaritalStatus, MaritalStatus),
        JobTitle = ISNULL(@JobTitle, JobTitle),
        DepartmentId = ISNULL(@DepartmentId, DepartmentId),
        ManagerId = ISNULL(@ManagerId, ManagerId),
        EmploymentType = ISNULL(@EmploymentType, EmploymentType),
        WorkEmail = ISNULL(@WorkEmail, WorkEmail),
        PersonalEmail = ISNULL(@PersonalEmail, PersonalEmail),
        WorkPhone = ISNULL(@WorkPhone, WorkPhone),
        MobilePhone = ISNULL(@MobilePhone, MobilePhone),
        WorkLocationId = ISNULL(@WorkLocationId, WorkLocationId),
        DateHired = ISNULL(@DateHired, DateHired),
        DateTerminated = ISNULL(@DateTerminated, DateTerminated),
        Status = ISNULL(@Status, Status),
        IsRemote = ISNULL(@IsRemote, IsRemote),
        EmploymentContractId = ISNULL(@EmploymentContractId, EmploymentContractId),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = ISNULL(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id
    AND TenantId = @TenantId
    AND IsDeleted = 0;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Delete Employee (Soft Delete)
CREATE OR ALTER PROCEDURE [HRM].[sp_DeleteEmployee]
    @Id UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate employee exists and belongs to tenant
    IF NOT EXISTS (SELECT 1 FROM [HRM].Employees WHERE Id = @Id AND TenantId = @TenantId)
    BEGIN
        RAISERROR('Employee not found or does not belong to the tenant.', 16, 1);
        RETURN;
    END
    
    -- Soft delete by setting IsDeleted = 1
    UPDATE [HRM].Employees
    SET 
        IsDeleted = 1,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id
    AND TenantId = @TenantId;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

PRINT 'HRM Module stored procedures created successfully!'