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
CREATE TABLE dbo.Brands
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
  Stores
  - Supports multiple warehouses/locations per pharmacy
========================================================*/
GO
CREATE TABLE dbo.Stores
(
    Id          INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Stores PRIMARY KEY,              -- معرف المخزن

    PharmacyId  INT NOT NULL,                          -- الصيدلية المالكة

    Name        NVARCHAR(150) NOT NULL,                -- اسم المخزن (Main Store / Branch 1...)
    Code        VARCHAR(30) NULL,                      -- كود مختصر ثابت (اختياري)
    Address     NVARCHAR(300) NULL,                    -- عنوان (اختياري)

    IsDefault   BIT NOT NULL CONSTRAINT DF_Stores_IsDefault DEFAULT (0), -- مخزن افتراضي؟
    IsActive    BIT NOT NULL CONSTRAINT DF_Stores_IsActive DEFAULT (1),  -- نشط؟

    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Stores_CreatedAt DEFAULT (SYSUTCDATETIME()),

    DeletedAt   DATETIME2(0) NULL,
    DeletedBy   NVARCHAR(100) NULL,

    RowVersion  ROWVERSION NOT NULL,

    CONSTRAINT FK_Stores_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

-- اسم المخزن Unique داخل الصيدلية (غير محذوف)
CREATE UNIQUE INDEX UX_Stores_Pharmacy_Name
ON dbo.Stores(PharmacyId, Name)
WHERE DeletedAt IS NULL;
GO

-- كود المخزن Unique داخل الصيدلية (لو مستخدم)
CREATE UNIQUE INDEX UX_Stores_Pharmacy_Code
ON dbo.Stores(PharmacyId, Code)
WHERE Code IS NOT NULL AND Code <> '' AND DeletedAt IS NULL;
GO

-- ضمان مخزن افتراضي واحد لكل صيدلية
CREATE UNIQUE INDEX UX_Stores_OneDefaultPerPharmacy
ON dbo.Stores(PharmacyId)
WHERE IsDefault = 1 AND DeletedAt IS NULL;
GO


INSERT INTO dbo.Stores
(
    PharmacyId,Name,Code,Address,IsDefault
)
VALUES
(
    1,N'المخزن الرئيسي',
    'MAIN',N'القاهرة - مدينة نصر - شارع مصطفى النحاس', 1
);


/*========================================================
  Products
  - Barcode/InternationalCode/StockProductCode are stored here (unified per product)
  - Selling price & per-unit inventory are NOT here (they are in ProductUnits / Inventory)
========================================================*/
GO
CREATE TABLE dbo.Products
(
    Id                   INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Products PRIMARY KEY,                -- معرف المنتج

    PharmacyId           INT NOT NULL,                     -- الصيدلية المالكة (Multi-tenant)
    CategoryId           INT NOT NULL,                     -- التصنيف (Existing table)
    BrandId              INT NULL,                         -- الماركة (اختياري)

    -- Unified product codes (as requested)
    Barcode              VARCHAR(50) NULL,                 -- باركود موحد للمنتج (بحث/تعريف)
    InternationalCode    VARCHAR(50) NULL,                 -- كود دولي موحد (إن كان موحدًا في ERP)
    StockProductCode     VARCHAR(50) NULL,                 -- كود المنتج في نظام المخزون/ERP (موحد)

    -- Names / descriptions
    Name                 NVARCHAR(250) NOT NULL,           -- اسم عربي
    NameEn               NVARCHAR(250) NOT NULL,           -- اسم إنجليزي
    Slug                 NVARCHAR(300) NULL,               -- SEO Slug (اختياري)

    Description          NVARCHAR(MAX) NULL,               -- وصف عربي
    DescriptionEn        NVARCHAR(MAX) NULL,               -- وصف إنجليزي

    -- Search helpers
    SearchKeywords       NVARCHAR(500) NULL,               -- كلمات مفتاحية
    NormalizedName       NVARCHAR(250) NULL,               -- اسم مُطبّع للبحث
    NormalizedNameEn     NVARCHAR(250) NULL,               -- اسم مُطبّع EN

    -- Packaging info (informational)
    DosageForm           NVARCHAR(50) NULL,                -- Tablet/Syrup...
    Strength             NVARCHAR(50) NULL,                -- 500mg...
    PackSize             NVARCHAR(80) NULL,                -- 3 strips x 10 tabs...
    Unit                 NVARCHAR(30) NULL,                -- وحدة العرض العامة (Box/Bottle...) (اختياري)

    -- Compliance / rules
    RequiresPrescription BIT NOT NULL
        CONSTRAINT DF_Products_RequiresPrescription DEFAULT (0), -- يحتاج روشتة؟
    EarnPoints           BIT NOT NULL
        CONSTRAINT DF_Products_EarnPoints DEFAULT (1),           -- يكتسب نقاط؟
    HasExpiry            BIT NOT NULL
        CONSTRAINT DF_Products_HasExpiry DEFAULT (1),            -- له صلاحية؟

    AgeRestricted        BIT NOT NULL
        CONSTRAINT DF_Products_AgeRestricted DEFAULT (0),        -- مقيد بعمر؟
    MinAge               INT NULL,                                -- أقل عمر

    RequiresColdChain    BIT NOT NULL
        CONSTRAINT DF_Products_RequiresColdChain DEFAULT (0),     -- يحتاج تبريد؟
    ControlledSubstance  BIT NOT NULL
        CONSTRAINT DF_Products_ControlledSubstance DEFAULT (0),   -- خاضع للرقابة؟
    StorageConditions    NVARCHAR(200) NULL,                       -- شروط تخزين

    -- Taxes (flexible)
    IsTaxable            BIT NOT NULL
        CONSTRAINT DF_Products_IsTaxable DEFAULT (1),             -- خاضع للضريبة؟
    VatRate              DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_Products_VatRate DEFAULT (0.00),            -- نسبة الضريبة %
    TaxCategoryCode      NVARCHAR(30) NULL,                        -- تصنيف ضريبي

    -- Order limits
    MinOrderQty          INT NOT NULL
        CONSTRAINT DF_Products_MinOrderQty DEFAULT (1),           -- أقل كمية
    MaxOrderQty          INT NULL,                                  -- أقصى كمية
    MaxPerDayQty         INT NULL,                                  -- أقصى يوميًا

    IsReturnable         BIT NOT NULL
        CONSTRAINT DF_Products_IsReturnable DEFAULT (1),          -- قابل للإرجاع؟
    ReturnWindowDays     INT NULL,                                  -- نافذة الإرجاع

    -- Split sale policy (informational; pricing handled later at sale time)
    AllowSplitSale       BIT NOT NULL
        CONSTRAINT DF_Products_AllowSplitSale DEFAULT (0),        -- يسمح ببيع مجزأ؟
    SplitLevel           TINYINT NULL,                              -- 1=Inner(Strip) | 2=Base(Tablet)

    -- Shipping / dimensions (optional)
    WeightGrams          INT NULL,                                  -- وزن
    LengthMm             INT NULL,                                  -- طول
    WidthMm              INT NULL,                                  -- عرض
    HeightMm             INT NULL,                                  -- ارتفاع

    -- Flags
    TrackInventory       BIT NOT NULL
        CONSTRAINT DF_Products_TrackInventory DEFAULT (1),         -- يتتبع مخزون؟
    IsFeatured           BIT NOT NULL
        CONSTRAINT DF_Products_IsFeatured DEFAULT (0),             -- مميز؟
    IsActive             BIT NOT NULL
        CONSTRAINT DF_Products_IsActive DEFAULT (1),               -- ظاهر؟

    -- Integration flags
    IsIntegrated         BIT NOT NULL
        CONSTRAINT DF_Products_IsIntegrated DEFAULT (0),           -- مدمج؟
    IntegratedAt         DATETIME2(0) NULL,                          -- تاريخ الدمج

    -- Auditing / soft delete
    CreatedAt            DATETIME2(0) NOT NULL
        CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()), -- إنشاء
    UpdatedAt            DATETIME2(0) NULL,                           -- تعديل
    CreatedBy            NVARCHAR(100) NULL,
    UpdatedBy            NVARCHAR(100) NULL,

    DeletedAt            DATETIME2(0) NULL,                           -- Soft delete
    DeletedBy            NVARCHAR(100) NULL,

    RowVersion           ROWVERSION NOT NULL,                         -- Concurrency

    -- Checks
    CONSTRAINT CK_Products_Tax CHECK (VatRate >= 0 AND VatRate <= 100),
    CONSTRAINT CK_Products_Age CHECK (
        (AgeRestricted = 0 AND MinAge IS NULL)
        OR (AgeRestricted = 1 AND MinAge IS NOT NULL AND MinAge >= 0)
    ),
    CONSTRAINT CK_Products_OrderLimits CHECK (
        MinOrderQty > 0
        AND (MaxOrderQty IS NULL OR MaxOrderQty >= MinOrderQty)
        AND (MaxPerDayQty IS NULL OR MaxPerDayQty > 0)
    ),
    CONSTRAINT CK_Products_ReturnWindow CHECK (
        (IsReturnable = 0 AND ReturnWindowDays IS NULL)
        OR (IsReturnable = 1 AND (ReturnWindowDays IS NULL OR ReturnWindowDays > 0))
    ),
    CONSTRAINT CK_Products_Dimensions CHECK (
        (WeightGrams IS NULL OR WeightGrams >= 0)
        AND (LengthMm IS NULL OR LengthMm >= 0)
        AND (WidthMm  IS NULL OR WidthMm  >= 0)
        AND (HeightMm IS NULL OR HeightMm >= 0)
    ),
    CONSTRAINT CK_Products_SplitRules CHECK (
        (AllowSplitSale = 0 AND SplitLevel IS NULL)
        OR (AllowSplitSale = 1 AND SplitLevel IN (1,2))
    ),

    -- FKs
    CONSTRAINT FK_Products_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)

    -- If you have dbo.Brands:
    -- ,CONSTRAINT FK_Products_Brands
    --     FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id)
);
GO

