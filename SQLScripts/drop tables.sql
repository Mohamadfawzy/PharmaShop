SET NOCOUNT ON;
GO

-- =====================================
-- 1) Tables with no children (Leaf nodes)
-- =====================================

DROP TABLE IF EXISTS PromoCodeUsages;
DROP TABLE IF EXISTS PromotionUsage;

DROP TABLE IF EXISTS PromotionCategories;
DROP TABLE IF EXISTS PromotionProducts;

DROP TABLE IF EXISTS ProductTags;
DROP TABLE IF EXISTS ProductImages;

DROP TABLE IF EXISTS PrescriptionItems;
DROP TABLE IF EXISTS PrescriptionImages;

DROP TABLE IF EXISTS SalesDetailsReturns;
DROP TABLE IF EXISTS SalesDetails;

DROP TABLE IF EXISTS CustomerAddresses;
DROP TABLE IF EXISTS CustomerPointsHistory;

GO

-- =====================================
-- 2) Intermediate dependency tables
-- =====================================

DROP TABLE IF EXISTS PromoCodes;
DROP TABLE IF EXISTS Promotions;

DROP TABLE IF EXISTS SalesHeaderReturns;
DROP TABLE IF EXISTS SalesHeader;

DROP TABLE IF EXISTS Prescriptions;

GO

-- =====================================
-- 3) Core business tables
-- =====================================
DROP TABLE IF EXISTS ProductImages
DROP TABLE IF EXISTS ProductBatches
DROP TABLE IF EXISTS ProductInventory 
DROP TABLE IF EXISTS ProductUnits
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Tags;
--DROP TABLE IF EXISTS [audit].[CategoryAuditLog]
-- DROP TABLE IF EXISTS Categories;

GO

-- =====================================
-- 4) Person / Account related
-- =====================================

DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS Pharmacists;

DROP TABLE IF EXISTS Pharmacies;

GO
