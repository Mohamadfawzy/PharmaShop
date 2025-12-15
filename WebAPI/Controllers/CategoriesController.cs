using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Category;

namespace WebAPI.Controllers;

[Route("api/Categories")]
[ApiController]
public class CategoriesController : ControllerBase
{

    private readonly ICategoryService categoryService;
    private readonly IWebHostEnvironment env;
    private readonly string rootPath;

    public CategoriesController(ICategoryService categoryService, IWebHostEnvironment env)
    {
        this.categoryService = categoryService;
        this.env = env;
        rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
    }

    // ===================================================================================

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryCreateDto dto, CancellationToken ct)
    {
        var response = await categoryService.CreateCategoryAsync(dto, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }


}
