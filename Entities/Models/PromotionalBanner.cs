namespace Entities.Models;

public partial class PromotionalBanner
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public byte Placement { get; set; }
    public byte ActionType { get; set; }
    public int? TargetId { get; set; }
    public int? ParentTargetId { get; set; }
    public string? ActionUrl { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
