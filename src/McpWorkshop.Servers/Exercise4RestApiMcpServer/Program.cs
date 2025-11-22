using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Exercise4RestApiMcpServer.Tools;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Console.WriteLine("✅ RestApiMcpServer initialized");

// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "healthy",
    server = "Exercise4RestApiMcpServer",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapPost("/mcp", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    var request = JsonSerializer.Deserialize<JsonElement>(requestBody);

    var method = request.GetProperty("method").GetString();
    var id = request.GetProperty("id");

    object? result = null;

    try
    {
        switch (method)
        {
            case "initialize":
                result = new
                {
                    protocolVersion = "2024-11-05",
                    capabilities = new Dictionary<string, object>
                    {
                        ["tools"] = new { }
                    },
                    serverInfo = new
                    {
                        name = "RestApiMcpServer",
                        version = "1.0.0",
                        description = "Servidor MCP para APIs externas (REST)"
                    }
                };
                break;

            case "tools/list":
                result = new
                {
                    tools = new[]
                    {
                        CheckInventoryTool.GetDefinition(),
                        GetShippingStatusTool.GetDefinition(),
                        GetTopProductsTool.GetDefinition()
                    }
                };
                break;

            case "tools/call":
                result = HandleToolCall(request);
                break;

            default:
                throw new InvalidOperationException($"Unknown method: {method}");
        }

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            result,
            id
        }));
    }
    catch (Exception ex)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            error = new { code = -32603, message = $"Internal error: {ex.Message}" },
            id
        }));
    }
});

Console.WriteLine("✅ RestApiMcpServer running on http://localhost:5012/mcp");
Console.WriteLine("🔧 Tools: check_inventory, get_shipping_status, get_top_products");
app.Run("http://localhost:5012");

object HandleToolCall(JsonElement request)
{
    var paramsObj = request.GetProperty("params");
    var toolName = paramsObj.GetProperty("name").GetString();
    var argsElement = paramsObj.GetProperty("arguments");

    var arguments = new Dictionary<string, JsonElement>();
    foreach (var prop in argsElement.EnumerateObject())
    {
        arguments[prop.Name] = prop.Value;
    }

    return toolName switch
    {
        "check_inventory" => CheckInventoryTool.Execute(arguments),
        "get_shipping_status" => GetShippingStatusTool.Execute(arguments),
        "get_top_products" => GetTopProductsTool.Execute(arguments),
        _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
    };
}
