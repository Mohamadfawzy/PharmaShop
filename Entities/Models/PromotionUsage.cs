using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PromotionUsage
{
    public int Id { get; set; }

    public int PromotionId { get; set; }

    public int UserId { get; set; }

    public DateTime UsageDate { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
