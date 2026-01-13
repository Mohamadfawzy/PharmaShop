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

    private static IQueryable<Product> ApplySorting(
        IQueryable<Product> q,
        ProductSortBy sortBy,
        SortDirection dir)
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




