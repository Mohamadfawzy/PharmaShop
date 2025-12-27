using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;

public sealed class UserRolesResponseDto
{
    public int UserId { get; set; }
    public List<string> Roles { get; set; } = new();
}
