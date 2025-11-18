using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Exercise3SecureServer.Models;

namespace Exercise3SecureServer.Services;

public class RateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimits = new();
    private readonly IConfiguration _configuration;
    private readonly int _resourcesLimit;
    private readonly int _toolsLimit;
    private readonly int _unauthenticatedLimit;
    private readonly int _windowMinutes;

    public RateLimitService(IConfiguration configuration)
    {
        _configuration = configuration;
        _resourcesLimit = int.Parse(configuration["RateLimiting:Resources"] ?? "1000");
        _toolsLimit = int.Parse(configuration["RateLimiting:Tools"] ?? "500");
        _unauthenticatedLimit = int.Parse(configuration["RateLimiting:Unauthenticated"] ?? "100");
        _windowMinutes = int.Parse(configuration["RateLimiting:WindowMinutes"] ?? "1");
    }

    public (bool allowed, int limit, int remaining, DateTime resetTime) IsAllowed(string userId, string endpoint)
    {
        var key = $"{userId}:{endpoint}";
        var now = DateTime.UtcNow;
        var limit = GetLimit(userId, endpoint);

        var info = _rateLimits.AddOrUpdate(key,
            _ => new RateLimitInfo
            {
                RequestCount = 1,
                WindowStart = now,
                ResetTime = now.AddMinutes(_windowMinutes)
            },
            (_, existing) =>
            {
                if ((now - existing.WindowStart).TotalMinutes >= _windowMinutes)
                {
                    // Reset window
                    existing.RequestCount = 1;
                    existing.WindowStart = now;
                    existing.ResetTime = now.AddMinutes(_windowMinutes);
                }
                else
                {
                    existing.RequestCount++;
                }
                return existing;
            });

        var allowed = info.RequestCount <= limit;
        var remaining = Math.Max(0, limit - info.RequestCount);

        return (allowed, limit, remaining, info.ResetTime);
    }

    private int GetLimit(string userId, string endpoint)
    {
        if (userId == "unauthenticated")
            return _unauthenticatedLimit;

        return endpoint.ToLower() switch
        {
            "resources" => _resourcesLimit,
            "tools" => _toolsLimit,
            _ => _resourcesLimit
        };
    }
}
