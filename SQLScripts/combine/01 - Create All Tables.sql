
-- ================================================================
-- Combined source: 1- identity.sql
-- ================================================================
 USE master;
 IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = 'pharma_shope_db')
 BEGIN
     CREATE DATABASE pharma_shope_db COLLATE Arabic_100_CI_AS_KS_WS_SC_UTF8;
 END
 GO

USE pharma_shope_db;

Go

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(200) NULL,
    [Description] nvarchar(500) NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] int NOT NULL IDENTITY,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(50) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX PhoneNumberIndex ON AspNetUsers([PhoneNumber]) WHERE PhoneNumber IS NOT NULL;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251224210329_InitIdentity', N'9.0.11');

COMMIT;
GO

-- ===================================================================
-- Insert Rolse
-- ===================================================================
USE pharma_shope_db;
GO

SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    RAISERROR('AspNetRoles table not found. Apply Identity migrations first.', 16, 1);
    RETURN;
END
GO

MERGE dbo.AspNetRoles AS target
USING (VALUES
    (N'Admin',         N'ADMIN',         N'Admin',         N'Full access'),
    (N'Support',       N'SUPPORT',       N'Support',       N'Support staff'),
    (N'Viewer',        N'VIEWER',        N'Viewer',        N'Read-only access'),
    (N'Customer',      N'CUSTOMER',      N'Customer',      N'Customer account'),
    (N'Employee',      N'EMPLOYEE',      N'Employee',      N'Employee account'),
    (N'DeliveryAgent', N'DELIVERYAGENT', N'Delivery Agent',N'Delivery agent account')
) AS source ([Name], [NormalizedName], [NameEn], [Description])
ON target.NormalizedName = source.NormalizedName
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Name], [NormalizedName], [ConcurrencyStamp], [NameEn], [Description])
    VALUES (source.[Name], source.[NormalizedName], NEWID(), source.[NameEn], source.[Description]);
GO

-- Move legacy pharmacist accounts to Employee with prescription-review permission.
DECLARE @PharmacistRoleId int = (SELECT [Id] FROM dbo.AspNetRoles WHERE [NormalizedName] = N'PHARMACIST');
DECLARE @EmployeeRoleId int = (SELECT [Id] FROM dbo.AspNetRoles WHERE [NormalizedName] = N'EMPLOYEE');

IF @PharmacistRoleId IS NOT NULL AND @EmployeeRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.AspNetUserRoles ([UserId], [RoleId])
    SELECT oldRole.[UserId], @EmployeeRoleId
    FROM dbo.AspNetUserRoles AS oldRole
    WHERE oldRole.[RoleId] = @PharmacistRoleId
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.AspNetUserRoles AS employeeRole
          WHERE employeeRole.[UserId] = oldRole.[UserId]
            AND employeeRole.[RoleId] = @EmployeeRoleId
      );

    INSERT INTO dbo.AspNetUserClaims ([UserId], [ClaimType], [ClaimValue])
    SELECT oldRole.[UserId], N'permission', N'CanReviewPrescriptions'
    FROM dbo.AspNetUserRoles AS oldRole
    WHERE oldRole.[RoleId] = @PharmacistRoleId
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.AspNetUserClaims AS permissionClaim
          WHERE permissionClaim.[UserId] = oldRole.[UserId]
            AND permissionClaim.[ClaimType] = N'permission'
            AND permissionClaim.[ClaimValue] = N'CanReviewPrescriptions'
      );

    DELETE FROM dbo.AspNetUserRoles WHERE [RoleId] = @PharmacistRoleId;
    DELETE FROM dbo.AspNetRoles WHERE [Id] = @PharmacistRoleId;
END
GO


-- ================================================================
-- Combined source: 2-Personses .sql
-- ================================================================

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



-- ================================================================
-- Combined source: 3- PharmaShopScript.sql
-- ================================================================
use  pharma_shope_db; 
Go

-- =======================================
-- Categories
-- =======================================

CREATE TABLE [dbo].[Categories] (
    [Id] int NOT NULL IDENTITY(1,1),
    [ParentCategoryId] int NULL,
    [NameAr] nvarchar(200) NOT NULL,
    [NameEn] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [ImageId] nvarchar(128) NULL,
    [ImageFormat] tinyint NULL,
    [ImageVersion] int NOT NULL CONSTRAINT [DF_Categories_ImageVersion] DEFAULT ((0)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Categories_IsActive] DEFAULT ((1)),
    [IsDeleted] bit NOT NULL CONSTRAINT [DF_Categories_IsDeleted] DEFAULT ((0)),
    [CreatedAt] datetime NOT NULL CONSTRAINT [DF_Categories_CreatedAt] DEFAULT (getdate()),
    [UpdatedAt] datetime NULL,
    [ImageUpdatedAt] datetime NULL,

    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Categories_ImageFormat] CHECK ([ImageFormat] IS NULL OR [ImageFormat] IN ((1), (2), (3))),
    CONSTRAINT [FK_Categories_ParentCategory] FOREIGN KEY ([ParentCategoryId]) REFERENCES [dbo].[Categories] ([Id]) ON DELETE NO ACTION
);
GO

-- Tree traversal support (parent-child queries)  
CREATE INDEX [IX_Categories_ParentCategoryId]
ON [dbo].[Categories] ([ParentCategoryId]);
GO

-- Optional: ImageId lookup (filtered)  
CREATE INDEX [IX_Categories_ImageId]
ON [dbo].[Categories] ([ImageId])
WHERE [ImageId] IS NOT NULL;
GO


-- ========================================================
-- Units 
-- ========================================================

GO
CREATE TABLE [dbo].[Units] (
    [Id] int NOT NULL IDENTITY(1,1),
    [Code] varchar(30) NOT NULL, -- Stable code (e.g., BOX, STRIP)
    [NameAr] nvarchar(60) NOT NULL,
    [NameEn] nvarchar(60) NOT NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Units_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Units_CreatedAt] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL, -- Concurrency
    CONSTRAINT [PK_Units] PRIMARY KEY ([Id])
);
GO

-- Unique stable unit code  
CREATE UNIQUE INDEX [UX_Units_Code]
ON [dbo].[Units] ([Code]);
GO

-- Optional: unique English name  
CREATE UNIQUE INDEX [UX_Units_NameEn]
ON [dbo].[Units] ([NameEn]);
GO

-- Optional seed data
INSERT INTO dbo.Units (Code, NameAr, NameEn)
VALUES
('BOX',     N'علبة',  N'Box'),
('STRIP',   N'شريط',  N'Strip'),
('AMPOULE', N'أمبول', N'Ampoule'),
('BOTTLE',  N'زجاجة', N'Bottle'),
('TUBE',    N'أنبوبة',N'Tube'),
('TABLET',  N'قرص',   N'Tablet'),
('CAPSULE', N'كبسولة',N'Capsule');
GO


-- ========================================================
-- Companies
-- ======================================================== 

CREATE TABLE [dbo].[Companies] (
    [Id] int NOT NULL IDENTITY(1,1),
    [NameAr] nvarchar(150) NOT NULL,
    [NameEn] nvarchar(150) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Companies_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Companies_CreatedAt] DEFAULT (SYSDATETIME()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
);
GO

 -- Unique Arabic name (ignoring soft-deleted rows)  
CREATE UNIQUE INDEX [UX_Companies_NameAr]
ON [dbo].[Companies] ([NameAr])
WHERE [DeletedAt] IS NULL;
GO

 -- Index for English name search/sort (ignoring soft-deleted rows)  
CREATE INDEX [IX_Companies_NameEn]
ON [dbo].[Companies] ([NameEn])
WHERE [DeletedAt] IS NULL AND [NameEn] IS NOT NULL;
GO

-- Index for active listing with covered columns (ignoring soft-deleted rows)  
CREATE INDEX [IX_Companies_IsActive]
ON [dbo].[Companies] ([IsActive])
INCLUDE ([NameAr], [NameEn], [CreatedAt])
WHERE [DeletedAt] IS NULL;
GO

-- ===========================================================
-- Stores
-- =========================================================== 
CREATE TABLE [dbo].[Stores] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PharmacyId] int NOT NULL,
    [NameAr] nvarchar(150) NOT NULL,
    [NameEn] nvarchar(150) NOT NULL,
    [Code] varchar(30) NULL,
    [Address] nvarchar(300) NULL,
    [IsDefault] bit NOT NULL CONSTRAINT [DF_Stores_IsDefault] DEFAULT ((0)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Stores_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Stores_CreatedAt] DEFAULT (sysutcdatetime()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,
    [RowVersion] rowversion NOT NULL, -- Concurrency
    CONSTRAINT [PK_Stores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Stores_Pharmacies_PharmacyId]
        FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);
GO

/* Unique store code per pharmacy (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_Stores_PharmacyId_Code]
ON [dbo].[Stores] ([PharmacyId], [Code])
WHERE [Code] IS NOT NULL AND [Code] <> '' AND [DeletedAt] IS NULL;
GO

/* Fast listing by pharmacy and active flag (ignoring soft-deleted rows) */
CREATE INDEX [IX_Stores_PharmacyId_IsActive]
ON [dbo].[Stores] ([PharmacyId], [IsActive])
INCLUDE ([NameAr], [NameEn], [IsDefault], [Code])
WHERE [DeletedAt] IS NULL;
GO

/* Enforce a single default store per pharmacy (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_Stores_PharmacyId_Default]
ON [dbo].[Stores] ([PharmacyId])
WHERE [IsDefault] = (1) AND [DeletedAt] IS NULL;
GO

/* Name search (Arabic) within pharmacy (ignoring soft-deleted rows) */
CREATE INDEX [IX_Stores_PharmacyId_NameAr]
ON [dbo].[Stores] ([PharmacyId], [NameAr])
WHERE [DeletedAt] IS NULL;
GO

