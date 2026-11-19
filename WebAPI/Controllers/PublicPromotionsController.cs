using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Promotion;

namespace WebAPI.Controllers;

[Route("api/v1/promotions")]
[ApiController]
public sealed class PublicPromotionsController : BaseApiController
{
    private readonly IPromotionService _service;
    public PublicPromotionsController(IPromotionService service) => _service = service;

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] PromotionListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _service.GetActivePromotionsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetActiveById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetActivePromotionByIdAsync(id, ct));
}
