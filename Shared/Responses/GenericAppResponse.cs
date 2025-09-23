using Shared.Constants;
using Shared.Enums;

namespace Shared.Responses;


public class AppResponse<T> : AppResponseBase
{
    public PaginationInfo? Pagination { get; set; }
    public T? Data { get; set; }
    public AppResponse() { }

    // ===== SUCCESS FACTORY METHODS =====

    public static AppResponse<T> Success(T data, string? title = null, string? detail = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = ResponseDefaults.SuccessStatusCode,
            Title = title ?? ResponseDefaults.DefaultSuccessTitle,
            Detail = detail
        };
    }
    
    public static AppResponse<T> Success(T data, PaginationInfo paginationInfo, string? title = null, string? detail = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = ResponseDefaults.SuccessStatusCode,
            Title = title ?? ResponseDefaults.DefaultSuccessTitle,
            Detail = detail
        };
    }

    public static AppResponse<T> Success(T data, int statusCode, string? title = null, string? detail = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode,
            Title = title ?? ResponseDefaults.DefaultSuccessTitle,
            Detail = detail
        };
    }

    public static AppResponse<T> Created(T data, string? location = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = ResponseDefaults.CreatedStatusCode,
            Title = "Resource Created Successfully",
            Instance = location
        };
    }

    // ===== FAILURE FACTORY METHODS =====

    public static AppResponse<T> Fail(string error, AppErrorCode errorCode = AppErrorCode.None, int? statusCode = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode ?? ResponseDefaults.GetDefaultStatusCode(errorCode),
            ErrorCode = errorCode,
            Title = ResponseDefaults.GetDefaultTitle(errorCode),
            Detail = error,
            Errors = new List<string> { error }
        };
    }

    public static AppResponse<T> Fail(IEnumerable<string> errors, AppErrorCode errorCode = AppErrorCode.None, int? statusCode = null)
    {
        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>();
        return new AppResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode ?? ResponseDefaults.GetDefaultStatusCode(errorCode),
            ErrorCode = errorCode,
            Title = ResponseDefaults.GetDefaultTitle(errorCode),
            Detail = errorList.Count == 1 ? errorList.First() : $"{errorList.Count} errors occurred",
            Errors = errorList
        };
    }
    
    public static AppResponse<T> Fail(List<string>? errors, AppErrorCode errorCode = AppErrorCode.None, int? statusCode = null)
    {
        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>();
        return new AppResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode ?? ResponseDefaults.GetDefaultStatusCode(errorCode),
            ErrorCode = errorCode,
            Title = ResponseDefaults.GetDefaultTitle(errorCode),
            Detail = errorList.Count == 1 ? errorList.First() : $"{errorList.Count} errors occurred",
            Errors = errorList
        };
    }

    public static AppResponse<T> NotFound(string? message = null)
    {
        return Fail(
            message ?? "Resource not found",
            AppErrorCode.NotFound,
            ResponseDefaults.NotFoundStatusCode
        );
    }

    public static AppResponse<T> Unauthorized(string? message = null)
    {
        return Fail(
            message ?? "Access denied",
            AppErrorCode.Unauthorized,
            ResponseDefaults.UnauthorizedStatusCode
        );
    }

    public static AppResponse<T> Forbidden(string? message = null)
    {
        return Fail(
            message ?? "Access forbidden",
            AppErrorCode.Forbidden,
            ResponseDefaults.ForbiddenStatusCode
        );
    }

    public static AppResponse<T> ValidationError(string error)
    {
        return Fail(error, AppErrorCode.ValidationError, ResponseDefaults.BadRequestStatusCode);
    }

    public static AppResponse<T> ValidationErrors(IEnumerable<string> errors)
    {
        return Fail(errors, AppErrorCode.ValidationError, ResponseDefaults.BadRequestStatusCode);
    }

    public static AppResponse<T> BusinessRuleViolation(string error)
    {
        return Fail(error, AppErrorCode.BusinessRuleViolation, ResponseDefaults.BadRequestStatusCode);
    }

    public static AppResponse<T> Conflict(string? message = null)
    {
        return Fail(
            message ?? "Resource conflict detected",
            AppErrorCode.Conflict,
            ResponseDefaults.ConflictStatusCode
        );
    }

    public static AppResponse<T> InternalError(string? message = null)
    {
        return Fail(
            message ?? "An internal error occurred",
            AppErrorCode.InternalServerError,
            ResponseDefaults.InternalServerErrorStatusCode
        );
    }

    public static AppResponse<T> FromError<U>(AppResponse<U> source) =>
    new AppResponse<T>
    {
        IsSuccess = false,
        Title = source.Title,
        ErrorCode = source.ErrorCode
    };

    // ===== CONVERSION METHODS =====

    public AppResponse<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (!IsSuccess)
        {
            return new AppResponse<TResult>
            {
                IsSuccess = false,
                StatusCode = StatusCode,
                ErrorCode = ErrorCode,
                Title = Title,
                Detail = Detail,
                Type = Type,
                Instance = Instance,
                TraceId = TraceId,
                Errors = new List<string>(Errors)
            };
        }

        try
        {
            return AppResponse<TResult>.Success(mapper(Data!), Title, Detail);
        }
        catch (Exception ex)
        {
            return AppResponse<TResult>.InternalError($"Mapping failed: {ex.Message}");
        }
    }

    public AppResponse ToNonGeneric()
    {
        if (IsSuccess)
        {
            return AppResponse.Success(Title, Detail);
        }

        return AppResponse.Fail(Errors, ErrorCode, StatusCode, Title, Detail);
    }

}

