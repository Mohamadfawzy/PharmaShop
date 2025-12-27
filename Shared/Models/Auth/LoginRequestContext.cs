using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;

public sealed class LoginRequestContext
{
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? TraceId { get; init; }

    // الآن صيدلية واحدة، لاحقًا ستأتي من Tenant/Domain logic
    public int PharmacyId { get; init; } = 1;
}