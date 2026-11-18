using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Locations;
using Shared.Responses;

namespace Service;

public sealed class LocationService : ILocationService
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<GovernorateDto>>> GetGovernoratesAsync(bool includeInactive, CancellationToken ct)
    {
        var items = await _unitOfWork.Governorates.GetAllAsync(
            criteria: g => includeInactive || g.IsActive,
            orderBy: q => q.OrderBy(g => g.DisplayOrder).ThenBy(g => g.NameAr), ct: ct);
        return AppResponse<List<GovernorateDto>>.Ok(items.Select(Map).ToList());
    }

    public async Task<AppResponse<GovernorateDto>> GetGovernorateByIdAsync(int id, CancellationToken ct)
    {
        var item = await _unitOfWork.Governorates.GetByIdNoTrackingAsync(id, ct);
        return item is null
            ? AppResponse<GovernorateDto>.NotFound("Governorate not found")
            : AppResponse<GovernorateDto>.Ok(Map(item));
    }

    public async Task<AppResponse<List<GovernorateAreaDto>>> GetPublicAreasAsync(int governorateId, CancellationToken ct)
    {
        var governorate = await _unitOfWork.Governorates.GetByIdNoTrackingAsync(governorateId, ct);
        if (governorate is null || !governorate.IsActive)
            return AppResponse<List<GovernorateAreaDto>>.NotFound("Active governorate not found");

        var items = await _unitOfWork.GovernorateAreas.GetAllAsync(
            criteria: a => a.GovernorateId == governorateId && a.IsActive,
            orderBy: q => q.OrderBy(a => a.DisplayOrder).ThenBy(a => a.NameAr), ct: ct);
        return AppResponse<List<GovernorateAreaDto>>.Ok(items.Select(a => Map(a, governorate.NameAr)).ToList());
    }

    public async Task<AppResponse<List<GovernorateAreaDto>>> GetAdminAreasAsync(int? governorateId, CancellationToken ct)
    {
        var items = await _unitOfWork.GovernorateAreas.GetAllAsync(
            criteria: a => !governorateId.HasValue || a.GovernorateId == governorateId.Value,
            orderBy: q => q.OrderBy(a => a.Governorate.DisplayOrder).ThenBy(a => a.DisplayOrder).ThenBy(a => a.NameAr),
            include: q => q.Include(a => a.Governorate), ct: ct);
        return AppResponse<List<GovernorateAreaDto>>.Ok(items.Select(a => Map(a, a.Governorate.NameAr)).ToList());
    }

    public async Task<AppResponse<GovernorateAreaDto>> GetAreaByIdAsync(int id, CancellationToken ct)
    {
        var item = await _unitOfWork.GovernorateAreas.FirstOrDefaultAsync(
            a => a.Id == id, include: q => q.Include(a => a.Governorate), ct: ct);
        return item is null
            ? AppResponse<GovernorateAreaDto>.NotFound("Area not found")
            : AppResponse<GovernorateAreaDto>.Ok(Map(item, item.Governorate.NameAr));
    }

    public async Task<AppResponse<int>> CreateGovernorateAsync(GovernorateWriteDto dto, CancellationToken ct)
    {
        var nameAr = dto.NameAr.Trim();
        if (await _unitOfWork.Governorates.AnyAsync(g => g.NameAr == nameAr, ct))
            return AppResponse<int>.Conflict("Governorate Arabic name already exists");

        var item = new Governorate
        {
            NameAr = nameAr,
            NameEn = dto.NameEn.Trim(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Governorates.AddAsync(item, ct);
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Created(item.Id, $"/api/v1/admin/governorates/{item.Id}", "Governorate created successfully");
    }

    public async Task<AppResponse<int>> UpdateGovernorateAsync(int id, GovernorateWriteDto dto, CancellationToken ct)
    {
        var item = await _unitOfWork.Governorates.GetByIdAsync(id, ct);
        if (item is null) return AppResponse<int>.NotFound("Governorate not found");
        var nameAr = dto.NameAr.Trim();
        if (await _unitOfWork.Governorates.AnyAsync(g => g.Id != id && g.NameAr == nameAr, ct))
            return AppResponse<int>.Conflict("Governorate Arabic name already exists");

        item.NameAr = nameAr;
        item.NameEn = dto.NameEn.Trim();
        item.DisplayOrder = dto.DisplayOrder;
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(item.Id, "Governorate updated successfully");
    }

    public async Task<AppResponse<int>> SetGovernorateActiveAsync(int id, LocationActiveDto dto, CancellationToken ct)
    {
        var item = await _unitOfWork.Governorates.GetByIdAsync(id, ct);
        if (item is null) return AppResponse<int>.NotFound("Governorate not found");
        item.IsActive = dto.IsActive;
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(item.Id, "Governorate status updated successfully");
    }

    public async Task<AppResponse<int>> CreateAreaAsync(GovernorateAreaWriteDto dto, CancellationToken ct)
    {
        var governorate = await _unitOfWork.Governorates.GetByIdNoTrackingAsync(dto.GovernorateId, ct);
        if (governorate is null || !governorate.IsActive)
            return AppResponse<int>.ValidationError("An active governorate is required");
        var nameAr = dto.NameAr.Trim();
        if (await _unitOfWork.GovernorateAreas.AnyAsync(a => a.GovernorateId == dto.GovernorateId && a.NameAr == nameAr, ct))
            return AppResponse<int>.Conflict("Area Arabic name already exists in this governorate");

        var item = new GovernorateArea
        {
            GovernorateId = dto.GovernorateId,
            NameAr = nameAr,
            NameEn = dto.NameEn.Trim(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.GovernorateAreas.AddAsync(item, ct);
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Created(item.Id, $"/api/v1/admin/areas/{item.Id}", "Area created successfully");
    }

    public async Task<AppResponse<int>> UpdateAreaAsync(int id, GovernorateAreaWriteDto dto, CancellationToken ct)
    {
        var item = await _unitOfWork.GovernorateAreas.GetByIdAsync(id, ct);
        if (item is null) return AppResponse<int>.NotFound("Area not found");
        var governorate = await _unitOfWork.Governorates.GetByIdNoTrackingAsync(dto.GovernorateId, ct);
        if (governorate is null || !governorate.IsActive)
            return AppResponse<int>.ValidationError("An active governorate is required");
        var nameAr = dto.NameAr.Trim();
        if (await _unitOfWork.GovernorateAreas.AnyAsync(
                a => a.Id != id && a.GovernorateId == dto.GovernorateId && a.NameAr == nameAr, ct))
            return AppResponse<int>.Conflict("Area Arabic name already exists in this governorate");

        item.GovernorateId = dto.GovernorateId;
        item.NameAr = nameAr;
        item.NameEn = dto.NameEn.Trim();
        item.DisplayOrder = dto.DisplayOrder;
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(item.Id, "Area updated successfully");
    }

    public async Task<AppResponse<int>> SetAreaActiveAsync(int id, LocationActiveDto dto, CancellationToken ct)
    {
        var item = await _unitOfWork.GovernorateAreas.GetByIdAsync(id, ct);
        if (item is null) return AppResponse<int>.NotFound("Area not found");
        if (dto.IsActive)
        {
            var governorate = await _unitOfWork.Governorates.GetByIdNoTrackingAsync(item.GovernorateId, ct);
            if (governorate is null || !governorate.IsActive)
                return AppResponse<int>.BusinessRuleViolation("An area cannot be activated while its governorate is inactive");
        }
        item.IsActive = dto.IsActive;
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(item.Id, "Area status updated successfully");
    }

    private static GovernorateDto Map(Governorate g) => new()
    {
        Id = g.Id,
        NameAr = g.NameAr,
        NameEn = g.NameEn,
        IsActive = g.IsActive,
        DisplayOrder = g.DisplayOrder
    };

    private static GovernorateAreaDto Map(GovernorateArea a, string governorateNameAr) => new()
    {
        Id = a.Id,
        GovernorateId = a.GovernorateId,
        GovernorateNameAr = governorateNameAr,
        NameAr = a.NameAr,
        NameEn = a.NameEn,
        IsActive = a.IsActive,
        DisplayOrder = a.DisplayOrder
    };
}
