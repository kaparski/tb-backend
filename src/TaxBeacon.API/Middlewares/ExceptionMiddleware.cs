using Newtonsoft.Json;
using System.Net;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.Middlewares
{
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
            var errorResult = new ErrorResult
            {
                Name = exception.GetType().Name,
                Exception = exception.Message,
                TraceId = Guid.NewGuid().ToString(),
            };
            if (exception is not CustomException && exception.InnerException != null)
            {
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
            }

            switch (exception)
            {
                case CustomException e:
                    errorResult.StatusCode = (int)e.StatusCode;
                    if (e.ErrorMessages is not null)
                    {
                        errorResult.Messages = e.ErrorMessages;
                    }

                    break;

                default:
                    errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = errorResult.StatusCode;
            await response.WriteAsync(JsonConvert.SerializeObject(errorResult));
        }
    }
}
