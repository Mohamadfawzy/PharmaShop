using Shared.Enums;

namespace Shared.Models.Dtos.Banners;

public sealed class BannerListQueryDto
{
    public string? Q { get; set; }
    public BannerPlacement? Placement { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
