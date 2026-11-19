using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Returns;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/returns")]
[Authorize(Policy = AppPolicies.CanManageOrders)]
public sealed class AdminReturnsController : BaseApiController
{
    private readonly IReturnRequestService _returns;
    public AdminReturnsController(IReturnRequestService returns) => _returns = returns;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ReturnRequestQueryDto query, CancellationToken ct)
        => FromAppResponse(await _returns.GetAdminReturnsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _returns.GetAdminReturnAsync(id, ct));

    [HttpPatch("{id:int}/decision")]
    public async Task<IActionResult> Decide(int id, [FromBody] ReviewReturnRequestDto dto, CancellationToken ct)
        => FromAppResponse(await _returns.ReviewAsync(id, dto, ct));
}
