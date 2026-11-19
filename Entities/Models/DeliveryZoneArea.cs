namespace Entities.Models;

public partial class DeliveryZoneArea
{
    public int Id { get; set; }
    public int DeliveryZoneId { get; set; }
    public int StoreId { get; set; }
    public int GovernorateAreaId { get; set; }
    public virtual DeliveryZone DeliveryZone { get; set; } = null!;
    public virtual GovernorateArea GovernorateArea { get; set; } = null!;
}
