using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class ProductAuditEventDto
{
    public Guid OperationId { get; init; }
    public string ChangeType { get; init; } = default!;

    public string? ChangedBy { get; init; }
    public DateTime ChangeDate { get; init; }
    public string? Reason { get; init; }

    public List<ProductAuditChangeDto> Changes { get; init; } = new();
}