/* Name search (English) within pharmacy (ignoring soft-deleted rows) */
CREATE INDEX [IX_Stores_PharmacyId_NameEn]
ON [dbo].[Stores] ([PharmacyId], [NameEn])
WHERE [DeletedAt] IS NULL;
GO

-- ===========================================================
-- Store-specific delivery zones and covered governorate areas
-- ===========================================================
CREATE TABLE [dbo].[DeliveryZones] (
    [Id] int NOT NULL IDENTITY(1,1),
    [StoreId] int NOT NULL,
    [NameAr] nvarchar(150) NOT NULL,
    [NameEn] nvarchar(150) NOT NULL,
    [DeliveryFee] decimal(18,2) NOT NULL,
    [FreeDeliveryMinimumAmount] decimal(18,2) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_DeliveryZones_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_DeliveryZones_CreatedAt] DEFAULT (sysutcdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    CONSTRAINT [PK_DeliveryZones] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_DeliveryZones_Id_StoreId] UNIQUE ([Id], [StoreId]),
    CONSTRAINT [FK_DeliveryZones_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CK_DeliveryZones_DeliveryFee_NonNegative] CHECK ([DeliveryFee] >= 0),
    CONSTRAINT [CK_DeliveryZones_FreeDeliveryMinimumAmount_NonNegative]
        CHECK ([FreeDeliveryMinimumAmount] IS NULL OR [FreeDeliveryMinimumAmount] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_DeliveryZones_StoreId_NameAr]
ON [dbo].[DeliveryZones] ([StoreId], [NameAr]);
GO

CREATE INDEX [IX_DeliveryZones_StoreId_IsActive]
ON [dbo].[DeliveryZones] ([StoreId], [IsActive]);
GO

CREATE TABLE [dbo].[DeliveryZoneAreas] (
    [Id] int NOT NULL IDENTITY(1,1),
    [DeliveryZoneId] int NOT NULL,
    [StoreId] int NOT NULL,
    [GovernorateAreaId] int NOT NULL,
    CONSTRAINT [PK_DeliveryZoneAreas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeliveryZoneAreas_DeliveryZones_ZoneId_StoreId]
        FOREIGN KEY ([DeliveryZoneId], [StoreId])
        REFERENCES [dbo].[DeliveryZones] ([Id], [StoreId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DeliveryZoneAreas_GovernorateAreas_AreaId]
        FOREIGN KEY ([GovernorateAreaId]) REFERENCES [dbo].[GovernorateAreas] ([Id]) ON DELETE NO ACTION
);
GO

-- A governorate area can belong to only one zone per store.
CREATE UNIQUE INDEX [UX_DeliveryZoneAreas_StoreId_GovernorateAreaId]
ON [dbo].[DeliveryZoneAreas] ([StoreId], [GovernorateAreaId]);
GO

CREATE INDEX [IX_DeliveryZoneAreas_GovernorateAreaId]
ON [dbo].[DeliveryZoneAreas] ([GovernorateAreaId]);
GO

-- ===========================================================
-- Products
-- =========================================================== 

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

CREATE TABLE [dbo].[Products] (
    [Id] int NOT NULL IDENTITY(1,1),
    [StoreId] int NOT NULL,
    [CategoryId] int NOT NULL,
    [CompanyId] int NULL,

    [ErpProductId] decimal(18,0) NULL,
    [ErpStoreId] int  NULL,
    [InternationalCode] varchar(50) NULL,

    [NameAr] nvarchar(250) NOT NULL,
    [NameEn] nvarchar(250) NULL,
    [DescriptionAr] nvarchar(max) NULL,
    [DescriptionEn] nvarchar(max) NULL,
    [SearchKeywords] nvarchar(500) NULL,

    [OuterUnitId] int NOT NULL,
    [InnerUnitId] int NULL,
    [InnerPerOuter] int NULL,

    [OuterUnitPrice] decimal(18,2) NOT NULL CONSTRAINT [DF_Products_OuterUnitPrice] DEFAULT ((0)),
    [InnerUnitPrice] decimal(18,2) NULL CONSTRAINT [DF_Products_InnerUnitPrice] DEFAULT ((0)),

    [MinOrderQty] int NOT NULL CONSTRAINT [DF_Products_MinOrderQty] DEFAULT ((1)),
    [MaxOrderQty] int NULL,
    [MaxPerDayQty] int NULL,


    [IsRedeemableByPoints] bit NOT NULL CONSTRAINT [DF_Products_IsRedeemableByPoints] DEFAULT ((0)),
    [IsReturnable] bit NOT NULL CONSTRAINT [DF_Products_IsReturnable] DEFAULT ((1)),
    [AllowSplitSale] bit NOT NULL CONSTRAINT [DF_Products_AllowSplitSale] DEFAULT ((0)),

    [Quantity] decimal(18,3) NOT NULL CONSTRAINT [DF_Products_Quantity] DEFAULT ((0)), -- Stored in OUTER units (e.g., 2.500)
    [HasExpiry] bit NOT NULL CONSTRAINT [DF_Products_HasExpiry] DEFAULT ((1)),
    [NearestExpiryDate] date NULL,
    [LastStockSyncAt] datetime2(0) NULL,

    [HasPromotion] bit NOT NULL CONSTRAINT [DF_Products_HasPromotion] DEFAULT ((0)),
    [PromotionDiscountPercent] decimal(5,2) NOT NULL CONSTRAINT [DF_Products_PromotionDiscountPercent] DEFAULT ((0)),
    [PromotionStartsAt] datetime2(0) NULL,
    [PromotionEndsAt] datetime2(0) NULL,

    [IsFeatured] bit NOT NULL CONSTRAINT [DF_Products_IsFeatured] DEFAULT ((0)),

    [IsIntegrated] bit NOT NULL CONSTRAINT [DF_Products_IsIntegrated] DEFAULT ((0)),
    [IntegratedAt] datetime2(0) NULL,

    [Points] int NOT NULL CONSTRAINT [DF_Products_PointsOuter] DEFAULT ((0)),

    [RequiresPrescription] bit NOT NULL CONSTRAINT [DF_Products_RequiresPrescription] DEFAULT ((0)),
    [IsAvailable] bit NOT NULL CONSTRAINT [DF_Products_IsAvailable] DEFAULT ((0)),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT ((1)),

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete

    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_Products_QuantityNonNegative]
        CHECK ([Quantity] >= (0)),

    CONSTRAINT [CK_Products_PricesNonNegative]
        CHECK ([OuterUnitPrice] >= (0) AND ([InnerUnitPrice] IS NULL OR [InnerUnitPrice] >= (0))),

    CONSTRAINT [CK_Products_PromotionPercent]
        CHECK ([PromotionDiscountPercent] >= (0) AND [PromotionDiscountPercent] <= (100)),

    CONSTRAINT [CK_Products_PromotionDates]
        CHECK (
            ([PromotionStartsAt] IS NULL AND [PromotionEndsAt] IS NULL)
            OR ([PromotionStartsAt] IS NOT NULL AND [PromotionEndsAt] IS NOT NULL AND [PromotionEndsAt] > [PromotionStartsAt])
        ),

    CONSTRAINT [CK_Products_OrderLimits]
        CHECK (
            [MinOrderQty] > (0)
            AND ([MaxOrderQty] IS NULL OR [MaxOrderQty] >= [MinOrderQty])
            AND ([MaxPerDayQty] IS NULL OR [MaxPerDayQty] > (0))
        ),

    CONSTRAINT [CK_Products_InnerRules]
        CHECK (
            ([InnerUnitId] IS NULL AND [InnerPerOuter] IS NULL)
            OR ([InnerUnitId] IS NOT NULL AND [InnerPerOuter] IS NOT NULL AND [InnerPerOuter] >= (1))
        )
);
GO



 -- -------------------- Foreign Keys --------------------  
ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Stores
    FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Categories
    FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Companies
    FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Units_Outer
    FOREIGN KEY (OuterUnitId) REFERENCES dbo.Units(Id);

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_Units_Inner
    FOREIGN KEY (InnerUnitId) REFERENCES dbo.Units(Id);
GO

 -- -------------------- Indexes --------------------  

-- Unique InternationalCode per Store (filtered: allow NULL, ignore soft-deleted)
CREATE UNIQUE INDEX UX_Products_Store_InternationalCode
ON dbo.Products(StoreId, InternationalCode)
WHERE InternationalCode IS NOT NULL AND InternationalCode <> '' AND DeletedAt IS NULL;
GO

-- Fast listing for store/category/active/available
CREATE INDEX IX_Products_List
ON dbo.Products(StoreId, CategoryId, IsActive, IsAvailable)
INCLUDE (NameAr, NameEn, OuterUnitPrice, InnerUnitPrice, Quantity, HasPromotion, PromotionDiscountPercent)
WHERE DeletedAt IS NULL;
GO

-- Search by Arabic name
CREATE INDEX IX_Products_NameAr
ON dbo.Products(StoreId, NameAr)
WHERE DeletedAt IS NULL;
GO

-- Search by English name (if present)
CREATE INDEX IX_Products_NameEn
ON dbo.Products(StoreId, NameEn)
WHERE DeletedAt IS NULL AND NameEn IS NOT NULL;
GO

-- Offers browsing
CREATE INDEX IX_Products_Promotions
ON dbo.Products(StoreId, HasPromotion, PromotionEndsAt)
INCLUDE (NameAr, OuterUnitPrice, InnerUnitPrice, PromotionDiscountPercent)
WHERE DeletedAt IS NULL AND HasPromotion = 1;
GO

