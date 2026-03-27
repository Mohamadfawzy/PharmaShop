using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;

public sealed class ProductAuditChangeDto
{
    public string FieldName { get; init; } = default!;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}