using Microsoft.Extensions.Caching.Memory;

namespace ParkingSystem.Helpers
{
    public interface IRateLimitMonitor
    {
        Task LogRateLimitEvent(string clientId, string endpoint, bool exceeded);
        Task<RateLimitStats> GetStats(string clientId);
    }

    public class RateLimitMonitor : IRateLimitMonitor
    {
        private readonly ILogger<RateLimitMonitor> _logger;
        private readonly IMemoryCache _cache;

        public async Task LogRateLimitEvent(string clientId, string endpoint, bool exceeded)
        {
            _logger.LogInformation(
                "Rate limit {Status} for client {ClientId} on {Endpoint}",
                exceeded ? "exceeded" : "checked",
                clientId,
                endpoint);

            // You could also store this in a database for analysis
        }

        public async Task<RateLimitStats> GetStats(string clientId)
        {
            // Implement statistics retrieval
            return new RateLimitStats();
        }
    }

    public class RateLimitStats
    {
        public int TotalRequests { get; set; }
        public int ExceededRequests { get; set; }
        public Dictionary<string, int> EndpointHits { get; set; }
    }
}
