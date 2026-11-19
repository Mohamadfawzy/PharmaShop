using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Points;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/points")]
[Authorize(Policy = AppPolicies.CanManagePoints)]
public sealed class AdminPointsController : BaseApiController
{
    private readonly IPointsService _pointsService;
    public AdminPointsController(IPointsService pointsService) => _pointsService = pointsService;

    [HttpPost("customers/{customerId:int}/adjust")]
    public async Task<IActionResult> Adjust(int customerId, [FromBody] ManualPointsAdjustmentDto dto, CancellationToken ct)
        => FromAppResponse(await _pointsService.AdjustCustomerPointsAsync(customerId, dto, ct));
}
