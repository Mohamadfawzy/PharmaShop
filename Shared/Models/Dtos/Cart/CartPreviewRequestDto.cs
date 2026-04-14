using Shared.Enums.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Cart;


public sealed class CartPreviewRequestDto
{
    public int CartId { get; set; }
    public int CustomerId { get; set; }     // For now (later: from token)
    public int AddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
}
