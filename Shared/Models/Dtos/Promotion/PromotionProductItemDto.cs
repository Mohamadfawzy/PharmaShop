using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Promotion;


public sealed class PromotionProductItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public decimal ErpProductId { get; set; }
    public decimal? ErpOfferId { get; set; }
}