-- Unique codes per pharmacy (filtered: allow NULL/empty)
CREATE UNIQUE INDEX UX_Products_Pharmacy_Barcode
ON dbo.Products(PharmacyId, Barcode)
WHERE Barcode IS NOT NULL AND Barcode <> '' AND DeletedAt IS NULL;
GO

CREATE UNIQUE INDEX UX_Products_Pharmacy_InternationalCode
ON dbo.Products(PharmacyId, InternationalCode)
WHERE InternationalCode IS NOT NULL AND InternationalCode <> '' AND DeletedAt IS NULL;
GO

CREATE UNIQUE INDEX UX_Products_Pharmacy_StockProductCode
ON dbo.Products(PharmacyId, StockProductCode)
WHERE StockProductCode IS NOT NULL AND StockProductCode <> '' AND DeletedAt IS NULL;
GO

-- Common query indexes
CREATE INDEX IX_Products_Pharmacy_Category_Active
ON dbo.Products(PharmacyId, CategoryId, IsActive)
INCLUDE (Name, NameEn, TrackInventory, RequiresPrescription, IsFeatured)
WHERE DeletedAt IS NULL;
GO

CREATE INDEX IX_Products_Pharmacy_Name
ON dbo.Products(PharmacyId, Name)
WHERE DeletedAt IS NULL;
GO

