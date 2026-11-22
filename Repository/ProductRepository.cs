using Contracts;
using Entities.Models;
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

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchQueryParams q, CancellationToken ct)
    {
        // 1) Normalize inputs
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
        if (pageSize > 200) pageSize = 200;

        var skip = (page - 1) * pageSize;

        var term = string.IsNullOrWhiteSpace(q.Q) ? null : q.Q.Trim();
        var tagIds = (q.TagIds ?? new List<int>()).Where(x => x > 0).Distinct().ToList();
        var now = DateTime.UtcNow;

        // 2) Base query (scope + tracking)
        IQueryable<Product> query = context.Products.AsNoTracking().Where(p => p.DeletedAt == null);

        // Store scope (recommended)
        if (q.StoreId.HasValue && q.StoreId > 0)
            query = query.Where(p => p.StoreId == q.StoreId);

        // 3) Basic filters
        if (q.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == q.CategoryId.Value);

        if (q.CompanyId.HasValue)
            query = query.Where(p => p.CompanyId == q.CompanyId.Value);

        if (q.UnitId.HasValue)
            query = query.Where(p => p.OuterUnitId == q.UnitId.Value || p.InnerUnitId == q.UnitId.Value);

        if (q.IsFeatured.HasValue) query = query.Where(p => p.IsFeatured == q.IsFeatured.Value);
        if (q.AllowSplitSale.HasValue) query = query.Where(p => p.AllowSplitSale == q.AllowSplitSale.Value);
        if (q.MinQuantity.HasValue) query = query.Where(p => p.Quantity >= q.MinQuantity.Value);
        if (q.MaxQuantity.HasValue) query = query.Where(p => p.Quantity <= q.MaxQuantity.Value);
        if (q.MinPoints.HasValue) query = query.Where(p => p.Points >= q.MinPoints.Value);
        if (q.MaxPoints.HasValue) query = query.Where(p => p.Points <= q.MaxPoints.Value);

        if (q.RequiresPrescription.HasValue)
            query = query.Where(p => p.RequiresPrescription == q.RequiresPrescription.Value);

        if (q.HasPromotion.HasValue)
            query = query.Where(p => p.HasPromotion == q.HasPromotion.Value);

        if (q.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == q.IsAvailable.Value);

        if (q.IsActive.HasValue)
            query = query.Where(p => p.IsActive == q.IsActive.Value);

        if (q.IsActive == true)
            query = query.Where(p => p.Store.IsActive && p.Store.DeletedAt == null);

        if (q.MinPrice.HasValue)
            query = query.Where(p => p.OuterUnitPrice >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            query = query.Where(p => p.OuterUnitPrice <= q.MaxPrice.Value);

        // 4) Search (Arabic + English + keywords + codes)
        if (!string.IsNullOrWhiteSpace(term))
        {
            // If term is numeric, we can also try ERP Id
            var isDecimal = decimal.TryParse(term, out var erpId);

            query = query.Where(p =>
                p.NameAr.Contains(term) ||
                (p.NameEn != null && p.NameEn.Contains(term)) ||
                (p.SearchKeywords != null && p.SearchKeywords.Contains(term)) ||
                (p.InternationalCode != null && p.InternationalCode.Contains(term)) ||
                (isDecimal && p.ErpProductId.HasValue && p.ErpProductId.Value == erpId)
            );
        }

        // 5) Tags filter (any/all)
        if (tagIds.Count > 0)
        {
            if (q.TagMatch == TagMatchMode.Any)
            {
                query = query.Where(p => p.ProductTags.Any(pt => tagIds.Contains(pt.TagId)));
            }
            else // TagMatchMode.All
            {
                // All tags must exist on product
                query = query.Where(p =>
                    p.ProductTags
                        .Where(pt => tagIds.Contains(pt.TagId))
                        .Select(pt => pt.TagId)
                        .Distinct()
                        .Count() == tagIds.Count
                );
            }
        }

        // 6) Total count (before pagination)
        var totalCount = await query.CountAsync(ct);

        // 7) Sorting
        query = q.Sort switch
        {
            ProductSortOption.NameAsc => query.OrderBy(p => p.NameAr),
            ProductSortOption.NameDesc => query.OrderByDescending(p => p.NameAr),

            ProductSortOption.PriceAsc => query.OrderBy(p => p.OuterUnitPrice).ThenBy(p => p.NameAr),
            ProductSortOption.PriceDesc => query.OrderByDescending(p => p.OuterUnitPrice).ThenBy(p => p.NameAr),

            ProductSortOption.Newest => query.OrderByDescending(p => p.CreatedAt),
            ProductSortOption.Oldest => query.OrderBy(p => p.CreatedAt),
            ProductSortOption.Id => query.OrderBy(p => p.Id),

            _ => query.OrderBy(p => p.NameAr)
        };

        // 8) Pagination + projection
        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                NameAr = p.NameAr,
                NameEn = p.NameEn,
                PrimaryImageUrl = p.ProductImages.Where(i => i.DeletedAt == null && i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                CategoryNameAr = p.Category.NameAr,
                CompanyNameAr = p.Company != null ? p.Company.NameAr : null,
                OuterUnitNameAr = p.OuterUnit.NameAr,
                OuterUnitPrice = p.OuterUnitPrice,
                HasPromotion = p.HasPromotion,
                PromotionDiscountPercent = p.PromotionDiscountPercent,
                PromotionStartsAt = p.PromotionStartsAt,
                PromotionEndsAt = p.PromotionEndsAt,
                IsFeatured = p.IsFeatured,
                Points = p.Points,
                RequiresPrescription = p.RequiresPrescription,
                IsAvailable = p.IsAvailable,
                IsActive = p.IsActive,
                TagsCount = p.ProductTags.Count
            })
            .ToListAsync(ct);

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ProductListItemDto>> GetBestSellersAsync(int storeId, int periodDays, int page, int pageSize, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var from = now.AddDays(-periodDays);
        byte[] countedOrderStatuses = [2, 3, 4, 5]; // confirmed through delivered

        var soldRows = await context.OrderItems.AsNoTracking()
            .Where(i => i.Order.StoreId == storeId && i.Product.DeletedAt == null && i.Product.IsActive && i.Product.IsAvailable
                && i.Product.Store.IsActive && i.Product.Store.DeletedAt == null
                && i.Order.CreatedAt >= from && i.Order.CreatedAt <= now
                && countedOrderStatuses.Contains(i.Order.Status))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToListAsync(ct);

        if (soldRows.Count == 0)
            return new PagedResult<ProductListItemDto> { Items = [], TotalCount = 0 };

        var productIds = soldRows.Select(x => x.ProductId).ToList();
        var returnedRows = await context.ReturnRequestItems.AsNoTracking()
            .Where(i => productIds.Contains(i.OrderItem.ProductId)
                && i.ReturnRequest.Status == 2
                && i.ReturnRequest.Order.StoreId == storeId
                && i.ReturnRequest.Order.CreatedAt >= from
                && i.ReturnRequest.Order.CreatedAt <= now
                && countedOrderStatuses.Contains(i.ReturnRequest.Order.Status))
            .GroupBy(i => i.OrderItem.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToListAsync(ct);

        var returnedByProduct = returnedRows.ToDictionary(x => x.ProductId, x => x.Quantity);
        var netSales = soldRows
            .Select(x => new { x.ProductId, Quantity = Math.Max(0m, x.Quantity - returnedByProduct.GetValueOrDefault(x.ProductId)) })
            .Where(x => x.Quantity > 0)
            .ToList();

        var rankedIds = netSales.OrderByDescending(x => x.Quantity).ThenBy(x => x.ProductId).Select(x => x.ProductId).ToList();
        var totalCount = rankedIds.Count;
        var pageIds = rankedIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        if (pageIds.Count == 0)
            return new PagedResult<ProductListItemDto> { Items = [], TotalCount = totalCount };

        var cards = await context.Products.AsNoTracking()
            .Where(p => pageIds.Contains(p.Id) && p.StoreId == storeId && p.DeletedAt == null
                && p.IsActive && p.IsAvailable && p.Store.IsActive && p.Store.DeletedAt == null)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id, NameAr = p.NameAr, NameEn = p.NameEn,
                PrimaryImageUrl = p.ProductImages.Where(i => i.DeletedAt == null && i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                CategoryNameAr = p.Category.NameAr, CompanyNameAr = p.Company != null ? p.Company.NameAr : null,
                OuterUnitNameAr = p.OuterUnit.NameAr, OuterUnitPrice = p.OuterUnitPrice,
                HasPromotion = p.HasPromotion, PromotionDiscountPercent = p.PromotionDiscountPercent,
                PromotionStartsAt = p.PromotionStartsAt, PromotionEndsAt = p.PromotionEndsAt,
                IsFeatured = p.IsFeatured, Points = p.Points, RequiresPrescription = p.RequiresPrescription,
                IsAvailable = p.IsAvailable, IsActive = p.IsActive, TagsCount = p.ProductTags.Count
            }).ToListAsync(ct);

        var byId = cards.ToDictionary(x => x.Id);
        var orderedCards = pageIds.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
        return new PagedResult<ProductListItemDto> { Items = orderedCards, TotalCount = totalCount };
    }


}




