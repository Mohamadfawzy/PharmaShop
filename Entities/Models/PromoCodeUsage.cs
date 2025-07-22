using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PromoCodeUsage
{
    public int Id { get; set; }

    public int PromoCodeId { get; set; }

    public int CustomerId { get; set; }

    public DateTime UsedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual PromoCode PromoCode { get; set; } = null!;
}
