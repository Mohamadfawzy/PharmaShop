namespace Shared.Models.Auth;

using Shared.Models.Dtos.Customer;

public sealed class CurrentUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int? PharmacyId { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public CustomerProfileDto? Customer { get; set; }
}

public sealed class SetUserPermissionsRequestDto
{
    public List<string> Permissions { get; set; } = new();
}
