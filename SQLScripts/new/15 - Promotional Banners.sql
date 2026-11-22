USE [pharma_shope_db];
GO

/*
    Promotional Banners
    Run after the base schema scripts. This script is safe to run more than once.
    BannerPlacement: Home=0, Offers=1, Categories=2
    BannerActionType: None=0, Product=1, Offer=2, Category=3, Brand=4, ExternalUrl=5
*/
IF OBJECT_ID(N'dbo.PromotionalBanners', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PromotionalBanners]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Placement] tinyint NOT NULL,
        [ActionType] tinyint NOT NULL,
        [TargetId] int NULL,
        [ParentTargetId] int NULL,
        [ActionUrl] nvarchar(2048) NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [ThumbnailUrl] nvarchar(500) NOT NULL,
        [AltText] nvarchar(200) NULL,
        [DisplayOrder] int NOT NULL CONSTRAINT [DF_PromotionalBanners_DisplayOrder] DEFAULT ((0)),
        [IsActive] bit NOT NULL CONSTRAINT [DF_PromotionalBanners_IsActive] DEFAULT ((1)),
        [StartsAt] datetime2(0) NULL,
        [EndsAt] datetime2(0) NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PromotionalBanners_CreatedAt] DEFAULT (sysutcdatetime()),
        [UpdatedAt] datetime2(0) NULL,
        [DeletedAt] datetime2(0) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PromotionalBanners] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PromotionalBanners_Placement] CHECK ([Placement] IN (0, 1, 2)),
        CONSTRAINT [CK_PromotionalBanners_ActionType] CHECK ([ActionType] IN (0, 1, 2, 3, 4, 5)),
        CONSTRAINT [CK_PromotionalBanners_DisplayOrder] CHECK ([DisplayOrder] >= 0),
        CONSTRAINT [CK_PromotionalBanners_DateWindow] CHECK ([StartsAt] IS NULL OR [EndsAt] IS NULL OR [EndsAt] > [StartsAt]),
        CONSTRAINT [CK_PromotionalBanners_ActionTarget] CHECK
        (
            ([ActionType] = 0 AND [TargetId] IS NULL AND [ParentTargetId] IS NULL AND [ActionUrl] IS NULL)
            OR ([ActionType] IN (1, 2, 4) AND [TargetId] IS NOT NULL AND [TargetId] > 0 AND [ParentTargetId] IS NULL AND [ActionUrl] IS NULL)
            OR ([ActionType] = 3 AND [TargetId] IS NOT NULL AND [TargetId] > 0 AND ([ParentTargetId] IS NULL OR [ParentTargetId] > 0) AND [ActionUrl] IS NULL)
            OR ([ActionType] = 5 AND [TargetId] IS NULL AND [ParentTargetId] IS NULL AND [ActionUrl] LIKE N'https://%')
        ),
        CONSTRAINT [CK_PromotionalBanners_ImagePaths] CHECK ([ImageUrl] LIKE N'/uploads/%' AND [ThumbnailUrl] LIKE N'/uploads/%')
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_PromotionalBanners_PublicOrder' AND [object_id] = OBJECT_ID(N'dbo.PromotionalBanners'))
    CREATE INDEX [IX_PromotionalBanners_PublicOrder]
        ON [dbo].[PromotionalBanners] ([Placement], [IsActive], [DisplayOrder], [Id])
        INCLUDE ([StartsAt], [EndsAt], [Name], [ActionType], [TargetId], [ParentTargetId], [ActionUrl], [ImageUrl], [ThumbnailUrl], [AltText])
        WHERE [DeletedAt] IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_PromotionalBanners_DateWindow' AND [object_id] = OBJECT_ID(N'dbo.PromotionalBanners'))
    CREATE INDEX [IX_PromotionalBanners_DateWindow]
        ON [dbo].[PromotionalBanners] ([StartsAt], [EndsAt])
        WHERE [IsActive] = 1 AND [DeletedAt] IS NULL;
GO
