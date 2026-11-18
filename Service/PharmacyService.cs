using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Models.Dtos.Organization;
using Shared.Responses;

namespace Service;

public sealed class PharmacyService : IPharmacyService
{
    private readonly IUnitOfWork _unitOfWork;

    public PharmacyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResponse<List<PharmacyDto>>> GetAllAsync(CancellationToken ct)
    {
        var pharmacies = await _unitOfWork.Pharmacies.GetAllAsync(
            orderBy: q => q.OrderBy(p => p.Name), ct: ct);
        return AppResponse<List<PharmacyDto>>.Ok(pharmacies.Select(Map).ToList());
    }

    public async Task<AppResponse<PharmacyDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        if (id <= 0) return AppResponse<PharmacyDto>.ValidationError("Invalid pharmacy id");
        var pharmacy = await _unitOfWork.Pharmacies.GetByIdNoTrackingAsync(id, ct);
        return pharmacy is null
            ? AppResponse<PharmacyDto>.NotFound("Pharmacy not found")
            : AppResponse<PharmacyDto>.Ok(Map(pharmacy));
    }

    public async Task<AppResponse<int>> CreateAsync(PharmacyCreateDto dto, CancellationToken ct)
    {
        var validation = ValidateCoordinates(dto.Latitude, dto.Longitude);
        if (validation is not null) return validation;

        var pharmacy = new Pharmacy
        {
            Name = dto.Name.Trim(),
            NameEn = dto.NameEn.Trim(),
            OwnerName = Normalize(dto.OwnerName),
            LicenseNumber = Normalize(dto.LicenseNumber),
            PhoneNumber = Normalize(dto.PhoneNumber),
            Email = Normalize(dto.Email),
            Address = Normalize(dto.Address),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Pharmacies.AddAsync(pharmacy, ct);
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Created(pharmacy.Id, $"/api/v1/admin/pharmacies/{pharmacy.Id}", "Pharmacy created successfully");
    }

    public async Task<AppResponse<int>> UpdateAsync(int id, PharmacyUpdateDto dto, CancellationToken ct)
    {
        if (id <= 0) return AppResponse<int>.ValidationError("Invalid pharmacy id");
        var validation = ValidateCoordinates(dto.Latitude, dto.Longitude);
        if (validation is not null) return validation;

        var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(id, ct);
        if (pharmacy is null) return AppResponse<int>.NotFound("Pharmacy not found");

        pharmacy.Name = dto.Name.Trim();
        pharmacy.NameEn = dto.NameEn.Trim();
        pharmacy.OwnerName = Normalize(dto.OwnerName);
        pharmacy.LicenseNumber = Normalize(dto.LicenseNumber);
        pharmacy.PhoneNumber = Normalize(dto.PhoneNumber);
        pharmacy.Email = Normalize(dto.Email);
        pharmacy.Address = Normalize(dto.Address);
        pharmacy.Latitude = dto.Latitude;
        pharmacy.Longitude = dto.Longitude;
        pharmacy.IsActive = dto.IsActive;

        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(pharmacy.Id, "Pharmacy updated successfully");
    }

    private static AppResponse<int>? ValidateCoordinates(decimal? latitude, decimal? longitude)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            return AppResponse<int>.ValidationError("Latitude or longitude is outside the valid range");
        return null;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PharmacyDto Map(Pharmacy p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        NameEn = p.NameEn,
        OwnerName = p.OwnerName,
        LicenseNumber = p.LicenseNumber,
        PhoneNumber = p.PhoneNumber,
        Email = p.Email,
        Address = p.Address,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        IsActive = p.IsActive
    };
}
