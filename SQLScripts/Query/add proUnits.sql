DECLARE @PharmacyId INT = 1;
DECLARE @ProductId  INT = 11;

DECLARE @UnitBoxId   INT = (SELECT Id FROM dbo.Units WHERE Code = 'BOX');
DECLARE @UnitStripId INT = (SELECT Id FROM dbo.Units WHERE Code = 'STRIP');

-- Parent (BOX)
INSERT INTO dbo.ProductUnits
(
    PharmacyId, ProductId, UnitId, SortOrder,
    CurrencyCode, ListPrice,CostPrice,
    IsPrimary, IsActive, PriceUpdatedAt
)
VALUES
(
    @PharmacyId, @ProductId, @UnitBoxId, 
	1, -- SortOrder
    'EGP', 120.00,100.00,
    1, -- IsPrimary
	1,-- IsActive
	SYSUTCDATETIME()
);

DECLARE @ParentProductUnitId INT = SCOPE_IDENTITY();

-- Child (STRIP)
INSERT INTO dbo.ProductUnits
(
    PharmacyId, ProductId, UnitId, SortOrder,
    ParentProductUnitId, UnitsPerParent,
    CurrencyCode, ListPrice,
    IsPrimary, IsActive, PriceUpdatedAt
)
VALUES
(
    @PharmacyId, @ProductId, @UnitStripId,
	2, -- SortOrder
    @ParentProductUnitId,
	3, -- UnitsPerParent
    'EGP', 45.00,
    0, -- IsPrimary
	1,-- IsActive
	SYSUTCDATETIME()
);
