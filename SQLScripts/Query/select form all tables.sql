use pharma_shope_db;
DECLARE @ProductId INT = 10;
DECLARE @StoreId   INT = 1;

select * from ProductUnits
where ProductId =  @ProductId;

select top 3 * from ProductBatches
where ProductId = @ProductId;


select * from dbo.ProductInventory
where ProductId = @ProductId;