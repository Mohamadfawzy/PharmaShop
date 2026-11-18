using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Organization;

public sealed class EmployeeDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string? FullNameEn { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? NationalId { get; set; }
    public string? JobTitle { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public sealed class EmployeeCreateDto
{
    [Range(1, int.MaxValue)] public int PharmacyId { get; set; }
    [Required, MaxLength(200)] public string FullNameAr { get; set; } = string.Empty;
    [MaxLength(200)] public string? FullNameEn { get; set; }
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = string.Empty;
    [MaxLength(20)] public string? NationalId { get; set; }
    [MaxLength(100)] public string? JobTitle { get; set; }
    [MaxLength(50)] public string? EmployeeCode { get; set; }
    [MaxLength(256)] public string? UserName { get; set; }
    [Required, MinLength(8), MaxLength(100)] public string InitialPassword { get; set; } = string.Empty;
    [Required] public string RoleName { get; set; } = "Employee";
}

public sealed class EmployeeUpdateDto
{
    [Range(1, int.MaxValue)] public int PharmacyId { get; set; }
    [Required, MaxLength(200)] public string FullNameAr { get; set; } = string.Empty;
    [MaxLength(200)] public string? FullNameEn { get; set; }
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [MaxLength(20)] public string? NationalId { get; set; }
    [MaxLength(100)] public string? JobTitle { get; set; }
    [MaxLength(50)] public string? EmployeeCode { get; set; }
}

public sealed class EmployeeActiveDto
{
    public bool IsActive { get; set; }
}
