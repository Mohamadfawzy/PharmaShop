use  pharma_shope_db; 
--=======================================
-- Personses
--=======================================

Go
CREATE TABLE Pharmacies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NameAr NVARCHAR(200) NOT NULL,
	NameEn NVARCHAR(200) NOT NULL,
    OwnerName NVARCHAR(150) NULL,               
    LicenseNumber NVARCHAR(100) NULL,           
    PhoneNumber NVARCHAR(20) NULL,              
    Email NVARCHAR(150) NULL,                   
    Address NVARCHAR(300) NULL,                 
	Latitude DECIMAL(10,7) NULL,               
    Longitude DECIMAL(10,7) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), 
    IsActive BIT NOT NULL DEFAULT 1            
);
go
INSERT INTO Pharmacies(NameAr,NameEn,OwnerName,LicenseNumber,PhoneNumber,Email,Address,Latitude,Longitude)
VALUES
(
    N'صيدلية الشفاء', N'Al Shifa Pharmacy',
	N'محمد أحمد',N'PH-2024-001',
	'01012345678','info@alshifa.com',
	N'القاهرة - مدينة نصر - شارع مصطفى النحاس',
    30.0566100,31.3304300
);

GO
CREATE TABLE [dbo].[Customers] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] int NULL,

    [FullNameAr] nvarchar(200) NOT NULL,
    [FullNameEn] nvarchar(200) NOT NULL,

    [PhoneNumber] nvarchar(20) NULL,
    [Gender] nvarchar(10) NULL,
    [DateOfBirth] date NULL,

    [Email] nvarchar(150) NULL,
    [NationalId] nvarchar(20) NULL,

    [CustomerType] nvarchar(50) NOT NULL CONSTRAINT [DF_Customers_CustomerType] DEFAULT (N'Regular'), -- Regular/VIP/Corporate

    [Points] int NOT NULL CONSTRAINT [DF_Customers_Points] DEFAULT ((0)),
    [PointsExpiryDate] datetime2(0) NULL,

    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Customers_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Customers_IsActive] DEFAULT ((1)),

    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Customers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
	
	
    CONSTRAINT [CK_Customers_Gender_Allowed]
        CHECK ([Gender] IS NULL OR [Gender] IN (N'Male', N'Female')),

    CONSTRAINT [CK_Customers_CustomerType_Allowed]
        CHECK ([CustomerType] IN (N'Regular', N'VIP', N'Corporate')),

    CONSTRAINT [CK_Customers_Points_NonNegative]
        CHECK ([Points] >= (0))


);
GO
/* Enforce one customer per AspNetUser (ignore NULLs) */
CREATE UNIQUE INDEX [UX_Customers_UserId]
ON [dbo].[Customers] ([UserId])
WHERE [UserId] IS NOT NULL;
GO

/* Phone lookup (useful for login/OTP or search) */
CREATE INDEX [IX_Customers_PhoneNumber]
ON [dbo].[Customers] ([PhoneNumber])
WHERE [PhoneNumber] IS NOT NULL;
GO

/* Email lookup (optional search/login) */
CREATE INDEX [IX_Customers_Email]
ON [dbo].[Customers] ([Email])
WHERE [Email] IS NOT NULL;
GO
GO

CREATE TABLE [dbo].[CustomerAddresses] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [Title] nvarchar(100) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Region] nvarchar(100) NULL,
    [Street] nvarchar(300) NOT NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [IsDefault] bit NOT NULL CONSTRAINT [DF_CustomerAddresses_IsDefault] DEFAULT ((0)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerAddresses_CreatedAt] DEFAULT (sysdatetime()),
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerAddresses_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION
);
GO

/* Fast lookup: all addresses for a customer */
CREATE INDEX [IX_CustomerAddresses_CustomerId]
ON [dbo].[CustomerAddresses] ([CustomerId]);
GO

/* Fast lookup: default address for a customer */
CREATE INDEX [IX_CustomerAddresses_CustomerId_IsDefault]
ON [dbo].[CustomerAddresses] ([CustomerId], [IsDefault]);
GO

