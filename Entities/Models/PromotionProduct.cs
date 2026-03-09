using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PromotionProduct
{
    public int Id { get; set; }

    public int PromotionId { get; set; }

    public int? ProductId { get; set; }

    public decimal? ErpOfferId { get; set; }

    public decimal ErpProductId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;
}
