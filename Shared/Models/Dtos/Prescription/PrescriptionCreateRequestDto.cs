namespace Shared.Models.Dtos.Prescription;

public sealed class PrescriptionCreateRequestDto
{
    public int CustomerId { get; set; }        // For now (later: from token)
    public int StoreId { get; set; }
    public string? Notes { get; set; }
    public int? PrimaryIndex { get; set; }     // Optional: 0-based primary image
}
