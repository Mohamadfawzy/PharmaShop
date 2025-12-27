using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;


public sealed class ResetPasswordRequestDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string Token { get; set; } = default!;

    [Required]
    public string NewPassword { get; set; } = default!;
}