using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Auth;


public sealed class LoginRequestDto
{
    [Required]
    [MaxLength(256)]
    public string Identifier { get; set; } = default!; // Email or Username

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = default!;

    // مستقبلًا: device info / remember me / 2FA code
    public bool RememberMe { get; set; } = false;
}