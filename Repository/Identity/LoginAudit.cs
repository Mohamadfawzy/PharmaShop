using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Identity;

public sealed class LoginAudit
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public string Outcome { get; set; } = default!;          // "Success" / "Failure"
    public string? FailureReason { get; set; }               // enum-like string

    public int? UserId { get; set; }

    public string? IdentifierMasked { get; set; }
    public byte[]? IdentifierHash { get; set; }              // 32 bytes SHA-256

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public string? TraceId { get; set; }
    public int? PharmacyId { get; set; }

    public int? LatencyMs { get; set; }
}