using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace LibrarySystem.WebApi.Middlewars
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApplicationException ex)
            {
                log(context, ex);
                await HandleExceptionAsync(context, ex.Message, 400);
            }
            catch (SqlException ex)
            {
                log(context, ex);
                await HandleExceptionAsync(context, "Internal Server Error", 500);
            }
            catch (Exception ex)
            {
                log(context, ex);
                await HandleExceptionAsync(context, "Unexpected error", 500);
            }
        }

        private void log(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unexpected error. Method: {Method}, Path: {Path}, User: {User}",
                                            context.Request.Method,
                                            context.Request.Path,
                                            context.User.Identity?.Name);
        }

        private static Task HandleExceptionAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                error = message
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }

}
