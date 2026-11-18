using Shared.Models.Dtos.Organization;
using Shared.Responses;

namespace Contracts.IServices;

public interface IEmployeeService
{
    Task<AppResponse<List<EmployeeDto>>> GetAllAsync(int? pharmacyId, bool includeInactive, CancellationToken ct);
    Task<AppResponse<List<EmployeeDto>>> GetDeletedAsync(int? pharmacyId, CancellationToken ct);
    Task<AppResponse<EmployeeDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<AppResponse<EmployeeDto>> GetCurrentAsync(string? userId, CancellationToken ct);
    Task<AppResponse<int>> CreateAsync(EmployeeCreateDto dto, CancellationToken ct);
    Task<AppResponse<int>> UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken ct);
    Task<AppResponse<int>> SetActiveAsync(int id, EmployeeActiveDto dto, CancellationToken ct);
    Task<AppResponse<int>> SoftDeleteAsync(int id, CancellationToken ct);
    Task<AppResponse<int>> RestoreAsync(int id, CancellationToken ct);
}
