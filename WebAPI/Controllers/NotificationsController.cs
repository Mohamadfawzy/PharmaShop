using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Notifications;

namespace WebAPI.Controllers;

[Route("api/v1/notifications")]
[Authorize(Policy = AppPolicies.Customer)]
public sealed class NotificationsController : BaseApiController
{
    private readonly INotificationService _service;
    public NotificationsController(INotificationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] NotificationQueryDto query, CancellationToken ct)
        => FromAppResponse(await _service.GetMineAsync(query, ct));
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        => FromAppResponse(await _service.MarkReadAsync(id, ct));
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        => FromAppResponse(await _service.MarkAllReadAsync(ct));
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
        => FromAppResponse(await _service.GetPreferencesAsync(ct));
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] NotificationPreferencesDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdatePreferencesAsync(dto, ct));
}
