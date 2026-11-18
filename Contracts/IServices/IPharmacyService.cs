using Shared.Models.Dtos.Organization;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPharmacyService
{
    Task<AppResponse<List<PharmacyDto>>> GetAllAsync(CancellationToken ct);
    Task<AppResponse<PharmacyDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> CreateAsync(PharmacyCreateDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateAsync(int id, PharmacyUpdateDto dto, CancellationToken ct);
}
