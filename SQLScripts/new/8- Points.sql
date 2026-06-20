
use pharma_shope_db;
GO

-- =====================================
-- CustomerPointLots
-- =====================================

CREATE TABLE [dbo].[CustomerPointLots] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [OrderId] int NULL,
    [OrderItemId] int NULL,

    [PointsTotal] int NOT NULL,
    [RemainingPoints] int NOT NULL,

    [Status] tinyint NOT NULL CONSTRAINT [DF_CustomerPointLots_Status] DEFAULT ((1)), -- 1=Pending,2=Available,3=Used,4=Expired,5=Cancelled
    [EarnedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerPointLots_EarnedAt] DEFAULT (sysdatetime()),
    [AvailableAt] datetime2(0) NULL,
    [ExpiresAt] datetime2(0) NOT NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerPointLots_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_CustomerPointLots] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_CustomerPointLots_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointLots_Orders_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointLots_OrderItems_OrderItemId]
        FOREIGN KEY ([OrderItemId]) REFERENCES [dbo].[OrderItems] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_CustomerPointLots_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5))),

    CONSTRAINT [CK_CustomerPointLots_Points]
        CHECK ([PointsTotal] > (0) AND [RemainingPoints] >= (0) AND [RemainingPoints] <= [PointsTotal]),

    CONSTRAINT [CK_CustomerPointLots_ExpiresAt]
        CHECK ([ExpiresAt] > [EarnedAt]),

    CONSTRAINT [CK_CustomerPointLots_AvailableRule]
        CHECK (
            ([Status] <> (2) AND [AvailableAt] IS NULL)
            OR ([Status] = (2) AND [AvailableAt] IS NOT NULL)
        )
);
GO

-- =====================================
-- CustomerPointTransactions
-- =====================================

CREATE TABLE [dbo].[CustomerPointTransactions] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,

    [LotId] int NULL,
    [OrderId] int NULL,
    [OrderItemId] int NULL,

    [TransactionType] tinyint NOT NULL, -- 1=EarnPending,2=EarnAvailable,3=Redeem,4=RefundRedeem,5=Expire,6=CancelEarn,7=ManualAdjust
    [PointsDelta] int NOT NULL,
    [BalanceAfter] int NULL,

    [PointsPerEGP] int NULL,
    [AmountEGP] decimal(18,2) NULL,

    [ExpiresAt] datetime2(0) NULL, -- Required for Earn transactions
    [ReferenceType] tinyint NULL, -- 1=Order,2=Return,3=Manual,4=ExpiryJob
    [ReferenceId] int NULL,

    [SourceTransactionId] bigint NULL,
    [CreatedByEmployeeId] int NULL,
    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerPointTransactions_CreatedAt] DEFAULT (sysdatetime()),

    CONSTRAINT [PK_CustomerPointTransactions] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_CustomerPointTransactions_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_Lots_LotId]
        FOREIGN KEY ([LotId]) REFERENCES [dbo].[CustomerPointLots] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_Orders_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_OrderItems_OrderItemId]
        FOREIGN KEY ([OrderItemId]) REFERENCES [dbo].[OrderItems] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_SourceTransactionId]
        FOREIGN KEY ([SourceTransactionId]) REFERENCES [dbo].[CustomerPointTransactions] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_Employees_CreatedByEmployeeId]
        FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_CustomerPointTransactions_Type]
        CHECK ([TransactionType] IN ((1),(2),(3),(4),(5),(6),(7))),

    CONSTRAINT [CK_CustomerPointTransactions_PointsDelta]
        CHECK (
            ([TransactionType] IN ((1),(2),(4)) AND [PointsDelta] > (0))
            OR ([TransactionType] IN ((3),(5),(6)) AND [PointsDelta] < (0))
            OR ([TransactionType] = (7) AND [PointsDelta] <> (0))
        ),

    CONSTRAINT [CK_CustomerPointTransactions_BalanceAfter]
        CHECK ([BalanceAfter] IS NULL OR [BalanceAfter] >= (0)),

    CONSTRAINT [CK_CustomerPointTransactions_Conversion]
        CHECK (
            ([PointsPerEGP] IS NULL AND [AmountEGP] IS NULL)
            OR ([PointsPerEGP] IS NOT NULL AND [PointsPerEGP] > (0) AND [AmountEGP] IS NOT NULL AND [AmountEGP] >= (0))
        ),

    CONSTRAINT [CK_CustomerPointTransactions_EarnExpiresAt]
        CHECK (
            ([TransactionType] NOT IN ((1),(2)) AND [ExpiresAt] IS NULL)
            OR ([TransactionType] IN ((1),(2)) AND [ExpiresAt] IS NOT NULL)
        ),

    CONSTRAINT [CK_CustomerPointTransactions_ReferenceType]
        CHECK ([ReferenceType] IS NULL OR [ReferenceType] IN ((1),(2),(3),(4)))
);
GO


-- ==========================
-- Indexes CustomerPointLots
-- ==========================

/* Fast lookup: available lots by customer and expiry */
CREATE INDEX [IX_CustomerPointLots_CustomerId_Status_ExpiresAt]
ON [dbo].[CustomerPointLots] ([CustomerId], [Status], [ExpiresAt])
INCLUDE ([RemainingPoints])
WHERE [RemainingPoints] > (0);
GO

/* Fast lookup: point lots generated from an order */
CREATE INDEX [IX_CustomerPointLots_OrderId]
ON [dbo].[CustomerPointLots] ([OrderId])
WHERE [OrderId] IS NOT NULL;
GO

/* Fast lookup: expirable available lots */
CREATE INDEX [IX_CustomerPointLots_Status_ExpiresAt]
ON [dbo].[CustomerPointLots] ([Status], [ExpiresAt])
INCLUDE ([CustomerId], [RemainingPoints])
WHERE [Status] = (2) AND [RemainingPoints] > (0);
GO

-- ==========================
-- Indexes CustomerPointTransactions
-- ==========================

/* Customer ledger timeline */
CREATE INDEX [IX_CustomerPointTransactions_CustomerId_CreatedAt]
ON [dbo].[CustomerPointTransactions] ([CustomerId], [CreatedAt] DESC)
INCLUDE ([TransactionType], [PointsDelta], [BalanceAfter], [OrderId], [LotId]);
GO

/* Fast lookup: transactions by order */
CREATE INDEX [IX_CustomerPointTransactions_OrderId]
ON [dbo].[CustomerPointTransactions] ([OrderId])
WHERE [OrderId] IS NOT NULL;
GO

/* Fast lookup: transactions by lot */
CREATE INDEX [IX_CustomerPointTransactions_LotId]
ON [dbo].[CustomerPointTransactions] ([LotId])
WHERE [LotId] IS NOT NULL;
GO

/* Prevent duplicate transaction per reference and type */
CREATE UNIQUE INDEX [UX_CustomerPointTransactions_Type_Reference]
ON [dbo].[CustomerPointTransactions] ([TransactionType], [ReferenceType], [ReferenceId])
WHERE [ReferenceType] IS NOT NULL AND [ReferenceId] IS NOT NULL;
GO

/* Fast lookup: earn transactions with expiry */
CREATE INDEX [IX_CustomerPointTransactions_CustomerId_ExpiresAt]
ON [dbo].[CustomerPointTransactions] ([CustomerId], [ExpiresAt])
WHERE [ExpiresAt] IS NOT NULL;
GO