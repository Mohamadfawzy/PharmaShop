using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order;


public sealed class CheckoutLineData
{
    public int ProductId { get; set; }
    public byte UnitLevel { get; set; }              // 1=Outer,2=Inner
    public decimal Quantity { get; set; }

    public int StoreId { get; set; }
    public int? PrescriptionId { get; set; }
    public int? CartId { get; set; }

    public decimal OuterUnitPrice { get; set; }
    public decimal? InnerUnitPrice { get; set; }
    public int OuterUnitId { get; set; }
    public int? InnerUnitId { get; set; }
    public int? InnerPerOuter { get; set; }

    public bool AllowSplitSale { get; set; }
    public int Points { get; set; }                  // 0 => excluded from points
}
