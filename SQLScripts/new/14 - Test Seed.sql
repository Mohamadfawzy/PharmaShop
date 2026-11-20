USE [pharma_shope_db];
GO
/*
    PharmaShop test seed data
    - Safe to run repeatedly: inserts missing seed records and reuses existing records.
    - No schema changes. Does not create carts, prescriptions, orders, or checkout records.
    - All test accounts use: ModernTest1!
    - Governorate areas are starter/test delivery areas; adjust them for the store's real coverage.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL OR OBJECT_ID(N'dbo.Products', N'U') IS NULL
    THROW 51000, 'Apply the PharmaShop database creation scripts before running this seed script.', 1;
IF OBJECT_ID(N'dbo.Governorates', N'U') IS NULL OR OBJECT_ID(N'dbo.ProductImages', N'U') IS NULL
    THROW 51001, 'Required location/image tables are missing. Apply the current schema scripts first.', 1;
IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL OR OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL OR OBJECT_ID(N'dbo.Pharmacies', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Stores', N'U') IS NULL OR OBJECT_ID(N'dbo.GovernorateAreas', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Employees', N'U') IS NULL OR OBJECT_ID(N'dbo.Units', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Companies', N'U') IS NULL OR OBJECT_ID(N'dbo.Categories', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Customers', N'U') IS NULL OR OBJECT_ID(N'dbo.CustomerAddresses', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Tags', N'U') IS NULL OR OBJECT_ID(N'dbo.ProductTags', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Promotions', N'U') IS NULL OR OBJECT_ID(N'dbo.PromotionProducts', N'U') IS NULL
   OR OBJECT_ID(N'dbo.DeliveryZones', N'U') IS NULL OR OBJECT_ID(N'dbo.DeliveryZoneAreas', N'U') IS NULL
    THROW 51002, 'Required PharmaShop tables are missing. Apply the current schema scripts first.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    /* Roles used by the API. */
    MERGE dbo.AspNetRoles AS target
    USING (VALUES
        (N'Admin', N'ADMIN', N'Admin', N'Full access'),
        (N'Support', N'SUPPORT', N'Support', N'Support staff'),
        (N'Viewer', N'VIEWER', N'Viewer', N'Read-only access'),
        (N'Customer', N'CUSTOMER', N'Customer', N'Customer account'),
        (N'Employee', N'EMPLOYEE', N'Employee', N'Employee account'),
        (N'DeliveryAgent', N'DELIVERYAGENT', N'Delivery Agent', N'Delivery agent account')
    ) AS source(Name, NormalizedName, NameEn, Description)
    ON target.NormalizedName = source.NormalizedName
    WHEN NOT MATCHED THEN INSERT(Name, NormalizedName, ConcurrencyStamp, NameEn, Description)
        VALUES(source.Name, source.NormalizedName, CONVERT(nvarchar(36), NEWID()), source.NameEn, source.Description);

    DECLARE @Users TABLE
    (
        Email nvarchar(256) PRIMARY KEY, FullNameAr nvarchar(200), FullNameEn nvarchar(200),
        Phone nvarchar(50), RoleName nvarchar(256), PermissionName nvarchar(100) NULL,
        JobTitle nvarchar(100) NULL, EmployeeCode nvarchar(50) NULL
    );
    INSERT @Users VALUES
      (N'admin@modernsoft.com', N'مدير النظام', N'System Admin', N'01000000001', N'Admin', NULL, NULL, NULL),
      (N'm.helal@modernsoft.com', N'محمد هلال', N'Mohamed Helal', N'01000000002', N'Employee', N'CanReviewPrescriptions', N'صيدلي', N'TEST-EMP-001'),
      (N'mofawzy@user.com', N'محمد فوزي', N'Mohamed Fawzy', N'01000000003', N'Customer', NULL, NULL, NULL),
      (N'inventory@modernsoft.com', N'مسؤول المخزون', N'Inventory Staff', N'01000000004', N'Employee', N'CanManageInventory', N'مسؤول مخزون', N'TEST-EMP-002'),
      (N'orders@modernsoft.com', N'مسؤول الطلبات', N'Orders Staff', N'01000000005', N'Employee', N'CanManageOrders', N'مسؤول طلبات', N'TEST-EMP-003'),
      (N'points@modernsoft.com', N'مسؤول النقاط', N'Points Staff', N'01000000006', N'Employee', N'CanManagePoints', N'مسؤول ولاء العملاء', N'TEST-EMP-004'),
      (N'delivery@modernsoft.com', N'مندوب التوصيل', N'Delivery Agent', N'01000000007', N'DeliveryAgent', N'CanDeliverOrders', N'مندوب توصيل', N'TEST-EMP-005');

    /* Password hash is ASP.NET Core Identity V3 hash for ModernTest1! */
    INSERT dbo.AspNetUsers
        (IsActive, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash,
         SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
         LockoutEnabled, AccessFailedCount)
    SELECT 1, u.Email, UPPER(u.Email), u.Email, UPPER(u.Email), 1,
        N'AQAAAAIAAYagAAAAEKI950Auax/PZ3OCb5vRsSKRouyF2zp2IFizJTkXABjiftOnL/EBXw/Ob5QcJNhHvA==',
        CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), u.Phone, 1, 0, 1, 0
    FROM @Users u
    WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers x WHERE x.NormalizedEmail = UPPER(u.Email));

    /* Ensure the known seed login credentials work when re-running this file. */
    UPDATE x SET IsActive = 1, UserName = u.Email, NormalizedUserName = UPPER(u.Email), EmailConfirmed = 1,
        PasswordHash = N'AQAAAAIAAYagAAAAEKI950Auax/PZ3OCb5vRsSKRouyF2zp2IFizJTkXABjiftOnL/EBXw/Ob5QcJNhHvA==',
        PhoneNumber = u.Phone, PhoneNumberConfirmed = 1, LockoutEnd = NULL, AccessFailedCount = 0
    FROM dbo.AspNetUsers x JOIN @Users u ON x.NormalizedEmail = UPPER(u.Email);

    INSERT dbo.AspNetUserRoles(UserId, RoleId)
    SELECT x.Id, r.Id FROM @Users u
    JOIN dbo.AspNetUsers x ON x.NormalizedEmail = UPPER(u.Email)
    JOIN dbo.AspNetRoles r ON r.NormalizedName = UPPER(u.RoleName)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur WHERE ur.UserId = x.Id AND ur.RoleId = r.Id);

    INSERT dbo.AspNetUserClaims(UserId, ClaimType, ClaimValue)
    SELECT x.Id, N'permission', u.PermissionName
    FROM @Users u JOIN dbo.AspNetUsers x ON x.NormalizedEmail = UPPER(u.Email)
    WHERE u.PermissionName IS NOT NULL AND NOT EXISTS
      (SELECT 1 FROM dbo.AspNetUserClaims c WHERE c.UserId = x.Id AND c.ClaimType = N'permission' AND c.ClaimValue = u.PermissionName);

    /* Reuse an existing pharmacy/default store or create one for a fresh database. */
    DECLARE @PharmacyId int, @StoreId int;
    SELECT TOP (1) @PharmacyId = Id FROM dbo.Pharmacies ORDER BY IsActive DESC, Id;
    IF @PharmacyId IS NULL
    BEGIN
        INSERT dbo.Pharmacies(Name, NameEn, OwnerName, LicenseNumber, PhoneNumber, Email, Address, IsActive)
        VALUES(N'صيدلية مودرن سوفت التجريبية', N'ModernSoft Test Pharmacy', N'ModernSoft', N'TEST-LICENSE-001',
               N'0220000000', N'pharmacy@modernsoft.com', N'القاهرة - عنوان تجريبي', 1);
        SET @PharmacyId = CONVERT(int, SCOPE_IDENTITY());
    END;
    SELECT TOP (1) @StoreId = Id FROM dbo.Stores WHERE PharmacyId = @PharmacyId AND DeletedAt IS NULL ORDER BY IsDefault DESC, IsActive DESC, Id;
    IF @StoreId IS NULL
    BEGIN
        INSERT dbo.Stores(PharmacyId, NameAr, NameEn, Code, Address, IsDefault, IsActive)
        VALUES(@PharmacyId, N'الفرع الرئيسي', N'Main Store', 'TEST-MAIN', N'القاهرة - عنوان تجريبي', 1, 1);
        SET @StoreId = CONVERT(int, SCOPE_IDENTITY());
    END;
    UPDATE dbo.Stores SET IsActive = 1 WHERE Id = @StoreId;
    IF NOT EXISTS (SELECT 1 FROM dbo.Stores WHERE PharmacyId=@PharmacyId AND IsDefault=1 AND DeletedAt IS NULL)
        UPDATE dbo.Stores SET IsDefault=1 WHERE Id=@StoreId;

    /* Seed Egyptian governorates. */
    DECLARE @Gov TABLE(NameAr nvarchar(100) PRIMARY KEY, NameEn nvarchar(100), DisplayOrder int);
    INSERT @Gov VALUES
    (N'القاهرة',N'Cairo',1),(N'الجيزة',N'Giza',2),(N'الإسكندرية',N'Alexandria',3),(N'الدقهلية',N'Dakahlia',4),
    (N'البحر الأحمر',N'Red Sea',5),(N'البحيرة',N'Beheira',6),(N'الفيوم',N'Faiyum',7),(N'الغربية',N'Gharbia',8),
    (N'الإسماعيلية',N'Ismailia',9),(N'المنوفية',N'Monufia',10),(N'المنيا',N'Minya',11),(N'القليوبية',N'Qalyubia',12),
    (N'الوادي الجديد',N'New Valley',13),(N'الشرقية',N'Sharqia',14),(N'سوهاج',N'Sohag',15),(N'جنوب سيناء',N'South Sinai',16),
    (N'شمال سيناء',N'North Sinai',17),(N'بورسعيد',N'Port Said',18),(N'دمياط',N'Damietta',19),(N'الأقصر',N'Luxor',20),
    (N'قنا',N'Qena',21),(N'كفر الشيخ',N'Kafr El Sheikh',22),(N'مطروح',N'Matrouh',23),(N'أسوان',N'Aswan',24),
    (N'أسيوط',N'Assiut',25),(N'بني سويف',N'Beni Suef',26),(N'السويس',N'Suez',27);
    INSERT dbo.Governorates(NameAr, NameEn, DisplayOrder)
    SELECT g.NameAr, g.NameEn, g.DisplayOrder FROM @Gov g
    WHERE NOT EXISTS(SELECT 1 FROM dbo.Governorates x WHERE x.NameAr=g.NameAr);

    /* Five representative city/area names per governorate for testing address and coverage APIs. */
    DECLARE @Area TABLE(GovernorateAr nvarchar(100), NameAr nvarchar(100), NameEn nvarchar(100));
    INSERT @Area VALUES
    (N'القاهرة',N'مدينة نصر',N'Nasr City'),(N'القاهرة',N'مصر الجديدة',N'Heliopolis'),(N'القاهرة',N'المعادي',N'Maadi'),(N'القاهرة',N'شبرا',N'Shoubra'),(N'القاهرة',N'الزمالك',N'Zamalek'),
    (N'الجيزة',N'الدقي',N'Dokki'),(N'الجيزة',N'المهندسين',N'Mohandessin'),(N'الجيزة',N'فيصل',N'Faisal'),(N'الجيزة',N'الهرم',N'Haram'),(N'الجيزة',N'السادس من أكتوبر',N'6th of October'),
    (N'الإسكندرية',N'سيدي جابر',N'Sidi Gaber'),(N'الإسكندرية',N'سموحة',N'Smouha'),(N'الإسكندرية',N'العجمي',N'Agami'),(N'الإسكندرية',N'المنتزه',N'Montaza'),(N'الإسكندرية',N'محرم بك',N'Moharam Bek'),
    (N'الدقهلية',N'المنصورة',N'Mansoura'),(N'الدقهلية',N'ميت غمر',N'Mit Ghamr'),(N'الدقهلية',N'طلخا',N'Talkha'),(N'الدقهلية',N'دكرنس',N'Dakernes'),(N'الدقهلية',N'أجا',N'Aga'),
    (N'البحر الأحمر',N'الغردقة',N'Hurghada'),(N'البحر الأحمر',N'رأس غارب',N'Ras Gharib'),(N'البحر الأحمر',N'سفاجا',N'Safaga'),(N'البحر الأحمر',N'القصير',N'El Quseir'),(N'البحر الأحمر',N'مرسى علم',N'Marsa Alam'),
    (N'البحيرة',N'دمنهور',N'Damanhour'),(N'البحيرة',N'كفر الدوار',N'Kafr El Dawwar'),(N'البحيرة',N'رشيد',N'Rosetta'),(N'البحيرة',N'إيتاي البارود',N'Itay El Barud'),(N'البحيرة',N'أبو حمص',N'Abu Hummus'),
    (N'الفيوم',N'مدينة الفيوم',N'Faiyum City'),(N'الفيوم',N'سنورس',N'Sennuris'),(N'الفيوم',N'إطسا',N'Itsa'),(N'الفيوم',N'طامية',N'Tamiya'),(N'الفيوم',N'يوسف الصديق',N'Youssef El Seddik'),
    (N'الغربية',N'طنطا',N'Tanta'),(N'الغربية',N'المحلة الكبرى',N'Mahalla El Kubra'),(N'الغربية',N'كفر الزيات',N'Kafr El Zayat'),(N'الغربية',N'زفتى',N'Zefta'),(N'الغربية',N'سمنود',N'Samannoud'),
    (N'الإسماعيلية',N'مدينة الإسماعيلية',N'Ismailia City'),(N'الإسماعيلية',N'فايد',N'Fayed'),(N'الإسماعيلية',N'القنطرة شرق',N'Qantara Sharq'),(N'الإسماعيلية',N'القنطرة غرب',N'Qantara Gharb'),(N'الإسماعيلية',N'التل الكبير',N'Tell El Kebir'),
    (N'المنوفية',N'شبين الكوم',N'Shebin El Kom'),(N'المنوفية',N'منوف',N'Menouf'),(N'المنوفية',N'أشمون',N'Ashmoun'),(N'المنوفية',N'قويسنا',N'Quesna'),(N'المنوفية',N'سرس الليان',N'Sers El Lyan'),
    (N'المنيا',N'مدينة المنيا',N'Minya City'),(N'المنيا',N'ملوي',N'Mallawi'),(N'المنيا',N'سمالوط',N'Samalut'),(N'المنيا',N'أبو قرقاص',N'Abu Qurqas'),(N'المنيا',N'بني مزار',N'Beni Mazar'),
    (N'القليوبية',N'بنها',N'Banha'),(N'القليوبية',N'شبرا الخيمة',N'Shubra El Kheima'),(N'القليوبية',N'قليوب',N'Qalyub'),(N'القليوبية',N'الخصوص',N'El Khusus'),(N'القليوبية',N'الخانكة',N'El Khanka'),
    (N'الوادي الجديد',N'الخارجة',N'Kharga'),(N'الوادي الجديد',N'الداخلة',N'Dakhla'),(N'الوادي الجديد',N'الفرافرة',N'Farafra'),(N'الوادي الجديد',N'باريس',N'Paris'),(N'الوادي الجديد',N'بلاط',N'Balat'),
    (N'الشرقية',N'الزقازيق',N'Zagazig'),(N'الشرقية',N'العاشر من رمضان',N'10th of Ramadan'),(N'الشرقية',N'بلبيس',N'Belbeis'),(N'الشرقية',N'فاقوس',N'Faqous'),(N'الشرقية',N'منيا القمح',N'Minya El Qamh'),
    (N'سوهاج',N'مدينة سوهاج',N'Sohag City'),(N'سوهاج',N'أخميم',N'Akhmim'),(N'سوهاج',N'جرجا',N'Girga'),(N'سوهاج',N'طهطا',N'Tahta'),(N'سوهاج',N'البلينا',N'El-Balyana'),
    (N'جنوب سيناء',N'شرم الشيخ',N'Sharm El Sheikh'),(N'جنوب سيناء',N'الطور',N'El Tor'),(N'جنوب سيناء',N'دهب',N'Dahab'),(N'جنوب سيناء',N'نويبع',N'Nuweiba'),(N'جنوب سيناء',N'سانت كاترين',N'Saint Catherine'),
    (N'شمال سيناء',N'العريش',N'El Arish'),(N'شمال سيناء',N'بئر العبد',N'Bir El Abd'),(N'شمال سيناء',N'الشيخ زويد',N'Sheikh Zuweid'),(N'شمال سيناء',N'رفح',N'Rafah'),(N'شمال سيناء',N'الحسنة',N'Hasana'),
    (N'بورسعيد',N'حي الشرق',N'El Sharq'),(N'بورسعيد',N'حي العرب',N'El Arab'),(N'بورسعيد',N'حي المناخ',N'El Manakh'),(N'بورسعيد',N'حي الضواحي',N'El Dawahi'),(N'بورسعيد',N'بورفؤاد',N'Port Fuad'),
    (N'دمياط',N'مدينة دمياط',N'Damietta City'),(N'دمياط',N'رأس البر',N'Ras El Bar'),(N'دمياط',N'فارسكور',N'Faraskur'),(N'دمياط',N'كفر سعد',N'Kafr Saad'),(N'دمياط',N'الزرقا',N'El Zarqa'),
    (N'الأقصر',N'مدينة الأقصر',N'Luxor City'),(N'الأقصر',N'الكرنك',N'Karnak'),(N'الأقصر',N'إسنا',N'Esna'),(N'الأقصر',N'القرنة',N'Qurna'),(N'الأقصر',N'البياضية',N'El Bayadieh'),
    (N'قنا',N'مدينة قنا',N'Qena City'),(N'قنا',N'نجع حمادي',N'Nag Hammadi'),(N'قنا',N'دشنا',N'Dishna'),(N'قنا',N'قوص',N'Qus'),(N'قنا',N'فرشوط',N'Farshut'),
    (N'كفر الشيخ',N'مدينة كفر الشيخ',N'Kafr El Sheikh City'),(N'كفر الشيخ',N'دسوق',N'Desouk'),(N'كفر الشيخ',N'فوه',N'Fuwwah'),(N'كفر الشيخ',N'بلطيم',N'Baltim'),(N'كفر الشيخ',N'الحامول',N'El Hamoul'),
    (N'مطروح',N'مرسى مطروح',N'Mersa Matruh'),(N'مطروح',N'العلمين',N'El Alamein'),(N'مطروح',N'الحمام',N'El Hammam'),(N'مطروح',N'الضبعة',N'El Dabaa'),(N'مطروح',N'سيوة',N'Siwa'),
    (N'أسوان',N'مدينة أسوان',N'Aswan City'),(N'أسوان',N'كوم أمبو',N'Kom Ombo'),(N'أسوان',N'إدفو',N'Edfu'),(N'أسوان',N'دراو',N'Daraw'),(N'أسوان',N'أبو سمبل',N'Abu Simbel'),
    (N'أسيوط',N'مدينة أسيوط',N'Assiut City'),(N'أسيوط',N'ديروط',N'Dayrout'),(N'أسيوط',N'القوصية',N'El Qusiya'),(N'أسيوط',N'أبنوب',N'Abnub'),(N'أسيوط',N'منفلوط',N'Manfalut'),
    (N'بني سويف',N'مدينة بني سويف',N'Beni Suef City'),(N'بني سويف',N'ناصر',N'Nasser'),(N'بني سويف',N'الواسطى',N'El Wasta'),(N'بني سويف',N'إهناسيا',N'Ihnasia'),(N'بني سويف',N'ببا',N'Beba'),
    (N'السويس',N'حي السويس',N'Suez District'),(N'السويس',N'الأربعين',N'El Arbaeen'),(N'السويس',N'عتاقة',N'Ataqa'),(N'السويس',N'الجناين',N'El Ganayen'),(N'السويس',N'فيصل',N'Faisal');
    INSERT dbo.GovernorateAreas(GovernorateId, NameAr, NameEn, DisplayOrder)
    SELECT g.Id, a.NameAr, a.NameEn,
           ROW_NUMBER() OVER(PARTITION BY a.GovernorateAr ORDER BY a.NameEn)
    FROM @Area a JOIN dbo.Governorates g ON g.NameAr=a.GovernorateAr
    WHERE NOT EXISTS(SELECT 1 FROM dbo.GovernorateAreas x WHERE x.GovernorateId=g.Id AND x.NameAr=a.NameAr);

    /* Pharmacy teams with distinct operational permissions. */
    INSERT dbo.Employees(UserId, PharmacyId, FullNameAr, FullNameEn, PhoneNumber, Email, JobTitle, EmployeeCode, IsActive)
    SELECT x.Id, @PharmacyId, u.FullNameAr, u.FullNameEn, u.Phone, u.Email, u.JobTitle, u.EmployeeCode, 1
    FROM @Users u JOIN dbo.AspNetUsers x ON x.NormalizedEmail=UPPER(u.Email)
    WHERE u.JobTitle IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.Employees e WHERE e.UserId=x.Id AND e.DeletedAt IS NULL);

    /* Reference units (safe if some/all already exist). */
    INSERT dbo.Units(Code, NameAr, NameEn)
    SELECT v.Code,v.NameAr,v.NameEn FROM (VALUES
      ('BOX',N'علبة',N'Box'),('STRIP',N'شريط',N'Strip'),('AMPOULE',N'أمبول',N'Ampoule'),
      ('BOTTLE',N'زجاجة',N'Bottle'),('TUBE',N'أنبوبة',N'Tube'),('TABLET',N'قرص',N'Tablet'),('CAPSULE',N'كبسولة',N'Capsule')
    ) v(Code,NameAr,NameEn) WHERE NOT EXISTS(SELECT 1 FROM dbo.Units u WHERE u.Code=v.Code);

    /* Pharmaceutical manufacturers. */
    DECLARE @Companies TABLE(NameAr nvarchar(150),NameEn nvarchar(150));
    INSERT @Companies VALUES
      (N'إيبيكو',N'EIPICO'),(N'جلاكسو سميث كلاين',N'GSK'),(N'أمون',N'Amoun'),(N'فاركو',N'Pharco'),
      (N'سيديكو',N'SEDICO'),(N'الحكمة',N'Hikma'),(N'سانوفي',N'Sanofi'),(N'نوفارتس',N'Novartis');
    INSERT dbo.Companies(NameAr,NameEn)
    SELECT c.NameAr,c.NameEn FROM @Companies c WHERE NOT EXISTS(SELECT 1 FROM dbo.Companies x WHERE x.NameAr=c.NameAr AND x.DeletedAt IS NULL);

    /* Product test catalog: 27 products, one under every starter category. */
    DECLARE @Catalog TABLE(CategoryAr nvarchar(200),CategoryEn nvarchar(200),ProductAr nvarchar(250),ProductEn nvarchar(250),CompanyAr nvarchar(150),Price decimal(18,2));
    INSERT @Catalog VALUES
    (N'صحة الطفل',N'Child Health',N'باراسيتامول 500 مجم',N'Paracetamol 500 mg',N'إيبيكو',35),
    (N'العناية بالبشرة',N'Skin Care',N'مرطب يومي للبشرة',N'Daily Skin Moisturizer',N'فاركو',95),
    (N'صحة المرأة',N'Women Health',N'مكمل غذائي للمرأة',N'Women Multivitamin',N'جلاكسو سميث كلاين',180),
    (N'أدوية الأطفال',N'Children Medicines',N'شراب باراسيتامول للأطفال',N'Children Paracetamol Syrup',N'إيبيكو',42),
    (N'فيتامينات الأطفال',N'Children Vitamins',N'فيتامين د للأطفال',N'Children Vitamin D Drops',N'سيديكو',75),
    (N'منتجات العناية اليومية',N'Daily Skin Care',N'غسول وجه لطيف',N'Gentle Face Wash',N'فاركو',120),
    (N'علاج مشاكل البشرة',N'Skin Treatment',N'كريم عناية بالبشرة',N'Skin Care Cream',N'الحكمة',110),
    (N'فيتامينات المرأة',N'Women Vitamins',N'فيتامينات متعددة للسيدات',N'Women Multivitamins',N'جلاكسو سميث كلاين',210),
    (N'العناية الشخصية للمرأة',N'Women Personal Care',N'غسول عناية شخصية',N'Personal Care Wash',N'فاركو',90),
    (N'خافضات الحرارة',N'Antipyretics',N'باراسيتامول أقراص',N'Paracetamol Tablets',N'إيبيكو',35),
    (N'أدوية السعال',N'Cough Medicines',N'شراب للسعال',N'Cough Syrup',N'أمون',65),
    (N'أدوية الحساسية',N'Allergy Medicines',N'أقراص مضادة للحساسية',N'Allergy Tablets',N'سانوفي',55),
    (N'فيتامين د',N'Vitamin D',N'فيتامين د 1000 وحدة',N'Vitamin D 1000 IU',N'سيديكو',85),
    (N'مكملات الحديد',N'Iron Supplements',N'أقراص الحديد',N'Iron Supplement Tablets',N'الحكمة',95),
    (N'مكملات الكالسيوم',N'Calcium Supplements',N'أقراص كالسيوم',N'Calcium Tablets',N'نوفارتس',125),
    (N'غسول البشرة',N'Face Wash',N'غسول للبشرة الحساسة',N'Sensitive Skin Cleanser',N'فاركو',130),
    (N'مرطبات البشرة',N'Moisturizers',N'كريم مرطب',N'Moisturizing Cream',N'فاركو',105),
    (N'واقي الشمس',N'Sun Screen',N'واقي شمس SPF 50',N'Sunscreen SPF 50',N'سانوفي',220),
    (N'علاج حب الشباب',N'Acne Treatment',N'جل للعناية بالبشرة',N'Acne Care Gel',N'الحكمة',140),
    (N'علاج التصبغات',N'Pigmentation Treatment',N'كريم توحيد لون البشرة',N'Skin Tone Cream',N'فاركو',160),
    (N'علاج الإكزيما',N'Eczema Treatment',N'كريم مرمم للبشرة',N'Skin Repair Cream',N'الحكمة',150),
    (N'فيتامينات الحمل',N'Pregnancy Vitamins',N'فيتامينات الحمل',N'Prenatal Vitamins',N'جلاكسو سميث كلاين',240),
    (N'مكملات ما بعد الولادة',N'Postnatal Supplements',N'مكمل غذائي للأم',N'Maternal Supplement',N'نوفارتس',195),
    (N'مكملات العظام',N'Bone Supplements',N'كالسيوم وفيتامين د',N'Calcium and Vitamin D',N'نوفارتس',170),
    (N'منتجات العناية بالجسم',N'Body Care Products',N'لوشن للجسم',N'Body Lotion',N'فاركو',115),
    (N'منتجات العناية بالمنطقة الحساسة',N'Intimate Care Products',N'منتج عناية شخصية',N'Personal Hygiene Product',N'فاركو',100),
    (N'منتجات العناية بالشعر',N'Hair Care Products',N'شامبو للعناية بالشعر',N'Hair Care Shampoo',N'فاركو',125);

    /* Insert categories as a flat test catalog if the supplied category seed was not applied. */
    INSERT dbo.Categories(NameAr,NameEn,Description,IsActive,IsDeleted)
    SELECT c.CategoryAr,c.CategoryEn,N'فئة بيانات اختبارية',1,0 FROM @Catalog c
    WHERE NOT EXISTS(SELECT 1 FROM dbo.Categories x WHERE (x.NameEn=c.CategoryEn OR x.NameAr=c.CategoryAr) AND x.IsDeleted=0);

    DECLARE @BoxUnitId int=(SELECT TOP(1) Id FROM dbo.Units WHERE Code='BOX');
    INSERT dbo.Products(StoreId,CategoryId,CompanyId,InternationalCode,NameAr,NameEn,DescriptionAr,DescriptionEn,
        SearchKeywords,OuterUnitId,OuterUnitPrice,MinOrderQty,Quantity,HasExpiry,NearestExpiryDate,
        IsFeatured,IsIntegrated,Points,RequiresPrescription,IsAvailable,IsActive)
    SELECT @StoreId,cat.Id,co.Id,
        CONCAT('TEST-',RIGHT(CONCAT('000000',CAST(ROW_NUMBER() OVER(ORDER BY c.ProductEn) AS varchar(6))),6)),
        c.ProductAr,c.ProductEn,N'بيانات منتج تجريبية للاختبار',N'Test product data',CONCAT(c.ProductAr,N' ',c.ProductEn),
        @BoxUnitId,c.Price,1,100,1,DATEADD(year,2,CONVERT(date,GETDATE())),0,0,10,0,1,1
    FROM @Catalog c JOIN (SELECT NameEn, MIN(Id) AS Id FROM dbo.Categories WHERE IsDeleted=0 GROUP BY NameEn) cat ON cat.NameEn=c.CategoryEn
    LEFT JOIN dbo.Companies co ON co.NameAr=c.CompanyAr AND co.DeletedAt IS NULL
    WHERE NOT EXISTS(SELECT 1 FROM dbo.Products p WHERE p.StoreId=@StoreId AND p.NameEn=c.ProductEn AND p.DeletedAt IS NULL);

    /* Customer profile and a default Cairo delivery address for later checkout tests. */
    INSERT dbo.Customers(UserId,FullNameAr,FullNameEn,PhoneNumber,Email,CustomerType,Points,IsActive)
    SELECT x.Id,u.FullNameAr,u.FullNameEn,u.Phone,u.Email,N'Regular',100,1
    FROM @Users u JOIN dbo.AspNetUsers x ON x.NormalizedEmail=UPPER(u.Email)
    WHERE u.RoleName=N'Customer' AND NOT EXISTS(SELECT 1 FROM dbo.Customers c WHERE c.UserId=x.Id);
    DECLARE @CustomerId int=(SELECT TOP(1) c.Id FROM dbo.Customers c JOIN dbo.AspNetUsers x ON x.Id=c.UserId WHERE x.NormalizedEmail=N'MOFAWZY@USER.COM');
    DECLARE @NasrCityAreaId int=(SELECT TOP(1) a.Id FROM dbo.GovernorateAreas a JOIN dbo.Governorates g ON g.Id=a.GovernorateId WHERE g.NameAr=N'القاهرة' AND a.NameAr=N'مدينة نصر');
    IF @CustomerId IS NOT NULL AND @NasrCityAreaId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.CustomerAddresses WHERE CustomerId=@CustomerId AND IsDefault=1 AND DeletedAt IS NULL)
        INSERT dbo.CustomerAddresses(CustomerId,GovernorateAreaId,Title,City,Region,Street,Landmark,IsDefault)
        VALUES(@CustomerId,@NasrCityAreaId,N'المنزل',N'القاهرة',N'مدينة نصر',N'شارع تجريبي - يرجى تعديله',N'علامة مميزة للاختبار',1);

    /* Tags and links to the sample catalog. */
    DECLARE @Tags TABLE(NameAr nvarchar(80),NameEn nvarchar(80));
    INSERT @Tags VALUES(N'مسكن',N'Analgesic'),(N'للأطفال',N'For Children'),(N'عناية بالبشرة',N'Skin Care'),
        (N'مكمل غذائي',N'Supplement'),(N'بدون وصفة',N'Over The Counter'),(N'عرض تجريبي',N'Test Offer');
    INSERT dbo.Tags(NameAr,NameEn) SELECT t.NameAr,t.NameEn FROM @Tags t WHERE NOT EXISTS(SELECT 1 FROM dbo.Tags x WHERE x.NameAr=t.NameAr);
    INSERT dbo.ProductTags(ProductId,TagId)
    SELECT p.Id,t.Id FROM dbo.Products p CROSS JOIN dbo.Tags t
    WHERE p.StoreId=@StoreId AND p.DeletedAt IS NULL AND
      ((t.NameEn=N'Analgesic' AND p.NameEn LIKE N'%Paracetamol%') OR
       (t.NameEn=N'For Children' AND p.NameEn LIKE N'Children%') OR
       (t.NameEn=N'Skin Care' AND p.CategoryId IN (SELECT Id FROM dbo.Categories WHERE NameEn LIKE N'%Skin%' OR NameEn LIKE N'%Moisturizer%')) OR
       (t.NameEn=N'Supplement' AND (p.NameEn LIKE N'%Vitamin%' OR p.NameEn LIKE N'%Supplement%' OR p.NameEn LIKE N'%Calcium%' OR p.NameEn LIKE N'%Iron%')) OR
       (t.NameEn=N'Over The Counter' AND p.RequiresPrescription=0) OR
       (t.NameEn=N'Test Offer' AND p.NameEn IN (N'Paracetamol 500 mg',N'Cough Syrup',N'Children Paracetamol Syrup')))
      AND NOT EXISTS(SELECT 1 FROM dbo.ProductTags pt WHERE pt.ProductId=p.Id AND pt.TagId=t.Id);

    /* One explicit buy-2/get-1 test promotion; ERP identifiers below are synthetic placeholders. */
    DECLARE @PromotionId int;
    SELECT TOP(1) @PromotionId=Id FROM dbo.Promotions WHERE ErpPgoId=990000001 AND DeletedAt IS NULL;
    IF @PromotionId IS NULL
    BEGIN
        INSERT dbo.Promotions(ErpPgoId,Name,Notes,StartAt,EndAt,TotalAmount,BasicAmount,OfferAmount,DiscountPercent,IsActive)
        VALUES(990000001,N'عرض تجريبي: اشترِ 2 واحصل على خصم 50% على الثالثة',N'معرفات ERP أرقام تجريبية وليست ربطًا فعليًا.',
            DATEADD(day,-1,SYSUTCDATETIME()),DATEADD(day,60,SYSUTCDATETIME()),3,2,1,50,1);
        SET @PromotionId=CONVERT(int,SCOPE_IDENTITY());
    END;
    INSERT dbo.PromotionProducts(PromotionId,ProductId,ErpOfferId,ErpProductId)
    SELECT @PromotionId,p.Id,990000001,990100000+p.Id
    FROM dbo.Products p
    WHERE p.StoreId=@StoreId AND p.DeletedAt IS NULL AND p.NameEn IN(N'Paracetamol 500 mg',N'Cough Syrup',N'Children Paracetamol Syrup')
      AND NOT EXISTS(SELECT 1 FROM dbo.PromotionProducts pp WHERE pp.PromotionId=@PromotionId AND pp.ProductId=p.Id AND pp.DeletedAt IS NULL);

    /* Delivery fee zones: one zone per governorate; assign its five seeded areas. */
    INSERT dbo.DeliveryZones(StoreId,NameAr,NameEn,DeliveryFee,FreeDeliveryMinimumAmount,IsActive)
    SELECT @StoreId,CONCAT(N'توصيل ',g.NameAr),CONCAT(N'Delivery - ',g.NameEn),25,500,1
    FROM dbo.Governorates g
    WHERE EXISTS(SELECT 1 FROM dbo.GovernorateAreas a WHERE a.GovernorateId=g.Id)
      AND NOT EXISTS(SELECT 1 FROM dbo.DeliveryZones z WHERE z.StoreId=@StoreId AND z.NameAr=CONCAT(N'توصيل ',g.NameAr));
    INSERT dbo.DeliveryZoneAreas(DeliveryZoneId,StoreId,GovernorateAreaId)
    SELECT z.Id,@StoreId,a.Id FROM dbo.DeliveryZones z
    JOIN dbo.Governorates g ON z.NameAr=CONCAT(N'توصيل ',g.NameAr)
    JOIN dbo.GovernorateAreas a ON a.GovernorateId=g.Id
    WHERE z.StoreId=@StoreId AND NOT EXISTS(SELECT 1 FROM dbo.DeliveryZoneAreas za WHERE za.StoreId=@StoreId AND za.GovernorateAreaId=a.Id);

    /* Attach the uploaded paracetamol image to its actual ProductId. */
    DECLARE @ParacetamolId int=(SELECT TOP(1) Id FROM dbo.Products WHERE StoreId=@StoreId AND NameEn=N'Paracetamol 500 mg' AND DeletedAt IS NULL ORDER BY Id);
    IF @ParacetamolId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.ProductImages WHERE ProductId=@ParacetamolId AND IsPrimary=1 AND DeletedAt IS NULL)
        INSERT dbo.ProductImages(PharmacyId,ProductId,ImageUrl,ThumbnailUrl,AltText,SortOrder,IsPrimary,CreatedBy)
        VALUES(@PharmacyId,@ParacetamolId,
            N'/uploads/original/product-9c06dd7f5c45449cbaec325e4c9d679e.webp',
            N'/uploads/small/product-9c06dd7f5c45449cbaec325e4c9d679e.webp',
            N'باراسيتامول 500 مجم',0,1,N'test-seed');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

/* Quick verification summary */
SELECT Email, RoleName, PermissionName FROM @Users;
SELECT COUNT(*) AS Governorates FROM dbo.Governorates;
SELECT COUNT(*) AS GovernorateAreas FROM dbo.GovernorateAreas;
SELECT COUNT(*) AS ActiveTestProducts FROM dbo.Products WHERE StoreId=@StoreId AND DeletedAt IS NULL;
SELECT COUNT(*) AS ActiveSeedEmployees FROM dbo.Employees WHERE PharmacyId=@PharmacyId AND DeletedAt IS NULL;







