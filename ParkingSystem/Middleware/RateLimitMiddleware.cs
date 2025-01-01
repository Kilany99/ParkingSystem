using ParkingSystem.Attributes;
using ParkingSystem.Handlers;
using System.Text.RegularExpressions;

namespace ParkingSystem.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly CustomRateLimitHandler _rateLimitHandler;

        public RateLimitMiddleware(
            RequestDelegate next,
            ILogger<RateLimitMiddleware> logger,
            CustomRateLimitHandler rateLimitHandler)
        {
            _next = next;
            _logger = logger;
            _rateLimitHandler = rateLimitHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var rateLimitAttribute = endpoint?.Metadata.GetMetadata<CustomRateLimitAttribute>();

            if (rateLimitAttribute != null)
            {
                var key = GetClientKey(context);
                var period = ParsePeriod(rateLimitAttribute.Period);

                if (!_rateLimitHandler.CheckRateLimit(key, rateLimitAttribute.Limit, period))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Too many requests",
                        retryAfter = period.TotalSeconds
                    });
                    return;
                }
            }

            await _next(context);
        }

        private string GetClientKey(HttpContext context)
        {
            // You can use IP, user ID, or any other identifier
            return context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";
        }

        private TimeSpan ParsePeriod(string period)
        {
            var match = Regex.Match(period, @"(\d+)([smhd])");
            if (!match.Success)
                throw new ArgumentException("Invalid period format");

            var value = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

            return unit switch
            {
                "s" => TimeSpan.FromSeconds(value),
                "m" => TimeSpan.FromMinutes(value),
                "h" => TimeSpan.FromHours(value),
                "d" => TimeSpan.FromDays(value),
                _ => throw new ArgumentException("Invalid time unit")
            };
        }
    }
}
