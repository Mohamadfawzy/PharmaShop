 USE master;
 IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = 'pharma_shope_db_temp')
 BEGIN
     CREATE DATABASE pharma_shope_db_temp COLLATE Arabic_100_CI_AS_KS_WS_SC_UTF8;
 END
 GO

USE pharma_shope_db_temp;

Go

/*========================================================
  Schema: audit
========================================================*/
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'audit')
    EXEC('CREATE SCHEMA audit');
GO


IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(200) NULL,
    [Description] nvarchar(500) NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] int NOT NULL IDENTITY,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(50) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX PhoneNumberIndex ON AspNetUsers([PhoneNumber]) WHERE PhoneNumber IS NOT NULL;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251224210329_InitIdentity', N'9.0.11');

COMMIT;
GO

-- ===================================================================
-- Insert Rolse
-- ===================================================================
 
GO

SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    RAISERROR('AspNetRoles table not found. Apply Identity migrations first.', 16, 1);
    RETURN;
END
GO

MERGE dbo.AspNetRoles AS target
USING (VALUES
    (N'Admin',      N'ADMIN',      N'Admin',      N'Full access'),
    (N'Support',    N'SUPPORT',    N'Support',    N'Support staff'),
    (N'Viewer',     N'VIEWER',     N'Viewer',     N'Read-only access'),
    (N'Customer',   N'CUSTOMER',   N'Customer',   N'Customer account'),
    (N'Pharmacist', N'PHARMACIST', N'Pharmacist', N'Pharmacist account')
) AS source ([Name], [NormalizedName], [NameEn], [Description])
ON target.NormalizedName = source.NormalizedName
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Name], [NormalizedName], [ConcurrencyStamp], [NameEn], [Description])
    VALUES (source.[Name], source.[NormalizedName], NEWID(), source.[NameEn], source.[Description]);
GO
--=======================================
-- Personses
--=======================================

