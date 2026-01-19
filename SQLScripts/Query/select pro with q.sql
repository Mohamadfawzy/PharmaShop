use pharma_shope_db;
DECLARE @ProductId INT = 10;
DECLARE @StoreId   INT = 1;

SELECT
    p.Id                     AS ProductId,
    p.Name                   AS ProductName,

    pu.Id                    AS ProductUnitId,
    u.NameAr                  AS UnitName,
    pu.ParentProductUnitId,
    pu.UnitsPerParent,

    ISNULL(pi.QuantityOnHand, 0) AS InventoryQty,

    b.Id                     AS BatchId,
    b.BatchNumber,
    b.ExpirationDate,
    b.QuantityOnHand         AS BatchQty
FROM dbo.Products p
INNER JOIN dbo.ProductUnits pu
    ON pu.ProductId = p.Id
   AND pu.DeletedAt IS NULL
LEFT JOIN dbo.Units u
    ON u.Id = pu.UnitId
LEFT JOIN dbo.ProductInventory pi
    ON pi.ProductId = p.Id
   AND pi.ProductUnitId = pu.Id
   AND pi.StoreId = @StoreId
LEFT JOIN dbo.ProductBatches b
    ON b.ProductId = p.Id
   AND b.ProductUnitId = pu.Id
   AND b.StoreId = @StoreId
   AND b.DeletedAt IS NULL
WHERE p.Id = @ProductId
  AND p.DeletedAt IS NULL
ORDER BY
    pu.ParentProductUnitId,
    pu.SortOrder,
    b.ExpirationDate;



