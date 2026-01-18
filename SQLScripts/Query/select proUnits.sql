DECLARE @PharmacyId INT = 1;
DECLARE @ProductId  INT = 10;

SELECT
    pu.Id                 AS ProductUnitId,
    pu.ProductId,
    pu.SortOrder,

    pu.UnitId,
    u.Code                AS UnitCode,
    u.NameAr              AS UnitNameAr,
    u.NameEn              AS UnitNameEn,

    pu.IsPrimary,
    pu.IsActive,

    pu.ParentProductUnitId,
    parentU.Code          AS ParentUnitCode,
    parentU.NameAr        AS ParentUnitNameAr,
    parentU.NameEn        AS ParentUnitNameEn,

    pu.UnitsPerParent,
    pu.BaseUnitId,
    baseU.Code            AS BaseUnitCode,
    baseU.NameAr          AS BaseUnitNameAr,
    baseU.NameEn          AS BaseUnitNameEn,
    pu.BaseQuantity,

    pu.CurrencyCode,
    pu.CostPrice,
    pu.ListPrice,
    pu.PriceUpdatedAt
FROM dbo.ProductUnits pu
JOIN dbo.Units u
    ON u.Id = pu.UnitId
LEFT JOIN dbo.ProductUnits parentPU
    ON parentPU.Id = pu.ParentProductUnitId
   AND parentPU.DeletedAt IS NULL
LEFT JOIN dbo.Units parentU
    ON parentU.Id = parentPU.UnitId
LEFT JOIN dbo.Units baseU
    ON baseU.Id = pu.BaseUnitId
WHERE
    pu.PharmacyId = @PharmacyId
    AND pu.ProductId = @ProductId
    AND pu.DeletedAt IS NULL
ORDER BY
    pu.IsPrimary DESC,
    pu.SortOrder ASC,
    pu.Id ASC;
