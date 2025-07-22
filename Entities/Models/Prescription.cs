using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int PharmacyId { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = null!;

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<PrescriptionImage> PrescriptionImages { get; set; } = new List<PrescriptionImage>();

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

    public virtual User? ReviewedByUser { get; set; }

    public virtual ICollection<SalesHeader> SalesHeaders { get; set; } = new List<SalesHeader>();
}
