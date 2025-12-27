using Contracts.IServices;
using Repository.Identity;
using Shared.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Auth;


public sealed class LoginAuditService : ILoginAuditService
{
    private readonly IdentityDbContext _identityDb;

    public LoginAuditService(IdentityDbContext identityDb)
    {
        _identityDb = identityDb;
    }

    public async Task WriteAsync(LoginAuditEntry entry, CancellationToken ct)
    {
        // لا ترمي exceptions للخارج: هذا Audit
        // الأفضل "يحاول" فقط
        try
        {
            var row = new LoginAudit
            {
                CreatedAtUtc = entry.CreatedAtUtc,
                Outcome = entry.Outcome,
                FailureReason = entry.FailureReason,
                UserId = entry.UserId,
                IdentifierMasked = entry.IdentifierMasked,
                IdentifierHash = entry.IdentifierHash,
                IpAddress = entry.IpAddress,
                UserAgent = entry.UserAgent,
                TraceId = entry.TraceId,
                PharmacyId = entry.PharmacyId,
                LatencyMs = entry.LatencyMs
            };

            _identityDb.LoginAudits.Add(row);
            await _identityDb.SaveChangesAsync(ct);
        }
        catch
        {
            // intentionally swallow – لا نريد أن يفشل login بسبب audit
        }
    }
}