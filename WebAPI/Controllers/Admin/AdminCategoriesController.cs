using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Dtos.Category;
using Shared.Responses;

namespace WebAPI.Controllers.Admin;


[Route("api/v1/admin/categories")]
public class AdminCategoriesController : AdminBaseApiController
{
    private readonly ICategoryService _categoryService;

    private const long MaxImageBytes = 5 * 1024 * 1024; // 5MB
    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public AdminCategoriesController(
        ICategoryService categoryService,
        IWebHostEnvironment env) : base(env)
    {
        _categoryService = categoryService;
    }

    // ========================= Queries =========================

    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetById(int categoryId, CancellationToken ct)
        => FromAppResponse(await _categoryService.GetCategoryByIdAsync(categoryId, ct));

    // ========================= Commands =========================

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto, CancellationToken ct)
        => FromAppResponse(await _categoryService.CreateCategoryAsync(dto, ct));

    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> Update(int categoryId, [FromBody] CategoryUpdateDto dto, CancellationToken ct)
    {
        if (dto is null)
            return ValidationError("Request body is required");

        if (categoryId != dto.Id)
            return ValidationError("Category ID mismatch");

        return FromAppResponse(await _categoryService.UpdateAsync(categoryId, dto, ct));
    }

    [HttpPut("{categoryId:int}/parent")]
    public async Task<IActionResult> ChangeParent(int categoryId, [FromQuery] int? newParentCategoryId, CancellationToken ct)
        => FromAppResponse(await _categoryService.ChangeCategoryParentAsync(categoryId, newParentCategoryId, ct));

    [HttpPut("{categoryId:int}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateImage(int categoryId, [FromForm] IFormFile image, CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return BadRequest(AppResponse<string>.ValidationError("Image is required"));

        if (image.Length > MaxImageBytes)
            return BadRequest(AppResponse<string>.ValidationError($"Image size must be <= {MaxImageBytes / (1024 * 1024)}MB"));

        if (!AllowedImageContentTypes.Contains(image.ContentType))
            return BadRequest(AppResponse<string>.ValidationError("Unsupported image type. Allowed: jpg, png, webp"));

        await using var stream = image.OpenReadStream();
        return FromAppResponse(await _categoryService.UpdateCategoryImageAsync(categoryId, stream, UploadsRootPath, ct));
    }

    [HttpPatch("{categoryId:int}/activate")]
    public async Task<IActionResult> Activate(int categoryId, CancellationToken ct)
        => FromAppResponse(await _categoryService.SetCategoryActiveStatusAsync(categoryId, true, ct));

    [HttpPatch("{categoryId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int categoryId, CancellationToken ct)
        => FromAppResponse(await _categoryService.SetCategoryActiveStatusAsync(categoryId, false, ct));
}

/*
[Route("api/admin/categories")]
[ApiController]
[Authorize(Roles = "Admin")] // عدّلها حسب نظامك
[Produces("application/json")]
public class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _env;

    // ضبط رفع الملفات (قيم واقعية)
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5MB
    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public AdminCategoriesController(ICategoryService categoryService, IWebHostEnvironment env)
    {
        _categoryService = categoryService;
        _env = env;
    }

    // ========================= Helpers =========================

    private string GetUploadsRootPath()
        => Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads");

    private IActionResult FromAppResponse<T>(AppResponse<T> response)
        => response.IsSuccess
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);

    // ========================= Queries =========================

    /// <summary>Get category details by id (Admin)</summary>
    [HttpGet("{categoryId:int}")]
    [ProducesResponseType(typeof(AppResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<CategoryDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<CategoryDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int categoryId, CancellationToken ct)
    {
        var response = await _categoryService.GetCategoryByIdAsync(categoryId, ct);
        return FromAppResponse(response);
    }

    // ========================= Commands =========================

    /// <summary>Create category (Admin)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<int>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<int>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<int>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto, CancellationToken ct)
    {
        var response = await _categoryService.CreateCategoryAsync(dto, ct);
        return FromAppResponse(response);
    }

    /// <summary>Update category basic fields (Admin)</summary>
    [HttpPut("{categoryId:int}")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int categoryId, [FromBody] CategoryUpdateDto dto, CancellationToken ct)
    {
        if (dto is null)
            return BadRequest(AppResponse.Fail("Request body is required", AppErrorCode.ValidationError));

        if (categoryId != dto.Id)
            return BadRequest(AppResponse.Fail("Category ID mismatch", AppErrorCode.ValidationError));

        var response = await _categoryService.UpdateAsync(categoryId, dto, ct);
        return FromAppResponse(response);
    }

    /// <summary>Change category parent (Admin)</summary>
    [HttpPut("{categoryId:int}/parent")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeParent(
        int categoryId,
        [FromQuery] int? newParentCategoryId,
        CancellationToken ct)
    {
        var response = await _categoryService.ChangeCategoryParentAsync(categoryId, newParentCategoryId, ct);
        return FromAppResponse(response);
    }

    /// <summary>Update category image (Admin)</summary>
    [HttpPut("{categoryId:int}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateImage(
        int categoryId,
        [FromForm] IFormFile image,
        CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return BadRequest(AppResponse<string>.ValidationError("Image is required"));

        if (image.Length > MaxImageBytes)
            return BadRequest(AppResponse<string>.ValidationError($"Image size must be <= {MaxImageBytes / (1024 * 1024)}MB"));

        if (!AllowedImageContentTypes.Contains(image.ContentType))
            return BadRequest(AppResponse<string>.ValidationError("Unsupported image type. Allowed: jpg, png, webp"));

        await using var stream = image.OpenReadStream();
        var rootPath = GetUploadsRootPath();

        var response = await _categoryService.UpdateCategoryImageAsync(categoryId, stream, rootPath, ct);
        return FromAppResponse(response);
    }

    /// <summary>Activate category (Admin)</summary>
    [HttpPatch("{categoryId:int}/activate")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Activate(int categoryId, CancellationToken ct)
    {
        var response = await _categoryService.SetCategoryActiveStatusAsync(categoryId, true, ct);
        return FromAppResponse(response);
    }

    /// <summary>Deactivate category (Admin)</summary>
    [HttpPatch("{categoryId:int}/deactivate")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deactivate(int categoryId, CancellationToken ct)
    {
        var response = await _categoryService.SetCategoryActiveStatusAsync(categoryId, false, ct);
        return FromAppResponse(response);
    }
}
 */