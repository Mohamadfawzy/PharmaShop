using Shared.Enums.Prescription;

namespace Shared.Models.Dtos.Prescription;
public sealed class AdminPrescriptionListQueryDto
{
    public int StoreId { get; set; }                 // Required for admin queue
    public byte? Status { get; set; }                // 1..5, null=all
    public int? CustomerId { get; set; }             // Optional filter
    public int? ReviewedBy { get; set; }             // Optional filter (employee)

    public DateTime? From { get; set; }              // CreatedAt from
    public DateTime? To { get; set; }                // CreatedAt to

    public PrescriptionSortOption Sort { get; set; } = PrescriptionSortOption.StatusUpdatedDesc;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}