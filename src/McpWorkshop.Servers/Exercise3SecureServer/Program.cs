using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Exercise3SecureServer.Models;
using Exercise3SecureServer.Services;
using Exercise3SecureServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<RateLimitService>();

var app = builder.Build();

// Load data
var customers = LoadData<Customer>("Data/customers.json");
var products = LoadData<Product>("Data/products.json");
Console.WriteLine($"Loaded {customers.Length} customers and {products.Length} products");

// Add middleware pipeline in order
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<JwtAuthMiddleware>();
app.UseMiddleware<ScopeAuthorizationMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "healthy",
    server = "Exercise3SecureServer",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

// MCP endpoint
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
                    name = "Exercise3SecureServer",
                    version = "1.0.0"
                }
            },

            "resources/list" => new
            {
                resources = new[]
                {
                    new
                    {
                        uri = "secure://data/customers",
                        name = "Secure Customers",
                        description = "Lista segura de clientes registrados (requiere scope: read)",
                        mimeType = "application/json"
                    },
                    new
                    {
                        uri = "secure://data/products",
                        name = "Secure Products",
                        description = "Catálogo seguro de productos (requiere scope: read)",
                        mimeType = "application/json"
                    }
                }
            },

            "resources/read" => HandleResourceRead(request),

            "tools/list" => new
            {
                tools = new[]
                {
                    new
                    {
                        name = "create_order",
                        description = "Crear un nuevo pedido (requiere scope: write)",
                        inputSchema = new Dictionary<string, object>
                        {
                            ["type"] = "object",
                            ["properties"] = new Dictionary<string, object>
                            {
                                ["customerId"] = new Dictionary<string, object>
                                {
                                    ["type"] = "number",
                                    ["description"] = "ID del cliente"
                                },
                                ["productId"] = new Dictionary<string, object>
                                {
                                    ["type"] = "number",
                                    ["description"] = "ID del producto"
                                },
                                ["quantity"] = new Dictionary<string, object>
                                {
                                    ["type"] = "number",
                                    ["description"] = "Cantidad a ordenar"
                                }
                            },
                            ["required"] = new[] { "customerId", "productId", "quantity" }
                        }
                    }
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

// Auth token endpoint
app.MapPost("/auth/token", async (HttpContext context, JwtTokenService tokenService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    var tokenRequest = JsonSerializer.Deserialize<TokenRequest>(requestBody);

    if (tokenRequest == null || string.IsNullOrEmpty(tokenRequest.UserId))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = "Invalid request: userId is required"
        }));
        return;
    }

    var token = tokenService.GenerateToken(tokenRequest);

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new TokenResponse
    {
        Token = token,
        ExpiresIn = 3600
    }));
});

Console.WriteLine("✅ Exercise3SecureServer running on http://localhost:5003");
Console.WriteLine("📋 MCP endpoint: http://localhost:5003/mcp");
Console.WriteLine("🔐 Auth endpoint: http://localhost:5003/auth/token");
Console.WriteLine("\n🛡️ Security Features:");
Console.WriteLine("  - JWT Authentication (Bearer token required)");
Console.WriteLine("  - Scope-based Authorization (read, write, admin)");
Console.WriteLine("  - Rate Limiting (resources: 100/min, tools: 50/min)");
Console.WriteLine("  - Structured Logging with sensitive field redaction");
app.Run("http://localhost:5003");

// Helper functions
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
        "secure://data/customers" => JsonSerializer.Serialize(customers),
        "secure://data/products" => JsonSerializer.Serialize(products),
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

    if (toolName == "create_order")
    {
        var args = paramsObj.GetProperty("arguments");
        var customerId = args.GetProperty("customerId").GetInt32();
        var productId = args.GetProperty("productId").GetInt32();
        var quantity = args.GetProperty("quantity").GetInt32();

        var customer = customers.FirstOrDefault(c => c.Id == customerId);
        var product = products.FirstOrDefault(p => p.Id == productId);

        if (customer == null)
            throw new ArgumentException($"Customer not found: {customerId}");
        if (product == null)
            throw new ArgumentException($"Product not found: {productId}");
        if (product.Stock < quantity)
            throw new ArgumentException($"Insufficient stock for product {productId}");

        var totalAmount = product.Price * quantity;
        var orderId = new Random().Next(1000, 9999);

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"✅ Order created successfully!\n\nOrder ID: {orderId}\nCustomer: {customer.Name}\nProduct: {product.Name}\nQuantity: {quantity}\nTotal: €{totalAmount:F2}\nStatus: Confirmed"
                }
            }
        };
    }

    throw new InvalidOperationException($"Unknown tool: {toolName}");
}

// Make Program accessible to WebApplicationFactory for integration tests
namespace Exercise3SecureServer
{
    public partial class Program { }
}
