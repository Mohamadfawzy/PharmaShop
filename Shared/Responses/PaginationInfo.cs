namespace Shared.Responses;
public class PaginationInfo
{
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    public int StartRecord { get; init; }
    public int EndRecord { get; init; }

    // 
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public int? PreviousPage => HasPrevious ? CurrentPage - 1 : null;
    public int? NextPage => HasNext ? CurrentPage + 1 : null;

    private PaginationInfo(int currentPage, int pageSize, int totalCount)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        StartRecord = totalCount == 0 ? 0 : (currentPage - 1) * pageSize + 1;
        EndRecord = Math.Min(currentPage * pageSize, totalCount);
    }

    public static PaginationInfo Create(int currentPage, int pageSize, int totalCount)
        => new(currentPage, pageSize, totalCount);

    public static PaginationInfo Empty(int currentPage = 1, int pageSize = 10)
        => new(currentPage, pageSize, 0);
}