using Shared.Enums;

namespace Shared.Responses;

public abstract class AppResponseBase
{
    public bool IsSuccess { get; set; }
    public bool HasErrors => Errors?.Count > 0;
    public List<string> Errors { get; set; } = new();
    public AppErrorCode? ErrorCode { get; set; }


    // Optional details (aligned with RFC 7807 Problem Details format)
    public string? Type { get; set; }        // A URI reference for the error type
    public string? Title { get; set; }       // A short summary of the error
    public int? StatusCode { get; set; }         // HTTP status code
    public string? Detail { get; set; }      // Detailed error message for developers
    public string? Instance { get; set; }    // URI reference that identifies the problem occurrence

    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
            Errors.Add(error);
    }

    public void AddErrors(IEnumerable<string> errors)
    {
        Errors.AddRange(errors.Where(e => !string.IsNullOrWhiteSpace(e)));
    }
}