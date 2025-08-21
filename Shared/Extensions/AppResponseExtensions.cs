using Shared.Enums;
using Shared.Responses;

namespace Shared.Extensions;
public static class AppResponseExtensions
{
    public static AppResponse<T> Ensure<T>(this AppResponse<T> response, Func<T, bool> predicate, string errorMessage)
    {
        if (!response.IsSuccess)
            return response;

        if (response.Data == null || !predicate(response.Data))
        {
            return AppResponse<T>.Fail(errorMessage, AppErrorCode.BusinessRuleViolation);
        }

        return response;
    }

    public static async Task<AppResponse<TResult>> Bind<T, TResult>(this AppResponse<T> response, Func<T, Task<AppResponse<TResult>>> func)
    {
        if (!response.IsSuccess)
        {
            return AppResponse<TResult>.Fail(response.Errors, response.ErrorCode, response.StatusCode);
        }

        return await func(response.Data!);
    }

    public static AppResponse<TResult> Bind<T, TResult>(this AppResponse<T> response, Func<T, AppResponse<TResult>> func)
    {
        if (!response.IsSuccess)
        {
            return AppResponse<TResult>.Fail(response.Errors, response.ErrorCode, response.StatusCode);
        }

        return func(response.Data!);
    }

    public static AppResponse<T> OnSuccess<T>(this AppResponse<T> response, Action<T> action)
    {
        if (response.IsSuccess && response.Data != null)
        {
            action(response.Data);
        }
        return response;
    }

    public static AppResponse<T> OnFailure<T>(this AppResponse<T> response, Action<AppResponse<T>> action)
    {
        if (!response.IsSuccess)
        {
            action(response);
        }
        return response;
    }

    public static TResult Match<T, TResult>(this AppResponse<T> response, Func<T, TResult> onSuccess, Func<AppResponse<T>, TResult> onFailure)
    {
        return response.IsSuccess ? onSuccess(response.Data!) : onFailure(response);
    }
}