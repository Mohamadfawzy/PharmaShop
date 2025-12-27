using Contracts.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Identity;
using Shared.Constants;
using Shared.Enums;
using Shared.Models.Auth;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Auth;


public sealed class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserRoleService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<AppResponse<UserRolesResponseDto>> SetRolesAsync(int userId, List<string> roles, CancellationToken ct)
    {
        var normalized = NormalizeRoles(roles);
        if (normalized.Count == 0)
            return AppResponse<UserRolesResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["Roles"] = new[] { "At least one role is required." } });

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return AppResponse<UserRolesResponseDto>.NotFound("User not found");

        // Validate roles exist
        var missing = await GetMissingRolesAsync(normalized);
        if (missing.Count > 0)
        {
            return AppResponse<UserRolesResponseDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Roles"] = missing.Select(r => $"Role '{r}' does not exist.").ToArray()
                },
                detail: "Validation failed"
            );
        }

        // Remove all current roles then add new set
        var current = await _userManager.GetRolesAsync(user);

        // Remove only those currently present
        if (current.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, current);
            if (!removeResult.Succeeded)
                return IdentityFailed(removeResult);
        }

        var addResult = await _userManager.AddToRolesAsync(user, normalized);
        if (!addResult.Succeeded)
            return IdentityFailed(addResult);

        var finalRoles = await _userManager.GetRolesAsync(user);

        return AppResponse<UserRolesResponseDto>.Ok(new UserRolesResponseDto
        {
            UserId = user.Id,
            Roles = finalRoles.OrderBy(x => x).ToList()
        }, "User roles updated successfully");
    }

    public async Task<AppResponse<UserRolesResponseDto>> AddRolesAsync(int userId, List<string> roles, CancellationToken ct)
    {
        var normalized = NormalizeRoles(roles);
        if (normalized.Count == 0)
            return AppResponse<UserRolesResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["Roles"] = new[] { "At least one role is required." } });

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return AppResponse<UserRolesResponseDto>.NotFound("User not found");

        var missing = await GetMissingRolesAsync(normalized);
        if (missing.Count > 0)
        {
            return AppResponse<UserRolesResponseDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Roles"] = missing.Select(r => $"Role '{r}' does not exist.").ToArray()
                });
        }

        var addResult = await _userManager.AddToRolesAsync(user, normalized);
        if (!addResult.Succeeded)
            return IdentityFailed(addResult);

        var finalRoles = await _userManager.GetRolesAsync(user);

        return AppResponse<UserRolesResponseDto>.Ok(new UserRolesResponseDto
        {
            UserId = user.Id,
            Roles = finalRoles.OrderBy(x => x).ToList()
        }, "Roles added successfully");
    }

    public async Task<AppResponse<UserRolesResponseDto>> RemoveRolesAsync(int userId, List<string> roles, CancellationToken ct)
    {
        var normalized = NormalizeRoles(roles);
        if (normalized.Count == 0)
            return AppResponse<UserRolesResponseDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["Roles"] = new[] { "At least one role is required." } });

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return AppResponse<UserRolesResponseDto>.NotFound("User not found");

        // (اختياري) لو دور غير موجود أصلاً في النظام، نرجع Validation
        var missing = await GetMissingRolesAsync(normalized);
        if (missing.Count > 0)
        {
            return AppResponse<UserRolesResponseDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Roles"] = missing.Select(r => $"Role '{r}' does not exist.").ToArray()
                });
        }

        var removeResult = await _userManager.RemoveFromRolesAsync(user, normalized);
        if (!removeResult.Succeeded)
            return IdentityFailed(removeResult);

        var finalRoles = await _userManager.GetRolesAsync(user);

        return AppResponse<UserRolesResponseDto>.Ok(new UserRolesResponseDto
        {
            UserId = user.Id,
            Roles = finalRoles.OrderBy(x => x).ToList()
        }, "Roles removed successfully");
    }


    public async Task<AppResponse<List<RoleDto>>> GetAllRolesAsync(CancellationToken ct)
    {
        // RoleManager.Roles -> IQueryable
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                NameEn = r.NameEn,
                Description = r.Description
            })
            .ToListAsync(ct);

        return AppResponse<List<RoleDto>>.Ok(roles);
    }

    public async Task<AppResponse<UserRolesResponseDto>> GetUserRolesAsync(int userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return AppResponse<UserRolesResponseDto>.NotFound("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        return AppResponse<UserRolesResponseDto>.Ok(new UserRolesResponseDto
        {
            UserId = user.Id,
            Roles = roles.OrderBy(x => x).ToList()
        });
    }

    // ---------------- Helpers ----------------

    private static List<string> NormalizeRoles(IEnumerable<string> roles)
        => roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task<List<string>> GetMissingRolesAsync(List<string> roles)
    {
        var missing = new List<string>();
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                missing.Add(role);
        }
        return missing;
    }

    private static AppResponse<UserRolesResponseDto> IdentityFailed(IdentityResult result)
    {
        var errors = result.Errors.Select(e => e.Description).ToList();

        // لو تحب تربطها بـ fieldErrors: ضعها تحت Roles أو General
        return AppResponse<UserRolesResponseDto>.Fail(
            errors,
            errorCode: AppErrorCode.ValidationError,
            statusCode: ResponseDefaults.BadRequestStatusCode,
            title: ResponseDefaults.GetDefaultTitle(AppErrorCode.ValidationError),
            detail: "Identity operation failed",
            fieldErrors: new Dictionary<string, string[]>
            {
                ["Roles"] = errors.ToArray()
            }
        );
    }
}