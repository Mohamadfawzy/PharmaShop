using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.RequestFeatures;

namespace WebAPI.Controllers;

[Route("api/v1/products")]
public sealed class PublicProductsController : BaseApiController
{
    private readonly IProductService _service;
    public PublicProductsController(IProductService service) => _service = service;
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ProductSearchQueryParams query, CancellationToken ct)
    {
        query.IsActive = true;
        return FromAppResponse(await _service.SearchProductsAsync(query, ct));
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct) => FromAppResponse(await _service.GetProductAsync(id, false, ct));
}
