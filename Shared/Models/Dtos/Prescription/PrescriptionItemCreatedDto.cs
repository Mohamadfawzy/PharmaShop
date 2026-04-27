namespace Shared.Models.Dtos.Prescription;

public sealed class PrescriptionItemCreatedDto
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }

    public int? ProductId { get; set; }
    public string RequestedName { get; set; } = default!;
    public decimal? RequestedQuantity { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}