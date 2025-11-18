using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Exercise3SecureServer.Models;

namespace Exercise3SecureServer.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    private static readonly Regex SensitiveFieldsRegex = new Regex(
        @"(""(?:password|token|secret|authorization)""\s*:\s*"")[^""]*("")",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Read and redact request body
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var redactedBody = RedactSensitiveFields(requestBody);
        var request = JsonSerializer.Deserialize<JsonElement>(requestBody);
        var method = request.TryGetProperty("method", out var methodProp) ? methodProp.GetString() : "unknown";

        var user = context.Items["User"] as AuthenticatedUser;
        var userId = user?.UserId ?? "anonymous";

        // Execute request
        await _next(context);

        stopwatch.Stop();
        var duration = stopwatch.ElapsedMilliseconds;

        // Log only if overhead is acceptable (<50ms per contract)
        if (duration < 50 || stopwatch.ElapsedMilliseconds - duration < 50)
        {
            _logger.LogInformation(
                "MCP Request: timestamp={Timestamp} level=Info method={Method} userId={UserId} requestId={RequestId} duration={Duration}ms statusCode={StatusCode}",
                DateTime.UtcNow.ToString("o"),
                method,
                userId,
                requestId,
                duration,
                context.Response.StatusCode
            );
        }
        else
        {
            _logger.LogWarning(
                "Logging overhead exceeded 50ms for requestId={RequestId}",
                requestId
            );
        }
    }

    private string RedactSensitiveFields(string json)
    {
        return SensitiveFieldsRegex.Replace(json, "$1[REDACTED]$2");
    }
}
