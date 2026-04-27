using Entities.Models;
using Shared.Models.Dtos.Prescription;
using Shared.Responses;

namespace Contracts;

public interface IPrescriptionRepository
{
    Task AddItemAsync(PrescriptionItem item, CancellationToken ct);
    Task AddPrescriptionAsync(Prescription entity, CancellationToken ct);
    Task AddPrescriptionImagesRangeAsync(IEnumerable<PrescriptionImage> images, CancellationToken ct);
    Task<Prescription?> GetByIdForAdminAsync(int prescriptionId, CancellationToken ct);
    Task<Prescription?> GetForDeleteAsync(int id, CancellationToken ct);
    void RemovePrescription(Prescription entity);
    Task<PagedResult<AdminPrescriptionListItemDto>> SearchAdminAsync(AdminPrescriptionListQueryDto q, CancellationToken ct);
}
