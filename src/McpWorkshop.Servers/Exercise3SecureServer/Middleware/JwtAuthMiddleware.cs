using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Exercise3SecureServer.Services;
using Exercise3SecureServer.Models;

namespace Exercise3SecureServer.Middleware;

public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtTokenService _tokenService;

    public JwtAuthMiddleware(RequestDelegate next, JwtTokenService tokenService)
    {
        _next = next;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth for token endpoint
        if (context.Request.Path.StartsWithSegments("/auth/token"))
        {
            await _next(context);
            return;
        }

        // Extract token from Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await ReturnUnauthorizedError(context);
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var user = _tokenService.ValidateToken(token);

        if (user == null)
        {
            await ReturnUnauthorizedError(context);
            return;
        }

        // Store authenticated user in context
        context.Items["User"] = user;

        await _next(context);
    }

    private async Task ReturnUnauthorizedError(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var error = new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = -32001,
                message = "Unauthorized: Missing or invalid token"
            },
            id = (object?)null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}
