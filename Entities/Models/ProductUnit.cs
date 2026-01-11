using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class ProductUnit
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public int ProductId { get; set; }

    public int UnitId { get; set; }

    public int SortOrder { get; set; }

    public string? UnitCode { get; set; }

    public string? SKU { get; set; }

    public int? ParentProductUnitId { get; set; }

    public decimal? UnitsPerParent { get; set; }

    public int? BaseUnitId { get; set; }

    public decimal? BaseQuantity { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal? CostPrice { get; set; }

    public decimal ListPrice { get; set; }

    public DateTime? PriceUpdatedAt { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Unit? BaseUnit { get; set; }

    public virtual ICollection<ProductUnit> InverseParentProductUnit { get; set; } = new List<ProductUnit>();

    public virtual ProductUnit? ParentProductUnit { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();

    public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();

    public virtual Unit Unit { get; set; } = null!;
}
