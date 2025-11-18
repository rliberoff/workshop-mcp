using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

    public async Task<JsonElement> CallToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        var requestParams = new Dictionary<string, object>
        {
            ["name"] = toolName,
            ["arguments"] = arguments
        };

        var request = new Dictionary<string, object>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "tools/call",
            ["params"] = requestParams,
            ["id"] = 1
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_serverUrl}/mcp", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

        if (result.TryGetProperty("result", out var resultProp))
            return resultProp;

        throw new InvalidOperationException("No result in response");
    }

    public async Task<JsonElement> GetResourceAsync(string uri)
    {
        var requestParams = new Dictionary<string, object>
        {
            ["uri"] = uri
        };

        var request = new Dictionary<string, object>
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "resources/read",
            ["params"] = requestParams,
            ["id"] = 1
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_serverUrl}/mcp", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

        if (result.TryGetProperty("result", out var resultProp))
        {
            if (resultProp.TryGetProperty("contents", out var contents))
            {
                var firstContent = contents.EnumerateArray().First();
                if (firstContent.TryGetProperty("text", out var text))
                {
                    return JsonSerializer.Deserialize<JsonElement>(text.GetString() ?? "{}");
                }
            }
        }

        throw new InvalidOperationException("No resource data in response");
    }
}
