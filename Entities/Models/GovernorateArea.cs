namespace Entities.Models;

public partial class GovernorateArea
{
    public int Id { get; set; }
    public int GovernorateId { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual Governorate Governorate { get; set; } = null!;
}
