using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Company
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
