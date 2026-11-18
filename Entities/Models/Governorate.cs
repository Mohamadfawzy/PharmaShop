namespace Entities.Models;

public partial class Governorate
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<GovernorateArea> Areas { get; set; } = new List<GovernorateArea>();
}
