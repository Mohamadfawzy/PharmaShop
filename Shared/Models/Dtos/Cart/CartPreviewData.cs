namespace Shared.Models.Dtos.Cart;

public sealed class CartPreviewData
{
    public int CartId { get; set; }
    public byte Status { get; set; } // 1=Active
    public List<CartPreviewItemData> Items { get; set; } = new();
}
