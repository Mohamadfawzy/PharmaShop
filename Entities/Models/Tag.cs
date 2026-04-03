using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
