using System.Text;
using System.Text.Json;
using Exercise4VirtualAnalyst.Services;
using Exercise4VirtualAnalyst.Parsers;

var builder = WebApplication.CreateBuilder(args);

// TODO: Registrar servicios
// builder.Services.AddSingleton<SpanishQueryParser>();
// builder.Services.AddSingleton<OrchestratorService>();

var app = builder.Build();

// Endpoint para consultas en español
app.MapPost("/query", async (HttpContext context) =>
{
    // TODO: Leer query del body
    // TODO: Parsear query con SpanishQueryParser
    // TODO: Ejecutar con OrchestratorService
    // TODO: Retornar resultado

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        query = "TODO",
        result = "Implementar orquestación"
    }));
});

Console.WriteLine("VirtualAnalyst Orchestrator listening on http://localhost:5004/query");
app.Run("http://localhost:5004");
