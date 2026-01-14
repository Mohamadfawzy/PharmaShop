using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Product;
using Shared.Models.Dtos.Product.Units;
using Shared.Models.RequestFeatures;
using WebAPI.Controllers.Admin;

namespace WebAPI.Controllers;

[Route("api/v1/admin/Products")]
[ApiController]
public class ProductsController : AdminBaseApiController

{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment env;
    private readonly string rootPath;

    public ProductsController(IProductService productService, IWebHostEnvironment env) : base(env)
    {
        this._productService = productService;
        this.env = env;
        rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
       => FromAppResponse(await _productService.CreateProductAsync(dto, ct));


    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ProductListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _productService.GetProductsAsync(query, ct));


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
    => FromAppResponse(await _productService.UpdateProductAsync(id, dto, ct));


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] bool includeDeleted = false, CancellationToken ct = default)
        => FromAppResponse(await _productService.GetProductByIdAsync(id, includeDeleted, ct));

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted(
    [FromQuery] int skip = 0,
    [FromQuery] int take = 20,
    CancellationToken ct = default)
    => FromAppResponse(await _productService.GetDeletedProductsAsync(skip, take, ct));





    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate([FromRoute] int id, [FromBody] ProductStateChangeDto dto, CancellationToken ct)
    => FromAppResponse(await _productService.SetActiveAsync(id, true, dto, ct));

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] int id, [FromBody] ProductStateChangeDto dto, CancellationToken ct)
        => FromAppResponse(await _productService.SetActiveAsync(id, false, dto, ct));


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDelete(
        [FromRoute] int id,
        [FromBody] ProductStateChangeDto dto,
        CancellationToken ct)
        => FromAppResponse(await _productService.SetDeletedAsync(id, true, dto, ct));

    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> Restore([FromRoute] int id,[FromBody] ProductStateChangeDto dto,CancellationToken ct)
        => FromAppResponse(await _productService.SetDeletedAsync(id, false, dto, ct));



    //----------------------------------------------
    // Units
    //----------------------------------------------

    [HttpPost("{productId:int}/units")]
    public async Task<IActionResult> AddUnit([FromRoute] int productId,[FromBody] ProductUnitCreateDto dto,CancellationToken ct)
         =>FromAppResponse(await _productService.AddProductUnitAsync(productId, dto, ct));



    //----------------------------------------------
    // ProductBatches 
    //----------------------------------------------


    [HttpPost("{productId:int}/open-box")]
    public async Task<IActionResult> OpenBox(
    [FromRoute] int productId,
    [FromBody] OpenBoxDto dto,
    CancellationToken ct)
    {
        return FromAppResponse(await _productService.OpenBoxAsync(productId, dto, ct));
    }










    // audit
    [HttpGet("{id:int}/audit")]
    public async Task<IActionResult> GetAudit(
    [FromRoute] int id,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 20,
    CancellationToken ct = default)
    => FromAppResponse(await _productService.GetProductAuditAsync(id, skip, take, ct));


    [HttpGet("audit/search")]
    public async Task<IActionResult> SearchAudit(
    [FromQuery] string? userId = null,
    [FromQuery] DateTime? fromUtc = null,
    [FromQuery] DateTime? toUtc = null,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 20,
    CancellationToken ct = default)
    => FromAppResponse(await _productService.SearchProductAuditAsync(userId, fromUtc, toUtc, skip, take, ct));

}