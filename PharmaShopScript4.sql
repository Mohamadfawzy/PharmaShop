

CREATE TABLE Pharmacies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,                
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


CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

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

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
	ParentCategoryId INT NULL, -- ++
);

CREATE TABLE Sub1categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CategoryId INT NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Subcategories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Sub2categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CategoryId INT NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Subcategories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	SubCategoryId INT NOT NULL,
    Name NVARCHAR(250) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Barcode VARCHAR(50) NOT NULL, -- الباركود الخاص بالصيدلية
    InternationalCode VARCHAR(50) NULL,  -- الكود الدولي إن وجد
    StockProductCode VARCHAR(50) NULL, -- الكود في نظام stock
    Price DECIMAL(18,2) NOT NULL,
    OldPrice DECIMAL(18,2) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1, -- InStock
    IsIntegrated BIT NOT NULL DEFAULT 0,
    IntegratedAt DATETIME NULL,
    CategoryId INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CreatedBy NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,

	CONSTRAINT FK_Products_CategoryId FOREIGN KEY (SubCategoryId) REFERENCES Sub2categories(Id)
);

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

CREATE TABLE Tags (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE ProductTags (
    ProductId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (ProductId, TagId),
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (TagId) REFERENCES Tags(Id)
);

CREATE TABLE SalesHeader (
    Id int PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) NOT NULL,
    CustomerId INT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Discount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    NetAmount AS (TotalAmount - Discount) PERSISTED,
    PaymentMethod NVARCHAR(50) NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', --Pending, Processing, Completed, Returned, PartiallyReturned, Cancelled
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_SalesHeader_Customers FOREIGN KEY (CustomerId)
        REFERENCES Customers(Id)
);

CREATE TABLE SalesDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18, 2) NOT NULL,
    Discount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TotalPrice AS ((UnitPrice * Quantity) - Discount) PERSISTED,
    Notes NVARCHAR(MAX) NULL,

    CONSTRAINT FK_SalesDetails_SalesHeader FOREIGN KEY (SalesHeaderId)
        REFERENCES SalesHeader(Id) ON DELETE CASCADE,
        
    CONSTRAINT FK_SalesDetails_Products FOREIGN KEY (ProductId)
        REFERENCES Products(Id)
);

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

    CONSTRAINT FK_SalesHeaderReturns_OriginalSalesHeader 
        FOREIGN KEY (OriginalSalesHeaderId) 
        REFERENCES SalesHeader(Id),

    -- إذا كنت تستخدم جداول Customers أو Pharmacies:
     CONSTRAINT FK_SalesHeaderReturns_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
     CONSTRAINT FK_SalesHeaderReturns_Pharmacies FOREIGN KEY (PharmacyId) REFERENCES Pharmacies(Id)
);

CREATE TABLE SalesDetailsReturns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderReturnId INT NOT NULL,         
    ProductId INT NOT NULL,                   
    Quantity DECIMAL(18,2) NOT NULL,          
    UnitPrice DECIMAL(18,2) NOT NULL,         
    TotalPrice AS (Quantity * UnitPrice) PERSISTED,  

    Reason NVARCHAR(500) NULL,                
    
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),  

    CONSTRAINT FK_SalesDetailsReturns_Header 
        FOREIGN KEY (SalesHeaderReturnId) 
        REFERENCES SalesHeaderReturns(Id),

    CONSTRAINT FK_SalesDetailsReturns_Product 
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
CREATE TABLE Prescriptions(
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderId INT NULL,  -- يمكن أن تكون الروشتة محفوظة بدون طلب بعد
    ImageUrl NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(500) NULL,

    CONSTRAINT FK_Prescriptions_SalesHeader 
        FOREIGN KEY (SalesHeaderId)
        REFERENCES SalesHeader(Id)
);

CREATE TABLE ProductsOffer (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DiscountPercentage DECIMAL(5,2) NOT NULL,  -- مثلاً 10.00 تعني خصم 10%
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,

    CONSTRAINT FK_ProductsOffer_Product 
        FOREIGN KEY (ProductId) 
        REFERENCES Products(Id)
);

CREATE TABLE CustomerPointsHistory (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
	RelatedOrderId INT NULL,
    Points INT NOT NULL,
    Reason NVARCHAR(500) NULL,
    IsAddition BIT NOT NULL,  -- true لإضافة نقاط، false لخصم
	Note NVARCHAR(500) NULL,
	ExpiresAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_CustomerPointsHistory_Customer 
        FOREIGN KEY (CustomerId) 
        REFERENCES Customers(Id)
);

CREATE TABLE Reviews (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    ProductId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(1000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Reviews_Customer 
        FOREIGN KEY (CustomerId) 
        REFERENCES Customers(Id),

    CONSTRAINT FK_Reviews_Product 
        FOREIGN KEY (ProductId) 
        REFERENCES Products(Id)
);
CREATE TABLE Chats (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Chats_Customer 
        FOREIGN KEY (CustomerId) 
        REFERENCES Customers(Id)
);
CREATE TABLE Messages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ChatId INT NOT NULL,
    SenderType NVARCHAR(50) NOT NULL,  -- Admin / Customer
    MessageText NVARCHAR(2000) NOT NULL,
    SentAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Messages_Chat 
        FOREIGN KEY (ChatId) 
        REFERENCES Chats(Id)
);
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Body NVARCHAR(1000) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    SentAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Notifications_Customer 
        FOREIGN KEY (CustomerId) 
        REFERENCES Customers(Id)
);
CREATE TABLE Admins (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PharmacyId INT NOT NULL, -- تشير إلى الصيدلية المالكة (مستقبلاً عند التوسع)
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(100) NOT NULL, -- Admin, Support, Viewer
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AdminId INT NULL,
    Action NVARCHAR(500) NOT NULL,
    TableName NVARCHAR(100) NULL,
    RecordId INT NULL,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(1000) NULL,

    CONSTRAINT FK_AuditLogs_Admin 
        FOREIGN KEY (AdminId) 
        REFERENCES Admins(Id)
);






