using Contracts;
using Contracts.IServices;
using Contracts.Notifications;
using Entities.Models;
using Microsoft.Extensions.Logging;
using Shared.Models.Dtos.Notifications;
using Shared.Responses;

namespace Service;

public sealed class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork uow, ICurrentUserService currentUser, IEmailSender emailSender, ILogger<NotificationService> logger)
    { _uow = uow; _currentUser = currentUser; _emailSender = emailSender; _logger = logger; }

    public async Task<AppResponse<List<NotificationDto>>> GetMineAsync(NotificationQueryDto query, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return AppResponse<List<NotificationDto>>.Unauthorized("User identity is invalid");
        if (query is null) return AppResponse<List<NotificationDto>>.ValidationError("Query is required");
        query.Page = Math.Max(1, query.Page); query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        var criteria = (System.Linq.Expressions.Expression<Func<UserNotification, bool>>)(n => n.UserId == userId.Value && (!query.UnreadOnly.HasValue || !query.UnreadOnly.Value || !n.IsRead));
        var total = await _uow.UserNotifications.CountAsync(criteria, ct);
        var rows = await _uow.UserNotifications.GetAllAsync<NotificationDto>(n => new NotificationDto
        {
            Id = n.Id, Type = n.Type, Title = n.Title, Message = n.Message,
            RelatedEntityType = n.RelatedEntityType, RelatedEntityId = n.RelatedEntityId,
            IsRead = n.IsRead, CreatedAt = n.CreatedAt, ReadAt = n.ReadAt
        }, criteria, q => q.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id),
            (query.Page - 1) * query.PageSize, query.PageSize, asNoTracking: true, ct: ct);
        return AppResponse<List<NotificationDto>>.Ok(rows, PaginationInfo.Create(query.Page, query.PageSize, total));
    }

    public async Task<AppResponse<int>> MarkReadAsync(int notificationId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return AppResponse<int>.Unauthorized("User identity is invalid");
        if (notificationId <= 0) return AppResponse<int>.ValidationError("Invalid notification id");
        var row = await _uow.UserNotifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId.Value, asNoTracking: false, ct: ct);
        if (row is null) return AppResponse<int>.NotFound("Notification not found");
        if (!row.IsRead) { row.IsRead = true; row.ReadAt = DateTime.UtcNow; _uow.UserNotifications.Update(row); await _uow.CompleteAsync(ct); }
        return AppResponse<int>.Ok(row.Id, "Notification marked as read");
    }

    public async Task<AppResponse<int>> MarkAllReadAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return AppResponse<int>.Unauthorized("User identity is invalid");
        var rows = await _uow.UserNotifications.FindAsync(n => n.UserId == userId.Value && !n.IsRead, asNoTracking: false, ct: ct);
        var now = DateTime.UtcNow;
        foreach (var row in rows) { row.IsRead = true; row.ReadAt = now; _uow.UserNotifications.Update(row); }
        if (rows.Count > 0) await _uow.CompleteAsync(ct);
        return AppResponse<int>.Ok(rows.Count, "Notifications marked as read");
    }

    public async Task<AppResponse<NotificationPreferencesDto>> GetPreferencesAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return AppResponse<NotificationPreferencesDto>.Unauthorized("User identity is invalid");
        var row = await _uow.NotificationPreferences.GetByIdAsync(userId.Value, ct);
        return AppResponse<NotificationPreferencesDto>.Ok(MapPreferences(row));
    }

    public async Task<AppResponse<NotificationPreferencesDto>> UpdatePreferencesAsync(NotificationPreferencesDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return AppResponse<NotificationPreferencesDto>.Unauthorized("User identity is invalid");
        if (dto is null) return AppResponse<NotificationPreferencesDto>.ValidationError("Request body is required");
        var row = await _uow.NotificationPreferences.GetByIdAsync(userId.Value, ct);
        if (row is null) { row = new CustomerNotificationPreference { UserId = userId.Value }; await _uow.NotificationPreferences.AddAsync(row, ct); }
        row.OrderUpdatesEnabled = dto.OrderUpdatesEnabled; row.PrescriptionUpdatesEnabled = dto.PrescriptionUpdatesEnabled;
        row.PointsUpdatesEnabled = dto.PointsUpdatesEnabled; row.EmailEnabled = dto.EmailEnabled; row.UpdatedAt = DateTime.UtcNow;
        _uow.NotificationPreferences.Update(row);
        await _uow.CompleteAsync(ct);
        return AppResponse<NotificationPreferencesDto>.Ok(MapPreferences(row), "Notification preferences updated");
    }

    public async Task NotifyCustomerAsync(int customerId, string type, string title, string message, string? relatedEntityType, long? relatedEntityId, CancellationToken ct)
    {
        try
        {
            var customer = await _uow.Customers.GetByIdAsync(customerId, ct);
            if (customer?.UserId is not int userId) return;
            var preferences = await _uow.NotificationPreferences.GetByIdAsync(userId, ct);
            var allowed = type switch
            {
                "Order" => preferences?.OrderUpdatesEnabled ?? true,
                "Prescription" => preferences?.PrescriptionUpdatesEnabled ?? true,
                "Points" => preferences?.PointsUpdatesEnabled ?? true,
                _ => false
            };
            if (!allowed) return;
            await _uow.UserNotifications.AddAsync(new UserNotification
            {
                UserId = userId, Type = type, Title = title, Message = message,
                RelatedEntityType = relatedEntityType, RelatedEntityId = relatedEntityId, CreatedAt = DateTime.UtcNow
            }, ct);
            await _uow.CompleteAsync(ct);
            if (preferences?.EmailEnabled == false) return;
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (!string.IsNullOrWhiteSpace(user?.Email))
            {
                try { await _emailSender.SendEmailAsync(user.Email, title, $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>", ct); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not send notification email to user {UserId}", userId); }
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not persist notification for customer {CustomerId}", customerId); }
    }

    private int? GetUserId() => int.TryParse(_currentUser.UserId, out var id) && id > 0 ? id : null;
    private static NotificationPreferencesDto MapPreferences(CustomerNotificationPreference? row) => new()
    {
        OrderUpdatesEnabled = row?.OrderUpdatesEnabled ?? true,
        PrescriptionUpdatesEnabled = row?.PrescriptionUpdatesEnabled ?? true,
        PointsUpdatesEnabled = row?.PointsUpdatesEnabled ?? true,
        EmailEnabled = row?.EmailEnabled ?? true
    };
}