-- ERP mapping index (useful for sync)
CREATE INDEX IX_Products_Store_ErpProductId
ON dbo.Products(StoreId, ErpProductId)
WHERE DeletedAt IS NULL AND ErpProductId IS NOT NULL;
GO

-- ========================================================
-- ProductImages
-- ========================================================

CREATE TABLE [dbo].[ProductImages] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [PharmacyId] int NOT NULL,
    [ProductId] int NOT NULL,

    [ImageUrl] nvarchar(600) NOT NULL,
    [ThumbnailUrl] nvarchar(600) NULL,
    [AltText] nvarchar(200) NULL,

    [SortOrder] int NOT NULL CONSTRAINT [DF_ProductImages_SortOrder] DEFAULT ((0)),
    [IsPrimary] bit NOT NULL CONSTRAINT [DF_ProductImages_IsPrimary] DEFAULT ((0)),

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ProductImages_CreatedAt] DEFAULT (sysdatetime()),
    [CreatedBy] nvarchar(100) NULL,

    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [DeletedBy] nvarchar(100) NULL,

    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_ProductImages_SortOrder]
        CHECK ([SortOrder] >= (0)),

    CONSTRAINT [FK_ProductImages_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_ProductImages_Pharmacies_PharmacyId]
        FOREIGN KEY ([PharmacyId]) REFERENCES [dbo].[Pharmacies] ([Id]) ON DELETE NO ACTION
);
GO


-- Fast lookup by tenant and product (ignoring soft-deleted rows) 
CREATE INDEX [IX_ProductImages_PharmacyId_ProductId]
ON [dbo].[ProductImages] ([PharmacyId], [ProductId])
WHERE [DeletedAt] IS NULL;
GO

-- Efficient ordering: primary first, then sort order (ignoring soft-deleted rows) 
CREATE INDEX [IX_ProductImages_ProductId_Sort]
ON [dbo].[ProductImages] ([ProductId], [IsPrimary] DESC, [SortOrder], [Id])
INCLUDE ([ImageUrl], [ThumbnailUrl], [AltText])
WHERE [DeletedAt] IS NULL;
GO

-- Enforce a single primary image per product (ignoring soft-deleted rows) 
CREATE UNIQUE INDEX [UX_ProductImages_ProductId_Primary]
ON [dbo].[ProductImages] ([ProductId], [IsPrimary])
WHERE [IsPrimary] = (1) AND [DeletedAt] IS NULL;
GO

-- Simple FK helper index for ORMs/scaffolding (ignoring soft-deleted rows) 
CREATE INDEX [IX_ProductImages_ProductId]
ON [dbo].[ProductImages] ([ProductId])
WHERE [DeletedAt] IS NULL;
GO


-- ================================================================
-- Combined source: 4- Promotions.sql
-- ================================================================

use pharma_shope_db;
go

CREATE TABLE [dbo].[Promotions] (
    [Id] int NOT NULL IDENTITY(1,1),
    [ErpPgoId] decimal(18,0) NULL, -- ERP group offer id (pgo_id)
    [Name] nvarchar(200) NULL,
    [Notes] nvarchar(250) NULL,
    [StartAt] datetime2(0) NULL,
    [EndAt] datetime2(0) NULL,
    [TotalAmount] int NULL,
    [BasicAmount] int NULL,
    [OfferAmount] int NULL,
    [DiscountPercent] decimal(5,2) NULL, -- ERP offer_amount_disc (assumed percent)
    [IsActive] bit NOT NULL CONSTRAINT [DF_Promotions_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Promotions_CreatedAt] DEFAULT (sysutcdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    [RowVersion] rowversion NOT NULL, -- Concurrency
    CONSTRAINT [PK_Promotions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Promotions_DiscountPercent] CHECK ([DiscountPercent] IS NULL OR ([DiscountPercent] >= (0) AND [DiscountPercent] <= (100))),
    CONSTRAINT [CK_Promotions_Dates] CHECK (([StartAt] IS NULL AND [EndAt] IS NULL) OR ([StartAt] IS NOT NULL AND [EndAt] IS NOT NULL AND [EndAt] > [StartAt]))
);
GO

/* Unique ERP group offer id (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_Promotions_ErpPgoId]
ON [dbo].[Promotions] ([ErpPgoId])
WHERE [ErpPgoId] IS NOT NULL AND [DeletedAt] IS NULL;
GO

/* Fast listing for active promotions by date window */
CREATE INDEX [IX_Promotions_ActiveWindow]
ON [dbo].[Promotions] ([IsActive], [StartAt], [EndAt])
INCLUDE ([Name], [DiscountPercent])
WHERE [DeletedAt] IS NULL;
GO



CREATE TABLE [dbo].[PromotionProducts] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PromotionId] int NOT NULL,
    [ProductId] int NULL, -- Local product id (nullable for sync)
    [ErpOfferId] decimal(18,0) NULL, -- ERP offer_list.offer_id (optional)
    [ErpProductId] decimal(18,0) NOT NULL, -- ERP product_id
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PromotionProducts_CreatedAt] DEFAULT (sysutcdatetime()),
    [DeletedAt] datetime2(0) NULL, -- Soft delete
    CONSTRAINT [PK_PromotionProducts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PromotionProducts_Promotions_PromotionId] FOREIGN KEY ([PromotionId]) REFERENCES [dbo].[Promotions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PromotionProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION
);
GO

/* Prevent duplicate ERP product within the same promotion (ignoring soft-deleted rows) */
CREATE UNIQUE INDEX [UX_PromotionProducts_PromotionId_ErpProductId]
ON [dbo].[PromotionProducts] ([PromotionId], [ErpProductId])
WHERE [DeletedAt] IS NULL;
GO

/* Fast join: all products under a promotion */
CREATE INDEX [IX_PromotionProducts_PromotionId]
ON [dbo].[PromotionProducts] ([PromotionId])
INCLUDE ([ProductId], [ErpProductId])
WHERE [DeletedAt] IS NULL;
GO

/* Fast lookup: all promotions affecting a product (local) */
CREATE INDEX [IX_PromotionProducts_ProductId]
ON [dbo].[PromotionProducts] ([ProductId])
INCLUDE ([PromotionId])
WHERE [DeletedAt] IS NULL AND [ProductId] IS NOT NULL;
GO

/* Fast lookup during sync using ERP product id */
CREATE INDEX [IX_PromotionProducts_ErpProductId]
ON [dbo].[PromotionProducts] ([ErpProductId])
INCLUDE ([PromotionId], [ProductId])
WHERE [DeletedAt] IS NULL;
GO

-- ================================================================
-- Combined source: 5- Tags.sql
-- ================================================================

use pharma_shope_db;

GO

--drop table ProductTags;
--drop table Tags
go 
CREATE TABLE [dbo].[Tags] (
    [Id] int NOT NULL IDENTITY(1,1),
    [NameAr] nvarchar(80) NOT NULL,
    [NameEn] nvarchar(80) NULL,
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Tags_CreatedAt] DEFAULT (sysdatetime()),
    [IsActive] bit NOT NULL CONSTRAINT [DF_Tags_IsActive] DEFAULT ((1)),
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id]),
);

GO

CREATE TABLE [dbo].[ProductTags] (
    [ProductId] int NOT NULL,
    [TagId] int NOT NULL,
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ProductTags_CreatedAt] DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_ProductTags] PRIMARY KEY ([ProductId], [TagId]),
    CONSTRAINT [FK_ProductTags_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tags] ([Id]) ON DELETE NO ACTION
);

GO

-- Fast lookup: all products under a tag
CREATE INDEX [IX_ProductTags_TagId]
ON [dbo].[ProductTags] ([TagId]);

GO

-- ================================================================
-- Combined source: 6- cart.sql
-- ================================================================

use pharma_shope_db;

GO
--drop table [CartItems];
--drop table Carts;

CREATE TABLE [dbo].[Carts] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,
    [Status] tinyint NOT NULL CONSTRAINT [DF_Carts_Status] DEFAULT ((1)), -- 1=Active,2=CheckedOut,3=Abandoned,4=Expired,5=Invalid
    [StatusUpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Carts_StatusUpdatedAt] DEFAULT (sysdatetime()),
    [ExpiredReason] nvarchar(300) NULL, -- Reason for Expired/Invalid states
    [DeviceId] nvarchar(100) NULL, -- Device identifier (optional)
    [AppInstanceId] nvarchar(100) NULL, -- App install/session identifier (optional)
    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Carts_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Carts_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Carts_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_Carts_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5)))
);
GO



/* Enforce a single active cart per customer */
CREATE UNIQUE INDEX [UX_Carts_CustomerId_Active]
ON [dbo].[Carts] ([CustomerId])
WHERE [Status] = (1);
GO

