using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public byte Status { get; set; }

    public DateTime StatusUpdatedAt { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReadyForCheckoutAt { get; set; }

    public string? RejectReason { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual ICollection<PrescriptionImage> PrescriptionImages { get; set; } = new List<PrescriptionImage>();

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

    public virtual Employee? ReviewedByNavigation { get; set; }

    public virtual Store Store { get; set; } = null!;
}
