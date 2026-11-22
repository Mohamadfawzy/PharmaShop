using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Organization;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/stores")]
public sealed class AdminStoresController : AdminBaseApiController
{
    private readonly IStoreService _service;

    public AdminStoresController(IStoreService service, IWebHostEnvironment env) : base(env)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? pharmacyId, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminAsync(pharmacyId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StoreUpdateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateAsync(id, dto, ct));

    [HttpPut("{id:int}/contact")]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] StoreContactUpdateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateContactAsync(id, dto, ct));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] StoreActiveDto dto, CancellationToken ct)
        => FromAppResponse(await _service.SetActiveAsync(id, dto, ct));

    [HttpPatch("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
        => FromAppResponse(await _service.SetDefaultAsync(id, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromAppResponse(await _service.SoftDeleteAsync(id, ct));
}
