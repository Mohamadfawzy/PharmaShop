using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Tag
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
