namespace Shared.Models.Dtos.Prescription;


public sealed class PrescriptionItemCreateDto
{
    public int? ProductId { get; set; }                // Optional matched product
    public string RequestedName { get; set; } = default!; // Required (table constraint)
    public decimal? RequestedQuantity { get; set; }    // Optional (> 0)
    public string? Notes { get; set; }
}

