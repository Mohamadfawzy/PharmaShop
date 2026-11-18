using Shared.Models.Dtos.Locations;
using Shared.Responses;

namespace Contracts.IServices;

public interface ILocationService
{
    Task<AppResponse<List<GovernorateDto>>> GetGovernoratesAsync(bool includeInactive, CancellationToken ct);
    Task<AppResponse<GovernorateDto>> GetGovernorateByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<List<GovernorateAreaDto>>> GetPublicAreasAsync(int governorateId, CancellationToken ct);
    Task<AppResponse<List<GovernorateAreaDto>>> GetAdminAreasAsync(int? governorateId, CancellationToken ct);
    Task<AppResponse<GovernorateAreaDto>> GetAreaByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> CreateGovernorateAsync(GovernorateWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateGovernorateAsync(int id, GovernorateWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetGovernorateActiveAsync(int id, LocationActiveDto dto, CancellationToken ct);
    Task<AppResponse<int>> CreateAreaAsync(GovernorateAreaWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateAreaAsync(int id, GovernorateAreaWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetAreaActiveAsync(int id, LocationActiveDto dto, CancellationToken ct);
}
