using Contracts;
using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.IServices;
using Entities.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shared.Extensions;
using Shared.Models.Dtos.Category;
using Shared.Models.RequestFeatures;
using Shared.Responses;
using System.Linq.Expressions;
using System.Net;

namespace Service;

public class CategoryService : ICategoryService
{
    private const int MaxPageSize = 100;

    private readonly IUnitOfWork unitOfWork;
    private readonly Contracts.IServices.IMyImageService imageService;
    private readonly Contracts.Images.Abstractions.IImageService imageService2;
    private readonly ILogger<CategoryService> logger;
    private readonly ICurrentUserService currentUser;
    private readonly IMemoryCache cache;

    private const string CategoryTreeCacheKey = "categories:tree:v1";
    private static readonly TimeSpan CategoryTreeTtl = TimeSpan.FromMinutes(10);

    public CategoryService(
        IUnitOfWork unitOfWork,
        Contracts.IServices.IMyImageService imageService,
        ILogger<CategoryService> logger,
        ICurrentUserService currentUser,
        IMemoryCache cache,
        Contracts.Images.Abstractions.IImageService imageService2)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
        this.currentUser = currentUser;
        this.cache = cache;
        this.imageService2 = imageService2;
    }

    // ===================================================================================
    // (1) استخدم ErrorCode/StatusCode الصحيحين بدل Fail عام
    // ===================================================================================

    public async Task<AppResponse<int>> CreateCategoryAsync(
        CategoryCreateDto dto, CancellationToken ct)
    {
        try
        {
            if (dto.ParentCategoryId.HasValue)
            {
                // الأفضل: ExistsByIdAsync يتأكد أيضًا من !IsDeleted داخل الـRepo
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
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Categories.AddAsync(category, ct);
            await unitOfWork.CompleteAsync(ct);

            // بحذف الكاش هنا
            InvalidateCategoryCaches();

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

            if (auditLogs.Count == 0)
                return AppResponse<bool>.Ok(true, "No changes detected");

            //  Apply changes
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

    public async Task<AppResponse<List<CategoryDetailsDto>>> GetAllCategoriesAsync(
        RequestParameters param, CancellationToken ct)
    {
        var pagingValidation = ValidatePaging(param.PageNumber, param.PageSize);
        if (!pagingValidation.IsSuccess)
            return pagingValidation.FromError<List<CategoryDetailsDto>>(); // Extension (FromError<T>)

        param.PageNumber = NormalizePageNumber(param.PageNumber);
        param.PageSize = NormalizePageSize(param.PageSize);

        try
        {

            Expression<Func<Category, bool>> criteria = c =>
            (param.IsActive == null || c.IsActive == param.IsActive.Value) &&
            (param.IsDeleted == null || c.IsDeleted == param.IsDeleted.Value);

            var totalCount = await unitOfWork.Categories.CountAsync(criteria,ct);

            if (totalCount == 0)
            {
                return AppResponse<List<CategoryDetailsDto>>.Ok(
                    new List<CategoryDetailsDto>(),
                    PaginationInfo.Create(param.PageNumber, param.PageSize, 0)
                );
            }

            var skip = (param.PageNumber - 1) * param.PageSize;

            //Expression<Func<Category, bool>> criteria = c =>
            //    (param.IsActive == null || c.IsActive == param.IsActive.Value) &&
            //    (param.IsDeleted == null || c.IsDeleted == param.IsDeleted.Value);

            var categories = await unitOfWork.Categories.GetAllAsync(
                criteria: criteria,
                orderBy: q => q.OrderBy(c => c.Name),
                skip: skip,
                take: param.PageSize,
                ct: ct
            );

            var result = categories.Select(c => new CategoryDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                NameEn = c.NameEn,
                ParentCategoryId = c.ParentCategoryId,
                ImageId = c.ImageId,
                IsActive = c.IsActive
            }).ToList();

            var pagination = PaginationInfo.Create(param.PageNumber, param.PageSize, totalCount);

            return AppResponse<List<CategoryDetailsDto>>.Ok(result, pagination);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAllCategoriesAsync failed page={PageNumber}, size={PageSize}", param.PageNumber, param.PageSize);
            return AppResponse<List<CategoryDetailsDto>>.InternalError("Failed to load categories");
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

            InvalidateCategoryCaches();

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

    
    public async Task<AppResponse<string>> UpdateCategoryImageAsync(
         int categoryId,
         Stream newImageStream,
         string rootPath,
         ImageOutputFormat outputFormat = ImageOutputFormat.Auto,
         CancellationToken ct= default)
    {
        // IMPORTANT: Avoid relying on stream.Length (may be non-seekable).
        if (newImageStream is null || !newImageStream.CanRead)
            return AppResponse<string>.ValidationError("Invalid image stream");

        if (string.IsNullOrWhiteSpace(rootPath))
            return AppResponse<string>.ValidationError("Invalid root path");

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var category = await unitOfWork.Categories.GetByIdAsync(categoryId, ct);
            if (category is null || category.IsDeleted)
                return AppResponse<string>.NotFound("Category not found");

            // IMPORTANT:
            // Store a stable ImageId in DB (not a full path and not a changing filename).
            // If currently ImageUrl stores a filename, migrate it to store ImageId.
            var imageId = category.ImageId; // treat as ImageId (stable)

            SavedImageResult saved;

            if (string.IsNullOrWhiteSpace(imageId))
            {
                // No image yet => create new one and store its Id.
                var prefix = $"CAT{category.Id}";
                saved = await imageService2.SaveAsync(newImageStream, rootPath, prefix, outputFormat,  ct);

                //var updated = await unitOfWork.Categories.UpdateImageUrlAsync(categoryId, saved.Id, ct);
                var updated = await unitOfWork.Categories.UpdateImageMetaAsync(categoryId, saved.Id, (byte)saved.Format, ct);
                if (!updated)
                {
                    // DB update failed => delete newly stored image (best-effort).
                    await imageService2.DeleteAsync(saved.Id, rootPath, ct);
                    await tx.RollbackAsync(ct);
                    return AppResponse<string>.InternalError("Failed to update category image");
                }
            }
            else
            {
                // Existing image => replace contents while keeping the same Id.
                saved = await imageService2.ReplaceAsync(newImageStream, imageId, rootPath, outputFormat, ct);

                // No DB update needed because ImageId stays the same.
                // If you store extra fields like Format/UpdatedAt, update them here.
                // e.g. await unitOfWork.Categories.UpdateImageMetaAsync(categoryId, saved.Format, ct);
                await unitOfWork.Categories.UpdateImageMetaAsync(categoryId, saved.Id, (byte)saved.Format, ct);
            }

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse<string>.Ok(saved.Id, "Category image updated successfully");
        }
        catch (OperationCanceledException)
        {
            try { await tx.RollbackAsync(CancellationToken.None); } catch { /* best-effort */ }
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateCategoryImageAsync failed CategoryId={CategoryId}", categoryId);

            try { await tx.RollbackAsync(ct); }
            catch (Exception exRb) { logger.LogWarning(exRb, "Rollback failed in UpdateCategoryImageAsync"); }

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
                ImageId = category.ImageId,
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
                ImageId = c.ImageId,
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
            // 1 هنجيب من الكاش الاول
            if (cache.TryGetValue(CategoryTreeCacheKey, out List<CategoryTreeDto>? cachedTree))
            {
                return AppResponse<List<CategoryTreeDto>>.Ok(cachedTree!);
            }

            // 2 لو ماكنش موجود في الكاش هنجبهامن الديتابيز
            var categories = await unitOfWork.Categories.GetAllForTreeAsync(ct);

            if (categories.Count == 0)
            {
                var empty = new List<CategoryTreeDto>();

                cache.Set(CategoryTreeCacheKey, empty, CategoryTreeTtl);

                return AppResponse<List<CategoryTreeDto>>.Ok(empty);
            }

            // 3 بناء الشجرة 
            var dict = categories.ToDictionary(
                c => c.Id,
                c => new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NameEn = c.NameEn,
                    ImageUrl = c.ImageId,
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

            // 4 خزّن النتيجة في الكاش
            // MemoryCacheEntryOptions تسمح لك تحدد Expiration + Priority.. إلخ
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CategoryTreeTtl,
                Priority = CacheItemPriority.High
            };

            cache.Set(CategoryTreeCacheKey, roots, options);

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
        category.Description = dto.Description;
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

        if (!string.Equals(category.Description, dto.Description, StringComparison.Ordinal))
            Add("DescriptionEn", category.Description, dto.Description);

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

    private void InvalidateCategoryCaches()
    {
        cache.Remove(CategoryTreeCacheKey);

        // لو عندك root cache أو غيره:
        // _cache.Remove("categories:root:v1");
    }





    // ============================================
    // Trash
    // ============================================
    public async Task<AppResponse<string>> MyUpdateCategoryImageAsync(int categoryId, Stream newImageStream, string rootPath, CancellationToken ct)
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

            oldFileName = category.ImageId;

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
}
