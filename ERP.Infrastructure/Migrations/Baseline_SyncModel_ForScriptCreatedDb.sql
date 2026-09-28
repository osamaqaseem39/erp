-- Run this when the database was created from Database/Schemas/*.sql (Core schema already exists)
-- and "update-database" fails on SyncModel with "Users already exists".
--
-- This marks SyncModel as applied without re-running it. Safe when Core tables already exist.

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260517095513_SyncModel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517095513_SyncModel', N'9.0.8');
END
GO
