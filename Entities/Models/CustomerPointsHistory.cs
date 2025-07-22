using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CustomerPointsHistory
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int PharmacyId { get; set; }

    public int Points { get; set; }

    public string Reason { get; set; } = null!;

    public string SourceType { get; set; } = null!;

    public int? SourceReferenceId { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
