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





/*========================================================
  Brands
  - Brand per Pharmacy (optional but useful)
========================================================*/
GO
CREATE TABLE dbo.Companies
(
    Id          INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Brands PRIMARY KEY,              -- معرف الماركة

    PharmacyId  INT NOT NULL,                          -- الصيدلية المالكة (Multi-tenant)

    Name        NVARCHAR(150) NOT NULL,                -- اسم الماركة عربي
    NameEn      NVARCHAR(150) NULL,                    -- اسم الماركة إنجليزي (اختياري)

    IsActive    BIT NOT NULL CONSTRAINT DF_Brands_IsActive DEFAULT (1), -- نشط؟
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Brands_CreatedAt DEFAULT (SYSUTCDATETIME()), -- تاريخ الإنشاء

    DeletedAt   DATETIME2(0) NULL,                     -- Soft delete
    DeletedBy   NVARCHAR(100) NULL,                    -- حُذف بواسطة

    RowVersion  ROWVERSION NOT NULL,                   -- Concurrency

    CONSTRAINT FK_Brands_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

-- اسم الماركة Unique داخل الصيدلية (غير محذوف)
CREATE UNIQUE INDEX UX_Brands_Pharmacy_Name
ON dbo.Brands(PharmacyId, Name)
WHERE DeletedAt IS NULL;
GO


INSERT INTO dbo.Brands (PharmacyId, Name, NameEn)
VALUES (1, N'جلَكسو سميث كلاين', N'GlaxoSmithKline');


/*========================================================
  Products
  - Barcode/InternationalCode/StockProductCode are stored here (unified per product)
  - Selling price & per-unit inventory are NOT here (they are in ProductUnits / Inventory)
========================================================*/
GO


/*===========================================================
  dbo.Products (Improved)
  - Default sale unit = Outer (big unit)
  - Optional inner unit with its own price
  - Stock quantity stored in Outer units as DECIMAL:
      Example: InnerPerOuter=4, 2 boxes + 2 strips => 2.500
===========================================================*/

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

CREATE TABLE dbo.Products
(
    /* -------------------- Identity / Tenant / Location -------------------- */
    Id                INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Products PRIMARY KEY,

    StoreId           INT NOT NULL,      -- الفرع/المخزن الذي يخدم المتجر

    CategoryId        INT NOT NULL,
    CompanyId         INT NULL,

    /* -------------------- ERP / Integration -------------------- */
    ErpProductId      DECIMAL(18,0) NULL,         -- product_id في ERP
    InternationalCode VARCHAR(50) NULL,           -- كود دولي 

    /* -------------------- Names / Content -------------------- */
    NameAr            NVARCHAR(250) NOT NULL,
    NameEn            NVARCHAR(250) NULL,

    DescriptionAr     NVARCHAR(MAX) NULL,
    DescriptionEn     NVARCHAR(MAX) NULL,

    SearchKeywords    NVARCHAR(500) NULL,

    /* -------------------- Units & Pricing (Outer default) -------------------- */
    OuterUnitId       INT NOT NULL,               -- الوحدة الكبرى (Box/Bottle...)
    InnerUnitId       INT NULL,                   -- الوحدة الصغرى (Strip/Tablet/Ampoule...) إن وُجدت

    InnerPerOuter     INT NULL,                   -- عدد Inner داخل Outer (مثال 4 شرائط/علبة)
	
	ListPrice        DECIMAL(18,2) NOT NULL,

    OuterUnitPrice        DECIMAL(18,2) NOT NULL DEFAULT (0),

    InnerUnitPrice        DECIMAL(18,2) NULL DEFAULT (0),

    /* -------------------- Stock & Expiry (summary) -------------------- */
    Quantity     DECIMAL(18,3) NOT NULL DEFAULT (0), 

    HasExpiry         BIT NOT NULL
        CONSTRAINT DF_Products_HasExpiry DEFAULT (1),

    NearestExpiryDate DATE NULL,                  -- أقرب صلاحية ضمن المخزون المتاح (ملخص)
    LastStockSyncAt   DATETIME2(0) NULL,          -- آخر وقت مزامنة للمخزون/الصلاحية

    /* -------------------- Offers / Discounts -------------------- */
    HasPromotion              BIT NOT NULL
        CONSTRAINT DF_Products_HasOffer DEFAULT (0),

    PromotionDiscountPercent  DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_Products_OfferDiscountPercent DEFAULT (0),

    PromotionStartsAt         DATETIME2(0) NULL,
    PromotionEndsAt           DATETIME2(0) NULL,

    /* -------------------- Points -------------------- */
    EarnPoints         BIT NOT NULL
        CONSTRAINT DF_Products_EarnPoints DEFAULT (1),

    /* -------------------- Selling Rules -------------------- */
    RequiresPrescription BIT NOT NULL
        CONSTRAINT DF_Products_RequiresPrescription DEFAULT (0),

    MinQuantity    DECIMAL(18,3) NOT NULL DEFAULT (1),

    MaxQuantity    DECIMAL(18,3) NULL,   -- NULL = بلا حد

    IsAvailable          BIT NOT NULL
        CONSTRAINT DF_Products_IsFeatured DEFAULT (0),

    IsActive            BIT NOT NULL
        CONSTRAINT DF_Products_IsActive DEFAULT (1),

   

    /* -------------------- Auditing / Soft Delete / Concurrency -------------------- */
    CreatedAt          DATETIME2(0) NOT NULL  DEFAULT (SYSDATETIME()),

    UpdatedAt          DATETIME2(0) NULL,

    DeletedAt          DATETIME2(0) NULL,
);
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






