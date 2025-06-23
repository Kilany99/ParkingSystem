using Microsoft.Extensions.Caching.Memory;

namespace ParkingSystem.Handlers
{
    public class CustomRateLimitHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CustomRateLimitHandler> _logger;

        public CustomRateLimitHandler(
            IMemoryCache cache,
            ILogger<CustomRateLimitHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public bool CheckRateLimit(string key, int limit, TimeSpan period)
        {
            var counter = _cache.GetOrCreate($"ratelimit_{key}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = period;
                return new CounterModel { Count = 0, FirstAccess = DateTime.UtcNow };
            });

            if (counter.Count >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
                return false;
            }

            counter.Count++;
            _cache.Set($"ratelimit_{key}", counter, period);
            return true;
        }

        private class CounterModel
        {
            public int Count { get; set; }
            public DateTime FirstAccess { get; set; }
        }
    }
}
