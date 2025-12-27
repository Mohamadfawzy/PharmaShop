using Shared.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;

public interface ILoginAuditService
{
    Task WriteAsync(LoginAuditEntry entry, CancellationToken ct);
}