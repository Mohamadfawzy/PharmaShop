SET IDENTITY_INSERT dbo.Products ON;

INSERT INTO dbo.Products
(
    Id,
    StoreId,
    CategoryId,
    CompanyId,

    NameAr,
    NameEn,
    DescriptionAr,
    DescriptionEn,
    SearchKeywords,

    OuterUnitId,
    InnerUnitId,
    InnerPerOuter,

    OuterUnitPrice,
    InnerUnitPrice,

    MinOrderQty,
    MaxOrderQty,
    MaxPerDayQty,

    IsReturnable,
    AllowSplitSale,

    Quantity,
    HasExpiry,
    NearestExpiryDate,

    HasPromotion,
    PromotionDiscountPercent,
    PromotionStartsAt,
    PromotionEndsAt,

    IsFeatured,
    IsIntegrated,
    Points,
    RequiresPrescription,
    IsAvailable,
    IsActive
)
VALUES

-- المنتج 1
(
    1,
    1,1,1,
    N'ديكلوفيناك جل', 'Diclofenac Gel',
    N'مسكن موضعي للآلام العضلية', 'Topical pain relief gel',
    N'ديكلوفيناك,جل,عضلات',

    1, NULL, NULL,
    20, NULL,

    1, 5, 10,
    1, 0,

    40, 0, NULL,

    0, 0, NULL, NULL,

    0, 0, 2, 0, 1, 1
),

-- المنتج 2
(
    2,
    1,1,1,
    N'أقراص زنك', 'Zinc Tablets',
    N'مكمل غذائي لدعم المناعة', 'Immune support supplement',
    N'زنك,مناعة',

    1, 2, 20,
    45, 2,

    1, 10, 15,
    1, 1,

    60, 1, '2027-01-01',

    1, 5, '2026-04-01', '2026-07-01',

    0, 0, 4, 0, 1, 1
),

-- المنتج 3
(
    3,
    1,1,1,
    N'محلول ملحي للأنف', 'Nasal Saline Solution',
    N'محلول لتنظيف وترطيب الأنف', 'Nasal cleaning solution',
    N'انف,محلول',

    1, NULL, NULL,
    15, NULL,

    1, NULL, NULL,
    1, 0,

    80, 0, NULL,

    0, 0, NULL, NULL,

    0, 0, 1, 0, 1, 1
),

-- المنتج 4
(
    4,
    1,1,1,
    N'كريم مرطب للبشرة', 'Moisturizing Cream',
    N'كريم لترطيب البشرة الجافة', 'Skin moisturizing cream',
    N'كريم,بشرة',

    1, 2, 5,
    75, 15,

    1, 3, 5,
    1, 1,

    25, 1, '2026-09-01',

    1, 20, '2026-05-01', '2026-08-01',

    1, 0, 6, 0, 1, 1
),

-- المنتج 5
(
    5,
    1,1,1,
    N'لاصق جروح', 'Adhesive Bandage',
    N'لاصق طبي للجروح الصغيرة', 'Medical adhesive bandage',
    N'جروح,لاصق',

    1, 2, 50,
    10, 1,

    1, 20, 50,
    1, 1,

    200, 0, NULL,

    0, 0, NULL, NULL,

    0, 0, 1, 0, 1, 1
);

SET IDENTITY_INSERT dbo.Products OFF;
