using System.Text;
using System.Text.Json;

namespace Exercise4VirtualAnalyst.Services;

public class McpServerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;

    public McpServerClient(string serverUrl)
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task<T?> CallToolAsync<T>(string toolName, Dictionary<string, object> arguments)
    {
        // TODO: Construir request JSON-RPC para tools/call
        // TODO: POST a {_serverUrl}/mcp
        // TODO: Deserializar resultado
        // TODO: Retornar resultado tipado

        return default(T);
    }

    public async Task<JsonElement> GetResourceAsync(string uri)
    {
        // TODO: Construir request JSON-RPC para resources/read
        // TODO: POST a {_serverUrl}/mcp
        // TODO: Extraer contents[0].text
        // TODO: Deserializar y retornar

        return new JsonElement();
    }
}
