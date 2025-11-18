using Exercise1StaticResources.Models;
using System.Text.Json;
using McpWorkshop.Shared.Monitoring;
using McpWorkshop.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<PerformanceTracker>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Add middleware
app.UseHttpsRedirection();
app.UseCors();
app.UseSecurityHeaders();

// Helper para cargar datos JSON
T[] LoadData<T>(string fileName) where T : class
{
    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName);
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"[ERROR] File not found: {filePath}");
        return Array.Empty<T>();
    }

    var json = File.ReadAllText(filePath);
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    var data = JsonSerializer.Deserialize<T[]>(json, options);
    return data ?? Array.Empty<T>();
}

// Endpoint MCP
app.MapPost("/mcp", async (HttpContext context, PerformanceTracker tracker) =>
{
    string? method = null;
    using var perfTracker = tracker.TrackRequest("mcp");

    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();

        Console.WriteLine($"[INFO] Request received: {requestBody}");

        var request = JsonSerializer.Deserialize<JsonElement>(requestBody);
        method = request.GetProperty("method").GetString();
        var id = request.GetProperty("id");

        using var methodTracker = tracker.TrackRequest(method ?? "unknown", id.ToString());

        object? result = method switch
        {
            "initialize" => new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    resources = new { }
                },
                serverInfo = new
                {
                    name = "exercise1-static-resources",
                    version = "1.0.0"
                }
            },

            "resources/list" => new
            {
                resources = new[]
                {
                    new
                    {
                        uri = "customer://all",
                        name = "Customers",
                        description = "Lista de clientes registrados",
                        mimeType = "application/json"
                    },
                    new
                    {
                        uri = "product://all",
                        name = "Products",
                        description = "Catálogo de productos disponibles",
                        mimeType = "application/json"
                    },
                    new
                    {
                        uri = "order://all",
                        name = "Orders",
                        description = "Lista de pedidos",
                        mimeType = "application/json"
                    }
                }
            },

            "resources/read" => HandleResourceRead(request),

            _ => null
        };

        if (result == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                jsonrpc = "2.0",
                error = new
                {
                    code = -32601,
                    message = $"Method not found: {method}"
                },
                id
            });
            return;
        }

        var response = new
        {
            jsonrpc = "2.0",
            result,
            id
        };

        Console.WriteLine($"[INFO] Response: {JsonSerializer.Serialize(response)}");
        await context.Response.WriteAsJsonAsync(response);
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"[ERROR] Invalid params: {ex.Message}");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = -32602,
                message = "Invalid params",
                data = ex.Message
            },
            id = (object?)null
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = -32603,
                message = "Internal error",
                data = ex.Message
            }
        });
    }
});

object HandleResourceRead(JsonElement request)
{
    if (!request.TryGetProperty("params", out var paramsElement))
    {
        throw new ArgumentException("Missing required parameter: params");
    }

    if (!paramsElement.TryGetProperty("uri", out var uriElement))
    {
        throw new ArgumentException("Missing required parameter: uri");
    }

    var uri = uriElement.GetString();

    var contents = uri switch
    {
        "customer://all" or "customers" => new[]
        {
            new
            {
                uri = "customer://all",
                mimeType = "application/json",
                text = JsonSerializer.Serialize(LoadData<Customer>("customers.json"), new JsonSerializerOptions { WriteIndented = true })
            }
        },
        "product://all" or "products" => new[]
        {
            new
            {
                uri = "product://all",
                mimeType = "application/json",
                text = JsonSerializer.Serialize(LoadData<Product>("products.json"), new JsonSerializerOptions { WriteIndented = true })
            }
        },
        "order://all" or "orders" => new[]
        {
            new
            {
                uri = "order://all",
                mimeType = "application/json",
                text = JsonSerializer.Serialize(LoadData<Order>("orders.json"), new JsonSerializerOptions { WriteIndented = true })
            }
        },
        _ => throw new ArgumentException($"Unknown resource: {uri}")
    };

    return new { contents };
}

// Configure URLs - allow override for testing
if (!builder.Environment.IsDevelopment() || args.Length == 0)
{
    app.Urls.Add("http://localhost:5001");
}

Console.WriteLine($"[INFO] Exercise 1 Static Resources Server starting on {string.Join(", ", app.Urls)}");
app.Run();

// Make Program accessible to WebApplicationFactory for integration tests
namespace Exercise1StaticResources
{
    public partial class Program { }
}
