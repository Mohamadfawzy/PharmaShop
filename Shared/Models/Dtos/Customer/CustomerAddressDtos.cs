using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Customer;

public sealed class CustomerAddressDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public string GovernorateNameAr { get; set; } = string.Empty;
    public int GovernorateAreaId { get; set; }
    public string AreaNameAr { get; set; } = string.Empty;
    public string AreaNameEn { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class CustomerAddressWriteDto
{
    [Required, MaxLength(100)] public string Title { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int GovernorateId { get; set; }
    [Range(1, int.MaxValue)] public int GovernorateAreaId { get; set; }
    [Required, MaxLength(300)] public string Street { get; set; } = string.Empty;
    [MaxLength(200)] public string? Landmark { get; set; }
    [Range(-90, 90)] public double? Latitude { get; set; }
    [Range(-180, 180)] public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}