/* Enforce a single default address per customer */
CREATE UNIQUE INDEX [UX_CustomerAddresses_CustomerId_Default]
ON [dbo].[CustomerAddresses] ([CustomerId])
WHERE [IsDefault] = (1);
GO

GO

CREATE TABLE [dbo].[Employees] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] int NULL,
    [PharmacyId] int NOT NULL,

    [FullNameAr] nvarchar(200) NOT NULL,
    [FullNameEn] nvarchar(200) NULL,

    [PhoneNumber] nvarchar(20) NULL,
    [Email] nvarchar(150) NULL,
    [NationalId] nvarchar(20) NULL,

    [JobTitle] nvarchar(100) NULL,
    [EmployeeCode] nvarchar(50) NULL,

    [IsActive] bit NOT NULL CONSTRAINT [DF_Employees_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Employees_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete

    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Employees_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Employees_Pharmacies_PharmacyId]
        FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);
GO

/* Enforce one employee per AspNetUser (ignore NULLs) */
CREATE UNIQUE INDEX [UX_Employees_UserId]
ON [dbo].[Employees] ([UserId])
WHERE [UserId] IS NOT NULL AND [DeletedAt] IS NULL;
GO

/* Fast filtering by pharmacy (tenant) */
CREATE INDEX [IX_Employees_PharmacyId]
ON [dbo].[Employees] ([PharmacyId])
INCLUDE ([FullNameAr], [PhoneNumber], [IsActive])
WHERE [DeletedAt] IS NULL;
GO

/* Phone lookup (useful for search/login/OTP) */
CREATE INDEX [IX_Employees_PhoneNumber]
ON [dbo].[Employees] ([PhoneNumber])
WHERE [PhoneNumber] IS NOT NULL AND [DeletedAt] IS NULL;
GO

/* Email lookup (optional search/login) */
CREATE INDEX [IX_Employees_Email]
ON [dbo].[Employees] ([Email])
WHERE [Email] IS NOT NULL AND [DeletedAt] IS NULL;
GO

/* Employee code lookup (optional) */
CREATE INDEX [IX_Employees_EmployeeCode]
ON [dbo].[Employees] ([EmployeeCode])
WHERE [EmployeeCode] IS NOT NULL AND [DeletedAt] IS NULL;
GO

GO

--=======================================
-- Categories
--=======================================
CREATE TABLE [dbo].[Categories] (
    [Id] int NOT NULL IDENTITY(1,1),
    [ParentCategoryId] int NULL,
    [NameAr] nvarchar(200) NOT NULL,
    [NameEn] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [ImageId] nvarchar(128) NULL,
    [ImageFormat] tinyint NULL,
    [ImageVersion] int NOT NULL CONSTRAINT [DF_Categories_ImageVersion] DEFAULT ((0)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Categories_IsActive] DEFAULT ((1)),
    [IsDeleted] bit NOT NULL CONSTRAINT [DF_Categories_IsDeleted] DEFAULT ((0)),
    [CreatedAt] datetime NOT NULL CONSTRAINT [DF_Categories_CreatedAt] DEFAULT (getdate()),
    [UpdatedAt] datetime NULL,
    [ImageUpdatedAt] datetime NULL,

    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_Categories_ImageFormat]
        CHECK ([ImageFormat] IS NULL OR [ImageFormat] IN ((1), (2), (3))),

    CONSTRAINT [FK_Categories_ParentCategory]
        FOREIGN KEY ([ParentCategoryId]) REFERENCES [dbo].[Categories] ([Id]) ON DELETE NO ACTION
);
GO

/* Tree traversal support (parent-child queries) */
CREATE INDEX [IX_Categories_ParentCategoryId]
ON [dbo].[Categories] ([ParentCategoryId]);
GO

/* Optional: ImageId lookup (filtered) */
CREATE INDEX [IX_Categories_ImageId]
ON [dbo].[Categories] ([ImageId])
WHERE [ImageId] IS NOT NULL;
GO



--========================================================
  --Units (Lookup)
  -- Dictionary of unit types used across the system
  -- e.g., Box / Strip / Ampoule / Tablet / Bottle ...
