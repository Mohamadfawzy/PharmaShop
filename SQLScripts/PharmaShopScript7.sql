 
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
-- Products (Production-ready, SQL Server)
--========================================================
GO
CREATE TABLE dbo.Products
(
    Id                    INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Products PRIMARY KEY,                -- رقم تعريفي داخلي

    PharmacyId            INT NOT NULL,                    -- الصيدلية المالكة (Multi-tenant)
    CategoryId            INT NOT NULL,                    -- التصنيف الرئيسي
    BrandId               INT NULL,                        -- الشركة/الماركة (اختياري)

    ProductCode           VARCHAR(50) NULL,                -- كود ثابت داخلي للمنتج (للطباعة/الفواتير)
    SKU                   VARCHAR(50) NULL,                -- Stock Keeping Unit: كود المخزون الداخلي
    Barcode               VARCHAR(50) NULL,                -- باركود (قد يكون غير موجود لبعض المنتجات)
    InternationalCode     VARCHAR(50) NULL,                -- كود دولي (GTIN/UPC... إن وجد)
    StockProductCode      VARCHAR(50) NULL,                -- كود منتج من نظام Stock/ERP خارجي

    Name                  NVARCHAR(250) NOT NULL,          -- اسم المنتج عربي
    NameEn                NVARCHAR(250) NOT NULL,          -- اسم المنتج إنجليزي
    Slug                  NVARCHAR(300) NULL,              -- رابط/Slug صديق للـ SEO (اختياري)

    Description           NVARCHAR(MAX) NULL,              -- وصف عربي
    DescriptionEn         NVARCHAR(MAX) NULL,              -- وصف إنجليزي

    -- =========================
    -- Packaging / Pharmacy specifics (بيانات العبوة)
    -- =========================
    DosageForm            NVARCHAR(50) NULL,               -- شكل الجرعة (Tablet, Syrup, Cream...)
    Strength              NVARCHAR(50) NULL,               -- التركيز (500mg, 1g, 5mg/5ml...)
    PackSize              NVARCHAR(50) NULL,               -- حجم/محتوى العبوة (20 tabs, 120 ml...)
    Unit                  NVARCHAR(30) NULL,               -- وحدة البيع (Piece/Box/Strip/Bottle...)

    -- =========================
    -- Pricing (حقائق التسعير الأساسية)
    -- =========================
    CurrencyCode          CHAR(3) NOT NULL
        CONSTRAINT DF_Products_CurrencyCode DEFAULT ('EGP'), -- عملة السعر (ISO 4217)

    CostPrice             DECIMAL(18,2) NULL,              -- سعر الشراء (للهامش/التقارير)
    ListPrice             DECIMAL(18,2) NOT NULL,          -- السعر الأساسي قبل العروض
    PriceUpdatedAt        DATETIME2(0) NULL,               -- آخر وقت تم فيه تعديل السعر

    -- =========================
    -- Taxes (ضرائب بشكل مرن)
    -- =========================
    IsTaxable             BIT NOT NULL
        CONSTRAINT DF_Products_IsTaxable DEFAULT (1),      -- هل المنتج خاضع للضريبة؟
    VatRate               DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_Products_VatRate DEFAULT (0.00),     -- نسبة الضريبة % (لو IsTaxable=1)
    TaxCategoryCode       NVARCHAR(30) NULL,               -- تصنيف ضريبي (إن احتجت قواعد مختلفة لاحقًا)

    -- =========================
    -- Order limits (حدود الطلب)
    -- =========================
    MinOrderQty           INT NOT NULL
        CONSTRAINT DF_Products_MinOrderQty DEFAULT (1),    -- أقل كمية مسموحة في الطلب
    MaxOrderQty           INT NULL,                         -- أقصى كمية مسموحة في الطلب (اختياري)
    MaxPerDayQty          INT NULL,                         -- أقصى كمية يوميًا (اختياري لبعض المنتجات)

    IsReturnable          BIT NOT NULL
        CONSTRAINT DF_Products_IsReturnable DEFAULT (1),   -- هل يمكن إرجاع المنتج؟
    ReturnWindowDays      INT NULL,                         -- عدد أيام السماح بالاسترجاع (اختياري)

    -- =========================
    -- Shipping / weight (الشحن والوزن)
    -- =========================
    WeightGrams           INT NULL,                         -- وزن المنتج بالجرام (للشحن والتسعير)
    LengthMm              INT NULL,                         -- طول المنتج (مم)
    WidthMm               INT NULL,                         -- عرض المنتج (مم)
    HeightMm              INT NULL,                         -- ارتفاع المنتج (مم)

    -- =========================
    -- Compliance & rules (الامتثال والسياسات)
    -- =========================
    RequiresPrescription  BIT NOT NULL
        CONSTRAINT DF_Products_RequiresPrescription DEFAULT (0), -- يحتاج روشتة؟
    AgeRestricted         BIT NOT NULL
        CONSTRAINT DF_Products_AgeRestricted DEFAULT (0),        -- مقيد بعمر؟
    MinAge                INT NULL,                               -- أقل عمر مسموح (إذا AgeRestricted=1)
    StorageConditions     NVARCHAR(200) NULL,                     -- شروط التخزين (درجة حرارة/بعيدًا عن الشمس...)
    RequiresColdChain     BIT NOT NULL
        CONSTRAINT DF_Products_RequiresColdChain DEFAULT (0),     -- يحتاج تبريد/سلسلة باردة؟
    ControlledSubstance   BIT NOT NULL
        CONSTRAINT DF_Products_ControlledSubstance DEFAULT (0),   -- مادة خاضعة للرقابة؟ (اختياري حسب المجال)

    EarnPoints            BIT NOT NULL
        CONSTRAINT DF_Products_EarnPoints DEFAULT (1),            -- هل يكتسب نقاط ولاء؟

    -- =========================
    -- Availability / Inventory flags (التوفر)
    -- =========================
    TrackInventory        BIT NOT NULL
        CONSTRAINT DF_Products_TrackInventory DEFAULT (1),        -- هل يتتبع المخزون؟
    IsAvailable           BIT NOT NULL
        CONSTRAINT DF_Products_IsAvailable DEFAULT (1),           -- إتاحة يدويًا (حتى لو المخزون 0)
    IsFeatured            BIT NOT NULL
        CONSTRAINT DF_Products_IsFeatured DEFAULT (0),            -- منتج مميز (يظهر في الرئيسية)

    -- =========================
    -- Search helper fields (البحث)
    -- =========================
    SearchKeywords        NVARCHAR(500) NULL,                     -- كلمات مفتاحية للبحث (synonyms/arabic variants)
    NormalizedName        NVARCHAR(250) NULL,                     -- اسم مُطبّع للبحث (اختياري)
    NormalizedNameEn      NVARCHAR(250) NULL,                     -- اسم مُطبّع إنجليزي (اختياري)

    -- =========================
    -- Integration flags (تكامل)
    -- =========================
    IsIntegrated          BIT NOT NULL
        CONSTRAINT DF_Products_IsIntegrated DEFAULT (0),          -- هل تم دمجه مع نظام خارجي؟
    IntegratedAt          DATETIME2(0) NULL,                      -- تاريخ الدمج

    -- =========================
    -- Auditing / soft delete (التدقيق والحذف المنطقي)
    -- =========================
    CreatedAt             DATETIME2(0) NOT NULL
        CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()), -- تاريخ الإنشاء (UTC)
    UpdatedAt             DATETIME2(0) NULL,                      -- تاريخ آخر تعديل
    CreatedBy             NVARCHAR(100) NULL,                     -- أنشأه من؟
    UpdatedBy             NVARCHAR(100) NULL,                     -- عدله من؟

    IsActive              BIT NOT NULL
        CONSTRAINT DF_Products_IsActive DEFAULT (1),              -- ظاهر/غير ظاهر (غير محذوف)

    DeletedAt             DATETIME2(0) NULL,                      -- تاريخ الحذف المنطقي (NULL = غير محذوف)
    DeletedBy             NVARCHAR(100) NULL,                     -- حُذف بواسطة

    RowVersion            ROWVERSION NOT NULL,                    -- لمنع تعارض التحديثات (Concurrency)

    -- =========================
    -- Constraints (قيود تحقق)
    -- =========================
    CONSTRAINT CK_Products_Prices
        CHECK (ListPrice >= 0 AND (CostPrice IS NULL OR CostPrice >= 0)),

    CONSTRAINT CK_Products_Tax
        CHECK (VatRate >= 0 AND VatRate <= 100),

    CONSTRAINT CK_Products_OrderLimits
        CHECK (
            MinOrderQty > 0
            AND (MaxOrderQty IS NULL OR MaxOrderQty >= MinOrderQty)
            AND (MaxPerDayQty IS NULL OR MaxPerDayQty > 0)
        ),

    CONSTRAINT CK_Products_ReturnWindow
        CHECK (
            (IsReturnable = 0 AND ReturnWindowDays IS NULL)
            OR (IsReturnable = 1 AND (ReturnWindowDays IS NULL OR ReturnWindowDays > 0))
        ),

    CONSTRAINT CK_Products_Dimensions
        CHECK (
            (WeightGrams IS NULL OR WeightGrams >= 0)
            AND (LengthMm IS NULL OR LengthMm >= 0)
            AND (WidthMm  IS NULL OR WidthMm  >= 0)
            AND (HeightMm IS NULL OR HeightMm >= 0)
        ),

    CONSTRAINT CK_Products_Age
        CHECK (
            (AgeRestricted = 0 AND MinAge IS NULL)
            OR (AgeRestricted = 1 AND MinAge IS NOT NULL AND MinAge >= 0)
        ),

    -- =========================
    -- Foreign Keys
    -- =========================
    CONSTRAINT FK_Products_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)

    -- إن كان لديك جدول Brands فعّل هذا:
    -- ,CONSTRAINT FK_Products_Brands
    --     FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id)
);
GO

