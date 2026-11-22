USE [pharma_shope_db];
GO

/* Add optional, per-store contact fields. Store.Address is reused for contact address. */
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Stores', N'ContactPhone') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [ContactPhone] nvarchar(50) NULL;

IF COL_LENGTH(N'dbo.Stores', N'ContactWhatsApp') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [ContactWhatsApp] nvarchar(50) NULL;

IF COL_LENGTH(N'dbo.Stores', N'ContactEmail') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [ContactEmail] nvarchar(256) NULL;

IF COL_LENGTH(N'dbo.Stores', N'WorkingHours') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [WorkingHours] nvarchar(500) NULL;

COMMIT TRANSACTION;
GO
