using Shared.Models.Dtos.Prescription;
using Shared.Models.ImageDtos;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPrescriptionService
{
    Task<AppResponse<PrescriptionCreateResultDto>> CreateWithImagesAsync(PrescriptionCreateRequestDto dto, IReadOnlyList<UploadStreamFile> files, CancellationToken ct);
}
