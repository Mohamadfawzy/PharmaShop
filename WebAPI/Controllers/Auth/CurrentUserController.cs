using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Auth;
using Shared.Responses;

namespace WebAPI.Controllers.Auth;

[Route("api/v1/me")]
[ApiController]
[Authorize]
public sealed class CurrentUserController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    public CurrentUserController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            return Unauthorized(AppResponse.Unauthorized("User identity is missing from the token"));

        var response = AppResponse<CurrentUserDto>.Ok(new CurrentUserDto
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Email = _currentUser.Email,
            PharmacyId = _currentUser.PharmacyId,
            Roles = _currentUser.Roles.OrderBy(r => r).ToList(),
            Permissions = _currentUser.Permissions.OrderBy(p => p).ToList()
        });

        return Ok(response);
    }

    [HttpGet("permissions")]
    public IActionResult GetPermissions()
        => Ok(AppResponse<List<string>>.Ok(_currentUser.Permissions.OrderBy(p => p).ToList()));
}