Go
CREATE TABLE Pharmacies (
   Id INT IDENTITY(1,1) NOT NULL
		CONSTRAINT PK_Pharmacies PRIMARY KEY,
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




--========================================================
-- Tags
--========================================================
GO
CREATE TABLE dbo.Tags
(
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tags PRIMARY KEY,
    PharmacyId INT NOT NULL,

    Name      NVARCHAR(80) NOT NULL,
    NameEn    NVARCHAR(80) NULL,

    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Tags_CreatedAt DEFAULT (SYSUTCDATETIME()),
    IsActive  BIT NOT NULL CONSTRAINT DF_Tags_IsActive DEFAULT (1),

    CONSTRAINT FK_Tags_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

-- اسم التاج Unique داخل الصيدلية
CREATE UNIQUE INDEX UX_Tags_Pharmacy_Name
ON dbo.Tags(PharmacyId, Name);
GO

--========================================================
-- ProductTags (many-to-many)
--========================================================
GO
CREATE TABLE dbo.ProductTags
(
    ProductId INT NOT NULL,
    TagId     INT NOT NULL,

    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ProductTags_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_ProductTags PRIMARY KEY (ProductId, TagId),

    CONSTRAINT FK_ProductTags_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_ProductTags_Tags
        FOREIGN KEY (TagId) REFERENCES dbo.Tags(Id)
);
GO

CREATE INDEX IX_ProductTags_TagId
ON dbo.ProductTags(TagId);
GO


--========================================================
-- PointSettings
--========================================================
GO
CREATE TABLE dbo.PointSettings
(
    PharmacyId INT NOT NULL CONSTRAINT PK_PointSettings PRIMARY KEY,

    -- earn
    EarnEnabled BIT NOT NULL CONSTRAINT DF_PointSettings_EarnEnabled DEFAULT (1),
    EarnPerAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PointSettings_EarnPerAmount DEFAULT (10.00), -- كل كام جنيه؟
    EarnPoints   INT NOT NULL CONSTRAINT DF_PointSettings_EarnPoints DEFAULT (1),                   -- قد إيه نقطة؟

    -- redeem
    RedeemEnabled BIT NOT NULL CONSTRAINT DF_PointSettings_RedeemEnabled DEFAULT (1),
    PointValueEGP DECIMAL(18,4) NOT NULL CONSTRAINT DF_PointSettings_PointValue DEFAULT (0.10),     -- قيمة النقطة بالجنيه
    MaxRedeemPercent DECIMAL(5,2) NOT NULL CONSTRAINT DF_PointSettings_MaxRedeem DEFAULT (50.00),  -- أقصى % من قيمة الطلب

    PointsExpireDays INT NULL, -- مثال 365 يوم، NULL = لا تنتهي

    UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PointSettings_UpdatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_PointSettings_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT CK_PointSettings_Rules
        CHECK (
            EarnPerAmount > 0 AND EarnPoints > 0
            AND PointValueEGP >= 0
            AND MaxRedeemPercent >= 0 AND MaxRedeemPercent <= 100
            AND (PointsExpireDays IS NULL OR PointsExpireDays > 0)
        )
);
GO





--========================================================
-- PointsTransactions
--========================================================
GO
CREATE TABLE dbo.PointsTransactions
(
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PointsTransactions PRIMARY KEY,
    PharmacyId  INT NOT NULL,
    CustomerId  INT NOT NULL,

    -- 1=Earn, 2=Redeem, 3=Expire, 4=Adjust
    TxType      TINYINT NOT NULL,

    Points      INT NOT NULL, -- Earn: + | Redeem: - (أو نخليها موجبة ونحدد النوع.. هنا نخليها signed)
    ReferenceType TINYINT NULL, -- 1=Order, 2=Manual, ...
    ReferenceId   BIGINT NULL,

    ExpiresAt   DATETIME2(0) NULL,
    Note        NVARCHAR(300) NULL,

    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_PointsTransactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   NVARCHAR(100) NULL,

    CONSTRAINT FK_PointsTransactions_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),

    CONSTRAINT FK_PointsTransactions_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT CK_PointsTransactions_Type
        CHECK (TxType IN (1,2,3,4))
);
GO

CREATE INDEX IX_PointsTransactions_Customer_CreatedAt
ON dbo.PointsTransactions(CustomerId, CreatedAt DESC)
INCLUDE (TxType, Points, ReferenceType, ReferenceId, ExpiresAt);
GO

 
GO

/*========================================================
  Orders
  - يمثل فاتورة بيع (POS/Online)
========================================================*/
IF OBJECT_ID('dbo.OrderItems','U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Orders','U') IS NOT NULL DROP TABLE dbo.Orders;
GO

CREATE TABLE dbo.Orders
(
    Id              BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Orders PRIMARY KEY,                 -- معرف الطلب (فاتورة)

    PharmacyId      INT NOT NULL,                         -- الصيدلية المالكة
    StoreId         INT NOT NULL,                         -- المخزن/الفرع الذي خرجت منه الفاتورة
    CustomerId      INT NULL,                             -- العميل (ممكن NULL في POS بدون حساب)

    OrderNumber     NVARCHAR(30) NOT NULL,                -- رقم فاتورة قابل للعرض (مثال: INV-2026-0001)

    -- 1=Draft,2=Pending,3=Paid,4=Cancelled,5=Refunded
    Status          TINYINT NOT NULL
        CONSTRAINT DF_Orders_Status DEFAULT (2),          -- حالة الطلب

    -- 1=POS,2=Online,3=Phone
    Channel         TINYINT NOT NULL
        CONSTRAINT DF_Orders_Channel DEFAULT (1),         -- مصدر الطلب

    Subtotal        DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_Subtotal DEFAULT (0),        -- إجمالي قبل الخصومات والضريبة والشحن

    DiscountTotal   DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_DiscountTotal DEFAULT (0),   -- إجمالي الخصم من كل العروض

    TaxTotal        DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_TaxTotal DEFAULT (0),        -- إجمالي الضريبة

    ShippingFee     DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_ShippingFee DEFAULT (0),     -- تكلفة الشحن (قبل عروض الشحن)

    GrandTotal      DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Orders_GrandTotal DEFAULT (0),      -- الإجمالي النهائي بعد كل شيء

    PointsEarned    INT NOT NULL
        CONSTRAINT DF_Orders_PointsEarned DEFAULT (0),    -- نقاط مكتسبة من هذا الطلب (بعد قواعدك)

    PointsRedeemed  INT NOT NULL
        CONSTRAINT DF_Orders_PointsRedeemed DEFAULT (0),  -- نقاط تم استخدامها في الخصم

    Notes           NVARCHAR(500) NULL,                   -- ملاحظات عامة

    CreatedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_Orders_CreatedAt DEFAULT (SYSUTCDATETIME()), -- تاريخ الإنشاء (UTC)

    UpdatedAt       DATETIME2(0) NULL,                    -- آخر تعديل

    CONSTRAINT FK_Orders_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT FK_Orders_Stores
        FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id),

    CONSTRAINT FK_Orders_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),

    CONSTRAINT CK_Orders_Status CHECK (Status IN (1,2,3,4,5)),
    CONSTRAINT CK_Orders_Channel CHECK (Channel IN (1,2,3)),
    CONSTRAINT CK_Orders_Totals CHECK (
        Subtotal >= 0 AND DiscountTotal >= 0 AND TaxTotal >= 0 AND ShippingFee >= 0 AND GrandTotal >= 0
    )
);
GO

-- رقم الفاتورة Unique داخل الصيدلية
CREATE UNIQUE INDEX UX_Orders_Pharmacy_OrderNumber
ON dbo.Orders(PharmacyId, OrderNumber);
GO

CREATE INDEX IX_Orders_Pharmacy_CreatedAt
ON dbo.Orders(PharmacyId, CreatedAt DESC)
INCLUDE (Status, GrandTotal, CustomerId);
GO


