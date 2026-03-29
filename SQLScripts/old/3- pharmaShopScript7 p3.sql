
USE pharma_shope_db;
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
-- id = 10 
CREATE TABLE dbo.Promotions
(
    Id              INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Promotions PRIMARY KEY,             -- معرف العرض

    PharmacyId      INT NOT NULL,                         -- الصيدلية المالكة (Multi-tenant)

    Name            NVARCHAR(200) NOT NULL,               -- اسم العرض (للوحة التحكم)
    Description     NVARCHAR(500) NULL,                   -- وصف مختصر

	    -- Stable image identifier (store only the id, not a URL/path)
    ImageId NVARCHAR(128) NULL,

    -- Stored format enum:
    -- 1 = Jpeg, 2 = Png, 3 = Webp
    ImageFormat TINYINT NULL
        CONSTRAINT CK_Promotions_ImageFormat CHECK (ImageFormat IS NULL OR ImageFormat IN (1, 2, 3)),

    StartAt         DATETIME2(0) NOT NULL,                -- بداية العرض
    EndAt           DATETIME2(0) NOT NULL,                -- نهاية العرض

    Priority        INT NOT NULL
        CONSTRAINT DF_Promotions_Priority DEFAULT(0),     -- أولوية العرض (لحل التعارض)
    
	-- PromotionUsageLimits
    MaxRedemptionsTotal       INT NULL,                    -- أقصى مرات إجمالي
    MaxRedemptionsPerCustomer INT NULL,                    -- أقصى مرات لكل عميل
    MaxRedemptionsPerOrder    INT NULL,                    -- أقصى مرات داخل الطلب

    CONSTRAINT CK_PromotionUsageLimits CHECK (
        (MaxRedemptionsTotal IS NULL OR MaxRedemptionsTotal > 0)
        AND (MaxRedemptionsPerCustomer IS NULL OR MaxRedemptionsPerCustomer > 0)
        AND (MaxRedemptionsPerOrder IS NULL OR MaxRedemptionsPerOrder > 0)),



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
