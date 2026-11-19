namespace Contracts.IServices;

public interface ICustomerService
{
    Task<object> ReadAllCustomers();
    Task<Shared.Responses.AppResponse<Shared.Models.Dtos.Customer.CustomerProfileDto>> GetMyProfileAsync(string? userId, CancellationToken ct);
    Task<Shared.Responses.AppResponse<Shared.Models.Dtos.Customer.CustomerProfileDto>> UpdateMyProfileAsync(string? userId, string? email, Shared.Models.Dtos.Customer.CustomerProfileUpdateDto dto, CancellationToken ct);
}
