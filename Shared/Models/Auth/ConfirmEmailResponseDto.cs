using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;

public sealed class ConfirmEmailResponseDto
{
    public int UserId { get; set; }
    public bool EmailConfirmed { get; set; }
}