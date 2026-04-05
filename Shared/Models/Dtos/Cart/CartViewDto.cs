using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class CartViewDto
{
    public int? CartId { get; set; }
    public int CustomerId { get; set; }
    public int? StoreId { get; set; }

    public byte? Status { get; set; } // Keep simple (can map to enum in UI)
    public DateTime? StatusUpdatedAt { get; set; }

    public decimal SubtotalCurrent { get; set; }

    public List<CartItemViewDto> Items { get; set; } = new();
}
