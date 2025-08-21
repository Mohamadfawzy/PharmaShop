using System.Net;
using Shared.Constants;
using Shared.Enums;

namespace Shared.Responses;
public abstract class AppResponseBase
{
    public bool IsSuccess { get; protected set; }
    public List<string> Errors { get;  set; } = new();

    // RFC 7807 Problem Details Properties
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int StatusCode { get; set; } = ResponseDefaults.SuccessStatusCode;
    public string? Detail { get; set; }
    public string? Instance { get; set; }

    // Additional Properties
    public AppErrorCode ErrorCode { get; set; } = AppErrorCode.None;
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    protected AppResponseBase()
    {
        TraceId = GenerateTraceId();
    }

    // ===== ERROR MANAGEMENT =====

    public void AddError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return;

        Errors.Add(error);
        MarkAsFailure();
    }

    public void AddErrors(IEnumerable<string> errors)
    {
        if (errors == null)
            return;

        var validErrors = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        if (validErrors.Any())
        {
            Errors.AddRange(validErrors);
            MarkAsFailure();
        }
    }

    public void ClearErrors()
    {
        Errors.Clear();
        if (!HasErrors())
        {
            MarkAsSuccess();
        }
    }

    public virtual void MarkAsSuccess()
    {
        IsSuccess = true;
        if (StatusCode >= 400)
            StatusCode = ResponseDefaults.SuccessStatusCode;
    }

    public virtual void MarkAsFailure()
    {
        IsSuccess = false;
        if (StatusCode < 400)
            StatusCode = ResponseDefaults.BadRequestStatusCode;
    }

    // ===== UTILITY METHODS =====

    public string GetFirstError() => Errors?.FirstOrDefault() ?? string.Empty;

    public string GetAllErrorsAsString(string separator = "; ") =>
        Errors?.Count > 0 ? string.Join(separator, Errors) : string.Empty;

    public bool HasErrorCode(AppErrorCode errorCode) => ErrorCode == errorCode;
    public bool HasErrors() => Errors.Count > 0;

    public bool IsHttpSuccess() => StatusCode >= 200 && StatusCode < 300;

    public HttpStatusCode GetHttpStatusCode() => (HttpStatusCode)StatusCode;

    private static string GenerateTraceId() => Guid.NewGuid().ToString("N")[..8];
}
