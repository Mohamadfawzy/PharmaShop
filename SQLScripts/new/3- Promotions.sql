
use pharma_shope_db;
go

CREATE TABLE [dbo].[Promotions] (
    [Id] int NOT NULL IDENTITY(1,1),
    [ErpPgoId] decimal(18,0) NULL, -- ERP group offer id (pgo_id)
    [Name] nvarchar(200) NULL,
    [Notes] nvarchar(250) NULL,
    [StartAt] datetime2(0) NULL,
    [EndAt] datetime2(0) NULL,
    [TotalAmount] int NULL,
    [BasicAmount] int NULL,
    [OfferAmount] int NULL,
    [DiscountPercent] decimal(5,2) NULL, -- ERP offer_amount_disc (assumed percent)
    [IsActive] bit NOT NULL CONSTRAINT [DF_Promotions_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Promotions_CreatedAt] DEFAULT (sysutcdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [RowVersion] rowversion NOT NULL, -- Concurrency
    CONSTRAINT [PK_Promotions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Promotions_DiscountPercent] CHECK ([DiscountPercent] IS NULL OR ([DiscountPercent] >= (0) AND [DiscountPercent] <= (100))),
    CONSTRAINT [CK_Promotions_Dates] CHECK (([StartAt] IS NULL AND [EndAt] IS NULL) OR ([StartAt] IS NOT NULL AND [EndAt] IS NOT NULL AND [EndAt] > [StartAt]))
);
GO

/* Unique ERP group offer id (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_Promotions_ErpPgoId]
ON [dbo].[Promotions] ([ErpPgoId])
WHERE [ErpPgoId] IS NOT NULL AND [DeletedAt] IS NULL;
GO

/* Fast listing for active promotions by date window */
CREATE INDEX [IX_Promotions_ActiveWindow]
ON [dbo].[Promotions] ([IsActive], [StartAt], [EndAt])
INCLUDE ([Name], [DiscountPercent])
WHERE [DeletedAt] IS NULL;
GO



CREATE TABLE [dbo].[PromotionProducts] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PromotionId] int NOT NULL,
    [ProductId] int NULL, -- Local product id (nullable for sync)
    [ErpOfferId] decimal(18,0) NULL, -- ERP offer_list.offer_id (optional)
    [ErpProductId] decimal(18,0) NOT NULL, -- ERP product_id
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PromotionProducts_CreatedAt] DEFAULT (sysutcdatetime()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    CONSTRAINT [PK_PromotionProducts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PromotionProducts_Promotions_PromotionId] FOREIGN KEY ([PromotionId]) REFERENCES [dbo].[Promotions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PromotionProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION
);
GO

/* Prevent duplicate ERP product within the same promotion (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_PromotionProducts_PromotionId_ErpProductId]
ON [dbo].[PromotionProducts] ([PromotionId], [ErpProductId])
WHERE [DeletedAt] IS NULL;
GO

/* Fast join: all products under a promotion */
CREATE INDEX [IX_PromotionProducts_PromotionId]
ON [dbo].[PromotionProducts] ([PromotionId])
INCLUDE ([ProductId], [ErpProductId])
WHERE [DeletedAt] IS NULL;
GO

/* Fast lookup: all promotions affecting a product (local) */
CREATE INDEX [IX_PromotionProducts_ProductId]
ON [dbo].[PromotionProducts] ([ProductId])
INCLUDE ([PromotionId])
WHERE [DeletedAt] IS NULL AND [ProductId] IS NOT NULL;
GO

/* Fast lookup during sync using ERP product id */
CREATE INDEX [IX_PromotionProducts_ErpProductId]
ON [dbo].[PromotionProducts] ([ErpProductId])
INCLUDE ([PromotionId], [ProductId])
WHERE [DeletedAt] IS NULL;
GO