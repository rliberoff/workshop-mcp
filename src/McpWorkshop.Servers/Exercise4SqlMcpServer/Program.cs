using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Exercise4SqlMcpServer.Models;
using Exercise4SqlMcpServer.Tools;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Load data
var customers = LoadData<Customer>("../../../data/customers.json");
var products = LoadData<Product>("../../../data/products.json");
var orders = LoadData<Order>("../../../data/orders.json");
Console.WriteLine($"✅ Loaded {customers.Length} customers, {products.Length} products, {orders.Length} orders");

// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "healthy",
    server = "Exercise4SqlMcpServer",
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
        result = method switch
        {
            "initialize" => new
            {
                protocolVersion = "2024-11-05",
                capabilities = new Dictionary<string, object>
                {
                    ["resources"] = new { },
                    ["tools"] = new { }
                },
                serverInfo = new
                {
                    name = "SqlMcpServer",
                    version = "1.0.0",
                    description = "Servidor MCP para datos transaccionales (SQL)"
                }
            },

            "resources/list" => new
            {
                resources = new[]
                {
                    new
                    {
                        uri = "sql://workshop/customers",
                        name = "SQL Customers",
                        description = "Lista completa de clientes registrados",
                        mimeType = "application/json"
                    },
                    new
                    {
                        uri = "sql://workshop/orders",
                        name = "SQL Orders",
                        description = "Historial de pedidos realizados",
                        mimeType = "application/json"
                    },
                    new
                    {
                        uri = "sql://workshop/products",
                        name = "SQL Products",
                        description = "Catálogo de productos disponibles",
                        mimeType = "application/json"
                    }
                }
            },

            "resources/read" => HandleResourceRead(request),

            "tools/list" => new
            {
                tools = new[]
                {
                    QueryCustomersByCountryTool.GetDefinition(),
                    GetSalesSummaryTool.GetDefinition()
                }
            },

            "tools/call" => HandleToolCall(request),

            _ => throw new InvalidOperationException($"Unknown method: {method}")
        };

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

Console.WriteLine("✅ SqlMcpServer running on http://localhost:5010/mcp");
Console.WriteLine("📊 Resources: customers, orders, products");
Console.WriteLine("🔧 Tools: query_customers_by_country, get_sales_summary");
app.Run("http://localhost:5010");

T[] LoadData<T>(string path)
{
    var json = File.ReadAllText(path);
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    return JsonSerializer.Deserialize<T[]>(json, options) ?? Array.Empty<T>();
}

object HandleResourceRead(JsonElement request)
{
    var paramsObj = request.GetProperty("params");
    var uri = paramsObj.GetProperty("uri").GetString();

    var data = uri switch
    {
        "sql://workshop/customers" => JsonSerializer.Serialize(customers),
        "sql://workshop/orders" => JsonSerializer.Serialize(orders),
        "sql://workshop/products" => JsonSerializer.Serialize(products),
        _ => throw new ArgumentException($"Unknown resource URI: {uri}")
    };

    return new
    {
        contents = new[]
        {
            new
            {
                uri,
                mimeType = "application/json",
                text = data
            }
        }
    };
}

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
        "query_customers_by_country" => QueryCustomersByCountryTool.Execute(arguments, customers),
        "get_sales_summary" => GetSalesSummaryTool.Execute(arguments, orders),
        _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
    };
}
