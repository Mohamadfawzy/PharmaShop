using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Responses;

namespace WebAPI.Filters;

public sealed class TraceIdResultFilter : IAsyncResultFilter
{
    private const string TraceHeaderName = "X-Trace-Id";

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var traceId = context.HttpContext.TraceIdentifier;

        // 1) Always add header
        if (!context.HttpContext.Response.Headers.ContainsKey(TraceHeaderName))
            context.HttpContext.Response.Headers[TraceHeaderName] = traceId;

        // 2) If the action returned AppResponseBase, fill TraceId if missing
        if (context.Result is ObjectResult objectResult && objectResult.Value is AppResponseBase appResponse)
        {
            if (string.IsNullOrWhiteSpace(appResponse.TraceId))
                appResponse.TraceId = traceId;
        }

        await next();
    }
}
