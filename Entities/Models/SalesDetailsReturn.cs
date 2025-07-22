using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class SalesDetailsReturn
{
    public int Id { get; set; }

    public int SalesHeaderReturnId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SalesHeaderReturn SalesHeaderReturn { get; set; } = null!;
}
