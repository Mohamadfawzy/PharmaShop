using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class SalesHeader
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? PrescriptionId { get; set; }

    public bool IsFromPrescription { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal Discount { get; set; }

    public decimal? NetAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public bool IsFreeShipping { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Prescription? Prescription { get; set; }

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();

    public virtual ICollection<SalesHeaderReturn> SalesHeaderReturns { get; set; } = new List<SalesHeaderReturn>();
}
