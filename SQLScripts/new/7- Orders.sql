
use pharma_shope_db;
GO

CREATE TABLE [dbo].[Orders] (
    [Id] int NOT NULL IDENTITY(1,1),
    [OrderNumber] nvarchar(30) NOT NULL, -- Human-readable order number
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,
    [OrderSource] tinyint NOT NULL CONSTRAINT [DF_Orders_OrderSource] DEFAULT ((1)), -- 1=Cart,2=Prescription,3=PointsRedemption

    [PrescriptionId] int NULL, -- FK to Prescriptions (nullable)

    [Status] tinyint NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT ((1)), -- 1=Pending,2=Confirmed,3=Preparing,4=OutForDelivery,5=Delivered,6=Cancelled,7=Returned,8=PendingApproval
    [StatusUpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Orders_StatusUpdatedAt] DEFAULT (sysdatetime()),

    [PaymentMethod] tinyint NOT NULL CONSTRAINT [DF_Orders_PaymentMethod] DEFAULT ((1)), -- 1=COD
    [PaymentStatus] tinyint NOT NULL CONSTRAINT [DF_Orders_PaymentStatus] DEFAULT ((1)), -- 1=Unpaid,2=Paid,3=Refunded

    [Subtotal] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_Subtotal] DEFAULT ((0)),
    [ItemsDiscountTotal] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_ItemsDiscountTotal] DEFAULT ((0)),
    [DeliveryFee] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_DeliveryFee] DEFAULT ((0)),
    [RedeemedPoints] int NOT NULL CONSTRAINT [DF_Orders_RedeemedPoints] DEFAULT ((0)),
    [RedeemedAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_RedeemedAmount] DEFAULT ((0)),
    [GrandTotal] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_GrandTotal] DEFAULT ((0)),
    [EarnedPoints] int NOT NULL CONSTRAINT [DF_Orders_EarnedPoints] DEFAULT ((0)),

    [AddressId] int NULL, -- FK to CustomerAddresses (optional)
    [DeliveryTitle] nvarchar(100) NULL,
    [DeliveryCity] nvarchar(100) NOT NULL,
    [DeliveryRegion] nvarchar(100) NULL,
    [DeliveryStreet] nvarchar(300) NOT NULL,
    [DeliveryLatitude] float NULL,
    [DeliveryLongitude] float NULL,
    [DeliveryPhone] nvarchar(20) NULL,

    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Orders_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Orders_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Orders_CustomerAddresses_AddressId]
        FOREIGN KEY ([AddressId]) REFERENCES [dbo].[CustomerAddresses] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Orders_Prescriptions_PrescriptionId]
        FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions] ([Id]) ON DELETE NO ACTION,

    
    CONSTRAINT [CK_Orders_OrderSource]
    CHECK ([OrderSource] IN ((1),(2),(3))),

    CONSTRAINT [CK_Orders_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5),(6),(7),(8))),

    CONSTRAINT [CK_Orders_PendingApproval_RequiresPrescription]
        CHECK ([Status] <> (8) OR ([Status] = (8) AND [PrescriptionId] IS NOT NULL)),

    CONSTRAINT [CK_Orders_PaymentMethod]
        CHECK ([PaymentMethod] IN ((1))), -- extend later

    CONSTRAINT [CK_Orders_PaymentStatus]
        CHECK ([PaymentStatus] IN ((1),(2),(3))),

    CONSTRAINT [CK_Orders_Totals_NonNegative]
        CHECK (
            [Subtotal] >= (0)
            AND [ItemsDiscountTotal] >= (0)
            AND [DeliveryFee] >= (0)
            AND [RedeemedAmount] >= (0)
            AND [GrandTotal] >= (0)
        ),

    CONSTRAINT [CK_Orders_Points_NonNegative]
        CHECK ([RedeemedPoints] >= (0) AND [EarnedPoints] >= (0))
);
GO

/* Unique order number */
CREATE UNIQUE INDEX [UX_Orders_OrderNumber]
ON [dbo].[Orders] ([OrderNumber]);
GO

/* Enforce one order per prescription (when PrescriptionId is present) */
CREATE UNIQUE INDEX [UX_Orders_PrescriptionId]
ON [dbo].[Orders] ([PrescriptionId])
WHERE [PrescriptionId] IS NOT NULL;
GO

