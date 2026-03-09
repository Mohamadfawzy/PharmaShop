using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models;


public sealed class ProductAuditLog
{
    public long Id { get; set; }

    public int PharmacyId { get; set; }
    public int ProductId { get; set; }

    public Guid OperationId { get; set; }
    public string ChangeType { get; set; } = default!;

    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string? ChangedBy { get; set; }
    public DateTime ChangeDate { get; set; }

    public string? Reason { get; set; }
}