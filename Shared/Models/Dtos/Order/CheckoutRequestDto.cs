using Shared.Enums.Cart;
using Shared.Enums.Order;
using System.Text.Json.Serialization;

namespace Shared.Models.Dtos.Order;


public sealed class CheckoutRequestDto
{
    [JsonIgnore]
    public int CustomerId { get; set; }           // Set from the authenticated customer by the service
    public CheckoutSourceType SourceType { get; set; }
    public int SourceId { get; set; }             // CartId or PrescriptionId
    public int AddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
}

