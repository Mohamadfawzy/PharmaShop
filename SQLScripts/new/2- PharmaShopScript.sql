use  pharma_shope_db; 
--=======================================
-- Personses
--=======================================

Go
CREATE TABLE Pharmacies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
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
INSERT INTO Pharmacies(Name,NameEn,OwnerName,LicenseNumber,PhoneNumber,Email,Address,Latitude,Longitude)
VALUES
(
    N'صيدلية الشفاء', N'Al Shifa Pharmacy',
	N'محمد أحمد',N'PH-2024-001',
	'01012345678','info@alshifa.com',
	N'القاهرة - مدينة نصر - شارع مصطفى النحاس',
    30.0566100,31.3304300
);

GO

CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT UNIQUE NULL, 
    PharmacyId INT NULL,                    -- العميل ينتمي لأي صيدلية
    FullName NVARCHAR(200) NOT NULL,  
    FullNameEn NVARCHAR(200) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,             
    Gender NVARCHAR(10) NULL,                
    DateOfBirth DATE NULL,

    Email NVARCHAR(150) NULL,
    NationalId NVARCHAR(20) NULL,               

    CustomerType NVARCHAR(50) DEFAULT 'Regular', -- Regular / VIP / Corporate

    Points INT NOT NULL DEFAULT 0,              -- عدد النقاط المتاحة
    PointsExpiryDate DATETIME NULL,             -- أقرب تاريخ انتهاء للنقاط

    Notes NVARCHAR(500) NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,

	CONSTRAINT FK_Customers_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    CONSTRAINT FK_Customers_Pharmacies FOREIGN KEY (PharmacyId) REFERENCES Pharmacies(Id)
);

GO

CREATE TABLE CustomerAddresses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    Title NVARCHAR(100) NOT NULL,      
    City NVARCHAR(100) NOT NULL,
    Region NVARCHAR(100) NULL,         
    Street NVARCHAR(300) NOT NULL,
    Latitude FLOAT NULL,               
    Longitude FLOAT NULL,
    IsDefault BIT NOT NULL DEFAULT 0,  
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_CustomerAddresses_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

GO

-- الصيدلي
CREATE TABLE Pharmacists (
    Id INT PRIMARY KEY IDENTITY,
    FullName NVARCHAR(100),
    FullNameEn NVARCHAR(200) NOT NULL,
    Specialty NVARCHAR(100),
    UserId INT UNIQUE NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

GO

--=======================================
-- Categories
--=======================================

CREATE TABLE dbo.Categories
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Categories PRIMARY KEY,
	
	ParentCategoryId INT NULL,

    Name NVARCHAR(200) NOT NULL,
    NameEn NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,

    -- Stable image identifier (store only the id, not a URL/path)
    ImageId NVARCHAR(128) NULL,

    -- Stored format enum:
    -- 1 = Jpeg, 2 = Png, 3 = Webp
    ImageFormat TINYINT NULL
        CONSTRAINT CK_Categories_ImageFormat CHECK (ImageFormat IS NULL OR ImageFormat IN (1, 2, 3)),

    -- Cache busting (increment when image content changes)
    ImageVersion INT NOT NULL
        CONSTRAINT DF_Categories_ImageVersion DEFAULT (0),

    -- =========================
    -- Standard fields
    -- =========================

    IsActive BIT NOT NULL
        CONSTRAINT DF_Categories_IsActive DEFAULT (1),

    IsDeleted BIT NOT NULL
        CONSTRAINT DF_Categories_IsDeleted DEFAULT (0),

    -- Use UTC timestamps for consistency across servers
    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Categories_CreatedAt DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,
	
	-- Last time the image content was updated (UTC)
    ImageUpdatedAt DATETIME NULL,

    CONSTRAINT FK_Categories_ParentCategory
        FOREIGN KEY (ParentCategoryId) REFERENCES dbo.Categories(Id)
);
GO

-- Helpful index for tree queries (parent-child traversal)
CREATE INDEX IX_Categories_ParentCategoryId
       ON dbo.Categories(ParentCategoryId);

GO

-- Optional index if you frequently query by ImageId (not always needed)
CREATE INDEX IX_Categories_ImageId
	   ON dbo.Categories(ImageId) WHERE ImageId IS NOT NULL;

