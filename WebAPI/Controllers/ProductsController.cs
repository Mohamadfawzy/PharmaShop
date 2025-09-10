using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Models.Dtos.Product;
using WebAPI.SpecificDtos;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
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
    public async Task<IActionResult> GetAll()
    {
        var res = await productService.ReadAllProducts();
        return Ok(res);
    }

    [HttpPost("create-with-images")]
    public async Task<IActionResult> CreateWithImages([FromForm] ProductCreateWithImageDto dto, CancellationToken ct)
    {
        var rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");

        using var stream = dto.Image?.OpenReadStream();

        var productId = await productService.CreateProductWithImagesAsync(dto, stream, rootPath);

        return Ok(new { ProductId = productId, Message = "Product created successfully." });
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
