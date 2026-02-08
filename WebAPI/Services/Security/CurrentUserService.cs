using Contracts.IServices;
using System.Security.Claims;

namespace WebAPI.Services.Security;


public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        GetClaimValue(ClaimTypes.NameIdentifier)
        ?? GetClaimValue("sub"); 

    public string? UserName =>
        GetClaimValue(ClaimTypes.Name)
        ?? GetClaimValue("preferred_username")
        ?? GetClaimValue("username");

    public string? Email =>
        GetClaimValue(ClaimTypes.Email)
        ?? GetClaimValue("email");

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
             .Select(c => c.Value)
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .ToList()
        ?? new List<string>();

    public bool IsInRole(string role)
        => !string.IsNullOrWhiteSpace(role) &&
           Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

    // للمستقبل: Multi-Pharmacy
    // ضع claim اسمه "pharmacy_id" في الـJWT عند تسجيل الدخول
    public int? PharmacyId
    {
        get
        {
            var v = GetClaimValue("pharmacy_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }

    private string? GetClaimValue(string claimType)
        => User?.FindFirst(claimType)?.Value;
}
