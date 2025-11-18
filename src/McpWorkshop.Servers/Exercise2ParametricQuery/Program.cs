using Exercise2ParametricQuery.Models;
using Exercise2ParametricQuery.Tools;
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

// Cargar datos
var customers = LoadData<Customer>("customers.json");
var products = LoadData<Product>("products.json");
var orders = LoadData<Order>("orders.json");

Console.WriteLine($"[INFO] Loaded {customers.Length} customers, {products.Length} products, {orders.Length} orders");

T[] LoadData<T>(string fileName) where T : class
{
    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName);
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"[ERROR] File not found: {filePath}");
        return Array.Empty<T>();
    }

    var json = File.ReadAllText(filePath);
    var data = JsonSerializer.Deserialize<T[]>(json);
    return data ?? Array.Empty<T>();
}

// Endpoint MCP
app.MapPost("/mcp", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();

        Console.WriteLine($"[INFO] Request: {requestBody}");

        var request = JsonSerializer.Deserialize<JsonElement>(requestBody);
        var method = request.GetProperty("method").GetString();
        var id = request.GetProperty("id");

        object? result = method switch
        {
            "initialize" => new
            {
                protocolVersion = "2024-11-05",
                capabilities = new Dictionary<string, object>
                {
                    ["tools"] = new { }
                },
                serverInfo = new
                {
                    name = "exercise2-parametric-query",
                    version = "1.0.0"
                }
            },

            "tools/list" => new
            {
                tools = new[]
                {
                    SearchCustomersTool.GetDefinition(),
                    FilterProductsTool.GetDefinition(),
                    SearchOrdersTool.GetDefinition(),
                    AggregateSalesTool.GetDefinition()
                }
            },

            "tools/call" => HandleToolCall(request),

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

        await context.Response.WriteAsJsonAsync(response);
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

object HandleToolCall(JsonElement request)
{
    var paramsElement = request.GetProperty("params");
    var toolName = paramsElement.GetProperty("name").GetString();

    var arguments = new Dictionary<string, JsonElement>();
    if (paramsElement.TryGetProperty("arguments", out var argsElement))
    {
        foreach (var property in argsElement.EnumerateObject())
        {
            arguments[property.Name] = property.Value;
        }
    }

    Console.WriteLine($"[INFO] Tool call: {toolName} with {arguments.Count} argument(s)");

    return toolName switch
    {
        "GetCustomers" => SearchCustomersTool.Execute(arguments, customers),
        "GetProducts" => FilterProductsTool.Execute(arguments, products),
        "SearchOrders" => SearchOrdersTool.Execute(arguments, orders),
        "CalculateTotal" => AggregateSalesTool.Execute(arguments, orders),
        _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
    };
}

Console.WriteLine("[INFO] Exercise 2 Parametric Query Server starting on http://localhost:5002");
app.Run("http://localhost:5002");

// Make Program accessible to WebApplicationFactory for integration tests
namespace Exercise2ParametricQuery
{
    public partial class Program { }
}
