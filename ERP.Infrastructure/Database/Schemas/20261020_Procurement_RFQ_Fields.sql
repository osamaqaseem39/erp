-- Extend RFQ header/lines for light RFQ service (number, title, priority, specs).
IF COL_LENGTH(N'dbo.RequestForQuotations', N'RFQNumber') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [RFQNumber] NVARCHAR(50) NOT NULL CONSTRAINT [DF_RFQ_Number] DEFAULT (N'');
GO
IF COL_LENGTH(N'dbo.RequestForQuotations', N'Title') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [Title] NVARCHAR(200) NOT NULL CONSTRAINT [DF_RFQ_Title] DEFAULT (N'');
GO
IF COL_LENGTH(N'dbo.RequestForQuotations', N'Description') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [Description] NVARCHAR(1000) NULL;
GO
IF COL_LENGTH(N'dbo.RequestForQuotations', N'Priority') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [Priority] NVARCHAR(30) NOT NULL CONSTRAINT [DF_RFQ_Priority] DEFAULT (N'Medium');
GO
IF COL_LENGTH(N'dbo.RequestForQuotations', N'EstimatedValue') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [EstimatedValue] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_RFQ_Est] DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.RequestForQuotations', N'Terms') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [Terms] NVARCHAR(1000) NULL;
GO
IF COL_LENGTH(N'dbo.RequestForQuotations', N'Notes') IS NULL
    ALTER TABLE [dbo].[RequestForQuotations] ADD [Notes] NVARCHAR(1000) NULL;
GO
IF COL_LENGTH(N'dbo.RFQDetails', N'Specifications') IS NULL
    ALTER TABLE [dbo].[RFQDetails] ADD [Specifications] NVARCHAR(1000) NULL;
GO
-- Allow RFQ lines without UoM
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.RFQDetails') AND name = N'UnitOfMeasureId' AND is_nullable = 0)
BEGIN
    ALTER TABLE [dbo].[RFQDetails] ALTER COLUMN [UnitOfMeasureId] UNIQUEIDENTIFIER NULL;
END
GO
