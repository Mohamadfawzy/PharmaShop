using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class ProductStateChangeDto
{
    public byte[] RowVersion { get; init; } = default!;

    public string? AuditReason { get; init; }
}