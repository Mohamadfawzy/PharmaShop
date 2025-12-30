using Contracts.Images.Abstractions;
using Shared.Models.Dtos.Category;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;
public interface ICategoryService
{
    // Admin: CRUD
    Task<AppResponse<int>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken ct);

    Task<AppResponse<bool>> UpdateAsync(int categoryId, CategoryUpdateDto dto, CancellationToken ct);

    Task<AppResponse<bool>> ChangeCategoryParentAsync(int categoryId, int? newParentCategoryId, CancellationToken ct);

    Task<AppResponse<string>> MyUpdateCategoryImageAsync(int categoryId, Stream newImageStream, string rootPath, CancellationToken ct);

    Task<AppResponse<bool>> SetCategoryActiveStatusAsync(int categoryId, bool isActive, CancellationToken ct);

    // Public / Shared: Queries
    Task<AppResponse<CategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct);

    Task<AppResponse<List<CategoryDetailsDto>>> GetAllCategoriesAsync(RequestParameters param, CancellationToken ct);

    Task<AppResponse<List<CategoryDto>>> GetRootCategoriesAsync(CancellationToken ct);

    Task<AppResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken ct);

    Task<AppResponse<string>> UpdateCategoryImageAsync(int categoryId, Stream newImageStream, string rootPath, ImageOutputFormat outputFormat, CancellationToken ct);
}