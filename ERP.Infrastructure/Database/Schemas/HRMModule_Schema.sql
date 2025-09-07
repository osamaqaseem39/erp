-- =============================================
-- HRM MODULE DATABASE SCHEMA
-- Multi-tenant SaaS ERP System
-- =============================================

-- Create HRM Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'HRM')
BEGIN
    EXEC('CREATE SCHEMA [HRM]')
END
GO

-- =============================================
-- EMPLOYEES TABLE
-- =============================================
CREATE TABLE [HRM].[Employees] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CompanyId] UNIQUEIDENTIFIER,
    [EmployeeCode] NVARCHAR(50) NOT NULL,
    [Salutation] NVARCHAR(10),
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [PreferredName] NVARCHAR(100),
    [Gender] NVARCHAR(5),
    [DateOfBirth] DATETIME2,
    [NationalId] NVARCHAR(100),
    [Nationality] NVARCHAR(100),
    [MaritalStatus] NVARCHAR(50),
    [JobTitle] NVARCHAR(150),
    [DepartmentId] UNIQUEIDENTIFIER,
    [ManagerId] UNIQUEIDENTIFIER,
    [EmploymentType] NVARCHAR(50),
    [WorkEmail] NVARCHAR(255),
    [PersonalEmail] NVARCHAR(255),
    [WorkPhone] NVARCHAR(50),
    [MobilePhone] NVARCHAR(50),
    [WorkLocationId] UNIQUEIDENTIFIER,
    [DateHired] DATETIME2,
    [DateTerminated] DATETIME2,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [IsRemote] BIT NOT NULL,
    [EmploymentContractId] UNIQUEIDENTIFIER,
    [IsDeleted] BIT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([ManagerId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- EMPLOYEE ADDRESSES TABLE
-- =============================================
CREATE TABLE [HRM].[EmployeeAddresses] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [AddressType] NVARCHAR(50) NOT NULL,
    [AddressLine1] NVARCHAR(200) NOT NULL,
    [AddressLine2] NVARCHAR(200),
    [City] NVARCHAR(100) NOT NULL,
    [State] NVARCHAR(100),
    [CountryCode] NVARCHAR(10) NOT NULL,
    [PostalCode] NVARCHAR(20),
    [IsPrimary] BIT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- EMPLOYEE DOCUMENTS TABLE
-- =============================================
CREATE TABLE [HRM].[EmployeeDocuments] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [DocumentType] NVARCHAR(50) NOT NULL,
    [FileUrl] NVARCHAR(1000) NOT NULL,
    [UploadedBy] UNIQUEIDENTIFIER NOT NULL,
    [UploadedAt] DATETIME2 NOT NULL,
    [ExpiryDate] DATETIME2,
    [IsSensitive] BIT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- EMPLOYMENT CONTRACTS TABLE
-- =============================================
CREATE TABLE [HRM].[EmploymentContracts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [ContractType] NVARCHAR(50) NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2,
    [WorkingHoursPerWeek] DECIMAL(5,2) NOT NULL,
    [BaseSalary] DECIMAL(18,2) NOT NULL,
    [Currency] NVARCHAR(10) NOT NULL,
    [PayFrequency] NVARCHAR(20) NOT NULL,
    [NoticePeriodDays] INT NOT NULL,
    [ProbationPeriodDays] INT NOT NULL,
    [ContractDocumentUrl] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- PAYROLLS TABLE
-- =============================================
CREATE TABLE [HRM].[Payrolls] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL,
    [PayrollPeriodStart] DATETIME2 NOT NULL,
    [PayrollPeriodEnd] DATETIME2 NOT NULL,
    [PayrollRunDate] DATETIME2 NOT NULL,
    [Currency] NVARCHAR(10) NOT NULL,
    [TotalGross] DECIMAL(18,2) NOT NULL,
    [TotalDeductions] DECIMAL(18,2) NOT NULL,
    [TotalNet] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    [PostedBy] UNIQUEIDENTIFIER,
    [PostedAt] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- PAYROLL LINES TABLE
-- =============================================
CREATE TABLE [HRM].[PayrollLines] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PayrollId] UNIQUEIDENTIFIER NOT NULL,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [Earnings] DECIMAL(18,2) NOT NULL,
    [Allowances] DECIMAL(18,2) NOT NULL,
    [Deductions] DECIMAL(18,2) NOT NULL,
    [TaxAmount] DECIMAL(18,2) NOT NULL,
    [NetPay] DECIMAL(18,2) NOT NULL,
    [BankAccountId] UNIQUEIDENTIFIER,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([PayrollId]) REFERENCES [HRM].[Payrolls]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- PAYROLL COMPONENTS TABLE
-- =============================================
CREATE TABLE [HRM].[PayrollComponents] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL,
    [CalculationType] NVARCHAR(50) NOT NULL,
    [Value] DECIMAL(18,6) NOT NULL,
    [IsTaxable] BIT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- TIME ENTRIES TABLE
-- =============================================
CREATE TABLE [HRM].[TimeEntries] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [ClockIn] DATETIMEOFFSET NOT NULL,
    [ClockOut] DATETIMEOFFSET,
    [DurationMinutes] INT,
    [Device] NVARCHAR(100),
    [Location] NVARCHAR(255),
    [Source] NVARCHAR(50) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- SHIFTS TABLE
-- =============================================
CREATE TABLE [HRM].[Shifts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ShiftName] NVARCHAR(100) NOT NULL,
    [StartTime] TIME NOT NULL,
    [EndTime] TIME NOT NULL,
    [IsOvernight] BIT NOT NULL,
    [GracePeriodMinutes] INT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- LEAVE TYPES TABLE
-- =============================================
CREATE TABLE [HRM].[LeaveTypes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Code] NVARCHAR(50) NOT NULL,
    [IsPaid] BIT NOT NULL,
    [MaxAllowedPerYear] DECIMAL(6,2) NOT NULL,
    [RequiresApproval] BIT NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- LEAVE REQUESTS TABLE
-- =============================================
CREATE TABLE [HRM].[LeaveRequests] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [LeaveTypeId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [TotalDays] DECIMAL(6,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [RequestedAt] DATETIME2 NOT NULL,
    [ApprovedAt] DATETIME2,
    [ApprovedBy] UNIQUEIDENTIFIER,
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([LeaveTypeId]) REFERENCES [HRM].[LeaveTypes]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- LEAVE BALANCES TABLE
-- =============================================
CREATE TABLE [HRM].[LeaveBalances] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [LeaveTypeId] UNIQUEIDENTIFIER NOT NULL,
    [Accrued] DECIMAL(6,2) NOT NULL,
    [Taken] DECIMAL(6,2) NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([LeaveTypeId]) REFERENCES [HRM].[LeaveTypes]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- PERFORMANCE REVIEWS TABLE
-- =============================================
CREATE TABLE [HRM].[PerformanceReviews] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [ReviewerId] UNIQUEIDENTIFIER NOT NULL,
    [PeriodStart] DATETIME2 NOT NULL,
    [PeriodEnd] DATETIME2 NOT NULL,
    [ReviewDate] DATETIME2 NOT NULL,
    [Rating] DECIMAL(3,2) NOT NULL,
    [Comments] NVARCHAR(4000),
    [GoalsJson] NVARCHAR(4000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ReviewerId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- JOB OPENINGS TABLE
-- =============================================
CREATE TABLE [HRM].[JobOpenings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [Department] NVARCHAR(100) NOT NULL,
    [Location] NVARCHAR(200) NOT NULL,
    [EmploymentType] NVARCHAR(50) NOT NULL,
    [MinSalary] DECIMAL(18,2),
    [MaxSalary] DECIMAL(18,2),
    [Currency] NVARCHAR(10) NOT NULL,
    [PostedDate] DATETIME2 NOT NULL,
    [ClosingDate] DATETIME2,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Open',
    [PostedBy] UNIQUEIDENTIFIER NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- CANDIDATES TABLE
-- =============================================
CREATE TABLE [HRM].[Candidates] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [Phone] NVARCHAR(50),
    [CurrentPosition] NVARCHAR(150),
    [CurrentCompany] NVARCHAR(200),
    [YearsOfExperience] INT,
    [Skills] NVARCHAR(1000),
    [Education] NVARCHAR(1000),
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- JOB APPLICATIONS TABLE
-- =============================================
CREATE TABLE [HRM].[JobApplications] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [JobOpeningId] UNIQUEIDENTIFIER NOT NULL,
    [CandidateId] UNIQUEIDENTIFIER NOT NULL,
    [ResumeUrl] NVARCHAR(1000) NOT NULL,
    [AppliedAt] DATETIME2 NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Applied',
    [Notes] NVARCHAR(1000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([JobOpeningId]) REFERENCES [HRM].[JobOpenings]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([CandidateId]) REFERENCES [HRM].[Candidates]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- INTERVIEWS TABLE
-- =============================================
CREATE TABLE [HRM].[Interviews] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [JobApplicationId] UNIQUEIDENTIFIER NOT NULL,
    [InterviewerId] UNIQUEIDENTIFIER NOT NULL,
    [ScheduledAt] DATETIME2 NOT NULL,
    [InterviewType] NVARCHAR(50) NOT NULL,
    [Location] NVARCHAR(200),
    [Notes] NVARCHAR(1000),
    [Rating] NVARCHAR(50),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([JobApplicationId]) REFERENCES [HRM].[JobApplications]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([InterviewerId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE NO ACTION
);
GO

-- =============================================
-- TRAININGS TABLE
-- =============================================
CREATE TABLE [HRM].[Trainings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [Trainer] NVARCHAR(200),
    [Location] NVARCHAR(200),
    [TrainingType] NVARCHAR(50) NOT NULL,
    [MaxParticipants] INT NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    [ParticipantsJson] NVARCHAR(4000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- HR SETTINGS TABLE
-- =============================================
CREATE TABLE [HRM].[HRSettings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [DefaultPayFrequency] NVARCHAR(50) NOT NULL,
    [WorkingDaysJson] NVARCHAR(1000),
    [OvertimePolicyJson] NVARCHAR(2000),
    [LeaveAccrualPolicyJson] NVARCHAR(2000),
    [TaxSettingsJson] NVARCHAR(2000),
    [ComplianceSettingsJson] NVARCHAR(2000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1
);
GO

-- =============================================
-- INDEXES FOR HRM MODULE
-- =============================================
CREATE INDEX IX_Employees_TenantId_EmployeeCode ON [HRM].[Employees]([TenantId], [EmployeeCode]);
CREATE INDEX IX_Employees_TenantId_WorkEmail ON [HRM].[Employees]([TenantId], [WorkEmail]);
CREATE INDEX IX_EmployeeAddresses_EmployeeId ON [HRM].[EmployeeAddresses]([EmployeeId]);
CREATE INDEX IX_EmployeeDocuments_EmployeeId ON [HRM].[EmployeeDocuments]([EmployeeId]);
CREATE INDEX IX_EmploymentContracts_EmployeeId ON [HRM].[EmploymentContracts]([EmployeeId]);
CREATE INDEX IX_PayrollLines_EmployeeId ON [HRM].[PayrollLines]([EmployeeId]);
CREATE INDEX IX_PayrollLines_PayrollId ON [HRM].[PayrollLines]([PayrollId]);
CREATE INDEX IX_TimeEntries_EmployeeId_ClockIn ON [HRM].[TimeEntries]([EmployeeId], [ClockIn]);
CREATE INDEX IX_LeaveRequests_EmployeeId ON [HRM].[LeaveRequests]([EmployeeId]);
CREATE INDEX IX_LeaveRequests_LeaveTypeId ON [HRM].[LeaveRequests]([LeaveTypeId]);
CREATE INDEX IX_LeaveBalances_EmployeeId ON [HRM].[LeaveBalances]([EmployeeId]);
CREATE INDEX IX_LeaveBalances_LeaveTypeId ON [HRM].[LeaveBalances]([LeaveTypeId]);
CREATE INDEX IX_PerformanceReviews_EmployeeId ON [HRM].[PerformanceReviews]([EmployeeId]);
CREATE INDEX IX_PerformanceReviews_ReviewerId ON [HRM].[PerformanceReviews]([ReviewerId]);
CREATE INDEX IX_JobApplications_CandidateId ON [HRM].[JobApplications]([CandidateId]);
CREATE INDEX IX_JobApplications_JobOpeningId ON [HRM].[JobApplications]([JobOpeningId]);
CREATE INDEX IX_Interviews_InterviewerId ON [HRM].[Interviews]([InterviewerId]);
CREATE INDEX IX_Interviews_JobApplicationId ON [HRM].[Interviews]([JobApplicationId]);

-- =============================================
-- EMPLOYEE TYPES TABLE
-- =============================================
CREATE TABLE [HRM].[EmployeeTypes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [TypeName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- JOB CATEGORIES TABLE
-- =============================================
CREATE TABLE [HRM].[JobCategories] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [CategoryName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- WORK LOCATIONS TABLE
-- =============================================
CREATE TABLE [HRM].[WorkLocations] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [LocationName] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [State] NVARCHAR(100),
    [Country] NVARCHAR(100),
    [ZipCode] NVARCHAR(20),
    [Phone] NVARCHAR(20),
    [Email] NVARCHAR(200),
    [ManagerId] UNIQUEIDENTIFIER,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- OVERTIME POLICIES TABLE
-- =============================================
CREATE TABLE [HRM].[OvertimePolicies] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [PolicyName] NVARCHAR(100) NOT NULL,
    [RegularHoursPerDay] DECIMAL(5,2) NOT NULL DEFAULT 8.00,
    [RegularHoursPerWeek] DECIMAL(5,2) NOT NULL DEFAULT 40.00,
    [OvertimeMultiplier] DECIMAL(5,2) NOT NULL DEFAULT 1.50,
    [DoubleTimeMultiplier] DECIMAL(5,2) NOT NULL DEFAULT 2.00,
    [WeekendMultiplier] DECIMAL(5,2) NOT NULL DEFAULT 1.50,
    [HolidayMultiplier] DECIMAL(5,2) NOT NULL DEFAULT 2.00,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100)
);
GO

-- =============================================
-- GRIEVANCES TABLE
-- =============================================
CREATE TABLE [HRM].[Grievances] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [GrievanceType] NVARCHAR(100) NOT NULL,
    [Subject] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
    [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    [AssignedTo] UNIQUEIDENTIFIER,
    [Resolution] NVARCHAR(4000),
    [ResolvedAt] DATETIME2,
    [ResolvedBy] UNIQUEIDENTIFIER,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- DISCIPLINARY ACTIONS TABLE
-- =============================================
CREATE TABLE [HRM].[DisciplinaryActions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [ActionType] NVARCHAR(100) NOT NULL,
    [Subject] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [ActionDate] DATETIME2 NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [Severity] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    [IssuedBy] UNIQUEIDENTIFIER NOT NULL,
    [ValidUntil] DATETIME2,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- EXIT INTERVIEWS TABLE
-- =============================================
CREATE TABLE [HRM].[ExitInterviews] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [InterviewDate] DATETIME2 NOT NULL,
    [InterviewerId] UNIQUEIDENTIFIER NOT NULL,
    [ReasonForLeaving] NVARCHAR(100) NOT NULL,
    [SatisfactionRating] INT NOT NULL, -- 1-5 scale
    [RecommendationRating] INT NOT NULL, -- 1-5 scale
    [Comments] NVARCHAR(4000),
    [Suggestions] NVARCHAR(4000),
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedBy] NVARCHAR(100),
    [UpdatedBy] NVARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    FOREIGN KEY ([EmployeeId]) REFERENCES [HRM].[Employees]([Id]) ON DELETE CASCADE
);
GO

-- =============================================
-- ADDITIONAL INDEXES FOR HRM MODULE
-- =============================================
CREATE INDEX IX_EmployeeTypes_TenantId ON [HRM].[EmployeeTypes]([TenantId]);
CREATE INDEX IX_JobCategories_TenantId ON [HRM].[JobCategories]([TenantId]);
CREATE INDEX IX_WorkLocations_TenantId ON [HRM].[WorkLocations]([TenantId]);
CREATE INDEX IX_OvertimePolicies_TenantId ON [HRM].[OvertimePolicies]([TenantId]);
CREATE INDEX IX_Grievances_EmployeeId ON [HRM].[Grievances]([EmployeeId]);
CREATE INDEX IX_Grievances_Status ON [HRM].[Grievances]([Status]);
CREATE INDEX IX_DisciplinaryActions_EmployeeId ON [HRM].[DisciplinaryActions]([EmployeeId]);
CREATE INDEX IX_DisciplinaryActions_Status ON [HRM].[DisciplinaryActions]([Status]);
CREATE INDEX IX_ExitInterviews_EmployeeId ON [HRM].[ExitInterviews]([EmployeeId]);

PRINT 'HRM Module schema created successfully!'