CREATE INDEX IX_Products_Pharmacy_NameEn
ON dbo.Products(PharmacyId, NameEn)
WHERE DeletedAt IS NULL;
GO

CREATE INDEX IX_Products_Pharmacy_Search
ON dbo.Products(PharmacyId, NormalizedName, NormalizedNameEn)
INCLUDE (Name, NameEn)
WHERE DeletedAt IS NULL;
GO



/*========================================================
  ProductUnits
  - Defines sellable units for a product (Box/Strip/Ampoule)
  - Pricing lives here (catalog/list); final price at sale is stored later in orders
  - NOTE: Barcode is NOT here (per your unified-barcode decision)
  - Includes UNIQUE (Id, ProductId) to support composite FK from Inventory/Batches
========================================================*/
GO
CREATE TABLE dbo.ProductUnits
(
    Id               INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ProductUnits PRIMARY KEY,            -- معرف وحدة البيع

    PharmacyId       INT NOT NULL,                         -- الصيدلية
    ProductId        INT NOT NULL,                         -- المنتج

    UnitId           INT NOT NULL,                         -- نوع الوحدة (FK -> Units.Id)
    SortOrder        INT NOT NULL
        CONSTRAINT DF_ProductUnits_SortOrder DEFAULT (0),  -- ترتيب العرض

    -- Optional internal codes (not required)
    UnitCode         VARCHAR(50) NULL,                     -- كود داخلي للوحدة (اختياري)
    SKU              VARCHAR(50) NULL,                     -- SKU داخلي (اختياري)

    -- Unit hierarchy / conversion (optional)
    ParentProductUnitId INT NULL,                          -- Strip تابع لـ Box...
    UnitsPerParent   DECIMAL(18,3) NULL,                   -- كم وحدة داخل الأب (مثال: 3 شرائط/علبة)

    -- Content to display price per base unit (optional)
    BaseUnitId       INT NULL,                             -- Tablet/ml/g... (FK -> Units.Id)
    BaseQuantity     DECIMAL(18,3) NULL,                   -- محتوى هذه الوحدة (مثال: 10 أقراص/شريط)

    -- Pricing
    CurrencyCode     CHAR(3) NOT NULL
        CONSTRAINT DF_ProductUnits_CurrencyCode DEFAULT ('EGP'), -- العملة
    CostPrice        DECIMAL(18,2) NULL,                   -- تكلفة افتراضية لهذه الوحدة (اختياري)
    ListPrice        DECIMAL(18,2) NOT NULL,               -- سعر بيع قبل العروض
    PriceUpdatedAt   DATETIME2(0) NULL,                    -- تاريخ تحديث السعر

    -- Flags
    IsPrimary        BIT NOT NULL
        CONSTRAINT DF_ProductUnits_IsPrimary DEFAULT (0),  -- الوحدة الافتراضية للعرض/القائمة
    IsActive         BIT NOT NULL
        CONSTRAINT DF_ProductUnits_IsActive DEFAULT (1),   -- نشطة؟

    -- Auditing / soft delete
    CreatedAt        DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductUnits_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt        DATETIME2(0) NULL,

    DeletedAt        DATETIME2(0) NULL,
    DeletedBy        NVARCHAR(100) NULL,

    RowVersion       ROWVERSION NOT NULL,

    -- Checks
    CONSTRAINT CK_ProductUnits_Prices
        CHECK (ListPrice >= 0 AND (CostPrice IS NULL OR CostPrice >= 0)),

    CONSTRAINT CK_ProductUnits_UnitsPerParent
        CHECK (UnitsPerParent IS NULL OR UnitsPerParent > 0),

    CONSTRAINT CK_ProductUnits_BaseQuantity
        CHECK (BaseQuantity IS NULL OR BaseQuantity > 0),

    -- FKs
    CONSTRAINT FK_ProductUnits_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT FK_ProductUnits_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_ProductUnits_Unit
        FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),

    CONSTRAINT FK_ProductUnits_ParentProductUnit
        FOREIGN KEY (ParentProductUnitId) REFERENCES dbo.ProductUnits(Id),

    CONSTRAINT FK_ProductUnits_BaseUnit
        FOREIGN KEY (BaseUnitId) REFERENCES dbo.Units(Id)
);
GO

