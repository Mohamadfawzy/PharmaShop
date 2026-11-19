namespace Entities.Models;

public sealed class CustomerNotificationPreference
{
    public int UserId { get; set; }
    public bool OrderUpdatesEnabled { get; set; } = true;
    public bool PrescriptionUpdatesEnabled { get; set; } = true;
    public bool PointsUpdatesEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
