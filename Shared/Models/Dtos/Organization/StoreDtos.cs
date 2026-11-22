using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Organization;

public sealed class StoreDto
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? WorkingHours { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? RowVersion { get; set; }
}

public sealed class StoreCreateDto
{
    [Range(1, int.MaxValue)] public int PharmacyId { get; set; }
    [Required, MaxLength(150)] public string NameAr { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string NameEn { get; set; } = string.Empty;
    [MaxLength(30)] public string? Code { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    [MaxLength(50)] public string? Phone { get; set; }
    [MaxLength(50)] public string? WhatsApp { get; set; }
    [EmailAddress, MaxLength(256)] public string? Email { get; set; }
    [MaxLength(500)] public string? WorkingHours { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class StoreUpdateDto
{
    [Required, MaxLength(150)] public string NameAr { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string NameEn { get; set; } = string.Empty;
    [MaxLength(30)] public string? Code { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public bool IsActive { get; set; }
    public string? RowVersion { get; set; }
}

public sealed class StoreContactDto
{
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? WorkingHours { get; set; }
}

public sealed class StoreContactUpdateDto
{
    [MaxLength(50)] public string? Phone { get; set; }
    [MaxLength(50)] public string? WhatsApp { get; set; }
    [EmailAddress, MaxLength(256)] public string? Email { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    [MaxLength(500)] public string? WorkingHours { get; set; }
    public string? RowVersion { get; set; }
}

public sealed class StoreActiveDto
{
    public bool IsActive { get; set; }
}
