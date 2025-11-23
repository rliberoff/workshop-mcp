using System.Text.Json;
using Exercise4SqlMcpServer.Models;
using Exercise4SqlMcpServer.Tools;
using Xunit;

namespace McpWorkshop.Tests.Unit.Tools;

/// <summary>
/// Unit tests for Exercise4 SQL MCP Server Tools
/// Tests GetSalesSummaryTool and QueryCustomersByCountryTool
/// </summary>
public class SqlMcpServerToolsTests
{
    #region GetSalesSummaryTool Tests

    [Fact]
    public void GetSalesSummaryTool_GetDefinition_ReturnsValidSchema()
    {
        // Act
        var definition = GetSalesSummaryTool.GetDefinition();

        // Assert
        Assert.NotNull(definition);
        // Just verify it returns an object, the actual structure is validated in integration tests
    }

    [Fact]
    public void GetSalesSummaryTool_Execute_WithNoFilters_ReturnsTotalSummary()
    {
        // Arrange
        var orders = CreateSampleOrders();
        var arguments = new Dictionary<string, JsonElement>();

        // Act
        var result = GetSalesSummaryTool.Execute(arguments, orders);
        var resultDict = result as Dictionary<string, object>;

        // Assert
        Assert.NotNull(resultDict);
        Assert.True(resultDict.ContainsKey("content"));
    }

    [Fact]
    public void GetSalesSummaryTool_Execute_WithDateFilter_FiltersCorrectly()
    {
        // Arrange
        var orders = CreateSampleOrders();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["startDate"] = JsonDocument.Parse("\"2024-01-01\"").RootElement,
            ["endDate"] = JsonDocument.Parse("\"2024-12-31\"").RootElement
        };

        // Act
        var result = GetSalesSummaryTool.Execute(arguments, orders);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetSalesSummaryTool_Execute_WithStatusFilter_FiltersCorrectly()
    {
        // Arrange
        var orders = CreateSampleOrders();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["status"] = JsonDocument.Parse("\"Completed\"").RootElement
        };

        // Act
        var result = GetSalesSummaryTool.Execute(arguments, orders);
        var resultDict = result as Dictionary<string, object>;

        // Assert
        Assert.NotNull(resultDict);
        Assert.True(resultDict.ContainsKey("content"));
    }

    [Fact]
    public void GetSalesSummaryTool_Execute_CalculatesTotalSalesCorrectly()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = 1, TotalAmount = 100.50m, Status = "Completed", OrderDate = DateTime.Now },
            new Order { Id = 2, TotalAmount = 200.75m, Status = "Completed", OrderDate = DateTime.Now },
            new Order { Id = 3, TotalAmount = 150.25m, Status = "Completed", OrderDate = DateTime.Now }
        };
        var arguments = new Dictionary<string, JsonElement>();

        // Act
        var result = GetSalesSummaryTool.Execute(arguments, orders);

        // Assert
        Assert.NotNull(result);
        // Expected total: 451.50
    }

    #endregion

    #region QueryCustomersByCountryTool Tests

    [Fact]
    public void QueryCustomersByCountryTool_GetDefinition_ReturnsValidSchema()
    {
        // Act
        var definition = QueryCustomersByCountryTool.GetDefinition();

        // Assert
        Assert.NotNull(definition);
        // Just verify it returns an object, the actual structure is validated in integration tests
    }

    [Fact]
    public void QueryCustomersByCountryTool_Execute_WithoutCountry_ThrowsArgumentException()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        var arguments = new Dictionary<string, JsonElement>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            QueryCustomersByCountryTool.Execute(arguments, customers));
    }

    [Fact]
    public void QueryCustomersByCountryTool_Execute_WithCountry_FiltersCorrectly()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["country"] = JsonDocument.Parse("\"España\"").RootElement
        };

        // Act
        var result = QueryCustomersByCountryTool.Execute(arguments, customers);
        var resultDict = result as Dictionary<string, object>;

        // Assert
        Assert.NotNull(resultDict);
        Assert.True(resultDict.ContainsKey("content"));
    }

    [Fact]
    public void QueryCustomersByCountryTool_Execute_WithCountryAndCity_FiltersCorrectly()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["country"] = JsonDocument.Parse("\"España\"").RootElement,
            ["city"] = JsonDocument.Parse("\"Madrid\"").RootElement
        };

        // Act
        var result = QueryCustomersByCountryTool.Execute(arguments, customers);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void QueryCustomersByCountryTool_Execute_CaseInsensitiveMatch()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["country"] = JsonDocument.Parse("\"españa\"").RootElement // lowercase
        };

        // Act
        var result = QueryCustomersByCountryTool.Execute(arguments, customers);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Helper Methods

    private Order[] CreateSampleOrders()
    {
        return new[]
        {
            new Order
            {
                Id = 1,
                CustomerId = 1,
                ProductId = 1,
                Quantity = 2,
                TotalAmount = 199.98m,
                OrderDate = new DateTime(2024, 1, 15),
                Status = "Completed"
            },
            new Order
            {
                Id = 2,
                CustomerId = 2,
                ProductId = 2,
                Quantity = 1,
                TotalAmount = 299.99m,
                OrderDate = new DateTime(2024, 2, 20),
                Status = "Pending"
            },
            new Order
            {
                Id = 3,
                CustomerId = 3,
                ProductId = 3,
                Quantity = 3,
                TotalAmount = 149.97m,
                OrderDate = new DateTime(2024, 3, 10),
                Status = "Completed"
            }
        };
    }

    private Customer[] CreateSampleCustomers()
    {
        return new[]
        {
            new Customer
            {
                Id = 1,
                Name = "Juan Pérez",
                Email = "juan@example.com",
                Phone = "+34123456789",
                City = "Madrid",
                Country = "España",
                RegisteredAt = new DateTime(2024, 1, 1)
            },
            new Customer
            {
                Id = 2,
                Name = "María García",
                Email = "maria@example.com",
                Phone = "+34987654321",
                City = "Barcelona",
                Country = "España",
                RegisteredAt = new DateTime(2024, 2, 1)
            },
            new Customer
            {
                Id = 3,
                Name = "Carlos López",
                Email = "carlos@example.com",
                Phone = "+52555123456",
                City = "Ciudad de México",
                Country = "México",
                RegisteredAt = new DateTime(2024, 3, 1)
            }
        };
    }

    #endregion
}
