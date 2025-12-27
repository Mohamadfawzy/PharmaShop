using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Auth;

public sealed class LoginAuditEntry
{
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public string Outcome { get; init; } = default!;      // "Success" / "Failure"
    public string? FailureReason { get; init; }

    public int? UserId { get; init; }

    public string? IdentifierMasked { get; init; }
    public byte[]? IdentifierHash { get; init; }

    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public string? TraceId { get; init; }
    public int? PharmacyId { get; init; }

    public int? LatencyMs { get; init; }
}