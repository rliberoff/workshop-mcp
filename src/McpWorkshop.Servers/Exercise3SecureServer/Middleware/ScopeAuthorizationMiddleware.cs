using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Exercise3SecureServer.Models;

namespace Exercise3SecureServer.Middleware;

public class ScopeAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public ScopeAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for token endpoint
        if (context.Request.Path.StartsWithSegments("/auth/token"))
        {
            await _next(context);
            return;
        }

        // Read request body to extract method
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var request = JsonSerializer.Deserialize<JsonElement>(requestBody);
        var method = request.GetProperty("method").GetString() ?? string.Empty;

        var requiredScope = GetRequiredScope(method);

        // If scope is required, check authorization
        if (!string.IsNullOrEmpty(requiredScope))
        {
            var user = context.Items["User"] as AuthenticatedUser;
            if (user == null || !HasScope(user.Scopes, requiredScope))
            {
                await ReturnForbiddenError(context, requiredScope);
                return;
            }
        }

        await _next(context);
    }

    private string GetRequiredScope(string method)
    {
        return method switch
        {
            "initialize" => "",
            "resources/list" => "",
            "resources/read" => "read",
            "tools/list" => "",
            "tools/call" => "write",
            _ => ""
        };
    }

    private bool HasScope(string[] userScopes, string requiredScope)
    {
        // Admin has all scopes
        if (userScopes.Contains("admin"))
            return true;

        // Write includes read
        if (requiredScope == "read" && userScopes.Contains("write"))
            return true;

        return userScopes.Contains(requiredScope);
    }

    private async Task ReturnForbiddenError(HttpContext context, string requiredScope)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        var error = new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = -32002,
                message = $"Forbidden: Insufficient permissions. Required scope: {requiredScope}"
            },
            id = (object?)null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}
