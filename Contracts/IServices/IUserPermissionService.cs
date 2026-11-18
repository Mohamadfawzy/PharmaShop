using Shared.Responses;

namespace Contracts.IServices;

public interface IUserPermissionService
{
    Task<AppResponse<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken ct);
    Task<AppResponse<List<string>>> SetUserPermissionsAsync(int userId, List<string> permissions, CancellationToken ct);
}