-- IMPORTANT: unique (Id, ProductId) for composite FK validation in Inventory/Batches
ALTER TABLE dbo.ProductUnits
ADD CONSTRAINT UQ_ProductUnits_Id_ProductId UNIQUE (Id, ProductId);
GO

-- Prevent duplicates: one row per (Product, Unit) inside pharmacy (non-deleted)
CREATE UNIQUE INDEX UX_ProductUnits_Pharmacy_Product_Unit
ON dbo.ProductUnits(PharmacyId, ProductId, UnitId)
WHERE DeletedAt IS NULL;
GO

-- One primary unit per product (non-deleted)
CREATE UNIQUE INDEX UX_ProductUnits_Product_Primary
ON dbo.ProductUnits(ProductId)
WHERE IsPrimary = 1 AND DeletedAt IS NULL;
GO

-- Optional codes uniqueness inside pharmacy (if used)
CREATE UNIQUE INDEX UX_ProductUnits_Pharmacy_SKU
ON dbo.ProductUnits(PharmacyId, SKU)
WHERE SKU IS NOT NULL AND SKU <> '' AND DeletedAt IS NULL;
GO

CREATE UNIQUE INDEX UX_ProductUnits_Pharmacy_UnitCode
ON dbo.ProductUnits(PharmacyId, UnitCode)
WHERE UnitCode IS NOT NULL AND UnitCode <> '' AND DeletedAt IS NULL;
GO

