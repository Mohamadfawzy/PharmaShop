
use pharma_shope_db;

GO

--drop table ProductTags;
--drop table Tags
go 
CREATE TABLE [dbo].[Tags] (
    [Id] int NOT NULL IDENTITY(1,1),
    [NameAr] nvarchar(80) NOT NULL,
    [NameEn] nvarchar(80) NULL,
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Tags_CreatedAt] DEFAULT (sysdatetime()),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Tags_IsActive] DEFAULT ((1)),
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id]),
);

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