using Shared.Constants;
using Shared.Enums;
using Shared.Responses;
using System.Text.Json;

namespace WebAPI.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // العميل قفل الاتصال/ألغى الطلب
            // أفضل ممارسة: لا ترجع InternalError هنا
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // قد لا يكون معرّف في constants
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        // Log with TraceId and key info
        _logger.LogError(
            ex,
            "Unhandled exception. TraceId={TraceId}, Path={Path}, Method={Method}",
            traceId,
            context.Request.Path,
            context.Request.Method);

        // Map exception -> AppResponse
        var response = MapExceptionToAppResponse(ex, context);

        // Always attach trace id
        response.TraceId = traceId;

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = response.StatusCode;

        // Use camelCase in JSON
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private AppResponse MapExceptionToAppResponse(Exception ex, HttpContext context)
    {
        // instance: request path (helps correlate problem)
        var instance = context.Request.Path.ToString();

        // type: you can standardize these later to URIs
        // e.g. "https://httpstatuses.com/500"
        string type500 = "https://httpstatuses.com/500";

        // 🔹 You can expand this mapping later with custom exceptions
        // For now: default internal error, hide details in production.
        if (_env.IsDevelopment())
        {
            // في التطوير: ممكن تُظهر رسالة أكثر (لكن لا تضع StackTrace في response عادةً)
            return AppResponse.Fail(
                error: "An internal error occurred",
                errorCode: AppErrorCode.InternalServerError,
                statusCode: ResponseDefaults.InternalServerErrorStatusCode,
                title: "Internal Server Error",
                detail: ex.Message,
                type: type500,
                instance: instance
            );
        }

        // في production: رسالة عامة
        return AppResponse.Fail(
            error: "An internal error occurred",
            errorCode: AppErrorCode.InternalServerError,
            statusCode: ResponseDefaults.InternalServerErrorStatusCode,
            title: "Internal Server Error",
            detail: "Something went wrong. Please try again later.",
            type: type500,
            instance: instance
        );
    }
}
