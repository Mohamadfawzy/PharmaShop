using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PromotionProduct
{
    public int PromotionId { get; set; }

    public int ProductId { get; set; }

    public int? MinQty { get; set; }

    public int? MaxQty { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
