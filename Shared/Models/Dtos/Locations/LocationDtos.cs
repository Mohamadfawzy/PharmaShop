using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Locations;

public sealed class GovernorateDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class GovernorateAreaDto
{
    public int Id { get; set; }
    public int GovernorateId { get; set; }
    public string GovernorateNameAr { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class GovernorateWriteDto
{
    [Required, MaxLength(100)] public string NameAr { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string NameEn { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int DisplayOrder { get; set; }
}

public sealed class GovernorateAreaWriteDto
{
    [Range(1, int.MaxValue)] public int GovernorateId { get; set; }
    [Required, MaxLength(100)] public string NameAr { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string NameEn { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int DisplayOrder { get; set; }
}

public sealed class LocationActiveDto
{
    public bool IsActive { get; set; }
}
