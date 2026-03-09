using Contracts;
using Entities.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Product;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly RepositoryContext context;

    public ProductRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }


    public Task<Product?> GetForUpdateAsync(int id, int pharmacyId, CancellationToken ct)
       => _dbSet.FirstOrDefaultAsync(p =>
           p.Id == id &&
           p.PharmacyId == pharmacyId &&
           p.DeletedAt == null, ct);

    public void SetRowVersionOriginalValue(Product entity, byte[] rowVersion)
    {
        _context.Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;
    }
    
    
    public async Task<PagedResult<ProductListItemDto>> SearchAsync(
       int pharmacyId,
       ProductListQueryDto query,
       CancellationToken ct)
    {
        // ----------------------------
        // 1) Normalize paging
        // ----------------------------
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 200 ? 200 : query.PageSize);
        var skip = (page - 1) * pageSize;

        // ----------------------------
        // 2) Base products query (keep it simple)
        // ----------------------------
        IQueryable<Product> products = _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.PharmacyId == pharmacyId);

        // Deleted filters
        if (query.OnlyDeleted)
            products = products.Where(p => p.DeletedAt != null);
        else if (!query.IncludeDeleted)
            products = products.Where(p => p.DeletedAt == null);

        // IDs filters
        if (query.CategoryId.HasValue)
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.CategoryIds is { Count: > 0 })
            products = products.Where(p => query.CategoryIds.Contains(p.CategoryId));

        if (query.BrandId.HasValue)
            products = products.Where(p => p.BrandId == query.BrandId.Value);

        if (query.BrandIds is { Count: > 0 })
            products = products.Where(p => p.BrandId.HasValue && query.BrandIds.Contains(p.BrandId.Value));

        // Flags filters
        if (query.IsActive.HasValue)
            products = products.Where(p => p.IsActive == query.IsActive.Value);

        if (query.RequiresPrescription.HasValue)
            products = products.Where(p => p.RequiresPrescription == query.RequiresPrescription.Value);

        if (query.EarnPoints.HasValue)
            products = products.Where(p => p.EarnPoints == query.EarnPoints.Value);

        if (query.HasExpiry.HasValue)
            products = products.Where(p => p.HasExpiry == query.HasExpiry.Value);

        if (query.AgeRestricted.HasValue)
            products = products.Where(p => p.AgeRestricted == query.AgeRestricted.Value);

        if (query.MinAgeFrom.HasValue)
            products = products.Where(p => p.MinAge.HasValue && p.MinAge.Value >= query.MinAgeFrom.Value);

        if (query.MinAgeTo.HasValue)
            products = products.Where(p => p.MinAge.HasValue && p.MinAge.Value <= query.MinAgeTo.Value);

        if (query.RequiresColdChain.HasValue)
            products = products.Where(p => p.RequiresColdChain == query.RequiresColdChain.Value);

        if (query.ControlledSubstance.HasValue)
            products = products.Where(p => p.ControlledSubstance == query.ControlledSubstance.Value);

        if (query.IsTaxable.HasValue)
            products = products.Where(p => p.IsTaxable == query.IsTaxable.Value);

        if (query.VatRateFrom.HasValue)
            products = products.Where(p => p.VatRate >= query.VatRateFrom.Value);

        if (query.VatRateTo.HasValue)
            products = products.Where(p => p.VatRate <= query.VatRateTo.Value);

        if (!string.IsNullOrWhiteSpace(query.TaxCategoryCode))
        {
            var taxCode = query.TaxCategoryCode.Trim();
            products = products.Where(p => p.TaxCategoryCode != null && p.TaxCategoryCode == taxCode);
        }

        if (query.TrackInventory.HasValue)
            products = products.Where(p => p.TrackInventory == query.TrackInventory.Value);

        if (query.IsFeatured.HasValue)
            products = products.Where(p => p.IsFeatured == query.IsFeatured.Value);

        if (query.AllowSplitSale.HasValue)
            products = products.Where(p => p.AllowSplitSale == query.AllowSplitSale.Value);

        if (query.SplitLevel.HasValue)
            products = products.Where(p => p.SplitLevel.HasValue && p.SplitLevel.Value == query.SplitLevel.Value);

        // Date ranges
        if (query.CreatedFromUtc.HasValue)
            products = products.Where(p => p.CreatedAt >= query.CreatedFromUtc.Value);

        if (query.CreatedToUtc.HasValue)
            products = products.Where(p => p.CreatedAt < query.CreatedToUtc.Value);

        if (query.UpdatedFromUtc.HasValue)
            products = products.Where(p => p.UpdatedAt.HasValue && p.UpdatedAt.Value >= query.UpdatedFromUtc.Value);

        if (query.UpdatedToUtc.HasValue)
            products = products.Where(p => p.UpdatedAt.HasValue && p.UpdatedAt.Value < query.UpdatedToUtc.Value);

        // Exact code filters
        if (!string.IsNullOrWhiteSpace(query.Barcode))
        {
            var barcode = query.Barcode.Trim();
            products = products.Where(p => p.Barcode != null && p.Barcode == barcode);
        }

        if (!string.IsNullOrWhiteSpace(query.InternationalCode))
        {
            var code = query.InternationalCode.Trim();
            products = products.Where(p => p.InternationalCode != null && p.InternationalCode == code);
        }

        if (!string.IsNullOrWhiteSpace(query.StockProductCode))
        {
            var code = query.StockProductCode.Trim();
            products = products.Where(p => p.StockProductCode != null && p.StockProductCode == code);
        }

        // Keyword search
        var keyword = (query.Q ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            products = query.SearchMode switch
            {
                ProductSearchMode.Exact => products.Where(p =>
                    p.Name == keyword ||
                    p.NameEn == keyword ||
                    (p.NormalizedName != null && p.NormalizedName == keyword) ||
                    (p.NormalizedNameEn != null && p.NormalizedNameEn == keyword) ||
                    (p.SearchKeywords != null && p.SearchKeywords.Contains(keyword)) ||
                    (p.Barcode != null && p.Barcode == keyword) ||
                    (p.InternationalCode != null && p.InternationalCode == keyword) ||
                    (p.StockProductCode != null && p.StockProductCode == keyword)
                ),

                ProductSearchMode.StartsWith => products.Where(p =>
                    p.Name.StartsWith(keyword) ||
                    p.NameEn.StartsWith(keyword) ||
                    (p.NormalizedName != null && p.NormalizedName.StartsWith(keyword)) ||
                    (p.NormalizedNameEn != null && p.NormalizedNameEn.StartsWith(keyword)) ||
                    (p.SearchKeywords != null && p.SearchKeywords.Contains(keyword)) ||
                    (p.Barcode != null && p.Barcode.StartsWith(keyword)) ||
                    (p.InternationalCode != null && p.InternationalCode.StartsWith(keyword)) ||
                    (p.StockProductCode != null && p.StockProductCode.StartsWith(keyword))
                ),

                _ => products.Where(p =>
                    p.Name.Contains(keyword) ||
                    p.NameEn.Contains(keyword) ||
                    (p.NormalizedName != null && p.NormalizedName.Contains(keyword)) ||
                    (p.NormalizedNameEn != null && p.NormalizedNameEn.Contains(keyword)) ||
                    (p.SearchKeywords != null && p.SearchKeywords.Contains(keyword)) ||
                    (p.Barcode != null && p.Barcode.Contains(keyword)) ||
                    (p.InternationalCode != null && p.InternationalCode.Contains(keyword)) ||
                    (p.StockProductCode != null && p.StockProductCode.Contains(keyword))
                )
            };
        }

        // ----------------------------
        // 3) Stock aggregation (ONLY if needed)
        //    - No joins with ProductInventory entity results
        //    - Use only needed columns
        // ----------------------------
        var needStockFilter =
            query.InStockOnly ||
            query.MinAvailableQty.HasValue ||
            query.StockStatus != StockStatus.Any ||
            query.StoreId.HasValue;

        var includeStockSummary = query.IncludeStockSummary; // output only

        IQueryable<StockRow>? stockAgg = null;

        if (needStockFilter || includeStockSummary)
        {
            IQueryable<ProductInventory> inv = _context.Set<ProductInventory>()
                .AsNoTracking()
                .Where(x => x.PharmacyId == pharmacyId);

            if (query.StoreId.HasValue)
                inv = inv.Where(x => x.StoreId == query.StoreId.Value);

            // ✅ Important: only QuantityOnHand/ReservedQty/ProductId are used
            stockAgg = inv
                .GroupBy(x => x.ProductId)
                .Select(g => new StockRow
                {
                    ProductId = g.Key,
                    AvailableQty =
                        (g.Sum(x => (int?)x.QuantityOnHand) ?? 0) -
                        (g.Sum(x => (int?)x.ReservedQty) ?? 0)
                });

            // Apply stock filters by producing product ids set
            IQueryable<int> stockFilteredIds = stockAgg.Select(s => s.ProductId);

            if (query.InStockOnly)
                stockFilteredIds = stockAgg.Where(s => s.AvailableQty > 0).Select(s => s.ProductId);

            if (query.MinAvailableQty.HasValue)
            {
                var min = query.MinAvailableQty.Value;
                stockFilteredIds = stockAgg.Where(s => s.AvailableQty >= min).Select(s => s.ProductId);
            }

            if (query.StockStatus != StockStatus.Any)
            {
                stockFilteredIds = query.StockStatus switch
                {
                    StockStatus.InStock => stockAgg.Where(s => s.AvailableQty > 0).Select(s => s.ProductId),
                    StockStatus.OutOfStock => stockAgg.Where(s => s.AvailableQty <= 0).Select(s => s.ProductId),

                    // ✅ LowStock intentionally NOT handled here (needs MinStockLevel join logic)
                    // Keep it as "Any" for list to avoid unstable logic in list endpoint.
                    StockStatus.LowStock => stockFilteredIds,

                    _ => stockFilteredIds
                };
            }

            if (needStockFilter)
                products = products.Where(p => stockFilteredIds.Contains(p.Id));
        }

        // ----------------------------
        // 4) Count
        // ----------------------------
        var totalCount = await products.CountAsync(ct);

        if (totalCount == 0)
        {
            return new PagedResult<ProductListItemDto>
            {
                Items = new List<ProductListItemDto>(),
                TotalCount = 0
            };
        }

        // ----------------------------
        // 5) Sorting (on Products)
        // ----------------------------
        products = ApplySorting(products, query.SortBy, query.SortDir);

        // ----------------------------
        // 6) Page projection (product first)
        // ----------------------------
        var pageRows = await products
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.PharmacyId,
                p.CategoryId,
                p.BrandId,
                p.Name,
                p.NameEn,
                p.Barcode,
                p.InternationalCode,
                p.StockProductCode,
                p.IsActive,
                p.RequiresPrescription,
                p.IsFeatured,
                p.TrackInventory,
                p.IsTaxable,
                p.VatRate,
                p.CreatedAt,
                p.UpdatedAt,
                p.DeletedAt,

                // ✅ image: correlated subquery (only when requested)
                PrimaryImageUrl = query.IncludePrimaryImage
                    ? _context.Set<ProductImage>()
                        .AsNoTracking()
                        .Where(i => i.PharmacyId == pharmacyId
                                    && i.ProductId == p.Id
                                    && i.DeletedAt == null
                                    && i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync(ct);

        // ----------------------------
        // 7) Stock summary for returned page (separate query; safe)
        // ----------------------------
        Dictionary<int, int> availableByProductId = new();

        if ((includeStockSummary) && pageRows.Count > 0)
        {
            // stockAgg is IQueryable grouped; we can re-query for page product ids only
            var ids = pageRows.Select(x => x.Id).ToList();

            var stockForPage = await _context.Set<ProductInventory>()
                .AsNoTracking()
                .Where(x => x.PharmacyId == pharmacyId && ids.Contains(x.ProductId))
                .Where(x => !query.StoreId.HasValue || x.StoreId == query.StoreId.Value)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvailableQty =
                        (g.Sum(x => (int?)x.QuantityOnHand) ?? 0) -
                        (g.Sum(x => (int?)x.ReservedQty) ?? 0)
                })
                .ToListAsync(ct);

            availableByProductId = stockForPage.ToDictionary(x => x.ProductId, x => x.AvailableQty);
        }

        // ----------------------------
        // 8) Map to DTO in memory (no EF materialization risks)
        // ----------------------------
        var items = pageRows.Select(x =>
        {
            int? available = null;

            if (includeStockSummary)
            {
                available = availableByProductId.TryGetValue(x.Id, out var v) ? v : 0;
            }

            return new ProductListItemDto
            {
                Id = x.Id,
                PharmacyId = x.PharmacyId,
                CategoryId = x.CategoryId,
                BrandId = x.BrandId,

                Name = x.Name,
                NameEn = x.NameEn,

                Barcode = x.Barcode,
                InternationalCode = x.InternationalCode,
                StockProductCode = x.StockProductCode,

                IsActive = x.IsActive,
                RequiresPrescription = x.RequiresPrescription,
                IsFeatured = x.IsFeatured,
                TrackInventory = x.TrackInventory,
                IsTaxable = x.IsTaxable,
                VatRate = x.VatRate,

                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,

                PrimaryImageUrl = x.PrimaryImageUrl,

                AvailableQty = available,
                IsInStock = includeStockSummary ? (available > 0) : null,
                IsLowStock = null // intentionally omitted from list endpoint
            };
        }).ToList();

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Convert stock from a parent ProductUnit to a direct child ProductUnit (Open/Convert).
    /// IMPORTANT: This method assumes the caller already started a transaction if needed.
    /// </summary>
    public async Task<(OpenBoxResultDto? Result, Dictionary<string, string[]>? FieldErrors, string? ErrorMessage)>
        ConvertUnitsAsync(int pharmacyId, int productId, OpenBoxDto dto, CancellationToken ct)
    {
        // 1) Basic input guards (keep it minimal here)
        if (dto.StoreId <= 0)
            return (null, new() { ["StoreId"] = new[] { "StoreId must be > 0" } }, null);

        if (dto.FromProductUnitId <= 0 || dto.ToProductUnitId <= 0)
            return (null, new()
            {
                ["FromProductUnitId"] = new[] { "FromProductUnitId must be > 0" },
                ["ToProductUnitId"] = new[] { "ToProductUnitId must be > 0" }
            }, null);

        if (dto.FromProductUnitId == dto.ToProductUnitId)
            return (null, new() { ["ToProductUnitId"] = new[] { "ToProductUnitId must be different" } }, null);

        if (dto.FromQtyToConvert <= 0)
            return (null, new() { ["FromQtyToConvert"] = new[] { "FromQtyToConvert must be > 0" } }, null);

        // 2) Load units (From + To) and validate direct parent-child conversion
        var units = await _context.Set<ProductUnit>()
            .AsNoTracking()
            .Where(u => u.PharmacyId == pharmacyId
                        && u.ProductId == productId
                        && u.DeletedAt == null
                        && u.IsActive
                        && (u.Id == dto.FromProductUnitId || u.Id == dto.ToProductUnitId))
            .ToListAsync(ct);

        var fromUnit = units.SingleOrDefault(x => x.Id == dto.FromProductUnitId);
        var toUnit = units.SingleOrDefault(x => x.Id == dto.ToProductUnitId);

        if (fromUnit is null)
            return (null, new() { ["FromProductUnitId"] = new[] { "FromProductUnitId is not valid for this product" } }, null);

        if (toUnit is null)
            return (null, new() { ["ToProductUnitId"] = new[] { "ToProductUnitId is not valid for this product" } }, null);

        if (toUnit.ParentProductUnitId != fromUnit.Id)
            return (null, new()
            {
                ["ToProductUnitId"] = new[] { "Invalid conversion: To unit must be a direct child of From unit" }
            }, null);

        if (toUnit.UnitsPerParent is null || toUnit.UnitsPerParent <= 0)
            return (null, new() { ["UnitsPerParent"] = new[] { "UnitsPerParent must be configured (> 0) for the To unit" } }, null);

        // UnitsPerParent is DECIMAL(18,3). For now we require it to be whole-number for inventory ints.
        if (toUnit.UnitsPerParent % 1 != 0)
            return (null, new() { ["UnitsPerParent"] = new[] { "UnitsPerParent must be an integer value for this conversion" } }, null);

        var unitsPerFrom = (int)toUnit.UnitsPerParent.Value;
        var toCreated = dto.FromQtyToConvert * unitsPerFrom;

        // 3) Pick "from" batch (FEFO or explicit)
        IQueryable<ProductBatch> fromBatchQuery = _context.Set<ProductBatch>()
            .AsTracking()
            .Where(b => b.PharmacyId == pharmacyId
                        && b.StoreId == dto.StoreId
                        && b.ProductId == productId
                        && b.ProductUnitId == fromUnit.Id
                        && b.DeletedAt == null
                        && b.IsActive);

        ProductBatch? fromBatch;

        if (dto.FromBatchId.HasValue)
        {
            fromBatch = await fromBatchQuery.SingleOrDefaultAsync(b => b.Id == dto.FromBatchId.Value, ct);
            if (fromBatch is null)
                return (null, new() { ["FromBatchId"] = new[] { "Batch not found for the From unit" } }, null);
        }
        else
        {
            // FEFO: earliest expiry first; NULL expiry goes last
            fromBatch = await fromBatchQuery
                .Where(b => b.QuantityOnHand > 0)
                .OrderBy(b => b.ExpirationDate == null)
                .ThenBy(b => b.ExpirationDate)
                .ThenBy(b => b.Id)
                .FirstOrDefaultAsync(ct);

            if (fromBatch is null)
                return (null, null, "No available batch to convert from (check batches/quantities).");
        }

        if (fromBatch.QuantityOnHand < dto.FromQtyToConvert)
            return (null, new() { ["FromQtyToConvert"] = new[] { "Not enough quantity in the selected batch" } }, null);

        // 4) Inventory rows (From must exist; To can be created)
        var invFrom = await _context.Set<ProductInventory>()
            .AsTracking()
            .SingleOrDefaultAsync(x =>
                x.PharmacyId == pharmacyId
                && x.StoreId == dto.StoreId
                && x.ProductId == productId
                && x.ProductUnitId == fromUnit.Id,
                ct);

        if (invFrom is null)
            return (null, null, "From inventory row not found. Receive stock first.");

        if (invFrom.QuantityOnHand < dto.FromQtyToConvert)
            return (null, new() { ["FromQtyToConvert"] = new[] { "Not enough quantity in inventory" } }, null);

        var invTo = await _context.Set<ProductInventory>()
            .AsTracking()
            .SingleOrDefaultAsync(x =>
                x.PharmacyId == pharmacyId
                && x.StoreId == dto.StoreId
                && x.ProductId == productId
                && x.ProductUnitId == toUnit.Id,
                ct);

        if (invTo is null)
        {
            invTo = new ProductInventory
            {
                PharmacyId = pharmacyId,
                StoreId = dto.StoreId,
                ProductId = productId,
                ProductUnitId = toUnit.Id,
                QuantityOnHand = 0,
                ReservedQty = 0,
                LastStockUpdateAt = DateTime.UtcNow
            };
            _context.Set<ProductInventory>().Add(invTo);
        }

        // 5) Upsert "to" batch using same BatchNumber + Expiry
        var batchNumber = fromBatch.BatchNumber;
        var expiry = fromBatch.ExpirationDate;

        var toBatch = await _context.Set<ProductBatch>()
            .AsTracking()
            .SingleOrDefaultAsync(b =>
                b.PharmacyId == pharmacyId
                && b.StoreId == dto.StoreId
                && b.ProductId == productId
                && b.ProductUnitId == toUnit.Id
                && b.BatchNumber == batchNumber
                && b.DeletedAt == null,
                ct);

        if (toBatch is not null)
        {
            // important: keep expiry consistent
            if (toBatch.ExpirationDate != expiry)
                return (null, null, "Data conflict: existing To batch has different ExpirationDate for the same BatchNumber.");
        }
        else
        {
            toBatch = new ProductBatch
            {
                PharmacyId = pharmacyId,
                StoreId = dto.StoreId,
                ProductId = productId,
                ProductUnitId = toUnit.Id,

                BatchNumber = batchNumber,
                ExpirationDate = expiry,

                ReceivedAt = DateTime.UtcNow,
                QuantityReceived = 0,
                QuantityOnHand = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<ProductBatch>().Add(toBatch);
        }

        // 6) Apply changes (no SaveChanges here if you prefer UnitOfWork.CompleteAsync)
        fromBatch.QuantityOnHand -= dto.FromQtyToConvert;

        toBatch.QuantityOnHand += toCreated;
        toBatch.QuantityReceived += toCreated; // internal conversion inflow (optional)

        invFrom.QuantityOnHand -= dto.FromQtyToConvert;
        invFrom.LastStockUpdateAt = DateTime.UtcNow;

        invTo.QuantityOnHand += toCreated;
        invTo.LastStockUpdateAt = DateTime.UtcNow;

        // TODO (future): write Audit/Ledger here (conversion out + conversion in) using dto.Reason + currentUser

        return (new OpenBoxResultDto
        {
            ProductId = productId,
            StoreId = dto.StoreId,

            FromProductUnitId = fromUnit.Id,
            ToProductUnitId = toUnit.Id,

            FromQtyConverted = dto.FromQtyToConvert,
            ToQtyCreated = toCreated,

            UsedFromBatchId = fromBatch.Id,
            BatchNumber = fromBatch.BatchNumber,
            ExpirationDate = fromBatch.ExpirationDate,

            FromQtyOnHandAfter = invFrom.QuantityOnHand,
            ToQtyOnHandAfter = invTo.QuantityOnHand
        }, null, null);
    }



    public async Task<(ReceiveStockResultDto? Result, Dictionary<string, string[]>? FieldErrors, string? ErrorMessage)>
        ReceiveStockAsync(int pharmacyId, int productId, ReceiveStockDto dto, CancellationToken ct)
    {
        // 1) Basic validation (خفيف)
        if (dto.StoreId <= 0)
            return (null, new() { ["StoreId"] = new[] { "StoreId must be > 0" } }, null);

        if (dto.ProductUnitId <= 0)
            return (null, new() { ["ProductUnitId"] = new[] { "ProductUnitId must be > 0" } }, null);

        if (dto.Quantity <= 0)
            return (null, new() { ["Quantity"] = new[] { "Quantity must be > 0" } }, null);

        if (string.IsNullOrWhiteSpace(dto.BatchNumber))
            return (null, new() { ["BatchNumber"] = new[] { "BatchNumber is required" } }, null);

        if (dto.BatchNumber.Length > 80)
            return (null, new() { ["BatchNumber"] = new[] { "BatchNumber max length is 80" } }, null);

        if (dto.Reason is not null && dto.Reason.Length > 500)
            return (null, new() { ["Reason"] = new[] { "Reason max length is 500" } }, null);

        // 2) Ensure product unit belongs to this product & pharmacy
        var pu = await _context.Set<ProductUnit>()
            .AsNoTracking()
            .SingleOrDefaultAsync(u =>
                u.PharmacyId == pharmacyId
                && u.ProductId == productId
                && u.Id == dto.ProductUnitId
                && u.DeletedAt == null
                && u.IsActive,
                ct);

        if (pu is null)
            return (null, new() { ["ProductUnitId"] = new[] { "Invalid ProductUnitId for this product" } }, null);

        // 3) Product flags (HasExpiry)
        var product = await _context.Set<Product>()
            .AsNoTracking()
            .SingleOrDefaultAsync(p =>
                p.Id == productId
                && p.PharmacyId == pharmacyId
                && p.DeletedAt == null,
                ct);

        if (product is null)
            return (null, null, "Product not found");

        if (product.HasExpiry && dto.ExpirationDate is null)
            return (null, new() { ["ExpirationDate"] = new[] { "ExpirationDate is required for expirable products" } }, null);

        // (اختياري) منع استلام صلاحية منتهية
        if (dto.ExpirationDate is not null && dto.ExpirationDate.Value.Date < DateTime.UtcNow.Date)
            return (null, new() { ["ExpirationDate"] = new[] { "ExpirationDate cannot be in the past" } }, null);

        // 4) Upsert Batch (tracking)
        // حسب unique index: (StoreId, ProductUnitId, BatchNumber) WHERE DeletedAt IS NULL
        var batch = await _context.Set<ProductBatch>()
            .AsTracking()
            .SingleOrDefaultAsync(b =>
                b.PharmacyId == pharmacyId
                && b.StoreId == dto.StoreId
                && b.ProductId == productId
                && b.ProductUnitId == dto.ProductUnitId
                && b.BatchNumber == dto.BatchNumber
                && b.DeletedAt == null,
                ct);

        if (batch is null)
        {
            batch = new ProductBatch
            {
                PharmacyId = pharmacyId,
                StoreId = dto.StoreId,
                ProductId = productId,
                ProductUnitId = dto.ProductUnitId,

                BatchNumber = dto.BatchNumber.Trim(),
                ExpirationDate = dto.ExpirationDate?.Date,

                ReceivedAt = DateTime.UtcNow,
                QuantityReceived = 0,
                QuantityOnHand = 0,
                CostPrice = dto.CostPrice,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<ProductBatch>().Add(batch);
        }
        else
        {
            // لو المنتج له صلاحية، نضمن عدم تعارض صلاحية نفس BatchNumber
            if (product.HasExpiry)
            {
                var existing = batch.ExpirationDate?.Date;
                var incoming = dto.ExpirationDate?.Date;

                if (existing != incoming)
                    return (null, null, "Data conflict: same BatchNumber exists with a different ExpirationDate.");
            }

            // تحديث CostPrice لو أرسل المستخدم قيمة (اختياري)
            if (dto.CostPrice.HasValue)
                batch.CostPrice = dto.CostPrice;
        }

        // 5) Upsert Inventory row (tracking)
        // PK: (StoreId, ProductUnitId)
        var inv = await _context.Set<ProductInventory>()
            .AsTracking()
            .SingleOrDefaultAsync(x =>
                x.PharmacyId == pharmacyId
                && x.StoreId == dto.StoreId
                && x.ProductUnitId == dto.ProductUnitId,
                ct);

        if (inv is null)
        {
            inv = new ProductInventory
            {
                PharmacyId = pharmacyId,
                StoreId = dto.StoreId,
                ProductId = productId,
                ProductUnitId = dto.ProductUnitId,
                QuantityOnHand = 0,
                ReservedQty = 0,
                LastStockUpdateAt = DateTime.UtcNow
            };

            _context.Set<ProductInventory>().Add(inv);
        }
        else
        {
            // safety: تأكد أن نفس ProductUnitId لا يستخدم مع ProductId مختلف (يجب ألا يحدث)
            if (inv.ProductId != productId)
                return (null, null, "Data conflict: Inventory row ProductId does not match this product.");
        }

        // 6) Apply quantities
        batch.QuantityReceived += dto.Quantity;
        batch.QuantityOnHand += dto.Quantity;

        inv.QuantityOnHand += dto.Quantity;
        inv.LastStockUpdateAt = DateTime.UtcNow;

        // TODO (future): write Audit/Ledger here (Receive movement) using dto.Reason + currentUser

        return (new ReceiveStockResultDto
        {
            ProductId = productId,
            StoreId = dto.StoreId,
            ProductUnitId = dto.ProductUnitId,

            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            ExpirationDate = batch.ExpirationDate,

            QuantityReceived = dto.Quantity,
            BatchQtyOnHandAfter = batch.QuantityOnHand,
            InventoryQtyOnHandAfter = inv.QuantityOnHand
        }, null, null);
    }


    // ------------------------

    public async Task<(StockAdjustmentResultDto? Result, Dictionary<string, string[]>? FieldErrors, string? ErrorMessage)>
        AdjustStockAsync(int pharmacyId, int productId, StockAdjustmentDto dto, CancellationToken ct)
    {
        // 1) Basic validation (خفيف)
        var errors = new Dictionary<string, string[]>();

        if (dto.StoreId <= 0) errors["StoreId"] = new[] { "StoreId must be > 0" };
        if (dto.ProductUnitId <= 0) errors["ProductUnitId"] = new[] { "ProductUnitId must be > 0" };
        if (dto.CountedQty < 0) errors["CountedQty"] = new[] { "CountedQty must be >= 0" };

        var hasBatchId = dto.BatchId.HasValue;
        var hasBatchNumber = !string.IsNullOrWhiteSpace(dto.BatchNumber);

        if (!hasBatchId && !hasBatchNumber)
            errors["BatchId"] = new[] { "Provide BatchId OR BatchNumber (and ExpirationDate if needed)" };

        if (hasBatchNumber && dto.BatchNumber!.Length > 80)
            errors["BatchNumber"] = new[] { "BatchNumber max length is 80" };

        if (dto.Reason is not null && dto.Reason.Length > 500)
            errors["Reason"] = new[] { "Reason max length is 500" };

        if (errors.Count > 0)
            return (null, errors, null);

        // 2) Ensure product exists
        var product = await _context.Set<Product>()
            .AsNoTracking()
            .SingleOrDefaultAsync(p =>
                p.Id == productId
                && p.PharmacyId == pharmacyId
                && p.DeletedAt == null,
                ct);

        if (product is null)
            return (null, null, "Product not found");

        // 3) Ensure ProductUnit belongs to product
        var pu = await _context.Set<ProductUnit>()
            .AsNoTracking()
            .SingleOrDefaultAsync(u =>
                u.PharmacyId == pharmacyId
                && u.ProductId == productId
                && u.Id == dto.ProductUnitId
                && u.DeletedAt == null
                && u.IsActive,
                ct);

        if (pu is null)
            return (null, new() { ["ProductUnitId"] = new[] { "Invalid ProductUnitId for this product" } }, null);

        // 4) Locate batch (tracking) - MUST exist for adjustment
        ProductBatch? batch;

        if (dto.BatchId.HasValue)
        {
            batch = await _context.Set<ProductBatch>()
                .AsTracking()
                .SingleOrDefaultAsync(b =>
                    b.Id == dto.BatchId.Value
                    && b.PharmacyId == pharmacyId
                    && b.StoreId == dto.StoreId
                    && b.ProductId == productId
                    && b.ProductUnitId == dto.ProductUnitId
                    && b.DeletedAt == null,
                    ct);

            if (batch is null)
                return (null, new() { ["BatchId"] = new[] { "Batch not found" } }, null);
        }
        else
        {
            // If product has expiry, require expiration date for precise match
            if (product.HasExpiry && dto.ExpirationDate is null)
                return (null, new() { ["ExpirationDate"] = new[] { "ExpirationDate is required for expirable products" } }, null);

            var exp = dto.ExpirationDate?.Date;

            batch = await _context.Set<ProductBatch>()
                .AsTracking()
                .SingleOrDefaultAsync(b =>
                    b.PharmacyId == pharmacyId
                    && b.StoreId == dto.StoreId
                    && b.ProductId == productId
                    && b.ProductUnitId == dto.ProductUnitId
                    && b.BatchNumber == dto.BatchNumber!.Trim()
                    && b.DeletedAt == null
                    && (!product.HasExpiry || b.ExpirationDate == exp),
                    ct);

            if (batch is null)
                return (null, new() { ["BatchNumber"] = new[] { "Batch not found for given BatchNumber/ExpirationDate" } }, null);
        }

        // 5) Inventory row (tracking) - must exist; if not, create it
        var inv = await _context.Set<ProductInventory>()
            .AsTracking()
            .SingleOrDefaultAsync(x =>
                x.PharmacyId == pharmacyId
                && x.StoreId == dto.StoreId
                && x.ProductUnitId == dto.ProductUnitId,
                ct);

        if (inv is null)
        {
            inv = new ProductInventory
            {
                PharmacyId = pharmacyId,
                StoreId = dto.StoreId,
                ProductId = productId,
                ProductUnitId = dto.ProductUnitId,
                QuantityOnHand = 0,
                ReservedQty = 0,
                LastStockUpdateAt = DateTime.UtcNow
            };
            _context.Set<ProductInventory>().Add(inv);
        }
        else
        {
            if (inv.ProductId != productId)
                return (null, null, "Data conflict: Inventory row ProductId does not match this product.");
        }

        // 6) Apply adjustment (absolute counted qty on batch)
        var oldBatchQty = batch.QuantityOnHand;
        var newBatchQty = dto.CountedQty;
        var delta = newBatchQty - oldBatchQty;

        // Important: do not allow counted qty below reserved (if you later reserve per batch)
        // currently reserved is at inventory-level only, so we just guard inventory not negative after applying delta.

        batch.QuantityOnHand = newBatchQty;
        batch.IsActive = newBatchQty > 0; // optional behavior

        if (dto.CostPrice.HasValue)
            batch.CostPrice = dto.CostPrice;

        // Update inventory snapshot by same delta
        var newInvQty = inv.QuantityOnHand + delta;
        if (newInvQty < 0)
            return (null, null, "Adjustment would make inventory negative. Check data consistency.");

        inv.QuantityOnHand = newInvQty;
        inv.LastStockUpdateAt = DateTime.UtcNow;

        // TODO (future): write Audit/Ledger here (StockAdjustment movement) using dto.Reason + currentUser

        return (new StockAdjustmentResultDto
        {
            ProductId = productId,
            StoreId = dto.StoreId,
            ProductUnitId = dto.ProductUnitId,

            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            ExpirationDate = batch.ExpirationDate,

            OldBatchQty = oldBatchQty,
            NewBatchQty = newBatchQty,
            DeltaQty = delta,

            InventoryQtyOnHandAfter = inv.QuantityOnHand
        }, null, null);
    }



    private static IQueryable<Product> ApplySorting(IQueryable<Product> q,ProductSortBy sortBy,SortDirection dir)
    {
        var desc = dir == SortDirection.Desc;

        return sortBy switch
        {
            ProductSortBy.Name => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            ProductSortBy.NameEn => desc ? q.OrderByDescending(x => x.NameEn) : q.OrderBy(x => x.NameEn),
            ProductSortBy.UpdatedAt => desc
                ? q.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                : q.OrderBy(x => x.UpdatedAt ?? x.CreatedAt),
            ProductSortBy.Id => desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id),
            _ => desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt)
        };
    }

    private sealed class StockRow
    {
        public int ProductId { get; init; }
        public int AvailableQty { get; init; }
    }
}




