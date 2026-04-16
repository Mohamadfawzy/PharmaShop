using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Promotion;

public sealed class PromotionUpdateDto
{
    public decimal? ErpPgoId { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public int? TotalAmount { get; set; }
    public int? BasicAmount { get; set; }
    public int? OfferAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public bool? IsActive { get; set; }

    // Base64 RowVersion for concurrency (optional for MVP)
    public string? RowVersion { get; set; }
}

