-- Bank statement reconciliation: link cleared txs to an open/closed recon header.
IF COL_LENGTH('Finance.BankTransactions', 'ReconciliationId') IS NULL
   AND COL_LENGTH('dbo.BankTransactions', 'ReconciliationId') IS NULL
BEGIN
    IF OBJECT_ID('Finance.BankTransactions', 'U') IS NOT NULL
        ALTER TABLE Finance.BankTransactions ADD ReconciliationId uniqueidentifier NULL;
    ELSE IF OBJECT_ID('dbo.BankTransactions', 'U') IS NOT NULL
        ALTER TABLE dbo.BankTransactions ADD ReconciliationId uniqueidentifier NULL;
END
GO
