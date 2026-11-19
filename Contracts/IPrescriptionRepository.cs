using Entities.Models;
using Shared.Models.Dtos.Prescription;
using Shared.Responses;

namespace Contracts;

public interface IPrescriptionRepository
{
    Task AddItemAsync(PrescriptionItem item, CancellationToken ct);
    Task AddItemsRangeAsync(IEnumerable<PrescriptionItem> items, CancellationToken ct);
    Task AddPrescriptionAsync(Prescription entity, CancellationToken ct);
    Task AddPrescriptionImagesRangeAsync(IEnumerable<PrescriptionImage> images, CancellationToken ct);
    Task<Prescription?> GetByIdForAdminAsync(int prescriptionId, CancellationToken ct);
    Task<Prescription?> GetForEmployeeReviewAsync(int prescriptionId, CancellationToken ct);
    Task<int> GetImageCountAsync(int prescriptionId, CancellationToken ct);
    Task<string?> GetImageIdentifierAsync(int prescriptionId, string imageIdentifier, CancellationToken ct);
    Task<CustomerPrescriptionDetailsDto?> GetCustomerDetailsAsync(int prescriptionId, int customerId, CancellationToken ct);
    Task<Prescription?> GetForDeleteAsync(int id, CancellationToken ct);
    Task<Prescription?> GetForStatusUpdateAsync(int id, CancellationToken ct);
    Task<PrescriptionItem?> GetItemForDeleteAsync(int prescriptionId, int itemId, CancellationToken ct);
    Task<List<PrescriptionItemListItemDto>> GetItemsByPrescriptionIdAsync(int prescriptionId, CancellationToken ct);
    Task<byte?> GetStatusAsync(int prescriptionId, CancellationToken ct);
    Task<bool> PrescriptionExistsAsync(int prescriptionId, CancellationToken ct);
    void RemoveItem(PrescriptionItem item);
    void RemovePrescription(Prescription entity);
    Task<PagedResult<AdminPrescriptionListItemDto>> SearchAdminAsync(AdminPrescriptionListQueryDto q, CancellationToken ct);
    Task<PagedResult<AdminPrescriptionListItemDto>> SearchForPharmacyAsync(AdminPrescriptionListQueryDto q, int pharmacyId, CancellationToken ct);
    Task<PagedResult<CustomerPrescriptionListItemDto>> SearchForCustomerAsync(CustomerPrescriptionListQueryDto q, int customerId, CancellationToken ct);
    void UpdatePrescription(Prescription entity);
}