/*========================================================
  OrderItems
  - بنخزن Snapshot للأسعار قبل وبعد العروض (محاسبة)
  - السعر الأساسي يأتي من ProductUnits.ListPrice وقت البيع
========================================================*/
CREATE TABLE dbo.OrderItems
(
    Id              BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_OrderItems PRIMARY KEY,             -- معرف سطر الفاتورة

    OrderId         BIGINT NOT NULL,                      -- المرجع للطلب
    PharmacyId      INT NOT NULL,                         -- الصيدلية (للاستعلام السريع)
    StoreId         INT NOT NULL,                         -- المخزن/الفرع (للاستعلام)

    ProductId       INT NOT NULL,                         -- المنتج
    ProductUnitId   INT NOT NULL,                         -- وحدة البيع الفعلية (شريط/علبة/قرص...)

    Quantity        INT NOT NULL,                         -- الكمية المباعة من هذه الوحدة

    ListUnitPrice   DECIMAL(18,2) NOT NULL,               -- السعر الأصلي للوحدة وقت البيع (Snapshot)
    OriginalLineTotal DECIMAL(18,2) NOT NULL,             -- Quantity * ListUnitPrice

    DiscountAmount  DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_OrderItems_DiscountAmount DEFAULT(0), -- خصم هذا السطر من العروض

    TaxAmount       DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_OrderItems_TaxAmount DEFAULT(0),    -- ضريبة هذا السطر

    FinalLineTotal  DECIMAL(18,2) NOT NULL,               -- النهائي بعد الخصم + الضريبة

    VatRate         DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_OrderItems_VatRate DEFAULT(0),      -- نسبة الضريبة Snapshot (من Products.VatRate وقت البيع)

    CreatedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_OrderItems_CreatedAt DEFAULT(SYSUTCDATETIME()), -- وقت إضافة السطر

    CONSTRAINT FK_OrderItems_Orders
        FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),

    CONSTRAINT FK_OrderItems_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT FK_OrderItems_Stores
        FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id),

    CONSTRAINT FK_OrderItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    -- ✅ يمنع mismatch بين ProductId و ProductUnitId
    CONSTRAINT FK_OrderItems_ProductUnits_Composite
        FOREIGN KEY (ProductUnitId, ProductId)
        REFERENCES dbo.ProductUnits(Id, ProductId),

    CONSTRAINT CK_OrderItems_Qty CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_Prices CHECK (
        ListUnitPrice >= 0 AND OriginalLineTotal >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND FinalLineTotal >= 0
    )
);
GO

CREATE INDEX IX_OrderItems_OrderId
ON dbo.OrderItems(OrderId)
INCLUDE (ProductId, ProductUnitId, Quantity, FinalLineTotal, DiscountAmount);
GO





/*========================================================
  Promotions
  - تعريف العرض (بدون Targets/Effects/Conditions)
========================================================*/
CREATE TABLE dbo.Promotions
(
    Id              INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Promotions PRIMARY KEY,             -- معرف العرض

    PharmacyId      INT NOT NULL,                         -- الصيدلية المالكة (Multi-tenant)

    Name            NVARCHAR(200) NOT NULL,               -- اسم العرض (للوحة التحكم)
    Description     NVARCHAR(500) NULL,                   -- وصف مختصر

    StartAt         DATETIME2(0) NOT NULL,                -- بداية العرض
    EndAt           DATETIME2(0) NOT NULL,                -- نهاية العرض

    Priority        INT NOT NULL
        CONSTRAINT DF_Promotions_Priority DEFAULT(0),     -- أولوية العرض (لحل التعارض)

    -- 0=Exclusive,1=Stackable,2=BestDiscountOnly,3=PriorityOnly
    StackPolicy     TINYINT NOT NULL
        CONSTRAINT DF_Promotions_StackPolicy DEFAULT(0),  -- سياسة التجميع/التعارض

    StackGroupKey   NVARCHAR(80) NULL,                    -- مفتاح تجميع (حل تعارض داخل مجموعة)

    AppliesToAllProducts BIT NOT NULL
        CONSTRAINT DF_Promotions_AllProducts DEFAULT(0),  -- هل العرض على كل المتجر؟ (Explicit)

    IsActive        BIT NOT NULL
        CONSTRAINT DF_Promotions_IsActive DEFAULT(1),     -- مفعل؟

    CreatedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_Promotions_CreatedAt DEFAULT(SYSUTCDATETIME()), -- إنشاء

    UpdatedAt       DATETIME2(0) NULL,                    -- آخر تعديل

    CONSTRAINT FK_Promotions_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT CK_Promotions_Dates CHECK (EndAt > StartAt),
    CONSTRAINT CK_Promotions_StackPolicy CHECK (StackPolicy IN (0,1,2,3))
);
GO

CREATE INDEX IX_Promotions_Pharmacy_Active_Dates
ON dbo.Promotions(PharmacyId, IsActive, StartAt, EndAt)
INCLUDE (Priority, StackPolicy, StackGroupKey, AppliesToAllProducts);
GO





/*========================================================
  PromotionTargets (Unified targets table - Option 2)
  - صف واحد = هدف واحد فقط
  - يدعم: Product / ProductUnit / Category / Tag
========================================================*/
CREATE TABLE dbo.PromotionTargets
(
    Id              BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_PromotionTargets PRIMARY KEY,       -- معرف الهدف (Row Id)

    PromotionId     INT NOT NULL,                          -- العرض الذي ينتمي له هذا الهدف

    -- ==== Target columns (ONLY ONE of them must be NOT NULL) ====
    ProductId       INT NULL,                              -- الهدف: منتج كامل (يشمل كل وحداته)
    ProductUnitId   INT NULL,                              -- الهدف: وحدة بيع محددة (الأدق في الصيدليات)
    CategoryId      INT NULL,                              -- الهدف: تصنيف (وممكن الفرعي)
    TagId           INT NULL,                              -- الهدف: Tag

    ProductIdForUnit INT NULL,                             -- المنتج الخاص بالـ ProductUnit (لعمل Composite FK)

    IncludeSubcategories BIT NOT NULL
        CONSTRAINT DF_PromotionTargets_IncludeSub DEFAULT(1), -- للتصنيف: هل يشمل التصنيفات الفرعية؟

    MinQty          INT NULL,                              -- شرط حد أدنى كمية (على هذا الهدف)
    MaxQty          INT NULL,                              -- شرط حد أقصى كمية (على هذا الهدف)

    CreatedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_PromotionTargets_CreatedAt DEFAULT(SYSUTCDATETIME()), -- إنشاء

    CONSTRAINT FK_PromotionTargets_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT FK_PromotionTargets_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_PromotionTargets_ProductUnits
        FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),

    CONSTRAINT FK_PromotionTargets_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),

    CONSTRAINT FK_PromotionTargets_Tags
        FOREIGN KEY (TagId) REFERENCES dbo.Tags(Id),

    -- ✅ يمنع mismatch: ProductUnitId يجب أن يتبع نفس ProductIdForUnit
    CONSTRAINT FK_PromotionTargets_ProductUnits_Composite
        FOREIGN KEY (ProductUnitId, ProductIdForUnit)
        REFERENCES dbo.ProductUnits(Id, ProductId),

    -- ✅ ensure only one target type per row
    CONSTRAINT CK_PromotionTargets_ExactlyOneTarget
        CHECK (
            (CASE WHEN ProductId     IS NULL THEN 0 ELSE 1 END) +
            (CASE WHEN ProductUnitId IS NULL THEN 0 ELSE 1 END) +
            (CASE WHEN CategoryId    IS NULL THEN 0 ELSE 1 END) +
            (CASE WHEN TagId         IS NULL THEN 0 ELSE 1 END)
            = 1
        ),

    CONSTRAINT CK_PromotionTargets_Qty
        CHECK (
            (MinQty IS NULL OR MinQty > 0)
            AND (MaxQty IS NULL OR MaxQty > 0)
            AND (MinQty IS NULL OR MaxQty IS NULL OR MaxQty >= MinQty)
        ),

    -- ✅ ProductIdForUnit must be provided only when ProductUnitId is used
    CONSTRAINT CK_PromotionTargets_UnitProductPair
        CHECK (
            (ProductUnitId IS NULL AND ProductIdForUnit IS NULL)
            OR (ProductUnitId IS NOT NULL AND ProductIdForUnit IS NOT NULL)
        )
);
GO

