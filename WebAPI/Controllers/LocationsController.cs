using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/v1/locations")]
public sealed class LocationsController : BaseApiController
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service) => _service = service;

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernorates(CancellationToken ct)
        => FromAppResponse(await _service.GetGovernoratesAsync(includeInactive: false, ct));

    [HttpGet("governorates/{id:int}/areas")]
    public async Task<IActionResult> GetAreas(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetPublicAreasAsync(id, ct));
}
