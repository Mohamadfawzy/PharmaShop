using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.RequestFeatures;
namespace WebAPI.Controllers;


[Route("api/categories")]
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


    [HttpGet("boom")]
    public IActionResult Boom() => throw new Exception("Test exception");



    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagingParameters paging, CancellationToken ct)
    {
        var response = await categoryService.GetAllCategoriesAsync(paging.PageNumber, paging.PageSize, ct);

        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCategories([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var response = await categoryService.GetAllCategoriesAsync(pageNumber, pageSize, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet("root")]
    public async Task<IActionResult> GetRootCategories(CancellationToken ct)
    {
        var response = await categoryService.GetRootCategoriesAsync(ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree(CancellationToken ct)
    {
        var response = await categoryService.GetCategoryTreeAsync(ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }
}