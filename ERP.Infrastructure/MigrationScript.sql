IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Candidates] (
    [Id] uniqueidentifier NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [Phone] nvarchar(50) NULL,
    [CurrentPosition] nvarchar(150) NULL,
    [CurrentCompany] nvarchar(200) NULL,
    [YearsOfExperience] int NULL,
    [Skills] nvarchar(1000) NULL,
    [Education] nvarchar(1000) NULL,
    [Notes] nvarchar(1000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Candidates] PRIMARY KEY ([Id])
);

CREATE TABLE [Employees] (
    [Id] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NULL,
    [EmployeeCode] nvarchar(50) NOT NULL,
    [Salutation] nvarchar(10) NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [PreferredName] nvarchar(100) NULL,
    [Gender] nvarchar(5) NULL,
    [DateOfBirth] datetime2 NULL,
    [NationalId] nvarchar(100) NULL,
    [Nationality] nvarchar(100) NULL,
    [MaritalStatus] nvarchar(50) NULL,
    [JobTitle] nvarchar(150) NULL,
    [DepartmentId] uniqueidentifier NULL,
    [ManagerId] uniqueidentifier NULL,
    [EmploymentType] nvarchar(50) NULL,
    [WorkEmail] nvarchar(255) NULL,
    [PersonalEmail] nvarchar(255) NULL,
    [WorkPhone] nvarchar(50) NULL,
    [MobilePhone] nvarchar(50) NULL,
    [WorkLocationId] uniqueidentifier NULL,
    [DateHired] datetime2 NULL,
    [DateTerminated] datetime2 NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Active',
    [IsRemote] bit NOT NULL,
    [EmploymentContractId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Employees_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [HRSettings] (
    [Id] uniqueidentifier NOT NULL,
    [DefaultPayFrequency] nvarchar(50) NOT NULL,
    [WorkingDaysJson] nvarchar(1000) NULL,
    [OvertimePolicyJson] nvarchar(2000) NULL,
    [LeaveAccrualPolicyJson] nvarchar(2000) NULL,
    [TaxSettingsJson] nvarchar(2000) NULL,
    [ComplianceSettingsJson] nvarchar(2000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_HRSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [JobOpenings] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(4000) NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [Location] nvarchar(200) NOT NULL,
    [EmploymentType] nvarchar(50) NOT NULL,
    [MinSalary] decimal(18,2) NULL,
    [MaxSalary] decimal(18,2) NULL,
    [Currency] nvarchar(10) NOT NULL,
    [PostedDate] datetime2 NOT NULL,
    [ClosingDate] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Open',
    [PostedBy] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_JobOpenings] PRIMARY KEY ([Id])
);

CREATE TABLE [LeaveTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [IsPaid] bit NOT NULL,
    [MaxAllowedPerYear] decimal(6,2) NOT NULL,
    [RequiresApproval] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_LeaveTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [PayrollComponents] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [CalculationType] nvarchar(50) NOT NULL,
    [Value] decimal(18,6) NOT NULL,
    [IsTaxable] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PayrollComponents] PRIMARY KEY ([Id])
);

CREATE TABLE [Payrolls] (
    [Id] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [PayrollPeriodStart] datetime2 NOT NULL,
    [PayrollPeriodEnd] datetime2 NOT NULL,
    [PayrollRunDate] datetime2 NOT NULL,
    [Currency] nvarchar(10) NOT NULL,
    [TotalGross] decimal(18,2) NOT NULL,
    [TotalDeductions] decimal(18,2) NOT NULL,
    [TotalNet] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [PostedBy] uniqueidentifier NULL,
    [PostedAt] datetime2 NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Payrolls] PRIMARY KEY ([Id])
);

