namespace WebAPI.SpecificDtos;


public sealed class PrescriptionCreateFormData
{
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public string? Notes { get; set; }
    public int? PrimaryIndex { get; set; }

    public List<IFormFile> Files { get; set; } = new();
}
