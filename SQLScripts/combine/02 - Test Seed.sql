USE [pharma_shope_db];
GO
/*
    PharmaShop test seed data
    - Safe to run repeatedly: inserts missing seed records and reuses existing records.
    - No schema changes. Seeds representative operational records for API testing.
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
   OR OBJECT_ID(N'dbo.Carts', N'U') IS NULL OR OBJECT_ID(N'dbo.CartItems', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Prescriptions', N'U') IS NULL OR OBJECT_ID(N'dbo.PrescriptionItems', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Orders', N'U') IS NULL OR OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
   OR OBJECT_ID(N'dbo.CustomerPointLots', N'U') IS NULL OR OBJECT_ID(N'dbo.CustomerPointTransactions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.ProductInventoryTransactions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.ReturnRequests', N'U') IS NULL OR OBJECT_ID(N'dbo.ReturnRequestItems', N'U') IS NULL
   OR OBJECT_ID(N'dbo.OrderDeliveryAssignments', N'U') IS NULL OR OBJECT_ID(N'dbo.OrderStatusHistories', N'U') IS NULL
   OR OBJECT_ID(N'dbo.UserNotifications', N'U') IS NULL OR OBJECT_ID(N'dbo.CustomerNotificationPreferences', N'U') IS NULL
   OR OBJECT_ID(N'dbo.PaymentAttempts', N'U') IS NULL OR OBJECT_ID(N'dbo.PaymentRefunds', N'U') IS NULL
   OR OBJECT_ID(N'dbo.PromotionalBanners', N'U') IS NULL
    THROW 51002, 'Required PharmaShop tables are missing. Apply the current schema scripts first.', 1;
IF COL_LENGTH(N'dbo.Orders', N'CustomerNotes') IS NULL OR COL_LENGTH(N'dbo.Stores', N'ContactPhone') IS NULL
    THROW 51003, 'Apply SQLScripts/new/17 and 18 before running this seed script.', 1;

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
    UPDATE dbo.Stores
       SET ContactPhone = COALESCE(ContactPhone, N'0220000000'),
           ContactWhatsApp = COALESCE(ContactWhatsApp, N'201000000001'),
           ContactEmail = COALESCE(ContactEmail, N'contact@modernsoft.com'),
           WorkingHours = COALESCE(WorkingHours, N'يوميًا من 09:00 إلى 23:00')
     WHERE Id = @StoreId;
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
    SELECT x.Id,u.FullNameAr,u.FullNameEn,u.Phone,u.Email,N'Regular',0,1
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

    /* Resolve the seed actors once for all operational records below. */
    DECLARE @AdminUserId int, @CustomerUserId int, @PharmacistUserId int, @DeliveryUserId int;
    DECLARE @PharmacistEmployeeId int, @PointsEmployeeId int, @StockEmployeeId int, @DeliveryEmployeeId int;
    SELECT @AdminUserId=Id FROM dbo.AspNetUsers WHERE NormalizedEmail=N'ADMIN@MODERNSOFT.COM';
    SELECT @CustomerUserId=Id FROM dbo.AspNetUsers WHERE NormalizedEmail=N'MOFAWZY@USER.COM';
    SELECT @PharmacistUserId=Id FROM dbo.AspNetUsers WHERE NormalizedEmail=N'M.HELAL@MODERNSOFT.COM';
    SELECT @DeliveryUserId=Id FROM dbo.AspNetUsers WHERE NormalizedEmail=N'DELIVERY@MODERNSOFT.COM';
    SELECT @PharmacistEmployeeId=Id FROM dbo.Employees WHERE UserId=@PharmacistUserId AND DeletedAt IS NULL;
    SELECT @PointsEmployeeId=Id FROM dbo.Employees WHERE Email=N'points@modernsoft.com' AND DeletedAt IS NULL;
    SELECT @StockEmployeeId=Id FROM dbo.Employees WHERE Email=N'inventory@modernsoft.com' AND DeletedAt IS NULL;
    SELECT @DeliveryEmployeeId=Id FROM dbo.Employees WHERE UserId=@DeliveryUserId AND DeletedAt IS NULL;
    IF @CustomerId IS NULL OR @AdminUserId IS NULL OR @CustomerUserId IS NULL OR @PharmacistUserId IS NULL
       OR @PharmacistEmployeeId IS NULL OR @PointsEmployeeId IS NULL OR @StockEmployeeId IS NULL
       OR @DeliveryUserId IS NULL OR @DeliveryEmployeeId IS NULL
        THROW 51004, 'Required seed accounts/employees are missing; check the user and employee setup above.', 1;

    /* Set one catalog item as eligible for points redemption. Product.Points remains its earn-points value. */
    DECLARE @RedeemableProductId int;
    SELECT TOP(1) @RedeemableProductId=Id FROM dbo.Products
    WHERE StoreId=@StoreId AND NameEn=N'Vitamin D 1000 IU' AND DeletedAt IS NULL ORDER BY Id;
    DECLARE @VitaminCategoryId int, @SampleCompanyId int;
    SELECT TOP(1) @VitaminCategoryId=Id FROM dbo.Categories WHERE NameEn=N'Vitamin D' AND IsDeleted=0 ORDER BY Id;
    SELECT TOP(1) @SampleCompanyId=Id FROM dbo.Companies WHERE NameEn=N'SEDICO' AND DeletedAt IS NULL ORDER BY Id;
    IF @RedeemableProductId IS NULL OR @VitaminCategoryId IS NULL OR @SampleCompanyId IS NULL
        THROW 51005, 'Required sample product/category/company records are missing from the seed catalog.', 1;
    IF @RedeemableProductId IS NOT NULL
        UPDATE dbo.Products SET IsRedeemableByPoints=1, IsAvailable=1, IsActive=1
        WHERE Id=@RedeemableProductId;

    /* Promotional banners use existing uploaded files and valid catalog/promotion targets. */
    INSERT dbo.PromotionalBanners(Name,Placement,ActionType,TargetId,ParentTargetId,ActionUrl,ImageUrl,ThumbnailUrl,AltText,DisplayOrder,IsActive,StartsAt,EndsAt)
    SELECT v.Name,v.Placement,v.ActionType,v.TargetId,NULL,v.ActionUrl,v.ImageUrl,v.ThumbnailUrl,v.AltText,v.DisplayOrder,1,NULL,NULL
    FROM (VALUES
      (N'بانر الصفحة الرئيسية - فيتامين د',CONVERT(tinyint,0),CONVERT(tinyint,1),@RedeemableProductId,CONVERT(nvarchar(2048),NULL),N'/uploads/original/product-9c06dd7f5c45449cbaec325e4c9d679e.webp',N'/uploads/small/product-9c06dd7f5c45449cbaec325e4c9d679e.webp',N'منتجات صيدلية تجريبية',0),
      (N'بانر العروض التجريبية',CONVERT(tinyint,1),CONVERT(tinyint,2),@PromotionId,CONVERT(nvarchar(2048),NULL),N'/uploads/original/product-9c06dd7f5c45449cbaec325e4c9d679e.webp',N'/uploads/small/product-9c06dd7f5c45449cbaec325e4c9d679e.webp',N'عرض تجريبي على منتجات مختارة',0),
      (N'بانر الفئات',CONVERT(tinyint,2),CONVERT(tinyint,3),@VitaminCategoryId,CONVERT(nvarchar(2048),NULL),N'/uploads/original/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'/uploads/small/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'تصفح تصنيف فيتامين د',0),
      (N'بانر الشركة التجريبية',CONVERT(tinyint,2),CONVERT(tinyint,4),@SampleCompanyId,CONVERT(nvarchar(2048),NULL),N'/uploads/original/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'/uploads/small/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'منتجات الشركة التجريبية',1),
      (N'بانر بدون إجراء',CONVERT(tinyint,0),CONVERT(tinyint,0),CONVERT(int,NULL),CONVERT(nvarchar(2048),NULL),N'/uploads/original/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'/uploads/small/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'بانر معلوماتي تجريبي',2),
      (N'بانر رابط خارجي تجريبي',CONVERT(tinyint,0),CONVERT(tinyint,5),CONVERT(int,NULL),N'https://example.com/pharmashop-offer',N'/uploads/original/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'/uploads/small/CAT1-6dcf25f1074742048090fa767113b678.jpg',N'رابط توضيحي خارجي',1)
    ) v(Name,Placement,ActionType,TargetId,ActionUrl,ImageUrl,ThumbnailUrl,AltText,DisplayOrder)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.PromotionalBanners b WHERE b.Name=v.Name AND b.DeletedAt IS NULL)
      AND (v.Name<>N'بانر الصفحة الرئيسية - فيتامين د' OR v.TargetId IS NOT NULL)
      AND (v.Name<>N'بانر العروض التجريبية' OR v.TargetId IS NOT NULL)
      AND (v.Name<>N'بانر الفئات' OR v.TargetId IS NOT NULL)
      AND (v.Name<>N'بانر الشركة التجريبية' OR v.TargetId IS NOT NULL);

    /* Three prescription states for mobile and pharmacy review flows. */
    DECLARE @PrescriptionSeeds TABLE(SeedKey varchar(30) PRIMARY KEY, Status tinyint, Notes nvarchar(500), RejectReason nvarchar(300));
    INSERT @PrescriptionSeeds VALUES
      ('SUBMITTED',1,N'وصفة تجريبية جديدة للاختبار',NULL),
      ('READY',3,N'وصفة تجريبية تمت مراجعتها',NULL),
      ('REJECTED',5,N'وصفة تجريبية مرفوضة',N'الصورة غير واضحة - سبب تجريبي');
    INSERT dbo.Prescriptions(CustomerId,StoreId,Status,StatusUpdatedAt,ReviewedBy,ReadyForCheckoutAt,RejectReason,Notes,CreatedAt)
    SELECT @CustomerId,@StoreId,s.Status,SYSUTCDATETIME(),
           CASE WHEN s.Status IN(3,5) THEN @PharmacistEmployeeId ELSE NULL END,
           CASE WHEN s.Status=3 THEN SYSUTCDATETIME() ELSE NULL END,
           s.RejectReason,s.Notes,DATEADD(day,-s.Status,SYSUTCDATETIME())
    FROM @PrescriptionSeeds s
    WHERE NOT EXISTS(SELECT 1 FROM dbo.Prescriptions p WHERE p.CustomerId=@CustomerId AND p.Notes=s.Notes);
    INSERT dbo.PrescriptionItems(PrescriptionId,ProductId,RequestedName,RequestedQuantity,Notes,CreatedAt)
    SELECT p.Id,@RedeemableProductId,
           CASE WHEN p.Status=5 THEN N'دواء تجريبي - وصفة مرفوضة' ELSE N'دواء موصوف تجريبي - يرجى مراجعة الصيدلي' END,
           1,N'سطر تجريبي لا يمثل وصفة طبية حقيقية',p.CreatedAt
    FROM dbo.Prescriptions p
    WHERE p.CustomerId=@CustomerId AND p.Notes IN(N'وصفة تجريبية جديدة للاختبار',N'وصفة تجريبية تمت مراجعتها',N'وصفة تجريبية مرفوضة')
      AND NOT EXISTS(SELECT 1 FROM dbo.PrescriptionItems i WHERE i.PrescriptionId=p.Id);

    /* One active regular cart, without touching checkout or creating a redemption cart. */
    DECLARE @CartId int;
    SELECT TOP(1) @CartId=Id FROM dbo.Carts WHERE CustomerId=@CustomerId AND Status=1 ORDER BY Id;
    IF @CartId IS NULL
    BEGIN
        INSERT dbo.Carts(CustomerId,StoreId,Status,DeviceId,AppInstanceId)
        VALUES(@CustomerId,@StoreId,1,N'test-seed-device',N'test-seed-app');
        SET @CartId=CONVERT(int,SCOPE_IDENTITY());
    END;
    INSERT dbo.CartItems(CartId,ProductId,SourceType,UnitLevel,Quantity,UnitPriceSnapshot,CurrentUnitPriceSnapshot,MinOrderQtySnapshot,IsValid)
    SELECT @CartId,p.Id,1,1,1,p.OuterUnitPrice,p.OuterUnitPrice,p.MinOrderQty,1
    FROM dbo.Products p
    WHERE p.StoreId=@StoreId AND p.NameEn IN(N'Paracetamol 500 mg',N'Vitamin D 1000 IU') AND p.DeletedAt IS NULL
      AND NOT EXISTS(SELECT 1 FROM dbo.CartItems ci WHERE ci.CartId=@CartId AND ci.ProductId=p.Id AND ci.UnitLevel=1);

    /* Eight idempotent sample orders, one for each supported order status. */
    DECLARE @ReadyPrescriptionId int;
    SELECT TOP(1) @ReadyPrescriptionId=Id FROM dbo.Prescriptions
    WHERE CustomerId=@CustomerId AND Notes=N'وصفة تجريبية تمت مراجعتها' ORDER BY Id;
    DECLARE @SeedOrders TABLE(OrderNumber nvarchar(30) PRIMARY KEY, OrderStatus tinyint, ProductNameEn nvarchar(250), DayOffset int, PrescriptionId int NULL, OrderSource tinyint);
    INSERT @SeedOrders VALUES
      (N'SEED-ORD-0001',1,N'Daily Skin Moisturizer',0,NULL,1),
      (N'SEED-ORD-0002',2,N'Allergy Tablets',1,NULL,1),
      (N'SEED-ORD-0003',3,N'Sensitive Skin Cleanser',2,NULL,1),
      (N'SEED-ORD-0004',4,N'Calcium Tablets',3,NULL,1),
      (N'SEED-ORD-0005',5,N'Paracetamol Tablets',4,NULL,1),
      (N'SEED-ORD-0006',6,N'Acne Care Gel',5,NULL,1),
      (N'SEED-ORD-0007',7,N'Iron Supplement Tablets',6,NULL,1),
      (N'SEED-ORD-0008',8,N'Vitamin D 1000 IU',7,@ReadyPrescriptionId,2);
    INSERT dbo.Orders(OrderNumber,CustomerId,StoreId,OrderSource,PrescriptionId,Status,StatusUpdatedAt,PaymentMethod,PaymentStatus,
        Subtotal,ItemsDiscountTotal,DeliveryFee,RedeemedPoints,RedeemedAmount,GrandTotal,EarnedPoints,AddressId,DeliveryTitle,DeliveryCity,
        DeliveryRegion,DeliveryStreet,DeliveryPhone,Notes,CustomerNotes,CreatedAt)
    SELECT s.OrderNumber,@CustomerId,@StoreId,s.OrderSource,s.PrescriptionId,s.OrderStatus,DATEADD(day,-s.DayOffset,SYSUTCDATETIME()),1,
        CASE WHEN s.OrderStatus=5 THEN 2 WHEN s.OrderStatus=7 THEN 3 ELSE 1 END,
        p.OuterUnitPrice,0,25,0,0,p.OuterUnitPrice+25,0,
        (SELECT TOP(1) Id FROM dbo.CustomerAddresses WHERE CustomerId=@CustomerId AND IsDefault=1 AND DeletedAt IS NULL),
        N'المنزل',N'القاهرة',N'مدينة نصر',N'شارع تجريبي - طلب بيانات اختبار',N'01000000003',
        N'طلب بيانات اختبار - لا يمثل معاملة حقيقية',CONCAT(N'ملاحظة عميل تجريبية للحالة ',s.OrderStatus),DATEADD(day,-s.DayOffset,SYSUTCDATETIME())
    FROM @SeedOrders s JOIN dbo.Products p ON p.StoreId=@StoreId AND p.NameEn=s.ProductNameEn AND p.DeletedAt IS NULL
    WHERE (s.OrderStatus<>8 OR s.PrescriptionId IS NOT NULL)
      AND NOT EXISTS(SELECT 1 FROM dbo.Orders o WHERE o.OrderNumber=s.OrderNumber);

    INSERT dbo.OrderItems(OrderId,ProductId,UnitLevel,Quantity,OuterUnitIdSnapshot,InnerUnitIdSnapshot,InnerPerOuterSnapshot,
        UnitPriceSnapshot,DiscountPercentSnapshot,FinalUnitPriceSnapshot,LineTotal,PointsSnapshot,EarnedPoints,AppliedPromotionId,CreatedAt)
    SELECT o.Id,p.Id,1,1,p.OuterUnitId,NULL,NULL,p.OuterUnitPrice,0,p.OuterUnitPrice,p.OuterUnitPrice,p.Points,0,NULL,o.CreatedAt
    FROM @SeedOrders s JOIN dbo.Orders o ON o.OrderNumber=s.OrderNumber
    JOIN dbo.Products p ON p.StoreId=@StoreId AND p.NameEn=s.ProductNameEn AND p.DeletedAt IS NULL
    WHERE NOT EXISTS(SELECT 1 FROM dbo.OrderItems oi WHERE oi.OrderId=o.Id AND oi.ProductId=p.Id AND oi.UnitLevel=1);

    /* Full status paths make the seeded order history useful in the admin API. */
    DECLARE @StatusPath TABLE(OrderNumber nvarchar(30), PreviousStatus tinyint NULL, NewStatus tinyint, StepNo int);
    INSERT @StatusPath VALUES
      (N'SEED-ORD-0001',NULL,1,1),
      (N'SEED-ORD-0002',NULL,1,1),(N'SEED-ORD-0002',1,2,2),
      (N'SEED-ORD-0003',NULL,1,1),(N'SEED-ORD-0003',1,2,2),(N'SEED-ORD-0003',2,3,3),
      (N'SEED-ORD-0004',NULL,1,1),(N'SEED-ORD-0004',1,2,2),(N'SEED-ORD-0004',2,3,3),(N'SEED-ORD-0004',3,4,4),
      (N'SEED-ORD-0005',NULL,1,1),(N'SEED-ORD-0005',1,2,2),(N'SEED-ORD-0005',2,3,3),(N'SEED-ORD-0005',3,4,4),(N'SEED-ORD-0005',4,5,5),
      (N'SEED-ORD-0006',NULL,1,1),(N'SEED-ORD-0006',1,2,2),(N'SEED-ORD-0006',2,6,3),
      (N'SEED-ORD-0007',NULL,1,1),(N'SEED-ORD-0007',1,2,2),(N'SEED-ORD-0007',2,3,3),(N'SEED-ORD-0007',3,4,4),(N'SEED-ORD-0007',4,5,5),(N'SEED-ORD-0007',5,7,6),
      (N'SEED-ORD-0008',NULL,1,1),(N'SEED-ORD-0008',1,8,2);
    INSERT dbo.OrderStatusHistories(OrderId,PreviousStatus,NewStatus,ChangedByUserId,ChangedByRole,Notes,ChangedAt)
    SELECT o.Id,h.PreviousStatus,h.NewStatus,CASE WHEN h.StepNo=1 THEN @CustomerUserId ELSE @AdminUserId END,
           CASE WHEN h.StepNo=1 THEN N'Customer' ELSE N'Admin' END,N'انتقال حالة تجريبي',DATEADD(minute,h.StepNo,o.CreatedAt)
    FROM @StatusPath h JOIN dbo.Orders o ON o.OrderNumber=h.OrderNumber
    WHERE NOT EXISTS(SELECT 1 FROM dbo.OrderStatusHistories x WHERE x.OrderId=o.Id AND x.NewStatus=h.NewStatus
        AND ISNULL(x.PreviousStatus,0)=ISNULL(h.PreviousStatus,0) AND x.Notes=N'انتقال حالة تجريبي');

    /* Delivery assignments for an in-transit order plus one completed assignment history. */
    INSERT dbo.OrderDeliveryAssignments(OrderId,DeliveryAgentUserId,AssignedByUserId,AssignedAt,UnassignedAt,IsActive,UnassignedByUserId,Notes)
    SELECT o.Id,@DeliveryUserId,@AdminUserId,o.CreatedAt,NULL,1,NULL,N'مندوب تجريبي - قيد التوصيل'
    FROM dbo.Orders o WHERE o.OrderNumber=N'SEED-ORD-0004'
      AND NOT EXISTS(SELECT 1 FROM dbo.OrderDeliveryAssignments a WHERE a.OrderId=o.Id AND a.IsActive=1);
    INSERT dbo.OrderDeliveryAssignments(OrderId,DeliveryAgentUserId,AssignedByUserId,AssignedAt,UnassignedAt,IsActive,UnassignedByUserId,Notes)
    SELECT o.Id,@DeliveryUserId,@AdminUserId,o.CreatedAt,o.StatusUpdatedAt,0,@AdminUserId,N'تسليم تجريبي مكتمل'
    FROM dbo.Orders o WHERE o.OrderNumber=N'SEED-ORD-0005'
      AND NOT EXISTS(SELECT 1 FROM dbo.OrderDeliveryAssignments a WHERE a.OrderId=o.Id AND a.Notes=N'تسليم تجريبي مكتمل');

    /* One approved return request linked to the returned order line. */
    IF NOT EXISTS(SELECT 1 FROM dbo.ReturnRequests r JOIN dbo.Orders o ON o.Id=r.OrderId WHERE o.OrderNumber=N'SEED-ORD-0007')
        INSERT dbo.ReturnRequests(OrderId,CustomerId,Status,Reason,RequestedAt,ReviewedByEmployeeId,ReviewedByUserId,ReviewedAt,ReviewNotes)
        SELECT o.Id,@CustomerId,2,N'طلب إرجاع تجريبي',o.CreatedAt,@PharmacistEmployeeId,@PharmacistUserId,SYSUTCDATETIME(),N'تمت الموافقة للاختبار'
        FROM dbo.Orders o WHERE o.OrderNumber=N'SEED-ORD-0007';
    INSERT dbo.ReturnRequestItems(ReturnRequestId,OrderItemId,Quantity)
    SELECT r.Id,oi.Id,oi.Quantity FROM dbo.ReturnRequests r JOIN dbo.Orders o ON o.Id=r.OrderId
    JOIN dbo.OrderItems oi ON oi.OrderId=o.Id
    WHERE o.OrderNumber=N'SEED-ORD-0007'
      AND NOT EXISTS(SELECT 1 FROM dbo.ReturnRequestItems ri WHERE ri.ReturnRequestId=r.Id AND ri.OrderItemId=oi.Id);

    /* Inventory movements mirror confirmation and the cancel/return restock of seeded orders. */
    DECLARE @InventoryOrderItems TABLE(OrderNumber nvarchar(30), MovementType tinyint, ItemId int, ProductId int, Qty decimal(18,3), UnitId int, EmployeeId int);
    INSERT @InventoryOrderItems
    SELECT o.OrderNumber,4,oi.Id,p.Id,oi.Quantity,p.OuterUnitId,@StockEmployeeId
    FROM dbo.Orders o JOIN dbo.OrderItems oi ON oi.OrderId=o.Id JOIN dbo.Products p ON p.Id=oi.ProductId
    WHERE o.OrderNumber IN(N'SEED-ORD-0002',N'SEED-ORD-0003',N'SEED-ORD-0004',N'SEED-ORD-0005',N'SEED-ORD-0006',N'SEED-ORD-0007');
    DECLARE @InventoryOrderNumber nvarchar(30), @InventoryType tinyint, @InventoryItemId int, @InventoryProductId int,
            @InventoryQty decimal(18,3), @InventoryUnitId int, @InventoryEmployeeId int, @InventoryBefore decimal(18,3);
    DECLARE InventorySeedCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT OrderNumber,MovementType,ItemId,ProductId,Qty,UnitId,EmployeeId FROM @InventoryOrderItems ORDER BY OrderNumber;
    OPEN InventorySeedCursor;
    FETCH NEXT FROM InventorySeedCursor INTO @InventoryOrderNumber,@InventoryType,@InventoryItemId,@InventoryProductId,@InventoryQty,@InventoryUnitId,@InventoryEmployeeId;
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM dbo.ProductInventoryTransactions WHERE OrderId=(SELECT Id FROM dbo.Orders WHERE OrderNumber=@InventoryOrderNumber) AND OrderItemId=@InventoryItemId AND TransactionType=4)
        BEGIN
            SELECT @InventoryBefore=Quantity FROM dbo.Products WHERE Id=@InventoryProductId;
            INSERT dbo.ProductInventoryTransactions(ProductId,EmployeeId,OrderId,OrderItemId,ProductUnitId,TransactionType,Quantity,QuantityBefore,QuantityAfter,Reason)
            SELECT @InventoryProductId,@InventoryEmployeeId,o.Id,@InventoryItemId,@InventoryUnitId,4,@InventoryQty,@InventoryBefore,@InventoryBefore-@InventoryQty,N'خصم مخزون لطلب تجريبي'
            FROM dbo.Orders o WHERE o.OrderNumber=@InventoryOrderNumber;
            UPDATE dbo.Products SET Quantity=@InventoryBefore-@InventoryQty WHERE Id=@InventoryProductId;
        END;
        FETCH NEXT FROM InventorySeedCursor INTO @InventoryOrderNumber,@InventoryType,@InventoryItemId,@InventoryProductId,@InventoryQty,@InventoryUnitId,@InventoryEmployeeId;
    END;
    CLOSE InventorySeedCursor;
    DEALLOCATE InventorySeedCursor;

    /* Restore stock for the cancelled order (type 5) and approved return (type 6). */
    INSERT dbo.ProductInventoryTransactions(ProductId,EmployeeId,OrderId,OrderItemId,ProductUnitId,TransactionType,Quantity,QuantityBefore,QuantityAfter,Reason)
    SELECT p.Id,@StockEmployeeId,o.Id,oi.Id,p.OuterUnitId,5,oi.Quantity,p.Quantity,p.Quantity+oi.Quantity,N'إعادة مخزون طلب ملغي تجريبي'
    FROM dbo.Orders o JOIN dbo.OrderItems oi ON oi.OrderId=o.Id JOIN dbo.Products p ON p.Id=oi.ProductId
    WHERE o.OrderNumber=N'SEED-ORD-0006'
      AND NOT EXISTS(SELECT 1 FROM dbo.ProductInventoryTransactions x WHERE x.OrderId=o.Id AND x.OrderItemId=oi.Id AND x.TransactionType=5);
    UPDATE p SET Quantity=p.Quantity+oi.Quantity FROM dbo.Products p JOIN dbo.OrderItems oi ON oi.ProductId=p.Id
      JOIN dbo.Orders o ON o.Id=oi.OrderId JOIN dbo.ProductInventoryTransactions it ON it.OrderId=o.Id AND it.OrderItemId=oi.Id AND it.TransactionType=5
    WHERE o.OrderNumber=N'SEED-ORD-0006' AND it.Reason=N'إعادة مخزون طلب ملغي تجريبي'
      AND it.QuantityAfter=p.Quantity+oi.Quantity; -- updated below only for a newly-created movement

    INSERT dbo.ProductInventoryTransactions(ProductId,EmployeeId,ReturnRequestItemId,OrderId,OrderItemId,ProductUnitId,TransactionType,Quantity,QuantityBefore,QuantityAfter,Reason)
    SELECT p.Id,@StockEmployeeId,ri.Id,o.Id,oi.Id,p.OuterUnitId,6,ri.Quantity,p.Quantity,p.Quantity+ri.Quantity,N'إعادة مخزون مرتجع تجريبي'
    FROM dbo.ReturnRequestItems ri JOIN dbo.ReturnRequests r ON r.Id=ri.ReturnRequestId JOIN dbo.Orders o ON o.Id=r.OrderId
    JOIN dbo.OrderItems oi ON oi.Id=ri.OrderItemId JOIN dbo.Products p ON p.Id=oi.ProductId
    WHERE o.OrderNumber=N'SEED-ORD-0007' AND r.Status=2
      AND NOT EXISTS(SELECT 1 FROM dbo.ProductInventoryTransactions x WHERE x.ReturnRequestItemId=ri.Id);
    UPDATE p SET Quantity=p.Quantity+ri.Quantity FROM dbo.Products p JOIN dbo.OrderItems oi ON oi.ProductId=p.Id
      JOIN dbo.ReturnRequestItems ri ON ri.OrderItemId=oi.Id JOIN dbo.ReturnRequests r ON r.Id=ri.ReturnRequestId
      JOIN dbo.Orders o ON o.Id=r.OrderId JOIN dbo.ProductInventoryTransactions it ON it.ReturnRequestItemId=ri.Id
    WHERE o.OrderNumber=N'SEED-ORD-0007' AND it.Reason=N'إعادة مخزون مرتجع تجريبي'
      AND it.QuantityAfter=p.Quantity+ri.Quantity; -- updated below only for a newly-created movement

    /* Test payments: captured on delivery, refunded after return. */
    INSERT dbo.PaymentAttempts(OrderId,Provider,ExternalReference,Amount,Status,CreatedByUserId,CreatedAt)
    SELECT o.Id,N'TEST',N'SEED-PAY-0005',o.GrandTotal,2,@AdminUserId,o.CreatedAt FROM dbo.Orders o
    WHERE o.OrderNumber=N'SEED-ORD-0005' AND NOT EXISTS(SELECT 1 FROM dbo.PaymentAttempts p WHERE p.Provider=N'TEST' AND p.ExternalReference=N'SEED-PAY-0005');
    INSERT dbo.PaymentAttempts(OrderId,Provider,ExternalReference,Amount,Status,CreatedByUserId,CreatedAt)
    SELECT o.Id,N'TEST',N'SEED-PAY-0007',o.GrandTotal,2,@AdminUserId,o.CreatedAt FROM dbo.Orders o
    WHERE o.OrderNumber=N'SEED-ORD-0007' AND NOT EXISTS(SELECT 1 FROM dbo.PaymentAttempts p WHERE p.Provider=N'TEST' AND p.ExternalReference=N'SEED-PAY-0007');
    INSERT dbo.PaymentRefunds(PaymentAttemptId,Amount,Status,ExternalReference,Reason,RequestedByUserId,ProcessedByUserId,CreatedAt,ProcessedAt)
    SELECT p.Id,p.Amount,3,N'SEED-REF-0007',N'استرداد تجريبي لطلب مرتجع',@CustomerUserId,@AdminUserId,SYSUTCDATETIME(),SYSUTCDATETIME()
    FROM dbo.PaymentAttempts p WHERE p.Provider=N'TEST' AND p.ExternalReference=N'SEED-PAY-0007'
      AND NOT EXISTS(SELECT 1 FROM dbo.PaymentRefunds r WHERE r.ExternalReference=N'SEED-REF-0007');

    /* Preserve any old aggregate-only test balance by moving the missing amount into a point lot. */
    DECLARE @SeedLegacyPoints int, @SeedLegacyBalance int, @SeedLegacyReferenceId int=1300000000+@CustomerId, @SeedLegacyLotId int;
    SELECT @SeedLegacyPoints=CASE WHEN c.Points>ISNULL(x.AvailablePoints,0) THEN c.Points-ISNULL(x.AvailablePoints,0) ELSE 0 END
    FROM dbo.Customers c OUTER APPLY
    (
        SELECT SUM(RemainingPoints) AS AvailablePoints FROM dbo.CustomerPointLots
        WHERE CustomerId=c.Id AND Status=2 AND RemainingPoints>0 AND ExpiresAt>SYSUTCDATETIME()
    ) x WHERE c.Id=@CustomerId;
    SELECT @SeedLegacyBalance=@SeedLegacyPoints+ISNULL(SUM(RemainingPoints),0)
    FROM dbo.CustomerPointLots
    WHERE CustomerId=@CustomerId AND Status=2 AND RemainingPoints>0 AND ExpiresAt>SYSUTCDATETIME();
    IF @SeedLegacyPoints>0 AND NOT EXISTS(SELECT 1 FROM dbo.CustomerPointTransactions WHERE TransactionType=7 AND ReferenceType=3 AND ReferenceId=@SeedLegacyReferenceId)
    BEGIN
        INSERT dbo.CustomerPointLots(CustomerId,PointsTotal,RemainingPoints,Status,EarnedAt,AvailableAt,ExpiresAt)
        VALUES(@CustomerId,@SeedLegacyPoints,@SeedLegacyPoints,2,SYSUTCDATETIME(),SYSUTCDATETIME(),DATEADD(day,90,SYSUTCDATETIME()));
        SET @SeedLegacyLotId=CONVERT(int,SCOPE_IDENTITY());
        INSERT dbo.CustomerPointTransactions(CustomerId,LotId,TransactionType,PointsDelta,BalanceAfter,ReferenceType,ReferenceId,CreatedByEmployeeId,Notes)
        SELECT @CustomerId,@SeedLegacyLotId,7,@SeedLegacyPoints,@SeedLegacyBalance,3,@SeedLegacyReferenceId,@PointsEmployeeId,
               N'تحويل رصيد نقاط الاختبار القديم إلى رصيد قابل للاستخدام';
    END;

    /* Large, available points balance in a 90-day lot; stable reference prevents duplicate grants. */
    DECLARE @SeedGrantReferenceId int=1400000000+@CustomerId, @SeedGrantPoints int=50000, @SeedGrantLotId int;
    IF NOT EXISTS(SELECT 1 FROM dbo.CustomerPointTransactions WHERE TransactionType=7 AND ReferenceType=3 AND ReferenceId=@SeedGrantReferenceId)
    BEGIN
        INSERT dbo.CustomerPointLots(CustomerId,PointsTotal,RemainingPoints,Status,EarnedAt,AvailableAt,ExpiresAt)
        VALUES(@CustomerId,@SeedGrantPoints,@SeedGrantPoints,2,SYSUTCDATETIME(),SYSUTCDATETIME(),DATEADD(day,90,SYSUTCDATETIME()));
        SET @SeedGrantLotId=CONVERT(int,SCOPE_IDENTITY());
        INSERT dbo.CustomerPointTransactions(CustomerId,LotId,TransactionType,PointsDelta,BalanceAfter,ExpiresAt,ReferenceType,ReferenceId,CreatedByEmployeeId,Notes)
        SELECT @CustomerId,@SeedGrantLotId,7,@SeedGrantPoints,
               ISNULL((SELECT SUM(RemainingPoints) FROM dbo.CustomerPointLots WHERE CustomerId=@CustomerId AND Status=2 AND ExpiresAt>SYSUTCDATETIME()),0),
               NULL,3,@SeedGrantReferenceId,@PointsEmployeeId,N'منحة نقاط اختبارية متاحة لمدة 90 يومًا';
    END;
    SELECT @SeedGrantLotId=LotId FROM dbo.CustomerPointTransactions
    WHERE TransactionType=7 AND ReferenceType=3 AND ReferenceId=@SeedGrantReferenceId;
    UPDATE dbo.Customers SET Points=ISNULL((SELECT SUM(RemainingPoints) FROM dbo.CustomerPointLots
        WHERE CustomerId=@CustomerId AND Status=2 AND ExpiresAt>SYSUTCDATETIME()),0)
    WHERE Id=@CustomerId;

    /* Notification settings and inbox examples. */
    IF NOT EXISTS(SELECT 1 FROM dbo.CustomerNotificationPreferences WHERE UserId=@CustomerUserId)
        INSERT dbo.CustomerNotificationPreferences(UserId) VALUES(@CustomerUserId);
    DECLARE @InTransitOrderId int;
    SELECT @InTransitOrderId=Id FROM dbo.Orders WHERE OrderNumber=N'SEED-ORD-0004';
    INSERT dbo.UserNotifications(UserId,Type,Title,Message,RelatedEntityType,RelatedEntityId,IsRead,CreatedAt)
    SELECT @CustomerUserId,v.Type,v.Title,v.Message,v.RelatedEntityType,v.RelatedEntityId,0,SYSUTCDATETIME()
    FROM (VALUES
      (N'Order',N'تحديث طلب تجريبي',N'تم تحديث حالة أحد طلباتك التجريبية.',N'Order',CONVERT(bigint,@InTransitOrderId)),
      (N'Points',N'تمت إضافة نقاط تجريبية',N'أضيفت نقاط اختبار إلى رصيدك وتنتهي خلال 90 يومًا.',N'Points',CONVERT(bigint,@SeedGrantLotId)),
      (N'Prescription',N'تحديث وصفة تجريبية',N'تمت مراجعة وصفة اختبارية.',N'Prescription',CONVERT(bigint,@ReadyPrescriptionId))
    ) v(Type,Title,Message,RelatedEntityType,RelatedEntityId)
    WHERE v.RelatedEntityId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.UserNotifications n WHERE n.UserId=@CustomerUserId AND n.Type=v.Type AND n.Title=v.Title);

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
SELECT OrderNumber,Status,PaymentStatus,GrandTotal FROM dbo.Orders WHERE CustomerId=@CustomerId AND OrderNumber LIKE N'SEED-ORD-%' ORDER BY OrderNumber;
SELECT Points AS AvailableCustomerPoints FROM dbo.Customers WHERE Id=@CustomerId;
SELECT COUNT(*) AS ActiveSeedBanners FROM dbo.PromotionalBanners WHERE DeletedAt IS NULL AND Name LIKE N'بانر %';







