using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;

public sealed class SetUserRolesRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one role is required.")]
    public List<string> Roles { get; set; } = new();
}