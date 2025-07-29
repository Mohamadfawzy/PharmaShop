namespace Shared.Responses;

public abstract class AppResponseBase
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ApiError>? Errors { get; set; }
    public List<ApiError>? Error { get; set; }
}