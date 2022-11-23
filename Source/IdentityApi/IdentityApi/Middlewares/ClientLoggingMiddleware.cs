using Serilog.Context;

namespace IdentityApi.Middlewares
{
    public class ClientLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ClientLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            LogContext.PushProperty("ClientIp", ip);

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }

    public static class ClientLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseClientLogging(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientLoggingMiddleware>();
        }
    }
}