CREATE INDEX IX_PromotionTargets_PromotionId
ON dbo.PromotionTargets(PromotionId);
GO

CREATE INDEX IX_PromotionTargets_ProductUnitId
ON dbo.PromotionTargets(ProductUnitId)
INCLUDE (PromotionId, ProductIdForUnit, MinQty, MaxQty);
GO

CREATE INDEX IX_PromotionTargets_ProductId
ON dbo.PromotionTargets(ProductId)
INCLUDE (PromotionId, MinQty, MaxQty);
GO

CREATE INDEX IX_PromotionTargets_CategoryId
ON dbo.PromotionTargets(CategoryId)
INCLUDE (PromotionId, IncludeSubcategories);
GO

CREATE INDEX IX_PromotionTargets_TagId
ON dbo.PromotionTargets(TagId)
INCLUDE (PromotionId);
GO




/*========================================================
  PromotionSchedules
  - تحديد أيام/ساعات تطبيق العرض (اختياري)
========================================================*/
CREATE TABLE dbo.PromotionSchedules
(
    Id          INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_PromotionSchedules PRIMARY KEY,     -- معرف الجدول الزمني

    PromotionId INT NOT NULL,                             -- العرض

    DaysMask    INT NOT NULL
        CONSTRAINT DF_PromotionSchedules_DaysMask DEFAULT(127), -- قناع الأيام: Sun=1..Sat=64

    StartTime   TIME(0) NULL,                             -- بداية الوقت (NULL = طوال اليوم)
    EndTime     TIME(0) NULL,                             -- نهاية الوقت (NULL = طوال اليوم)

    CONSTRAINT FK_PromotionSchedules_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT CK_PromotionSchedules_DaysMask CHECK (DaysMask BETWEEN 1 AND 127),
    CONSTRAINT CK_PromotionSchedules_TimeWindow CHECK (
        (StartTime IS NULL AND EndTime IS NULL)
        OR (StartTime IS NOT NULL AND EndTime IS NOT NULL AND EndTime > StartTime)
    )
);
GO

CREATE INDEX IX_PromotionSchedules_PromotionId
ON dbo.PromotionSchedules(PromotionId);
GO



/*========================================================
  PromotionConditions
  - شروط تطبيق العرض (Order/Customer/Coupon)
========================================================*/
CREATE TABLE dbo.PromotionConditions
(
    Id            BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_PromotionConditions PRIMARY KEY,    -- معرف الشرط

    PromotionId   INT NOT NULL,                            -- العرض

    -- 1=MinOrderAmount,2=MaxOrderAmount,3=MinOrderQty,4=CustomerType,
    -- 5=FirstOrderOnly,6=RequiresCoupon,7=MinDistinctItems
    ConditionType TINYINT NOT NULL,                        -- نوع الشرط

    IntValue      INT NULL,                                -- قيمة رقمية (مثلاً MinOrderQty)
    DecimalValue  DECIMAL(18,2) NULL,                       -- قيمة مالية (مثلاً MinOrderAmount)
    StringValue   NVARCHAR(200) NULL,                       -- قيمة نصية (مثلاً CustomerType=VIP)
    BitValue      BIT NULL,                                 -- شرط Boolean (FirstOrderOnly)
    JsonValue     NVARCHAR(MAX) NULL,                       -- توسعة مستقبلية

    CreatedAt     DATETIME2(0) NOT NULL
        CONSTRAINT DF_PromotionConditions_CreatedAt DEFAULT(SYSUTCDATETIME()),

    CONSTRAINT FK_PromotionConditions_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT CK_PromotionConditions_Type CHECK (ConditionType IN (1,2,3,4,5,6,7))
);
GO

CREATE INDEX IX_PromotionConditions_PromotionId
ON dbo.PromotionConditions(PromotionId, ConditionType);
GO





