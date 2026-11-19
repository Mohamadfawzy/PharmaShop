
USE pharma_shope_db;


CREATE TABLE Pharmacies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
	NameEn NVARCHAR(200) NOT NULL,
    OwnerName NVARCHAR(150) NULL,               
    LicenseNumber NVARCHAR(100) NULL,           
    PhoneNumber NVARCHAR(20) NULL,              
    Email NVARCHAR(150) NULL,                   
    Address NVARCHAR(300) NULL,                 
	Latitude DECIMAL(10,7) NULL,               
    Longitude DECIMAL(10,7) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), 
    IsActive BIT NOT NULL DEFAULT 1            
);
GO

--=======================================
-- Personses
--=======================================

CREATE TABLE [dbo].[Customers] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] int NULL,

    [FullNameAr] nvarchar(200) NOT NULL,
    [FullNameEn] nvarchar(200) NOT NULL,

    [PhoneNumber] nvarchar(20) NULL,
    [Gender] nvarchar(10) NULL,
    [DateOfBirth] date NULL,

    [Email] nvarchar(150) NULL,
    [NationalId] nvarchar(20) NULL,

    [CustomerType] nvarchar(50) NOT NULL CONSTRAINT [DF_Customers_CustomerType] DEFAULT (N'Regular'), -- Regular/VIP/Corporate

    [Points] int NOT NULL CONSTRAINT [DF_Customers_Points] DEFAULT ((0)),
    [PointsExpiryDate] datetime2(0) NULL,

    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Customers_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Customers_IsActive] DEFAULT ((1)),

    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Customers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


-- indexes

-- Enforce one customer per AspNetUser (ignore NULLs)  
CREATE UNIQUE INDEX [UX_Customers_UserId]
ON [dbo].[Customers] ([UserId])
WHERE [UserId] IS NOT NULL;
GO

-- Phone lookup (useful for login/OTP or search)  
CREATE INDEX [IX_Customers_PhoneNumber]
ON [dbo].[Customers] ([PhoneNumber])
WHERE [PhoneNumber] IS NOT NULL;
GO

-- Email lookup (optional search/login)  
CREATE INDEX [IX_Customers_Email]
ON [dbo].[Customers] ([Email])
WHERE [Email] IS NOT NULL;

GO

--.......................................
-- CustomerAddresses
--.......................................

CREATE TABLE [dbo].[CustomerAddresses] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [GovernorateAreaId] int NOT NULL,
    [Title] nvarchar(100) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Region] nvarchar(100) NULL,
    [Street] nvarchar(300) NOT NULL,
    [Landmark] nvarchar(200) NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [IsDefault] bit NOT NULL CONSTRAINT [DF_CustomerAddresses_IsDefault] DEFAULT ((0)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerAddresses_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL,
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerAddresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION
);
GO


-- Fast lookup: all addresses for a customer  
CREATE INDEX [IX_CustomerAddresses_CustomerId]
ON [dbo].[CustomerAddresses] ([CustomerId]);
GO

-- Fast lookup: default address for a customer  
CREATE INDEX [IX_CustomerAddresses_CustomerId_IsDefault]
ON [dbo].[CustomerAddresses] ([CustomerId], [IsDefault]);
GO

CREATE INDEX [IX_CustomerAddresses_GovernorateAreaId]
ON [dbo].[CustomerAddresses] ([GovernorateAreaId]);
GO

CREATE INDEX [IX_CustomerAddresses_CustomerId_DeletedAt_CreatedAt]
ON [dbo].[CustomerAddresses] ([CustomerId], [DeletedAt], [CreatedAt]);
GO

-- Enforce a single default address per customer  
CREATE UNIQUE INDEX [UX_CustomerAddresses_CustomerId_Default]
ON [dbo].[CustomerAddresses] ([CustomerId])
WHERE [IsDefault] = (1) AND [DeletedAt] IS NULL;
GO
GO

--=======================================
-- Governorates and their areas
--=======================================

