using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CustomerPointTransaction
{
    public long Id { get; set; }

    public int CustomerId { get; set; }

    public int? LotId { get; set; }

    public int? OrderId { get; set; }

    public int? OrderItemId { get; set; }

    public byte TransactionType { get; set; }

    public int PointsDelta { get; set; }

    public int? BalanceAfter { get; set; }

    public int? PointsPerEGP { get; set; }

    public decimal? AmountEGP { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public byte? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public long? SourceTransactionId { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee? CreatedByEmployee { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<CustomerPointTransaction> InverseSourceTransaction { get; set; } = new List<CustomerPointTransaction>();

    public virtual CustomerPointLot? Lot { get; set; }

    public virtual Order? Order { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual CustomerPointTransaction? SourceTransaction { get; set; }
}
