using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace WebAPI.Controllers;

[Route("api/v1/me/employee")]
[Authorize(Roles = AppRoles.Employee + "," + AppRoles.DeliveryAgent)]
public sealed class CurrentEmployeeController : BaseApiController
{
    private readonly IEmployeeService _service;
    private readonly ICurrentUserService _currentUser;

    public CurrentEmployeeController(IEmployeeService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => FromAppResponse(await _service.GetCurrentAsync(_currentUser.UserId, ct));
}
