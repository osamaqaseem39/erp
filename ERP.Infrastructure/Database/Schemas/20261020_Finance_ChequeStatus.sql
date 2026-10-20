-- Cheque register lifecycle on journal vouchers that carry ChequeNumber.
DECLARE @JournalTable sysname =
    CASE WHEN OBJECT_ID('Finance.Journals', 'U') IS NOT NULL THEN 'Finance.Journals'
         WHEN OBJECT_ID('dbo.Journals', 'U') IS NOT NULL THEN 'dbo.Journals'
         ELSE NULL END;

IF @JournalTable IS NOT NULL
BEGIN
    IF COL_LENGTH(@JournalTable, 'ChequeStatus') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ChequeStatus nvarchar(30) NULL');
    IF COL_LENGTH(@JournalTable, 'ChequeStatusAt') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ChequeStatusAt datetime2 NULL');
    IF COL_LENGTH(@JournalTable, 'ChequeStatusNotes') IS NULL
        EXEC('ALTER TABLE ' + @JournalTable + ' ADD ChequeStatusNotes nvarchar(500) NULL');
END
GO
