use  pharma_shope_db; 

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
-- Promotions
--========================================================
GO
CREATE TABLE dbo.Promotions
(
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Promotions PRIMARY KEY,
    PharmacyId   INT NOT NULL,

    Name         NVARCHAR(200) NOT NULL,
    Description  NVARCHAR(500) NULL,

    -- 1=PercentDiscount, 2=FixedDiscount, 3=BuyXGetY, 4=FreeShipping ...
    PromotionType TINYINT NOT NULL,

    -- للخصومات:
    DiscountValue DECIMAL(18,2) NULL, -- إذا Percent => 10 يعني 10% | إذا Fixed => 20 جنيه

    StartAt      DATETIME2(0) NOT NULL,
    EndAt        DATETIME2(0) NOT NULL,

    Priority     INT NOT NULL CONSTRAINT DF_Promotions_Priority DEFAULT (0),
    IsStackable  BIT NOT NULL CONSTRAINT DF_Promotions_IsStackable DEFAULT (0), -- هل تتجمع مع عروض أخرى؟
    IsActive     BIT NOT NULL CONSTRAINT DF_Promotions_IsActive DEFAULT (1),

    CreatedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Promotions_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Promotions_Pharmacies
        FOREIGN KEY (PharmacyId) REFERENCES dbo.Pharmacies(Id),

    CONSTRAINT CK_Promotions_Dates
        CHECK (EndAt > StartAt),

    CONSTRAINT CK_Promotions_Discount
        CHECK (
            (PromotionType IN (1,2) AND DiscountValue IS NOT NULL AND DiscountValue >= 0)
            OR (PromotionType NOT IN (1,2))
        )
);
GO

CREATE INDEX IX_Promotions_Pharmacy_Active_Dates
ON dbo.Promotions(PharmacyId, IsActive, StartAt, EndAt)
INCLUDE (PromotionType, DiscountValue, Priority, IsStackable);
GO



--========================================================
-- PromotionProducts
--========================================================
GO
CREATE TABLE dbo.PromotionProducts
(
    PromotionId INT NOT NULL,
    ProductId   INT NOT NULL,

    -- optional constraints per product
    MinQty      INT NULL,
    MaxQty      INT NULL,

    CONSTRAINT PK_PromotionProducts PRIMARY KEY (PromotionId, ProductId),

    CONSTRAINT FK_PromotionProducts_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT FK_PromotionProducts_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),

    CONSTRAINT CK_PromotionProducts_Qty
        CHECK (
            (MinQty IS NULL OR MinQty > 0)
            AND (MaxQty IS NULL OR MaxQty > 0)
            AND (MinQty IS NULL OR MaxQty IS NULL OR MaxQty >= MinQty)
        )
);
GO

CREATE INDEX IX_PromotionProducts_ProductId
ON dbo.PromotionProducts(ProductId);
GO




--========================================================
-- PromotionCategories
--========================================================
GO
CREATE TABLE dbo.PromotionCategories
(
    PromotionId INT NOT NULL,
    CategoryId  INT NOT NULL,

    CONSTRAINT PK_PromotionCategories PRIMARY KEY (PromotionId, CategoryId),

    CONSTRAINT FK_PromotionCategories_Promotions
        FOREIGN KEY (PromotionId) REFERENCES dbo.Promotions(Id),

    CONSTRAINT FK_PromotionCategories_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);
GO

CREATE INDEX IX_PromotionCategories_CategoryId
ON dbo.PromotionCategories(CategoryId);
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