CREATE TABLE [Permissions] (
    [Id] uniqueidentifier NOT NULL,
    [ModuleName] nvarchar(100) NOT NULL,
    [ActionName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

CREATE TABLE [Plans] (
    [Id] uniqueidentifier NOT NULL,
    [PlanName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [MonthlyPrice] decimal(18,2) NOT NULL,
    [YearlyPrice] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [MaxUsers] int NOT NULL,
    [MaxStorage] int NOT NULL,
    [FeaturesJSON] nvarchar(4000) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Plans] PRIMARY KEY ([Id])
);

CREATE TABLE [Shifts] (
    [Id] uniqueidentifier NOT NULL,
    [ShiftName] nvarchar(100) NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [IsOvernight] bit NOT NULL,
    [GracePeriodMinutes] int NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Shifts] PRIMARY KEY ([Id])
);

CREATE TABLE [Tenants] (
    [Id] uniqueidentifier NOT NULL,
    [TenantName] nvarchar(200) NOT NULL,
    [Domain] nvarchar(100) NOT NULL,
    [Industry] nvarchar(100) NOT NULL,
    [Country] nvarchar(100) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [DatabaseName] nvarchar(100) NOT NULL,
    [ConnectionString] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);

CREATE TABLE [Trainings] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(4000) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Trainer] nvarchar(200) NULL,
    [Location] nvarchar(200) NULL,
    [TrainingType] nvarchar(50) NOT NULL,
    [MaxParticipants] int NOT NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Scheduled',
    [ParticipantsJson] nvarchar(4000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Trainings] PRIMARY KEY ([Id])
);

