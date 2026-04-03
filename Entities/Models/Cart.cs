using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public byte Status { get; set; }

    public DateTime StatusUpdatedAt { get; set; }

    public string? ExpiredReason { get; set; }

    public string? DeviceId { get; set; }

    public string? AppInstanceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