/* Fast lookup: customer cart by status */
CREATE INDEX [IX_Carts_CustomerId_Status_StatusUpdatedAt]
ON [dbo].[Carts] ([CustomerId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([StoreId]);
GO

/* Fast lookup: store cart activity (optional reporting) */
CREATE INDEX [IX_Carts_StoreId_Status_StatusUpdatedAt]
ON [dbo].[Carts] ([StoreId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([CustomerId]);
GO






CREATE TABLE [dbo].[CartItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CartId] int NOT NULL,
    [ProductId] int NOT NULL,
    [SourceType] tinyint NOT NULL CONSTRAINT [DF_CartItems_SourceType] DEFAULT ((1)), -- 1=Normal,2=PointsRedemption

    [UnitLevel] tinyint NOT NULL, -- 1=Outer,2=Inner
    [Quantity] decimal(18,3) NOT NULL,

    [UnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_CartItems_UnitPriceSnapshot] DEFAULT ((0)), -- old price for comparison
    [CurrentUnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_CartItems_CurrentUnitPriceSnapshot] DEFAULT ((0)), -- last known current price for comparison

    [MinOrderQtySnapshot] int NOT NULL CONSTRAINT [DF_CartItems_MinOrderQtySnapshot] DEFAULT ((1)),
    [MaxOrderQtySnapshot] int NULL,
    [MaxPerDayQtySnapshot] int NULL,

    [IsValid] bit NOT NULL CONSTRAINT [DF_CartItems_IsValid] DEFAULT ((1)),
    [InvalidReason] nvarchar(300) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_CartItems_Carts_CartId]
        FOREIGN KEY ([CartId]) REFERENCES [dbo].[Carts] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_CartItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

        
    CONSTRAINT [CK_CartItems_SourceType]
    CHECK ([SourceType] IN ((1),(2))),

    CONSTRAINT [CK_CartItems_UnitLevel]
        CHECK ([UnitLevel] IN ((1),(2))),

    CONSTRAINT [CK_CartItems_QuantityPositive]
        CHECK ([Quantity] > (0)),

    CONSTRAINT [CK_CartItems_PricesNonNegative]
        CHECK ([UnitPriceSnapshot] >= (0) AND [CurrentUnitPriceSnapshot] >= (0)),

    CONSTRAINT [CK_CartItems_SnapshotsNonNegative]
        CHECK ([MinOrderQtySnapshot] > (0) AND ([MaxPerDayQtySnapshot] IS NULL OR [MaxPerDayQtySnapshot] > (0))),

    CONSTRAINT [CK_CartItems_InvalidReasonRule]
        CHECK (
            ([IsValid] = (1) AND [InvalidReason] IS NULL)
            OR ([IsValid] = (0) AND [InvalidReason] IS NOT NULL)
        )
);
GO


/* Fast lookup: all items for a cart */
CREATE INDEX [IX_CartItems_CartId]
ON [dbo].[CartItems] ([CartId])
INCLUDE ([ProductId], [UnitLevel], [Quantity], [IsValid]);
GO

/* Prevent duplicate product lines per unit level within the same cart */
CREATE UNIQUE INDEX [UX_CartItems_CartId_ProductId_UnitLevel]
ON [dbo].[CartItems] ([CartId], [ProductId], [UnitLevel]);
GO

/* Useful for availability checks and analytics */
CREATE INDEX [IX_CartItems_ProductId]
ON [dbo].[CartItems] ([ProductId]);
GO

/* Fast lookup: invalid items within a cart */
CREATE INDEX [IX_CartItems_CartId_IsValid]
ON [dbo].[CartItems] ([CartId], [IsValid]);
GO

-- ================================================================
-- Combined source: 7 - Prescriptions .sql
-- ================================================================

use pharma_shope_db;

GO
CREATE TABLE [dbo].[Prescriptions] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,

    [Status] tinyint NOT NULL CONSTRAINT [DF_Prescriptions_Status] DEFAULT ((1)), -- 1=Submitted,2=InReview,3=ReadyForCheckout,4=Closed,5=Rejected
    [StatusUpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Prescriptions_StatusUpdatedAt] DEFAULT (sysdatetime()),

    [ReviewedBy] int NULL, -- EmployeeId (reviewer)
    [ReadyForCheckoutAt] datetime2(0) NULL,

    [RejectReason] nvarchar(300) NULL,
    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Prescriptions_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Prescriptions_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Prescriptions_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Prescriptions_Employees_ReviewedBy]
        FOREIGN KEY ([ReviewedBy]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_Prescriptions_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5))),

    CONSTRAINT [CK_Prescriptions_ReadyForCheckoutRule]
        CHECK (
            [Status] <> (3)
            OR ([Status] = (3) AND [ReviewedBy] IS NOT NULL AND [ReadyForCheckoutAt] IS NOT NULL)
        ),

    CONSTRAINT [CK_Prescriptions_RejectReasonRule]
        CHECK (
            ([Status] <> (5) AND [RejectReason] IS NULL)
            OR ([Status] = (5) AND [RejectReason] IS NOT NULL)
        )
);
GO

/* Customer view: prescriptions by status and creation time */
CREATE INDEX [IX_Prescriptions_CustomerId_Status_CreatedAt]
ON [dbo].[Prescriptions] ([CustomerId], [Status], [CreatedAt] DESC)
INCLUDE ([StoreId], [StatusUpdatedAt], [ReviewedBy], [ReadyForCheckoutAt]);
GO

/* Store dashboard: queue by status and last status change */
CREATE INDEX [IX_Prescriptions_StoreId_Status_StatusUpdatedAt]
ON [dbo].[Prescriptions] ([StoreId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([CustomerId], [ReviewedBy], [ReadyForCheckoutAt]);
GO

/* Reviewer workload: prescriptions assigned/reviewed by employee */
CREATE INDEX [IX_Prescriptions_ReviewedBy_StatusUpdatedAt]
ON [dbo].[Prescriptions] ([ReviewedBy], [StatusUpdatedAt] DESC)
WHERE [ReviewedBy] IS NOT NULL;
GO

-- --------------------------------------------
-- PrescriptionImages
-- --------------------------------------------
CREATE TABLE [dbo].[PrescriptionImages] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [PrescriptionId] int NOT NULL,

    [ImageUrl] nvarchar(600) NOT NULL,
    [ThumbnailUrl] nvarchar(600) NULL,
    [AltText] nvarchar(200) NULL,

    [SortOrder] int NOT NULL CONSTRAINT [DF_PrescriptionImages_SortOrder] DEFAULT ((0)),
    [IsPrimary] bit NOT NULL CONSTRAINT [DF_PrescriptionImages_IsPrimary] DEFAULT ((0)),

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PrescriptionImages_CreatedAt] DEFAULT (sysdatetime()),
    [CreatedBy] nvarchar(100) NULL,

    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_PrescriptionImages] PRIMARY KEY ([Id]),

    CONSTRAINT [CK_PrescriptionImages_SortOrder]
        CHECK ([SortOrder] >= (0)),

    CONSTRAINT [FK_PrescriptionImages_Prescriptions_PrescriptionId]
        FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions] ([Id]) ON DELETE CASCADE
);
GO


/* Fast lookup: all images for a prescription */
CREATE INDEX [IX_PrescriptionImages_PrescriptionId]
ON [dbo].[PrescriptionImages] ([PrescriptionId]);
GO

/* Efficient ordering: primary first, then sort order */
CREATE INDEX [IX_PrescriptionImages_PrescriptionId_Sort]
ON [dbo].[PrescriptionImages] ([PrescriptionId], [IsPrimary] DESC, [SortOrder], [Id])
INCLUDE ([ImageUrl], [ThumbnailUrl], [AltText]);
GO

/* Enforce a single primary image per prescription */
CREATE UNIQUE INDEX [UX_PrescriptionImages_PrescriptionId_Primary]
ON [dbo].[PrescriptionImages] ([PrescriptionId], [IsPrimary])
WHERE [IsPrimary] = (1);
GO


-- --------------------------------------------
-- PrescriptionItems
-- --------------------------------------------

CREATE TABLE [dbo].[PrescriptionItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [PrescriptionId] int NOT NULL,

    [ProductId] int NULL, -- Matched product (optional)
    [RequestedName] nvarchar(250) NOT NULL, -- Free text when product not matched
    [RequestedQuantity] decimal(18,3) NULL, -- Optional (if user/employee provides)
    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PrescriptionItems_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_PrescriptionItems] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_PrescriptionItems_Prescriptions_PrescriptionId]
        FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_PrescriptionItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_PrescriptionItems_RequestedQuantityPositive]
        CHECK ([RequestedQuantity] IS NULL OR [RequestedQuantity] > (0))
);
GO

/* Fast lookup: items for a prescription */
CREATE INDEX [IX_PrescriptionItems_PrescriptionId]
ON [dbo].[PrescriptionItems] ([PrescriptionId]);
GO

/* Fast lookup: matched items by product (optional reporting) */
CREATE INDEX [IX_PrescriptionItems_ProductId]
ON [dbo].[PrescriptionItems] ([ProductId])
WHERE [ProductId] IS NOT NULL;
GO

-- ================================================================
-- Combined source: 7- Orders.sql
-- ================================================================

use pharma_shope_db;
GO

