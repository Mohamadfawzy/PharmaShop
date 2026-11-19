using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Customer;

public sealed class CustomerProfileDto
{
    public int Id { get; set; }
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

public sealed class CustomerProfileUpdateDto
{
    [Required, MaxLength(200)] public string FullNameAr { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FullNameEn { get; set; } = string.Empty;
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [MaxLength(10)] public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
