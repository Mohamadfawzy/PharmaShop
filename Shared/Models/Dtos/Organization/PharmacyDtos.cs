using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Organization;

public sealed class PharmacyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? LicenseNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
}

public class PharmacyCreateDto
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string NameEn { get; set; } = string.Empty;
    [MaxLength(150)] public string? OwnerName { get; set; }
    [MaxLength(100)] public string? LicenseNumber { get; set; }
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [EmailAddress, MaxLength(150)] public string? Email { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public sealed class PharmacyUpdateDto : PharmacyCreateDto
{
    public bool IsActive { get; set; }
}
