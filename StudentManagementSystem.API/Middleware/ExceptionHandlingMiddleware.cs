using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Common;

namespace StudentManagementSystem.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                var (statusCode, message) = ex switch
                {
                    AppException appEx => (appEx.StatusCode, appEx.Message),
                    DbUpdateException when ex.InnerException?.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) == true
                        => ((int)HttpStatusCode.Conflict, "Cannot delete: the record is referenced by other data."),
                    DbUpdateException => ((int)HttpStatusCode.Conflict, "A data conflict occurred while saving changes."),
                    _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("Response already started; rethrowing exception.");
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json; charset=utf-8";

                var response = new ResponseApi<object>
                {
                    IsSuccess = false,
                    StatusCode = statusCode,
                    Message = message,
                    Error = _environment.IsDevelopment() && statusCode == 500 ? ex.ToString() : null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
        }
    }
}