--========================================================
-- Uniqueness (Filtered unique indexes)
--========================================================

-- كود ثابت Unique داخل الصيدلية (إن وجد)
CREATE UNIQUE INDEX UX_Products_Pharmacy_ProductCode
ON dbo.Products(PharmacyId, ProductCode)
WHERE ProductCode IS NOT NULL AND ProductCode <> '';
GO

-- SKU Unique داخل الصيدلية (إن وجد)
CREATE UNIQUE INDEX UX_Products_Pharmacy_SKU
ON dbo.Products(PharmacyId, SKU)
WHERE SKU IS NOT NULL AND SKU <> '';
GO

-- Barcode Unique داخل الصيدلية (إن وجد)
CREATE UNIQUE INDEX UX_Products_Pharmacy_Barcode
ON dbo.Products(PharmacyId, Barcode)
WHERE Barcode IS NOT NULL AND Barcode <> '';
GO

-- InternationalCode Unique داخل الصيدلية (إن وجد)
CREATE UNIQUE INDEX UX_Products_Pharmacy_InternationalCode
ON dbo.Products(PharmacyId, InternationalCode)
WHERE InternationalCode IS NOT NULL AND InternationalCode <> '';
GO

--========================================================
-- Query performance indexes
--========================================================

-- الاستعلام الأكثر شيوعًا: منتجات الصيدلية النشطة غير المحذوفة داخل تصنيف
CREATE INDEX IX_Products_Pharmacy_Category_Active
ON dbo.Products(PharmacyId, CategoryId, IsActive)
INCLUDE (Name, NameEn, ListPrice, IsAvailable, IsFeatured, TrackInventory, RequiresPrescription)
WHERE DeletedAt IS NULL;
GO

