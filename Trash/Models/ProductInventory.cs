using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class ProductInventory
{
    public int PharmacyId { get; set; }

    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReservedQty { get; set; }

    public int? MinStockLevel { get; set; }

    public int? MaxStockLevel { get; set; }

    public DateTime? LastStockUpdateAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
