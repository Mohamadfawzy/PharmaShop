
use pharma_shope_db;
go

INSERT INTO dbo.Companies (PharmacyId, NameAr, NameEn)
VALUES (1, N'جلَكسو سميث كلاين', N'GlaxoSmithKline');
GO




CREATE TABLE [dbo].[Companies] (
    [Id] int NOT NULL IDENTITY(1,1),
    [NameAr] nvarchar(150) NOT NULL,
    [NameEn] nvarchar(150) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Companies_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Companies_CreatedAt] DEFAULT (SYSDATETIME()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,
    [RowVersion] rowversion NOT NULL, -- Concurrency
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
);
GO

 -- Unique Arabic name (ignoring soft-deleted rows)  
CREATE UNIQUE INDEX [UX_Companies_NameAr]
ON [dbo].[Companies] ([NameAr])
WHERE [DeletedAt] IS NULL;
GO

 -- Index for English name search/sort (ignoring soft-deleted rows)  
CREATE INDEX [IX_Companies_NameEn]
ON [dbo].[Companies] ([NameEn])
WHERE [DeletedAt] IS NULL AND [NameEn] IS NOT NULL;
GO

-- Index for active listing with covered columns (ignoring soft-deleted rows)  
CREATE INDEX [IX_Companies_IsActive]
ON [dbo].[Companies] ([IsActive])
INCLUDE ([NameAr], [NameEn], [CreatedAt])
WHERE [DeletedAt] IS NULL;
GO

