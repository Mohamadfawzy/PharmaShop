using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;
using SixLabors.ImageSharp;
using WebAPI.SpecificDtos;

namespace WebAPI.Controllers;

[Route("api/Products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService productService;
    private readonly IWebHostEnvironment env;
    private readonly string rootPath;

    public ProductsController(IProductService productService, IWebHostEnvironment env)
    {
        this.productService = productService;
        this.env = env;
        rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
    }

    // ===================================================================================

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto productDto, CancellationToken ct)
    {
        var productId = await productService.CreateProductAsync(productDto, ct);
        if (productId > 0)
            return Ok(AppResponse<int>.Success(productId));

        return BadRequest(AppResponse.Fail("error"));
    }


    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductParameters parameters)
    {
        var res = await productService.GetProductsAsync(parameters);
        return Ok(res);
    }

    [HttpPost("create-with-images")]
    public async Task<IActionResult> CreateWithImages([FromForm] ProductCreateWithImageDto dto, CancellationToken ct)
    {
        if (dto.Images == null || dto.Images.Count == 0)
            return BadRequest(new { Message = "At least 1 image is required." });

        // نحول الصور إلى Streams
        var streams = dto.Images.Select(img => img.OpenReadStream());

        var response = await productService.CreateProductWithImagesAsync(dto, streams, rootPath, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpPost("{productId}/images")]
    public async Task<IActionResult> AddImages([FromForm] List<IFormFile>? images, int productId, CancellationToken ct)
    {
        try
        {

            if (images == null || images.Count == 0)
                return BadRequest(new { Message = "At least 1 image is required." });

            // نحول الصور إلى Streams
            var streams = images.Select(img => img.OpenReadStream());

            var response = await productService.AddProductImagesAsync(streams, productId, rootPath, ct);

            return Ok(AppResponse.Success());
        }
        catch (Exception ex)
        {
            return BadRequest(AppResponse.Fail(ex.Message + ex.InnerException?.Message));
        }
    }

    [HttpDelete("{productId:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteProductImage(int productId, int imageId, CancellationToken ct)
    {
        try
        {
            await productService.DeleteProductImageAsync(productId, imageId, rootPath, ct);
            return Ok(AppResponse.Success("Image deleted successfully."));
        }
        catch (Exception ex)
        {
            // يمكن تخصيص الرسالة أو استخدام ErrorCode حسب نوع الخطأ
            return BadRequest(AppResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
    {
        var response = await productService.UpdateProductAsync(id, dto, ct);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id:int}/soft-delete")]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
    {
        var response = await productService.SoftDeleteProductAsync(id, ct);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPatch("{productId:int}/active")]
    public async Task<IActionResult> UpdateIsActive(int productId,[FromQuery] bool isActive,CancellationToken ct)
    {
        var response = await productService.UpdateProductIsActiveAsync(productId,isActive,ct);

        return StatusCode((int)response.StatusCode, response);
    }

}