 
 USE master;
 IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = 'pharma_shope_db')
 BEGIN
     CREATE DATABASE pharma_shope_db COLLATE Arabic_100_CI_AS_KS_WS_SC_UTF8;
 END
 GO

USE pharma_shope_db;
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
	Latitude FLOAT NULL,               
    Longitude FLOAT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), 
    IsActive BIT NOT NULL DEFAULT 1            
);

GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(150) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

GO

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE, -- أمثلة: Customer, Admin, Doctor, DeliveryRep
	NameEn NVARCHAR(200) NOT NULL,
    Description NVARCHAR(200) NULL
);

GO

CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    EntityId INT NULL,  -- مثل CustomerId أو DoctorId إن أردت الربط الصريح
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    -- ملاحظة: لا يوجد FK مباشر على EntityId لأنه يمكن أن يشير لجداول مختلفة
);

GO

CREATE TABLE Admins (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PharmacyId INT NOT NULL, -- تشير إلى الصيدلية المالكة (مستقبلاً عند التوسع)
    FullName NVARCHAR(200) NOT NULL,
	FullNameEn NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,

    Role NVARCHAR(100) NOT NULL, -- Admin, Support, Viewer
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
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

	CONSTRAINT FK_Customers_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
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
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

GO

--=======================================
--=======================================
--=======================================
--=======================================
--=======================================
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
	NameEn NVARCHAR(200) NOT NULL,
	DescriptionEn NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
	ParentCategoryId INT NULL, -- ++

	CONSTRAINT FK_Categories_ParentCategory 
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
);

GO
INSERT INTO Categories (Name, NameEn)
VALUES (N'أدوية عامة', 'General Medicines');


CREATE TABLE Products (
    Id INT IDENTITY(1,1),
	CategoryId INT NOT NULL,

    Name NVARCHAR(250) NOT NULL,
	NameEn NVARCHAR(250) NOT NULL,
    Description NVARCHAR(MAX) NULL,
	DescriptionEn NVARCHAR(MAX) NULL,

    Barcode VARCHAR(50) NOT NULL, -- الباركود الخاص بالصيدلية
    InternationalCode VARCHAR(50) NULL ,  -- الكود الدولي إن وجد
    StockProductCode VARCHAR(50) NULL , -- الكود في نظام stock

    Price DECIMAL(18,2) NOT NULL, -- 200 EGP
    OldPrice DECIMAL(18,2) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1, -- InStock
    IsIntegrated BIT NOT NULL DEFAULT 0,
    IntegratedAt DATETIME NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CreatedBy NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 1,

	-- +
	Points DECIMAL(18,2) NULL,-- 5% == 10 EPG = 50 points
	PromoDisc DECIMAL(18,2) NULL, -- عرض خصم بيع مباشر
	PromoEndDate DATETIME NULL, 

	IsGroupOffer BIT NOT NULL DEFAULT 0, -- هل عليه عرض؟

	CONSTRAINT UQ_Products_Barcode UNIQUE (Barcode),
    --CONSTRAINT UQ_Products_InternationalCode UNIQUE (InternationalCode),
    --CONSTRAINT UQ_Products_StockProductCode UNIQUE (StockProductCode),


    INDEX IX_Products_CategoryId (CategoryId),
    INDEX IX_Products_CreatedAt (CreatedAt),
	INDEX IX_Products_Name (Name),
	INDEX IX_Products_NameEn (NameEn),

	CONSTRAINT PK_Products PRIMARY KEY (Id),

    CONSTRAINT FK_Products_Category_CategoryId
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);


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
		REFERENCES Products(Id)ON DELETE CASCADE ON UPDATE CASCADE,
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
		REFERENCES Users(Id)
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
		REFERENCES Users(Id)

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






