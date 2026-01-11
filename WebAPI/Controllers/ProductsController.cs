using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;
using SixLabors.ImageSharp;
using WebAPI.Controllers.Admin;
using WebAPI.SpecificDtos;

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


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        [FromRoute] int id, 
        [FromBody] ProductUpdateDto dto,
        CancellationToken ct)
    => FromAppResponse(await _productService.UpdateProductAsync(id, dto, ct));


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    => FromAppResponse(await _productService.GetProductByIdAsync(id, ct));
}