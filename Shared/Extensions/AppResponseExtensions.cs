using Shared.Enums;
using Shared.Responses;

namespace Shared.Extensions;
public static class AppResponseExtensions
{
    public static AppResponse<T> FromError<T>(this AppResponseBase source)
    {
        // Copy all error metadata to a typed response
        var fieldErrors = source.FieldErrors is null
            ? null
            : new Dictionary<string, string[]>(source.FieldErrors);

        var r = AppResponse<T>.Fail(
            errors: source.Errors,
            errorCode: source.ErrorCode,
            statusCode: source.StatusCode,
            title: source.Title,
            detail: source.Detail,
            type: source.Type,
            instance: source.Instance,
            fieldErrors: fieldErrors
        );

        r.TraceId = source.TraceId;
        return r;
    }

    public static AppResponse ToNonGeneric(this AppResponseBase source)
    {
        if (source.IsSuccess)
        {
            var ok = AppResponse.Ok(source.Title, source.Detail);
            ok.TraceId = source.TraceId;
            return ok;
        }

        var fieldErrors = source.FieldErrors is null
            ? null
            : new Dictionary<string, string[]>(source.FieldErrors);

        var fail = AppResponse.Fail(
            errors: source.Errors,
            errorCode: source.ErrorCode,
            statusCode: source.StatusCode,
            title: source.Title,
            detail: source.Detail,
            type: source.Type,
            instance: source.Instance,
            fieldErrors: fieldErrors
        );

        fail.TraceId = source.TraceId;
        return fail;
    }


    public static AppResponse<TResult> Map<T, TResult>(
        this AppResponse<T> source,
        Func<T, TResult> mapper,
        string? successTitle = null,
        string? successDetail = null)
    {
        if (!source.IsSuccess)
            return source.FromError<TResult>();

        if (source.Data is null)
            return AppResponse<TResult>.InternalError("Mapping failed: Data is null");

        try
        {
            var mapped = mapper(source.Data);
            var r = AppResponse<TResult>.Ok(mapped, successTitle ?? source.Title, successDetail ?? source.Detail);
            r.TraceId = source.TraceId;
            return r;
        }
        catch (Exception ex)
        {
            return AppResponse<TResult>.InternalError($"Mapping failed: {ex.Message}");
        }
    }

    public static AppResponse<List<T>> WithPagination<T>(
        this AppResponse<List<T>> response,
        PaginationInfo pagination)
    {
        // لو response فاشلة، ما نعدلهاش
        if (!response.IsSuccess) return response;

        // بما أن Pagination private set، نحتاج طريقة "داخلية" لضبطها
        // الحل: إما تجعل setter internal، أو تضيف method داخلي داخل AppResponse<T>
        throw new NotSupportedException("Enable internal setter for Pagination or add internal method on AppResponse<T>.");
    }

}