-- Fast list units for a product
CREATE INDEX IX_ProductUnits_Product_List
ON dbo.ProductUnits(ProductId, IsActive, IsPrimary, SortOrder)
INCLUDE (UnitId, ListPrice, CurrencyCode, UnitsPerParent, BaseUnitId, BaseQuantity)
WHERE DeletedAt IS NULL;
GO



/*========================================================
  ProductInventory (Snapshot per store + product unit)
  - Contains ProductId + ProductUnitId (as requested)
  - Uses composite FK (ProductUnitId, ProductId) -> ProductUnits(Id, ProductId)
    to prevent mismatch / inconsistency
========================================================*/
GO
CREATE TABLE dbo.ProductInventory
(
    PharmacyId       INT NOT NULL,                          -- الصيدلية
    StoreId          INT NOT NULL,                          -- المخزن

    ProductId        INT NOT NULL,                          -- المنتج (مكرر لغرض الاستعلامات/سهولة التقارير)
    ProductUnitId    INT NOT NULL,                          -- وحدة البيع

    QuantityOnHand   INT NOT NULL
        CONSTRAINT DF_ProductInventory_QtyOnHand DEFAULT (0), -- الكمية المتاحة في المخزن
    ReservedQty      INT NOT NULL
        CONSTRAINT DF_ProductInventory_Reserved DEFAULT (0),  -- الكمية المحجوزة (طلبات لم تُسلم)

    MinStockLevel    INT NULL,                               -- حد أدنى للتنبيه
    MaxStockLevel    INT NULL,                               -- حد أقصى (اختياري)

    LastStockUpdateAt DATETIME2(0) NULL,                     -- آخر تحديث

    RowVersion       ROWVERSION NOT NULL,

    CONSTRAINT PK_ProductInventory
        PRIMARY KEY (StoreId, ProductUnitId),                -- واحد لكل (مخزن + وحدة)

    CONSTRAINT CK_ProductInventory_Qty
        CHECK (
            QuantityOnHand >= 0 AND ReservedQty >= 0 AND ReservedQty <= QuantityOnHand
            AND (MinStockLevel IS NULL OR MinStockLevel >= 0)
            AND (MaxStockLevel IS NULL OR MaxStockLevel >= 0)
        ),

    CONSTRAINT FK_ProductInventory_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    -- If Stores table exists, keep this FK; otherwise comment it temporarily.
    CONSTRAINT FK_ProductInventory_Stores
        FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id),

    CONSTRAINT FK_ProductInventory_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    -- Composite FK to prevent mismatch between ProductId and ProductUnitId
    CONSTRAINT FK_ProductInventory_ProductUnits_Composite
        FOREIGN KEY (ProductUnitId, ProductId)
        REFERENCES dbo.ProductUnits (Id, ProductId)
);
GO

