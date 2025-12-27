using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Auth;

public sealed class ConfirmEmailRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [MinLength(10)]
    public string Token { get; set; } = default!;
}