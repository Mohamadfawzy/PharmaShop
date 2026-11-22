namespace Shared.Models.Dtos.Banners;

public sealed class BannerReorderDto
{
    public List<BannerOrderItemDto> Items { get; set; } = [];
}

public sealed class BannerOrderItemDto
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
}
