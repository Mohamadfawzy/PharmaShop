using Shared.Enums;
using Shared.Models.Dtos.Banners;
using Shared.Responses;

namespace Contracts.IServices;

public interface IBannerService
{
    Task<AppResponse<List<BannerDto>>> GetPublicAsync(BannerPlacement? placement, string publicBaseUrl, CancellationToken ct);
    Task<AppResponse<List<BannerDto>>> SearchAdminAsync(BannerListQueryDto query, string publicBaseUrl, CancellationToken ct);
    Task<AppResponse<BannerDto>> GetAdminByIdAsync(int id, string publicBaseUrl, CancellationToken ct);
    Task<AppResponse<BannerDto>> CreateAsync(BannerCreateDto dto, string imageUrl, string thumbnailUrl, string publicBaseUrl, CancellationToken ct);
    Task<AppResponse<BannerDto>> UpdateAsync(int id, BannerUpdateDto dto, string? imageUrl, string? thumbnailUrl, string publicBaseUrl, CancellationToken ct);
    Task<AppResponse<int>> SoftDeleteAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> SetActiveAsync(int id, bool isActive, CancellationToken ct);
    Task<AppResponse<int>> ReorderAsync(BannerReorderDto dto, CancellationToken ct);
}
