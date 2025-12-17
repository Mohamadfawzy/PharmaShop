using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.Enums;
using Shared.Models.Dtos.Category;
using Shared.Responses;

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

    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> UpdateCategory(int categoryId,[FromBody] CategoryUpdateDto dto,CancellationToken ct)
    {
        if (categoryId != dto.Id)
            return BadRequest(
                AppResponse.Fail(
                    "Category ID mismatch",
                    AppErrorCode.ValidationError
                )
            );

        var response = await categoryService.UpdateAsync(categoryId,dto, ct);

        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery]int pageNumber, int pageSize,CancellationToken ct)
    {
        var response = await categoryService.GetAllCategoriesAsync(pageNumber,pageSize, ct);

        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpPut("{categoryId}/change-parent")]
    public async Task<IActionResult> ChangeCategoryParent(int categoryId, [FromQuery] int? newParentCategoryId,CancellationToken ct)
    {
        var response = await categoryService
            .ChangeCategoryParentAsync(categoryId, newParentCategoryId, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpPut("{categoryId:int}/image")]
    public async Task<IActionResult> UpdateCategoryImage(int categoryId,IFormFile image,CancellationToken ct)
    {
        using var stream = image.OpenReadStream();
        var response = await categoryService.UpdateCategoryImageAsync(categoryId, stream, rootPath, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }



    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetById(int categoryId, CancellationToken ct)
    {
        var response = await categoryService.GetCategoryByIdAsync(categoryId, ct);

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

    /*
A
-> a -->
    ---> c1
        q2
q3
B
C
     */






}
