using Shared.Models.Dtos.Customer;
using Shared.Responses;

namespace Contracts.IServices;

public interface ICustomerAddressService
{
    Task<AppResponse<List<CustomerAddressDto>>> GetMyAddressesAsync(string? userId, CancellationToken ct);
    Task<AppResponse<CustomerAddressDto>> GetMyAddressAsync(string? userId, int id, CancellationToken ct);
    Task<AppResponse<int>> CreateAsync(string? userId, CustomerAddressWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateAsync(string? userId, int id, CustomerAddressWriteDto dto, CancellationToken ct);
    Task<AppResponse<int>> SoftDeleteAsync(string? userId, int id, CancellationToken ct);
    Task<AppResponse<int>> SetDefaultAsync(string? userId, int id, CancellationToken ct);
}
