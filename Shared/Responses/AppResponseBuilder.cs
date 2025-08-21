using Shared.Constants;
using Shared.Enums;

namespace Shared.Responses;
public class AppResponseBuilder<T>
{
    private readonly AppResponse<T> _response = new();

    public AppResponseBuilder<T> WithSuccess()
    {
        _response.MarkAsSuccess();
        _response.StatusCode = ResponseDefaults.SuccessStatusCode;
        return this;
    }

    public AppResponseBuilder<T> WithFailure()
    {
        _response.MarkAsSuccess();
        _response.StatusCode = ResponseDefaults.BadRequestStatusCode;
        return this;
    }

    public AppResponseBuilder<T> WithData(T data)
    {
        _response.Data = data;
        return this;
    }

    public AppResponseBuilder<T> WithError(string error)
    {
        _response.AddError(error);
        return this;
    }

    public AppResponseBuilder<T> WithErrors(IEnumerable<string> errors)
    {
        _response.AddErrors(errors);
        return this;
    }

    public AppResponseBuilder<T> WithStatusCode(int statusCode)
    {
        _response.StatusCode = statusCode;
        return this;
    }

    public AppResponseBuilder<T> WithErrorCode(AppErrorCode errorCode)
    {
        _response.ErrorCode = errorCode;
        return this;
    }

    public AppResponseBuilder<T> WithTitle(string title)
    {
        _response.Title = title;
        return this;
    }

    public AppResponseBuilder<T> WithDetail(string detail)
    {
        _response.Detail = detail;
        return this;
    }

    public AppResponse<T> Build() => _response;

    public static implicit operator AppResponse<T>(AppResponseBuilder<T> builder) => builder.Build();
}