CREATE TABLE [dbo].[Orders] (
    [Id] int NOT NULL IDENTITY(1,1),
    [OrderNumber] nvarchar(30) NOT NULL, -- Human-readable order number
    [CustomerId] int NOT NULL,
    [StoreId] int NOT NULL,
    [OrderSource] tinyint NOT NULL CONSTRAINT [DF_Orders_OrderSource] DEFAULT ((1)), -- 1=Cart,2=Prescription,3=PointsRedemption

    [PrescriptionId] int NULL, -- FK to Prescriptions (nullable)

    [Status] tinyint NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT ((1)), -- 1=Pending,2=Confirmed,3=Preparing,4=OutForDelivery,5=Delivered,6=Cancelled,7=Returned,8=PendingApproval
    [StatusUpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Orders_StatusUpdatedAt] DEFAULT (sysdatetime()),

    [PaymentMethod] tinyint NOT NULL CONSTRAINT [DF_Orders_PaymentMethod] DEFAULT ((1)), -- 1=COD
    [PaymentStatus] tinyint NOT NULL CONSTRAINT [DF_Orders_PaymentStatus] DEFAULT ((1)), -- 1=Unpaid,2=Paid,3=Refunded

    [Subtotal] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_Subtotal] DEFAULT ((0)),
    [ItemsDiscountTotal] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_ItemsDiscountTotal] DEFAULT ((0)),
    [DeliveryFee] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_DeliveryFee] DEFAULT ((0)),
    [RedeemedPoints] int NOT NULL CONSTRAINT [DF_Orders_RedeemedPoints] DEFAULT ((0)),
    [RedeemedAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_RedeemedAmount] DEFAULT ((0)),
    [GrandTotal] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_GrandTotal] DEFAULT ((0)),
    [EarnedPoints] int NOT NULL CONSTRAINT [DF_Orders_EarnedPoints] DEFAULT ((0)),

    [AddressId] int NULL, -- FK to CustomerAddresses (optional)
    [DeliveryTitle] nvarchar(100) NULL,
    [DeliveryCity] nvarchar(100) NOT NULL,
    [DeliveryRegion] nvarchar(100) NULL,
    [DeliveryStreet] nvarchar(300) NOT NULL,
    [DeliveryLatitude] float NULL,
    [DeliveryLongitude] float NULL,
    [DeliveryPhone] nvarchar(20) NULL,

    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,
    [RowVersion] rowversion NOT NULL, -- Concurrency

    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_Orders_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Orders_Stores_StoreId]
        FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Orders_CustomerAddresses_AddressId]
        FOREIGN KEY ([AddressId]) REFERENCES [dbo].[CustomerAddresses] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_Orders_Prescriptions_PrescriptionId]
        FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions] ([Id]) ON DELETE NO ACTION,

    
    CONSTRAINT [CK_Orders_OrderSource]
    CHECK ([OrderSource] IN ((1),(2),(3))),

    CONSTRAINT [CK_Orders_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5),(6),(7),(8))),

    CONSTRAINT [CK_Orders_PendingApproval_RequiresPrescription]
        CHECK ([Status] <> (8) OR ([Status] = (8) AND [PrescriptionId] IS NOT NULL)),

    CONSTRAINT [CK_Orders_PaymentMethod]
        CHECK ([PaymentMethod] IN ((1))), -- extend later

    CONSTRAINT [CK_Orders_PaymentStatus]
        CHECK ([PaymentStatus] IN ((1),(2),(3))),

    CONSTRAINT [CK_Orders_Totals_NonNegative]
        CHECK (
            [Subtotal] >= (0)
            AND [ItemsDiscountTotal] >= (0)
            AND [DeliveryFee] >= (0)
            AND [RedeemedAmount] >= (0)
            AND [GrandTotal] >= (0)
        ),

    CONSTRAINT [CK_Orders_Points_NonNegative]
        CHECK ([RedeemedPoints] >= (0) AND [EarnedPoints] >= (0))
);
GO

/* Unique order number */
CREATE UNIQUE INDEX [UX_Orders_OrderNumber]
ON [dbo].[Orders] ([OrderNumber]);
GO

/* Enforce one order per prescription (when PrescriptionId is present) */
CREATE UNIQUE INDEX [UX_Orders_PrescriptionId]
ON [dbo].[Orders] ([PrescriptionId])
WHERE [PrescriptionId] IS NOT NULL;
GO

/* Fast lookup: customer orders by status and date */
CREATE INDEX [IX_Orders_CustomerId_Status_CreatedAt]
ON [dbo].[Orders] ([CustomerId], [Status], [CreatedAt] DESC)
INCLUDE ([OrderNumber], [GrandTotal], [PaymentStatus], [StoreId], [PrescriptionId]);
GO

/* Fast lookup: store orders queue (preparing/delivery) */
CREATE INDEX [IX_Orders_StoreId_Status_StatusUpdatedAt]
ON [dbo].[Orders] ([StoreId], [Status], [StatusUpdatedAt] DESC)
INCLUDE ([OrderNumber], [CustomerId], [GrandTotal], [PaymentStatus], [PrescriptionId]);
GO

/* Fast lookup: payment status reporting */
CREATE INDEX [IX_Orders_PaymentStatus_CreatedAt]
ON [dbo].[Orders] ([PaymentStatus], [CreatedAt] DESC)
INCLUDE ([OrderNumber], [CustomerId], [GrandTotal], [PaymentMethod], [PrescriptionId]);
GO

CREATE TABLE [dbo].[OrderItems] (
    [Id] int NOT NULL IDENTITY(1,1),
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,

    [UnitLevel] tinyint NOT NULL, -- 1=Outer,2=Inner
    [Quantity] decimal(18,3) NOT NULL,

    [OuterUnitIdSnapshot] int NOT NULL,
    [InnerUnitIdSnapshot] int NULL,
    [InnerPerOuterSnapshot] int NULL,

    [UnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_UnitPriceSnapshot] DEFAULT ((0)),
    [DiscountPercentSnapshot] decimal(5,2) NOT NULL CONSTRAINT [DF_OrderItems_DiscountPercentSnapshot] DEFAULT ((0)),
    [FinalUnitPriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_FinalUnitPriceSnapshot] DEFAULT ((0)),
    [LineTotal] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_LineTotal] DEFAULT ((0)),

    [PointsSnapshot] int NOT NULL CONSTRAINT [DF_OrderItems_PointsSnapshot] DEFAULT ((0)),
    [EarnedPoints] int NOT NULL CONSTRAINT [DF_OrderItems_EarnedPoints] DEFAULT ((0)),

    [AppliedPromotionId] int NULL, -- Group promotion applied (if any)

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_OrderItems_CreatedAt] DEFAULT (sysdatetime()),

    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_OrderItems_Orders_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,

    CONSTRAINT [FK_OrderItems_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_OrderItems_Promotions_AppliedPromotionId]
        FOREIGN KEY ([AppliedPromotionId]) REFERENCES [dbo].[Promotions] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_OrderItems_UnitLevel]
        CHECK ([UnitLevel] IN ((1),(2))),

    CONSTRAINT [CK_OrderItems_QuantityPositive]
        CHECK ([Quantity] > (0)),

    CONSTRAINT [CK_OrderItems_DiscountPercent]
        CHECK ([DiscountPercentSnapshot] >= (0) AND [DiscountPercentSnapshot] <= (100)),

    CONSTRAINT [CK_OrderItems_PricesNonNegative]
        CHECK (
            [UnitPriceSnapshot] >= (0)
            AND [FinalUnitPriceSnapshot] >= (0)
            AND [LineTotal] >= (0)
        ),

    CONSTRAINT [CK_OrderItems_PointsNonNegative]
        CHECK ([PointsSnapshot] >= (0) AND [EarnedPoints] >= (0)),

    CONSTRAINT [CK_OrderItems_InnerSnapshotRules]
        CHECK (
            ([InnerUnitIdSnapshot] IS NULL AND [InnerPerOuterSnapshot] IS NULL)
            OR ([InnerUnitIdSnapshot] IS NOT NULL AND [InnerPerOuterSnapshot] IS NOT NULL AND [InnerPerOuterSnapshot] >= (1))
        )
);
GO


/* Fast lookup: all items for an order */
CREATE INDEX [IX_OrderItems_OrderId]
ON [dbo].[OrderItems] ([OrderId])
INCLUDE ([ProductId], [UnitLevel], [Quantity], [FinalUnitPriceSnapshot], [LineTotal], [EarnedPoints], [AppliedPromotionId]);
GO

/* Prevent duplicate lines for same product + unit within the same order (optional but recommended) */
CREATE UNIQUE INDEX [UX_OrderItems_OrderId_ProductId_UnitLevel]
ON [dbo].[OrderItems] ([OrderId], [ProductId], [UnitLevel]);
GO

/* Useful for reporting: product sales lines */
CREATE INDEX [IX_OrderItems_ProductId_CreatedAt]
ON [dbo].[OrderItems] ([ProductId], [CreatedAt] DESC)
INCLUDE ([Quantity], [LineTotal], [UnitLevel]);
GO

-- ================================================================
-- Combined source: 8- Points.sql
-- ================================================================

use pharma_shope_db;
GO

-- =====================================
-- CustomerPointLots
-- =====================================

CREATE TABLE [dbo].[CustomerPointLots] (
    [Id] int NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,
    [OrderId] int NULL,
    [OrderItemId] int NULL,

    [PointsTotal] int NOT NULL,
    [RemainingPoints] int NOT NULL,

    [Status] tinyint NOT NULL CONSTRAINT [DF_CustomerPointLots_Status] DEFAULT ((1)), -- 1=Pending,2=Available,3=Used,4=Expired,5=Cancelled
    [EarnedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerPointLots_EarnedAt] DEFAULT (sysdatetime()),
    [AvailableAt] datetime2(0) NULL,
    [ExpiresAt] datetime2(0) NOT NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerPointLots_CreatedAt] DEFAULT (sysdatetime()),
    [UpdatedAt] datetime2(0) NULL,

    CONSTRAINT [PK_CustomerPointLots] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_CustomerPointLots_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointLots_Orders_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointLots_OrderItems_OrderItemId]
        FOREIGN KEY ([OrderItemId]) REFERENCES [dbo].[OrderItems] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_CustomerPointLots_Status]
        CHECK ([Status] IN ((1),(2),(3),(4),(5))),

    CONSTRAINT [CK_CustomerPointLots_Points]
        CHECK ([PointsTotal] > (0) AND [RemainingPoints] >= (0) AND [RemainingPoints] <= [PointsTotal]),

    CONSTRAINT [CK_CustomerPointLots_ExpiresAt]
        CHECK ([ExpiresAt] > [EarnedAt]),

    --CONSTRAINT [CK_CustomerPointLots_AvailableRule]
    --    CHECK (
    --        ([Status] <> (2) AND [AvailableAt] IS NULL)
    --        OR ([Status] = (2) AND [AvailableAt] IS NOT NULL)
    --    )
    
    CONSTRAINT [CK_CustomerPointLots_AvailableRule]
        CHECK (
            ([Status] = (1) AND [AvailableAt] IS NULL)
            OR ([Status] = (2) AND [AvailableAt] IS NOT NULL)
            OR ([Status] = (3) AND [AvailableAt] IS NOT NULL)
            OR ([Status] = (4) AND [AvailableAt] IS NOT NULL)
            OR ([Status] = (5) AND [AvailableAt] IS NULL)
        )

);
GO

