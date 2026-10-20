-- Optional warehouse for credit-note stock restock on post / issue on void.
IF OBJECT_ID('dbo.SalesCreditNotes', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SalesCreditNotes', 'WarehouseId') IS NULL
        ALTER TABLE dbo.SalesCreditNotes ADD WarehouseId uniqueidentifier NULL;
    IF COL_LENGTH('dbo.SalesCreditNotes', 'StockReceived') IS NULL
        ALTER TABLE dbo.SalesCreditNotes ADD StockReceived bit NOT NULL
            CONSTRAINT DF_SalesCreditNotes_StockReceived DEFAULT (0);
END
GO
