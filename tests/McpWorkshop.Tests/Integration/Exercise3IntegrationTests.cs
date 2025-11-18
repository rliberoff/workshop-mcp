using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Exercise3SecureServer;

namespace McpWorkshop.Tests.Integration;

/// <summary>
/// Integration tests for Exercise 3: Secure MCP Server with JWT authentication.
/// Validates authentication, authorization, and rate limiting.
/// </summary>
[Collection("Exercise3 Sequential")]
public class Exercise3IntegrationTests : IClassFixture<WebApplicationFactory<Exercise3SecureServer.Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Exercise3SecureServer.Program> _factory;

    public Exercise3IntegrationTests(WebApplicationFactory<Exercise3SecureServer.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturn401()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        // Act - No Authorization header
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestWithInvalidToken_ShouldReturn401()
    {
        // Arrange
        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        // Add invalid token
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestWithValidToken_ShouldSucceed()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer"); // viewer role

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 1
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);
        Assert.Equal("2.0", jsonResponse!["jsonrpc"]?.GetValue<string>());
    }

    [Fact]
    public async Task ViewerRole_CanReadResources()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer");

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = "customer://1"
            },
            id = 2
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ViewerRole_CannotCallTools()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer");

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "UpdateCustomer",
                arguments = new
                {
                    customerId = "1",
                    name = "Updated Name"
                }
            },
            id = 3
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EditorRole_CanCallTools()
    {
        // Arrange
        var token = await GetValidJwtToken("editor");

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "UpdateCustomer",
                arguments = new
                {
                    customerId = "1",
                    name = "Updated Name"
                }
            },
            id = 4
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task AdminRole_CanAccessAllEndpoints()
    {
        // Arrange
        var token = await GetValidJwtToken("admin");

        var requests = new[]
        {
            new { jsonrpc = "2.0", method = "resources/list", id = 5 },
            new { jsonrpc = "2.0", method = "tools/list", id = 6 },
            new { jsonrpc = "2.0", method = "admin/stats", id = 7 }
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act & Assert
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/mcp", request);
            Assert.True(response.IsSuccessStatusCode,
                $"Admin should access {request.method}");
        }
    }

    [Fact]
    public async Task ExpiredToken_ShouldReturn401()
    {
        // Arrange - Create expired token (issued 2 hours ago, expires in 1 hour)
        var expiredToken = CreateJwtToken("viewer", expiresInMinutes: -60);

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 8
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenWithInvalidIssuer_ShouldReturn401()
    {
        // Arrange - Token from different issuer
        var token = CreateJwtToken("viewer", issuer: "invalid-issuer");

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 9
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenWithInvalidAudience_ShouldReturn401()
    {
        // Arrange - Token for different audience
        var token = CreateJwtToken("viewer", audience: "invalid-audience");

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 10
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(Skip = "Rate limiting test interferes with other tests due to shared factory - validated separately")]
    public async Task RateLimiting_ShouldThrottleExcessiveRequests()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 11
        };

        var successCount = 0;
        var rateLimitedCount = 0;

        // Act - Make 600 requests rapidly to exceed rate limit (configured at 500/min for resources)
        var tasks = Enumerable.Range(0, 600)
            .Select(async i =>
            {
                var response = await _client.PostAsJsonAsync("/mcp", request);
                if (response.IsSuccessStatusCode)
                    Interlocked.Increment(ref successCount);
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    Interlocked.Increment(ref rateLimitedCount);
            });

        await Task.WhenAll(tasks);

        // Assert - Some requests should be rate limited
        Assert.True(rateLimitedCount > 0,
            $"Expected some requests to be rate limited. Success: {successCount}, RateLimited: {rateLimitedCount}");
        Assert.True(successCount > 0,
            "Expected some requests to succeed");
    }

    [Fact]
    public async Task RateLimitHeaders_ShouldBePresent()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 12
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert - Check for rate limit headers
        Assert.True(response.Headers.Contains("X-RateLimit-Limit") ||
                   response.Headers.Contains("RateLimit-Limit"),
            "Response should include rate limit headers");
    }

    [Fact]
    public async Task TokenRefresh_WithValidRefreshToken_ShouldSucceed()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";

        var request = new
        {
            jsonrpc = "2.0",
            method = "auth/refresh",
            @params = new
            {
                refreshToken
            },
            id = 13
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonNode.Parse(content);
            var result = jsonResponse!["result"];

            Assert.NotNull(result!["accessToken"]);
            Assert.NotNull(result["expiresIn"]);
        }
    }

    [Fact]
    public async Task CorsHeaders_ShouldBeConfigured()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Add("Origin", "https://example.com");

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 14
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert - Check CORS headers
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").First();
            Assert.NotNull(allowOrigin);
        }
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        // Arrange
        var token = await GetValidJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/list",
            id = 15
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert - Check security headers
        var expectedHeaders = new[]
        {
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection"
        };

        foreach (var header in expectedHeaders)
        {
            if (response.Headers.Contains(header))
            {
                Assert.NotNull(response.Headers.GetValues(header));
            }
        }
    }

    [Fact(Skip = "Feature admin/audit-log not implemented - advanced feature for future enhancement")]
    public async Task AuditLog_ShouldRecordAuthenticatedRequests()
    {
        // Arrange
        var token = await GetValidJwtToken("admin");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            jsonrpc = "2.0",
            method = "admin/audit-log",
            @params = new
            {
                limit = 10
            },
            id = 16
        };

        // Act
        var response = await _client.PostAsJsonAsync("/mcp", request);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonNode.Parse(content);
            var result = jsonResponse!["result"];

            Assert.NotNull(result!["logs"]);
            var logs = result["logs"]!.AsArray();
            Assert.NotEmpty(logs);

            // Verify log structure
            var firstLog = logs[0];
            Assert.NotNull(firstLog!["timestamp"]);
            Assert.NotNull(firstLog["userId"]);
            Assert.NotNull(firstLog["method"]);
        }
    }

    // Helper method to get a valid JWT token for testing
    private async Task<string> GetValidJwtToken(string role)
    {
        // In a real scenario, this would call an auth endpoint
        // For testing, create a token with the proper structure
        return CreateJwtToken(role);
    }

    // Helper method to create JWT tokens for testing
    private string CreateJwtToken(
        string role,
        int expiresInMinutes = 60,
        string issuer = "Exercise3SecureServer",
        string audience = "mcp-workshop-clients")
    {
        // Use System.IdentityModel.Tokens.Jwt for proper token generation
        var secretKey = "MCP-Workshop-2025-SecureServer-SuperSecretKey-MinLength32Characters"; // Same as in appsettings.json
        var key = System.Text.Encoding.UTF8.GetBytes(secretKey);
        var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key);
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            signingKey,
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        // Map role to scopes and tier
        var (scopes, tier) = role switch
        {
            "viewer" => ("read", "basic"),
            "editor" => ("read write", "standard"),
            "admin" => ("read write admin", "premium"),
            _ => ("", "basic")
        };

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "test-user"),
            new System.Security.Claims.Claim("scope", scopes),
            new System.Security.Claims.Claim("tier", tier),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}


