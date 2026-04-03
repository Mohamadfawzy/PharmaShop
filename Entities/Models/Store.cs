using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Store
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Code { get; set; }

    public string? Address { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
