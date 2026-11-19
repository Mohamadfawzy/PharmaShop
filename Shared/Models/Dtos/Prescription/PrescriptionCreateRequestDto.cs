namespace Shared.Models.Dtos.Prescription;

public sealed class PrescriptionCreateRequestDto
{
    public int StoreId { get; set; }
    public string? Notes { get; set; }
    public int? PrimaryIndex { get; set; }     // Optional: 0-based primary image
}
