using EnergyOptimizer.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace EnergyOptimizer.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: {Message}", ex.Message);

                context.Response.ContentType = "application/json";

                var statusCode = ex is BaseException baseEx
                    ? baseEx.StatusCode
                    : (int)HttpStatusCode.InternalServerError;

                context.Response.StatusCode = statusCode;
                var response = _env.IsDevelopment()
                   ? new ApiResponse(statusCode, ex.Message, ex.StackTrace?.ToString())
                   : new ApiResponse(statusCode, statusCode == 500 ? "Internal Server Error" : ex.Message);

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
        public record ApiResponse(int StatusCode, string Message, object? Details = null);
    }
}