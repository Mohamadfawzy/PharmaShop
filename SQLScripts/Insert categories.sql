use pharma_shope_db
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
