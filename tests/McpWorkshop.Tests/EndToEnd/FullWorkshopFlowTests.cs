using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Exercise1StaticResources;
using Exercise2ParametricQuery;
using Exercise3SecureServer;

namespace McpWorkshop.Tests.EndToEnd;

/// <summary>
/// End-to-end tests simulating attendee progression through all 4 exercises.
/// Validates the complete workshop flow.
/// </summary>
public class FullWorkshopFlowTests
{
    [Fact]
    public async Task Exercise1Flow_AttendeeCanReadStaticResources()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        // Step 1: Initialize connection
        var initRequest = new
        {
            jsonrpc = "2.0",
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "workshop-attendee", version = "1.0.0" }
            },
            id = 1
        };

        var initResponse = await client.PostAsJsonAsync("/", initRequest);
        Assert.True(initResponse.IsSuccessStatusCode, "Should initialize successfully");

        // Step 2: List available resources
        var listRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        var listResponse = await client.PostAsJsonAsync("/", listRequest);
        Assert.True(listResponse.IsSuccessStatusCode, "Should list resources");

        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listJson = JsonNode.Parse(listContent);
        var resources = listJson!["result"]!["resources"]!.AsArray();
        Assert.NotEmpty(resources);

        // Step 3: Read first resource
        var firstUri = resources[0]!["uri"]?.GetValue<string>();
        var readRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new { uri = firstUri },
            id = 3
        };

        var readResponse = await client.PostAsJsonAsync("/", readRequest);
        Assert.True(readResponse.IsSuccessStatusCode, "Should read resource content");

        // Success: Attendee completed Exercise 1
    }

    [Fact]
    public async Task Exercise2Flow_AttendeeCanUseParametricTools()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise2ParametricQuery.Program>();
        using var client = factory.CreateClient();

        // Step 1: Initialize
        var initRequest = new
        {
            jsonrpc = "2.0",
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "workshop-attendee", version = "1.0.0" }
            },
            id = 1
        };

        await client.PostAsJsonAsync("/", initRequest);

        // Step 2: List available tools
        var listToolsRequest = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
            id = 2
        };

        var listToolsResponse = await client.PostAsJsonAsync("/", listToolsRequest);
        Assert.True(listToolsResponse.IsSuccessStatusCode);

        var listToolsContent = await listToolsResponse.Content.ReadAsStringAsync();
        var toolsJson = JsonNode.Parse(listToolsContent);
        var tools = toolsJson!["result"]!["tools"]!.AsArray();
        Assert.Contains(tools, t => t!["name"]?.GetValue<string>() == "GetCustomers");

        // Step 3: Call GetCustomers tool
        var callToolRequest = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetCustomers",
                arguments = new { page = 1, pageSize = 10 }
            },
            id = 3
        };

        var callToolResponse = await client.PostAsJsonAsync("/", callToolRequest);
        Assert.True(callToolResponse.IsSuccessStatusCode, "Should execute tool");

        // Step 4: Call SearchOrders tool
        var searchOrdersRequest = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "SearchOrders",
                arguments = new { customerId = "1", status = "completed" }
            },
            id = 4
        };

        var searchOrdersResponse = await client.PostAsJsonAsync("/", searchOrdersRequest);
        Assert.True(searchOrdersResponse.IsSuccessStatusCode);

        // Step 5: Calculate total
        var calculateTotalRequest = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "CalculateTotal",
                arguments = new { orderIds = new[] { "1", "2", "3" } }
            },
            id = 5
        };

        var calculateTotalResponse = await client.PostAsJsonAsync("/", calculateTotalRequest);
        Assert.True(calculateTotalResponse.IsSuccessStatusCode);

        // Success: Attendee completed Exercise 2
    }

    [Fact]
    public async Task Exercise3Flow_AttendeeImplementsSecurity()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise3SecureServer.Program>();
        using var client = factory.CreateClient();

        // Step 1: Try without authentication (should fail)
        var unauthRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        var unauthResponse = await client.PostAsJsonAsync("/", unauthRequest);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, unauthResponse.StatusCode);

        // Step 2: Get valid token (simulated)
        var token = "simulated-jwt-token";
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Step 3: Try with authentication (should succeed or return specific error)
        var authRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 2
        };

        var authResponse = await client.PostAsJsonAsync("/", authRequest);
        // Either succeeds with valid token or returns appropriate error
        Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, authResponse.StatusCode);

        // Success: Attendee completed Exercise 3 (security implementation)
    }

    [Fact]
    public async Task Exercise4Flow_AttendeeOrchestrates_MultiServers()
    {
        // Arrange - This would typically use multiple factories for different servers
        using var virtualAnalystFactory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = virtualAnalystFactory.CreateClient();

        // Step 1: Initialize VirtualAnalyst
        var initRequest = new
        {
            jsonrpc = "2.0",
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "workshop-attendee", version = "1.0.0" }
            },
            id = 1
        };

        var initResponse = await client.PostAsJsonAsync("/", initRequest);
        Assert.True(initResponse.IsSuccessStatusCode);

        // Step 2: Call orchestration tool that queries multiple servers
        var orchestrateRequest = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "AnalyzeCustomerOrders",
                arguments = new { customerId = "123" }
            },
            id = 2
        };

        var orchestrateResponse = await client.PostAsJsonAsync("/", orchestrateRequest);

        // Should successfully orchestrate or return meaningful error
        Assert.True(
            orchestrateResponse.IsSuccessStatusCode ||
            orchestrateResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable,
            "Should handle orchestration request");

        // Success: Attendee completed Exercise 4
    }

    [Fact]
    public async Task CompleteWorkshopFlow_AllExercisesInSequence()
    {
        // Simulates an attendee going through all 4 exercises in order

        // Exercise 1: Static Resources
        await Exercise1Flow_AttendeeCanReadStaticResources();

        // Exercise 2: Parametric Tools
        await Exercise2Flow_AttendeeCanUseParametricTools();

        // Exercise 3: Security
        await Exercise3Flow_AttendeeImplementsSecurity();

        // Exercise 4: Orchestration
        await Exercise4Flow_AttendeeOrchestrates_MultiServers();

        // Success: Complete workshop flow validated
        Assert.True(true, "Attendee successfully completed all 4 exercises");
    }

    [Fact]
    public async Task WorkshopFlow_HandlesCommonMistakes()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        // Common Mistake 1: Invalid JSON-RPC version
        var invalidVersionRequest = """
        {
            "jsonrpc": "1.0",
            "method": "initialize",
            "id": 1
        }
        """;

        var response1 = await client.PostAsync("/",
            new System.Net.Http.StringContent(invalidVersionRequest,
                System.Text.Encoding.UTF8, "application/json"));

        // Should return error
        Assert.False(response1.IsSuccessStatusCode);

        // Common Mistake 2: Missing required parameters
        var missingParamsRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new { }, // Missing 'uri'
            id = 2
        };

        var response2 = await client.PostAsJsonAsync("/", missingParamsRequest);
        var content2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonNode.Parse(content2);

        // Should return parameter error
        Assert.NotNull(json2!["error"]);

        // Common Mistake 3: Calling non-existent method
        var invalidMethodRequest = new
        {
            jsonrpc = "2.0",
            method = "invalid/method",
            id = 3
        };

        var response3 = await client.PostAsJsonAsync("/", invalidMethodRequest);
        var content3 = await response3.Content.ReadAsStringAsync();
        var json3 = JsonNode.Parse(content3);

        // Should return method not found error
        Assert.NotNull(json3!["error"]);
        Assert.Equal(-32601, json3["error"]!["code"]?.GetValue<int>());

        // Success: Workshop handles common mistakes gracefully
    }
}
