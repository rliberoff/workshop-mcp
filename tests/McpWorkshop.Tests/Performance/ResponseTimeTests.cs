using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Exercise1StaticResources;
using Exercise2ParametricQuery;
using Exercise3SecureServer;

namespace McpWorkshop.Tests.Performance;

/// <summary>
/// Performance tests to validate response time requirements.
/// Target: 500ms p95 for resource reads, &lt;1000ms for tool execution.
/// </summary>
public class ResponseTimeTests
{
    [Fact]
    public async Task ResourceRead_P95_ShouldBeFasterThan500ms()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var listRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        // Get first resource URI
        var listResponse = await client.PostAsJsonAsync("/", listRequest);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listJson = JsonNode.Parse(listContent);
        var resources = listJson!["result"]!["resources"]!.AsArray();
        var uri = resources[0]!["uri"]?.GetValue<string>();

        var readRequest = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new { uri },
            id = 2
        };

        var measurements = new List<long>();
        const int iterations = 100;

        // Act - Measure 100 requests
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/", readRequest);
            sw.Stop();

            response.EnsureSuccessStatusCode();
            measurements.Add(sw.ElapsedMilliseconds);
        }

        // Calculate p95
        measurements.Sort();
        var p95Index = (int)Math.Ceiling(0.95 * measurements.Count) - 1;
        var p95 = measurements[p95Index];

        // Assert
        Assert.True(p95 < 500,
            $"P95 response time was {p95}ms, expected < 500ms. " +
            $"Min: {measurements.Min()}ms, Max: {measurements.Max()}ms, " +
            $"Avg: {measurements.Average():F2}ms");
    }

    [Fact]
    public async Task ToolExecution_P95_ShouldBeFasterThan1000ms()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise2ParametricQuery.Program>();
        using var client = factory.CreateClient();

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
            id = 1
        };

        var measurements = new List<long>();
        const int iterations = 100;

        // Act - Measure 100 tool executions
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/", request);
            sw.Stop();

            response.EnsureSuccessStatusCode();
            measurements.Add(sw.ElapsedMilliseconds);
        }

        // Calculate p95
        measurements.Sort();
        var p95Index = (int)Math.Ceiling(0.95 * measurements.Count) - 1;
        var p95 = measurements[p95Index];

        // Assert
        Assert.True(p95 < 1000,
            $"P95 tool execution time was {p95}ms, expected < 1000ms. " +
            $"Min: {measurements.Min()}ms, Max: {measurements.Max()}ms, " +
            $"Avg: {measurements.Average():F2}ms");
    }

    [Fact]
    public async Task Initialize_ShouldBeInstant()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

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

        var sw = Stopwatch.StartNew();

        // Act
        var response = await client.PostAsJsonAsync("/", request);
        sw.Stop();

        // Assert - Initialize should be very fast (< 100ms)
        response.EnsureSuccessStatusCode();
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"Initialize took {sw.ElapsedMilliseconds}ms, expected < 100ms");
    }

    [Fact]
    public async Task ResourcesList_ShouldBeInstant()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        var measurements = new List<long>();

        // Act - Measure 50 requests
        for (int i = 0; i < 50; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/", request);
            sw.Stop();

            response.EnsureSuccessStatusCode();
            measurements.Add(sw.ElapsedMilliseconds);
        }

        var avg = measurements.Average();
        var max = measurements.Max();

        // Assert - List operations should be very fast
        Assert.True(avg < 100, $"Average time was {avg:F2}ms, expected < 100ms");
        Assert.True(max < 200, $"Max time was {max}ms, expected < 200ms");
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldMaintainPerformance()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        var measurements = new System.Collections.Concurrent.ConcurrentBag<long>();
        const int concurrentRequests = 50;

        // Act - Make 50 concurrent requests
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(async i =>
            {
                var sw = Stopwatch.StartNew();
                var response = await client.PostAsJsonAsync("/", request);
                sw.Stop();

                response.EnsureSuccessStatusCode();
                measurements.Add(sw.ElapsedMilliseconds);
            });

        await Task.WhenAll(tasks);

        var sortedMeasurements = measurements.OrderBy(x => x).ToList();
        var p95Index = (int)Math.Ceiling(0.95 * sortedMeasurements.Count) - 1;
        var p95 = sortedMeasurements[p95Index];

        // Assert - Performance should not degrade significantly under concurrent load
        Assert.True(p95 < 500,
            $"P95 under concurrent load was {p95}ms, expected < 500ms");
    }

    [Fact]
    public async Task MemoryUsage_ShouldBeReasonable()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        var initialMemory = GC.GetTotalMemory(true);

        // Act - Make 1000 requests to check for memory leaks
        for (int i = 0; i < 1000; i++)
        {
            var response = await client.PostAsJsonAsync("/", request);
            response.EnsureSuccessStatusCode();

            // Read and discard content
            await response.Content.ReadAsStringAsync();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);

        // Assert - Memory increase should be reasonable (< 50MB)
        Assert.True(memoryIncreaseMB < 50,
            $"Memory increased by {memoryIncreaseMB:F2}MB after 1000 requests, expected < 50MB");
    }
}
