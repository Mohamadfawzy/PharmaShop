using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Delivery;

public sealed class DeliveryZoneAreaItemDto
{
    public int GovernorateAreaId { get; set; }
    public int GovernorateId { get; set; }
    public string GovernorateNameAr { get; set; } = string.Empty;
    public string AreaNameAr { get; set; } = string.Empty;
    public string AreaNameEn { get; set; } = string.Empty;
}

public sealed class DeliveryZoneDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal DeliveryFee { get; set; }
    public decimal? FreeDeliveryMinimumAmount { get; set; }
    public bool IsActive { get; set; }
    public List<DeliveryZoneAreaItemDto> Areas { get; set; } = new();
}

public sealed class DeliveryZoneWriteDto
{
    [Required, MaxLength(150)] public string NameAr { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string NameEn { get; set; } = string.Empty;
    [Range(typeof(decimal), "0", "10000000")] public decimal DeliveryFee { get; set; }
    [Range(typeof(decimal), "0", "10000000")] public decimal? FreeDeliveryMinimumAmount { get; set; }
    [Required, MinLength(1)] public List<int> GovernorateAreaIds { get; set; } = new();
}

public sealed class DeliveryZoneActiveDto
{
    public bool IsActive { get; set; }
}
