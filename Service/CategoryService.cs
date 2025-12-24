using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.Extensions.Logging;
using Shared.Models.Dtos.Category;
using Shared.Extensions;
using Shared.Responses;
using System.Net;

namespace Service;

public class CategoryService : ICategoryService
{
    private const int MaxPageSize = 100;

    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService imageService;
    private readonly ILogger<CategoryService> logger;
    private readonly ICurrentUserService currentUser; // ✅ (3) إزالة الـ hardcode

    public CategoryService(
        IUnitOfWork unitOfWork,
        IImageService imageService,
        ILogger<CategoryService> logger,
        ICurrentUserService currentUser)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
        this.currentUser = currentUser;
    }

    // ===================================================================================
    // (1) استخدم ErrorCode/StatusCode الصحيحين بدل Fail عام
    // ===================================================================================

    public async Task<AppResponse<int>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken ct)
    {
        try
        {
            if (dto.ParentCategoryId.HasValue)
            {
                // ✅ الأفضل: ExistsByIdAsync يتأكد أيضًا من !IsDeleted داخل الـRepo
                var parentExists = await unitOfWork.Categories.ExistsByIdAsync(dto.ParentCategoryId.Value, ct);
                if (!parentExists)
                    return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
                    {
                        ["ParentCategoryId"] = ["Parent category does not exist"]
                    });
            }

            var category = new Category
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                DescriptionEn = dto.DescriptionEn,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Categories.AddAsync(category, ct);
            await unitOfWork.CompleteAsync(ct);

            return AppResponse<int>.Ok(category.Id, "Category created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateCategoryAsync failed");
            return AppResponse<int>.InternalError("Failed to create category");
        }
    }

    // ===================================================================================
    // (2) Transaction فقط عند وجود تغييرات + Audit
    // (4) Logging في catch
    // ===================================================================================

    public async Task<AppResponse<bool>> UpdateAsync(int categoryId, CategoryUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
            if (category is null || category.IsDeleted)
                return AppResponse<bool>.NotFound("Category not found");

            var userId = currentUser.UserId ?? "system";
            var auditLogs = BuildUpdateAuditLogs(category, dto, userId);

            // ✅ (2) لا تبدأ Transaction إن لا تغييرات
            if (auditLogs.Count == 0)
                return AppResponse<bool>.Ok(true, "No changes detected");

            // Apply changes
            ApplyUpdate(category, dto);
            category.UpdatedAt = DateTime.UtcNow;

            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            await unitOfWork.Categories.UpdateAsync(category, ct);
            await unitOfWork.Categories.AddCategoryAuditLogsRangeAsync(auditLogs);

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse<bool>.Ok(true, "Category updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateAsync failed for CategoryId={CategoryId}", categoryId);
            return AppResponse<bool>.InternalError("Failed to update category");
        }
    }

    // ===================================================================================
    // (9) Pagination validation لا تُخفي أخطاء العميل
    // (10) رجّع List بدلاً من IEnumerable لتفادي deferred execution
    // ===================================================================================

    public async Task<AppResponse<List<CategoryDto>>> GetAllCategoriesAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        var pagingValidation = ValidatePaging(pageNumber, pageSize);
        if (!pagingValidation.IsSuccess)
            return pagingValidation.FromError<List<CategoryDto>>(); // Extension (FromError<T>)

        pageNumber = NormalizePageNumber(pageNumber);
        pageSize = NormalizePageSize(pageSize);

        try
        {
            var totalCount = await unitOfWork.Categories.CountAsync(
                c => !c.IsDeleted && c.IsActive,
                ct
            );

            if (totalCount == 0)
            {
                return AppResponse<List<CategoryDto>>.Ok(
                    new List<CategoryDto>(),
                    PaginationInfo.Create(pageNumber, pageSize, 0)
                );
            }

            var skip = (pageNumber - 1) * pageSize;

            var categories = await unitOfWork.Categories.GetAllAsync(
                criteria: c => c.IsActive && !c.IsDeleted,
                orderBy: q => q.OrderBy(c => c.Name),
                skip: skip,
                take: pageSize,
                ct: ct
            );

            var result = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                NameEn = c.NameEn,
                ParentCategoryId = c.ParentCategoryId,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive
            }).ToList();

            var pagination = PaginationInfo.Create(pageNumber, pageSize, totalCount);

            return AppResponse<List<CategoryDto>>.Ok(result, pagination);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAllCategoriesAsync failed page={PageNumber}, size={PageSize}", pageNumber, pageSize);
            return AppResponse<List<CategoryDto>>.InternalError("Failed to load categories");
        }
    }

    // ===================================================================================
    // (7) ChangeParent: منع cycles + rules واضحة
    // ===================================================================================

    public async Task<AppResponse<bool>> ChangeCategoryParentAsync(int categoryId, int? newParentCategoryId, CancellationToken ct)
    {
        try
        {
            var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
            if (category is null || category.IsDeleted)
                return AppResponse<bool>.NotFound("Category not found");

            if (newParentCategoryId == categoryId)
                return AppResponse<bool>.ValidationError("Category cannot be parent of itself");

            if (newParentCategoryId.HasValue)
            {
                var parent = await unitOfWork.Categories.GetByIdNoTrackingAsync(newParentCategoryId.Value, ct);
                if (parent is null || parent.IsDeleted)
                    return AppResponse<bool>.NotFound("Parent category not found");

                // ✅ (7) Cycle check
                var cycle = await WouldCreateCycleAsync(categoryId, newParentCategoryId.Value, ct);
                if (cycle)
                    return AppResponse<bool>.BusinessRuleViolation("Invalid parent: would create a cycle");
            }

            category.ParentCategoryId = newParentCategoryId;
            category.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.Categories.UpdateAsync(category, ct);
            await unitOfWork.CompleteAsync(ct);

            return AppResponse<bool>.Ok(true, "Parent updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ChangeCategoryParentAsync failed CategoryId={CategoryId}, NewParentId={NewParentId}",
                categoryId, newParentCategoryId);
            return AppResponse<bool>.InternalError("Failed to change category parent");
        }
    }

    // ===================================================================================
    // (6) Image update: لا تحذف القديم قبل نجاح commit
    // (5) لا تُظهر ex.Message للعميل
    // (4) Logging
    // ===================================================================================

    public async Task<AppResponse<string>> UpdateCategoryImageAsync(int categoryId, Stream newImageStream, string rootPath, CancellationToken ct)
    {
        if (newImageStream is null || !newImageStream.CanRead || newImageStream.Length == 0)
            return AppResponse<string>.ValidationError("Invalid image stream");

        string? newFileName = null;
        string? oldFileName = null;

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
            if (category is null || category.IsDeleted)
                return AppResponse<string>.NotFound("Category not found");

            oldFileName = category.ImageUrl;

            // Save new image to disk first
            var prefix = $"category-{categoryId}";
            newFileName = await imageService.SaveImageAsync(newImageStream, rootPath, prefix, ct);

            // Update DB to point to the new image
            var updated = await unitOfWork.Categories.UpdateImageUrlAsync(categoryId, newFileName, ct);

            if (!updated)
            {
                // DB update failed => remove saved new image
                await imageService.RemoveImageAsync(newFileName, rootPath);
                return AppResponse<string>.InternalError("Failed to update category image");
            }

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            // ✅ حذف القديم بعد نجاح commit (Best-effort)
            if (!string.IsNullOrWhiteSpace(oldFileName))
            {
                try
                {
                    await imageService.RemoveImageAsync(oldFileName, rootPath);
                }
                catch (Exception exDel)
                {
                    logger.LogWarning(exDel, "Failed to remove old category image {OldImage}", oldFileName);
                }
            }

            return AppResponse<string>.Ok(newFileName, "Category image updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateCategoryImageAsync failed CategoryId={CategoryId}", categoryId);

            try
            {
                await tx.RollbackAsync(ct);
            }
            catch (Exception exRb)
            {
                logger.LogWarning(exRb, "Rollback failed in UpdateCategoryImageAsync");
            }

            // Remove newly saved image if exists
            if (!string.IsNullOrWhiteSpace(newFileName))
            {
                try { await imageService.RemoveImageAsync(newFileName, rootPath); }
                catch (Exception exDelNew) { logger.LogWarning(exDelNew, "Failed to remove new image {NewImage}", newFileName); }
            }

            return AppResponse<string>.InternalError("An error occurred while updating category image");
        }
    }

    // ===================================================================================

    public async Task<AppResponse<CategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct)
    {
        try
        {
            var category = await unitOfWork.Categories.GetByIdNoTrackingAsync(categoryId, ct);

            if (category is null || category.IsDeleted)
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

            return AppResponse<CategoryDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetCategoryByIdAsync failed CategoryId={CategoryId}", categoryId);
            return AppResponse<CategoryDto>.InternalError("Failed to load category");
        }
    }

    public async Task<AppResponse<List<CategoryDto>>> GetRootCategoriesAsync(CancellationToken ct)
    {
        try
        {
            var categories = await unitOfWork.Categories.GetRootCategoriesAsync(ct);

            var result = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                NameEn = c.NameEn,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive
            }).ToList();

            return AppResponse<List<CategoryDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetRootCategoriesAsync failed");
            return AppResponse<List<CategoryDto>>.InternalError("Failed to load root categories");
        }
    }

    public async Task<AppResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken ct)
    {
        try
        {
            var categories = await unitOfWork.Categories.GetAllForTreeAsync(ct);

            if (categories.Count == 0)
                return AppResponse<List<CategoryTreeDto>>.Ok(new List<CategoryTreeDto>());

            var dict = categories.ToDictionary(
                c => c.Id,
                c => new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NameEn = c.NameEn,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive
                });

            var roots = new List<CategoryTreeDto>();

            foreach (var c in categories)
            {
                if (c.ParentCategoryId is null)
                {
                    roots.Add(dict[c.Id]);
                }
                else if (dict.TryGetValue(c.ParentCategoryId.Value, out var parentNode))
                {
                    parentNode.Children.Add(dict[c.Id]);
                }
            }

            return AppResponse<List<CategoryTreeDto>>.Ok(roots);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetCategoryTreeAsync failed");
            return AppResponse<List<CategoryTreeDto>>.InternalError("Failed to load categories tree");
        }
    }

    // ===================================================================================
    // (8) SetActiveStatus: تحقق من affected rows (لا ترجع نجاح دائمًا)
    // NOTE: يتطلب تعديل Repo لتعيد bool / affectedRows
    // ===================================================================================

    public async Task<AppResponse<bool>> SetCategoryActiveStatusAsync(int categoryId, bool isActive, CancellationToken ct)
    {
        try
        {
            // ✅ يتطلب Repo method يرجع bool:
            // Task<bool> SetActiveStatusAsync(int categoryId, bool isActive, CancellationToken ct);
            var updated = await unitOfWork.Categories.SetActiveStatusAsync(categoryId, isActive, ct);

            if (!updated)
                return AppResponse<bool>.NotFound("Category not found");

            await unitOfWork.CompleteAsync(ct);
            return AppResponse<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SetCategoryActiveStatusAsync failed CategoryId={CategoryId}", categoryId);
            return AppResponse<bool>.InternalError("Failed to update category status");
        }
    }



    // ===================================================================================
    // Helpers
    // ===================================================================================

    private static void ApplyUpdate(Category category, CategoryUpdateDto dto)
    {
        category.Name = dto.Name;
        category.NameEn = dto.NameEn;
        category.DescriptionEn = dto.DescriptionEn;
    }

    private static List<CategoryAuditLog> BuildUpdateAuditLogs(Category category, CategoryUpdateDto dto, string userId)
    {
        var logs = new List<CategoryAuditLog>();

        void Add(string field, string? oldVal, string? newVal)
        {
            logs.Add(new CategoryAuditLog
            {
                CategoryId = category.Id,
                ChangeType = "Update",
                FieldName = field,
                OldValue = oldVal,
                NewValue = newVal,
                ChangedBy = userId,
                ChangeDate = DateTime.UtcNow
            });
        }

        if (!string.Equals(category.Name, dto.Name, StringComparison.Ordinal))
            Add("Name", category.Name, dto.Name);

        if (!string.Equals(category.NameEn, dto.NameEn, StringComparison.Ordinal))
            Add("NameEn", category.NameEn, dto.NameEn);

        if (!string.Equals(category.DescriptionEn, dto.DescriptionEn, StringComparison.Ordinal))
            Add("DescriptionEn", category.DescriptionEn, dto.DescriptionEn);

        return logs;
    }

    private static int NormalizePageNumber(int pageNumber) => pageNumber <= 0 ? 1 : pageNumber;

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0) return 10;
        return Math.Min(pageSize, MaxPageSize);
    }

    private static AppResponse ValidatePaging(int pageNumber, int pageSize)
    {
        var errors = new Dictionary<string, string[]>();

        if (pageNumber <= 0)
            errors["PageNumber"] = ["PageNumber must be >= 1"];

        if (pageSize <= 0)
            errors["PageSize"] = ["PageSize must be >= 1"];
        else if (pageSize > MaxPageSize)
            errors["PageSize"] = [$"PageSize must be <= {MaxPageSize}"];

        return errors.Count == 0
            ? AppResponse.Ok()
            : AppResponse.ValidationErrors(errors, "Invalid pagination parameters");
    }

    private async Task<bool> WouldCreateCycleAsync(int categoryId, int newParentId, CancellationToken ct)
    {
        // Strategy: load all categories (id, parentId) once and walk up parents from newParentId
        var all = await unitOfWork.Categories.GetAllForTreeAsync(ct); // should be AsNoTracking in repo
        var parentMap = all.ToDictionary(x => x.Id, x => x.ParentCategoryId);

        var current = (int?)newParentId;
        while (current.HasValue)
        {
            if (current.Value == categoryId)
                return true;

            if (!parentMap.TryGetValue(current.Value, out var next))
                break;

            current = next;
        }

        return false;
    }
}
