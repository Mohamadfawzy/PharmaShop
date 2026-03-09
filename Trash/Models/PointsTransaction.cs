using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PointsTransaction
{
    public long Id { get; set; }

    public int PharmacyId { get; set; }

    public int CustomerId { get; set; }

    public byte TxType { get; set; }

    public int Points { get; set; }

    public byte? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
