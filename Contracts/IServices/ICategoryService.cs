using Shared.Models.Dtos.Category;
using Shared.Responses;

namespace Contracts.IServices;

public interface ICategoryService
{
    Task<AppResponse<bool>> ChangeCategoryParentAsync(int categoryId, int? newParentCategoryId, CancellationToken ct);
    Task<AppResponse<int>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken ct);
    Task<AppResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<AppResponse<CategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct);
    Task<AppResponse<IEnumerable<CategoryDto>>> GetRootCategoriesAsync(CancellationToken ct);
    Task<AppResponse<bool>> UpdateAsync(int categoryId, CategoryUpdateDto dto, CancellationToken ct);
    Task<AppResponse<string>> UpdateCategoryImageAsync(int categoryId, Stream newImageStream, string rootPath, CancellationToken ct);
}
