using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Auth;

public sealed class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = default!;

    [MaxLength(50)]
    public string? UserName { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = default!;
}
