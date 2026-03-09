using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Customer
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? PharmacyId { get; set; }

    public string FullName { get; set; } = null!;

    public string FullNameEn { get; set; } = null!;

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

    public virtual Pharmacy? Pharmacy { get; set; }

    public virtual ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();

    public virtual AspNetUser? User { get; set; }
}
