using Shared.Models.Auth;
using Shared.Responses;

namespace Contracts.IServices;

public interface IAuthService
{
    Task<AppResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto, CancellationToken ct);

    Task<AppResponse<ConfirmEmailResponseDto>> ConfirmEmailAsync(ConfirmEmailRequestDto dto, CancellationToken ct);

    Task<AppResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, LoginRequestContext ctx, CancellationToken ct);

    Task<AppResponse> ResendConfirmEmailAsync(ResendConfirmEmailRequestDto dto, CancellationToken ct);

    Task<AppResponse> ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct);
    Task<AppResponse> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct);
    Task<AppResponse> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto, CancellationToken ct);

}
