using Shared.Constants;
using Shared.Enums;

namespace Shared.Responses;


public class AppResponse : AppResponseBase
{
    private AppResponse() { }

    private AppResponse(bool isSuccess)
    {
        IsSuccess = isSuccess;
        StatusCode = isSuccess ? ResponseDefaults.SuccessStatusCode : ResponseDefaults.BadRequestStatusCode;
    }

    // ===== SUCCESS FACTORY METHODS =====

    /// <summary>
    /// Creates a successful response with an optional title and detail message.
    /// </summary>

    public static AppResponse Success(string? title = null, string? detail = null)
    {
        return new AppResponse
        {
            IsSuccess = true,
            StatusCode = ResponseDefaults.SuccessStatusCode,
            Title = title ?? ResponseDefaults.DefaultSuccessTitle,
            Detail = detail
        };
    }

    public static AppResponse Created(string? location = null)
    {
        return new AppResponse
        {
            IsSuccess = true,
            StatusCode = ResponseDefaults.CreatedStatusCode,
            Title = "Resource Created Successfully",
            Instance = location
        };
    }

    public static AppResponse NoContent()
    {
        return new AppResponse
        {
            IsSuccess = true,
            StatusCode = ResponseDefaults.NoContentStatusCode,
            Title = "Operation Completed Successfully"
        };
    }

    // ===== FAILURE FACTORY METHODS =====

    public static AppResponse Fail(string error, AppErrorCode errorCode = AppErrorCode.None, int? statusCode = null, string? title = null, string? detail = null)
    {
        return new AppResponse
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            StatusCode = statusCode ?? ResponseDefaults.GetDefaultStatusCode(errorCode),
            Title = title ?? ResponseDefaults.GetDefaultTitle(errorCode),
            Detail = detail ?? error,
            Errors = [error]
        };
    }

    public static AppResponse Fail(IEnumerable<string> errors, AppErrorCode errorCode = AppErrorCode.None, int? statusCode = null, string? title = null, string? detail = null)
    {
        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>();
        return new AppResponse
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            StatusCode = statusCode ?? ResponseDefaults.GetDefaultStatusCode(errorCode),
            Title = title ?? ResponseDefaults.GetDefaultTitle(errorCode),
            Detail = detail ?? (errorList.Count == 1 ? errorList.First() : $"{errorList.Count} errors occurred"),
            Errors = errorList
        };
    }

    // Specific failure methods (same as generic version)
    public static AppResponse NotFound(string? message = null) =>
        Fail(message ?? "Resource not found", AppErrorCode.NotFound);

    public static AppResponse Unauthorized(string? message = null) =>
        Fail(message ?? "Access denied", AppErrorCode.Unauthorized, ResponseDefaults.UnauthorizedStatusCode);

    public static AppResponse Forbidden(string? message = null) =>
        Fail(message ?? "Access forbidden", AppErrorCode.Forbidden, ResponseDefaults.ForbiddenStatusCode);

    public static AppResponse ValidationError(string error) =>
        Fail(error, AppErrorCode.ValidationError, ResponseDefaults.BadRequestStatusCode);

    public static AppResponse ValidationErrors(IEnumerable<string> errors) =>
        Fail(errors, AppErrorCode.ValidationError, ResponseDefaults.BadRequestStatusCode);

    public static AppResponse BusinessRuleViolation(string error) =>
        Fail(error, AppErrorCode.BusinessRuleViolation, ResponseDefaults.BadRequestStatusCode);

    public static AppResponse Conflict(string? message = null) =>
        Fail(message ?? "Resource conflict detected", AppErrorCode.Conflict, ResponseDefaults.ConflictStatusCode);

    public static AppResponse InternalError(string? message = null) =>
        Fail(message ?? "An internal error occurred", AppErrorCode.InternalServerError, ResponseDefaults.InternalServerErrorStatusCode);

}

