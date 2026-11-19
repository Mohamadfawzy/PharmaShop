using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Inventory;
using Shared.Models.Dtos.Product;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/inventory")]
[Authorize(Policy = AppPolicies.CanManageInventory)]
public sealed class AdminInventoryController : BaseApiController
{
    private readonly IInventoryService _inventory;
    public AdminInventoryController(IInventoryService inventory) => _inventory = inventory;

    [HttpPost("products/{productId:int}/receipts")]
    public async Task<IActionResult> Receive(int productId, [FromBody] ReceiveStockDto dto, CancellationToken ct)
        => FromAppResponse(await _inventory.ReceiveAsync(productId, dto, ct));

    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust([FromBody] AdjustInventoryDto dto, CancellationToken ct)
        => FromAppResponse(await _inventory.AdjustAsync(dto, ct));

    [HttpGet("transactions")]
    public async Task<IActionResult> Search([FromQuery] InventoryTransactionQueryDto query, CancellationToken ct)
        => FromAppResponse(await _inventory.SearchAsync(query, ct));
}