/* Fast lookup: customer orders by status and date */
CREATE INDEX [IX_Orders_CustomerId_Status_CreatedAt]
ON [dbo].[Orders] ([CustomerId], [Status], [CreatedAt] DESC)
INCLUDE ([OrderNumber], [GrandTotal], [PaymentStatus], [StoreId], [PrescriptionId]);
GO

/* Fast lookup: store orders queue (preparing/delivery) */
CREATE INDEX [IX_Orders_StoreId_Status_StatusUpdatedAt]
ON [dbo].[Orders] ([StoreId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([OrderNumber], [CustomerId], [GrandTotal], [PaymentStatus], [PrescriptionId]);
GO

/* Fast lookup: payment status reporting */
CREATE INDEX [IX_Orders_PaymentStatus_CreatedAt]
ON [dbo].[Orders] ([PaymentStatus], [CreatedAt] DESC)
INCLUDE ([OrderNumber], [CustomerId], [GrandTotal], [PaymentMethod], [PrescriptionId]);
GO

CREATE TABLE [dbo].[OrderItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,

    [UnitLevel] tinyint NOT NULL, -- 1=Outer,2=Inner
    [Quantity] decimal(18,3) NOT NULL,

    [OuterUnitIdSnapshot] int NOT NULL,
    [InnerUnitIdSnapshot] int NULL,
    [InnerPerOuterSnapshot] int NULL,

    [UnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_UnitPriceSnapshot] DEFAULT ((0)),
    [DiscountPercentSnapshot] decimal(5,2) NOT NULL CONSTRAINT [DF_OrderItems_DiscountPercentSnapshot] DEFAULT ((0)),
    [FinalUnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_FinalUnitPriceSnapshot] DEFAULT ((0)),
    [LineTotal] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_LineTotal] DEFAULT ((0)),

    [PointsSnapshot] int NOT NULL CONSTRAINT [DF_OrderItems_PointsSnapshot] DEFAULT ((0)),
    [EarnedPoints] int NOT NULL CONSTRAINT [DF_OrderItems_EarnedPoints] DEFAULT ((0)),

    [AppliedPromotionId] int NULL, -- Group promotion applied (if any)

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_OrderItems_CreatedAt] DEFAULT (sysdatetime()),

    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_OrderItems_Orders_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_OrderItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_OrderItems_Promotions_AppliedPromotionId]
        FOREIGN KEY ([AppliedPromotionId]) REFERENCES [dbo].[Promotions] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_OrderItems_UnitLevel]
        CHECK ([UnitLevel] IN ((1),(2))),

    CONSTRAINT [CK_OrderItems_QuantityPositive]
        CHECK ([Quantity] > (0)),

    CONSTRAINT [CK_OrderItems_DiscountPercent]
        CHECK ([DiscountPercentSnapshot] >= (0) AND [DiscountPercentSnapshot] <= (100)),

    CONSTRAINT [CK_OrderItems_PricesNonNegative]
        CHECK (
            [UnitPriceSnapshot] >= (0)
            AND [FinalUnitPriceSnapshot] >= (0)
            AND [LineTotal] >= (0)
        ),

    CONSTRAINT [CK_OrderItems_PointsNonNegative]
        CHECK ([PointsSnapshot] >= (0) AND [EarnedPoints] >= (0)),

    CONSTRAINT [CK_OrderItems_InnerSnapshotRules]
        CHECK (
            ([InnerUnitIdSnapshot] IS NULL AND [InnerPerOuterSnapshot] IS NULL)
            OR ([InnerUnitIdSnapshot] IS NOT NULL AND [InnerPerOuterSnapshot] IS NOT NULL AND [InnerPerOuterSnapshot] >= (1))
        )
);
GO


/* Fast lookup: all items for an order */
CREATE INDEX [IX_OrderItems_OrderId]
ON [dbo].[OrderItems] ([OrderId])
INCLUDE ([ProductId], [UnitLevel], [Quantity], [FinalUnitPriceSnapshot], [LineTotal], [EarnedPoints], [AppliedPromotionId]);
GO

/* Prevent duplicate lines for same product + unit within the same order (optional but recommended) */
CREATE UNIQUE INDEX [UX_OrderItems_OrderId_ProductId_UnitLevel]
ON [dbo].[OrderItems] ([OrderId], [ProductId], [UnitLevel]);
GO

/* Useful for reporting: product sales lines */
CREATE INDEX [IX_OrderItems_ProductId_CreatedAt]
ON [dbo].[OrderItems] ([ProductId], [CreatedAt] DESC)
INCLUDE ([Quantity], [LineTotal], [UnitLevel]);
GO