CREATE TABLE [EmployeeAddresses] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [AddressType] nvarchar(50) NOT NULL,
    [AddressLine1] nvarchar(200) NOT NULL,
    [AddressLine2] nvarchar(200) NULL,
    [City] nvarchar(100) NOT NULL,
    [State] nvarchar(100) NULL,
    [CountryCode] nvarchar(10) NOT NULL,
    [PostalCode] nvarchar(20) NULL,
    [IsPrimary] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_EmployeeAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeAddresses_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [EmployeeDocuments] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [DocumentType] nvarchar(50) NOT NULL,
    [FileUrl] nvarchar(1000) NOT NULL,
    [UploadedBy] uniqueidentifier NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NULL,
    [IsSensitive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_EmployeeDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeDocuments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [EmploymentContracts] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [ContractType] nvarchar(50) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [WorkingHoursPerWeek] decimal(5,2) NOT NULL,
    [BaseSalary] decimal(18,2) NOT NULL,
    [Currency] nvarchar(10) NOT NULL,
    [PayFrequency] nvarchar(20) NOT NULL,
    [NoticePeriodDays] int NOT NULL,
    [ProbationPeriodDays] int NOT NULL,
    [ContractDocumentUrl] nvarchar(1000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_EmploymentContracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmploymentContracts_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PerformanceReviews] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [ReviewerId] uniqueidentifier NOT NULL,
    [PeriodStart] datetime2 NOT NULL,
    [PeriodEnd] datetime2 NOT NULL,
    [ReviewDate] datetime2 NOT NULL,
    [Rating] decimal(3,2) NOT NULL,
    [Comments] nvarchar(4000) NULL,
    [GoalsJson] nvarchar(4000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PerformanceReviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerformanceReviews_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PerformanceReviews_Employees_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TimeEntries] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [ClockIn] datetimeoffset NOT NULL,
    [ClockOut] datetimeoffset NULL,
    [DurationMinutes] int NULL,
    [Device] nvarchar(100) NULL,
    [Location] nvarchar(255) NULL,
    [Source] nvarchar(50) NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_TimeEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TimeEntries_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [JobApplications] (
    [Id] uniqueidentifier NOT NULL,
    [JobOpeningId] uniqueidentifier NOT NULL,
    [CandidateId] uniqueidentifier NOT NULL,
    [ResumeUrl] nvarchar(1000) NOT NULL,
    [AppliedAt] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Applied',
    [Notes] nvarchar(1000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_JobApplications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobApplications_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JobApplications_JobOpenings_JobOpeningId] FOREIGN KEY ([JobOpeningId]) REFERENCES [JobOpenings] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [LeaveBalances] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [LeaveTypeId] uniqueidentifier NOT NULL,
    [Accrued] decimal(6,2) NOT NULL,
    [Taken] decimal(6,2) NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_LeaveBalances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveBalances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeaveBalances_LeaveTypes_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [LeaveTypes] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LeaveRequests] (
    [Id] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [LeaveTypeId] uniqueidentifier NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [TotalDays] decimal(6,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Pending',
    [RequestedAt] datetime2 NOT NULL,
    [ApprovedAt] datetime2 NULL,
    [ApprovedBy] uniqueidentifier NULL,
    [Notes] nvarchar(1000) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeaveRequests_LeaveTypes_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [LeaveTypes] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PayrollLines] (
    [Id] uniqueidentifier NOT NULL,
    [PayrollId] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [Earnings] decimal(18,2) NOT NULL,
    [Allowances] decimal(18,2) NOT NULL,
    [Deductions] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [NetPay] decimal(18,2) NOT NULL,
    [BankAccountId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PayrollLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PayrollLines_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PayrollLines_Payrolls_PayrollId] FOREIGN KEY ([PayrollId]) REFERENCES [Payrolls] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Companies] (
    [Id] uniqueidentifier NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [LegalName] nvarchar(200) NOT NULL,
    [RegistrationNumber] nvarchar(50) NOT NULL,
    [TaxNumber] nvarchar(50) NOT NULL,
    [AddressLine1] nvarchar(200) NOT NULL,
    [AddressLine2] nvarchar(200) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [State] nvarchar(100) NOT NULL,
    [Country] nvarchar(100) NOT NULL,
    [ZipCode] nvarchar(20) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Website] nvarchar(200) NOT NULL,
    [LogoUrl] nvarchar(500) NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Companies_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [RoleName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [IsSystemRole] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Roles_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Settings] (
    [Id] uniqueidentifier NOT NULL,
    [SettingKey] nvarchar(100) NOT NULL,
    [SettingValue] nvarchar(1000) NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Settings_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Subscriptions] (
    [Id] uniqueidentifier NOT NULL,
    [PlanId] uniqueidentifier NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [RenewalDate] datetime2 NOT NULL,
    [PaymentStatus] nvarchar(50) NOT NULL,
    [IsAutoRenew] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Subscriptions_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Subscriptions_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Interviews] (
    [Id] uniqueidentifier NOT NULL,
    [JobApplicationId] uniqueidentifier NOT NULL,
    [InterviewerId] uniqueidentifier NOT NULL,
    [ScheduledAt] datetime2 NOT NULL,
    [InterviewType] nvarchar(50) NOT NULL,
    [Location] nvarchar(200) NULL,
    [Notes] nvarchar(1000) NULL,
    [Rating] nvarchar(50) NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Scheduled',
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Interviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Interviews_Employees_InterviewerId] FOREIGN KEY ([InterviewerId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Interviews_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [Salutation] nvarchar(10) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [Gender] nvarchar(10) NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [JobTitle] nvarchar(100) NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [Nationality] nvarchar(100) NOT NULL,
    [PrimaryEmail] nvarchar(200) NOT NULL,
    [SecondaryEmail] nvarchar(200) NOT NULL,
    [PrimaryPhone] nvarchar(20) NOT NULL,
    [SecondaryPhone] nvarchar(20) NOT NULL,
    [MobilePhone] nvarchar(20) NOT NULL,
    [Fax] nvarchar(20) NOT NULL,
    [LinkedIn] nvarchar(200) NOT NULL,
    [Twitter] nvarchar(200) NOT NULL,
    [Facebook] nvarchar(200) NOT NULL,
    [Instagram] nvarchar(200) NOT NULL,
    [PreferredCommunicationMethod] nvarchar(20) NOT NULL,
    [LanguagePreference] nvarchar(10) NOT NULL,
    [TimeZone] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PasswordSalt] nvarchar(max) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [LastLoginDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsLocked] bit NOT NULL,
    [FailedLoginAttempts] int NOT NULL,
    [LockedUntil] datetime2 NULL,
    [EmailVerificationToken] nvarchar(max) NULL,
    [PasswordResetToken] nvarchar(max) NULL,
    [PasswordResetExpires] datetime2 NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpires] datetime2 NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RolePermissions] (
    [Id] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    [IsAllowed] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(50) NOT NULL,
    [OldValues] nvarchar(4000) NOT NULL,
    [NewValues] nvarchar(4000) NOT NULL,
    [IpAddress] nvarchar(50) NOT NULL,
    [UserAgent] nvarchar(500) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserRoles] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [AssignedDate] datetime2 NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AuditLogs_TenantId] ON [AuditLogs] ([TenantId]);

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);

