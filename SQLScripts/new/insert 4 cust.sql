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

DECLARE @GovernorateId int;
SELECT @GovernorateId = [Id]
FROM [dbo].[Governorates]
WHERE [NameAr] = N'القاهرة';

IF @GovernorateId IS NULL
BEGIN
    INSERT INTO [dbo].[Governorates] ([NameAr], [NameEn], [DisplayOrder])
    VALUES (N'القاهرة', N'Cairo', 1);
    SET @GovernorateId = CONVERT(int, SCOPE_IDENTITY());
END;

DECLARE @GovernorateAreaId int;
SELECT @GovernorateAreaId = [Id]
FROM [dbo].[GovernorateAreas]
WHERE [GovernorateId] = @GovernorateId AND [NameAr] = N'مدينة نصر';

IF @GovernorateAreaId IS NULL
BEGIN
    INSERT INTO [dbo].[GovernorateAreas] ([GovernorateId], [NameAr], [NameEn], [DisplayOrder])
    VALUES (@GovernorateId, N'مدينة نصر', N'Nasr City', 1);
    SET @GovernorateAreaId = CONVERT(int, SCOPE_IDENTITY());
END;

INSERT INTO [dbo].[CustomerAddresses]
(
    [CustomerId],
    [GovernorateAreaId],
    [Title],
    [City],
    [Region],
    [Street],
    [Landmark],
    [Latitude],
    [Longitude],
    [IsDefault],
    [CreatedAt]
)
VALUES
(
    1,                                      -- CustomerId (لازم يكون موجود في جدول Customers)
    @GovernorateAreaId,                     -- selected area (مدينة نصر)
    N'المنزل',                              -- Title (Home / Work / ...)
    N'القاهرة',                             -- City
    N'مدينة نصر',                           -- Region
    N'شارع عباس العقاد، بجوار سيتي ستارز', -- Street
    N'بجوار سيتي ستارز',                    -- Landmark
    30.0728,                                -- Latitude
    31.3460,                                -- Longitude
    1,                                      -- IsDefault (العنوان الافتراضي)
    SYSUTCDATETIME()                        -- CreatedAt
);
GO
