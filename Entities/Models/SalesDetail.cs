using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class SalesDetail
{
    public int Id { get; set; }

    public int SalesHeaderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Notes { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SalesHeader SalesHeader { get; set; } = null!;
}
