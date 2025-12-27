using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;

public sealed class LoginResponseDto
{
    public int UserId { get; set; }
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }

    // in the future RefreshToken + TokenType
    // public string RefreshToken { get; set; } = default!;
    // public string TokenType { get; set; } = "Bearer";
}