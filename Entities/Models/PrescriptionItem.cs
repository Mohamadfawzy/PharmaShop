using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PrescriptionItem
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public int? ProductId { get; set; }

    public string RequestedName { get; set; } = null!;

    public decimal? RequestedQuantity { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
