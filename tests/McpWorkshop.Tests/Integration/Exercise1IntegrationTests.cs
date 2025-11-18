using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Exercise1StaticResources;

namespace McpWorkshop.Tests.Integration;

/// <summary>
/// Integration tests for Exercise 1: Static Resources MCP Server.
/// Validates resources/list and resources/read endpoints.
/// </summary>
public class Exercise1IntegrationTests : IClassFixture<WebApplicationFactory<Exercise1StaticResources.Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Exercise1StaticResources.Program> _factory;

    public Exercise1IntegrationTests(WebApplicationFactory<Exercise1StaticResources.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Initialize_ShouldReturnValidServerInfo()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new
                {
                    name = "test-client",
                    version = "1.0.0"
                }
            },
            id = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.Equal("2.0", jsonResponse["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(jsonResponse["result"]);
        Assert.NotNull(jsonResponse["result"]!["serverInfo"]);

        var serverInfo = jsonResponse["result"]!["serverInfo"];
        Assert.Equal("exercise1-static-resources", serverInfo!["name"]?.GetValue<string>());
        Assert.Matches(@"^\d+\.\d+\.\d+$", serverInfo["version"]?.GetValue<string>() ?? "");
    }

    [Fact]
    public async Task ResourcesList_ShouldReturnAllAvailableResources()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.Equal("2.0", jsonResponse["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(jsonResponse["result"]);
        Assert.NotNull(jsonResponse["result"]!["resources"]);

        var resources = jsonResponse["result"]!["resources"]!.AsArray();
        Assert.NotEmpty(resources);

        // Verify each resource has required fields
        foreach (var resource in resources)
        {
            Assert.NotNull(resource!["uri"]);
            Assert.NotNull(resource["name"]);
            Assert.NotNull(resource["description"]);
            Assert.NotNull(resource["mimeType"]);
        }
    }

    [Fact]
    public async Task ResourcesList_ShouldIncludeExpectedResourceTypes()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        var resources = jsonResponse!["result"]!["resources"]!.AsArray();
        var uris = resources.Select(r => r!["uri"]?.GetValue<string>()).ToList();

        // Assert
        Assert.Contains(uris, uri => uri!.StartsWith("customer://"));
        Assert.Contains(uris, uri => uri!.StartsWith("product://"));
        Assert.Contains(uris, uri => uri!.StartsWith("order://"));
    }

    [Fact]
    public async Task ResourcesRead_WithValidUri_ShouldReturnResourceContent()
    {
        // Arrange - First get the list to find a valid URI
        var listRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        var listResponse = await _client.PostAsJsonAsync("/mcp", listRequest);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listJson = JsonNode.Parse(listContent);
        var resources = listJson!["result"]!["resources"]!.AsArray();
        var firstResourceUri = resources[0]!["uri"]?.GetValue<string>();

        // Arrange - Read request
        var readRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = firstResourceUri
            },
            id = 3
        };

        // Act
        var readResponse = await _client.PostAsJsonAsync("/mcp", readRequest);
        readResponse.EnsureSuccessStatusCode();

        var readContent = await readResponse.Content.ReadAsStringAsync();
        var readJson = JsonNode.Parse(readContent);

        // Assert
        Assert.NotNull(readJson);
        Assert.Equal("2.0", readJson["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(readJson["result"]);
        Assert.NotNull(readJson["result"]!["contents"]);

        var contents = readJson["result"]!["contents"]!.AsArray();
        Assert.NotEmpty(contents);

        var firstContent = contents[0];
        Assert.NotNull(firstContent!["uri"]);
        Assert.NotNull(firstContent["mimeType"]);
        Assert.NotNull(firstContent["text"]); // Static resources should have text content
    }

    [Fact]
    public async Task ResourcesRead_WithInvalidUri_ShouldReturnError()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = "invalid://nonexistent-resource"
            },
            id = 4
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.Equal("2.0", jsonResponse["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(jsonResponse["error"]);
        Assert.Null(jsonResponse["result"]);

        var error = jsonResponse["error"];
        Assert.NotNull(error!["code"]);
        Assert.NotNull(error["message"]);
    }

    [Fact]
    public async Task ResourcesRead_ContentShouldBeValidJson()
    {
        // Arrange - Get first customer resource
        var listRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        var listResponse = await _client.PostAsJsonAsync("/mcp", listRequest);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listJson = JsonNode.Parse(listContent);
        var resources = listJson!["result"]!["resources"]!.AsArray();
        var customerUri = resources.FirstOrDefault(r => r!["uri"]?.GetValue<string>()?.StartsWith("customer://") == true)?["uri"]?.GetValue<string>();

        Assert.NotNull(customerUri);

        var readRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = customerUri
            },
            id = 3
        };

        // Act
        var readResponse = await _client.PostAsJsonAsync("/mcp", readRequest);
        var readContent = await readResponse.Content.ReadAsStringAsync();
        var readJson = JsonNode.Parse(readContent);

        var contents = readJson!["result"]!["contents"]!.AsArray();
        var textContent = contents[0]!["text"]?.GetValue<string>();

        // Assert - Verify content is valid JSON
        Assert.NotNull(textContent);
        var parsedContent = JsonNode.Parse(textContent);
        Assert.NotNull(parsedContent);

        // Content can be array or object - both are valid
        Assert.True(parsedContent is JsonArray || parsedContent is JsonObject, "Parsed content should be valid JSON array or object");

        // If it's an array, validate first element structure; if object, validate directly
        if (parsedContent is JsonArray arrayContent && arrayContent.Count > 0)
        {
            var firstItem = arrayContent[0];
            Assert.NotNull(firstItem);
            // Convert to JsonObject to access properties (JSON property names are capitalized)
            var firstObj = firstItem.AsObject();
            Assert.NotNull(firstObj["Id"]);
            Assert.NotNull(firstObj["Name"]);
            Assert.NotNull(firstObj["Email"]);
        }
        else if (parsedContent is JsonObject objContent)
        {
            Assert.NotNull(objContent["Id"]);
            Assert.NotNull(objContent["Name"]);
            Assert.NotNull(objContent["Email"]);
        }
    }

    [Fact]
    public async Task ResourcesList_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        stopwatch.Stop();

        // Assert - Should respond within 500ms (p95 target)
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"resources/list took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task ResourcesRead_ResponseTime_ShouldBeFast()
    {
        // Arrange - Get first resource URI
        var listRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        var listResponse = await _client.PostAsJsonAsync("/mcp", listRequest);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listJson = JsonNode.Parse(listContent);
        var resources = listJson!["result"]!["resources"]!.AsArray();
        var firstResourceUri = resources[0]!["uri"]?.GetValue<string>();

        var readRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = firstResourceUri
            },
            id = 3
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var readResponse = await _client.PostAsJsonAsync("/mcp", readRequest);
        readResponse.EnsureSuccessStatusCode();

        stopwatch.Stop();

        // Assert - Should respond within 500ms (p95 target)
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"resources/read took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task Capabilities_ShouldIndicateResourcesSupport()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new
                {
                    name = "test-client",
                    version = "1.0.0"
                }
            },
            id = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        var capabilities = jsonResponse!["result"]!["capabilities"];
        Assert.NotNull(capabilities);
        Assert.NotNull(capabilities!["resources"]);
    }

    [Fact]
    public async Task MultipleRequests_ShouldMaintainConsistentState()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        // Act - Make multiple requests
        var response1 = await _client.PostAsJsonAsync("/mcp", request);
        var content1 = await response1.Content.ReadAsStringAsync();
        var json1 = JsonNode.Parse(content1);

        var response2 = await _client.PostAsJsonAsync("/mcp", request);
        var content2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonNode.Parse(content2);

        // Assert - Both responses should be identical
        var resources1 = json1!["result"]!["resources"]!.AsArray();
        var resources2 = json2!["result"]!["resources"]!.AsArray();

        Assert.Equal(resources1.Count, resources2.Count);

        // Verify same URIs in same order
        for (int i = 0; i < resources1.Count; i++)
        {
            Assert.Equal(
                resources1[i]!["uri"]?.GetValue<string>(),
                resources2[i]!["uri"]?.GetValue<string>()
            );
        }
    }

    [Fact]
    public async Task InvalidMethod_ShouldReturnMethodNotFoundError()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "invalid/method",
            id = 99
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["error"]);
        Assert.Equal(-32601, jsonResponse["error"]!["code"]?.GetValue<int>());
        Assert.Contains("not found", jsonResponse["error"]!["message"]?.GetValue<string>()?.ToLower() ?? "");
    }

    [Fact]
    public async Task MissingRequiredParameter_ShouldReturnInvalidParamsError()
    {
        // Arrange - resources/read requires 'uri' parameter
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new { }, // Empty params, missing 'uri'
            id = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["error"]);
        Assert.Equal(-32602, jsonResponse["error"]!["code"]?.GetValue<int>());
    }
}

