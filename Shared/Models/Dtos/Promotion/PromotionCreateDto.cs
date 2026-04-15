using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Promotion;


public sealed class PromotionCreateDto
{
    public decimal? ErpPgoId { get; set; }      // Optional unique ERP id
    public string? Name { get; set; }
    public string? Notes { get; set; }

    public DateTime? StartAt { get; set; }      // Must be paired with EndAt
    public DateTime? EndAt { get; set; }        // Must be > StartAt

    public int? TotalAmount { get; set; }       // Optional
    public int? BasicAmount { get; set; }       // Optional (e.g., buy count)
    public int? OfferAmount { get; set; }       // Optional (e.g., discounted/free count)

    public decimal? DiscountPercent { get; set; } // Optional (0..100)

    public bool? IsActive { get; set; }         // Optional, default true
}