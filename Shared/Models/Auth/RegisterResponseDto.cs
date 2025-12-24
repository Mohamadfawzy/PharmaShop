using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;
public sealed class RegisterResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = default!;
    public bool EmailConfirmationRequired { get; set; } = true;
}