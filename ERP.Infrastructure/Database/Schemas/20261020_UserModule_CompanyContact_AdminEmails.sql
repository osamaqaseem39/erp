-- User module U1/U4: company contact fields + admin email directory
-- Idempotent for SQL Server.

IF COL_LENGTH('Core.Companies', 'ContactPerson') IS NULL
    ALTER TABLE [Core].[Companies] ADD [ContactPerson] NVARCHAR(200) NULL;
GO
IF COL_LENGTH('Core.Companies', 'MobilePhone') IS NULL
    ALTER TABLE [Core].[Companies] ADD [MobilePhone] NVARCHAR(30) NULL;
GO

IF OBJECT_ID(N'[Core].[AdminEmails]', N'U') IS NULL
BEGIN
    CREATE TABLE [Core].[AdminEmails] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_AdminEmails_IsActive] DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AdminEmails_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_AdminEmails_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [Core].[Tenants]([Id])
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_AdminEmails_Tenant_Email' AND object_id = OBJECT_ID(N'[Core].[AdminEmails]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_AdminEmails_Tenant_Email]
        ON [Core].[AdminEmails]([TenantId], [Email])
        WHERE [IsActive] = 1;
END
GO
