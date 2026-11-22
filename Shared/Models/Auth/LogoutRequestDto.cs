using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Auth;

public sealed class LogoutRequestDto
{
    [Required, MaxLength(512)]
    public string RefreshToken { get; set; } = default!;
}
