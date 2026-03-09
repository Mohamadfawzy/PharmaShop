using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Brand
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
