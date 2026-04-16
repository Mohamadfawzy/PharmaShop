using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Models.Dtos.Promotion;
using WebAPI.Controllers.Admin;

namespace WebAPI.Controllers;

[Route("api/v1/promotions")]
[ApiController]
public class PromotionsController : AdminBaseApiController
{
    private readonly IPromotionService promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        this.promotionService = promotionService;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await promotionService.CreatePromotionAsync(dto, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PromotionListQueryDto query, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await promotionService.GetPromotionsAsync(query, ct);

        // 2) Unified response
        return FromAppResponse(result);
    }

    // PromotionsController
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await promotionService.GetPromotionByIdAsync(id, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement: add caching for frequently accessed details
    }



    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PromotionUpdateDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await promotionService.UpdatePromotionAsync(id, dto, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement: use ETag header for RowVersion instead of body
    }



}
