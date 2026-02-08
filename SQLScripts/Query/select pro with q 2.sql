USE pharma_shope_db;
DECLARE @PharmacyId INT = 1;
DECLARE @ProductId  INT = 1;

SELECT
    pu.Id                 AS ProductUnitId,
    pu.ProductId,
    pu.SortOrder,

    pu.UnitId,
    u.Code                AS UnitCode,
    u.NameAr              AS UnitNameAr,
    u.NameEn              AS UnitNameEn,

	    -- 🔹 الكمية المتاحة لكل وحدة
    ISNULL(SUM(pi.QuantityOnHand), 0) AS AvailableQuantity,


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

LEFT JOIN dbo.ProductInventory pi
    ON pi.PharmacyId = pu.PharmacyId
   AND pi.ProductId  = pu.ProductId
   AND pi.ProductUnitId = pu.Id

WHERE
    pu.PharmacyId = @PharmacyId
    AND pu.ProductId = @ProductId
    AND pu.DeletedAt IS NULL

GROUP BY
    pu.Id,
    pu.ProductId,
    pu.SortOrder,
    pu.UnitId,
    u.Code, u.NameAr, u.NameEn,
    pu.IsPrimary,
    pu.IsActive,
    pu.ParentProductUnitId,
    parentU.Code, parentU.NameAr, parentU.NameEn,
    pu.UnitsPerParent,
    pu.BaseUnitId,
    baseU.Code, baseU.NameAr, baseU.NameEn,
    pu.BaseQuantity,
    pu.CurrencyCode,
    pu.CostPrice,
    pu.ListPrice,
    pu.PriceUpdatedAt

ORDER BY
    pu.IsPrimary DESC,
    pu.SortOrder ASC,
    pu.Id ASC;
