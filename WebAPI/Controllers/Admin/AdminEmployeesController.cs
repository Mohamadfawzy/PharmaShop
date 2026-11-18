using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Organization;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/employees")]
public sealed class AdminEmployeesController : AdminBaseApiController
{
    private readonly IEmployeeService _service;

    public AdminEmployeesController(IEmployeeService service, IWebHostEnvironment env) : base(env)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? pharmacyId, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => FromAppResponse(await _service.GetAllAsync(pharmacyId, includeInactive, ct));

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted([FromQuery] int? pharmacyId, CancellationToken ct)
        => FromAppResponse(await _service.GetDeletedAsync(pharmacyId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateAsync(id, dto, ct));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] EmployeeActiveDto dto, CancellationToken ct)
        => FromAppResponse(await _service.SetActiveAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromAppResponse(await _service.SoftDeleteAsync(id, ct));

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
        => FromAppResponse(await _service.RestoreAsync(id, ct));
}
