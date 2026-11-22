using Shared.Enums;

namespace Shared.Models.Dtos.Banners;

public sealed class BannerUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public BannerPlacement Placement { get; set; }
    public BannerActionType ActionType { get; set; }
    public int? TargetId { get; set; }
    public int? ParentTargetId { get; set; }
    public string? ActionUrl { get; set; }
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
