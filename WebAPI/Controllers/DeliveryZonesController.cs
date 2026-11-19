using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/v1/delivery-zones")]
public sealed class DeliveryZonesController : BaseApiController
{
    private readonly IDeliveryZoneService _service;

    public DeliveryZonesController(IDeliveryZoneService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByStore([FromQuery] int storeId, CancellationToken ct)
        => FromAppResponse(await _service.GetPublicAsync(storeId, ct));
}
