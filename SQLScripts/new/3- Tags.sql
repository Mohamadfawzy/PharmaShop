
use pharma_shope_db;

GO

CREATE TABLE [dbo].[Tags] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PharmacyId] int NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [NameEn] nvarchar(80) NULL,
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Tags_CreatedAt] DEFAULT (sysutcdatetime()),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Tags_IsActive] DEFAULT ((1)),
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tags_Pharmacies_PharmacyId] FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);

GO

-- Unique tag name per pharmacy
CREATE UNIQUE INDEX [UX_Tags_PharmacyId_Name]
ON [dbo].[Tags] ([PharmacyId], [Name]);

GO

CREATE TABLE [dbo].[ProductTags] (
    [ProductId] int NOT NULL,
    [TagId] int NOT NULL,
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ProductTags_CreatedAt] DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_ProductTags] PRIMARY KEY ([ProductId], [TagId]),
    CONSTRAINT [FK_ProductTags_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tags] ([Id]) ON DELETE NO ACTION
);

GO

-- Fast lookup: all products under a tag
CREATE INDEX [IX_ProductTags_TagId]
ON [dbo].[ProductTags] ([TagId]);

GO