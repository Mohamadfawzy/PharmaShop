using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Promotion;


public sealed class PromotionReplaceProductsDto
{
    public List<PromotionProductReplaceItemDto> Items { get; set; } = new();
}

public sealed class PromotionProductReplaceItemDto
{
    public int? ProductId { get; set; }           // Optional local mapping
    public decimal ErpProductId { get; set; }     // Required by schema
    public decimal? ErpOfferId { get; set; }      // Optional
}

public sealed class PromotionReplaceProductsResultDto
{
    public int PromotionId { get; set; }

    public int RequestedCount { get; set; }
    public int FinalActiveCount { get; set; }

    public int InsertedCount { get; set; }
    public int RestoredCount { get; set; }
    public int SoftDeletedCount { get; set; }
    public int UnchangedCount { get; set; }

    public List<decimal> DuplicatesInRequest { get; set; } = new();
}


public sealed class PromotionProductRow
{
    public int Id { get; set; }
    public decimal ErpProductId { get; set; }
    public DateTime? DeletedAt { get; set; }
}
