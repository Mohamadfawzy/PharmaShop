using Shared.Models.Dtos.Promotion;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPromotionService
{
    Task<AppResponse<int>> CreatePromotionAsync(PromotionCreateDto dto, CancellationToken ct);
    Task<AppResponse<List<PromotionListItemDto>>> GetPromotionsAsync(PromotionListQueryDto query, CancellationToken ct);
}