
-- ==============================
-- جدول الأصناف
-- ==============================
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1), -- المعرف الفريد للصنف
    Name NVARCHAR(200) NOT NULL, -- اسم الصنف
    Description NVARCHAR(500) NULL -- وصف إضافي للصنف
);

-- ==============================
-- جدول المنتجات
-- ==============================
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1), -- المعرف الفريد للمنتج
    Name NVARCHAR(200) NOT NULL, -- اسم المنتج
    Description NVARCHAR(1000) NULL, -- وصف المنتج
    Barcode NVARCHAR(100) NOT NULL, -- الباركود الفريد للمنتج
    Price DECIMAL(18,2) NOT NULL, -- السعر
    Quantity INT NOT NULL, -- الكمية المتوفرة
    CategoryId INT NOT NULL, -- معرف الصنف
    ImageUrl NVARCHAR(500) NULL, -- رابط الصورة الرئيسية (لأغراض التوافق)
    IsActive BIT NOT NULL DEFAULT 1, -- حالة تفعيل المنتج
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), -- تاريخ الإضافة

    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- ==============================
-- جدول صور المنتجات
-- ==============================
CREATE TABLE ProductImages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL, -- معرف المنتج
    ImageUrl NVARCHAR(500) NOT NULL, -- رابط الصورة
    IsMain BIT NOT NULL DEFAULT 0, -- هل هي الصورة الرئيسية؟

    CONSTRAINT FK_ProductImages_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- ==============================
-- جدول ربط المنتجات بالأصناف المتعددة (إن وجد)ـ
-- ==============================
CREATE TABLE ProductCategories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    CategoryId INT NOT NULL,

    CONSTRAINT FK_ProductCategories_Product FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_ProductCategories_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- ==============================
-- جدول العملاء
-- ==============================
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Points INT NOT NULL DEFAULT 0, -- نقاط الولاء
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- ==============================
-- جدول عناوين العملاء
-- ==============================
CREATE TABLE CustomerAddresses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    Title NVARCHAR(100) NOT NULL, -- مثل "المنزل"، "العمل"
    AddressDetails NVARCHAR(500) NOT NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,

    CONSTRAINT FK_CustomerAddresses_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- ==============================
-- جدول فواتير المبيعات
-- ==============================
CREATE TABLE SalesHeader (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- حالة الطلب: مكتمل، معلق، ملغي
    HasReturns BIT NOT NULL DEFAULT 0, -- هل تحتوي الفاتورة على مرتجعات؟

    CONSTRAINT FK_SalesHeader_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- ==============================
-- تفاصيل فواتير المبيعات
-- ==============================
CREATE TABLE SalesDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    IsReturned BIT NOT NULL DEFAULT 0, -- هل تم إرجاع هذا البند

    CONSTRAINT FK_SalesDetails_SalesHeader FOREIGN KEY (SalesHeaderId) REFERENCES SalesHeader(Id),
    CONSTRAINT FK_SalesDetails_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- ==============================
-- مرتجعات الفواتير
-- ==============================
CREATE TABLE SalesHeaderReturns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderId INT NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalReturnAmount DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_SalesHeaderReturns_SalesHeader FOREIGN KEY (SalesHeaderId) REFERENCES SalesHeader(Id)
);

-- ==============================
-- تفاصيل المرتجعات
-- ==============================
CREATE TABLE SalesDetailsReturns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SalesHeaderReturnsId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    RefundAmount DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_SalesDetailsReturns_SalesHeaderReturns FOREIGN KEY (SalesHeaderReturnsId) REFERENCES SalesHeaderReturns(Id),
    CONSTRAINT FK_SalesDetailsReturns_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
