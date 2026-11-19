using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Delivery;
using Shared.Models.Dtos.Order;
using Shared.Responses;

namespace Service;

public sealed class DeliveryZoneService : IDeliveryZoneService
{
    private readonly IUnitOfWork _unitOfWork;

    public DeliveryZoneService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<DeliveryZoneDto>>> GetPublicAsync(int storeId, CancellationToken ct)
    {
        var store = await _unitOfWork.Stores.FirstOrDefaultAsync(
            s => s.Id == storeId && s.IsActive && s.DeletedAt == null && s.Pharmacy.IsActive, ct: ct);
        if (store is null) return AppResponse<List<DeliveryZoneDto>>.NotFound("Active store not found");

        var zones = await _unitOfWork.DeliveryZones.GetAllAsync(
            criteria: z => z.StoreId == storeId && z.IsActive && z.Areas.Any(a => a.GovernorateArea.IsActive && a.GovernorateArea.Governorate.IsActive),
            orderBy: q => q.OrderBy(z => z.NameAr),
            include: q => q.Include(z => z.Areas).ThenInclude(a => a.GovernorateArea).ThenInclude(a => a.Governorate), ct: ct);
        return AppResponse<List<DeliveryZoneDto>>.Ok(zones.Select(z => Map(z, activeAreasOnly: true)).ToList());
    }

    public async Task<AppResponse<List<DeliveryZoneDto>>> GetAdminAsync(int storeId, CancellationToken ct)
    {
        var store = await FindStoreAsync(storeId, ct);
        if (store is null) return AppResponse<List<DeliveryZoneDto>>.NotFound("Store not found");

        var zones = await _unitOfWork.DeliveryZones.GetAllAsync(
            criteria: z => z.StoreId == storeId,
            orderBy: q => q.OrderBy(z => z.NameAr),
            include: q => q.Include(z => z.Areas).ThenInclude(a => a.GovernorateArea).ThenInclude(a => a.Governorate), ct: ct);
        return AppResponse<List<DeliveryZoneDto>>.Ok(zones.Select(z => Map(z, activeAreasOnly: false)).ToList());
    }

    public async Task<AppResponse<DeliveryZoneDto>> GetAdminByIdAsync(int storeId, int id, CancellationToken ct)
    {
        var zone = await FindZoneAsync(storeId, id, asNoTracking: true, ct);
        return zone is null
            ? AppResponse<DeliveryZoneDto>.NotFound("Delivery zone not found")
            : AppResponse<DeliveryZoneDto>.Ok(Map(zone, activeAreasOnly: false));
    }