GO



--========================================================
  --Units (Lookup)
  -- Dictionary of unit types used across the system
  -- e.g., Box / Strip / Ampoule / Tablet / Bottle ...
--========================================================
GO
CREATE TABLE dbo.Units
(
    Id          INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Units PRIMARY KEY,          -- معرف الوحدة

    Code        VARCHAR(30) NOT NULL,             -- كود ثابت (BOX, STRIP, AMPOULE...)
    NameAr      NVARCHAR(60) NOT NULL,            -- اسم عربي (علبة/شريط/أمبول)
    NameEn      NVARCHAR(60) NOT NULL,            -- اسم إنجليزي (Box/Strip/Ampoule)

    IsActive    BIT NOT NULL CONSTRAINT DF_Units_IsActive DEFAULT (1), -- نشطة؟
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Units_CreatedAt DEFAULT (SYSUTCDATETIME()), -- تاريخ الإنشاء

    RowVersion  ROWVERSION NOT NULL               -- Concurrency
);
GO

-- Code يجب أن يكون Unique على مستوى النظام (Lookup)
CREATE UNIQUE INDEX UX_Units_Code
ON dbo.Units(Code);
GO

-- (اختياري) منع تكرار الاسم الإنجليزي
CREATE UNIQUE INDEX UX_Units_NameEn
ON dbo.Units(NameEn);
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





 --========================================================
  Brands
  - Brand per Pharmacy (optional but useful)
======================================================== 

