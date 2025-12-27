using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Auth;


[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public sealed class AdminRolesController : ControllerBase
{
    private readonly IUserRoleService _query;

    public AdminRolesController(IUserRoleService query)
    {
        _query = query;
    }

    //  show all roles in our database
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles(CancellationToken ct)
    {
        var result = await _query.GetAllRolesAsync(ct);
        return StatusCode(result.StatusCode, result);
    }
}