using Entities.Models;
using Shared.Models.Dtos.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Carts;

internal sealed class CartItemUpsertResult
{
    public CartItem CartItem { get; set; } = default!;
    public bool RequiresRefresh { get; set; }
    public string? RefreshReason { get; set; }
    public ItemPriceChangeDto? PriceChange { get; set; }
}