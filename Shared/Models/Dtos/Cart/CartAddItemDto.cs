using Shared.Enums.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class CartAddItemDto
{
    public int CustomerId { get; set; }     // For now (later: from token)
    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public UnitLevel UnitLevel { get; set; } = UnitLevel.Outer;

    public decimal Quantity { get; set; }   // Must be > 0

    public string? DeviceId { get; set; }       // Optional
    public string? AppInstanceId { get; set; }  // Optional
}
