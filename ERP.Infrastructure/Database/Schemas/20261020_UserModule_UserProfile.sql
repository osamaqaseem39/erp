-- User module U6: address fields on Core.Users
-- Idempotent for SQL Server.

IF COL_LENGTH('Core.Users', 'AddressLine1') IS NULL
    ALTER TABLE [Core].[Users] ADD [AddressLine1] NVARCHAR(200) NULL;
GO
IF COL_LENGTH('Core.Users', 'City') IS NULL
    ALTER TABLE [Core].[Users] ADD [City] NVARCHAR(100) NULL;
GO

-- Username unique within a tenant (email uniqueness already enforced in app + existing index).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_Users_Tenant_Username' AND object_id = OBJECT_ID(N'[Core].[Users]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_Users_Tenant_Username]
        ON [Core].[Users]([TenantId], [Username])
        WHERE [Username] IS NOT NULL AND [Username] <> N'';
END
GO
