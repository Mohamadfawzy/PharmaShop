
use pharma_shope_db;

GO
--drop table [CartItems];
--drop table Carts;

CREATE TABLE [dbo].[Carts] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,
    [Status] tinyint NOT NULL CONSTRAINT [DF_Carts_Status] DEFAULT ((1)), -- 1=Active,2=CheckedOut,3=Abandoned,4=Expired,5=Invalid
    [StatusUpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Carts_StatusUpdatedAt] DEFAULT (sysdatetime()),
    [ExpiredReason] nvarchar(300) NULL, -- Reason for Expired/Invalid states
    [DeviceId] nvarchar(100) NULL, -- Device identifier (optional)
    [AppInstanceId] nvarchar(100) NULL, -- App install/session identifier (optional)
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Carts_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Carts_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Carts_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_Carts_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5)))
);
GO



/* Enforce a single active cart per customer */
CREATE UNIQUE INDEX [UX_Carts_CustomerId_Active]
ON [dbo].[Carts] ([CustomerId])
WHERE [Status] = (1);
GO

/* Fast lookup: customer cart by status */
CREATE INDEX [IX_Carts_CustomerId_Status_StatusUpdatedAt]
ON [dbo].[Carts] ([CustomerId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([StoreId]);
GO

/* Fast lookup: store cart activity (optional reporting) */
CREATE INDEX [IX_Carts_StoreId_Status_StatusUpdatedAt]
ON [dbo].[Carts] ([StoreId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([CustomerId]);
GO






CREATE TABLE [dbo].[CartItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CartId] int NOT NULL,
    [ProductId] int NOT NULL,

    [UnitLevel] tinyint NOT NULL, -- 1=Outer,2=Inner
    [Quantity] decimal(18,3) NOT NULL,

    [UnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_CartItems_UnitPriceSnapshot] DEFAULT ((0)), -- old price for comparison
    [CurrentUnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_CartItems_CurrentUnitPriceSnapshot] DEFAULT ((0)), -- last known current price for comparison

    [MinOrderQtySnapshot] int NOT NULL CONSTRAINT [DF_CartItems_MinOrderQtySnapshot] DEFAULT ((1)),
    [MaxOrderQtySnapshot] int NULL,
    [MaxPerDayQtySnapshot] int NULL,

    [IsValid] bit NOT NULL CONSTRAINT [DF_CartItems_IsValid] DEFAULT ((1)),
    [InvalidReason] nvarchar(300) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_CartItems_Carts_CartId]
        FOREIGN KEY ([CartId]) REFERENCES [dbo].[Carts] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_CartItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_CartItems_UnitLevel]
        CHECK ([UnitLevel] IN ((1),(2))),

    CONSTRAINT [CK_CartItems_QuantityPositive]
        CHECK ([Quantity] > (0)),

    CONSTRAINT [CK_CartItems_PricesNonNegative]
        CHECK ([UnitPriceSnapshot] >= (0) AND [CurrentUnitPriceSnapshot] >= (0)),

    CONSTRAINT [CK_CartItems_SnapshotsNonNegative]
        CHECK ([MinOrderQtySnapshot] > (0) AND ([MaxPerDayQtySnapshot] IS NULL OR [MaxPerDayQtySnapshot] > (0))),

    CONSTRAINT [CK_CartItems_InvalidReasonRule]
        CHECK (
            ([IsValid] = (1) AND [InvalidReason] IS NULL)
            OR ([IsValid] = (0) AND [InvalidReason] IS NOT NULL)
        )
);
GO


/* Fast lookup: all items for a cart */
CREATE INDEX [IX_CartItems_CartId]
ON [dbo].[CartItems] ([CartId])
INCLUDE ([ProductId], [UnitLevel], [Quantity], [IsValid]);
GO

/* Prevent duplicate product lines per unit level within the same cart */
CREATE UNIQUE INDEX [UX_CartItems_CartId_ProductId_UnitLevel]
ON [dbo].[CartItems] ([CartId], [ProductId], [UnitLevel]);
GO

/* Useful for availability checks and analytics */
CREATE INDEX [IX_CartItems_ProductId]
ON [dbo].[CartItems] ([ProductId]);
GO

/* Fast lookup: invalid items within a cart */
CREATE INDEX [IX_CartItems_CartId_IsValid]
ON [dbo].[CartItems] ([CartId], [IsValid]);
GO