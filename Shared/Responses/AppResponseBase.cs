using Shared.Constants;
using Shared.Enums;
using System.Net;
using System.Text.Json.Serialization;

namespace Shared.Responses;

public abstract class AppResponseBase
{
    public bool IsSuccess { get; protected set; }

    // RFC7807-like fields
    public string? Title { get; protected set; }
    public string? Type { get;  set; }
    public int StatusCode { get; protected set; } = ResponseDefaults.SuccessStatusCode;
    public string? Detail { get; protected set; }
    public string? Instance { get;  set; } // instance for the problem (not a resource location)

    public string? Location { get;  set; } = null;
    // Extra metadata
    public AppErrorCode ErrorCode { get; protected set; } = AppErrorCode.None;

    /// <summary>
    /// Set this from an ActionFilter / middleware using HttpContext.TraceIdentifier or Activity TraceId.
    /// </summary>
    public string? TraceId { get; set; }

    public DateTime Timestamp { get; } = DateTime.UtcNow;

    // General errors (not field-level) - read-only externally
    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors;

    // Field-level errors (validation) - optional
    public IReadOnlyDictionary<string, string[]>? FieldErrors { get; protected set; }

    [JsonIgnore] public bool HasErrors => _errors.Count > 0;
    [JsonIgnore] public bool IsHttpSuccess => StatusCode is >= 200 and < 300;
    [JsonIgnore] public HttpStatusCode HttpStatusCode => (HttpStatusCode)StatusCode;

    // ------- Protected helpers -------
    protected void SetSuccess(int statusCode, string? title, string? detail)
    {
        IsSuccess = true;
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
        ErrorCode = AppErrorCode.None;
    }

    protected void SetFailure(
        int statusCode,
        AppErrorCode errorCode,
        string? title,
        string? detail,
        IEnumerable<string>? errors,
        string? type = null,
        string? instance = null,
        IReadOnlyDictionary<string, string[]>? fieldErrors = null)
    {
        IsSuccess = false;
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Title = title;
        Detail = detail;
        Type = type;
        Instance = instance;
        FieldErrors = fieldErrors;

        _errors.Clear();
        if (errors is null) return;

        foreach (var e in errors)
        {
            if (!string.IsNullOrWhiteSpace(e))
                _errors.Add(e);
        }
    }
}