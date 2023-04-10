using System.Text.Json;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;

namespace TaxBeacon.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ConflictException e => new CustomProblemDetails()
            {
                Status = StatusCodes.Status409Conflict,
                Title = e.Message,
                ErrorCode = (int)e.Key,
            },
            _ => new CustomProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error occurred"
            }
        };

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = problemDetails.Status!.Value;
        await response.WriteAsync(JsonSerializer.Serialize(problemDetails));

        _logger.LogError(exception, "Unhandled exception");
    }
}
