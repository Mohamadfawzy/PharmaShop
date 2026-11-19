using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Models.Dtos.Catalog;
using Shared.Responses;

namespace Service;

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly IUnitOfWork _uow;
    public ReferenceDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<AppResponse<List<CompanyDto>>> GetCompaniesAsync(bool includeInactive, CancellationToken ct)
    {
        var rows = await _uow.Companies.GetAllAsync(x => includeInactive || (x.IsActive && x.DeletedAt == null), q => q.OrderBy(x => x.NameAr), ct: ct);
        return AppResponse<List<CompanyDto>>.Ok(rows.Select(Map).ToList());
    }
    public async Task<AppResponse<CompanyDto>> GetCompanyAsync(int id, CancellationToken ct)
    {
        var row = await _uow.Companies.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct: ct);
        return row is null ? AppResponse<CompanyDto>.NotFound("Company not found") : AppResponse<CompanyDto>.Ok(Map(row));
    }
    public async Task<AppResponse<int>> CreateCompanyAsync(CompanyWriteDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NameAr)) return AppResponse<int>.ValidationError("NameAr is required");
        var name = dto.NameAr.Trim();
        if (await _uow.Companies.AnyAsync(x => x.DeletedAt == null && x.NameAr == name, ct)) return AppResponse<int>.ValidationError("Company name already exists");
        var row = new Company { NameAr = name, NameEn = dto.NameEn?.Trim(), IsActive = dto.IsActive ?? true, CreatedAt = DateTime.UtcNow };
        await _uow.Companies.AddAsync(row, ct); await _uow.CompleteAsync(ct);
        return AppResponse<int>.Created(row.Id, $"/api/v1/admin/companies/{row.Id}");
    }
    public async Task<AppResponse<bool>> UpdateCompanyAsync(int id, CompanyWriteDto dto, CancellationToken ct)
    {
        var row = await _uow.Companies.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, asNoTracking: false, ct: ct);
        if (row is null) return AppResponse<bool>.NotFound("Company not found");
        if (string.IsNullOrWhiteSpace(dto.NameAr)) return AppResponse<bool>.ValidationError("NameAr is required");
        var normalizedName = dto.NameAr.Trim();
        if (await _uow.Companies.AnyAsync(x => x.Id != id && x.DeletedAt == null && x.NameAr == normalizedName, ct)) return AppResponse<bool>.ValidationError("Company name already exists");
        row.NameAr = normalizedName; row.NameEn = dto.NameEn?.Trim(); if (dto.IsActive.HasValue) row.IsActive = dto.IsActive.Value;
        await _uow.Companies.UpdateAsync(row, ct); await _uow.CompleteAsync(ct); return AppResponse<bool>.Ok(true, "Company updated");
    }
    public async Task<AppResponse<bool>> DeleteCompanyAsync(int id, bool restore, CancellationToken ct)
    {
        var row = await _uow.Companies.GetByIdAsync(id, ct); if (row is null) return AppResponse<bool>.NotFound("Company not found");
        row.DeletedAt = restore ? null : DateTime.UtcNow; row.DeletedBy = restore ? null : "admin"; if (!restore) row.IsActive = false;
        await _uow.Companies.UpdateAsync(row, ct); await _uow.CompleteAsync(ct); return AppResponse<bool>.Ok(true, restore ? "Company restored" : "Company deleted");
    }
    public async Task<AppResponse<List<UnitDto>>> GetUnitsAsync(bool includeInactive, CancellationToken ct)
    {
        var rows = await _uow.Units.GetAllAsync(x => includeInactive || x.IsActive, q => q.OrderBy(x => x.NameAr), ct: ct);
        return AppResponse<List<UnitDto>>.Ok(rows.Select(Map).ToList());
    }
    public async Task<AppResponse<UnitDto>> GetUnitAsync(int id, CancellationToken ct)
    {
        var row = await _uow.Units.GetByIdNoTrackingAsync(id, ct); return row is null ? AppResponse<UnitDto>.NotFound("Unit not found") : AppResponse<UnitDto>.Ok(Map(row));
    }
    public async Task<AppResponse<int>> CreateUnitAsync(UnitWriteDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn)) return AppResponse<int>.ValidationError("Code, NameAr and NameEn are required");
        var code = dto.Code.Trim();
        var englishName = dto.NameEn.Trim();
        if (await _uow.Units.AnyAsync(x => x.Code == code || x.NameEn == englishName, ct)) return AppResponse<int>.ValidationError("Unit code or English name already exists");
        var row = new Entities.Models.Unit { Code = code, NameAr = dto.NameAr.Trim(), NameEn = englishName, IsActive = dto.IsActive ?? true, CreatedAt = DateTime.UtcNow };
        await _uow.Units.AddAsync(row, ct); await _uow.CompleteAsync(ct); return AppResponse<int>.Created(row.Id, $"/api/v1/admin/units/{row.Id}");
    }
    public async Task<AppResponse<bool>> UpdateUnitAsync(int id, UnitWriteDto dto, CancellationToken ct)
    {
        var row = await _uow.Units.GetByIdAsync(id, ct); if (row is null) return AppResponse<bool>.NotFound("Unit not found");
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn)) return AppResponse<bool>.ValidationError("Code, NameAr and NameEn are required");
        var code = dto.Code.Trim();
        if (await _uow.Units.AnyAsync(x => x.Id != id && (x.Code == code || x.NameEn == dto.NameEn.Trim()), ct)) return AppResponse<bool>.ValidationError("Unit code or English name already exists");
        row.Code = code; row.NameAr = dto.NameAr.Trim(); row.NameEn = dto.NameEn.Trim(); if (dto.IsActive.HasValue) row.IsActive = dto.IsActive.Value;
        await _uow.Units.UpdateAsync(row, ct); await _uow.CompleteAsync(ct); return AppResponse<bool>.Ok(true, "Unit updated");
    }
    public async Task<AppResponse<bool>> SetUnitActiveAsync(int id, bool active, CancellationToken ct)
    {
        var row = await _uow.Units.GetByIdAsync(id, ct); if (row is null) return AppResponse<bool>.NotFound("Unit not found");
        row.IsActive = active; await _uow.Units.UpdateAsync(row, ct); await _uow.CompleteAsync(ct); return AppResponse<bool>.Ok(true, active ? "Unit activated" : "Unit deactivated");
    }
    private static CompanyDto Map(Company x) => new(x.Id, x.NameAr, x.NameEn, x.IsActive);
    private static UnitDto Map(Entities.Models.Unit x) => new(x.Id, x.Code, x.NameAr, x.NameEn, x.IsActive);
}
