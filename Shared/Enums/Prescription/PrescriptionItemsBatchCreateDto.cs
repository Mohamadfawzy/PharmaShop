using Shared.Models.Dtos.Prescription;

namespace Shared.Enums.Prescription;


public sealed class PrescriptionItemsBatchCreateDto
{
    public List<PrescriptionItemCreateDto> Items { get; set; } = new();
}


public sealed class PrescriptionItemsBatchCreateResultDto
{
    public int PrescriptionId { get; set; }
    public int RequestedCount { get; set; }
    public int InsertedCount { get; set; }
    public List<int> CreatedItemIds { get; set; } = new();
}

