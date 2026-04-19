using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public int? PrescriptionId { get; set; }

    public byte Status { get; set; }

    public DateTime StatusUpdatedAt { get; set; }

    public byte PaymentMethod { get; set; }

    public byte PaymentStatus { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ItemsDiscountTotal { get; set; }

    public decimal DeliveryFee { get; set; }

    public int RedeemedPoints { get; set; }

    public decimal RedeemedAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public int EarnedPoints { get; set; }

    public int? AddressId { get; set; }

    public string? DeliveryTitle { get; set; }

    public string DeliveryCity { get; set; } = null!;

    public string? DeliveryRegion { get; set; }

    public string DeliveryStreet { get; set; } = null!;

    public double? DeliveryLatitude { get; set; }

    public double? DeliveryLongitude { get; set; }

    public string? DeliveryPhone { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual CustomerAddress? Address { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Prescription? Prescription { get; set; }

    public virtual Store Store { get; set; } = null!;
}
