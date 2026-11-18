using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Organization;
using Shared.Responses;

namespace Service;

public sealed class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;

    public StoreService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResponse<List<StoreDto>>> GetPublicAsync(int? pharmacyId, CancellationToken ct)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync(
            criteria: s => s.IsActive && s.DeletedAt == null && s.Pharmacy.IsActive &&
                (!pharmacyId.HasValue || s.PharmacyId == pharmacyId.Value),
            orderBy: q => q.OrderBy(s => s.NameAr),
            include: q => q.Include(s => s.Pharmacy),
            ct: ct);
        return AppResponse<List<StoreDto>>.Ok(stores.Select(Map).ToList());
    }

    public async Task<AppResponse<StoreDto>> GetPublicByIdAsync(int id, CancellationToken ct)
    {
        var store = await FindStoreAsync(id, activeOnly: true, ct);
        return store is null
            ? AppResponse<StoreDto>.NotFound("Store not found")
            : AppResponse<StoreDto>.Ok(Map(store));
    }

    public async Task<AppResponse<List<StoreDto>>> GetAdminAsync(int? pharmacyId, CancellationToken ct)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync(
            criteria: s => s.DeletedAt == null && (!pharmacyId.HasValue || s.PharmacyId == pharmacyId.Value),
            orderBy: q => q.OrderBy(s => s.PharmacyId).ThenBy(s => s.NameAr),
            include: q => q.Include(s => s.Pharmacy),
            ct: ct);
        return AppResponse<List<StoreDto>>.Ok(stores.Select(Map).ToList());
    }

    public async Task<AppResponse<StoreDto>> GetAdminByIdAsync(int id, CancellationToken ct)
    {
        var store = await FindStoreAsync(id, activeOnly: false, ct);
        return store is null
            ? AppResponse<StoreDto>.NotFound("Store not found")
            : AppResponse<StoreDto>.Ok(Map(store));
    }

    public async Task<AppResponse<int>> CreateAsync(StoreCreateDto dto, CancellationToken ct)
    {
        var pharmacy = await _unitOfWork.Pharmacies.GetByIdNoTrackingAsync(dto.PharmacyId, ct);
        if (pharmacy is null || !pharmacy.IsActive)
            return AppResponse<int>.ValidationError("An active pharmacy is required");

        var code = Normalize(dto.Code);
        if (code is not null && await _unitOfWork.Stores.AnyAsync(
                s => s.PharmacyId == dto.PharmacyId && s.Code == code && s.DeletedAt == null, ct))
            return AppResponse<int>.Conflict("Store code already exists in this pharmacy");

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        var store = new Store
        {
            PharmacyId = dto.PharmacyId,
            NameAr = dto.NameAr.Trim(),
            NameEn = dto.NameEn.Trim(),
            Code = code,
            Address = Normalize(dto.Address),
            IsDefault = dto.IsDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (store.IsDefault)
            await ClearDefaultAsync(store.PharmacyId, null, ct);
        else if (!await _unitOfWork.Stores.AnyAsync(
                     s => s.PharmacyId == store.PharmacyId && s.IsActive && s.DeletedAt == null, ct))
            store.IsDefault = true;

        await _unitOfWork.Stores.AddAsync(store, ct);
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Created(store.Id, $"/api/v1/admin/stores/{store.Id}", "Store created successfully");
    }

    public async Task<AppResponse<int>> UpdateAsync(int id, StoreUpdateDto dto, CancellationToken ct)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id, ct);
        if (store is null || store.DeletedAt is not null)
            return AppResponse<int>.NotFound("Store not found");

        var code = Normalize(dto.Code);
        if (code is not null && await _unitOfWork.Stores.AnyAsync(
                s => s.Id != id && s.PharmacyId == store.PharmacyId && s.Code == code && s.DeletedAt == null, ct))
            return AppResponse<int>.Conflict("Store code already exists in this pharmacy");

        if (!TrySetOriginalRowVersion(id, dto.RowVersion, store, out var concurrencyError))
            return AppResponse<int>.ValidationError(concurrencyError!);

        store.NameAr = dto.NameAr.Trim();
        store.NameEn = dto.NameEn.Trim();
        store.Code = code;
        store.Address = Normalize(dto.Address);
        store.IsActive = dto.IsActive;

        if (!store.IsActive && store.IsDefault)
            await SetAnotherDefaultAsync(store, ct);

        try
        {
            await _unitOfWork.CompleteAsync(ct);
            return AppResponse<int>.Ok(store.Id, "Store updated successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResponse<int>.Conflict("Store changed by another request; reload it and try again");
        }
    }

    public async Task<AppResponse<int>> SetActiveAsync(int id, StoreActiveDto dto, CancellationToken ct)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id, ct);
        if (store is null || store.DeletedAt is not null)
            return AppResponse<int>.NotFound("Store not found");

        if (dto.IsActive)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdNoTrackingAsync(store.PharmacyId, ct);
            if (pharmacy is null || !pharmacy.IsActive)
                return AppResponse<int>.BusinessRuleViolation("The store cannot be activated while its pharmacy is inactive");
        }

        store.IsActive = dto.IsActive;
        if (!dto.IsActive && store.IsDefault)
            await SetAnotherDefaultAsync(store, ct);

        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(store.Id, "Store status updated successfully");
    }

    public async Task<AppResponse<int>> SetDefaultAsync(int id, CancellationToken ct)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id, ct);
        if (store is null || store.DeletedAt is not null)
            return AppResponse<int>.NotFound("Store not found");
        if (!store.IsActive)
            return AppResponse<int>.BusinessRuleViolation("Only an active store can be the default store");

        var pharmacy = await _unitOfWork.Pharmacies.GetByIdNoTrackingAsync(store.PharmacyId, ct);
        if (pharmacy is null || !pharmacy.IsActive)
            return AppResponse<int>.BusinessRuleViolation("The pharmacy is inactive");

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        await ClearDefaultAsync(store.PharmacyId, id, ct);
        store.IsDefault = true;
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Ok(store.Id, "Default store updated successfully");
    }

    public async Task<AppResponse<int>> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id, ct);
        if (store is null || store.DeletedAt is not null)
            return AppResponse<int>.NotFound("Store not found");

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        if (store.IsDefault)
            await SetAnotherDefaultAsync(store, ct);
        store.IsActive = false;
        store.IsDefault = false;
        store.DeletedAt = DateTime.UtcNow;
        store.DeletedBy = "Admin";
        await _unitOfWork.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<int>.Ok(store.Id, "Store deleted successfully");
    }

    private async Task<Store?> FindStoreAsync(int id, bool activeOnly, CancellationToken ct)
        => await _unitOfWork.Stores.FirstOrDefaultAsync(
            s => s.Id == id && s.DeletedAt == null && (!activeOnly || s.IsActive && s.Pharmacy.IsActive),
            include: q => q.Include(s => s.Pharmacy), ct: ct);

    private async Task ClearDefaultAsync(int pharmacyId, int? exceptId, CancellationToken ct)
    {
        var defaults = await _unitOfWork.Stores.GetAllAsync(
            criteria: s => s.PharmacyId == pharmacyId && s.IsDefault && s.DeletedAt == null &&
                (!exceptId.HasValue || s.Id != exceptId.Value),
            ct: ct);
        foreach (var item in defaults) item.IsDefault = false;
    }

    private async Task SetAnotherDefaultAsync(Store current, CancellationToken ct)
    {
        current.IsDefault = false;
        var candidates = await _unitOfWork.Stores.GetAllAsync(
            criteria: s => s.PharmacyId == current.PharmacyId && s.Id != current.Id && s.IsActive && s.DeletedAt == null,
            orderBy: q => q.OrderBy(s => s.CreatedAt), asNoTracking: false, ct: ct);
        var candidate = candidates.FirstOrDefault();
        if (candidate is not null) candidate.IsDefault = true;
    }

    private bool TrySetOriginalRowVersion(int id, string? encoded, Store store, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(encoded)) return true;
        try
        {
            _unitOfWork.SetOriginalRowVersion(store, Convert.FromBase64String(encoded));
            return true;
        }
        catch (FormatException)
        {
            error = "RowVersion must be a valid Base64 value";
            return false;
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static StoreDto Map(Store s) => new()
    {
        Id = s.Id,
        PharmacyId = s.PharmacyId,
        PharmacyName = s.Pharmacy?.Name ?? string.Empty,
        NameAr = s.NameAr,
        NameEn = s.NameEn,
        Code = s.Code,
        Address = s.Address,
        IsDefault = s.IsDefault,
        IsActive = s.IsActive,
        RowVersion = s.RowVersion is { Length: > 0 } ? Convert.ToBase64String(s.RowVersion) : null
    };
}
