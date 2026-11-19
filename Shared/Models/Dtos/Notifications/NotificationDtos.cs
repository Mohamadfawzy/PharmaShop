namespace Shared.Models.Dtos.Notifications;

public sealed class NotificationQueryDto
{
    public bool? UnreadOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public sealed class NotificationPreferencesDto
{
    public bool OrderUpdatesEnabled { get; set; } = true;
    public bool PrescriptionUpdatesEnabled { get; set; } = true;
    public bool PointsUpdatesEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
}
