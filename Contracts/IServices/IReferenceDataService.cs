using Shared.Models.Dtos.Catalog;
using Shared.Responses;

namespace Contracts.IServices;

public interface IReferenceDataService
{
    Task<AppResponse<List<CompanyDto>>> GetCompaniesAsync(bool includeInactive, CancellationToken ct);
    Task<AppResponse<CompanyDto>> GetCompanyAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> CreateCompanyAsync(CompanyWriteDto dto, CancellationToken ct);
    Task<AppResponse<bool>> UpdateCompanyAsync(int id, CompanyWriteDto dto, CancellationToken ct);
    Task<AppResponse<bool>> DeleteCompanyAsync(int id, bool restore, CancellationToken ct);
    Task<AppResponse<List<UnitDto>>> GetUnitsAsync(bool includeInactive, CancellationToken ct);
    Task<AppResponse<UnitDto>> GetUnitAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> CreateUnitAsync(UnitWriteDto dto, CancellationToken ct);
    Task<AppResponse<bool>> UpdateUnitAsync(int id, UnitWriteDto dto, CancellationToken ct);
    Task<AppResponse<bool>> SetUnitActiveAsync(int id, bool active, CancellationToken ct);
}
