using Shared.Models.Dtos.Promotion;
using Shared.Responses;

namespace Contracts.IServices;

public interface IPromotionService
{
    Task<AppResponse<PromotionAddProductsResultDto>> AddProductsToPromotionAsync(int promotionId, PromotionAddProductsDto dto, CancellationToken ct);
    Task<AppResponse<int>> CreatePromotionAsync(PromotionCreateDto dto, CancellationToken ct);
    Task<AppResponse<PromotionDetailsDto>> GetPromotionByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<List<PromotionListItemDto>>> GetPromotionsAsync(PromotionListQueryDto query, CancellationToken ct);
    Task<AppResponse<int>> RemoveProductFromPromotionAsync(int promotionId, int promotionProductId, CancellationToken ct);
    Task<AppResponse<PromotionReplaceProductsResultDto>> ReplacePromotionProductsAsync(int promotionId, PromotionReplaceProductsDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetPromotionActiveAsync(int id, PromotionActivateDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdatePromotionAsync(int id, PromotionUpdateDto dto, CancellationToken ct);
}