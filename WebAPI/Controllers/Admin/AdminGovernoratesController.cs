using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Locations;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/governorates")]
public sealed class AdminGovernoratesController : AdminBaseApiController
{
    private readonly ILocationService _service;

    public AdminGovernoratesController(ILocationService service, IWebHostEnvironment env) : base(env)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromAppResponse(await _service.GetGovernoratesAsync(includeInactive: true, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetGovernorateByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GovernorateWriteDto dto, CancellationToken ct)
    {
        var result = await _service.CreateGovernorateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GovernorateWriteDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateGovernorateAsync(id, dto, ct));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] LocationActiveDto dto, CancellationToken ct)
        => FromAppResponse(await _service.SetGovernorateActiveAsync(id, dto, ct));
}
