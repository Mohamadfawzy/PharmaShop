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
    Task<bool> ExistsByErpPgoIdAsync(decimal erpPgoId, CancellationToken ct);
    Task<bool> ExistsByErpPgoIdAsync(decimal erpPgoId, int excludeId, CancellationToken ct);
    Task<Promotion?> GetByIdForUpdateAsync(int id, CancellationToken ct);
    Task<PromotionDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<PromotionListItemDto>> SearchAsync(string? term, bool? isActive, DateTime? from, DateTime? to, bool? onlyRunningNow, PromotionSortOption sort, int page, int pageSize, CancellationToken ct);
}

