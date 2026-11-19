using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Auth;
using Shared.Models.Dtos.Customer;
using Shared.Constants;
using Shared.Responses;

namespace WebAPI.Controllers.Auth;

[Route("api/v1/me")]
[ApiController]
[Authorize]
public sealed class CurrentUserController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICustomerService _customerService;

    public CurrentUserController(ICurrentUserService currentUser, ICustomerService customerService)
    {
        _currentUser = currentUser;
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            return Unauthorized(AppResponse.Unauthorized("User identity is missing from the token"));

        CustomerProfileDto? customerProfile = null;
        if (_currentUser.IsInRole(AppRoles.Customer))
        {
            var profileResponse = await _customerService.GetMyProfileAsync(_currentUser.UserId, ct);
            if (profileResponse.IsSuccess) customerProfile = profileResponse.Data;
        }

        var response = AppResponse<CurrentUserDto>.Ok(new CurrentUserDto
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Email = _currentUser.Email,
            PharmacyId = _currentUser.PharmacyId,
            Roles = _currentUser.Roles.OrderBy(r => r).ToList(),
            Permissions = _currentUser.Permissions.OrderBy(p => p).ToList(),
            Customer = customerProfile
        });

        return Ok(response);
    }

    [HttpGet("permissions")]
    public IActionResult GetPermissions()
        => Ok(AppResponse<List<string>>.Ok(_currentUser.Permissions.OrderBy(p => p).ToList()));

    [HttpPut]
    [Authorize(Roles = AppRoles.Customer)]
    public async Task<IActionResult> UpdateCustomerProfile([FromBody] CustomerProfileUpdateDto dto, CancellationToken ct)
    {
        var response = await _customerService.UpdateMyProfileAsync(_currentUser.UserId, _currentUser.Email, dto, ct);
        return StatusCode(response.StatusCode, response);
    }
}
