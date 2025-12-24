using Shared.Models.Auth;
using Shared.Responses;

namespace Contracts.IServices;

public interface IAuthService
{
    Task<AppResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct);

}
