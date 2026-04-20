using Shared.Enums.Cart;
using Shared.Enums.Order;

namespace Shared.Models.Dtos.Order;


public sealed class CheckoutRequestDto
{
    public int CustomerId { get; set; }           // For now (later: from token)
    public CheckoutSourceType SourceType { get; set; }
    public int SourceId { get; set; }             // CartId or PrescriptionId
    public int AddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
}

