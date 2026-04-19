using Entities.Models;
using Shared.Enums.Promotion;
using Shared.Models.Dtos.Promotion;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts;


public interface IPromotionRepository : IGenericRepository<Promotion>
{
    Task AddPromotionProductsRangeAsync(List<PromotionProduct> entities, CancellationToken ct);
    Task<bool> ExistsByErpPgoIdAsync(decimal erpPgoId, CancellationToken ct);
    Task<bool> ExistsByErpPgoIdAsync(decimal erpPgoId, int excludeId, CancellationToken ct);
    Task<HashSet<decimal>> GetActiveErpProductIdsAsync(int promotionId, List<decimal> erpProductIds, CancellationToken ct);
    Task<Promotion?> GetByIdForUpdateAsync(int id, CancellationToken ct);
    Task<PromotionDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken ct);
    Task<List<PromotionProductRow>> GetPromotionProductsRowsAsync(int promotionId, CancellationToken ct);
    Task<List<int>> GetSoftDeletedPromotionProductIdsAsync(int promotionId, List<decimal> erpProductIds, CancellationToken ct);
    Task<bool> PromotionExistsAsync(int promotionId, CancellationToken ct);
    Task<int> RestoreByIdsAsync(List<int> ids, CancellationToken ct);
    Task<int> RestorePromotionProductsAsync(List<int> ids, CancellationToken ct);
    Task<PagedResult<PromotionListItemDto>> SearchAsync(string? term, bool? isActive, DateTime? from, DateTime? to, bool? onlyRunningNow, PromotionSortOption sort, int page, int pageSize, CancellationToken ct);
    Task<int> SetActiveAsync(int id, bool isActive, DateTime nowUtc, CancellationToken ct);
    Task<int> SoftDeleteByIdsAsync(List<int> ids, DateTime nowUtc, CancellationToken ct);
    Task<int> SoftDeletePromotionProductAsync(int promotionId, int promotionProductId, DateTime nowUtc, CancellationToken ct);
    Task<int> TouchPromotionUpdatedAtAsync(int promotionId, DateTime nowUtc, CancellationToken ct);
}

