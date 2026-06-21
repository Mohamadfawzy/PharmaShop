using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Carts;

internal sealed class CartPricingInfo
{
    public decimal CurrentUnitPrice { get; set; }
    public decimal AvailableQty { get; set; }
}
