-- Optional TOTP 2FA on Core.Users
IF COL_LENGTH('Core.Users', 'TwoFactorEnabled') IS NULL
BEGIN
    ALTER TABLE Core.Users ADD
        TwoFactorEnabled bit NOT NULL CONSTRAINT DF_Users_TwoFactorEnabled DEFAULT (0),
        TwoFactorSecret nvarchar(128) NULL,
        TwoFactorPendingSecret nvarchar(128) NULL,
        TwoFactorChallengeTokenHash nvarchar(128) NULL,
        TwoFactorChallengeExpires datetime2 NULL;
END
GO
