using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.Extensions.Logging;
using Repository;
using Shared.Models.Dtos.Category;
using Shared.Responses;
using System.Net;

namespace Service;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService imageService;
    private readonly ILogger<CategoryService> logger;

    public CategoryService(IUnitOfWork unitOfWork, IImageService imageService, ILogger<CategoryService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
    }

    public async Task<AppResponse<int>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken ct)
    {
        // Validate parent category if provided
        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await unitOfWork.Categories
                .ExistsByIdAsync(dto.ParentCategoryId.Value, ct);

            if (!parentExists)
                return AppResponse<int>.Fail("Parent category does not exist");
        }

        var category = new Category
        {
            Name = dto.Name,
            NameEn = dto.NameEn,
            DescriptionEn = dto.DescriptionEn,
            ImageUrl = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Categories.AddAsync(category, ct);
        await unitOfWork.CompleteAsync(ct);

        return AppResponse<int>.Success(category.Id, "Category created successfully");
    }


    public async Task<AppResponse<bool>> UpdateCategoryAsync(int categoryId, CategoryUpdateDto dto,
    CancellationToken ct)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
        if (category == null)
            return AppResponse<bool>.Fail(
                "Category not found");

        category.Name = dto.Name;
        category.NameEn = dto.NameEn;
        category.DescriptionEn = dto.DescriptionEn;
        category.ImageUrl = dto.ImageUrl;
        category.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.Categories.UpdateAsync(category, ct);
        await unitOfWork.CompleteAsync(ct);

        return AppResponse<bool>.Success(true, "Category updated successfully");
    }

}
