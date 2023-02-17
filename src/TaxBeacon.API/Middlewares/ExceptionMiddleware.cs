using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TaxBeacon.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

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
                Title = "Something went wrong"
            }
        };

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = problemDetails.Status!.Value;
        await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
