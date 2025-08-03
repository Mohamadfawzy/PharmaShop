using Shared.Enums;

namespace Shared.Responses;

public class AppResponse<T>: AppResponseBase
{
    public T? Data { get; set; }

    public static AppResponse<T> Success(T data)
    {
        return new AppResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = 200 
        };
    }

    public static AppResponse<T> Success()
    {
        return new AppResponse<T>
        {
            IsSuccess = true,
            StatusCode = 200
        };
    }

    public static AppResponse<T> Fail(string errorMessage, AppErrorCode? errorCode = null, int? statusCode = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode ?? 400,
            ErrorCode = errorCode,
            Title = "Operation Failed",
            Detail = errorMessage,
            Errors = new List<string> { errorMessage }
        };
    }

    public static AppResponse<T> Fail(IEnumerable<string> errors, AppErrorCode? errorCode = null, int? statusCode = null)
    {
        return new AppResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode ?? 400,
            ErrorCode = errorCode,
            Title = "Operation Failed",
            Detail = "Multiple validation errors occurred.",
            Errors = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
        };
    }
}

public class AppResponse : AppResponseBase
{
    public AppResponse()
    {
    }

    public AppResponse(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public static AppResponse Success(string? title = null, string? detail = null)
    {
        return new AppResponse
        {
            IsSuccess = true,
            Title = title,
            Detail = detail,
            StatusCode = 200
        };
    }

    public static AppResponse Fail(string error, AppErrorCode? errorCode = null, int? statusCode = 400, string? title = null, string? detail = null)
    {
        return new AppResponse
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            Errors = new List<string> { error },
            StatusCode = statusCode,
            Title = title,
            Detail = detail
        };
    }

    public static AppResponse Fail(IEnumerable<string> errors, AppErrorCode? errorCode = null, int? statusCode = 400, string? title = null, string? detail = null)
    {
        return new AppResponse
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            Errors = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList(),
            StatusCode = statusCode,
            Title = title,
            Detail = detail
        };
    }
}
