using Shared.Enums.Prescription;
using Shared.Models.Dtos.Prescription;
using Shared.Models.ImageDtos;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPrescriptionService
{
    Task<AppResponse<PrescriptionItemCreatedDto>> AddPrescriptionItemAsync(int prescriptionId, PrescriptionItemCreateDto dto, CancellationToken ct);
    Task<AppResponse<PrescriptionItemsBatchCreateResultDto>> AddPrescriptionItemsBatchAsync(int prescriptionId, PrescriptionItemsBatchCreateDto dto, CancellationToken ct);
    Task<AppResponse<PrescriptionCreateResultDto>> CreateWithImagesAsync(PrescriptionCreateRequestDto dto, IReadOnlyList<UploadStreamFile> files, CancellationToken ct);
    Task<AppResponse<int>> DeletePrescriptionItemAsync(int prescriptionId, int itemId, CancellationToken ct);
    Task<AppResponse<List<PrescriptionItemListItemDto>>> GetAdminPrescriptionItemsAsync(int prescriptionId, CancellationToken ct);
    Task<AppResponse<List<AdminPrescriptionListItemDto>>> GetAdminPrescriptionsAsync(AdminPrescriptionListQueryDto query, CancellationToken ct);
}
