using System.Text.Json;
using Exercise4SqlMcpServer.Models;
using Exercise4CosmosMcpServer.Models;
using Xunit;

namespace McpWorkshop.Tests.Unit.Models;

/// <summary>
/// Unit tests for data models from Exercise4 servers
/// Tests serialization, deserialization, and validation
/// </summary>
public class DataModelsTests
{
    #region SQL Models Tests

    [Fact]
    public void Customer_Serialization_PreservesAllProperties()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+34123456789",
            City = "Madrid",
            Country = "España",
            RegisteredAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(customer);
        var deserialized = JsonSerializer.Deserialize<Customer>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(customer.Id, deserialized.Id);
        Assert.Equal(customer.Name, deserialized.Name);
        Assert.Equal(customer.Email, deserialized.Email);
        Assert.Equal(customer.Phone, deserialized.Phone);
        Assert.Equal(customer.City, deserialized.City);
        Assert.Equal(customer.Country, deserialized.Country);
    }

    [Fact]
    public void Product_Serialization_PreservesAllProperties()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Category = "Electronics",
            Price = 299.99m,
            Stock = 50,
            Description = "A test product"
        };

        // Act
        var json = JsonSerializer.Serialize(product);
        var deserialized = JsonSerializer.Deserialize<Product>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(product.Id, deserialized.Id);
        Assert.Equal(product.Name, deserialized.Name);
        Assert.Equal(product.Category, deserialized.Category);
        Assert.Equal(product.Price, deserialized.Price);
        Assert.Equal(product.Stock, deserialized.Stock);
        Assert.Equal(product.Description, deserialized.Description);
    }

    [Fact]
    public void Order_Serialization_PreservesAllProperties()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            CustomerId = 123,
            ProductId = 456,
            Quantity = 2,
            TotalAmount = 599.98m,
            OrderDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            Status = "Completed"
        };

        // Act
        var json = JsonSerializer.Serialize(order);
        var deserialized = JsonSerializer.Deserialize<Order>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(order.Id, deserialized.Id);
        Assert.Equal(order.CustomerId, deserialized.CustomerId);
        Assert.Equal(order.ProductId, deserialized.ProductId);
        Assert.Equal(order.Quantity, deserialized.Quantity);
        Assert.Equal(order.TotalAmount, deserialized.TotalAmount);
        Assert.Equal(order.Status, deserialized.Status);
    }

    [Fact]
    public void Customer_DefaultValues_AreCorrect()
    {
        // Act
        var customer = new Customer();

        // Assert
        Assert.Equal(0, customer.Id);
        Assert.Equal(string.Empty, customer.Name);
        Assert.Equal(string.Empty, customer.Email);
        Assert.Equal(string.Empty, customer.Phone);
        Assert.Equal(string.Empty, customer.City);
        Assert.Equal(string.Empty, customer.Country);
    }

    [Fact]
    public void Product_DefaultValues_AreCorrect()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.Equal(0, product.Id);
        Assert.Equal(string.Empty, product.Name);
        Assert.Equal(string.Empty, product.Category);
        Assert.Equal(0m, product.Price);
        Assert.Equal(0, product.Stock);
        Assert.Equal(string.Empty, product.Description);
    }

    [Fact]
    public void Order_DefaultStatus_IsConfirmed()
    {
        // Act
        var order = new Order();

        // Assert
        Assert.Equal("Confirmed", order.Status);
    }

    #endregion

    #region Cosmos Models Tests

    [Fact]
    public void UserSession_Serialization_PreservesAllProperties()
    {
        // Arrange
        var session = new UserSession
        {
            UserId = "user-123",
            SessionStart = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            SessionEnd = new DateTime(2024, 1, 1, 11, 0, 0, DateTimeKind.Utc),
            PageViews = 25,
            Actions = 15,
            LastPage = "/checkout"
        };

        // Act
        var json = JsonSerializer.Serialize(session);
        var deserialized = JsonSerializer.Deserialize<UserSession>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(session.UserId, deserialized.UserId);
        Assert.Equal(session.PageViews, deserialized.PageViews);
        Assert.Equal(session.Actions, deserialized.Actions);
        Assert.Equal(session.LastPage, deserialized.LastPage);
    }

    [Fact]
    public void CartEvent_Serialization_PreservesAllProperties()
    {
        // Arrange
        var cartEvent = new CartEvent
        {
            UserId = "user-456",
            ProductId = 789,
            Action = "addToCart",
            Timestamp = new DateTime(2024, 1, 1, 10, 30, 0, DateTimeKind.Utc),
            Quantity = 3
        };

        // Act
        var json = JsonSerializer.Serialize(cartEvent);
        var deserialized = JsonSerializer.Deserialize<CartEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(cartEvent.UserId, deserialized.UserId);
        Assert.Equal(cartEvent.ProductId, deserialized.ProductId);
        Assert.Equal(cartEvent.Action, deserialized.Action);
        Assert.Equal(cartEvent.Quantity, deserialized.Quantity);
    }

    [Fact]
    public void UserSession_DefaultValues_AreCorrect()
    {
        // Act
        var session = new UserSession();

        // Assert
        Assert.Equal(string.Empty, session.UserId);
        Assert.Equal(0, session.PageViews);
        Assert.Equal(0, session.Actions);
        Assert.Equal(string.Empty, session.LastPage);
    }

    [Fact]
    public void CartEvent_DefaultValues_AreCorrect()
    {
        // Act
        var cartEvent = new CartEvent();

        // Assert
        Assert.Equal(string.Empty, cartEvent.UserId);
        Assert.Equal(0, cartEvent.ProductId);
        Assert.Equal(string.Empty, cartEvent.Action);
        Assert.Equal(0, cartEvent.Quantity);
    }

    [Fact]
    public void CartEvent_ValidActions_AreSupported()
    {
        // Arrange & Act
        var addEvent = new CartEvent { Action = "addToCart" };
        var removeEvent = new CartEvent { Action = "removeFromCart" };
        var checkoutEvent = new CartEvent { Action = "checkout" };

        // Assert
        Assert.Equal("addToCart", addEvent.Action);
        Assert.Equal("removeFromCart", removeEvent.Action);
        Assert.Equal("checkout", checkoutEvent.Action);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Customer_WithNullValues_SerializesCorrectly()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = null!,
            Email = null!
        };

        // Act
        var json = JsonSerializer.Serialize(customer);
        var deserialized = JsonSerializer.Deserialize<Customer>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Name);
        Assert.Null(deserialized.Email);
    }

    [Fact]
    public void Order_WithZeroAmount_IsValid()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = 1,
            TotalAmount = 0m
        };

        // Assert
        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void Product_WithNegativeStock_IsAllowed()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Stock = -5 // Backorder scenario
        };

        // Assert
        Assert.Equal(-5, product.Stock);
    }

    #endregion
}