/*========================================================
  PromotionEffects
  - تأثير/أثر العرض (يمكن وجود أكثر من Effect للعرض الواحد)
========================================================*/
CREATE TABLE dbo.PromotionEffects
(
    Id            BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_PromotionEffects PRIMARY KEY,       -- معرف التأثير

    PromotionId   INT NOT NULL,                            -- العرض

    -- 1=PercentDiscount,2=FixedDiscount,3=BuyXGetY,4=FreeShipping,5=BonusPoints,6=PointsMultiplier
    EffectType    TINYINT NOT NULL,                        -- نوع التأثير

    PercentValue  DECIMAL(9,4) NULL,                       -- قيمة النسبة % (مثال: 10.00)
    AmountValue   DECIMAL(18,2) NULL,                      -- مبلغ ثابت (مثال: 20 جنيه)
    PointsValue   INT NULL,                                -- نقاط إضافية مباشرة
    Multiplier    DECIMAL(9,4) NULL,                       -- مضاعف نقاط (مثال: 2.0)

    MaxDiscountAmount DECIMAL(18,2) NULL,                  -- سقف للخصم (اختياري)

    CreatedAt     DATETIME2(0) NOT NULL
        CONSTRAINT DF_PromotionEffects_CreatedAt DEFAULT(SYSUTCDATETIME()),

    CONSTRAINT FK_PromotionEffects_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT CK_PromotionEffects_Type CHECK (EffectType IN (1,2,3,4,5,6)),

    CONSTRAINT CK_PromotionEffects_Values CHECK (
        (EffectType = 1 AND PercentValue IS NOT NULL AND PercentValue >= 0)
        OR
        (EffectType = 2 AND AmountValue  IS NOT NULL AND AmountValue  >= 0)
        OR
        (EffectType = 3) -- التفاصيل في جدول BuyXGetY
        OR
        (EffectType = 4) -- FreeShipping (AmountValue ممكن سقف)
        OR
        (EffectType = 5 AND PointsValue IS NOT NULL AND PointsValue >= 0)
        OR
        (EffectType = 6 AND Multiplier IS NOT NULL AND Multiplier >= 0)
    )
);
GO

CREATE INDEX IX_PromotionEffects_PromotionId
ON dbo.PromotionEffects(PromotionId, EffectType);
GO




