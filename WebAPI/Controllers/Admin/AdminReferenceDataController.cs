using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Catalog;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin")]
public sealed class AdminReferenceDataController : AdminBaseApiController
{
    private readonly IReferenceDataService _service;
    public AdminReferenceDataController(IReferenceDataService service, IWebHostEnvironment env) : base(env) => _service = service;

    [HttpGet("companies")]
    public async Task<IActionResult> Companies([FromQuery] bool includeInactive = false, CancellationToken ct = default) => FromAppResponse(await _service.GetCompaniesAsync(includeInactive, ct));
    [HttpGet("companies/{id:int}")]
    public async Task<IActionResult> Company(int id, CancellationToken ct) => FromAppResponse(await _service.GetCompanyAsync(id, ct));
    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany(CompanyWriteDto dto, CancellationToken ct) => FromAppResponse(await _service.CreateCompanyAsync(dto, ct));
    [HttpPut("companies/{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, CompanyWriteDto dto, CancellationToken ct) => FromAppResponse(await _service.UpdateCompanyAsync(id, dto, ct));
    [HttpDelete("companies/{id:int}")]
    public async Task<IActionResult> DeleteCompany(int id, CancellationToken ct) => FromAppResponse(await _service.DeleteCompanyAsync(id, false, ct));
    [HttpPatch("companies/{id:int}/restore")]
    public async Task<IActionResult> RestoreCompany(int id, CancellationToken ct) => FromAppResponse(await _service.DeleteCompanyAsync(id, true, ct));

    [HttpGet("units")]
    public async Task<IActionResult> Units([FromQuery] bool includeInactive = false, CancellationToken ct = default) => FromAppResponse(await _service.GetUnitsAsync(includeInactive, ct));
    [HttpGet("units/{id:int}")]
    public async Task<IActionResult> Unit(int id, CancellationToken ct) => FromAppResponse(await _service.GetUnitAsync(id, ct));
    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit(UnitWriteDto dto, CancellationToken ct) => FromAppResponse(await _service.CreateUnitAsync(dto, ct));
    [HttpPut("units/{id:int}")]
    public async Task<IActionResult> UpdateUnit(int id, UnitWriteDto dto, CancellationToken ct) => FromAppResponse(await _service.UpdateUnitAsync(id, dto, ct));
    [HttpDelete("units/{id:int}")]
    public async Task<IActionResult> DeleteUnit(int id, CancellationToken ct) => FromAppResponse(await _service.SetUnitActiveAsync(id, false, ct));
    [HttpPatch("units/{id:int}/activate")]
    public async Task<IActionResult> ActivateUnit(int id, CancellationToken ct) => FromAppResponse(await _service.SetUnitActiveAsync(id, true, ct));
}
