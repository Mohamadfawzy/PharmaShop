using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using Shared.Constants;
using Shared.Enums;
using Shared.Models.Dtos.Organization;
using Shared.Responses;

namespace Service;

public sealed class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<AppResponse<List<EmployeeDto>>> GetAllAsync(int? pharmacyId, bool includeInactive, CancellationToken ct)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(
            criteria: e => e.DeletedAt == null &&
                (!pharmacyId.HasValue || e.PharmacyId == pharmacyId.Value) &&
                (includeInactive || e.IsActive),
            orderBy: q => q.OrderBy(e => e.PharmacyId).ThenBy(e => e.FullNameAr),
            include: q => q.Include(e => e.Pharmacy), ct: ct);
        return AppResponse<List<EmployeeDto>>.Ok(employees.Select(Map).ToList());
    }

    public async Task<AppResponse<List<EmployeeDto>>> GetDeletedAsync(int? pharmacyId, CancellationToken ct)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(
            criteria: e => e.DeletedAt != null && (!pharmacyId.HasValue || e.PharmacyId == pharmacyId.Value),
            orderBy: q => q.OrderBy(e => e.PharmacyId).ThenBy(e => e.FullNameAr),
            include: q => q.Include(e => e.Pharmacy), ct: ct);
        return AppResponse<List<EmployeeDto>>.Ok(employees.Select(Map).ToList());
    }

    public async Task<AppResponse<EmployeeDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var employee = await FindEmployeeAsync(e => e.Id == id, ct);
        return employee is null
            ? AppResponse<EmployeeDto>.NotFound("Employee not found")
            : AppResponse<EmployeeDto>.Ok(Map(employee));
    }

    public async Task<AppResponse<EmployeeDto>> GetCurrentAsync(string? userId, CancellationToken ct)
    {
        if (!int.TryParse(userId, out var id) || id <= 0)
            return AppResponse<EmployeeDto>.Unauthorized("User identity is invalid");

        var employee = await FindEmployeeAsync(e => e.UserId == id && e.DeletedAt == null && e.IsActive, ct);
        return employee is null
            ? AppResponse<EmployeeDto>.NotFound("Active employee profile not found")
            : AppResponse<EmployeeDto>.Ok(Map(employee));
    }

    public async Task<AppResponse<int>> CreateAsync(EmployeeCreateDto dto, CancellationToken ct)
    {
        var pharmacy = await _unitOfWork.Pharmacies.GetByIdNoTrackingAsync(dto.PharmacyId, ct);
        if (pharmacy is null || !pharmacy.IsActive)
            return AppResponse<int>.ValidationError("An active pharmacy is required");

        var roleName = NormalizeRole(dto.RoleName);
        if (roleName is null)
            return AppResponse<int>.ValidationError("RoleName must be Employee or DeliveryAgent");
        if (!await _roleManager.RoleExistsAsync(roleName))
            return AppResponse<int>.BusinessRuleViolation($"Required role '{roleName}' is missing; apply the identity role script first");

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) is not null)
            return AppResponse<int>.Conflict("Email is already registered");

        var user = new ApplicationUser
        {
            UserName = string.IsNullOrWhiteSpace(dto.UserName) ? email : dto.UserName.Trim(),
            Email = email,
            PhoneNumber = Normalize(dto.PhoneNumber),
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.InitialPassword);
        if (!createResult.Succeeded)
            return IdentityValidation(createResult);

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return IdentityValidation(roleResult);
        }

        var employee = new Employee
        {
            UserId = user.Id,
            PharmacyId = dto.PharmacyId,
            FullNameAr = dto.FullNameAr.Trim(),
            FullNameEn = Normalize(dto.FullNameEn),
            PhoneNumber = Normalize(dto.PhoneNumber),
            Email = email,
            NationalId = Normalize(dto.NationalId),
            JobTitle = Normalize(dto.JobTitle),
            EmployeeCode = Normalize(dto.EmployeeCode),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.Employees.AddAsync(employee, ct);
            await _unitOfWork.CompleteAsync(ct);
        }
        catch (DbUpdateException)
        {
            await _userManager.DeleteAsync(user);
            return AppResponse<int>.Conflict("Employee code or account is already linked to another employee");
        }

        return AppResponse<int>.Created(employee.Id, $"/api/v1/admin/employees/{employee.Id}", "Employee created successfully");
    }

    public async Task<AppResponse<int>> UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken ct)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null || employee.DeletedAt is not null)
            return AppResponse<int>.NotFound("Employee not found");

        var pharmacy = await _unitOfWork.Pharmacies.GetByIdNoTrackingAsync(dto.PharmacyId, ct);
        if (pharmacy is null || !pharmacy.IsActive)
            return AppResponse<int>.ValidationError("An active pharmacy is required");

        var code = Normalize(dto.EmployeeCode);
        if (code is not null && await _unitOfWork.Employees.AnyAsync(
                e => e.Id != id && e.PharmacyId == dto.PharmacyId && e.EmployeeCode == code && e.DeletedAt == null, ct))
            return AppResponse<int>.Conflict("Employee code already exists in this pharmacy");

        employee.PharmacyId = dto.PharmacyId;
        employee.FullNameAr = dto.FullNameAr.Trim();
        employee.FullNameEn = Normalize(dto.FullNameEn);
        employee.PhoneNumber = Normalize(dto.PhoneNumber);
        employee.NationalId = Normalize(dto.NationalId);
        employee.JobTitle = Normalize(dto.JobTitle);
        employee.EmployeeCode = code;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync(ct);
        return AppResponse<int>.Ok(employee.Id, "Employee updated successfully");
    }

    public async Task<AppResponse<int>> SetActiveAsync(int id, EmployeeActiveDto dto, CancellationToken ct)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null || employee.DeletedAt is not null)
            return AppResponse<int>.NotFound("Employee not found");

        return await UpdateStatusAsync(employee, dto.IsActive, softDelete: false, ct);
    }

    public async Task<AppResponse<int>> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null || employee.DeletedAt is not null)
            return AppResponse<int>.NotFound("Employee not found");

        return await UpdateStatusAsync(employee, active: false, softDelete: true, ct);
    }

    public async Task<AppResponse<int>> RestoreAsync(int id, CancellationToken ct)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null || employee.DeletedAt is null)
            return AppResponse<int>.NotFound("Deleted employee not found");

        ApplicationUser? user = null;
        bool? oldUserActive = null;
        if (employee.UserId.HasValue)
        {
            user = await _userManager.FindByIdAsync(employee.UserId.Value.ToString());
            if (user is not null)
            {
                oldUserActive = user.IsActive;
                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) return IdentityValidation(result);
            }
        }

        employee.IsActive = true;
        employee.DeletedAt = null;
        employee.UpdatedAt = DateTime.UtcNow;
        try
        {
            await _unitOfWork.CompleteAsync(ct);
        }
        catch
        {
            if (user is not null && oldUserActive.HasValue)
            {
                user.IsActive = oldUserActive.Value;
                await _userManager.UpdateAsync(user);
            }
            throw;
        }

        return AppResponse<int>.Ok(employee.Id, "Employee restored and activated successfully");
    }

    private async Task<AppResponse<int>> UpdateStatusAsync(Employee employee, bool active, bool softDelete, CancellationToken ct)
    {
        ApplicationUser? user = null;
        bool? oldUserActive = null;
        if (employee.UserId.HasValue)
        {
            user = await _userManager.FindByIdAsync(employee.UserId.Value.ToString());
            if (user is not null)
            {
                oldUserActive = user.IsActive;
                user.IsActive = active;
                var identityResult = await _userManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                    return IdentityValidation(identityResult);
            }
        }

        employee.IsActive = active;
        employee.UpdatedAt = DateTime.UtcNow;
        if (softDelete) employee.DeletedAt = employee.UpdatedAt;

        try
        {
            await _unitOfWork.CompleteAsync(ct);
        }
        catch
        {
            if (user is not null && oldUserActive.HasValue)
            {
                user.IsActive = oldUserActive.Value;
                await _userManager.UpdateAsync(user);
            }
            throw;
        }

        return AppResponse<int>.Ok(employee.Id, softDelete ? "Employee deleted successfully" : "Employee status updated successfully");
    }

    private Task<Employee?> FindEmployeeAsync(
        System.Linq.Expressions.Expression<Func<Employee, bool>> criteria,
        CancellationToken ct)
        => _unitOfWork.Employees.FirstOrDefaultAsync(
            criteria, include: q => q.Include(e => e.Pharmacy), ct: ct);

    private static string? NormalizeRole(string? role)
    {
        if (string.Equals(role?.Trim(), AppRoles.Employee, StringComparison.OrdinalIgnoreCase)) return AppRoles.Employee;
        if (string.Equals(role?.Trim(), AppRoles.DeliveryAgent, StringComparison.OrdinalIgnoreCase)) return AppRoles.DeliveryAgent;
        return null;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AppResponse<int> IdentityValidation(IdentityResult result)
    {
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]> { ["Identity"] = errors }, "Identity validation failed");
    }

    private static EmployeeDto Map(Employee e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        PharmacyId = e.PharmacyId,
        PharmacyName = e.Pharmacy?.Name ?? string.Empty,
        FullNameAr = e.FullNameAr,
        FullNameEn = e.FullNameEn,
        PhoneNumber = e.PhoneNumber,
        Email = e.Email,
        NationalId = e.NationalId,
        JobTitle = e.JobTitle,
        EmployeeCode = e.EmployeeCode,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        DeletedAt = e.DeletedAt
    };
}
