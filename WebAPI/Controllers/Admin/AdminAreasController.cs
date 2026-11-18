using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Locations;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/areas")]
public sealed class AdminAreasController : AdminBaseApiController
{
    private readonly ILocationService _service;

    public AdminAreasController(ILocationService service, IWebHostEnvironment env) : base(env)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? governorateId, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminAreasAsync(governorateId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetAreaByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GovernorateAreaWriteDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAreaAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GovernorateAreaWriteDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateAreaAsync(id, dto, ct));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] LocationActiveDto dto, CancellationToken ct)
        => FromAppResponse(await _service.SetAreaActiveAsync(id, dto, ct));
}
