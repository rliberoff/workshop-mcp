using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// TODO: Agregar middleware de autenticación JWT
// app.UseMiddleware<JwtAuthMiddleware>();

// TODO: Agregar middleware de autorización por scopes
// app.UseMiddleware<ScopeAuthorizationMiddleware>();

// TODO: Agregar middleware de rate limiting
// app.UseMiddleware<RateLimitMiddleware>();

// TODO: Agregar middleware de logging estructurado
// app.UseMiddleware<LoggingMiddleware>();

app.MapPost("/mcp", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    var request = JsonSerializer.Deserialize<JsonElement>(requestBody);

    var method = request.GetProperty("method").GetString();
    var id = request.GetProperty("id");

    switch (method)
    {
        case "initialize":
            // TODO: Implementar initialize con serverInfo
            break;

        case "resources/list":
            // TODO: Implementar listado de recursos (requiere scope: read)
            break;

        case "resources/read":
            // TODO: Implementar lectura de recursos (requiere scope: read)
            break;

        case "tools/list":
            // TODO: Implementar listado de herramientas
            break;

        case "tools/call":
            // TODO: Implementar llamada a herramientas (requiere scope: write)
            break;

        default:
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                jsonrpc = "2.0",
                error = new { code = -32601, message = $"Method not found: {method}" },
                id
            }));
            return;
    }
});

// Endpoint de autenticación para testing
app.MapPost("/auth/token", async (HttpContext context) =>
{
    // TODO: Implementar generación de JWT token
    // Recibir TokenRequest { userId, scopes[], tier }
    // Generar token con claims: sub (userId), scope (string separado por espacios), tier
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        token = "TODO_GENERATE_JWT_TOKEN",
        expiresIn = 3600
    }));
});

Console.WriteLine("Exercise3SecureServer listening on http://localhost:5003/mcp");
Console.WriteLine("Auth endpoint: http://localhost:5003/auth/token");
app.Run("http://localhost:5003");
