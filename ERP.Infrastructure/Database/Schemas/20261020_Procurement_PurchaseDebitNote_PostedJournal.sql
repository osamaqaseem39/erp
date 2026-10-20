-- Debit note GL link: journal created on post (Dr AP / Cr PurchaseReturns).
IF COL_LENGTH('dbo.PurchaseDebitNotes', 'PostedJournalId') IS NULL
   AND OBJECT_ID('dbo.PurchaseDebitNotes', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.PurchaseDebitNotes ADD PostedJournalId uniqueidentifier NULL;
END
GO
