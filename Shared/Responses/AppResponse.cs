using Shared.Constants;
using Shared.Enums;

namespace Shared.Responses;


public sealed class AppResponse : AppResponseBase
{
    private AppResponse() { }

    /// <summary>
    /// Resource location (for Created responses). Not RFC7807 Instance.
    /// </summary>

    // ---------- Success ----------
    public static AppResponse Ok(string? title = null, string? detail = null)
    {
        var r = new AppResponse();
        r.SetSuccess(ResponseDefaults.SuccessStatusCode, title ?? ResponseDefaults.DefaultSuccessTitle, detail);
        return r;
    }

    public static AppResponse NoContent(string? title = null)
    {
        var r = new AppResponse();
        r.SetSuccess(ResponseDefaults.NoContentStatusCode, title ?? "Operation Completed Successfully", null);
        return r;
    }

    public static AppResponse Created(string? location = null, string? title = null, string? detail = null)
    {
        var r = new AppResponse();
        r.SetSuccess(ResponseDefaults.CreatedStatusCode, title ?? "Resource Created Successfully", detail);
        r.Location = location;
        return r;
    }

    // ---------- Failure ----------
    public static AppResponse Fail(
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

        var r = new AppResponse();
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

    public static AppResponse Fail(
        string error,
        AppErrorCode errorCode = AppErrorCode.None,
        int? statusCode = null,
        string? title = null,
        string? detail = null,
        string? type = null,
        string? instance = null)
        => Fail(new[] { error }, errorCode, statusCode, title, detail, type,instance);

    // ---------- Common failures ----------
    public static AppResponse NotFound(string? message = null) =>
        Fail(message ?? "Resource not found", AppErrorCode.NotFound, ResponseDefaults.NotFoundStatusCode);

    public static AppResponse Unauthorized(string? message = null) =>
        Fail(message ?? "Access denied", AppErrorCode.Unauthorized, ResponseDefaults.UnauthorizedStatusCode);

    public static AppResponse Forbidden(string? message = null) =>
        Fail(message ?? "Access forbidden", AppErrorCode.Forbidden, ResponseDefaults.ForbiddenStatusCode);

    public static AppResponse Conflict(string? message = null) =>
        Fail(message ?? "Resource conflict detected", AppErrorCode.Conflict, ResponseDefaults.ConflictStatusCode);

    public static AppResponse InternalError(string? message = null) =>
        Fail(message ?? "An internal error occurred", AppErrorCode.InternalServerError, ResponseDefaults.InternalServerErrorStatusCode);

    // ---------- Validation ----------
    public static AppResponse ValidationError(string message) =>
        Fail(message, AppErrorCode.ValidationError, ResponseDefaults.BadRequestStatusCode);

    public static AppResponse ValidationErrors(Dictionary<string, string[]> fieldErrors, string? detail = null)
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
}