/*========================================================
  PromotionBuyXGetYRules
  - تفاصيل Buy X Get Y (على مستوى Unit/Product/Category/Tag)
========================================================*/
CREATE TABLE dbo.PromotionBuyXGetYRules
(
    EffectId     BIGINT NOT NULL
        CONSTRAINT PK_PromotionBuyXGetYRules PRIMARY KEY, -- نفس EffectId (one-to-one)

    -- Buy side
    BuyProductUnitId   INT NULL,                           -- اشتري هذه الوحدة
    BuyProductId       INT NULL,                           -- أو اشتري منتج
    BuyCategoryId      INT NULL,                           -- أو اشتري تصنيف
    BuyTagId           INT NULL,                           -- أو اشتري Tag
    BuyQty             INT NOT NULL,                       -- كمية الشراء المطلوبة

    -- Get side
    GetProductUnitId   INT NULL,                           -- خذ هذه الوحدة
    GetProductId       INT NULL,                           -- أو منتج
    GetCategoryId      INT NULL,                           -- أو تصنيف
    GetTagId           INT NULL,                           -- أو Tag
    GetQty             INT NOT NULL,                       -- كمية الهدية/المكافأة

    -- RewardType: 1=Free, 2=PercentOff, 3=FixedOff
    RewardType         TINYINT NOT NULL
        CONSTRAINT DF_BXGY_RewardType DEFAULT(1),          -- نوع المكافأة

    RewardPercent      DECIMAL(9,4) NULL,                  -- نسبة خصم على الهدية (لو RewardType=2)
    RewardAmount       DECIMAL(18,2) NULL,                 -- مبلغ خصم (لو RewardType=3)

    MaxSetsPerOrder    INT NULL,                           -- أقصى عدد مجموعات في الطلب (NULL = بلا حد)

    CONSTRAINT FK_BXGY_Effect
        FOREIGN KEY (EffectId) REFERENCES dbo.PromotionEffects(Id),

    CONSTRAINT FK_BXGY_BuyProductUnit
        FOREIGN KEY (BuyProductUnitId) REFERENCES dbo.ProductUnits(Id),

    CONSTRAINT FK_BXGY_GetProductUnit
        FOREIGN KEY (GetProductUnitId) REFERENCES dbo.ProductUnits(Id),

    CONSTRAINT FK_BXGY_BuyProduct
        FOREIGN KEY (BuyProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_BXGY_GetProduct
        FOREIGN KEY (GetProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_BXGY_BuyCategory
        FOREIGN KEY (BuyCategoryId) REFERENCES dbo.Categories(Id),

    CONSTRAINT FK_BXGY_GetCategory
        FOREIGN KEY (GetCategoryId) REFERENCES dbo.Categories(Id),

    CONSTRAINT FK_BXGY_BuyTag
        FOREIGN KEY (BuyTagId) REFERENCES dbo.Tags(Id),

    CONSTRAINT FK_BXGY_GetTag
        FOREIGN KEY (GetTagId) REFERENCES dbo.Tags(Id),

    CONSTRAINT CK_BXGY_Qty CHECK (BuyQty > 0 AND GetQty > 0),
    CONSTRAINT CK_BXGY_RewardType CHECK (RewardType IN (1,2,3)),
    CONSTRAINT CK_BXGY_Targets CHECK (
        (BuyProductUnitId IS NOT NULL OR BuyProductId IS NOT NULL OR BuyCategoryId IS NOT NULL OR BuyTagId IS NOT NULL)
        AND
        (GetProductUnitId IS NOT NULL OR GetProductId IS NOT NULL OR GetCategoryId IS NOT NULL OR GetTagId IS NOT NULL)
    ),
    CONSTRAINT CK_BXGY_RewardValues CHECK (
        (RewardType = 1 AND RewardPercent IS NULL AND RewardAmount IS NULL)
        OR
        (RewardType = 2 AND RewardPercent IS NOT NULL AND RewardPercent >= 0)
        OR
        (RewardType = 3 AND RewardAmount IS NOT NULL AND RewardAmount >= 0)
    )
);
GO




/*========================================================
  PromotionCoupons
  - كوبونات مرتبطة بعرض
========================================================*/
CREATE TABLE dbo.PromotionCoupons
(
    Id              INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_PromotionCoupons PRIMARY KEY,       -- معرف الكوبون

    PromotionId     INT NOT NULL,                          -- العرض

    Code            NVARCHAR(40) NOT NULL,                 -- كود الكوبون
    IsActive        BIT NOT NULL
        CONSTRAINT DF_PromotionCoupons_IsActive DEFAULT(1),-- مفعل؟

    MaxUsesTotal    INT NULL,                              -- أقصى استخدام إجمالي
    MaxUsesPerCustomer INT NULL,                           -- أقصى استخدام لكل عميل

    CreatedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_PromotionCoupons_CreatedAt DEFAULT(SYSUTCDATETIME()),

    CONSTRAINT FK_PromotionCoupons_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT CK_PromotionCoupons_Uses CHECK (
        (MaxUsesTotal IS NULL OR MaxUsesTotal > 0)
        AND (MaxUsesPerCustomer IS NULL OR MaxUsesPerCustomer > 0)
    )
);
GO

CREATE UNIQUE INDEX UX_PromotionCoupons_Code
ON dbo.PromotionCoupons(Code);
GO

CREATE INDEX IX_PromotionCoupons_PromotionId
ON dbo.PromotionCoupons(PromotionId, IsActive);
GO


/*========================================================
  PromotionUsageLimits
  - حدود الاستخدام للعرض (بدون كوبون أيضًا)
========================================================*/
CREATE TABLE dbo.PromotionUsageLimits
(
    PromotionId     INT NOT NULL
        CONSTRAINT PK_PromotionUsageLimits PRIMARY KEY,   -- نفس PromotionId

    MaxRedemptionsTotal       INT NULL,                    -- أقصى مرات إجمالي
    MaxRedemptionsPerCustomer INT NULL,                    -- أقصى مرات لكل عميل
    MaxRedemptionsPerOrder    INT NULL,                    -- أقصى مرات داخل الطلب

    CONSTRAINT FK_PromotionUsageLimits_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT CK_PromotionUsageLimits CHECK (
        (MaxRedemptionsTotal IS NULL OR MaxRedemptionsTotal > 0)
        AND (MaxRedemptionsPerCustomer IS NULL OR MaxRedemptionsPerCustomer > 0)
        AND (MaxRedemptionsPerOrder IS NULL OR MaxRedemptionsPerOrder > 0)
    )
);
GO




/*========================================================
  OrderPromotions
  - عروض طبقت على مستوى الطلب (مثل FreeShipping أو خصم على إجمالي الطلب)
========================================================*/
CREATE TABLE dbo.OrderPromotions
(
    Id              BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_OrderPromotions PRIMARY KEY,        -- معرف السجل

    OrderId         BIGINT NOT NULL,                       -- الطلب
    PromotionId     INT NOT NULL,                          -- العرض
    EffectId        BIGINT NULL,                           -- التأثير الذي طبق

    CouponId        INT NULL,                              -- إن كان بكوبون

    DiscountAmount  DECIMAL(18,2) NOT NULL,                -- قيمة الخصم الناتج
    MetadataJson    NVARCHAR(MAX) NULL,                    -- Snapshot تفاصيل التطبيق

    AppliedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_OrderPromotions_AppliedAt DEFAULT(SYSUTCDATETIME()),

    CONSTRAINT FK_OrderPromotions_Orders
        FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),

    CONSTRAINT FK_OrderPromotions_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT FK_OrderPromotions_Effects
        FOREIGN KEY (EffectId) REFERENCES dbo.PromotionEffects(Id),

    CONSTRAINT FK_OrderPromotions_Coupons
        FOREIGN KEY (CouponId) REFERENCES dbo.PromotionCoupons(Id),

    CONSTRAINT CK_OrderPromotions_Discount CHECK (DiscountAmount >= 0)
);
GO

CREATE INDEX IX_OrderPromotions_OrderId
ON dbo.OrderPromotions(OrderId)
INCLUDE (PromotionId, DiscountAmount);
GO


/*========================================================
  OrderItemPromotions
  - ما الذي طُبق على كل سطر (محاسبة + تقرير خصومات)
========================================================*/
CREATE TABLE dbo.OrderItemPromotions
(
    Id              BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_OrderItemPromotions PRIMARY KEY,    -- معرف السجل

    OrderItemId     BIGINT NOT NULL,                       -- سطر الفاتورة
    PromotionId     INT NOT NULL,                          -- العرض
    EffectId        BIGINT NULL,                           -- التأثير الذي طبق

    AppliedQty      INT NULL,                              -- الكمية التي استفادت من العرض (مفيد لـ BXGY)
    DiscountAmount  DECIMAL(18,2) NOT NULL,                -- خصم هذا العرض على هذا السطر

    MetadataJson    NVARCHAR(MAX) NULL,                    -- Snapshot (sets, matched targets, ...)

    AppliedAt       DATETIME2(0) NOT NULL
        CONSTRAINT DF_OrderItemPromotions_AppliedAt DEFAULT(SYSUTCDATETIME()),

    CONSTRAINT FK_OrderItemPromotions_OrderItems
        FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(Id),

    CONSTRAINT FK_OrderItemPromotions_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT FK_OrderItemPromotions_Effects
        FOREIGN KEY (EffectId) REFERENCES dbo.PromotionEffects(Id),

    CONSTRAINT CK_OrderItemPromotions_Discount CHECK (DiscountAmount >= 0),
    CONSTRAINT CK_OrderItemPromotions_Qty CHECK (AppliedQty IS NULL OR AppliedQty > 0)
);
GO

CREATE INDEX IX_OrderItemPromotions_OrderItemId
ON dbo.OrderItemPromotions(OrderItemId)
INCLUDE (PromotionId, DiscountAmount, AppliedQty);
GO


/*========================================================
  Link PointsTransactions to Orders/Promotions (optional)
========================================================*/
IF COL_LENGTH('dbo.PointsTransactions', 'OrderId') IS NULL
BEGIN
    ALTER TABLE dbo.PointsTransactions
    ADD OrderId BIGINT NULL;                               -- الطلب المرتبط (اختياري)
END
GO

IF COL_LENGTH('dbo.PointsTransactions', 'PromotionId') IS NULL
BEGIN
    ALTER TABLE dbo.PointsTransactions
    ADD PromotionId INT NULL,                              -- العرض الذي سبب النقاط (اختياري)
        PromotionEffectId BIGINT NULL;                     -- التأثير الذي سبب النقاط (اختياري)
END
GO

-- FKs (اختياري تفعيلهم لو تريد strict)
-- ALTER TABLE dbo.PointsTransactions
-- ADD CONSTRAINT FK_PointsTransactions_Orders
--     FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id);
-- GO
-- ALTER TABLE dbo.PointsTransactions
-- ADD CONSTRAINT FK_PointsTransactions_Promotions
--     FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id);
-- GO
-- ALTER TABLE dbo.PointsTransactions
-- ADD CONSTRAINT FK_PointsTransactions_PromotionEffects
--     FOREIGN KEY (PromotionEffectId) REFERENCES dbo.PromotionEffects(Id);
-- GO

