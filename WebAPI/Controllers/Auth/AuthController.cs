using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Auth;
using Shared.Responses;

namespace WebAPI.Controllers.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(dto, ct);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        // لو Created => 201
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailGet([FromQuery] int userId,[FromQuery] string token,CancellationToken ct)
    {
        var dto = new ConfirmEmailRequestDto
        {
            UserId = userId,
            Token = token
        };

        var result = await _authService.ConfirmEmailAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var ctx = new LoginRequestContext
        {
            TraceId = HttpContext.TraceIdentifier,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            PharmacyId = 1 // الآن ثابت، لاحقًا من tenant resolution
        };

        var result = await _authService.LoginAsync(dto, ctx, ct);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPost("resend-confirm-email")]
    public async Task<IActionResult> ResendConfirmEmail([FromBody] ResendConfirmEmailRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.ResendConfirmEmailAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return StatusCode(401, AppResponse.Unauthorized("Unauthorized"));

        var result = await _authService.ChangePasswordAsync(userId, dto, ct);
        return StatusCode(result.StatusCode, result);
    }
}