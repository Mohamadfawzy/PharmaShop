using Shared.Enums.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Promotion;


public sealed class PromotionListQueryDto
{
    public string? Q { get; set; }               // Search in Name (contains)
    public bool? IsActive { get; set; }          // null=all, true=active, false=inactive

    public DateTime? From { get; set; }          // Optional date window filter (overlap)
    public DateTime? To { get; set; }            // Optional date window filter (overlap)

    public bool? OnlyRunningNow { get; set; }    // Optional: currently running promotions only

    public PromotionSortOption Sort { get; set; } = PromotionSortOption.Newest;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