--========================================================
GO
CREATE TABLE [dbo].[Units] (
    [Id] int NOT NULL IDENTITY(1,1),
    [Code] varchar(30) NOT NULL, -- Stable code (e.g., BOX, STRIP)
    [NameAr] nvarchar(60) NOT NULL,
    [NameEn] nvarchar(60) NOT NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Units_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Units_CreatedAt] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL, -- Concurrency
    CONSTRAINT [PK_Units] PRIMARY KEY ([Id])
);
GO

/* Unique stable unit code */
CREATE UNIQUE INDEX [UX_Units_Code]
ON [dbo].[Units] ([Code]);
GO

/* Optional: unique English name */
CREATE UNIQUE INDEX [UX_Units_NameEn]
ON [dbo].[Units] ([NameEn]);
GO

-- Optional seed data
INSERT INTO dbo.Units (Code, NameAr, NameEn)
VALUES
('BOX',     N'علبة',  N'Box'),
('STRIP',   N'شريط',  N'Strip'),
('AMPOULE', N'أمبول', N'Ampoule'),
('BOTTLE',  N'زجاجة', N'Bottle'),
('TUBE',    N'أنبوبة',N'Tube'),
('TABLET',  N'قرص',   N'Tablet'),
('CAPSULE', N'كبسولة',N'Capsule');
GO

-- ========================================================
 -- Companies
-- ======================================================== 

CREATE TABLE [dbo].[Companies] (
    [Id] int NOT NULL IDENTITY(1,1),
    [NameAr] nvarchar(150) NOT NULL,
    [NameEn] nvarchar(150) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Companies_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Companies_CreatedAt] DEFAULT (SYSDATETIME()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,
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

-- ========================================================
 -- Stores
-- ======================================================== 

CREATE TABLE [dbo].[Stores] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PharmacyId] int NOT NULL,
    [NameAr] nvarchar(150) NOT NULL,
    [NameEn] nvarchar(150) NOT NULL,
    [Code] varchar(30) NULL,
    [Address] nvarchar(300) NULL,
    [IsDefault] bit NOT NULL CONSTRAINT [DF_Stores_IsDefault] DEFAULT ((0)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Stores_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Stores_CreatedAt] DEFAULT (sysutcdatetime()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Stores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Stores_Pharmacies_PharmacyId] FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);
GO

/* Unique store code per pharmacy (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_Stores_PharmacyId_Code]
ON [dbo].[Stores] ([PharmacyId], [Code])
WHERE [Code] IS NOT NULL AND [Code] <> '' AND [DeletedAt] IS NULL;
GO

/* Fast listing by pharmacy and active flag (ignoring soft-deleted rows) */
CREATE INDEX [IX_Stores_PharmacyId_IsActive]
ON [dbo].[Stores] ([PharmacyId], [IsActive])
INCLUDE ([NameAr], [NameEn], [IsDefault], [Code])
WHERE [DeletedAt] IS NULL;
GO

/* Enforce a single default store per pharmacy (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_Stores_PharmacyId_Default]
ON [dbo].[Stores] ([PharmacyId])
WHERE [IsDefault] = (1) AND [DeletedAt] IS NULL;
GO

/* Name search (Arabic) within pharmacy (ignoring soft-deleted rows) */
CREATE INDEX [IX_Stores_PharmacyId_NameAr]
ON [dbo].[Stores] ([PharmacyId], [NameAr])
WHERE [DeletedAt] IS NULL;
GO

/* Name search (English) within pharmacy (ignoring soft-deleted rows) */
CREATE INDEX [IX_Stores_PharmacyId_NameEn]
ON [dbo].[Stores] ([PharmacyId], [NameEn])
WHERE [DeletedAt] IS NULL;
GO


-- ===========================================================
-- Products
-- =========================================================== 

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

