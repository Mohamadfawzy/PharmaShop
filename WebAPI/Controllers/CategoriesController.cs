using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Category;
using Shared.Models.RequestFeatures;
using Shared.Responses;
namespace WebAPI.Controllers;


[Route("api/v1/categories")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ========================= Queries =========================

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagingParameters paging, CancellationToken ct)
    {
        var pageNumber = paging?.PageNumber ?? 1;
        var pageSize = paging?.PageSize ?? 10;

        return FromAppResponse(await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize, ct));
    }

    [HttpGet("root")]
    public async Task<IActionResult> GetRoots(CancellationToken ct)
        => FromAppResponse(await _categoryService.GetRootCategoriesAsync(ct));

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken ct)
        => FromAppResponse(await _categoryService.GetCategoryTreeAsync(ct));
}

/*

[Route("api/categories")]
[ApiController]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ========================= Helpers =========================

    private IActionResult FromAppResponse<T>(AppResponse<T> response)
        => response.IsSuccess
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);

    // ========================= Queries (Public) =========================

    /// <summary>Get all active categories with paging</summary>
    [HttpGet]
    [ProducesResponseType(typeof(AppResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<List<CategoryDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<List<CategoryDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] PagingParameters paging, CancellationToken ct)
    {
        var pageNumber = paging?.PageNumber ?? 1;
        var pageSize = paging?.PageSize ?? 10;

        var response = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize, ct);
        return FromAppResponse(response);
    }

    /// <summary>Legacy alias for older clients (redirects to GET /api/categories)</summary>
    [HttpGet("active")]
    [ApiExplorerSettings(IgnoreApi = true)] // لا تظهر في Swagger (اختياري)
    public IActionResult ActiveLegacy([FromQuery] PagingParameters paging)
    {
        // 301 أو 307 حسب رغبتك، 307 يحافظ على method
        return RedirectPermanent($"/api/categories?pageNumber={paging?.PageNumber ?? 1}&pageSize={paging?.PageSize ?? 10}");
    }

    /// <summary>Get root categories</summary>
    [HttpGet("root")]
    [ProducesResponseType(typeof(AppResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<List<CategoryDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRoots(CancellationToken ct)
    {
        var response = await _categoryService.GetRootCategoriesAsync(ct);
        return FromAppResponse(response);
    }

    /// <summary>Get categories tree</summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(AppResponse<List<CategoryTreeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<List<CategoryTreeDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var response = await _categoryService.GetCategoryTreeAsync(ct);
        return FromAppResponse(response);
    }
}
 */