-- البحث بالاسم
CREATE INDEX IX_Products_Pharmacy_Name
ON dbo.Products(PharmacyId, Name)
WHERE DeletedAt IS NULL;
GO

CREATE INDEX IX_Products_Pharmacy_NameEn
ON dbo.Products(PharmacyId, NameEn)
WHERE DeletedAt IS NULL;
GO

-- كلمات بحث/تطبيع (اختياري لكنه مفيد)
CREATE INDEX IX_Products_Pharmacy_Search
ON dbo.Products(PharmacyId, NormalizedName, NormalizedNameEn)
INCLUDE (Name, NameEn)
WHERE DeletedAt IS NULL;
GO

-- الفرز بالإنشاء
CREATE INDEX IX_Products_CreatedAt
ON dbo.Products(CreatedAt DESC);
GO


-- factor= 5
-- user Points= 1000 points = 1000/factor= 200 EGP

GO

CREATE TABLE ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsMain BIT NOT NULL DEFAULT 0, -- لتحديد إذا كانت الصورة الرئيسية
    SortOrder INT NULL, -- ترتيب عرض الصور
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ProductImages_Products_ID FOREIGN KEY (ProductId) 
		REFERENCES Products(Id)ON DELETE CASCADE ON UPDATE CASCADE
);

GO

