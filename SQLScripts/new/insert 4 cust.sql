USE [pharma_shope_db];
GO

INSERT INTO [dbo].[Customers]
(
    [UserId],
    [FullNameAr],
    [FullNameEn],
    [PhoneNumber],
    [Gender],
    [DateOfBirth],
    [Email],
    [NationalId],
    [CustomerType],
    [Points],
    [PointsExpiryDate],
    [Notes],
    [CreatedAt],
    [UpdatedAt],
    [IsActive]
)
VALUES
(
    1,                                      -- UserId (لو مربوط بجدول Users)
    N'محمد فوزي هلال',                      -- FullNameAr
    N'Ahmed Mohamed Ali',                  -- FullNameEn
    N'01012345678',                        -- PhoneNumber
    N'Male',                               -- Gender (Male / Female)
    '1990-05-15',                          -- DateOfBirth
    N'ahmed.mohamed@example.com',          -- Email
    N'29805151234567',                     -- NationalId
    N'Retail',                             -- CustomerType (Retail / VIP / Wholesale)
    100,                                   -- Points
    DATEADD(YEAR, 1, SYSUTCDATETIME()),    -- PointsExpiryDate (بعد سنة)
    N'عميل دائم - يفضل الدفع كاش',         -- Notes
    SYSUTCDATETIME(),                      -- CreatedAt
    NULL,                                  -- UpdatedAt
    1                                      -- IsActive
);
GO


USE [pharma_shope_db];
GO

INSERT INTO [dbo].[CustomerAddresses]
(
    [CustomerId],
    [Title],
    [City],
    [Region],
    [Street],
    [Latitude],
    [Longitude],
    [IsDefault],
    [CreatedAt]
)
VALUES
(
    2,                                      -- CustomerId (لازم يكون موجود في جدول Customers)
    N'المنزل',                              -- Title (Home / Work / ...)
    N'القاهرة',                             -- City
    N'مدينة نصر',                           -- Region
    N'شارع عباس العقاد، بجوار سيتي ستارز', -- Street
    30.0728,                                -- Latitude
    31.3460,                                -- Longitude
    1,                                      -- IsDefault (العنوان الافتراضي)
    SYSUTCDATETIME()                        -- CreatedAt
);
GO
