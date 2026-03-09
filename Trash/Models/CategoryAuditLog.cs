using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CategoryAuditLog
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string ChangeType { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime ChangeDate { get; set; }

    public string? Reason { get; set; }

    public virtual Category Category { get; set; } = null!;
}
