using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Customer
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int PharmacyId { get; set; }

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? NationalId { get; set; }

    public string? CustomerType { get; set; }

    public int Points { get; set; }

    public DateTime? PointsExpiryDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    public virtual ICollection<CustomerPointsHistory> CustomerPointsHistories { get; set; } = new List<CustomerPointsHistory>();

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<PromoCodeUsage> PromoCodeUsages { get; set; } = new List<PromoCodeUsage>();

    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();

    public virtual ICollection<SalesHeaderReturn> SalesHeaderReturns { get; set; } = new List<SalesHeaderReturn>();

    public virtual ICollection<SalesHeader> SalesHeaders { get; set; } = new List<SalesHeader>();

    public virtual User? User { get; set; }
}