CREATE TABLE [dbo].[Governorates] (
    [Id] int NOT NULL IDENTITY(1,1),
    [NameAr] nvarchar(100) NOT NULL,
    [NameEn] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Governorates_IsActive] DEFAULT ((1)),
    [DisplayOrder] int NOT NULL CONSTRAINT [DF_Governorates_DisplayOrder] DEFAULT ((0)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Governorates_CreatedAt] DEFAULT (sysdatetime()),
    CONSTRAINT [PK_Governorates] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [UX_Governorates_NameAr]
ON [dbo].[Governorates] ([NameAr]);
GO

CREATE TABLE [dbo].[GovernorateAreas] (
    [Id] int NOT NULL IDENTITY(1,1),
    [GovernorateId] int NOT NULL,
    [NameAr] nvarchar(100) NOT NULL,
    [NameEn] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_GovernorateAreas_IsActive] DEFAULT ((1)),
    [DisplayOrder] int NOT NULL CONSTRAINT [DF_GovernorateAreas_DisplayOrder] DEFAULT ((0)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_GovernorateAreas_CreatedAt] DEFAULT (sysdatetime()),
    CONSTRAINT [PK_GovernorateAreas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GovernorateAreas_Governorates_GovernorateId]
        FOREIGN KEY ([GovernorateId]) REFERENCES [dbo].[Governorates] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [UX_GovernorateAreas_GovernorateId_NameAr]
ON [dbo].[GovernorateAreas] ([GovernorateId], [NameAr]);
GO

CREATE INDEX [IX_GovernorateAreas_GovernorateId_IsActive_DisplayOrder]
ON [dbo].[GovernorateAreas] ([GovernorateId], [IsActive], [DisplayOrder]);
GO

ALTER TABLE [dbo].[CustomerAddresses]
ADD CONSTRAINT [FK_CustomerAddresses_GovernorateAreas_GovernorateAreaId]
    FOREIGN KEY ([GovernorateAreaId]) REFERENCES [dbo].[GovernorateAreas] ([Id]) ON DELETE NO ACTION;
GO

-- =======================================
-- Employees
-- =======================================

CREATE TABLE [dbo].[Employees] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] int NULL,
    [PharmacyId] int NOT NULL,

    [FullNameAr] nvarchar(200) NOT NULL,
    [FullNameEn] nvarchar(200) NULL,

    [PhoneNumber] nvarchar(20) NULL,
    [Email] nvarchar(150) NULL,
    [NationalId] nvarchar(20) NULL,

    [JobTitle] nvarchar(100) NULL,
    [EmployeeCode] nvarchar(50) NULL,

    [IsActive] bit NOT NULL CONSTRAINT [DF_Employees_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Employees_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete

    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Employees_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Employees_Pharmacies_PharmacyId] FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);
GO



-- Enforce one employee per AspNetUser (ignore NULLs)  
CREATE UNIQUE INDEX [UX_Employees_UserId]
ON [dbo].[Employees] ([UserId])
WHERE [UserId] IS NOT NULL AND [DeletedAt] IS NULL;
GO

-- Fast filtering by pharmacy (tenant)  
CREATE INDEX [IX_Employees_PharmacyId]
ON [dbo].[Employees] ([PharmacyId])
INCLUDE ([FullNameAr], [PhoneNumber], [IsActive])
WHERE [DeletedAt] IS NULL;
GO

-- Phone lookup (useful for search/login/OTP)  
CREATE INDEX [IX_Employees_PhoneNumber]
ON [dbo].[Employees] ([PhoneNumber])
WHERE [PhoneNumber] IS NOT NULL AND [DeletedAt] IS NULL;
GO

-- Email lookup (optional search/login)  
CREATE INDEX [IX_Employees_Email]
ON [dbo].[Employees] ([Email])
WHERE [Email] IS NOT NULL AND [DeletedAt] IS NULL;
GO

-- Employee code lookup (optional)  
CREATE INDEX [IX_Employees_EmployeeCode]
ON [dbo].[Employees] ([EmployeeCode])
WHERE [EmployeeCode] IS NOT NULL AND [DeletedAt] IS NULL;

GO

