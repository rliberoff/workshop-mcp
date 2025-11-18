namespace Exercise3SecureServer.Middleware;

public class ScopeAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public ScopeAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Obtener método MCP del request body
        // TODO: Mapear método a scope requerido:
        //   - initialize, resources/list, tools/list: sin scope
        //   - resources/read: requiere "read"
        //   - tools/call: requiere "write"
        // TODO: Verificar que el usuario tenga el scope requerido
        // TODO: Si falla autorización, retornar 403 -32002

        await _next(context);
    }
}
