-- Credit note GL link: journal created on post (Dr SalesReturns / Cr AR).
IF COL_LENGTH('dbo.SalesCreditNotes', 'PostedJournalId') IS NULL
   AND OBJECT_ID('dbo.SalesCreditNotes', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SalesCreditNotes ADD PostedJournalId uniqueidentifier NULL;
END
GO
