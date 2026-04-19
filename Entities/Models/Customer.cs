using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Customer
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string FullNameAr { get; set; } = null!;

    public string FullNameEn { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? NationalId { get; set; }

    public string CustomerType { get; set; } = null!;

    public int Points { get; set; }

    public DateTime? PointsExpiryDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual CustomerAddress? CustomerAddress { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual AspNetUser? User { get; set; }
}
