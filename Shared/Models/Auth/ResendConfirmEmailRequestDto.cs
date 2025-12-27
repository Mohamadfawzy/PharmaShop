using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Auth;

public sealed class ResendConfirmEmailRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;
}