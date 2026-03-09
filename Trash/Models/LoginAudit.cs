using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class LoginAudit
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string Outcome { get; set; } = null!;

    public string? FailureReason { get; set; }

    public int? UserId { get; set; }

    public string? IdentifierMasked { get; set; }

    public byte[]? IdentifierHash { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? TraceId { get; set; }

    public int? PharmacyId { get; set; }

    public int? LatencyMs { get; set; }
}
