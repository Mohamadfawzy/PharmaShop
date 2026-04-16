using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Promotion;
using Shared.Models.Dtos.Promotion;
using Shared.Responses;

namespace Repository;


public sealed class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
{
    private readonly RepositoryContext _db;

    public PromotionRepository(RepositoryContext db) : base(db)
    {
        _db = db;
    }

    public Task<bool> ExistsByErpPgoIdAsync(decimal erpPgoId, CancellationToken ct)
    {
        // Check uniqueness ignoring soft-deleted rows
        return _db.Promotions
            .AsNoTracking()
            .AnyAsync(p => p.ErpPgoId == erpPgoId && p.DeletedAt == null, ct);
    }

    public async Task<PagedResult<PromotionListItemDto>> SearchAsync(
        string? term,
        bool? isActive,
        DateTime? from,
        DateTime? to,
        bool? onlyRunningNow,
        PromotionSortOption sort,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        // 1) Base query (ignore soft-deleted)
        IQueryable<Promotion> q = _db.Promotions.AsNoTracking()
            .Where(p => p.DeletedAt == null);

        // 2) Filter by IsActive
        if (isActive.HasValue)
            q = q.Where(p => p.IsActive == isActive.Value);

        // 3) Search by Name (contains)
        if (!string.IsNullOrWhiteSpace(term))
            q = q.Where(p => p.Name != null && p.Name.Contains(term));

        // 4) Filter by running now (optional)
        if (onlyRunningNow == true)
        {
            var now = DateTime.UtcNow;
            q = q.Where(p =>
                p.IsActive &&
                p.StartAt != null && p.EndAt != null &&
                p.StartAt <= now && p.EndAt >= now
            );
        }

        // 5) Filter by overlap window (optional)
        if (from.HasValue || to.HasValue)
        {
            var f = from ?? DateTime.MinValue;
            var t = to ?? DateTime.MaxValue;

            // Overlap rule: [StartAt, EndAt] overlaps [f, t]
            // Promotions with null dates are treated as "always" and included
            q = q.Where(p =>
                (p.StartAt == null && p.EndAt == null) ||
                (p.StartAt != null && p.EndAt != null && p.StartAt <= t && p.EndAt >= f)
            );
        }

        // 6) Total count (before pagination)
        var total = await q.CountAsync(ct);

        // 7) Sorting
        q = sort switch
        {
            PromotionSortOption.Oldest => q.OrderBy(p => p.CreatedAt),
            PromotionSortOption.StartAtAsc => q.OrderBy(p => p.StartAt ?? DateTime.MaxValue),
            PromotionSortOption.StartAtDesc => q.OrderByDescending(p => p.StartAt ?? DateTime.MinValue),
            _ => q.OrderByDescending(p => p.CreatedAt) // Newest
        };

        // 8) Pagination + projection
        var skip = (page - 1) * pageSize;

        var items = await q
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new PromotionListItemDto
            {
                Id = p.Id,
                ErpPgoId = p.ErpPgoId,
                Name = p.Name,
                IsActive = p.IsActive,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                TotalAmount = p.TotalAmount,
                BasicAmount = p.BasicAmount,
                OfferAmount = p.OfferAmount,
                DiscountPercent = p.DiscountPercent,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        // 9) Return paged result
        return new PagedResult<PromotionListItemDto>
        {
            Items = items,
            TotalCount = total
        };

        // Future improvements:
        // - Add compiled queries for hot paths
        // - Add full-text search on Name/Notes if needed
        // - Add filtering by date status (upcoming/expired)
    }



    public async Task<PromotionDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken ct)
    {
        // 1) Load promotion header (ignore soft-deleted)
        var promo = await _db.Promotions
            .AsNoTracking()
            .Where(p => p.Id == id && p.DeletedAt == null)
            .Select(p => new PromotionDetailsDto
            {
                Id = p.Id,
                ErpPgoId = p.ErpPgoId,
                Name = p.Name,
                Notes = p.Notes,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                TotalAmount = p.TotalAmount,
                BasicAmount = p.BasicAmount,
                OfferAmount = p.OfferAmount,
                DiscountPercent = p.DiscountPercent,
                IsActive = p.IsActive,

                // Count products (ignore soft-deleted)
                ProductsCount = p.PromotionProducts.Count(pp => pp.DeletedAt == null),

                // Products list (simple for admin)
                Products = p.PromotionProducts
                    .Where(pp => pp.DeletedAt == null)
                    .OrderBy(pp => pp.Id)
                    .Select(pp => new PromotionProductItemDto
                    {
                        Id = pp.Id,
                        ProductId = pp.ProductId,
                        ErpProductId = pp.ErpProductId,
                        ErpOfferId = pp.ErpOfferId
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        // 2) Return DTO (or null)
        return promo;

        // Future improvements:
        // - Add product name/image by joining Products/ProductImages for better admin UX
        // - Add pagination for products when promotion contains many products
        // - Add filtering/sorting for products list

    }

    public async Task<Promotion?> GetByIdForUpdateAsync(int id, CancellationToken ct)
    {
        // 1) Load promotion for update (tracked) and ignore soft-deleted
        return await _db.Promotions
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct);

        // Future improvement: include related data only when needed (e.g., products count)
    }

    public async Task<bool> ExistsByErpPgoIdAsync(decimal erpPgoId, int excludeId, CancellationToken ct)
    {
        // 1) Check uniqueness ignoring soft-deleted rows
        return await _db.Promotions
            .AsNoTracking()
            .AnyAsync(p => p.ErpPgoId == erpPgoId && p.DeletedAt == null && p.Id != excludeId, ct);

        // Future improvement: normalize ERP ids if needed
    }



}
