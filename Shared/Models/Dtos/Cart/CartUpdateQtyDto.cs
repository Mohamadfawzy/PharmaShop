using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class CartUpdateQtyDto
{
    public int CustomerId { get; set; }     // For now (later: from token)
    public decimal Quantity { get; set; }   // Must be >= 1
}
