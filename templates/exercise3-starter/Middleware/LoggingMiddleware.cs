namespace Exercise3SecureServer.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Iniciar cronómetro
        // TODO: Generar requestId (Guid)
        // TODO: Leer request body (preservar para siguiente middleware)
        // TODO: Redactar campos sensibles (password, token, secret)
        // TODO: Ejecutar siguiente middleware
        // TODO: Capturar statusCode y duration
        // TODO: Loguear con campos estructurados:
        //   - timestamp, level, method, userId, requestId, duration, statusCode

        await _next(context);
    }
}
