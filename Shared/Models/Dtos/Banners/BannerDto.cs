using Shared.Enums;

namespace Shared.Models.Dtos.Banners;

public sealed class BannerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BannerPlacement Placement { get; set; }
    public BannerActionType ActionType { get; set; }
    public int? TargetId { get; set; }
    public int? ParentTargetId { get; set; }
    public string? ActionUrl { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