-- =====================================
-- CustomerPointTransactions
-- =====================================

CREATE TABLE [dbo].[CustomerPointTransactions] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [CustomerId] int NOT NULL,

    [LotId] int NULL,
    [OrderId] int NULL,
    [OrderItemId] int NULL,

    [TransactionType] tinyint NOT NULL, -- 1=EarnPending,2=EarnAvailable,3=Redeem,4=RefundRedeem,5=Expire,6=CancelEarn,7=ManualAdjust
    [PointsDelta] int NOT NULL,
    [BalanceAfter] int NULL,

    [PointsPerEGP] int NULL,
    [AmountEGP] decimal(18,2) NULL,

    [ExpiresAt] datetime2(0) NULL, -- Required for Earn transactions
    [ReferenceType] tinyint NULL, -- 1=Order,2=Return,3=Manual,4=ExpiryJob
    [ReferenceId] int NULL,

    [SourceTransactionId] bigint NULL,
    [CreatedByEmployeeId] int NULL,
    [Notes] nvarchar(500) NULL,

    [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerPointTransactions_CreatedAt] DEFAULT (sysdatetime()),

    CONSTRAINT [PK_CustomerPointTransactions] PRIMARY KEY ([Id]),

    CONSTRAINT [FK_CustomerPointTransactions_Customers_CustomerId]
        FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_Lots_LotId]
        FOREIGN KEY ([LotId]) REFERENCES [dbo].[CustomerPointLots] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_Orders_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_OrderItems_OrderItemId]
        FOREIGN KEY ([OrderItemId]) REFERENCES [dbo].[OrderItems] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_SourceTransactionId]
        FOREIGN KEY ([SourceTransactionId]) REFERENCES [dbo].[CustomerPointTransactions] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [FK_CustomerPointTransactions_Employees_CreatedByEmployeeId]
        FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [dbo].[Employees] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT [CK_CustomerPointTransactions_Type]
        CHECK ([TransactionType] IN ((1),(2),(3),(4),(5),(6),(7),(8))),

    CONSTRAINT [CK_CustomerPointTransactions_PointsDelta]
        CHECK (
            ([TransactionType] IN ((1),(2),(4),(8)) AND [PointsDelta] > (0))
            OR ([TransactionType] IN ((3),(5),(6)) AND [PointsDelta] < (0))
            OR ([TransactionType] = (7) AND [PointsDelta] <> (0))
        ),

    CONSTRAINT [CK_CustomerPointTransactions_BalanceAfter]
        CHECK ([BalanceAfter] IS NULL OR [BalanceAfter] >= (0)),

    CONSTRAINT [CK_CustomerPointTransactions_Conversion]
        CHECK (
            ([PointsPerEGP] IS NULL AND [AmountEGP] IS NULL)
            OR ([PointsPerEGP] IS NOT NULL AND [PointsPerEGP] > (0) AND [AmountEGP] IS NOT NULL AND [AmountEGP] >= (0))
        ),

    CONSTRAINT [CK_CustomerPointTransactions_EarnExpiresAt]
        CHECK (
            ([TransactionType] NOT IN ((1),(2),(8)) AND [ExpiresAt] IS NULL)
            OR ([TransactionType] IN ((1),(2),(8)) AND [ExpiresAt] IS NOT NULL)
        ),

    CONSTRAINT [CK_CustomerPointTransactions_ReferenceType]
        CHECK ([ReferenceType] IS NULL OR [ReferenceType] IN ((1),(2),(3),(4),(5)))
);
GO


-- ==========================
-- Indexes CustomerPointLots
-- ==========================

/* Fast lookup: available lots by customer and expiry */
CREATE INDEX [IX_CustomerPointLots_CustomerId_Status_ExpiresAt]
ON [dbo].[CustomerPointLots] ([CustomerId], [Status], [ExpiresAt])
INCLUDE ([RemainingPoints])
WHERE [RemainingPoints] > (0);
GO

/* Fast lookup: point lots generated from an order */
CREATE INDEX [IX_CustomerPointLots_OrderId]
ON [dbo].[CustomerPointLots] ([OrderId])
WHERE [OrderId] IS NOT NULL;
GO

/* Fast lookup: expirable available lots */
CREATE INDEX [IX_CustomerPointLots_Status_ExpiresAt]
ON [dbo].[CustomerPointLots] ([Status], [ExpiresAt])
INCLUDE ([CustomerId], [RemainingPoints])
WHERE [Status] = (2) AND [RemainingPoints] > (0);
GO

-- ==========================
-- Indexes CustomerPointTransactions
-- ==========================

/* Customer ledger timeline */
CREATE INDEX [IX_CustomerPointTransactions_CustomerId_CreatedAt]
ON [dbo].[CustomerPointTransactions] ([CustomerId], [CreatedAt] DESC)
INCLUDE ([TransactionType], [PointsDelta], [BalanceAfter], [OrderId], [LotId]);
GO

/* Fast lookup: transactions by order */
CREATE INDEX [IX_CustomerPointTransactions_OrderId]
ON [dbo].[CustomerPointTransactions] ([OrderId])
WHERE [OrderId] IS NOT NULL;
GO

/* Fast lookup: transactions by lot */
CREATE INDEX [IX_CustomerPointTransactions_LotId]
ON [dbo].[CustomerPointTransactions] ([LotId])
WHERE [LotId] IS NOT NULL;
GO

/* Prevent duplicate transaction per reference and type */
--CREATE UNIQUE INDEX [UX_CustomerPointTransactions_Type_Reference]
--ON [dbo].[CustomerPointTransactions] ([TransactionType], [ReferenceType], [ReferenceId])
--WHERE [ReferenceType] IS NOT NULL AND [ReferenceId] IS NOT NULL;
--GO


/* Prevent duplicate point transaction per type, reference, and lot */
CREATE UNIQUE INDEX [UX_CustomerPointTransactions_Type_Reference_Lot]
ON [dbo].[CustomerPointTransactions] ([TransactionType], [ReferenceType], [ReferenceId], [LotId])
WHERE [ReferenceType] IS NOT NULL
  AND [ReferenceId] IS NOT NULL
  AND [LotId] IS NOT NULL;
GO


/* Fast lookup: earn transactions with expiry */
CREATE INDEX [IX_CustomerPointTransactions_CustomerId_ExpiresAt]
ON [dbo].[CustomerPointTransactions] ([CustomerId], [ExpiresAt])
WHERE [ExpiresAt] IS NOT NULL;
GO

/* A redemption transaction can be refunded at most once. */
CREATE UNIQUE INDEX [UX_CustomerPointTransactions_SourceRefund]
ON [dbo].[CustomerPointTransactions] ([SourceTransactionId])
WHERE [TransactionType] = (4) AND [SourceTransactionId] IS NOT NULL;
GO

/* Manual adjustment reference IDs are idempotency keys supplied by the caller. */
CREATE UNIQUE INDEX [UX_CustomerPointTransactions_ManualAdjustmentReference]
ON [dbo].[CustomerPointTransactions] ([ReferenceId])
WHERE [TransactionType] = (7) AND [ReferenceType] = (3) AND [ReferenceId] IS NOT NULL;
GO


-- ================================================================
-- Combined source: 10 - Inventory.sql
-- ================================================================
USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.ProductInventoryTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductInventoryTransactions
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductInventoryTransactions PRIMARY KEY,
        ProductId INT NOT NULL,
        EmployeeId INT NULL,
        OrderId INT NULL,
        OrderItemId INT NULL,
        ProductUnitId INT NULL,
        TransactionType TINYINT NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        QuantityBefore DECIMAL(18,3) NOT NULL,
        QuantityAfter DECIMAL(18,3) NOT NULL,
        Reason NVARCHAR(500) NOT NULL,
        BatchNumber NVARCHAR(80) NULL,
        ExpirationDate DATETIME2(0) NULL,
        CostPrice DECIMAL(18,2) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ProductInventoryTransactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_ProductInventoryTransactions_Type CHECK (TransactionType BETWEEN 1 AND 6),
        CONSTRAINT CK_ProductInventoryTransactions_Quantity CHECK (Quantity > 0),
        CONSTRAINT FK_ProductInventoryTransactions_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
        CONSTRAINT FK_ProductInventoryTransactions_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id),
        CONSTRAINT FK_ProductInventoryTransactions_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_ProductInventoryTransactions_OrderItems FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(Id),
        CONSTRAINT FK_ProductInventoryTransactions_Units FOREIGN KEY (ProductUnitId) REFERENCES dbo.Units(Id)
    );

    CREATE INDEX IX_ProductInventoryTransactions_Product_CreatedAt
        ON dbo.ProductInventoryTransactions(ProductId, CreatedAt DESC);
    CREATE INDEX IX_ProductInventoryTransactions_Order
        ON dbo.ProductInventoryTransactions(OrderId, TransactionType) WHERE OrderId IS NOT NULL;
    CREATE UNIQUE INDEX UX_ProductInventoryTransactions_OrderDeduction
        ON dbo.ProductInventoryTransactions(OrderId, OrderItemId)
        WHERE OrderId IS NOT NULL AND OrderItemId IS NOT NULL AND TransactionType = 4;
    CREATE UNIQUE INDEX UX_ProductInventoryTransactions_OrderRestock
        ON dbo.ProductInventoryTransactions(OrderId, OrderItemId)
        WHERE OrderId IS NOT NULL AND OrderItemId IS NOT NULL AND TransactionType = 5;
