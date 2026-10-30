-- CRM leads: reason captured when a lead is marked lost
-- Idempotent COL_LENGTH checks

IF COL_LENGTH(N'dbo.Leads', N'LostReason') IS NULL
    ALTER TABLE [dbo].[Leads] ADD [LostReason] NVARCHAR(100) NULL;
GO
IF COL_LENGTH(N'dbo.Leads', N'LostNote') IS NULL
    ALTER TABLE [dbo].[Leads] ADD [LostNote] NVARCHAR(1000) NULL;
GO
