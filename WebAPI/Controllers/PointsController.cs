using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Points;

namespace WebAPI.Controllers;

[Route("api/v1/points")]
[Authorize(Policy = AppPolicies.Customer)]
public sealed class PointsController : BaseApiController
{
    private readonly IPointsService _pointsService;
    public PointsController(IPointsService pointsService) => _pointsService = pointsService;

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(CancellationToken ct)
        => FromAppResponse(await _pointsService.GetMyBalanceAsync(ct));

    [HttpGet("lots")]
    public async Task<IActionResult> GetLots([FromQuery] PointsListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _pointsService.GetMyLotsAsync(query, ct));

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] PointsListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _pointsService.GetMyTransactionsAsync(query, ct));

    [HttpGet("redeemable-products")]
    public async Task<IActionResult> GetRedeemableProducts([FromQuery] int? storeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => FromAppResponse(await _pointsService.GetRedeemableProductsAsync(storeId, page, pageSize, ct));
}
