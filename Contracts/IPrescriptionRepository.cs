using Entities.Models;

namespace Contracts;

public interface IPrescriptionRepository
{
    Task AddPrescriptionAsync(Prescription entity, CancellationToken ct);
    Task AddPrescriptionImagesRangeAsync(IEnumerable<PrescriptionImage> images, CancellationToken ct);
    Task<Prescription?> GetForDeleteAsync(int id, CancellationToken ct);
    void RemovePrescription(Prescription entity);
}
