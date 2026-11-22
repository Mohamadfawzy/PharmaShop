using Microsoft.AspNetCore.Http;
using Shared.Enums;
using Shared.Models.Dtos.Banners;

namespace WebAPI.SpecificDtos;

public sealed class BannerAdminFormDto
{
    public string Name { get; set; } = string.Empty;
    public BannerPlacement Placement { get; set; }
    public BannerActionType ActionType { get; set; }
    public int? TargetId { get; set; }
    public int? ParentTargetId { get; set; }
    public string? ActionUrl { get; set; }
    public IFormFile? Image { get; set; }
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public BannerCreateDto ToCreateDto() => new()
    {
        Name = Name, Placement = Placement, ActionType = ActionType, TargetId = TargetId,
        ParentTargetId = ParentTargetId, ActionUrl = ActionUrl, AltText = AltText,
        DisplayOrder = DisplayOrder, IsActive = IsActive, StartsAt = StartsAt, EndsAt = EndsAt
    };

    public BannerUpdateDto ToUpdateDto() => new()
    {
        Name = Name, Placement = Placement, ActionType = ActionType, TargetId = TargetId,
        ParentTargetId = ParentTargetId, ActionUrl = ActionUrl, AltText = AltText,
        DisplayOrder = DisplayOrder, IsActive = IsActive, StartsAt = StartsAt, EndsAt = EndsAt
    };
}

public sealed class BannerSetActiveDto
{
    public bool IsActive { get; set; }
}
