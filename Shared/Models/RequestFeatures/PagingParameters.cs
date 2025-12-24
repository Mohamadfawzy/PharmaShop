using System.ComponentModel.DataAnnotations;

namespace Shared.Models.RequestFeatures;

public class PagingParameters
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 10)]
    public int PageSize { get; set; } = 10;
}