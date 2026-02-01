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
                // for logging the exception details
                _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);
                
                // preparing the error response
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new ApiResponse(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiResponse(context.Response.StatusCode, "An internal server error occurred.");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
    public record ApiResponse(int StatusCode, string Message, string Details = null);
}