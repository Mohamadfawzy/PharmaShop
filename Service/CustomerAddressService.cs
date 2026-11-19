using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Customer;
using Shared.Responses;

namespace Service;

public sealed class CustomerAddressService : ICustomerAddressService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerAddressService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<CustomerAddressDto>>> GetMyAddressesAsync(string? userId, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(userId, ct);
        if (customer is null) return AppResponse<List<CustomerAddressDto>>.NotFound("Customer profile not found");

        var addresses = await _unitOfWork.CustomerAddresses.GetAllAsync(
            criteria: a => a.CustomerId == customer.Id && a.DeletedAt == null,
            orderBy: q => q.OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt),
            include: q => q.Include(a => a.GovernorateArea).ThenInclude(a => a.Governorate), ct: ct);
        return AppResponse<List<CustomerAddressDto>>.Ok(addresses.Select(Map).ToList());
    }

    public async Task<AppResponse<CustomerAddressDto>> GetMyAddressAsync(string? userId, int id, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(userId, ct);
        if (customer is null) return AppResponse<CustomerAddressDto>.NotFound("Customer profile not found");
        var address = await FindAddressAsync(customer.Id, id, asNoTracking: true, ct);
        return address is null
            ? AppResponse<CustomerAddressDto>.NotFound("Address not found")
            : AppResponse<CustomerAddressDto>.Ok(Map(address));
    }

    public async Task<AppResponse<int>> CreateAsync(string? userId, CustomerAddressWriteDto dto, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(userId, ct);
        if (customer is null) return AppResponse<int>.NotFound("Customer profile not found; update your profile first");
        var area = await FindActiveAreaAsync(dto.GovernorateId, dto.GovernorateAreaId, ct);
        if (area is null) return AppResponse<int>.ValidationError("Select an active area belonging to the selected governorate");

        var existingDefault = await _unitOfWork.CustomerAddresses.GetAllAsync(
            criteria: a => a.CustomerId == customer.Id && a.DeletedAt == null && a.IsDefault,
            asNoTracking: false, ct: ct);
        var isDefault = dto.IsDefault || existingDefault.Count == 0;
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        if (isDefault && existingDefault.Count > 0)
        {
            foreach (var address in existingDefault) address.IsDefault = false;
            await _unitOfWork.CompleteAsync(ct);
        }

        var item = new CustomerAddress
        {
            CustomerId = customer.Id,
            GovernorateAreaId = area.Id,
            Title = dto.Title.Trim(),
            City = area.Governorate.NameAr,
            Region = area.NameAr,
            Street = dto.Street.Trim(),
            Landmark = Normalize(dto.Landmark),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.CustomerAddresses.AddAsync(item, ct);
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Created(item.Id, $"/api/v1/addresses/{item.Id}", "Address created successfully");
    }

    public async Task<AppResponse<int>> UpdateAsync(string? userId, int id, CustomerAddressWriteDto dto, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(userId, ct);
        if (customer is null) return AppResponse<int>.NotFound("Customer profile not found");
        var address = await FindAddressAsync(customer.Id, id, asNoTracking: false, ct);
        if (address is null) return AppResponse<int>.NotFound("Address not found");
        var area = await FindActiveAreaAsync(dto.GovernorateId, dto.GovernorateAreaId, ct);
        if (area is null) return AppResponse<int>.ValidationError("Select an active area belonging to the selected governorate");

        address.GovernorateAreaId = area.Id;
        address.Title = dto.Title.Trim();
        address.City = area.Governorate.NameAr;
        address.Region = area.NameAr;
        address.Street = dto.Street.Trim();
        address.Landmark = Normalize(dto.Landmark);
        address.Latitude = dto.Latitude;
        address.Longitude = dto.Longitude;
        address.UpdatedAt = DateTime.UtcNow;

        if (dto.IsDefault && !address.IsDefault)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
            var defaults = await _unitOfWork.CustomerAddresses.GetAllAsync(
                criteria: a => a.CustomerId == customer.Id && a.IsDefault && a.DeletedAt == null,
                asNoTracking: false, ct: ct);
            foreach (var current in defaults)
            {
                current.IsDefault = false;
                current.UpdatedAt = DateTime.UtcNow;
            }
            if (defaults.Count > 0) await _unitOfWork.CompleteAsync(ct);
            address.IsDefault = true;
            await _unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);
            return AppResponse<int>.Ok(id, "Address updated and set as default");
        }

        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(id, "Address updated successfully");
    }

    public async Task<AppResponse<int>> SoftDeleteAsync(string? userId, int id, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(userId, ct);
        if (customer is null) return AppResponse<int>.NotFound("Customer profile not found");
        var address = await FindAddressAsync(customer.Id, id, asNoTracking: false, ct);
        if (address is null) return AppResponse<int>.NotFound("Address not found");

        var wasDefault = address.IsDefault;
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        address.IsDefault = false;
        address.DeletedAt = DateTime.UtcNow;
        address.UpdatedAt = address.DeletedAt;
        await _unitOfWork.CompleteAsync(ct);

        if (wasDefault)
        {
            var remaining = await _unitOfWork.CustomerAddresses.GetAllAsync(
                criteria: a => a.CustomerId == customer.Id && a.Id != id && a.DeletedAt == null,
                orderBy: q => q.OrderBy(a => a.CreatedAt),
                asNoTracking: false, ct: ct);
            var nextDefault = remaining.FirstOrDefault();
            if (nextDefault is not null)
            {
                nextDefault.IsDefault = true;
                await _unitOfWork.CompleteAsync(ct);
            }
        }

        await tx.CommitAsync(ct);
        return AppResponse<int>.Ok(id, "Address deleted successfully");
    }

    public async Task<AppResponse<int>> SetDefaultAsync(string? userId, int id, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(userId, ct);
        if (customer is null) return AppResponse<int>.NotFound("Customer profile not found");
        var address = await FindAddressAsync(customer.Id, id, asNoTracking: false, ct);
        if (address is null) return AppResponse<int>.NotFound("Address not found");
        if (address.IsDefault) return AppResponse<int>.Ok(id, "Address is already the default");

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        var defaults = await _unitOfWork.CustomerAddresses.GetAllAsync(
            criteria: a => a.CustomerId == customer.Id && a.IsDefault && a.DeletedAt == null,
            asNoTracking: false, ct: ct);
        foreach (var current in defaults) current.IsDefault = false;
        if (defaults.Count > 0) await _unitOfWork.CompleteAsync(ct);

        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Ok(id, "Default address updated successfully");
    }

    private async Task<Customer?> FindCustomerAsync(string? userId, CancellationToken ct)
    {
        if (!int.TryParse(userId, out var identityId) || identityId <= 0) return null;
        return await _unitOfWork.Customers.FirstOrDefaultAsync(
            c => c.UserId == identityId && c.IsActive, ct: ct);
    }

    private Task<CustomerAddress?> FindAddressAsync(int customerId, int addressId, bool asNoTracking, CancellationToken ct)
        => _unitOfWork.CustomerAddresses.FirstOrDefaultAsync(
            a => a.Id == addressId && a.CustomerId == customerId && a.DeletedAt == null,
            include: q => q.Include(a => a.GovernorateArea).ThenInclude(a => a.Governorate),
            asNoTracking: asNoTracking, ct: ct);

    private Task<GovernorateArea?> FindActiveAreaAsync(int governorateId, int areaId, CancellationToken ct)
        => _unitOfWork.GovernorateAreas.FirstOrDefaultAsync(
            a => a.Id == areaId && a.GovernorateId == governorateId && a.IsActive && a.Governorate.IsActive,
            include: q => q.Include(a => a.Governorate), ct: ct);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CustomerAddressDto Map(CustomerAddress a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        GovernorateId = a.GovernorateArea.GovernorateId,
        GovernorateNameAr = a.GovernorateArea.Governorate.NameAr,
        GovernorateAreaId = a.GovernorateAreaId,
        AreaNameAr = a.GovernorateArea.NameAr,
        AreaNameEn = a.GovernorateArea.NameEn,
        City = a.City,
        Region = a.Region,
        Street = a.Street,
        Landmark = a.Landmark,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
        IsDefault = a.IsDefault,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
