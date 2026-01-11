using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Store
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public string? Address { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();

    public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
}
