using McpWorkshop.Shared.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace McpWorkshop.Tests.Unit.Security;

/// <summary>
/// Unit tests for SecurityHeadersMiddleware
/// Tests security header injection in HTTP responses
/// </summary>
public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Middleware_AddsXContentTypeOptionsHeader()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
    }

    [Fact]
    public async Task Middleware_AddsXFrameOptionsHeader()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
    }

    [Fact]
    public async Task Middleware_AddsXXssProtectionHeader()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        Assert.Equal("1; mode=block", response.Headers.GetValues("X-XSS-Protection").First());
    }

    [Fact]
    public async Task Middleware_AddsReferrerPolicyHeader()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());
    }

    [Fact]
    public async Task Middleware_AddsContentSecurityPolicyHeader()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        Assert.Equal("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").First());
    }

    [Fact]
    public async Task Middleware_AddsAllSecurityHeaders()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert - Verify all 5 security headers are present
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task Middleware_ExecutesBeforeEndpoint()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Headers should be added before endpoint execution
        Assert.Equal("test-response", content);
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
    }

    [Fact]
    public async Task Middleware_DoesNotModifyResponseBody()
    {
        // Arrange
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal("test-response", content);
    }

    private async Task<IHost> CreateTestHost()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => { });
                webHost.Configure(app =>
                {
                    app.UseSecurityHeaders();
                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("test-response");
                    });
                });
            });

        return await hostBuilder.StartAsync();
    }
}
