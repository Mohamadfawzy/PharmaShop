namespace Shared.Models.Dtos.Prescription;

public sealed class AdminPrescriptionListItemDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int StoreId { get; set; }

    public byte Status { get; set; }                 // 1=Submitted,2=InReview,3=ReadyForCheckout,4=Closed,5=Rejected
    public DateTime StatusUpdatedAt { get; set; }

    public int? ReviewedBy { get; set; }
    public DateTime? ReadyForCheckoutAt { get; set; }

    public DateTime CreatedAt { get; set; }
}