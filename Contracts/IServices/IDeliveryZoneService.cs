using Shared.Models.Dtos.Delivery;
using Shared.Models.Dtos.Order;
using Shared.Responses;

namespace Contracts.IServices;

public interface IDeliveryZoneService
{
    Task<AppResponse<List<DeliveryZoneDto>>> GetPublicAsync(int storeId, CancellationToken ct);
    Task<AppResponse<List<DeliveryZoneDto>>> GetAdminAsync(int storeId, CancellationToken ct);
    Task<AppResponse<DeliveryZoneDto>> GetAdminByIdAsync(int storeId, int id, CancellationToken ct);
    Task<AppResponse<int>> CreateAsync(int storeId, DeliveryZoneWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateAsync(int storeId, int id, DeliveryZoneWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetActiveAsync(int storeId, int id, DeliveryZoneActiveDto dto, CancellationToken ct);
    Task<decimal?> ResolveDeliveryFeeAsync(int storeId, CustomerAddressSnapshot address, decimal discountedProductSubtotal, CancellationToken ct);
}
