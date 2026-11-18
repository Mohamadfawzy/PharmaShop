using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Auth;

namespace WebAPI.Controllers.Auth;

[Route("api/admin/users")]
[ApiController]
[Authorize(Policy = AppPolicies.Admin)]
public sealed class AdminUsersPermissionsController : ControllerBase
{
    private readonly IUserPermissionService _service;

    public AdminUsersPermissionsController(IUserPermissionService service)
    {
        _service = service;
    }

    [HttpGet("{userId:int}/permissions")]
    public async Task<IActionResult> Get(int userId, CancellationToken ct)
    {
        var result = await _service.GetUserPermissionsAsync(userId, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{userId:int}/permissions")]
    public async Task<IActionResult> Set(
        int userId,
        [FromBody] SetUserPermissionsRequestDto dto,
        CancellationToken ct)
    {
        if (dto is null)
            return BadRequest(Shared.Responses.AppResponse.ValidationError("Request body is required"));

        var result = await _service.SetUserPermissionsAsync(userId, dto.Permissions, ct);
        return StatusCode(result.StatusCode, result);
    }
}
