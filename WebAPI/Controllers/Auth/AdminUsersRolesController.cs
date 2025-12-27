using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Auth;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAPI.Controllers.Auth;


[Route("api/admin/users")]
[ApiController]
//[Authorize(Roles = "Admin")] // أو Policy لاحقاً
public sealed class AdminUsersRolesController : ControllerBase
{
    private readonly IUserRoleService _service;

    public AdminUsersRolesController(IUserRoleService service)
    {
        _service = service;
    }

    // Show specific user roles
    [HttpGet("{userId:int}/roles")]
    public async Task<IActionResult> GetUserRoles(int userId, CancellationToken ct)
    {
        var result = await _service.GetUserRolesAsync(userId, ct);
        return StatusCode(result.StatusCode, result);
    }

    // Replace all roles for specific user
    [HttpPut("{userId:int}/roles")]
    public async Task<IActionResult> SetRoles(int userId, [FromBody] SetUserRolesRequestDto dto, CancellationToken ct)
    {
        var result = await _service.SetRolesAsync(userId, dto.Roles, ct);
        return StatusCode(result.StatusCode, result);
    }

    // 2) Add roles
    // POST /api/admin/users/{userId}/roles
    [HttpPost("{userId:int}/roles")]
    public async Task<IActionResult> AddRoles(int userId, [FromBody] ModifyUserRolesRequestDto dto, CancellationToken ct)
    {
        var result = await _service.AddRolesAsync(userId, dto.Roles, ct);
        return StatusCode(result.StatusCode, result);
    }

    // 3) Remove roles
    // DELETE /api/admin/users/{userId}/roles
    [HttpDelete("{userId:int}/roles")]
    public async Task<IActionResult> RemoveRoles(int userId, [FromBody] ModifyUserRolesRequestDto dto, CancellationToken ct)
    {
        var result = await _service.RemoveRolesAsync(userId, dto.Roles, ct);
        return StatusCode(result.StatusCode, result);
    }
}