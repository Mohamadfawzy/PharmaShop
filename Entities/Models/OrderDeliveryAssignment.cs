namespace Entities.Models;

public class OrderDeliveryAssignment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int DeliveryAgentUserId { get; set; }
    public int? AssignedByUserId { get; set; }
    public int? UnassignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual AspNetUser DeliveryAgentUser { get; set; } = null!;
    public virtual AspNetUser? AssignedByUser { get; set; }
}
