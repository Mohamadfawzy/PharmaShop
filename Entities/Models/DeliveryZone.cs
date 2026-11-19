namespace Entities.Models;

public partial class DeliveryZone
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public decimal DeliveryFee { get; set; }
    public decimal? FreeDeliveryMinimumAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual Store Store { get; set; } = null!;
    public virtual ICollection<DeliveryZoneArea> Areas { get; set; } = new List<DeliveryZoneArea>();
}
