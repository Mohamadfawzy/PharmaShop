using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class SalesHeaderReturn
{
    public int Id { get; set; }

    public string ReturnNumber { get; set; } = null!;

    public DateTime ReturnDate { get; set; }

    public int OriginalSalesHeaderId { get; set; }

    public int PharmacyId { get; set; }

    public int? CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual SalesHeader OriginalSalesHeader { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<SalesDetailsReturn> SalesDetailsReturns { get; set; } = new List<SalesDetailsReturn>();
}
