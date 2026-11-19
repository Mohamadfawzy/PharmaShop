using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/v1")]
public sealed class ReferenceDataController : BaseApiController
{
    private readonly IReferenceDataService _service;
    public ReferenceDataController(IReferenceDataService service) => _service = service;
    [HttpGet("companies")]
    public async Task<IActionResult> Companies(CancellationToken ct) => FromAppResponse(await _service.GetCompaniesAsync(false, ct));
    [HttpGet("units")]
    public async Task<IActionResult> Units(CancellationToken ct) => FromAppResponse(await _service.GetUnitsAsync(false, ct));
}