CREATE TABLE Tags (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
	NameEn NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE ProductTags (
    ProductId INT NOT NULL,
    TagId INT NOT NULL,

    CONSTRAINT PK_ProductTags PRIMARY KEY (ProductId, TagId),

	CONSTRAINT FK_ProductTags_ProductId FOREIGN KEY (ProductId) 
		REFERENCES Products(Id),

	CONSTRAINT FK_ProductTags_TagId FOREIGN KEY (TagId) 
		REFERENCES Tags(Id)
);
-- =======================================Prescriptions===========================================
-- ========================================================================================================
-- الروشته

GO

CREATE TABLE Prescriptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    CustomerId INT NOT NULL,
    PharmacyId INT NOT NULL,

    Notes NVARCHAR(500) NULL,                      -- ملاحظات يكتبها العميل
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',-- Pending, Approved, Rejected

    ReviewedByUserId INT NULL,                     -- من راجع الروشتة
    ReviewedAt DATETIME NULL,                      -- متى تمت المراجعة

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_Orders_Customers_CustomerId FOREIGN KEY (CustomerId) 
		REFERENCES Customers(Id),

	CONSTRAINT FK_Orders_Pharmacies_PharmacyId FOREIGN KEY (PharmacyId) 
		REFERENCES Pharmacies(Id),

	CONSTRAINT FK_Orders_Users_ReviewedByUserId FOREIGN KEY (ReviewedByUserId) 
		REFERENCES AspNetUsers(Id)
);

GO


CREATE TABLE PrescriptionImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    PrescriptionId INT NOT NULL,
    FileUrl NVARCHAR(500) NOT NULL,     -- رابط الصورة المرفوعة
    SortOrder INT NULL,                 -- لترتيب الصور عند العرض

    UploadedAt DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_PrescriptionImages_Prescriptions_PrescriptionId 
		FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id)
);

GO


CREATE TABLE PrescriptionItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    PrescriptionId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,

	CONSTRAINT FK_PrescriptionItems_Prescriptions_PrescriptionId 
		FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id),

	CONSTRAINT FK_PrescriptionItems_Products_ProductId 
		FOREIGN KEY (ProductId) REFERENCES Products(Id)

);

GO


CREATE TABLE SalesHeader (
    Id int PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NULL,
    PrescriptionId INT NULL, -- إذا كان الطلب نتيجة لروشتة

	IsFromPrescription BIT NOT NULL DEFAULT 0, 
	-- TRUE If the request is a result of a prescription
    InvoiceNumber NVARCHAR(50) NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Discount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    NetAmount AS (TotalAmount - Discount) PERSISTED,
    PaymentMethod NVARCHAR(50) NOT NULL,
	IsFreeShipping BIT NOT NULL DEFAULT 0, -- هل الطلب يحتوي على شحن مجاني
    Notes NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', --Pending, ConfirmedByCustomer,RejectedByCustomer, Processing, Completed, Returned, PartiallyReturned, Cancelled, Delivered
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_SalesHeader_Customers_CustomerId 
		FOREIGN KEY (CustomerId) REFERENCES Customers(Id),

	CONSTRAINT FK_SalesHeader_Prescriptions_PrescriptionId 
		FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id)

);

GO