END;
GO


-- ================================================================
-- Combined source: 11 - Returns.sql
-- ================================================================
USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.ReturnRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReturnRequests
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReturnRequests PRIMARY KEY,
        OrderId INT NOT NULL,
        CustomerId INT NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_ReturnRequests_Status DEFAULT (1),
        Reason NVARCHAR(500) NOT NULL,
        RequestedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReturnRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),
        ReviewedByEmployeeId INT NULL,
        ReviewedByUserId INT NULL,
        ReviewedAt DATETIME2(0) NULL,
        ReviewNotes NVARCHAR(500) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReturnRequests_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT CK_ReturnRequests_Status CHECK (Status IN (1,2,3)),
        CONSTRAINT FK_ReturnRequests_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_ReturnRequests_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
        CONSTRAINT FK_ReturnRequests_Employees FOREIGN KEY (ReviewedByEmployeeId) REFERENCES dbo.Employees(Id),
        CONSTRAINT FK_ReturnRequests_Users FOREIGN KEY (ReviewedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );

    CREATE INDEX IX_ReturnRequests_Status_RequestedAt ON dbo.ReturnRequests(Status, RequestedAt DESC);
    CREATE INDEX IX_ReturnRequests_Order_Status ON dbo.ReturnRequests(OrderId, Status);

    CREATE TABLE dbo.ReturnRequestItems
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReturnRequestItems PRIMARY KEY,
        ReturnRequestId INT NOT NULL,
        OrderItemId INT NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReturnRequestItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_ReturnRequestItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT FK_ReturnRequestItems_ReturnRequests FOREIGN KEY (ReturnRequestId) REFERENCES dbo.ReturnRequests(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReturnRequestItems_OrderItems FOREIGN KEY (OrderItemId) REFERENCES dbo.OrderItems(Id)
    );

    CREATE UNIQUE INDEX UX_ReturnRequestItems_Return_OrderItem ON dbo.ReturnRequestItems(ReturnRequestId, OrderItemId);
    CREATE INDEX IX_ReturnRequestItems_OrderItem ON dbo.ReturnRequestItems(OrderItemId);
END;
GO

IF COL_LENGTH(N'dbo.ReturnRequests', N'ReviewedByUserId') IS NULL
    ALTER TABLE dbo.ReturnRequests ADD ReviewedByUserId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ReturnRequests_Users')
    ALTER TABLE dbo.ReturnRequests WITH CHECK ADD CONSTRAINT FK_ReturnRequests_Users
        FOREIGN KEY (ReviewedByUserId) REFERENCES dbo.AspNetUsers(Id);
GO

IF COL_LENGTH(N'dbo.ProductInventoryTransactions', N'ReturnRequestItemId') IS NULL
    ALTER TABLE dbo.ProductInventoryTransactions ADD ReturnRequestItemId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductInventoryTransactions_ReturnRequestItems')
    ALTER TABLE dbo.ProductInventoryTransactions WITH CHECK
        ADD CONSTRAINT FK_ProductInventoryTransactions_ReturnRequestItems
        FOREIGN KEY (ReturnRequestItemId) REFERENCES dbo.ReturnRequestItems(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ProductInventoryTransactions_ReturnRequestItem')
    CREATE UNIQUE INDEX UX_ProductInventoryTransactions_ReturnRequestItem
        ON dbo.ProductInventoryTransactions(ReturnRequestItemId)
        WHERE ReturnRequestItemId IS NOT NULL;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ProductInventoryTransactions_Type')
    ALTER TABLE dbo.ProductInventoryTransactions DROP CONSTRAINT CK_ProductInventoryTransactions_Type;
ALTER TABLE dbo.ProductInventoryTransactions WITH CHECK
    ADD CONSTRAINT CK_ProductInventoryTransactions_Type CHECK (TransactionType BETWEEN 1 AND 6);
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_Type')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_Type;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_PointsDelta')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_PointsDelta;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_EarnExpiresAt')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_EarnExpiresAt;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CustomerPointTransactions_ReferenceType')
    ALTER TABLE dbo.CustomerPointTransactions DROP CONSTRAINT CK_CustomerPointTransactions_ReferenceType;

ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_Type CHECK (TransactionType IN (1,2,3,4,5,6,7,8));
ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_PointsDelta CHECK (
        (TransactionType IN (1,2,4,8) AND PointsDelta > 0)
        OR (TransactionType IN (3,5,6) AND PointsDelta < 0)
        OR (TransactionType = 7 AND PointsDelta <> 0));
ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_EarnExpiresAt CHECK (
        (TransactionType NOT IN (1,2,8) AND ExpiresAt IS NULL)
        OR (TransactionType IN (1,2,8) AND ExpiresAt IS NOT NULL));
ALTER TABLE dbo.CustomerPointTransactions WITH CHECK
    ADD CONSTRAINT CK_CustomerPointTransactions_ReferenceType CHECK (ReferenceType IS NULL OR ReferenceType IN (1,2,3,4,5));
GO


