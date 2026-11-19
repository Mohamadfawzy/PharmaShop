using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Models.Dtos.Customer;
using Shared.Responses;

namespace Service;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
    public Task<object> ReadAllCustomers()
    {
        var result = unitOfWork.Customers.GetAllCustomers();
        return result;
    }

    public async Task<AppResponse<CustomerProfileDto>> GetMyProfileAsync(string? userId, CancellationToken ct)
    {
        if (!TryGetIdentityId(userId, out var identityId))
            return AppResponse<CustomerProfileDto>.Unauthorized("User identity is invalid");

        var customer = await unitOfWork.Customers.FirstOrDefaultAsync(
            c => c.UserId == identityId && c.IsActive, ct: ct);
        return customer is null
            ? AppResponse<CustomerProfileDto>.NotFound("Customer profile not found; update your profile first")
            : AppResponse<CustomerProfileDto>.Ok(Map(customer));
    }

    public async Task<AppResponse<CustomerProfileDto>> UpdateMyProfileAsync(
        string? userId, string? email, CustomerProfileUpdateDto dto, CancellationToken ct)
    {
        if (!TryGetIdentityId(userId, out var identityId))
            return AppResponse<CustomerProfileDto>.Unauthorized("User identity is invalid");

        var customer = await unitOfWork.Customers.FirstOrDefaultAsync(
            c => c.UserId == identityId, asNoTracking: false, ct: ct);
        if (customer is not null && !customer.IsActive)
            return AppResponse<CustomerProfileDto>.Forbidden("Customer profile is inactive");

        if (customer is null)
        {
            customer = new Customer
            {
                UserId = identityId,
                CustomerType = "Regular",
                Points = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Email = Normalize(email)
            };
            customer.FullNameAr = dto.FullNameAr.Trim();
            customer.FullNameEn = dto.FullNameEn.Trim();
            customer.PhoneNumber = Normalize(dto.PhoneNumber);
            customer.Gender = Normalize(dto.Gender);
            customer.DateOfBirth = dto.DateOfBirth;
            await unitOfWork.Customers.AddAsync(customer, ct);
        }
        else
        {
            customer.FullNameAr = dto.FullNameAr.Trim();
            customer.FullNameEn = dto.FullNameEn.Trim();
            customer.PhoneNumber = Normalize(dto.PhoneNumber);
            customer.Gender = Normalize(dto.Gender);
            customer.DateOfBirth = dto.DateOfBirth;
            customer.UpdatedAt = DateTime.UtcNow;
        }

        await unitOfWork.CompleteAsync(ct);
        return AppResponse<CustomerProfileDto>.Ok(Map(customer), "Customer profile saved successfully");
    }

    private static bool TryGetIdentityId(string? userId, out int identityId)
        => int.TryParse(userId, out identityId) && identityId > 0;

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CustomerProfileDto Map(Customer customer) => new()
    {
        Id = customer.Id,
        FullNameAr = customer.FullNameAr,
        FullNameEn = customer.FullNameEn,
        PhoneNumber = customer.PhoneNumber,
        Email = customer.Email,
        Gender = customer.Gender,
        DateOfBirth = customer.DateOfBirth
    };
}