CREATE TABLE SalesDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18, 2) NOT NULL,
    Discount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TotalPrice AS ((UnitPrice * Quantity) - Discount) PERSISTED,
    Notes NVARCHAR(MAX) NULL,

	CONSTRAINT FK_SalesDetails_SalesHeader_SalesHeaderId 
		FOREIGN KEY (SalesHeaderId) 
		REFERENCES SalesHeader(Id) ON DELETE CASCADE,

	CONSTRAINT FK_SalesDetails_Products_ProductId 
		FOREIGN KEY (ProductId) 
		REFERENCES Products(Id)

);

GO


CREATE TABLE SalesHeaderReturns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ReturnNumber NVARCHAR(50) NOT NULL,              
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),  
    OriginalSalesHeaderId INT NOT NULL,              

    PharmacyId INT NOT NULL,                         
    CustomerId INT NULL,                             
    
    TotalAmount DECIMAL(18,2) NOT NULL,              
    Notes NVARCHAR(500) NULL,                        
    
    CreatedBy NVARCHAR(100) NOT NULL,                
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),   

	CONSTRAINT FK_SalesHeaderReturns_SalesHeader_OriginalSalesHeaderId 
		FOREIGN KEY (OriginalSalesHeaderId) 
		REFERENCES SalesHeader(Id),

	CONSTRAINT FK_SalesHeaderReturns_Customers_CustomerId 
		FOREIGN KEY (CustomerId) 
		REFERENCES Customers(Id),

	CONSTRAINT FK_SalesHeaderReturns_Pharmacies_PharmacyId 
		FOREIGN KEY (PharmacyId) 
		REFERENCES Pharmacies(Id)

);

GO


CREATE TABLE SalesDetailsReturns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderReturnId INT NOT NULL,         
    ProductId INT NOT NULL,                   
    Quantity DECIMAL(18,2) NOT NULL,          
    UnitPrice DECIMAL(18,2) NOT NULL,         
    TotalPrice AS (Quantity * UnitPrice) PERSISTED,  

    Reason NVARCHAR(500) NULL,                
    
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	
	CONSTRAINT FK_SalesDetailsReturns_SalesHeaderReturns_SalesHeaderReturnId 
		FOREIGN KEY (SalesHeaderReturnId) 
		REFERENCES SalesHeaderReturns(Id),

	CONSTRAINT FK_SalesDetailsReturns_Products_ProductId 
		FOREIGN KEY (ProductId) 
		REFERENCES Products(Id)

);

--CREATE TABLE Payments (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    SalesHeaderId INT NOT NULL,
--    PaymentMethod NVARCHAR(100) NOT NULL,  -- مثال: Cash, Credit Card, Wallet
--    Amount DECIMAL(18,2) NOT NULL,
--    PaidAt DATETIME NOT NULL DEFAULT GETDATE(),

--    Notes NVARCHAR(500) NULL,

--    CONSTRAINT FK_Payments_SalesHeader 
--        FOREIGN KEY (SalesHeaderId) 
--        REFERENCES SalesHeader(Id)
--);

-- الوصفات الطبية




--CREATE TABLE PromotionTypes (
--    Id INT IDENTITY(1,1) PRIMARY KEY,
--    Code NVARCHAR(50) NOT NULL UNIQUE, -- مثال: 'Discount', 'FreeShipping', etc.
--    Name NVARCHAR(150) NOT NULL        -- يمكن استخدامه للعرض بالعربية أو الإنجليزية
--);

GO


-- ProductsOffer
CREATE TABLE Promotions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PharmacyId INT NOT NULL,                        -- العرض مرتبط بأي صيدلية

    Title NVARCHAR(200) NOT NULL,                   -- عنوان العرض الظاهر للمستخدم
    Description NVARCHAR(MAX) NULL,                 -- تفاصيل العرض
	TitleEn NVARCHAR(200) NOT NULL,
	DescriptionEn NVARCHAR(MAX) NULL,
    
    PromoType NVARCHAR(50) NOT NULL CHECK (PromoType IN ('PercentDiscount','FixedDiscount','FreeShipping', 'ExtraPoints', 'BuyXGetY', 'FreeItem')),
        
	DiscountValue DECIMAL(18,2) NULL,				-- قيمة الخصم (نسبة أو مبلغ ثابت)
    ExtraPoints INT NULL,                           -- عدد النقاط الإضافية إن وُجد
    FreeProductId INT NULL,                         -- المنتج المجاني إن وُجد

    MinPurchaseAmount DECIMAL(18,2) NULL,           -- الحد الأدنى للطلب لتفعيل العرض
    MaxUsagePerCustomer INT NULL,                   -- أقصى عدد مرات للمستخدم الواحد
    MaxUsageOverall INT NULL,                       -- أقصى عدد مرات للعرض بشكل عام

    StartsAt DATETIME NOT NULL,
    EndsAt DATETIME NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_Promotions_Pharmacies_PharmacyId 
		FOREIGN KEY (PharmacyId) 
		REFERENCES Pharmacies(Id)
);