-- ================================================================
-- Combined source: 12 - Delivery.sql
-- ================================================================
USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.OrderDeliveryAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDeliveryAssignments
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDeliveryAssignments PRIMARY KEY,
        OrderId INT NOT NULL,
        DeliveryAgentUserId INT NOT NULL,
        AssignedByUserId INT NULL,
        UnassignedByUserId INT NULL,
        AssignedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OrderDeliveryAssignments_AssignedAt DEFAULT (SYSUTCDATETIME()),
        UnassignedAt DATETIME2(0) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_OrderDeliveryAssignments_IsActive DEFAULT (1),
        Notes NVARCHAR(500) NULL,
        CONSTRAINT FK_OrderDeliveryAssignments_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderDeliveryAssignments_DeliveryAgents FOREIGN KEY (DeliveryAgentUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_OrderDeliveryAssignments_AssignedByUsers FOREIGN KEY (AssignedByUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_OrderDeliveryAssignments_UnassignedByUsers FOREIGN KEY (UnassignedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE UNIQUE INDEX UX_OrderDeliveryAssignments_ActiveOrder
        ON dbo.OrderDeliveryAssignments(OrderId) WHERE IsActive = 1;
    CREATE INDEX IX_OrderDeliveryAssignments_Agent_Active_AssignedAt
        ON dbo.OrderDeliveryAssignments(DeliveryAgentUserId, IsActive, AssignedAt DESC);
END;
GO

IF COL_LENGTH(N'dbo.OrderDeliveryAssignments', N'UnassignedByUserId') IS NULL
    ALTER TABLE dbo.OrderDeliveryAssignments ADD UnassignedByUserId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_OrderDeliveryAssignments_UnassignedByUsers')
    ALTER TABLE dbo.OrderDeliveryAssignments WITH CHECK ADD CONSTRAINT FK_OrderDeliveryAssignments_UnassignedByUsers
        FOREIGN KEY (UnassignedByUserId) REFERENCES dbo.AspNetUsers(Id);
GO

IF OBJECT_ID(N'dbo.OrderStatusHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderStatusHistories
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderStatusHistories PRIMARY KEY,
        OrderId INT NOT NULL,
        PreviousStatus TINYINT NULL,
        NewStatus TINYINT NOT NULL,
        ChangedByUserId INT NULL,
        ChangedByRole NVARCHAR(50) NOT NULL,
        Notes NVARCHAR(500) NULL,
        ChangedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OrderStatusHistories_ChangedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_OrderStatusHistories_Status CHECK (NewStatus BETWEEN 1 AND 8 AND (PreviousStatus IS NULL OR PreviousStatus BETWEEN 1 AND 8)),
        CONSTRAINT FK_OrderStatusHistories_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderStatusHistories_Users FOREIGN KEY (ChangedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_OrderStatusHistories_Order_ChangedAt
        ON dbo.OrderStatusHistories(OrderId, ChangedAt, Id);
END;
GO

INSERT INTO dbo.OrderStatusHistories
    (OrderId, PreviousStatus, NewStatus, ChangedByUserId, ChangedByRole, Notes, ChangedAt)
SELECT o.Id, NULL, o.Status, NULL, N'System', N'History initialized from the current order status during Epic 16 rollout', o.StatusUpdatedAt
FROM dbo.Orders AS o
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrderStatusHistories AS h WHERE h.OrderId = o.Id);
GO


-- ================================================================
-- Combined source: 13 - Payments and Notifications.sql
-- ================================================================
USE [pharma_shope_db];
GO

IF OBJECT_ID(N'dbo.UserNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserNotifications
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserNotifications PRIMARY KEY,
        UserId INT NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        RelatedEntityType NVARCHAR(50) NULL,
        RelatedEntityId BIGINT NULL,
        IsRead BIT NOT NULL CONSTRAINT DF_UserNotifications_IsRead DEFAULT (0),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_UserNotifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ReadAt DATETIME2(0) NULL,
        CONSTRAINT FK_UserNotifications_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_UserNotifications_User_Read_CreatedAt
        ON dbo.UserNotifications(UserId, IsRead, CreatedAt DESC, Id DESC);
END;
GO

IF OBJECT_ID(N'dbo.CustomerNotificationPreferences', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerNotificationPreferences
    (
        UserId INT NOT NULL CONSTRAINT PK_CustomerNotificationPreferences PRIMARY KEY,
        OrderUpdatesEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Order DEFAULT (1),
        PrescriptionUpdatesEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Prescription DEFAULT (1),
        PointsUpdatesEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Points DEFAULT (1),
        EmailEnabled BIT NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_Email DEFAULT (1),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerNotificationPreferences_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_CustomerNotificationPreferences_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.PaymentAttempts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentAttempts
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentAttempts PRIMARY KEY,
        OrderId INT NOT NULL,
        Provider NVARCHAR(50) NOT NULL,
        ExternalReference NVARCHAR(200) NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_PaymentAttempts_Status DEFAULT (1),
        FailureReason NVARCHAR(500) NULL,
        CreatedByUserId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PaymentAttempts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT CK_PaymentAttempts_Amount CHECK (Amount > 0),
        CONSTRAINT CK_PaymentAttempts_Status CHECK (Status BETWEEN 1 AND 4),
        CONSTRAINT FK_PaymentAttempts_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_PaymentAttempts_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_PaymentAttempts_Order_CreatedAt ON dbo.PaymentAttempts(OrderId, CreatedAt DESC);
    CREATE UNIQUE INDEX UX_PaymentAttempts_Provider_ExternalReference
        ON dbo.PaymentAttempts(Provider, ExternalReference) WHERE ExternalReference IS NOT NULL;
END;
GO

IF OBJECT_ID(N'dbo.PaymentRefunds', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentRefunds
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentRefunds PRIMARY KEY,
        PaymentAttemptId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_PaymentRefunds_Status DEFAULT (1),
        ExternalReference NVARCHAR(200) NULL,
        Reason NVARCHAR(500) NOT NULL,
        RequestedByUserId INT NULL,
        ProcessedByUserId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PaymentRefunds_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ProcessedAt DATETIME2(0) NULL,
        CONSTRAINT CK_PaymentRefunds_Amount CHECK (Amount > 0),
        CONSTRAINT CK_PaymentRefunds_Status CHECK (Status BETWEEN 1 AND 4),
        CONSTRAINT FK_PaymentRefunds_PaymentAttempts FOREIGN KEY (PaymentAttemptId) REFERENCES dbo.PaymentAttempts(Id),
        CONSTRAINT FK_PaymentRefunds_RequestedBy FOREIGN KEY (RequestedByUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_PaymentRefunds_ProcessedBy FOREIGN KEY (ProcessedByUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    CREATE INDEX IX_PaymentRefunds_Attempt_Status ON dbo.PaymentRefunds(PaymentAttemptId, Status);
    CREATE UNIQUE INDEX UX_PaymentRefunds_ExternalReference
        ON dbo.PaymentRefunds(ExternalReference) WHERE ExternalReference IS NOT NULL;
END;
GO

-- Provider-specific payment initiation and webhook verification are intentionally
-- not enabled until the selected payment provider and its credentials are configured.


-- ================================================================
-- Combined source: 15 - Promotional Banners.sql
-- ================================================================
USE [pharma_shope_db];
GO

/*
    Promotional Banners
    Run after the base schema scripts. This script is safe to run more than once.
    BannerPlacement: Home=0, Offers=1, Categories=2
    BannerActionType: None=0, Product=1, Offer=2, Category=3, Brand=4, ExternalUrl=5
*/
IF OBJECT_ID(N'dbo.PromotionalBanners', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PromotionalBanners]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Placement] tinyint NOT NULL,
        [ActionType] tinyint NOT NULL,
        [TargetId] int NULL,
        [ParentTargetId] int NULL,
        [ActionUrl] nvarchar(2048) NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [ThumbnailUrl] nvarchar(500) NOT NULL,
        [AltText] nvarchar(200) NULL,
        [DisplayOrder] int NOT NULL CONSTRAINT [DF_PromotionalBanners_DisplayOrder] DEFAULT ((0)),
        [IsActive] bit NOT NULL CONSTRAINT [DF_PromotionalBanners_IsActive] DEFAULT ((1)),
        [StartsAt] datetime2(0) NULL,
        [EndsAt] datetime2(0) NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_PromotionalBanners_CreatedAt] DEFAULT (sysutcdatetime()),
        [UpdatedAt] datetime2(0) NULL,
        [DeletedAt] datetime2(0) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PromotionalBanners] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PromotionalBanners_Placement] CHECK ([Placement] IN (0, 1, 2)),
        CONSTRAINT [CK_PromotionalBanners_ActionType] CHECK ([ActionType] IN (0, 1, 2, 3, 4, 5)),
        CONSTRAINT [CK_PromotionalBanners_DisplayOrder] CHECK ([DisplayOrder] >= 0),
        CONSTRAINT [CK_PromotionalBanners_DateWindow] CHECK ([StartsAt] IS NULL OR [EndsAt] IS NULL OR [EndsAt] > [StartsAt]),
        CONSTRAINT [CK_PromotionalBanners_ActionTarget] CHECK
        (
            ([ActionType] = 0 AND [TargetId] IS NULL AND [ParentTargetId] IS NULL AND [ActionUrl] IS NULL)
            OR ([ActionType] IN (1, 2, 4) AND [TargetId] IS NOT NULL AND [TargetId] > 0 AND [ParentTargetId] IS NULL AND [ActionUrl] IS NULL)
            OR ([ActionType] = 3 AND [TargetId] IS NOT NULL AND [TargetId] > 0 AND ([ParentTargetId] IS NULL OR [ParentTargetId] > 0) AND [ActionUrl] IS NULL)
            OR ([ActionType] = 5 AND [TargetId] IS NULL AND [ParentTargetId] IS NULL AND [ActionUrl] LIKE N'https://%')
        ),
        CONSTRAINT [CK_PromotionalBanners_ImagePaths] CHECK ([ImageUrl] LIKE N'/uploads/%' AND [ThumbnailUrl] LIKE N'/uploads/%')
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_PromotionalBanners_PublicOrder' AND [object_id] = OBJECT_ID(N'dbo.PromotionalBanners'))
    CREATE INDEX [IX_PromotionalBanners_PublicOrder]
        ON [dbo].[PromotionalBanners] ([Placement], [IsActive], [DisplayOrder], [Id])
        INCLUDE ([StartsAt], [EndsAt], [Name], [ActionType], [TargetId], [ParentTargetId], [ActionUrl], [ImageUrl], [ThumbnailUrl], [AltText])
        WHERE [DeletedAt] IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_PromotionalBanners_DateWindow' AND [object_id] = OBJECT_ID(N'dbo.PromotionalBanners'))
    CREATE INDEX [IX_PromotionalBanners_DateWindow]
        ON [dbo].[PromotionalBanners] ([StartsAt], [EndsAt])
        WHERE [IsActive] = 1 AND [DeletedAt] IS NULL;
GO


-- ================================================================
-- Combined source: 17 - Checkout Idempotency and Refresh Tokens.sql
-- ================================================================
USE [pharma_shope_db];
GO

/*
   Additive schema update for checkout idempotency and rotating refresh tokens.
   Safe to run more than once. This script does not modify or recreate existing data.
   COD remains PaymentMethod = 1, matching the existing Orders constraint.
*/

SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Orders', N'CheckoutIdempotencyKey') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CheckoutIdempotencyKey] nvarchar(100) NULL;

IF COL_LENGTH(N'dbo.Orders', N'CustomerNotes') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CustomerNotes] nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Orders', N'CheckoutRequestHash') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CheckoutRequestHash] varchar(64) NULL;

IF COL_LENGTH(N'dbo.Orders', N'CheckoutResponseJson') IS NULL
    ALTER TABLE [dbo].[Orders] ADD [CheckoutResponseJson] nvarchar(max) NULL;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'dbo.Orders')
      AND [name] = N'UX_Orders_CustomerId_CheckoutIdempotencyKey'
)
    EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [UX_Orders_CustomerId_CheckoutIdempotencyKey]
        ON [dbo].[Orders] ([CustomerId], [CheckoutIdempotencyKey])
        WHERE [CheckoutIdempotencyKey] IS NOT NULL;';

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RefreshTokens]
    (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [TokenHash] varchar(64) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL CONSTRAINT [DF_RefreshTokens_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [ExpiresAtUtc] datetime2(3) NOT NULL,
        [RevokedAtUtc] datetime2(3) NULL,
        [ReplacedByTokenHash] varchar(64) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [CK_RefreshTokens_Expiry] CHECK ([ExpiresAtUtc] > [CreatedAtUtc])
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'dbo.RefreshTokens') AND [name] = N'UX_RefreshTokens_TokenHash'
)
    EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [UX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens] ([TokenHash]);';

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'dbo.RefreshTokens') AND [name] = N'IX_RefreshTokens_UserId_ExpiresAtUtc'
)
    EXEC sys.sp_executesql N'CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAtUtc]
        ON [dbo].[RefreshTokens] ([UserId], [ExpiresAtUtc]);';

COMMIT TRANSACTION;
GO


-- ================================================================
-- Combined source: 18 - Store Contact Information.sql
-- ================================================================
USE [pharma_shope_db];
GO

/* Add optional, per-store contact fields. Store.Address is reused for contact address. */
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Stores', N'ContactPhone') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [ContactPhone] nvarchar(50) NULL;

IF COL_LENGTH(N'dbo.Stores', N'ContactWhatsApp') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [ContactWhatsApp] nvarchar(50) NULL;

IF COL_LENGTH(N'dbo.Stores', N'ContactEmail') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [ContactEmail] nvarchar(256) NULL;

IF COL_LENGTH(N'dbo.Stores', N'WorkingHours') IS NULL
    ALTER TABLE [dbo].[Stores] ADD [WorkingHours] nvarchar(500) NULL;

COMMIT TRANSACTION;
GO

