using McpWorkshop.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace McpWorkshop.Tests.Unit.Configuration;

/// <summary>
/// Unit tests for WorkshopSettings and related configuration classes
/// Tests configuration binding and validation
/// </summary>
public class WorkshopSettingsTests
{
    [Fact]
    public void WorkshopSettings_DefaultConstructor_InitializesAllProperties()
    {
        // Act
        var settings = new WorkshopSettings();

        // Assert
        Assert.NotNull(settings.Server);
        Assert.NotNull(settings.Security);
        Assert.NotNull(settings.Performance);
        Assert.NotNull(settings.DataSources);
    }

    [Fact]
    public void ServerInfo_DefaultValues_AreCorrect()
    {
        // Act
        var serverInfo = new ServerInfo();

        // Assert
        Assert.Equal("mcp-workshop-server", serverInfo.Name);
        Assert.Equal("1.0.0", serverInfo.Version);
        Assert.Equal("2024-11-05", serverInfo.ProtocolVersion);
        Assert.Equal(5000, serverInfo.Port);
        Assert.Equal("localhost", serverInfo.Host);
    }

    [Fact]
    public void SecuritySettings_DefaultValues_DisablesAuthByDefault()
    {
        // Act
        var security = new SecuritySettings();

        // Assert
        Assert.False(security.RequireAuthentication);
        Assert.Equal(string.Empty, security.JwtSecret);
        Assert.Equal("mcp-workshop", security.JwtIssuer);
        Assert.Equal("mcp-client", security.JwtAudience);
        Assert.Equal(60, security.JwtExpirationMinutes);
    }

    [Fact]
    public void RateLimitSettings_DefaultValues_DisablesRateLimiting()
    {
        // Act
        var rateLimit = new RateLimitSettings();

        // Assert
        Assert.False(rateLimit.Enabled);
        Assert.Equal(100, rateLimit.ResourcesPerMinute);
        Assert.Equal(50, rateLimit.ToolsPerMinute);
        Assert.Equal(10, rateLimit.UnauthenticatedPerMinute);
    }

    [Fact]
    public void PerformanceSettings_DefaultValues_AreReasonable()
    {
        // Act
        var performance = new PerformanceSettings();

        // Assert
        Assert.Equal(500, performance.ResourceResponseTimeMs);
        Assert.Equal(1000, performance.ToolResponseTimeMs);
        Assert.Equal(50, performance.MaxLoggingOverheadMs);
        Assert.True(performance.EnablePerformanceTracking);
    }

    [Fact]
    public void DataSourceSettings_DefaultValues_AreEmpty()
    {
        // Act
        var dataSources = new DataSourceSettings();

        // Assert
        Assert.Equal(string.Empty, dataSources.SqlConnectionString);
        Assert.Equal(string.Empty, dataSources.CosmosConnectionString);
        Assert.Equal("workshop", dataSources.CosmosDatabase);
        Assert.Equal(string.Empty, dataSources.BlobStorageConnectionString);
        Assert.Equal("../../../data", dataSources.LocalDataPath);
    }

    [Fact]
    public void WorkshopSettings_BindsFromConfiguration()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Workshop:Server:Name"] = "test-server",
            ["Workshop:Server:Port"] = "6000",
            ["Workshop:Security:RequireAuthentication"] = "true",
            ["Workshop:Security:JwtSecret"] = "test-secret",
            ["Workshop:Performance:ResourceResponseTimeMs"] = "300"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var settings = new WorkshopSettings();
        configuration.GetSection(WorkshopSettings.SectionName).Bind(settings);

        // Assert
        Assert.Equal("test-server", settings.Server.Name);
        Assert.Equal(6000, settings.Server.Port);
        Assert.True(settings.Security.RequireAuthentication);
        Assert.Equal("test-secret", settings.Security.JwtSecret);
        Assert.Equal(300, settings.Performance.ResourceResponseTimeMs);
    }

    [Fact]
    public void WorkshopSettings_SectionName_IsCorrect()
    {
        // Assert
        Assert.Equal("Workshop", WorkshopSettings.SectionName);
    }

    [Fact]
    public void SecuritySettings_RateLimit_IsNotNull()
    {
        // Act
        var security = new SecuritySettings();

        // Assert
        Assert.NotNull(security.RateLimit);
    }

    [Fact]
    public void PerformanceSettings_ValidatesReasonableValues()
    {
        // Arrange
        var performance = new PerformanceSettings
        {
            ResourceResponseTimeMs = 100,
            ToolResponseTimeMs = 500,
            MaxLoggingOverheadMs = 10
        };

        // Assert - Values should be positive
        Assert.True(performance.ResourceResponseTimeMs > 0);
        Assert.True(performance.ToolResponseTimeMs > 0);
        Assert.True(performance.MaxLoggingOverheadMs > 0);

        // Tool response time should be >= resource response time
        Assert.True(performance.ToolResponseTimeMs >= performance.ResourceResponseTimeMs);
    }

    [Fact]
    public void DataSourceSettings_WithCustomValues_PreservesValues()
    {
        // Arrange
        var dataSources = new DataSourceSettings
        {
            SqlConnectionString = "Server=localhost;Database=test;",
            CosmosConnectionString = "AccountEndpoint=https://test;",
            CosmosDatabase = "test-db",
            LocalDataPath = "./custom-data"
        };

        // Assert
        Assert.Equal("Server=localhost;Database=test;", dataSources.SqlConnectionString);
        Assert.Equal("AccountEndpoint=https://test;", dataSources.CosmosConnectionString);
        Assert.Equal("test-db", dataSources.CosmosDatabase);
        Assert.Equal("./custom-data", dataSources.LocalDataPath);
    }
}