GO
CREATE TABLE dbo.Companies
(
    Id          INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Companies PRIMARY KEY,

    PharmacyId  INT NOT NULL,  -- Tenant key

    NameAr      NVARCHAR(150) NOT NULL,
    NameEn      NVARCHAR(150) NULL,

    IsActive    BIT NOT NULL
        CONSTRAINT DF_Companies_IsActive DEFAULT (1),

    CreatedAt   DATETIME2(0) NOT NULL
        CONSTRAINT DF_Companies_CreatedAt DEFAULT (SYSUTCDATETIME()),

    DeletedAt   DATETIME2(0) NULL,   -- Soft delete
    DeletedBy   NVARCHAR(100) NULL,

    RowVersion  ROWVERSION NOT NULL, -- Concurrency

    CONSTRAINT FK_Companies_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

-- Unique per tenant (ignoring soft-deleted rows)
CREATE UNIQUE INDEX UX_Companies_Pharmacy_NameAr
ON dbo.Companies (PharmacyId, NameAr)
WHERE DeletedAt IS NULL;
GO
-- Search / sort by English name (if present)
CREATE INDEX IX_Companies_Pharmacy_NameEn
ON dbo.Companies (PharmacyId, NameEn)
WHERE DeletedAt IS NULL AND NameEn IS NOT NULL;
GO

-- Fast listing by tenant and active flag
CREATE INDEX IX_Companies_Pharmacy_IsActive
ON dbo.Companies (PharmacyId, IsActive)
INCLUDE (NameAr, NameEn, CreatedAt)
WHERE DeletedAt IS NULL;
GO

-- Optional: enforce unique English name per tenant
CREATE UNIQUE INDEX UX_Companies_Pharmacy_NameEn
ON dbo.Companies (PharmacyId, NameEn)
WHERE DeletedAt IS NULL AND NameEn IS NOT NULL AND NameEn <> N'';
GO


 --===========================================================
  dbo.Products (Improved)
  - Default sale unit = Outer (big unit)
  - Optional inner unit with its own price
  - Stock quantity stored in Outer units as DECIMAL:
      Example: InnerPerOuter=4, 2 boxes + 2 strips => 2.500
=========================================================== 

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

CREATE TABLE dbo.Products
(
     -- -------------------- Identity / Location --------------------  
    Id                INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Products PRIMARY KEY,                 -- معرف المنتج

    StoreId           INT NOT NULL,                         -- الفرع/المخزن الذي يخدم المتجر

    CategoryId        INT NOT NULL,                         -- التصنيف
    CompanyId         INT NULL,                             -- الشركة (اختياري)

     -- -------------------- ERP / Integration --------------------  
    ErpProductId      DECIMAL(18,0) NULL,                   -- product_id في ERP
    InternationalCode VARCHAR(50) NULL,                     -- كود دولي

     -- -------------------- Names / Content --------------------  
    NameAr            NVARCHAR(250) NOT NULL,               -- اسم عربي
    NameEn            NVARCHAR(250) NULL,                   -- اسم إنجليزي (اختياري)

    DescriptionAr     NVARCHAR(MAX) NULL,                   -- وصف عربي
    DescriptionEn     NVARCHAR(MAX) NULL,                   -- وصف إنجليزي

    SearchKeywords    NVARCHAR(500) NULL,                   -- كلمات مفتاحية للبحث

     -- -------------------- Units & Pricing (Outer default) --------------------  
    OuterUnitId       INT NOT NULL,                         -- الوحدة الكبرى (Box/Bottle...)
    InnerUnitId       INT NULL,                             -- الوحدة الصغرى (Strip/Tablet/Ampoule...) إن وُجدت

    InnerPerOuter     INT NULL,                             -- عدد Inner داخل Outer (مثال 4 شرائط/علبة)

    OuterUnitPrice    DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Products_OuterUnitPrice DEFAULT (0),  -- سعر الوحدة الكبرى

    InnerUnitPrice    DECIMAL(18,2) NULL
        CONSTRAINT DF_Products_InnerUnitPrice DEFAULT (0),  -- سعر الوحدة الصغرى (إن وُجدت)

     -- -------------------- Order limits --------------------  
    MinOrderQty       INT NOT NULL
        CONSTRAINT DF_Products_MinOrderQty DEFAULT (1),     -- أقل كمية
    MaxOrderQty       INT NULL,                             -- أقصى كمية
    MaxPerDayQty      INT NULL,                             -- أقصى يوميًا

    IsReturnable      BIT NOT NULL
        CONSTRAINT DF_Products_IsReturnable DEFAULT (1),    -- قابل للإرجاع؟

    AllowSplitSale    BIT NOT NULL
        CONSTRAINT DF_Products_AllowSplitSale DEFAULT (0),  -- يسمح ببيع مجزأ؟

     -- -------------------- Stock & Expiry (summary) --------------------  
    Quantity          DECIMAL(18,3) NOT NULL
        CONSTRAINT DF_Products_Quantity DEFAULT (0),        -- الكمية (مثال 2.500)

    HasExpiry         BIT NOT NULL
        CONSTRAINT DF_Products_HasExpiry DEFAULT (1),       -- له صلاحية؟

    NearestExpiryDate DATE NULL,                            -- أقرب صلاحية ضمن المخزون المتاح (ملخص)
    LastStockSyncAt   DATETIME2(0) NULL,                    -- آخر وقت مزامنة للمخزون/الصلاحية

     -- -------------------- Offers / Discounts --------------------  
    HasPromotion      BIT NOT NULL
        CONSTRAINT DF_Products_HasPromotion DEFAULT (0),    -- هل يوجد عرض؟

    PromotionDiscountPercent  DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_Products_PromotionDiscountPercent DEFAULT (0), -- نسبة الخصم %

    PromotionStartsAt DATETIME2(0) NULL,                    -- بداية العرض
    PromotionEndsAt   DATETIME2(0) NULL,                    -- نهاية العرض

     -- -------------------- Flags --------------------  
    IsFeatured        BIT NOT NULL
        CONSTRAINT DF_Products_IsFeatured DEFAULT (0),      -- مميز؟

     -- -------------------- Integration flags --------------------  
    IsIntegrated      BIT NOT NULL
        CONSTRAINT DF_Products_IsIntegrated DEFAULT (0),    -- مدمج؟
    IntegratedAt      DATETIME2(0) NULL,                    -- تاريخ الدمج

     -- -------------------- Points --------------------  
    Points INT NOT NULL
        CONSTRAINT DF_Products_PointsOuter DEFAULT (0),

     -- -------------------- Selling Rules --------------------  
    RequiresPrescription BIT NOT NULL
        CONSTRAINT DF_Products_RequiresPrescription DEFAULT (0), -- يحتاج روشتة؟

    IsAvailable       BIT NOT NULL
        CONSTRAINT DF_Products_IsAvailable DEFAULT (0),     -- متاح؟ (يتم ضبطه بالمزامنة/المنطق)

    IsActive          BIT NOT NULL
        CONSTRAINT DF_Products_IsActive DEFAULT (1),        -- نشط/ظاهر؟

     -- -------------------- Auditing / Soft Delete / Concurrency --------------------  
    CreatedAt         DATETIME2(0) NOT NULL
        CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSDATETIME()), -- إنشاء

    UpdatedAt         DATETIME2(0) NULL,                    -- تعديل
    DeletedAt         DATETIME2(0) NULL,                    -- حذف منطقي

     -- -------------------- Data integrity (Checks) --------------------  
    CONSTRAINT CK_Products_QuantityNonNegative
        CHECK (Quantity >= 0),

    CONSTRAINT CK_Products_PricesNonNegative
        CHECK (OuterUnitPrice >= 0 AND (InnerUnitPrice IS NULL OR InnerUnitPrice >= 0)),

    CONSTRAINT CK_Products_PromotionPercent
        CHECK (PromotionDiscountPercent >= 0 AND PromotionDiscountPercent <= 100),

    CONSTRAINT CK_Products_PromotionDates
        CHECK (
            (PromotionStartsAt IS NULL AND PromotionEndsAt IS NULL)
            OR (PromotionStartsAt IS NOT NULL AND PromotionEndsAt IS NOT NULL AND PromotionEndsAt > PromotionStartsAt)
        ),

    CONSTRAINT CK_Products_OrderLimits
        CHECK (
            MinOrderQty > 0
            AND (MaxOrderQty IS NULL OR MaxOrderQty >= MinOrderQty)
            AND (MaxPerDayQty IS NULL OR MaxPerDayQty > 0)
        ),

     -- Inner rule: إذا يوجد InnerUnitId يجب وجود InnerPerOuter >= 1  
    CONSTRAINT CK_Products_InnerRules
        CHECK (
            (InnerUnitId IS NULL AND InnerPerOuter IS NULL)
            OR (InnerUnitId IS NOT NULL AND InnerPerOuter IS NOT NULL AND InnerPerOuter >= 1)
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


CREATE TABLE dbo.ProductImages
(
    Id            BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ProductImages PRIMARY KEY,

    PharmacyId    INT NOT NULL,
    ProductId     INT NOT NULL,

    ImageUrl      NVARCHAR(600) NOT NULL,
    ThumbnailUrl  NVARCHAR(600) NULL,
    AltText       NVARCHAR(200) NULL,

    SortOrder     INT NOT NULL CONSTRAINT DF_ProductImages_SortOrder DEFAULT (0),
    IsPrimary     BIT NOT NULL CONSTRAINT DF_ProductImages_IsPrimary DEFAULT (0),

    CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_ProductImages_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy     NVARCHAR(100) NULL,

    DeletedAt     DATETIME2(0) NULL,
    DeletedBy     NVARCHAR(100) NULL,

    RowVersion    ROWVERSION NOT NULL,

    CONSTRAINT CK_ProductImages_SortOrder CHECK (SortOrder >= 0),

    CONSTRAINT FK_ProductImages_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_ProductImages_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

CREATE INDEX IX_ProductImages_Pharmacy_Product
ON dbo.ProductImages(PharmacyId, ProductId)
WHERE DeletedAt IS NULL;
GO

CREATE INDEX IX_ProductImages_Product_Sort
ON dbo.ProductImages(ProductId, IsPrimary DESC, SortOrder, Id)
INCLUDE (ImageUrl, ThumbnailUrl, AltText)
WHERE DeletedAt IS NULL;
GO

-- ✅ primary واحدة فقط لكل منتج
CREATE UNIQUE INDEX UX_ProductImages_Product_Primary
ON dbo.ProductImages(ProductId, IsPrimary)
WHERE IsPrimary = 1 AND DeletedAt IS NULL;
GO

-- (اختياري) يوضح للـ scaffold أنها one-to-many
CREATE INDEX IX_ProductImages_ProductId
ON dbo.ProductImages(ProductId)
WHERE DeletedAt IS NULL;
GO






