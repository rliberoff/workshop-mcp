using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Exercise3SecureServer.Models;
using Exercise3SecureServer.Services;

namespace Exercise3SecureServer.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitService _rateLimitService;

    public RateLimitMiddleware(RequestDelegate next, RateLimitService rateLimitService)
    {
        _next = next;
        _rateLimitService = rateLimitService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for token endpoint
        if (context.Request.Path.StartsWithSegments("/auth/token"))
        {
            await _next(context);
            return;
        }

        // Get user and endpoint
        var user = context.Items["User"] as AuthenticatedUser;
        var userId = user?.UserId ?? "unauthenticated";

        // Read request body to extract method
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var request = JsonSerializer.Deserialize<JsonElement>(requestBody);
        var method = request.GetProperty("method").GetString() ?? string.Empty;

        var endpoint = method.StartsWith("resources/") ? "resources" : "tools";

        // Check rate limit
        var (allowed, limit, remaining, resetTime) = _rateLimitService.IsAllowed(userId, endpoint);

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();

        if (!allowed)
        {
            await ReturnRateLimitError(context, resetTime);
            return;
        }

        await _next(context);
    }

    private async Task ReturnRateLimitError(HttpContext context, DateTime resetTime)
    {
        context.Response.StatusCode = 429;
        context.Response.ContentType = "application/json";

        var secondsUntilReset = (int)(resetTime - DateTime.UtcNow).TotalSeconds;
        context.Response.Headers["Retry-After"] = secondsUntilReset.ToString();

        var error = new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = -32003,
                message = $"Rate limit exceeded. Try again in {secondsUntilReset} seconds",
                data = new
                {
                    resetTime = resetTime.ToString("o")
                }
            },
            id = (object?)null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}
