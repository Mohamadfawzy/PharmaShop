using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

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
}



