using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Exercise2ParametricQuery;

namespace McpWorkshop.Tests.Integration;

/// <summary>
/// Integration tests for Exercise 2: Parametric Query Tools MCP Server.
/// Validates tools/list and tools/call endpoints with GetCustomers, SearchOrders, and CalculateTotal tools.
/// </summary>
public class Exercise2IntegrationTests : IClassFixture<WebApplicationFactory<Exercise2ParametricQuery.Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Exercise2ParametricQuery.Program> _factory;

    public Exercise2IntegrationTests(WebApplicationFactory<Exercise2ParametricQuery.Program> factory)
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

        var serverInfo = jsonResponse["result"]!["serverInfo"];
        Assert.Equal("exercise2-parametric-query", serverInfo!["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task ToolsList_ShouldReturnAllAvailableTools()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
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
        Assert.NotNull(jsonResponse["result"]!["tools"]);

        var tools = jsonResponse["result"]!["tools"]!.AsArray();
        Assert.NotEmpty(tools);

        // Verify each tool has required fields
        foreach (var tool in tools)
        {
            Assert.NotNull(tool!["name"]);
            Assert.NotNull(tool["description"]);
            Assert.NotNull(tool["inputSchema"]);
        }
    }

    [Fact]
    public async Task ToolsList_ShouldIncludeExpectedTools()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
            id = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        var tools = jsonResponse!["result"]!["tools"]!.AsArray();
        var toolNames = tools.Select(t => t!["name"]?.GetValue<string>()).ToList();

        // Assert - Verify all 3 expected tools are present
        Assert.Contains("GetCustomers", toolNames);
        Assert.Contains("SearchOrders", toolNames);
        Assert.Contains("CalculateTotal", toolNames);
    }

    [Fact]
    public async Task GetCustomersTool_ShouldHaveValidInputSchema()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
            id = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        var tools = jsonResponse!["result"]!["tools"]!.AsArray();
        var getCustomersTool = tools.FirstOrDefault(t => t!["name"]?.GetValue<string>() == "GetCustomers");

        // Assert
        Assert.NotNull(getCustomersTool);
        var inputSchema = getCustomersTool!["inputSchema"];
        Assert.NotNull(inputSchema);
        Assert.Equal("object", inputSchema!["type"]?.GetValue<string>());

        var properties = inputSchema["properties"];
        Assert.NotNull(properties);
        Assert.NotNull(properties!["page"]);
        Assert.NotNull(properties["pageSize"]);
    }

    [Fact]
    public async Task GetCustomersToolCall_WithDefaultParameters_ShouldReturnCustomers()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetCustomers",
                arguments = new
                {
                    page = 1,
                    pageSize = 10
                }
            },
            id = 3
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
        Assert.NotNull(jsonResponse["result"]!["content"]);

        var contentArray = jsonResponse["result"]!["content"]!.AsArray();
        Assert.NotEmpty(contentArray);

        var firstContent = contentArray[0];
        Assert.Equal("text", firstContent!["type"]?.GetValue<string>());
        Assert.NotNull(firstContent["text"]);
    }

    [Fact]
    public async Task GetCustomersToolCall_WithPagination_ShouldReturnCorrectPageSize()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetCustomers",
                arguments = new
                {
                    page = 1,
                    pageSize = 5
                }
            },
            id = 4
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        var contentArray = jsonResponse!["result"]!["content"]!.AsArray();
        var textContent = contentArray[0]!["text"]?.GetValue<string>();

        // Parse the JSON response text
        var customersData = JsonNode.Parse(textContent!);
        var customers = customersData!["customers"]!.AsArray();

        // Assert
        Assert.True(customers.Count <= 5, $"Expected <= 5 customers, got {customers.Count}");
    }

    [Fact]
    public async Task SearchOrdersTool_WithValidParameters_ShouldReturnOrders()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "SearchOrders",
                arguments = new
                {
                    customerId = "1",
                    status = "completed"
                }
            },
            id = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["result"]);

        var contentArray = jsonResponse["result"]!["content"]!.AsArray();
        var textContent = contentArray[0]!["text"]?.GetValue<string>();

        // Verify it's valid JSON
        var ordersData = JsonNode.Parse(textContent!);
        Assert.NotNull(ordersData);
    }

    [Fact]
    public async Task SearchOrdersTool_WithDateRange_ShouldFilterCorrectly()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "SearchOrders",
                arguments = new
                {
                    startDate = "2024-01-01",
                    endDate = "2024-12-31"
                }
            },
            id = 6
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["result"]);

        var contentArray = jsonResponse["result"]!["content"]!.AsArray();
        Assert.NotEmpty(contentArray);
    }

    [Fact]
    public async Task CalculateTotalTool_WithValidOrderIds_ShouldReturnTotal()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "CalculateTotal",
                arguments = new
                {
                    orderIds = new[] { "1", "2", "3" }
                }
            },
            id = 7
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["result"]);

        var contentArray = jsonResponse["result"]!["content"]!.AsArray();
        var textContent = contentArray[0]!["text"]?.GetValue<string>();

        var totalData = JsonNode.Parse(textContent!);
        Assert.NotNull(totalData!["total"]);
        Assert.NotNull(totalData["currency"]);
    }

    [Fact]
    public async Task CalculateTotalTool_WithEmptyOrderIds_ShouldReturnZero()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "CalculateTotal",
                arguments = new
                {
                    orderIds = Array.Empty<string>()
                }
            },
            id = 8
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        var contentArray = jsonResponse!["result"]!["content"]!.AsArray();
        var textContent = contentArray[0]!["text"]?.GetValue<string>();

        var totalData = JsonNode.Parse(textContent!);
        var total = totalData!["total"]?.GetValue<decimal>();

        // Assert
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task ToolsCall_WithInvalidToolName_ShouldReturnError()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "NonExistentTool",
                arguments = new { }
            },
            id = 9
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["error"]);
        Assert.Null(jsonResponse["result"]);
    }

    // NOTE: Test removed - GetCustomers has no required parameters
    // All parameters (name, country, page, pageSize) are optional with defaults
    // Calling with empty arguments {} returns all customers (valid behavior)

    [Fact]
    public async Task ToolsCall_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetCustomers",
                arguments = new
                {
                    page = 1,
                    pageSize = 10
                }
            },
            id = 11
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        stopwatch.Stop();

        // Assert - Should respond within 1000ms (p95 target for tool execution)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"tools/call took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public async Task Capabilities_ShouldIndicateToolsSupport()
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
        Assert.NotNull(capabilities!["tools"]);
    }

    [Fact]
    public async Task MultipleToolCalls_ShouldBeIndependent()
    {
        // Arrange - Call the same tool twice with different parameters
        var request1 = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetCustomers",
                arguments = new
                {
                    page = 1,
                    pageSize = 5
                }
            },
            id = 12
        };

        var request2 = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetCustomers",
                arguments = new
                {
                    page = 2,
                    pageSize = 10
                }
            },
            id = 13
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/mcp", request1);
        var content1 = await response1.Content.ReadAsStringAsync();
        var json1 = JsonNode.Parse(content1);

        var response2 = await _client.PostAsJsonAsync("/mcp", request2);
        var content2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonNode.Parse(content2);

        // Assert - Both should succeed but return different results
        Assert.NotNull(json1!["result"]);
        Assert.NotNull(json2!["result"]);
        Assert.NotEqual(json1["result"]!.ToJsonString(), json2["result"]!.ToJsonString());
    }

    [Fact]
    public async Task ToolInputSchema_ShouldDefineParameterTypes()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
            id = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        var tools = jsonResponse!["result"]!["tools"]!.AsArray();

        // Assert - Each tool should have properly typed parameters in inputSchema
        foreach (var tool in tools)
        {
            var inputSchema = tool!["inputSchema"];
            Assert.NotNull(inputSchema);
            Assert.Equal("object", inputSchema!["type"]?.GetValue<string>());
            Assert.NotNull(inputSchema["properties"]);

            var properties = inputSchema["properties"]!.AsObject();
            foreach (var property in properties)
            {
                var propValue = property.Value;
                Assert.NotNull(propValue!["type"]); // Each property should have a type
            }
        }
    }
}