    public async Task<AppResponse<int>> CreateAsync(int storeId, DeliveryZoneWriteDto dto, CancellationToken ct)
    {
        var store = await FindStoreAsync(storeId, ct);
        if (store is null || !store.IsActive || !store.Pharmacy.IsActive)
            return AppResponse<int>.BusinessRuleViolation("Delivery zones require an active store and pharmacy");

        var areaResult = await ValidateAreasAsync(storeId, dto.GovernorateAreaIds, zoneId: null, ct);
        if (areaResult.Error is not null) return AppResponse<int>.ValidationError(areaResult.Error);
        var nameAr = dto.NameAr.Trim();
        if (await _unitOfWork.DeliveryZones.AnyAsync(z => z.StoreId == storeId && z.NameAr == nameAr, ct))
            return AppResponse<int>.Conflict("A delivery zone with this Arabic name already exists in the store");

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        var zone = new DeliveryZone
        {
            StoreId = storeId,
            NameAr = nameAr,
            NameEn = dto.NameEn.Trim(),
            DeliveryFee = dto.DeliveryFee,
            FreeDeliveryMinimumAmount = dto.FreeDeliveryMinimumAmount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.DeliveryZones.AddAsync(zone, ct);
        await _unitOfWork.CompleteAsync(ct);
        await AddAreasAsync(zone, areaResult.Areas!, ct);
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Created(zone.Id, $"/api/v1/admin/stores/{storeId}/delivery-zones/{zone.Id}", "Delivery zone created successfully");
    }

    public async Task<AppResponse<int>> UpdateAsync(int storeId, int id, DeliveryZoneWriteDto dto, CancellationToken ct)
    {
        var zone = await FindZoneAsync(storeId, id, asNoTracking: false, ct);
        if (zone is null) return AppResponse<int>.NotFound("Delivery zone not found");

        var areaResult = await ValidateAreasAsync(storeId, dto.GovernorateAreaIds, id, ct);
        if (areaResult.Error is not null) return AppResponse<int>.ValidationError(areaResult.Error);
        var nameAr = dto.NameAr.Trim();
        if (await _unitOfWork.DeliveryZones.AnyAsync(z => z.Id != id && z.StoreId == storeId && z.NameAr == nameAr, ct))
            return AppResponse<int>.Conflict("A delivery zone with this Arabic name already exists in the store");

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        zone.NameAr = nameAr;
        zone.NameEn = dto.NameEn.Trim();
        zone.DeliveryFee = dto.DeliveryFee;
        zone.FreeDeliveryMinimumAmount = dto.FreeDeliveryMinimumAmount;
        zone.UpdatedAt = DateTime.UtcNow;

        var wantedIds = dto.GovernorateAreaIds.ToHashSet();
        var linksToRemove = zone.Areas.Where(a => !wantedIds.Contains(a.GovernorateAreaId)).ToList();
        await _unitOfWork.DeliveryZoneAreas.DeleteRangeAsync(linksToRemove, ct);
        var existingIds = zone.Areas.Select(a => a.GovernorateAreaId).ToHashSet();
        var areasToAdd = areaResult.Areas!.Where(a => !existingIds.Contains(a.Id)).ToList();
        await AddAreasAsync(zone, areasToAdd, ct);
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Ok(id, "Delivery zone updated successfully");
    }

    public async Task<AppResponse<int>> SetActiveAsync(int storeId, int id, DeliveryZoneActiveDto dto, CancellationToken ct)
    {
        var zone = await _unitOfWork.DeliveryZones.GetByIdAsync(id, ct);
        if (zone is null || zone.StoreId != storeId)
            return AppResponse<int>.NotFound("Delivery zone not found");
        if (dto.IsActive)
        {
            var store = await FindStoreAsync(storeId, ct);
            if (store is null || !store.IsActive || !store.Pharmacy.IsActive)
                return AppResponse<int>.BusinessRuleViolation("The zone cannot be activated while its store or pharmacy is inactive");
            if (!await _unitOfWork.DeliveryZoneAreas.AnyAsync(
                    a => a.DeliveryZoneId == id && a.GovernorateArea.IsActive && a.GovernorateArea.Governorate.IsActive, ct))
                return AppResponse<int>.BusinessRuleViolation("The zone cannot be activated without an active covered area");
        }
        zone.IsActive = dto.IsActive;
        zone.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(id, "Delivery zone status updated successfully");
    }

    public async Task<decimal?> ResolveDeliveryFeeAsync(
        int storeId, CustomerAddressSnapshot address, decimal discountedProductSubtotal, CancellationToken ct)
    {
        if (address.GovernorateAreaId <= 0) return null;
        var zone = await _unitOfWork.DeliveryZones.FirstOrDefaultAsync(
            z => z.StoreId == storeId && z.IsActive && z.Store.IsActive && z.Store.DeletedAt == null &&
                z.Store.Pharmacy.IsActive && z.Areas.Any(a => a.GovernorateAreaId == address.GovernorateAreaId &&
                    a.GovernorateArea.IsActive && a.GovernorateArea.Governorate.IsActive), ct: ct);
        if (zone is null) return null;

        var threshold = zone.FreeDeliveryMinimumAmount;
        return threshold.HasValue && discountedProductSubtotal >= threshold.Value ? 0m : zone.DeliveryFee;
    }

    private async Task<(List<GovernorateArea>? Areas, string? Error)> ValidateAreasAsync(
        int storeId, List<int>? areaIds, int? zoneId, CancellationToken ct)
    {
        if (areaIds is null || areaIds.Count == 0)
            return (null, "At least one governorate area is required");
        if (areaIds.Any(id => id <= 0) || areaIds.Distinct().Count() != areaIds.Count)
            return (null, "GovernorateAreaIds must contain unique positive ids");

        var areas = await _unitOfWork.GovernorateAreas.GetAllAsync(
            criteria: a => areaIds.Contains(a.Id) && a.IsActive && a.Governorate.IsActive, ct: ct);
        if (areas.Count != areaIds.Count)
            return (null, "Every assigned area must exist and be active under an active governorate");

        var conflict = await _unitOfWork.DeliveryZoneAreas.AnyAsync(
            a => a.StoreId == storeId && areaIds.Contains(a.GovernorateAreaId) &&
                (!zoneId.HasValue || a.DeliveryZoneId != zoneId.Value), ct);
        if (conflict)
            return (null, "An area cannot be assigned to multiple delivery zones in the same store");

        return (areas, null);
    }

    private async Task AddAreasAsync(DeliveryZone zone, IEnumerable<GovernorateArea> areas, CancellationToken ct)
    {
        var links = areas.Select(area => new DeliveryZoneArea
        {
            DeliveryZoneId = zone.Id,
            StoreId = zone.StoreId,
            GovernorateAreaId = area.Id
        });
        await _unitOfWork.DeliveryZoneAreas.AddRangeAsync(links, ct);
    }

    private Task<Store?> FindStoreAsync(int storeId, CancellationToken ct)
        => _unitOfWork.Stores.FirstOrDefaultAsync(
            s => s.Id == storeId && s.DeletedAt == null,
            include: q => q.Include(s => s.Pharmacy), ct: ct);

    private Task<DeliveryZone?> FindZoneAsync(int storeId, int id, bool asNoTracking, CancellationToken ct)
        => _unitOfWork.DeliveryZones.FirstOrDefaultAsync(
            z => z.StoreId == storeId && z.Id == id,
            include: q => q.Include(z => z.Areas).ThenInclude(a => a.GovernorateArea).ThenInclude(a => a.Governorate),
            asNoTracking: asNoTracking, ct: ct);

    private static DeliveryZoneDto Map(DeliveryZone z, bool activeAreasOnly) => new()
    {
        Id = z.Id,
        StoreId = z.StoreId,
        NameAr = z.NameAr,
        NameEn = z.NameEn,
        DeliveryFee = z.DeliveryFee,
        FreeDeliveryMinimumAmount = z.FreeDeliveryMinimumAmount,
        IsActive = z.IsActive,
        Areas = z.Areas
            .Where(a => !activeAreasOnly || a.GovernorateArea.IsActive && a.GovernorateArea.Governorate.IsActive)
            .OrderBy(a => a.GovernorateArea.Governorate.DisplayOrder)
            .ThenBy(a => a.GovernorateArea.DisplayOrder)
            .Select(a => new DeliveryZoneAreaItemDto
            {
                GovernorateAreaId = a.GovernorateAreaId,
                GovernorateId = a.GovernorateArea.GovernorateId,
                GovernorateNameAr = a.GovernorateArea.Governorate.NameAr,
                AreaNameAr = a.GovernorateArea.NameAr,
                AreaNameEn = a.GovernorateArea.NameEn
            }).ToList()
    };

}