GO

-- CategoryAuditLog 
CREATE TABLE audit.CategoryAuditLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    ChangeType NVARCHAR(50) NOT NULL,
    FieldName NVARCHAR(100) NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    ChangedBy NVARCHAR(100) NULL,
    ChangeDate DATETIME NOT NULL DEFAULT GETDATE(),
    Reason NVARCHAR(500) NULL,
    CONSTRAINT FK_CategoryAuditLog_Category
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);


/*========================================================
  audit.ProductAuditLog
  - Field-level audit (row per changed field)
  - OperationId groups one update into one event
========================================================*/
CREATE TABLE audit.ProductAuditLog
(
    Id            BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ProductAuditLog PRIMARY KEY,

    PharmacyId    INT NOT NULL,                       -- Multi-tenant
    ProductId     INT NOT NULL,                       -- Target product

    OperationId   UNIQUEIDENTIFIER NOT NULL,          -- Groups one update operation
    ChangeType    NVARCHAR(50) NOT NULL,              -- Update | Create | SoftDelete | Restore

    FieldName     NVARCHAR(100) NULL,                 -- Which field changed
    OldValue      NVARCHAR(MAX) NULL,                 -- Old value (stringified)
    NewValue      NVARCHAR(MAX) NULL,                 -- New value (stringified)

    ChangedBy     NVARCHAR(100) NULL,                 -- UserId / Username
    ChangeDate    DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductAuditLog_ChangeDate DEFAULT (SYSUTCDATETIME()),

    Reason        NVARCHAR(500) NULL,                 -- Optional reason

    CONSTRAINT FK_ProductAuditLog_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT FK_ProductAuditLog_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id)
);
GO

/*========================================================
  Indexes
========================================================*/

-- Fast timeline per product
CREATE INDEX IX_ProductAuditLog_Product_ChangeDate
ON audit.ProductAuditLog(ProductId, ChangeDate DESC)
INCLUDE (OperationId, ChangeType, FieldName, OldValue, NewValue, ChangedBy, Reason, PharmacyId);
GO

-- Fast grouping by operation
CREATE INDEX IX_ProductAuditLog_Product_Operation
ON audit.ProductAuditLog(ProductId, OperationId)
INCLUDE (ChangeDate, ChangeType, FieldName, OldValue, NewValue, ChangedBy, Reason, PharmacyId);
GO

-- Filter by user (audit for admin/user)
CREATE INDEX IX_ProductAuditLog_Pharmacy_User_Date
ON audit.ProductAuditLog(PharmacyId, ChangedBy, ChangeDate DESC)
INCLUDE (ProductId, OperationId, ChangeType, FieldName, OldValue, NewValue, Reason);
GO



