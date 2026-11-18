using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Organization;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/pharmacies")]
public sealed class AdminPharmaciesController : AdminBaseApiController
{
    private readonly IPharmacyService _service;

    public AdminPharmaciesController(IPharmacyService service, IWebHostEnvironment env) : base(env)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromAppResponse(await _service.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PharmacyCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PharmacyUpdateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateAsync(id, dto, ct));
}
