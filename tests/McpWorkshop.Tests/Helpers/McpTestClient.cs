using System.Net.Http.Json;
using System.Text.Json;
using McpWorkshop.Shared.Mcp;

namespace McpWorkshop.Tests.Helpers;

/// <summary>
/// Cliente de prueba para servidores MCP que proporciona métodos helper
/// para enviar solicitudes JSON-RPC 2.0 y validar respuestas.
/// </summary>
public class McpTestClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public McpTestClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Envía una solicitud de inicialización al servidor MCP.
    /// </summary>
    public async Task<JsonRpcResponse> InitializeAsync(
        string clientName = "McpTestClient",
        string clientVersion = "1.0.0",
        CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Method = "initialize",
            Params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    roots = new { listChanged = true },
                    sampling = new { }
                },
                clientInfo = new
                {
                    name = clientName,
                    version = clientVersion
                }
            },
            Id = GenerateRequestId()
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Envía una solicitud para listar recursos disponibles.
    /// </summary>
    public async Task<JsonRpcResponse> ListResourcesAsync(
        CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Method = "resources/list",
            Params = new { },
            Id = GenerateRequestId()
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Envía una solicitud para leer un recurso específico.
    /// </summary>
    public async Task<JsonRpcResponse> ReadResourceAsync(
        string resourceUri,
        CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Method = "resources/read",
            Params = new { uri = resourceUri },
            Id = GenerateRequestId()
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Envía una solicitud para listar herramientas disponibles.
    /// </summary>
    public async Task<JsonRpcResponse> ListToolsAsync(
        CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Method = "tools/list",
            Params = new { },
            Id = GenerateRequestId()
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Envía una solicitud para invocar una herramienta.
    /// </summary>
    public async Task<JsonRpcResponse> CallToolAsync(
        string toolName,
        object? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Method = "tools/call",
            Params = new
            {
                name = toolName,
                arguments = arguments ?? new { }
            },
            Id = GenerateRequestId()
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    /// <summary>
    /// Envía una solicitud JSON-RPC 2.0 genérica.
    /// </summary>
    public async Task<JsonRpcResponse> SendRequestAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var response = await _httpClient.PostAsJsonAsync(
            "/mcp",
            request,
            _jsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonRpcResponse>(
            _jsonOptions,
            cancellationToken);

        return jsonResponse ?? throw new InvalidOperationException(
            "La respuesta del servidor no pudo ser deserializada.");
    }

    /// <summary>
    /// Envía una solicitud JSON-RPC 2.0 sin esperar respuesta válida
    /// (útil para tests de errores).
    /// </summary>
    public async Task<HttpResponseMessage> SendRawRequestAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await _httpClient.PostAsJsonAsync(
            "/mcp",
            request,
            _jsonOptions,
            cancellationToken);
    }

    /// <summary>
    /// Valida que una respuesta tenga el formato JSON-RPC 2.0 correcto.
    /// </summary>
    public static bool IsValidJsonRpcResponse(JsonRpcResponse response)
    {
        if (response == null)
            return false;

        // Debe tener jsonrpc="2.0"
        if (response.JsonRpc != "2.0")
            return false;

        // Debe tener id
        if (response.Id == null)
            return false;

        // Debe tener result o error (pero no ambos)
        if (response.Result == null && response.Error == null)
            return false;

        if (response.Result != null && response.Error != null)
            return false;

        return true;
    }

    /// <summary>
    /// Valida que una respuesta de error tenga el formato correcto.
    /// </summary>
    public static bool IsValidErrorResponse(JsonRpcResponse response, int expectedErrorCode)
    {
        if (!IsValidJsonRpcResponse(response))
            return false;

        if (response.Error == null)
            return false;

        return response.Error.Code == expectedErrorCode;
    }

    /// <summary>
    /// Genera un ID único para una solicitud.
    /// </summary>
    private static string GenerateRequestId()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _httpClient?.Dispose();
        }

        _disposed = true;
    }
}
