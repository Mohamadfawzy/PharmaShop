using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class CartUpdateQtyDto
{
    public decimal Quantity { get; set; }   // Must be >= 1
}
