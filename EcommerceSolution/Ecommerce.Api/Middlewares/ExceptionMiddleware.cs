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
                    case NotFoundException notFoundException:
                        statusCode = (int)HttpStatusCode.NotFound;
                        result = JsonConvert.SerializeObject(
                            new CodeErrorException(
                                statusCode,
                                new string[] { notFoundException.Message },
                                notFoundException.StackTrace!
                            ));
                        break;

                    case FluentValidation.ValidationException validationException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        var errors = validationException.Errors.Select(ers => ers.ErrorMessage).ToArray();
                        var validationJsons = JsonConvert.SerializeObject(errors);
                        result = JsonConvert.SerializeObject(
                            new CodeErrorException(statusCode, errors, validationJsons)
                        );
                        break;

                    case BadRequestException badRequestException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        #if DEBUG
                            var details = badRequestException.StackTrace;
                        #else
                            var details = null;
                        #endif
                        result = JsonConvert.SerializeObject(
                            new CodeErrorException(statusCode,
                            new string[] { badRequestException.Message },
                            details!));
                        break;
                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonConvert.SerializeObject(
                            new CodeErrorException(
                                statusCode,
                                new string[] { "Ocurrió un error inesperado en el servidor. Inténtelo más tarde." },
                                ex.StackTrace!
                            ));
                        break;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = JsonConvert.SerializeObject(
                        new CodeErrorException(statusCode,
                                                new string[] { ex.Message },
                                                ex.StackTrace!));
                }

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(result);
            }
        }
    }
}