CREATE INDEX [IX_Companies_TenantId] ON [Companies] ([TenantId]);

CREATE INDEX [IX_EmployeeAddresses_EmployeeId] ON [EmployeeAddresses] ([EmployeeId]);

CREATE INDEX [IX_EmployeeDocuments_EmployeeId] ON [EmployeeDocuments] ([EmployeeId]);

CREATE INDEX [IX_Employees_ManagerId] ON [Employees] ([ManagerId]);

CREATE UNIQUE INDEX [IX_Employees_TenantId_EmployeeCode] ON [Employees] ([TenantId], [EmployeeCode]);

CREATE INDEX [IX_Employees_TenantId_WorkEmail] ON [Employees] ([TenantId], [WorkEmail]);

CREATE UNIQUE INDEX [IX_EmploymentContracts_EmployeeId] ON [EmploymentContracts] ([EmployeeId]);

CREATE INDEX [IX_Interviews_InterviewerId] ON [Interviews] ([InterviewerId]);

CREATE INDEX [IX_Interviews_JobApplicationId] ON [Interviews] ([JobApplicationId]);

CREATE INDEX [IX_JobApplications_CandidateId] ON [JobApplications] ([CandidateId]);

CREATE INDEX [IX_JobApplications_JobOpeningId] ON [JobApplications] ([JobOpeningId]);

CREATE INDEX [IX_LeaveBalances_EmployeeId] ON [LeaveBalances] ([EmployeeId]);

CREATE INDEX [IX_LeaveBalances_LeaveTypeId] ON [LeaveBalances] ([LeaveTypeId]);

CREATE INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests] ([EmployeeId]);

CREATE INDEX [IX_LeaveRequests_LeaveTypeId] ON [LeaveRequests] ([LeaveTypeId]);

CREATE INDEX [IX_PayrollLines_EmployeeId] ON [PayrollLines] ([EmployeeId]);

CREATE INDEX [IX_PayrollLines_PayrollId] ON [PayrollLines] ([PayrollId]);

CREATE INDEX [IX_PerformanceReviews_EmployeeId] ON [PerformanceReviews] ([EmployeeId]);

CREATE INDEX [IX_PerformanceReviews_ReviewerId] ON [PerformanceReviews] ([ReviewerId]);

CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);

CREATE INDEX [IX_RolePermissions_RoleId] ON [RolePermissions] ([RoleId]);

CREATE INDEX [IX_Roles_TenantId] ON [Roles] ([TenantId]);

CREATE INDEX [IX_Settings_TenantId] ON [Settings] ([TenantId]);

CREATE INDEX [IX_Subscriptions_PlanId] ON [Subscriptions] ([PlanId]);

CREATE INDEX [IX_Subscriptions_TenantId] ON [Subscriptions] ([TenantId]);

CREATE UNIQUE INDEX [IX_Tenants_Domain] ON [Tenants] ([Domain]);

CREATE INDEX [IX_TimeEntries_EmployeeId_ClockIn] ON [TimeEntries] ([EmployeeId], [ClockIn]);

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

CREATE INDEX [IX_UserRoles_UserId] ON [UserRoles] ([UserId]);

CREATE INDEX [IX_Users_CompanyId] ON [Users] ([CompanyId]);

CREATE UNIQUE INDEX [IX_Users_PrimaryEmail] ON [Users] ([PrimaryEmail]);

CREATE INDEX [IX_Users_TenantId] ON [Users] ([TenantId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906093437_InitialCreate', N'9.0.8');

COMMIT;
GO

