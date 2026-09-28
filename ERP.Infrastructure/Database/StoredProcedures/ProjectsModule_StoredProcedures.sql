-- =============================================
-- Projects Module Stored Procedures
-- Multi-tenant SaaS ERP System
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Projects')
BEGIN
    EXEC('CREATE SCHEMA [Projects]')
END
GO

-- =============================================
-- PROJECT MANAGEMENT PROCEDURES
-- =============================================

-- Get Project by ID
CREATE OR ALTER PROCEDURE [Projects].[sp_GetProjectById]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        p.CompanyId,
        p.TenantId,
        p.ProjectCode,
        p.ProjectName,
        p.Description,
        p.ClientId,
        p.ProjectManagerId,
        p.StartDate,
        p.EndDate,
        p.ActualStartDate,
        p.ActualEndDate,
        p.Status,
        p.Priority,
        p.Budget,
        p.ActualCost,
        p.Progress,
        p.Notes,
        p.CreatedAt,
        p.UpdatedAt,
        p.CreatedBy,
        p.UpdatedBy,
        c.CompanyName as ClientName,
        e.FirstName + ' ' + e.LastName as ProjectManagerName
    FROM dbo.Projects p
    LEFT JOIN [Core].[Companies] c ON p.ClientId = c.Id
    LEFT JOIN [HRM].[Employees] e ON p.ProjectManagerId = e.Id
    WHERE p.Id = @Id
    AND p.TenantId = @TenantID;
END
GO

-- Get All Projects
CREATE OR ALTER PROCEDURE [Projects].[sp_GetAllProjects]
    @TenantID UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        p.Id,
        p.ProjectCode,
        p.ProjectName,
        p.Description,
        p.ClientId,
        p.ProjectManagerId,
        p.StartDate,
        p.EndDate,
        p.Status,
        p.Priority,
        p.Budget,
        p.ActualCost,
        p.Progress,
        p.CreatedAt,
        c.CompanyName as ClientName,
        e.FirstName + ' ' + e.LastName as ProjectManagerName
    FROM dbo.Projects p
    LEFT JOIN [Core].[Companies] c ON p.ClientId = c.Id
    LEFT JOIN [HRM].[Employees] e ON p.ProjectManagerId = e.Id
    WHERE p.TenantId = @TenantID
    AND (@CompanyId IS NULL OR p.CompanyId = @CompanyId)
    AND (@Status IS NULL OR p.Status = @Status)
    ORDER BY p.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    SELECT COUNT(*) as TotalCount
    FROM dbo.Projects p
    WHERE p.TenantId = @TenantID
    AND (@CompanyId IS NULL OR p.CompanyId = @CompanyId)
    AND (@Status IS NULL OR p.Status = @Status);
END
GO