CREATE TABLE [dbo].[Products] (
    [Id] int NOT NULL IDENTITY(1,1),
    [StoreId] int NOT NULL,
    [CategoryId] int NOT NULL,
    [CompanyId] int NULL,

    [ErpProductId] decimal(18,0) NULL,
    [InternationalCode] varchar(50) NULL,

    [NameAr] nvarchar(250) NOT NULL,
    [NameEn] nvarchar(250) NULL,
    [DescriptionAr] nvarchar(max) NULL,
    [DescriptionEn] nvarchar(max) NULL,
    [SearchKeywords] nvarchar(500) NULL,

    [OuterUnitId] int NOT NULL,
    [InnerUnitId] int NULL,
    [InnerPerOuter] int NULL,

    [OuterUnitPrice] decimal(18,2) NOT NULL CONSTRAINT [DF_Products_OuterUnitPrice] DEFAULT ((0)),
    [InnerUnitPrice] decimal(18,2) NULL CONSTRAINT [DF_Products_InnerUnitPrice] DEFAULT ((0)),

    [MinOrderQty] int NOT NULL CONSTRAINT [DF_Products_MinOrderQty] DEFAULT ((1)),
    [MaxOrderQty] int NULL,
    [MaxPerDayQty] int NULL,

    [IsReturnable] bit NOT NULL CONSTRAINT [DF_Products_IsReturnable] DEFAULT ((1)),
    [AllowSplitSale] bit NOT NULL CONSTRAINT [DF_Products_AllowSplitSale] DEFAULT ((0)),

    [Quantity] decimal(18,3) NOT NULL CONSTRAINT [DF_Products_Quantity] DEFAULT ((0)), -- Stored in OUTER units (e.g., 2.500)
    [HasExpiry] bit NOT NULL CONSTRAINT [DF_Products_HasExpiry] DEFAULT ((1)),
    [NearestExpiryDate] date NULL,
    [LastStockSyncAt] datetime2(0) NULL,

    [HasPromotion] bit NOT NULL CONSTRAINT [DF_Products_HasPromotion] DEFAULT ((0)),
    [PromotionDiscountPercent] decimal(5,2) NOT NULL CONSTRAINT [DF_Products_PromotionDiscountPercent] DEFAULT ((0)),
    [PromotionStartsAt] datetime2(0) NULL,
    [PromotionEndsAt] datetime2(0) NULL,

    [IsFeatured] bit NOT NULL CONSTRAINT [DF_Products_IsFeatured] DEFAULT ((0)),

    [IsIntegrated] bit NOT NULL CONSTRAINT [DF_Products_IsIntegrated] DEFAULT ((0)),
    [IntegratedAt] datetime2(0) NULL,

    [Points] int NOT NULL CONSTRAINT [DF_Products_PointsOuter] DEFAULT ((0)),

    [RequiresPrescription] bit NOT NULL CONSTRAINT [DF_Products_RequiresPrescription] DEFAULT ((0)),
    [IsAvailable] bit NOT NULL CONSTRAINT [DF_Products_IsAvailable] DEFAULT ((0)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT ((1)),

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete

    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_Products_QuantityNonNegative]
        CHECK ([Quantity] >= (0)),

    CONSTRAINT [CK_Products_PricesNonNegative]
        CHECK ([OuterUnitPrice] >= (0) AND ([InnerUnitPrice] IS NULL OR [InnerUnitPrice] >= (0))),

    CONSTRAINT [CK_Products_PromotionPercent]
        CHECK ([PromotionDiscountPercent] >= (0) AND [PromotionDiscountPercent] <= (100)),

    CONSTRAINT [CK_Products_PromotionDates]
        CHECK (
            ([PromotionStartsAt] IS NULL AND [PromotionEndsAt] IS NULL)
            OR ([PromotionStartsAt] IS NOT NULL AND [PromotionEndsAt] IS NOT NULL AND [PromotionEndsAt] > [PromotionStartsAt])
        ),

    CONSTRAINT [CK_Products_OrderLimits]
        CHECK (
            [MinOrderQty] > (0)
            AND ([MaxOrderQty] IS NULL OR [MaxOrderQty] >= [MinOrderQty])
            AND ([MaxPerDayQty] IS NULL OR [MaxPerDayQty] > (0))
        ),

    CONSTRAINT [CK_Products_InnerRules]
        CHECK (
            ([InnerUnitId] IS NULL AND [InnerPerOuter] IS NULL)
            OR ([InnerUnitId] IS NOT NULL AND [InnerPerOuter] IS NOT NULL AND [InnerPerOuter] >= (1))
        )
);
GO

 -- -------------------- Foreign Keys --------------------  
ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Stores
    FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Categories
    FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Companies
    FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Units_Outer
    FOREIGN KEY (OuterUnitId) REFERENCES dbo.Units(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Units_Inner
    FOREIGN KEY (InnerUnitId) REFERENCES dbo.Units(Id);
GO



 -- -------------------- Indexes --------------------  

-- Unique InternationalCode per Store (filtered: allow NULL, ignore soft-deleted)
CREATE UNIQUE INDEX UX_Products_Store_InternationalCode
ON dbo.Products(StoreId, InternationalCode)
WHERE InternationalCode IS NOT NULL AND InternationalCode <> '' AND DeletedAt IS NULL;
GO

-- Fast listing for store/category/active/available
CREATE INDEX IX_Products_List
ON dbo.Products(StoreId, CategoryId, IsActive, IsAvailable)
INCLUDE (NameAr, NameEn, OuterUnitPrice, InnerUnitPrice, Quantity, HasPromotion, PromotionDiscountPercent)
WHERE DeletedAt IS NULL;
GO

-- Search by Arabic name
CREATE INDEX IX_Products_NameAr
ON dbo.Products(StoreId, NameAr)
WHERE DeletedAt IS NULL;
GO

-- Search by English name (if present)
CREATE INDEX IX_Products_NameEn
ON dbo.Products(StoreId, NameEn)
WHERE DeletedAt IS NULL AND NameEn IS NOT NULL;
GO

-- Offers browsing
CREATE INDEX IX_Products_Promotions
ON dbo.Products(StoreId, HasPromotion, PromotionEndsAt)
INCLUDE (NameAr, OuterUnitPrice, InnerUnitPrice, PromotionDiscountPercent)
WHERE DeletedAt IS NULL AND HasPromotion = 1;
GO

-- ERP mapping index (useful for sync)
CREATE INDEX IX_Products_Store_ErpProductId
ON dbo.Products(StoreId, ErpProductId)
WHERE DeletedAt IS NULL AND ErpProductId IS NOT NULL;
GO

CREATE TABLE [dbo].[ProductImages] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [PharmacyId] int NOT NULL,
    [ProductId] int NOT NULL,

    [ImageUrl] nvarchar(600) NOT NULL,
    [ThumbnailUrl] nvarchar(600) NULL,
    [AltText] nvarchar(200) NULL,

    [SortOrder] int NOT NULL CONSTRAINT [DF_ProductImages_SortOrder] DEFAULT ((0)),
    [IsPrimary] bit NOT NULL CONSTRAINT [DF_ProductImages_IsPrimary] DEFAULT ((0)),

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ProductImages_CreatedAt] DEFAULT (sysdatetime()),
    [CreatedBy] nvarchar(100) NULL,

    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,

    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_ProductImages_SortOrder]
        CHECK ([SortOrder] >= (0)),

    CONSTRAINT [FK_ProductImages_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_ProductImages_Pharmacies_PharmacyId]
        FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);
GO


-- Fast lookup by tenant and product (ignoring soft-deleted rows) 
CREATE INDEX [IX_ProductImages_PharmacyId_ProductId]
ON [dbo].[ProductImages] ([PharmacyId], [ProductId])
WHERE [DeletedAt] IS NULL;
GO

-- Efficient ordering: primary first, then sort order (ignoring soft-deleted rows) 
CREATE INDEX [IX_ProductImages_ProductId_Sort]
ON [dbo].[ProductImages] ([ProductId], [IsPrimary] DESC, [SortOrder], [Id])
INCLUDE ([ImageUrl], [ThumbnailUrl], [AltText])
WHERE [DeletedAt] IS NULL;
GO

-- Enforce a single primary image per product (ignoring soft-deleted rows) 
CREATE UNIQUE INDEX [UX_ProductImages_ProductId_Primary]
ON [dbo].[ProductImages] ([ProductId], [IsPrimary])
WHERE [IsPrimary] = (1) AND [DeletedAt] IS NULL;
GO

-- Simple FK helper index for ORMs/scaffolding (ignoring soft-deleted rows) 
CREATE INDEX [IX_ProductImages_ProductId]
ON [dbo].[ProductImages] ([ProductId])
WHERE [DeletedAt] IS NULL;
GO