GO


CREATE TABLE PromotionProducts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PromotionId INT NOT NULL,
    ProductId INT NOT NULL,

	CONSTRAINT FK_PromotionProducts_Promotions_PromotionId 
		FOREIGN KEY (PromotionId) 
		REFERENCES Promotions(Id),

	CONSTRAINT FK_PromotionProducts_Products_ProductId 
		FOREIGN KEY (ProductId) 
		REFERENCES Products(Id)

);


CREATE TABLE PromotionCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PromotionId INT NOT NULL,
    CategoryId INT NOT NULL,

	CONSTRAINT FK_PromotionCategories_Promotions_PromotionId 
		FOREIGN KEY (PromotionId) 
		REFERENCES Promotions(Id),

	CONSTRAINT FK_PromotionCategories_Categories_CategoryId 
		FOREIGN KEY (CategoryId) 
		REFERENCES Categories(Id)

);

GO


CREATE TABLE PromotionUsage (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PromotionId INT NOT NULL,
    UserId INT NOT NULL,
    UsageDate DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_PromotionUsage_Promotions_PromotionId 
		FOREIGN KEY (PromotionId) 
		REFERENCES Promotions(Id),

	CONSTRAINT FK_PromotionUsage_Users_UserId 
		FOREIGN KEY (UserId) 
		REFERENCES AspNetUsers(Id)

);



GO


--=========================================
CREATE TABLE PromoCodes (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    Code NVARCHAR(50) NOT NULL UNIQUE,         -- كود الخصم نفسه

    Description NVARCHAR(500) NULL,
	DescriptionEn NVARCHAR(500) NULL,

    PromoType NVARCHAR(50) NOT NULL CHECK (PromoType IN ('Discount', 'ExtraPoints', 'FreeShipping', 'GiftProduct')),

    DiscountType NVARCHAR(20) NULL CHECK (DiscountType IN ('Fixed', 'Percent')),
    DiscountValue DECIMAL(18,2) NULL,

    ExtraPointsValue INT NULL,

    MaxUsageCount INT NULL,                     -- الحد الأقصى لاستخدام الكود عمومًا
    UsedCount INT NOT NULL DEFAULT 0,           -- كم مرة تم استخدامه حتى الآن

    IsSingleUsePerCustomer BIT NOT NULL DEFAULT 0, -- هل يمكن استخدامه مرة واحدة فقط لكل عميل

    CustomerId INT NULL,                        -- لو مخصص لعميل معين
    ProductId INT NULL,                         -- لو مرتبط بمنتج معين
    CategoryId INT NULL,                        -- لو مرتبط بتصنيف

    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_PromoCodes_Customers_CustomerId 
		FOREIGN KEY (CustomerId) 
		REFERENCES Customers(Id),

	CONSTRAINT FK_PromoCodes_Products_ProductId 
		FOREIGN KEY (ProductId) 
		REFERENCES Products(Id),

	CONSTRAINT FK_PromoCodes_Categories_CategoryId 
		FOREIGN KEY (CategoryId) 
		REFERENCES Categories(Id)

);

GO


CREATE TABLE PromoCodeUsages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PromoCodeId INT NOT NULL,
    CustomerId INT NOT NULL,
    UsedAt DATETIME NOT NULL DEFAULT GETDATE(),

	CONSTRAINT FK_PromoCodeUsages_PromoCodes_PromoCodeId 
		FOREIGN KEY (PromoCodeId) 
		REFERENCES PromoCodes(Id),

	CONSTRAINT FK_PromoCodeUsages_Customers_CustomerId 
		FOREIGN KEY (CustomerId) 
		REFERENCES Customers(Id)

);