CREATE INDEX IX_ProductInventory_Pharmacy_Store
ON dbo.ProductInventory(PharmacyId, StoreId)
INCLUDE (ProductId, ProductUnitId, QuantityOnHand, ReservedQty);
GO

CREATE INDEX IX_ProductInventory_Product
ON dbo.ProductInventory(ProductId)
INCLUDE (StoreId, ProductUnitId, QuantityOnHand, ReservedQty);
GO



/*========================================================
  ProductBatches (Lots / Expiry per store + product unit)
  - Contains ProductId + ProductUnitId (as requested)
  - Uses composite FK (ProductUnitId, ProductId) -> ProductUnits(Id, ProductId)
========================================================*/
GO
CREATE TABLE dbo.ProductBatches
(
    Id               BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ProductBatches PRIMARY KEY,            -- معرف الدفعة

    PharmacyId       INT NOT NULL,                           -- الصيدلية
    StoreId          INT NOT NULL,                           -- المخزن

    ProductId        INT NOT NULL,                           -- المنتج (مكرر)
    ProductUnitId    INT NOT NULL,                           -- وحدة البيع

    BatchNumber      NVARCHAR(80) NOT NULL,                  -- رقم الدفعة (Lot/Batch)
    ExpirationDate   DATE NULL,                              -- تاريخ الصلاحية (قد يكون NULL لبعض الأصناف)

    ReceivedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductBatches_ReceivedAt DEFAULT (SYSUTCDATETIME()), -- تاريخ الاستلام

    QuantityReceived INT NOT NULL
        CONSTRAINT DF_ProductBatches_QtyReceived DEFAULT (0), -- الكمية المستلمة
    QuantityOnHand   INT NOT NULL
        CONSTRAINT DF_ProductBatches_QtyOnHand DEFAULT (0),   -- المتبقي من الدفعة

    CostPrice        DECIMAL(18,2) NULL,                      -- تكلفة هذه الدفعة (اختياري ومفيد للربح)
    IsActive         BIT NOT NULL
        CONSTRAINT DF_ProductBatches_IsActive DEFAULT (1),    -- نشطة؟

    CreatedAt        DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductBatches_CreatedAt DEFAULT (SYSUTCDATETIME()),

    DeletedAt        DATETIME2(0) NULL,
    DeletedBy        NVARCHAR(100) NULL,

    RowVersion       ROWVERSION NOT NULL,

    CONSTRAINT CK_ProductBatches_Qty
        CHECK (
            QuantityReceived >= 0
            AND QuantityOnHand >= 0
            AND QuantityOnHand <= QuantityReceived
            AND (CostPrice IS NULL OR CostPrice >= 0)
        ),

    CONSTRAINT FK_ProductBatches_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    -- If Stores table exists, keep this FK; otherwise comment it temporarily.
    CONSTRAINT FK_ProductBatches_Stores
        FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id),

    CONSTRAINT FK_ProductBatches_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    -- Composite FK to prevent mismatch between ProductId and ProductUnitId
    CONSTRAINT FK_ProductBatches_ProductUnits_Composite
        FOREIGN KEY (ProductUnitId, ProductId)
        REFERENCES dbo.ProductUnits (Id, ProductId)
);
GO

-- Usually unique per (store + unit + batch number) (non-deleted)
CREATE UNIQUE INDEX UX_ProductBatches_Store_Unit_Batch
ON dbo.ProductBatches(StoreId, ProductUnitId, BatchNumber)
WHERE DeletedAt IS NULL;
GO

-- FEFO support (earliest expiry first)
CREATE INDEX IX_ProductBatches_Unit_Expiry
ON dbo.ProductBatches(ProductUnitId, ExpirationDate, Id)
INCLUDE (StoreId, QuantityOnHand, BatchNumber, CostPrice)
WHERE DeletedAt IS NULL;
GO

-- Helpful for product-level reports
CREATE INDEX IX_ProductBatches_Product
ON dbo.ProductBatches(ProductId, StoreId, ExpirationDate)
INCLUDE (ProductUnitId, QuantityOnHand, BatchNumber)
WHERE DeletedAt IS NULL;
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






