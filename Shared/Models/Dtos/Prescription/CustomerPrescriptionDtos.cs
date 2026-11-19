namespace Shared.Models.Dtos.Prescription;

public sealed class CustomerPrescriptionListQueryDto
{
    public byte? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool NewestFirst { get; set; } = true;
}

public class CustomerPrescriptionListItemDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public byte Status { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadyForCheckoutAt { get; set; }
    public string? RejectReason { get; set; }
    public string? PrimaryImageUrl { get; set; }
}

public sealed class CustomerPrescriptionDetailsDto : CustomerPrescriptionListItemDto
{
    public string? Notes { get; set; }
    public List<PrescriptionCreatedImageDto> Images { get; set; } = [];
    public List<PrescriptionItemListItemDto> Items { get; set; } = [];
}
