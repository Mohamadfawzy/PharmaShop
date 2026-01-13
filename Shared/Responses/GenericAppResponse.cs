using Shared.Constants;
using Shared.Enums;

namespace Shared.Responses;


public sealed class AppResponse<T> : AppResponseBase
{
    private AppResponse() { }

    public T? Data { get; private set; }
    public PaginationInfo? Pagination { get;  set; }

    /// <summary>
    /// Resource location (for Created responses). Not RFC7807 Instance.
    /// </summary>
   

    // ---------- Success ----------
    public static AppResponse<T> Ok(T data, string? title = null, string? detail = null)
    {
        var r = new AppResponse<T>();
        r.SetSuccess(ResponseDefaults.SuccessStatusCode, title ?? ResponseDefaults.DefaultSuccessTitle, detail);
        r.Data = data;
        return r;
    }

    public static AppResponse<T> Ok(T data, PaginationInfo pagination, string? title = null, string? detail = null)
    {
        var r = Ok(data, title, detail);
        r.Pagination = pagination;
        return r;
    }

    public static AppResponse<T> Created(T data, string? location = null, string? title = null, string? detail = null)
    {
        var r = new AppResponse<T>();
        r.SetSuccess(ResponseDefaults.CreatedStatusCode, title ?? "Resource Created Successfully", detail);
        r.Data = data;
        r.Location = location;
        return r;
    }

    // ---------- Failure ----------
    public static AppResponse<T> Fail(
        IEnumerable<string> errors,
        AppErrorCode errorCode = AppErrorCode.None,
        int? statusCode = null,
        string? title = null,
        string? detail = null,
        string? type = null,
        string? instance = null,
        IReadOnlyDictionary<string, string[]>? fieldErrors = null)
    {
        var errorList = (errors ?? Array.Empty<string>())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        var code = statusCode ?? ResponseDefaults.GetDefaultStatusCode(errorCode);
        var resolvedTitle = title ?? ResponseDefaults.GetDefaultTitle(errorCode);

        var resolvedDetail = detail;
        if (string.IsNullOrWhiteSpace(resolvedDetail))
        {
            resolvedDetail = errorList.Count switch
            {
                0 => "An error occurred",
                1 => errorList[0],
                _ => $"{errorList.Count} errors occurred"
            };
        }

        var r = new AppResponse<T>();
        r.SetFailure(
            statusCode: code,
            errorCode: errorCode,
            title: resolvedTitle,
            detail: resolvedDetail,
            errors: errorList,
            type: type,
            instance: instance,
            fieldErrors: fieldErrors
        );

        return r;
    }

    public static AppResponse<T> Fail(
        string error,
        AppErrorCode errorCode = AppErrorCode.None,
        int? statusCode = null,
        string? title = null,
        string? detail = null)
        => Fail(new[] { error }, errorCode, statusCode, title, detail);

    // ---------- Common failures ----------
    public static AppResponse<T> NotFound(string? message = null) =>
        Fail(message ?? "Resource not found", AppErrorCode.NotFound, ResponseDefaults.NotFoundStatusCode);

    public static AppResponse<T> Unauthorized(string? message = null) =>
        Fail(message ?? "Access denied", AppErrorCode.Unauthorized, ResponseDefaults.UnauthorizedStatusCode);

    public static AppResponse<T> Forbidden(string? message = null) =>
        Fail(message ?? "Access forbidden", AppErrorCode.Forbidden, ResponseDefaults.ForbiddenStatusCode);

    public static AppResponse<T> Conflict(string? message = null) =>
        Fail(message ?? "Resource conflict detected", AppErrorCode.Conflict, ResponseDefaults.ConflictStatusCode);

    public static AppResponse<T> InternalError(string? message = null) =>
        Fail(message ?? "An internal error occurred", AppErrorCode.InternalServerError, ResponseDefaults.InternalServerErrorStatusCode);

    // ---------- Validation ----------
    public static AppResponse<T> ValidationError(string message) =>
    Fail(message, AppErrorCode.ValidationError, ResponseDefaults.BadRequestStatusCode);

    public static AppResponse<T> ValidationErrors(Dictionary<string, string[]> fieldErrors, string? detail = null)
    {
        var flat = fieldErrors
            .SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))
            .ToList();

        return Fail(
            errors: flat,
            errorCode: AppErrorCode.ValidationError,
            statusCode: ResponseDefaults.BadRequestStatusCode,
            title: ResponseDefaults.GetDefaultTitle(AppErrorCode.ValidationError),
            detail: detail ?? "Validation failed",
            fieldErrors: fieldErrors
        );
    }
   
    


    public static AppResponse<T> BusinessRuleViolation(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            message = "Business rule violation";

        return new AppResponse<T>
        {
            IsSuccess = false,
            StatusCode = ResponseDefaults.BadRequestStatusCode,
            ErrorCode = AppErrorCode.BusinessRuleViolation,
            Title = ResponseDefaults.GetDefaultTitle(AppErrorCode.BusinessRuleViolation),
            Detail = message,
            //Errors = new List<string> { message }
        };
    }

}