using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Employee
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int PharmacyId { get; set; }

    public string FullNameAr { get; set; } = null!;

    public string? FullNameEn { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? NationalId { get; set; }

    public string? JobTitle { get; set; }

    public string? EmployeeCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual AspNetUser? User { get; set; }
}
