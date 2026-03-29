
use pharma_shope_db;

GO
CREATE TABLE [dbo].[Carts] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,
    [Status] tinyint NOT NULL CONSTRAINT [DF_Carts_Status] DEFAULT ((1)), -- 1=Active,2=CheckedOut,3=Abandoned
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Carts_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [ExpiresAt] datetime2(0) NULL, -- Optional TTL for cleanup
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Carts_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Carts_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CK_Carts_Status]
        CHECK ([Status] IN ((1),(2),(3)))
);
GO
/* Fast lookup: active cart by customer and store (ignoring soft-deleted rows) */
CREATE INDEX [IX_Carts_CustomerId_StoreId_Status]
ON [dbo].[Carts] ([CustomerId], [StoreId], [Status])
WHERE [DeletedAt] IS NULL;
GO

/* Enforce a single active cart per customer per store */
CREATE UNIQUE INDEX [UX_Carts_CustomerId_StoreId_Active]
ON [dbo].[Carts] ([CustomerId], [StoreId])
WHERE [Status] = (1) AND [DeletedAt] IS NULL;
GO


CREATE TABLE [dbo].[CartItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CartId] int NOT NULL,
    [ProductId] int NOT NULL,
    [UnitLevel] tinyint NOT NULL, -- 1=Outer,2=Inner
    [Quantity] decimal(18,3) NOT NULL,
    [UnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_CartItems_UnitPriceSnapshot] DEFAULT ((0)),
    [PromotionDiscountPercentSnapshot] decimal(5,2) NOT NULL CONSTRAINT [DF_CartItems_PromoDiscSnapshot] DEFAULT ((0)),
    [PointsSnapshot] int NOT NULL CONSTRAINT [DF_CartItems_PointsSnapshot] DEFAULT ((0)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CartItems_Carts_CartId]
        FOREIGN KEY ([CartId]) REFERENCES [dbo].[Carts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CartItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CK_CartItems_UnitLevel]
        CHECK ([UnitLevel] IN ((1),(2))),
    CONSTRAINT [CK_CartItems_QuantityPositive]
        CHECK ([Quantity] > (0)),
    CONSTRAINT [CK_CartItems_PromoPercent]
        CHECK ([PromotionDiscountPercentSnapshot] >= (0) AND [PromotionDiscountPercentSnapshot] <= (100)),
    CONSTRAINT [CK_CartItems_UnitPriceNonNegative]
        CHECK ([UnitPriceSnapshot] >= (0)),
    CONSTRAINT [CK_CartItems_PointsNonNegative]
        CHECK ([PointsSnapshot] >= (0))
);
GO

/* Fast lookup: all items in a cart (ignoring soft-deleted rows) */
CREATE INDEX [IX_CartItems_CartId]
ON [dbo].[CartItems] ([CartId])
INCLUDE ([ProductId], [UnitLevel], [Quantity], [UnitPriceSnapshot], [PromotionDiscountPercentSnapshot], [PointsSnapshot])
WHERE [DeletedAt] IS NULL;
GO

/* Prevent duplicate lines for the same product + unit level within a cart */
CREATE UNIQUE INDEX [UX_CartItems_CartId_ProductId_UnitLevel]
ON [dbo].[CartItems] ([CartId], [ProductId], [UnitLevel])
WHERE [DeletedAt] IS NULL;
GO

/* Useful for reverse lookup (product -> carts) during analytics or stock checks */
CREATE INDEX [IX_CartItems_ProductId]
ON [dbo].[CartItems] ([ProductId])
WHERE [DeletedAt] IS NULL;
GO