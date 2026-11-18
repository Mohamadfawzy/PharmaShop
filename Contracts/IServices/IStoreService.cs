using Shared.Models.Dtos.Organization;
using Shared.Responses;

namespace Contracts.IServices;

public interface IStoreService
{
    Task<AppResponse<List<StoreDto>>> GetPublicAsync(int? pharmacyId, CancellationToken ct);
    Task<AppResponse<StoreDto>> GetPublicByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<List<StoreDto>>> GetAdminAsync(int? pharmacyId, CancellationToken ct);
    Task<AppResponse<StoreDto>> GetAdminByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> CreateAsync(StoreCreateDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateAsync(int id, StoreUpdateDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetActiveAsync(int id, StoreActiveDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetDefaultAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> SoftDeleteAsync(int id, CancellationToken ct);
}
