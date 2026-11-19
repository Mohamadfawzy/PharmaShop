using Shared.Models.Dtos.Notifications;
using Shared.Responses;

namespace Contracts.IServices;

public interface INotificationService
{
    Task<AppResponse<List<NotificationDto>>> GetMineAsync(NotificationQueryDto query, CancellationToken ct);
    Task<AppResponse<int>> MarkReadAsync(int notificationId, CancellationToken ct);
    Task<AppResponse<int>> MarkAllReadAsync(CancellationToken ct);
    Task<AppResponse<NotificationPreferencesDto>> GetPreferencesAsync(CancellationToken ct);
    Task<AppResponse<NotificationPreferencesDto>> UpdatePreferencesAsync(NotificationPreferencesDto dto, CancellationToken ct);
    Task NotifyCustomerAsync(int customerId, string type, string title, string message, string? relatedEntityType, long? relatedEntityId, CancellationToken ct);
}
