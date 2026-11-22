USE [pharma_shope_db];
GO

/*
   Additive schema update for checkout idempotency and rotating refresh tokens.
   Safe to run more than once. This script does not modify or recreate existing data.
   COD remains PaymentMethod = 1, matching the existing Orders constraint.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Orders', N'CheckoutIdempotencyKey') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CheckoutIdempotencyKey] nvarchar(100) NULL;

IF COL_LENGTH(N'dbo.Orders', N'CustomerNotes') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CustomerNotes] nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Orders', N'CheckoutRequestHash') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CheckoutRequestHash] varchar(64) NULL;

IF COL_LENGTH(N'dbo.Orders', N'CheckoutResponseJson') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CheckoutResponseJson] nvarchar(max) NULL;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'dbo.Orders')
      AND [name] = N'UX_Orders_CustomerId_CheckoutIdempotencyKey'
)
    CREATE UNIQUE INDEX [UX_Orders_CustomerId_CheckoutIdempotencyKey]
        ON [dbo].[Orders] ([CustomerId], [CheckoutIdempotencyKey])
        WHERE [CheckoutIdempotencyKey] IS NOT NULL;

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RefreshTokens]
    (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [TokenHash] varchar(64) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL CONSTRAINT [DF_RefreshTokens_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [ExpiresAtUtc] datetime2(3) NOT NULL,
        [RevokedAtUtc] datetime2(3) NULL,
        [ReplacedByTokenHash] varchar(64) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [CK_RefreshTokens_Expiry] CHECK ([ExpiresAtUtc] > [CreatedAtUtc])
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'dbo.RefreshTokens') AND [name] = N'UX_RefreshTokens_TokenHash'
)
    CREATE UNIQUE INDEX [UX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens] ([TokenHash]);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'dbo.RefreshTokens') AND [name] = N'IX_RefreshTokens_UserId_ExpiresAtUtc'
)
    CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAtUtc]
        ON [dbo].[RefreshTokens] ([UserId], [ExpiresAtUtc]);

COMMIT TRANSACTION;
GO