GO


--=========================================
--=========================================
--=========================================

GO


CREATE TABLE CustomerPointsHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CustomerId INT NOT NULL,                           -- العميل المرتبط
    PharmacyId INT NOT NULL,                           -- تابعة لأي صيدلية

    Points INT NOT NULL,                               -- عدد النقاط (سالب إذا تم خصمها)
    Reason NVARCHAR(200) NOT NULL,                     -- سبب الإضافة/الخصم (مثلاً: طلب #123, عرض ترويجي)

    SourceType NVARCHAR(50) NOT NULL,                  -- مصدر النقاط (Purchase, Promo, AdminAdjustment)
    SourceReferenceId INT NULL,                        -- معرف العملية المرجعية (مثلاً: SalesHeaderId)

    ExpiryDate DATETIME NULL,                          -- تاريخ انتهاء صلاحية هذه الدفعة من النقاط

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,                  -- اسم أو معرف المسؤول أو النظام

	CONSTRAINT FK_CustomerPointsHistory_Customers_CustomerId 
		FOREIGN KEY (CustomerId) 
		REFERENCES Customers(Id),

	CONSTRAINT FK_CustomerPointsHistory_Pharmacies_PharmacyId 
		FOREIGN KEY (PharmacyId) 
		REFERENCES Pharmacies(Id)

);

GO














--CREATE TABLE Reviews (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    CustomerId INT NOT NULL,
--    ProductId INT NOT NULL,
--    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
--    Comment NVARCHAR(1000) NULL,
--    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

--    CONSTRAINT FK_Reviews_Customer 
--        FOREIGN KEY (CustomerId) 
--        REFERENCES Customers(Id),

--    CONSTRAINT FK_Reviews_Product 
--        FOREIGN KEY (ProductId) 
--        REFERENCES Products(Id)
--);

--CREATE TABLE Chats (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    CustomerId INT NOT NULL,
--    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

--    CONSTRAINT FK_Chats_Customer 
--        FOREIGN KEY (CustomerId) 
--        REFERENCES Customers(Id)
--);
--CREATE TABLE Messages (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    ChatId INT NOT NULL,
--    SenderType NVARCHAR(50) NOT NULL,  -- Admin / Customer
--    MessageText NVARCHAR(2000) NOT NULL,
--    SentAt DATETIME NOT NULL DEFAULT GETDATE(),

--    CONSTRAINT FK_Messages_Chat 
--        FOREIGN KEY (ChatId) 
--        REFERENCES Chats(Id)
--);
--CREATE TABLE Notifications (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    CustomerId INT NOT NULL,
--    Title NVARCHAR(200) NOT NULL,
--    Body NVARCHAR(1000) NOT NULL,
--    IsRead BIT NOT NULL DEFAULT 0,
--    SentAt DATETIME NOT NULL DEFAULT GETDATE(),

--    CONSTRAINT FK_Notifications_Customer 
--        FOREIGN KEY (CustomerId) 
--        REFERENCES Customers(Id)
--);

--CREATE TABLE AuditLogs (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    AdminId INT NULL,
--    Action NVARCHAR(500) NOT NULL,
--    TableName NVARCHAR(100) NULL,
--    RecordId INT NULL,
--    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
--    Notes NVARCHAR(1000) NULL,

--    CONSTRAINT FK_AuditLogs_Admin 
--        FOREIGN KEY (AdminId) 
--        REFERENCES Admins(Id)
--);

--أداء وفهارس (Indexes) مفيدة جدًا للـ API

--انت عامل Indexes كويسة في Products ✅
--بس هتحتاج غالبًا:

--Index على SalesHeader(OrderDate, CustomerId, Status)

--Index على SalesDetails(SalesHeaderId)

--Index على Prescriptions(CustomerId, PharmacyId, Status, CreatedAt)

--Index على PromotionUsage(PromotionId, UserId) (ممكن Unique لمنع تكرار الاستخدام حسب قواعدك)






