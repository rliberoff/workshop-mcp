namespace Exercise3SecureServer.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public RateLimitMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Obtener userId del usuario autenticado
        // TODO: Obtener endpoint (resources o tools) del método MCP
        // TODO: Verificar límite de rate (sliding window)
        // TODO: Incrementar contador de requests
        // TODO: Si excede límite, retornar 429 -32003
        // TODO: Agregar headers X-RateLimit-*

        await _next(context);
    }
}
