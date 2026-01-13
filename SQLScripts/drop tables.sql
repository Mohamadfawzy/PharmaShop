USE pharma_shope_db;
GO

/*========================================================
  DROP TABLES (Safe order to respect FK relationships)
  - Child tables first, then parents
  - Idempotent (won't fail if table doesn't exist)
========================================================*/

-- =========================
-- audit schema tables
-- =========================
IF OBJECT_ID(N'audit.ProductAuditLog', N'U') IS NOT NULL
    DROP TABLE audit.ProductAuditLog;
GO

IF OBJECT_ID(N'audit.CategoryAuditLog', N'U') IS NOT NULL
    DROP TABLE audit.CategoryAuditLog;
GO

-- =========================
-- Loyalty / Points
-- =========================
IF OBJECT_ID(N'dbo.PointsTransactions', N'U') IS NOT NULL
    DROP TABLE dbo.PointsTransactions;
GO

IF OBJECT_ID(N'dbo.PointSettings', N'U') IS NOT NULL
    DROP TABLE dbo.PointSettings;
GO

-- =========================
-- Promotions
-- =========================
IF OBJECT_ID(N'dbo.PromotionProducts', N'U') IS NOT NULL
    DROP TABLE dbo.PromotionProducts;
GO

IF OBJECT_ID(N'dbo.PromotionCategories', N'U') IS NOT NULL
    DROP TABLE dbo.PromotionCategories;
GO

IF OBJECT_ID(N'dbo.Promotions', N'U') IS NOT NULL
    DROP TABLE dbo.Promotions;
GO

-- =========================
-- Tags
-- =========================
IF OBJECT_ID(N'dbo.ProductTags', N'U') IS NOT NULL
    DROP TABLE dbo.ProductTags;
GO

IF OBJECT_ID(N'dbo.Tags', N'U') IS NOT NULL
    DROP TABLE dbo.Tags;
GO

-- =========================
-- Product related
-- =========================
IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NOT NULL
    DROP TABLE dbo.ProductImages;
GO

IF OBJECT_ID(N'dbo.ProductBatches', N'U') IS NOT NULL
    DROP TABLE dbo.ProductBatches;
GO

IF OBJECT_ID(N'dbo.ProductInventory', N'U') IS NOT NULL
    DROP TABLE dbo.ProductInventory;
GO

IF OBJECT_ID(N'dbo.ProductUnits', N'U') IS NOT NULL
    DROP TABLE dbo.ProductUnits;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

-- =========================
-- Stores / Brands / Units
-- =========================
IF OBJECT_ID(N'dbo.Stores', N'U') IS NOT NULL
    DROP TABLE dbo.Stores;
GO

IF OBJECT_ID(N'dbo.Brands', N'U') IS NOT NULL
    DROP TABLE dbo.Brands;
GO

IF OBJECT_ID(N'dbo.Units', N'U') IS NOT NULL
    DROP TABLE dbo.Units;
GO

-- =========================
-- Categories (self-referencing)
-- =========================
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL
    DROP TABLE dbo.Categories;
GO

-- =========================
-- Persons (Customers/Addresses/Pharmacists)
-- =========================
IF OBJECT_ID(N'dbo.CustomerAddresses', N'U') IS NOT NULL
    DROP TABLE dbo.CustomerAddresses;
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
    DROP TABLE dbo.Customers;
GO

IF OBJECT_ID(N'dbo.Pharmacists', N'U') IS NOT NULL
    DROP TABLE dbo.Pharmacists;
GO

-- =========================
-- Misc
-- =========================
IF OBJECT_ID(N'dbo.LoginAudits', N'U') IS NOT NULL
    DROP TABLE dbo.LoginAudits;
GO

-- =========================
-- Pharmacies (parent for most)
-- =========================
IF OBJECT_ID(N'dbo.Pharmacies', N'U') IS NOT NULL
    DROP TABLE dbo.Pharmacies;
GO

-- =========================
-- Identity tables (ASP.NET Core Identity)
-- =========================
IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserTokens;
GO

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserLogins;
GO

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserClaims;
GO

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserRoles;
GO

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetRoleClaims;
GO

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetUsers;
GO

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL
    DROP TABLE dbo.AspNetRoles;
GO

-- =========================
-- EF Migrations History
-- =========================
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
    DROP TABLE dbo.__EFMigrationsHistory;
GO

-- =========================
-- Drop schema "audit" (after dropping its tables)
-- =========================
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'audit')
BEGIN
    -- will succeed only if schema is empty
    EXEC(N'DROP SCHEMA audit;');
END
GO
