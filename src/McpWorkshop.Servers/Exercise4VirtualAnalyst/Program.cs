using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Exercise4VirtualAnalyst.Models;
using Exercise4VirtualAnalyst.Parsers;
using Exercise4VirtualAnalyst.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<SpanishQueryParser>();
builder.Services.AddSingleton<OrchestratorService>();

var app = builder.Build();

// Query endpoint
app.MapPost("/query", async (HttpContext context, SpanishQueryParser parser, OrchestratorService orchestrator) =>
{
    var stopwatch = Stopwatch.StartNew();

    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    var request = JsonSerializer.Deserialize<QueryRequest>(requestBody);

    if (request == null || string.IsNullOrEmpty(request.Query))
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = "Query is required"
        }));
        return;
    }

    var parsedQuery = parser.Parse(request.Query);
    var result = await orchestrator.ProcessQueryAsync(parsedQuery);

    stopwatch.Stop();

    var response = new QueryResponse
    {
        Query = request.Query,
        Intent = parsedQuery.Intent,
        Result = result,
        ServersUsed = parsedQuery.RequiredServers,
        FromCache = result.StartsWith("[CACHED]"),
        DurationMs = (int)stopwatch.ElapsedMilliseconds
    };

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
});

Console.WriteLine("✅ VirtualAnalyst Orchestrator running on http://localhost:5004/query");
Console.WriteLine("📋 Intenciones soportadas:");
Console.WriteLine("  - new_customers: '¿Cuántos clientes nuevos hay en Madrid?'");
Console.WriteLine("  - abandoned_carts: '¿Usuarios con carrito abandonado últimas 24 horas?'");
Console.WriteLine("  - order_status: '¿Estado del pedido 1001?'");
Console.WriteLine("  - sales_summary: 'Resumen de ventas de esta semana'");
Console.WriteLine("  - top_products: 'Top 10 productos más vendidos'");
Console.WriteLine("\n🔧 Servidores MCP requeridos:");
Console.WriteLine("  - SqlMcpServer (http://localhost:5010)");
Console.WriteLine("  - CosmosMcpServer (http://localhost:5011)");
Console.WriteLine("  - RestApiMcpServer (http://localhost:5012)");
app.Run("http://localhost:5004");
