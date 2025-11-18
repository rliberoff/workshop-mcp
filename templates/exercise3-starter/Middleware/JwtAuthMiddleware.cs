using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Exercise3SecureServer.Middleware;

public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Extraer token del header Authorization: Bearer {token}
        // TODO: Validar token con TokenValidationParameters
        // TODO: Extraer claims (sub, scope)
        // TODO: Almacenar usuario autenticado en context.Items["User"]
        // TODO: Si falla validación, retornar 401 -32001

        await _next(context);
    }
}
