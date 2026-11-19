using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Delivery;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/stores/{storeId:int}/delivery-zones")]
public sealed class AdminDeliveryZonesController : AdminBaseApiController
{
    private readonly IDeliveryZoneService _service;

    public AdminDeliveryZonesController(IDeliveryZoneService service, IWebHostEnvironment env) : base(env)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(int storeId, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminAsync(storeId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int storeId, int id, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminByIdAsync(storeId, id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(int storeId, [FromBody] DeliveryZoneWriteDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(storeId, dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int storeId, int id, [FromBody] DeliveryZoneWriteDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateAsync(storeId, id, dto, ct));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int storeId, int id, [FromBody] DeliveryZoneActiveDto dto, CancellationToken ct)
        => FromAppResponse(await _service.SetActiveAsync(storeId, id, dto, ct));
}
