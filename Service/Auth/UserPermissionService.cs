using Contracts.IServices;
using Microsoft.AspNetCore.Identity;
using Repository.Identity;
using Shared.Constants;
using Shared.Enums;
using Shared.Models.Auth;
using Shared.Responses;
using System.Security.Claims;

namespace Service.Auth;

public sealed class UserPermissionService : IUserPermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserPermissionService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AppResponse<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return AppResponse<List<string>>.NotFound("User not found");

        var permissions = (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == AppPermissions.ClaimType)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        return AppResponse<List<string>>.Ok(permissions);
    }

    public async Task<AppResponse<List<string>>> SetUserPermissionsAsync(
        int userId,
        List<string> permissions,
        CancellationToken ct)
    {
        if (userId <= 0)
            return AppResponse<List<string>>.ValidationError("Invalid userId");

        var normalized = (permissions ?? new List<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => AppPermissions.PolicyPermissions.Values
                .FirstOrDefault(known => string.Equals(known, p.Trim(), StringComparison.OrdinalIgnoreCase)) ?? p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var invalid = normalized.Where(p => !AppPermissions.IsKnown(p)).ToArray();
        if (invalid.Length > 0)
        {
            return AppResponse<List<string>>.ValidationErrors(
                new Dictionary<string, string[]> { [nameof(SetUserPermissionsRequestDto.Permissions)] = invalid },
                detail: "One or more permissions are not recognized");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return AppResponse<List<string>>.NotFound("User not found");

        if (normalized.Count > 0)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains(AppRoles.Admin, StringComparer.OrdinalIgnoreCase);
            var isEmployee = roles.Contains(AppRoles.Employee, StringComparer.OrdinalIgnoreCase);
            var isDeliveryAgent = roles.Contains(AppRoles.DeliveryAgent, StringComparer.OrdinalIgnoreCase);

            if (!isAdmin && !isEmployee && !isDeliveryAgent)
                return AppResponse<List<string>>.ValidationError("Permissions can only be assigned to staff accounts");

            if (normalized.Contains(AppPolicies.CanDeliverOrders, StringComparer.OrdinalIgnoreCase) && !isDeliveryAgent)
                return AppResponse<List<string>>.ValidationError("CanDeliverOrders requires the DeliveryAgent role");

            if (normalized.Any(p => p != AppPolicies.CanDeliverOrders) && !isAdmin && !isEmployee)
                return AppResponse<List<string>>.ValidationError("Staff permissions require the Employee role");
        }

        var existing = (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == AppPermissions.ClaimType)
            .ToList();

        if (existing.Count > 0)
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, existing);
            if (!removeResult.Succeeded)
                return IdentityFailure(removeResult);
        }

        var requestedClaims = normalized
            .Select(p => new Claim(AppPermissions.ClaimType, p))
            .ToList();

        if (requestedClaims.Count > 0)
        {
            var addResult = await _userManager.AddClaimsAsync(user, requestedClaims);
            if (!addResult.Succeeded)
            {
                if (existing.Count > 0)
                    await _userManager.AddClaimsAsync(user, existing);

                return IdentityFailure(addResult);
            }
        }

        return AppResponse<List<string>>.Ok(normalized.OrderBy(p => p).ToList(), "User permissions updated successfully");
    }

    private static AppResponse<List<string>> IdentityFailure(IdentityResult result)
    {
        var errors = result.Errors.Select(e => e.Description).ToArray();
        return AppResponse<List<string>>.Fail(
            errors,
            errorCode: AppErrorCode.InternalServerError,
            statusCode: 500,
            detail: "Failed to update user permissions");
    }
}
