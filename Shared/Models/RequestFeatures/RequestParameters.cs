namespace Shared.Models.RequestFeatures;

public class RequestParameters
{
    const int maxPageSize = 50;
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
    public bool? IsActive { get; set; } = null;
    public bool? IsDeleted { get; set; } = null;

    // Sorting
    public bool OrderDescending { get; set; } = true;

    //public string? Fields { get; set; }
}
