-- CRM sale enquiries: reason captured when an enquiry is rejected
-- Idempotent COL_LENGTH check

IF COL_LENGTH(N'CRM.SaleEnquiries', N'RejectionReason') IS NULL
    ALTER TABLE [CRM].[SaleEnquiries] ADD [RejectionReason] NVARCHAR(1000) NULL;
GO
