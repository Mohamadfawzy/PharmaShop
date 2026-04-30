using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Prescription;
using Shared.Models.Dtos.Prescription;
using Shared.Responses;

namespace Repository;

public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
{
    private readonly RepositoryContext _db;

    public PrescriptionRepository(RepositoryContext context) : base(context)
    {
        this._db = context;
    }


    // 1) Add new prescription (tracked)
    public async Task AddPrescriptionAsync(Prescription entity, CancellationToken ct)
    {
        // 1) Add entity (no save here)
        await _db.Prescriptions.AddAsync(entity, ct);

        // Future improvement: validate FK existence (Customer/Store) at repo level if desired
    }

    // 2) Add prescription images range (tracked)              
    public async Task AddPrescriptionImagesRangeAsync(IEnumerable<PrescriptionImage> images, CancellationToken ct)
    {
        // 1) Add entities (no save here)
        await _db.PrescriptionImages.AddRangeAsync(images, ct);

        // Future improvement: bulk insert for very large batches
    }

    // 3) Delete prescription hard (used for rollback)
    public Task<Prescription?> GetForDeleteAsync(int id, CancellationToken ct)
        => _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id, ct);

    public void RemovePrescription(Prescription entity)
    {
        // 1) Remove entity (hard delete)
        _db.Prescriptions.Remove(entity);

        // Future improvement: add soft delete if you later want audit/history
    }

    public async Task<Prescription?> GetByIdForAdminAsync(int prescriptionId, CancellationToken ct)
    {
        // 1) Load prescription header (read-only)
        return await _db.Prescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct);

        // Future improvement: include StoreId scoping for admin permissions
    }

    public async Task AddItemAsync(PrescriptionItem item, CancellationToken ct)
    {
        // 1) Add prescription item (tracked)
        await _db.PrescriptionItems.AddAsync(item, ct);

        // Future improvement: validate prescription exists at repo level if needed
    }


    public async Task AddItemsRangeAsync(IEnumerable<PrescriptionItem> items, CancellationToken ct)
    {
        // 1) Add items in bulk (tracked)
        await _db.PrescriptionItems.AddRangeAsync(items, ct);

        // Future improvement: use bulk insert library for very large batches
    }


    // ==================================================================

    public async Task<PagedResult<AdminPrescriptionListItemDto>> SearchAdminAsync(AdminPrescriptionListQueryDto q, CancellationToken ct)
    {
        // 1) Base query for admin list
        IQueryable<Prescription> query = _db.Prescriptions.AsNoTracking();

        // 2) Store filter (required)
        query = query.Where(p => p.StoreId == q.StoreId);

        // 3) Status filter (optional)
        if (q.Status.HasValue)
            query = query.Where(p => p.Status == q.Status.Value);

        // 4) Customer filter (optional)
        if (q.CustomerId.HasValue)
            query = query.Where(p => p.CustomerId == q.CustomerId.Value);

        // 5) Reviewer filter (optional)
        if (q.ReviewedBy.HasValue)
            query = query.Where(p => p.ReviewedBy == q.ReviewedBy.Value);

        // 6) Date range filter on CreatedAt (optional)
        if (q.From.HasValue)
            query = query.Where(p => p.CreatedAt >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(p => p.CreatedAt <= q.To.Value);

        // 7) Total count before pagination
        var total = await query.CountAsync(ct);

        // 8) Sorting
        query = q.Sort switch
        {
            PrescriptionSortOption.Oldest => query.OrderBy(p => p.CreatedAt),
            PrescriptionSortOption.Newest => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.StatusUpdatedAt) // default
        };

        // 9) Pagination + projection
        var skip = (q.Page - 1) * q.PageSize;

        var items = await query
            .Skip(skip)
            .Take(q.PageSize)
            .Select(p => new AdminPrescriptionListItemDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                StoreId = p.StoreId,
                Status = p.Status,
                StatusUpdatedAt = p.StatusUpdatedAt,
                ReviewedBy = p.ReviewedBy,
                ReadyForCheckoutAt = p.ReadyForCheckoutAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        // 10) Return paged result
        return new PagedResult<AdminPrescriptionListItemDto>
        {
            Items = items,
            TotalCount = total
        };

        // Future improvements:
        // - Add search term (by customer phone/name) via join if needed
        // - Add PrimaryImageUrl in list using left join + IsPrimary
    }
}



