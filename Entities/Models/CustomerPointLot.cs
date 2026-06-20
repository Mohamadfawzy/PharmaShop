using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CustomerPointLot
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int? OrderId { get; set; }

    public int? OrderItemId { get; set; }

    public int PointsTotal { get; set; }

    public int RemainingPoints { get; set; }

    public byte Status { get; set; }

    public DateTime EarnedAt { get; set; }

    public DateTime? AvailableAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<CustomerPointTransaction> CustomerPointTransactions { get; set; } = new List<CustomerPointTransaction>();

    public virtual Order? Order { get; set; }

    public virtual OrderItem? OrderItem { get; set; }
}
