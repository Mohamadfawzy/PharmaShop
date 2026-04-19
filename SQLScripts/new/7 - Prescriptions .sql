
use pharma_shope_db;

GO

CREATE TABLE [dbo].[Prescriptions] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,

    [Status] tinyint NOT NULL CONSTRAINT [DF_Prescriptions_Status] DEFAULT ((1)), -- 1=Submitted,2=InReview,3=AwaitingApproval,4=Approved,5=Rejected,6=Closed
    [StatusUpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Prescriptions_StatusUpdatedAt] DEFAULT (sysdatetime()),

    [RejectReason] nvarchar(300) NULL, -- Reason when status=Rejected
    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Prescriptions_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Prescriptions_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Prescriptions_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_Prescriptions_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5),(6))),

    CONSTRAINT [CK_Prescriptions_RejectReasonRule]
        CHECK (
            ([Status] <> (5) AND [RejectReason] IS NULL)
            OR ([Status] = (5) AND [RejectReason] IS NOT NULL)
        )
);
GO


/* Fast lookup: customer prescriptions by status and date */
CREATE INDEX [IX_Prescriptions_CustomerId_Status_CreatedAt]
ON [dbo].[Prescriptions] ([CustomerId], [Status], [CreatedAt] DESC)
INCLUDE ([StoreId], [StatusUpdatedAt]);
GO

/* Fast lookup: store dashboard queue */
CREATE INDEX [IX_Prescriptions_StoreId_Status_StatusUpdatedAt]
ON [dbo].[Prescriptions] ([StoreId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([CustomerId]);
GO

-- --------------------------------------------
-- PrescriptionImages
-- --------------------------------------------
CREATE TABLE [dbo].[PrescriptionImages] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [PrescriptionId] int NOT NULL,

    [ImageUrl] nvarchar(600) NOT NULL,
    [ThumbnailUrl] nvarchar(600) NULL,
    [AltText] nvarchar(200) NULL,

    [SortOrder] int NOT NULL CONSTRAINT [DF_PrescriptionImages_SortOrder] DEFAULT ((0)),
    [IsPrimary] bit NOT NULL CONSTRAINT [DF_PrescriptionImages_IsPrimary] DEFAULT ((0)),

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PrescriptionImages_CreatedAt] DEFAULT (sysdatetime()),
    [CreatedBy] nvarchar(100) NULL,

    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_PrescriptionImages] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_PrescriptionImages_SortOrder]
        CHECK ([SortOrder] >= (0)),

    CONSTRAINT [FK_PrescriptionImages_Prescriptions_PrescriptionId]
        FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions] ([Id]) ON DELETE CASCADE
);
GO


/* Fast lookup: all images for a prescription */
CREATE INDEX [IX_PrescriptionImages_PrescriptionId]
ON [dbo].[PrescriptionImages] ([PrescriptionId]);
GO

/* Efficient ordering: primary first, then sort order */
CREATE INDEX [IX_PrescriptionImages_PrescriptionId_Sort]
ON [dbo].[PrescriptionImages] ([PrescriptionId], [IsPrimary] DESC, [SortOrder], [Id])
INCLUDE ([ImageUrl], [ThumbnailUrl], [AltText]);
GO

/* Enforce a single primary image per prescription */
CREATE UNIQUE INDEX [UX_PrescriptionImages_PrescriptionId_Primary]
ON [dbo].[PrescriptionImages] ([PrescriptionId], [IsPrimary])
WHERE [IsPrimary] = (1);
GO


-- --------------------------------------------
-- PrescriptionItems
-- --------------------------------------------

CREATE TABLE [dbo].[PrescriptionItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PrescriptionId] int NOT NULL,

    [ProductId] int NULL, -- Matched product (optional)
    [RequestedName] nvarchar(250) NOT NULL, -- Free text when product not matched
    [RequestedQuantity] decimal(18,3) NULL, -- Optional (if user/employee provides)
    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PrescriptionItems_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_PrescriptionItems] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_PrescriptionItems_Prescriptions_PrescriptionId]
        FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_PrescriptionItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_PrescriptionItems_RequestedQuantityPositive]
        CHECK ([RequestedQuantity] IS NULL OR [RequestedQuantity] > (0))
);
GO

/* Fast lookup: items for a prescription */
CREATE INDEX [IX_PrescriptionItems_PrescriptionId]
ON [dbo].[PrescriptionItems] ([PrescriptionId]);
GO

/* Fast lookup: matched items by product (optional reporting) */
CREATE INDEX [IX_PrescriptionItems_ProductId]
ON [dbo].[PrescriptionItems] ([ProductId])
WHERE [ProductId] IS NOT NULL;
GO