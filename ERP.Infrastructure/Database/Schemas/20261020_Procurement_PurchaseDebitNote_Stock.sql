-- Optional warehouse for debit-note stock issue on post / receive on void.
IF OBJECT_ID('dbo.PurchaseDebitNotes', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.PurchaseDebitNotes', 'WarehouseId') IS NULL
        ALTER TABLE dbo.PurchaseDebitNotes ADD WarehouseId uniqueidentifier NULL;
    IF COL_LENGTH('dbo.PurchaseDebitNotes', 'StockIssued') IS NULL
        ALTER TABLE dbo.PurchaseDebitNotes ADD StockIssued bit NOT NULL
            CONSTRAINT DF_PurchaseDebitNotes_StockIssued DEFAULT (0);
END
GO
