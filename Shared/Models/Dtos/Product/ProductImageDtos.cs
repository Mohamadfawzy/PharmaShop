namespace Shared.Models.Dtos.Product;
public sealed record ProductImageDto(long Id, int ProductId, string ImageUrl, string? ThumbnailUrl, string? AltText, int SortOrder, bool IsPrimary);
public sealed record ProductImageUpdateDto(string? AltText, int? SortOrder, bool? IsPrimary);
public sealed record ProductTagsUpdateDto(List<int> TagIds);
