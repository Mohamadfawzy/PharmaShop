using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class ProductBatch
{
    public long Id { get; set; }

    public int PharmacyId { get; set; }

    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public string BatchNumber { get; set; } = null!;

    public DateOnly? ExpirationDate { get; set; }

    public DateTime ReceivedAt { get; set; }

    public int QuantityReceived { get; set; }

    public int QuantityOnHand { get; set; }

    public decimal? CostPrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
