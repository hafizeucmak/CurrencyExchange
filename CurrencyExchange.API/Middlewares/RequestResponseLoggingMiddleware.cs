using System.Diagnostics;

namespace CurrencyExchange.API.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var method = context.Request.Method;
            var endpoint = context.Request.Path;
            var statusCode = context.Response.StatusCode;
            var responseTime = stopwatch.ElapsedMilliseconds;

            var clientId = context.User.Claims.FirstOrDefault(c => c.Type == "clientId")?.Value ?? "anonymous";

            _logger.LogInformation("lOG : Request from {ClientIP}, ClientId: {ClientId}, {Method} {Endpoint}, StatusCode: {StatusCode}, ResponseTime: {ResponseTime}ms",
                clientIp, clientId, method, endpoint, statusCode, responseTime);
        }
    }
}
