using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/v1/stores")]
public sealed class StoresController : BaseApiController
{
    private readonly IStoreService _service;

    public StoresController(IStoreService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? pharmacyId, CancellationToken ct)
        => FromAppResponse(await _service.GetPublicAsync(pharmacyId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetPublicByIdAsync(id, ct));

    [HttpGet("{id:int}/contact")]
    public async Task<IActionResult> GetContact(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetPublicContactAsync(id, ct));
}