-- Create Project
CREATE OR ALTER PROCEDURE [Projects].[sp_CreateProject]
    @Id UNIQUEIDENTIFIER,
    @CompanyId UNIQUEIDENTIFIER = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @ProjectCode NVARCHAR(50),
    @ProjectName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @ClientId UNIQUEIDENTIFIER = NULL,
    @ProjectManagerId UNIQUEIDENTIFIER = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @Status NVARCHAR(50) = 'Planning',
    @Priority NVARCHAR(50) = 'Medium',
    @Budget DECIMAL(18,2) = 0,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.Projects (
        Id, CompanyId, TenantId, ProjectCode, ProjectName, Description,
        ClientId, ProjectManagerId, StartDate, EndDate, Status, Priority,
        Budget, ActualCost, Progress, Notes, CreatedBy
    )
    VALUES (
        @Id, @CompanyId, @TenantID, @ProjectCode, @ProjectName, @Description,
        @ClientId, @ProjectManagerId, @StartDate, @EndDate, @Status, @Priority,
        @Budget, 0, 0, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Project
CREATE OR ALTER PROCEDURE [Projects].[sp_UpdateProject]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ProjectName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @ClientId UNIQUEIDENTIFIER = NULL,
    @ProjectManagerId UNIQUEIDENTIFIER = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @ActualStartDate DATETIME2 = NULL,
    @ActualEndDate DATETIME2 = NULL,
    @Status NVARCHAR(50),
    @Priority NVARCHAR(50),
    @Budget DECIMAL(18,2),
    @ActualCost DECIMAL(18,2),
    @Progress DECIMAL(5,2),
    @Notes NVARCHAR(MAX) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Projects
    SET 
        ProjectName = @ProjectName,
        Description = @Description,
        ClientId = @ClientId,
        ProjectManagerId = @ProjectManagerId,
        StartDate = @StartDate,
        EndDate = @EndDate,
        ActualStartDate = @ActualStartDate,
        ActualEndDate = @ActualEndDate,
        Status = @Status,
        Priority = @Priority,
        Budget = @Budget,
        ActualCost = @ActualCost,
        Progress = @Progress,
        Notes = @Notes,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND TenantId = @TenantID;
END
GO

-- =============================================
-- PROJECT TASK MANAGEMENT PROCEDURES
-- =============================================

-- Get Project Tasks by Project ID
CREATE OR ALTER PROCEDURE [Projects].[sp_GetProjectTasksByProjectId]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pt.Id,
        pt.ProjectId,
        pt.TaskName,
        pt.Description,
        pt.AssignedToId,
        pt.ParentTaskId,
        pt.StartDate,
        pt.DueDate,
        pt.CompletedDate,
        pt.Status,
        pt.Priority,
        pt.EstimatedHours,
        pt.ActualHours,
        pt.Progress,
        pt.Notes,
        pt.CreatedAt,
        pt.UpdatedAt,
        e.FirstName + ' ' + e.LastName as AssignedToName
    FROM dbo.ProjectTasks pt
    INNER JOIN dbo.Projects p ON pt.ProjectId = p.Id
    LEFT JOIN [HRM].[Employees] e ON pt.AssignedToId = e.Id
    WHERE pt.ProjectId = @ProjectId
    AND p.TenantId = @TenantID
    AND (@Status IS NULL OR pt.Status = @Status)
    ORDER BY pt.Priority DESC, pt.DueDate;
END
GO

-- Create Project Task
CREATE OR ALTER PROCEDURE [Projects].[sp_CreateProjectTask]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ProjectId UNIQUEIDENTIFIER,
    @TaskName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @AssignedToId UNIQUEIDENTIFIER = NULL,
    @ParentTaskId UNIQUEIDENTIFIER = NULL,
    @StartDate DATETIME2 = NULL,
    @DueDate DATETIME2 = NULL,
    @Status NVARCHAR(50) = 'NotStarted',
    @Priority NVARCHAR(50) = 'Medium',
    @EstimatedHours DECIMAL(18,2) = 0,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.ProjectTasks (
        Id, ProjectId, TaskName, Description, AssignedToId, ParentTaskId,
        StartDate, DueDate, Status, Priority, EstimatedHours, ActualHours,
        Progress, Notes, CreatedBy
    )
    VALUES (
        @Id, @ProjectId, @TaskName, @Description, @AssignedToId, @ParentTaskId,
        @StartDate, @DueDate, @Status, @Priority, @EstimatedHours, 0,
        0, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- Update Project Task Status
CREATE OR ALTER PROCEDURE [Projects].[sp_UpdateProjectTaskStatus]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @CompletedDate DATETIME2 = NULL,
    @ActualHours DECIMAL(18,2) = NULL,
    @Progress DECIMAL(5,2) = NULL,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.ProjectTasks
    SET 
        Status = @Status,
        CompletedDate = CASE WHEN @Status = 'Completed' THEN ISNULL(@CompletedDate, GETUTCDATE()) ELSE CompletedDate END,
        ActualHours = ISNULL(@ActualHours, ActualHours),
        Progress = ISNULL(@Progress, Progress),
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = @UpdatedBy
    WHERE Id = @Id
    AND EXISTS (
        SELECT 1 FROM dbo.Projects p
        WHERE p.Id = ProjectTasks.ProjectId
        AND p.TenantId = @TenantID
    );
END
GO

-- =============================================
-- PROJECT RESOURCE MANAGEMENT PROCEDURES
-- =============================================

-- Get Project Resources by Project ID
CREATE OR ALTER PROCEDURE [Projects].[sp_GetProjectResourcesByProjectId]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pr.Id,
        pr.ProjectId,
        pr.EmployeeId,
        pr.ResourceType,
        pr.ResourceName,
        pr.AllocatedHours,
        pr.ActualHours,
        pr.HourlyRate,
        pr.Status,
        pr.Notes,
        pr.CreatedAt,
        pr.UpdatedAt,
        e.FirstName + ' ' + e.LastName as EmployeeName
    FROM dbo.ProjectResources pr
    INNER JOIN dbo.Projects p ON pr.ProjectId = p.Id
    LEFT JOIN [HRM].[Employees] e ON pr.EmployeeId = e.Id
    WHERE pr.ProjectId = @ProjectId
    AND p.TenantId = @TenantID
    ORDER BY pr.ResourceType, pr.ResourceName;
END
GO

-- Assign Resource to Project
CREATE OR ALTER PROCEDURE [Projects].[sp_AssignResourceToProject]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ProjectId UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER = NULL,
    @ResourceType NVARCHAR(50),
    @ResourceName NVARCHAR(200),
    @AllocatedHours DECIMAL(18,2),
    @HourlyRate DECIMAL(18,2) = 0,
    @Status NVARCHAR(50) = 'Active',
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.ProjectResources (
        Id, ProjectId, EmployeeId, ResourceType, ResourceName,
        AllocatedHours, ActualHours, HourlyRate, Status, Notes, CreatedBy
    )
    VALUES (
        @Id, @ProjectId, @EmployeeId, @ResourceType, @ResourceName,
        @AllocatedHours, 0, @HourlyRate, @Status, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- PROJECT MILESTONE MANAGEMENT PROCEDURES
-- =============================================

-- Get Project Milestones by Project ID
CREATE OR ALTER PROCEDURE [Projects].[sp_GetProjectMilestonesByProjectId]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pm.Id,
        pm.ProjectId,
        pm.MilestoneName,
        pm.Description,
        pm.TargetDate,
        pm.ActualDate,
        pm.Status,
        pm.Notes,
        pm.CreatedAt,
        pm.UpdatedAt
    FROM dbo.ProjectMilestones pm
    INNER JOIN dbo.Projects p ON pm.ProjectId = p.Id
    WHERE pm.ProjectId = @ProjectId
    AND p.TenantId = @TenantID
    ORDER BY pm.TargetDate;
END
GO

-- Create Project Milestone
CREATE OR ALTER PROCEDURE [Projects].[sp_CreateProjectMilestone]
    @Id UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER,
    @ProjectId UNIQUEIDENTIFIER,
    @MilestoneName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @TargetDate DATETIME2 = NULL,
    @Status NVARCHAR(50) = 'Pending',
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO dbo.ProjectMilestones (
        Id, ProjectId, MilestoneName, Description, TargetDate, Status, Notes, CreatedBy
    )
    VALUES (
        @Id, @ProjectId, @MilestoneName, @Description, @TargetDate, @Status, @Notes, @CreatedBy
    );
    
    SELECT @Id as Id;
END
GO

-- =============================================
-- PROJECT DOCUMENT MANAGEMENT PROCEDURES
-- =============================================

-- Get Project Documents by Project ID
CREATE OR ALTER PROCEDURE [Projects].[sp_GetProjectDocumentsByProjectId]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pd.Id,
        pd.ProjectId,
        pd.DocumentName,
        pd.DocumentType,
        pd.FilePath,
        pd.Description,
        pd.FileSize,
        pd.Version,
        pd.IsPublic,
        pd.CreatedAt,
        pd.UpdatedAt,
        pd.CreatedBy
    FROM dbo.ProjectDocuments pd
    WHERE pd.ProjectId = @ProjectId
      AND pd.TenantId = @TenantID
    ORDER BY pd.DocumentType, pd.DocumentName;
END
GO