IF OBJECT_ID(N'dbo.LoginAudits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginAudits
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginAudits PRIMARY KEY,
        CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_LoginAudits_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        Outcome         NVARCHAR(20) NOT NULL,       -- Success / Failure
        FailureReason   NVARCHAR(50) NULL,           -- InvalidCredentials / LockedOut / EmailNotConfirmed / UserDisabled / ...

        UserId          INT NULL,                    -- لو عرفنا المستخدم
        IdentifierMasked NVARCHAR(256) NULL,         -- masked email/username
        IdentifierHash  VARBINARY(32) NULL,          -- SHA-256(identifierLower)

        IpAddress       NVARCHAR(45) NULL,           -- IPv4/IPv6
        UserAgent       NVARCHAR(512) NULL,

        TraceId         NVARCHAR(64) NULL,
        PharmacyId      INT NULL,

        LatencyMs       INT NULL
    );
END
GO

-- Indexes (Idempotent)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_UserId' AND object_id = OBJECT_ID('dbo.LoginAudits'))
    CREATE INDEX IX_LoginAudits_UserId ON dbo.LoginAudits(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_CreatedAtUtc' AND object_id = OBJECT_ID('dbo.LoginAudits'))
    CREATE INDEX IX_LoginAudits_CreatedAtUtc ON dbo.LoginAudits(CreatedAtUtc);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_Ip_CreatedAtUtc' AND object_id = OBJECT_ID('dbo.LoginAudits'))
    CREATE INDEX IX_LoginAudits_Ip_CreatedAtUtc ON dbo.LoginAudits(IpAddress, CreatedAtUtc);
GO



BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE 
        @ChildHealthId INT,
        @SkinCareId INT,
        @WomenHealthId INT,

        @ChildMedId INT,
        @ChildVitId INT,

        @DailySkinCareId INT,
        @SkinTreatmentId INT,

        @WomenVitId INT,
        @WomenPersonalCareId INT;

    /* =======================
       Level 1 - Root Categories
       ======================= */

    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'صحة الطفل', 'Child Health', NULL);
    SET @ChildHealthId = SCOPE_IDENTITY();

    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'العناية بالبشرة', 'Skin Care', NULL);
    SET @SkinCareId = SCOPE_IDENTITY();

    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'صحة المرأة', 'Women Health', NULL);
    SET @WomenHealthId = SCOPE_IDENTITY();

    /* =======================
       Level 2
       ======================= */

    -- صحة الطفل
    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'أدوية الأطفال', 'Children Medicines', @ChildHealthId);
    SET @ChildMedId = SCOPE_IDENTITY();

    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'فيتامينات الأطفال', 'Children Vitamins', @ChildHealthId);
    SET @ChildVitId = SCOPE_IDENTITY();

    -- العناية بالبشرة
    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'منتجات العناية اليومية', 'Daily Skin Care', @SkinCareId);
    SET @DailySkinCareId = SCOPE_IDENTITY();

    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'علاج مشاكل البشرة', 'Skin Treatment', @SkinCareId);
    SET @SkinTreatmentId = SCOPE_IDENTITY();

    -- صحة المرأة
    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'فيتامينات المرأة', 'Women Vitamins', @WomenHealthId);
    SET @WomenVitId = SCOPE_IDENTITY();

    INSERT INTO Categories (Name, NameEn, ParentCategoryId)
    VALUES (N'العناية الشخصية للمرأة', 'Women Personal Care', @WomenHealthId);
    SET @WomenPersonalCareId = SCOPE_IDENTITY();

    /* =======================
       Level 3
       ======================= */

    -- أدوية الأطفال
    INSERT INTO Categories (Name, NameEn, ParentCategoryId) VALUES
    (N'خافضات الحرارة', 'Antipyretics', @ChildMedId),
    (N'أدوية السعال', 'Cough Medicines', @ChildMedId),
    (N'أدوية الحساسية', 'Allergy Medicines', @ChildMedId);

    -- فيتامينات الأطفال
    INSERT INTO Categories (Name, NameEn, ParentCategoryId) VALUES
    (N'فيتامين د', 'Vitamin D', @ChildVitId),
    (N'مكملات الحديد', 'Iron Supplements', @ChildVitId),
    (N'مكملات الكالسيوم', 'Calcium Supplements', @ChildVitId);

    -- منتجات العناية اليومية
    INSERT INTO Categories (Name, NameEn, ParentCategoryId) VALUES
    (N'غسول البشرة', 'Face Wash', @DailySkinCareId),
    (N'مرطبات البشرة', 'Moisturizers', @DailySkinCareId),
    (N'واقي الشمس', 'Sun Screen', @DailySkinCareId);

    -- علاج مشاكل البشرة
    INSERT INTO Categories (Name, NameEn, ParentCategoryId) VALUES
    (N'علاج حب الشباب', 'Acne Treatment', @SkinTreatmentId),
    (N'علاج التصبغات', 'Pigmentation Treatment', @SkinTreatmentId),
    (N'علاج الإكزيما', 'Eczema Treatment', @SkinTreatmentId);

    -- فيتامينات المرأة
    INSERT INTO Categories (Name, NameEn, ParentCategoryId) VALUES
    (N'فيتامينات الحمل', 'Pregnancy Vitamins', @WomenVitId),
    (N'مكملات ما بعد الولادة', 'Postnatal Supplements', @WomenVitId),
    (N'مكملات العظام', 'Bone Supplements', @WomenVitId);

    -- العناية الشخصية للمرأة
    INSERT INTO Categories (Name, NameEn, ParentCategoryId) VALUES
    (N'منتجات العناية بالجسم', 'Body Care Products', @WomenPersonalCareId),
    (N'منتجات العناية بالمنطقة الحساسة', 'Intimate Care Products', @WomenPersonalCareId),
    (N'منتجات العناية بالشعر', 'Hair Care Products', @WomenPersonalCareId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;
