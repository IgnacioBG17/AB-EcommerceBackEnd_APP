using Ecommerce.Api.Errors;
using Ecommerce.Application.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Ecommerce.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                var statusCode = (int)HttpStatusCode.InternalServerError;
                var result = string.Empty;

                switch (ex)
                {
                    case UnauthorizedException authEx:
                        statusCode = (int)HttpStatusCode.Unauthorized;
                        result = CreateJsonError(statusCode, new[] { authEx.Message }, authEx.StackTrace);
                        break;

                    case ForbiddenException forbEx:
                        statusCode = (int)HttpStatusCode.Forbidden;
                        result = CreateJsonError(statusCode, new[] { forbEx.Message }, forbEx.StackTrace);
                        break;
                    case NotFoundException notFoundException:
                        statusCode = (int)HttpStatusCode.NotFound;
                        result = CreateJsonError(statusCode, new[] { notFoundException.Message }, notFoundException.StackTrace);
                        break;

                    case FluentValidation.ValidationException validationException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        var errors = validationException.Errors.Select(ers => ers.ErrorMessage).ToArray();
                        result = CreateJsonError(statusCode, errors, validationException.StackTrace);
                        break;

                    case BadRequestException badRequestException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        result = CreateJsonError(statusCode, new[] { badRequestException.Message }, badRequestException.StackTrace);
                        break;
                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        result = CreateJsonError(statusCode, new[] { "Ocurrió un error inesperado en el servidor. Inténtelo más tarde." }, ex.StackTrace);
                        break;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = CreateJsonError(statusCode, new string[] { ex.Message }, ex.StackTrace!);
                }

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(result);
            }
        }

        private string CreateJsonError(int statusCode, string[] messages, string? stackTrace)
        {
#if DEBUG
            var details = stackTrace;
#else
            var details = null;
#endif

            return JsonConvert.SerializeObject(new CodeErrorException(
                statusCode,
                messages,
                details!
            ));
        }
    }
}
