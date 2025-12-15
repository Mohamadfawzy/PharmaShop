using Shared.Models.Dtos.Category;
using Shared.Responses;

namespace Contracts.IServices;

public interface ICategoryService
{
    Task<AppResponse<int>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken ct);
}
