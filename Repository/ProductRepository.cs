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
        IQueryable<Product> query = context.Products.AsNoTracking();

        // Store scope (recommended)
        if (q.StoreId.HasValue && q.StoreId > 0)
            query = query.Where(p => p.StoreId == q.StoreId);

        // 3) Basic filters
        if (q.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == q.CategoryId.Value);

        if (q.CompanyId.HasValue)
            query = query.Where(p => p.CompanyId == q.CompanyId.Value);

        if (q.RequiresPrescription.HasValue)
            query = query.Where(p => p.RequiresPrescription == q.RequiresPrescription.Value);

        if (q.HasPromotion.HasValue)
            query = query.Where(p => p.HasPromotion == q.HasPromotion.Value);

        if (q.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == q.IsAvailable.Value);

        if (q.IsActive.HasValue)
            query = query.Where(p => p.IsActive == q.IsActive.Value);

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
                OuterUnitPrice = p.OuterUnitPrice,
                HasPromotion = p.HasPromotion,
                PromotionDiscountPercent = p.PromotionDiscountPercent,
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



}




