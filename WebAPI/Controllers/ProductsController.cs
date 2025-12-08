using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.RequestFeatures;
using WebAPI.SpecificDtos;

namespace WebAPI.Controllers;

[Route("api/Products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService productService;
    private readonly IWebHostEnvironment env;

    public ProductsController(IProductService productService, IWebHostEnvironment env)
    {
        this.productService = productService;
        this.env = env;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductParameters parameters)
    {
        var res = await productService.GetProductsAsync(parameters);
        return Ok(res);
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var res = new ProductParameters();
        return Ok(res);
    }

    [HttpPost("create-with-images")]
    public async Task<IActionResult> CreateWithImages([FromForm] ProductCreateWithImageDto dto, CancellationToken ct)
    {
        var rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");

        if (dto.Images == null || dto.Images.Count == 0)
            return BadRequest(new { Message = "At least 1 image is required." });

        // نحول الصور إلى Streams
        var streams = dto.Images.Select(img => img.OpenReadStream());

        var response = await productService.CreateProductWithImagesAsync(dto, streams, rootPath, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }


    //[HttpPost("upload")]
    //public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
    //{
    //    if (file == null || file.Length == 0)
    //        return BadRequest("لم يتم رفع أي صورة.");

    //    var rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");

    //    try
    //    {
    //        await using var stream = file.OpenReadStream();

    //        // استخدم اسم الملف بدون الامتداد
    //        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);

    //        var result = await productService.CreateProductWithImagesAsync(stream, rootPath);

    //        return Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, $"حدث خطأ أثناء رفع الصورة: {ex.Message} {ex.InnerException?.Message}");
    //    }
    //}

}
