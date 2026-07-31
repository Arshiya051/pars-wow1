using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ParsWoW.Api.Application.Common;

namespace ParsWoW.Api.Presentation.Filters;

/// <summary>
/// Wraps any unhandled exception from a controller into the standard
/// <see cref="ApiResponse{T}"/> envelope so clients never get the
/// internal stack trace or a generic ASP.NET Core error view.
/// </summary>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    /// <summary>
    /// Initialises the filter with an <see cref="ILogger{TCategoryName}"/> for structured error logging.
    /// </summary>
    /// <param name="logger">Logger scoped to <see cref="ApiExceptionFilter"/>.</param>
    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) => _logger = logger;

    /// <summary>
    /// Called when an unhandled exception propagates out of a controller action.
    /// Logs the error, replaces the result with a 500 <see cref="ApiResponse{T}"/>,
    /// and marks the exception as handled.
    /// </summary>
    /// <param name="ctx">The <see cref="ExceptionContext"/> provided by the ASP.NET Core pipeline.</param>
    public void OnException(ExceptionContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        _logger.LogError(ctx.Exception, "Unhandled exception in {Path}", ctx.HttpContext.Request.Path);
        var resp = ApiResponse<object>.Fail(500, "INTERNAL_ERROR", "An unexpected error occurred.");
        ctx.Result = new ObjectResult(resp) { StatusCode = 500 };
        ctx.ExceptionHandled = true;
    }
}
