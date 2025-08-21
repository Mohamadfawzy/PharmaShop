namespace Shared.Responses;

public class PagedAppResponse<T> : AppResponse<T>
{
    public required PaginationInfo Pagination { get; set; }
}
