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
            //ImageUrl = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Categories.AddAsync(category, ct);
        await unitOfWork.CompleteAsync(ct);

        return AppResponse<int>.Success(category.Id, "Category created successfully");
    }


    public async Task<AppResponse<bool>> UpdateAsync(int categoryId, CategoryUpdateDto dto, CancellationToken ct)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
        if (category == null)
            return AppResponse<bool>.Fail(
                "Category not found");

        category.Name = dto.Name;
        category.NameEn = dto.NameEn;
        category.DescriptionEn = dto.DescriptionEn;
        //category.ImageUrl = dto.ImageUrl;
        category.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.Categories.UpdateAsync(category);
        await unitOfWork.CompleteAsync(ct);

        return AppResponse<bool>.Success(true, "Category updated successfully");
    }

    public async Task<AppResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        // 1️⃣ Get total count (WITHOUT pagination)
        var totalCount = await unitOfWork.Categories.CountAsync(
            c => !c.IsDeleted,
            ct
        );

        if (totalCount == 0)
        {
            return AppResponse<IEnumerable<CategoryDto>>.Success(
                Enumerable.Empty<CategoryDto>(),
                paginationInfo: PaginationInfo.Empty(pageNumber, pageSize)
            );
        }

        // 2️⃣ Calculate skip
        int skip = (pageNumber - 1) * pageSize;

        // 3️⃣ Get paginated data
        var categories = await unitOfWork.Categories.GetAllAsync(
            filter: c => !c.IsDeleted,
            orderBy: q => q.OrderBy(c => c.Name),
            skip: skip,
            take: pageSize
        );

        // 4️⃣ Map to DTO
        var result = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            NameEn = c.NameEn,
            ParentCategoryId = c.ParentCategoryId,
            IsActive = c.IsActive
        });

        // 5️ Create pagination info
        var pagination = PaginationInfo.Create(
            pageNumber,
            pageSize,
            totalCount
        );

        // 6️ Return AppResponse
        return AppResponse<IEnumerable<CategoryDto>>.Success(
            result,
            pagination
        );
    }


    public async Task<AppResponse<bool>> ChangeCategoryParentAsync(int categoryId, int? newParentCategoryId, CancellationToken ct)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);

        if (category is null || category.IsDeleted)
            return AppResponse<bool>.Fail("Category not found");

        // منع جعل التصنيف والدًا لنفسه
        if (newParentCategoryId == categoryId)
            return AppResponse<bool>.Fail("Category cannot be parent of itself");

        // التحقق من وجود التصنيف الأب (إن وُجد)
        if (newParentCategoryId.HasValue)
        {
            var parent = await unitOfWork.Categories
                .GetByIdAsync(newParentCategoryId.Value, ct);

            if (parent is null || parent.IsDeleted)
                return AppResponse<bool>.Fail("Parent category not found");
        }

        category.ParentCategoryId = newParentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.Categories.UpdateAsync(category, ct);
        await unitOfWork.CompleteAsync(ct);

        return AppResponse<bool>.Success(true);
    }


    //public async Task<bool> UpdateCategoryImageAsync(int categoryId, Stream newImageStream, string rootPath, CancellationToken ct)
    //{
    //    var newFileName = string.Empty;
    //    if (newImageStream == null || newImageStream.Length == 0)
    //        throw new ArgumentException("Invalid image stream.", nameof(newImageStream));

    //    // 1️ Ensure category exists
    //    var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
    //    if (category == null)
    //        throw new Exception("Category not found");

    //    var savedImageNames = new List<string>();

    //    // Begin transaction
    //    await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

    //    try
    //    {
    //        // 2️ Save the new image
    //        newFileName = await imageService.SaveImageAsync(newImageStream, rootPath, ct);
    //        savedImageNames.Add(newFileName);

    //        // 3️ Delete old image from file system if exists
    //        if (!string.IsNullOrWhiteSpace(category.ImageUrl))
    //        {
    //            await imageService.RemoveImageAsync(category.ImageUrl, rootPath);
    //        }

    //        // 4️ Update ImageUrl in database
    //        await unitOfWork.Categories.UpdateImageUrlAsync(categoryId,newFileName ,ct);
    //        await unitOfWork.Categories.UpdateAsync(category, ct);

    //        // 5️ Save changes
    //        await unitOfWork.CompleteAsync(ct);

    //        // 6️ Commit transaction
    //        await transaction.CommitAsync(ct);

    //        return true;
    //    }
    //    catch
    //    {
    //        // Rollback transaction
    //        await transaction.RollbackAsync(ct);

    //        // Remove newly saved image if something failed
    //        await imageService.RemoveImageAsync(newFileName, rootPath);
    //        throw;
    //    }
    //}

    public async Task<AppResponse<string>> UpdateCategoryImageAsync(int categoryId,Stream newImageStream,string rootPath,CancellationToken ct)
    {
        if (newImageStream == null || newImageStream.Length == 0)
            return AppResponse<string>.Fail("Invalid image stream.");

        string newFileName = string.Empty;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Ensure category exists
            var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
            if (category == null)
                return AppResponse<string>.Fail("Category not found.");

            // Save the new image
            string imagePrefix = $"category-{categoryId}";
            newFileName = await imageService.SaveImageAsync(newImageStream, rootPath, imagePrefix, ct);

            // Remove old image if exists
            if (!string.IsNullOrWhiteSpace(category.ImageUrl))
            {
                await imageService.RemoveImageAsync(category.ImageUrl, rootPath);
            }

            // Update ImageUrl using ExecuteUpdateAsync

            var updatedRows =  await unitOfWork.Categories.UpdateImageUrlAsync(categoryId, newFileName, ct);


            if (!updatedRows)
            {
                // Cleanup saved image if update failed
                await imageService.RemoveImageAsync(newFileName, rootPath);
                return AppResponse<string>.Fail("Failed to update category image.");
            }

            await unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);

            return AppResponse<string>.Success(newFileName, "Category image updated successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            // Remove newly saved image if something failed
            if (!string.IsNullOrWhiteSpace(newFileName))
                await imageService.RemoveImageAsync(newFileName, rootPath);

            return AppResponse<string>.Fail("An error occurred while updating category image."+ ex.Message);
        }
    }


    public async Task<AppResponse<CategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct)
    {
        var category = await unitOfWork.Categories.GetByIdNoTrackingAsync(categoryId, ct);

        if (category == null || category.IsDeleted)
            return AppResponse<CategoryDto>.NotFound("Category not found");

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            NameEn = category.NameEn,
            ParentCategoryId = category.ParentCategoryId,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive
        };

        return AppResponse<CategoryDto>.Success(dto);
    }


    public async Task<AppResponse<IEnumerable<CategoryDto>>> GetRootCategoriesAsync(CancellationToken ct)
    {
        var categories = await unitOfWork.Categories.GetRootCategoriesAsync(ct);

        if (!categories.Any())
            return AppResponse<IEnumerable<CategoryDto>>.Success(Enumerable.Empty<CategoryDto>());

        var result = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            NameEn = c.NameEn,
            ImageUrl = c.ImageUrl,
            IsActive = c.IsActive
        });

        return AppResponse<IEnumerable<CategoryDto>>.Success(result);
    }








}
