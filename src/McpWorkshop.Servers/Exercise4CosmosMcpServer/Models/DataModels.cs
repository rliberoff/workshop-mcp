using System;

namespace Exercise4CosmosMcpServer.Models;

public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public DateTime SessionStart { get; set; }
    public DateTime SessionEnd { get; set; }
    public int PageViews { get; set; }
    public int Actions { get; set; }
    public string LastPage { get; set; } = string.Empty;
}

public class CartEvent
{
    public string UserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string Action { get; set; } = string.Empty; // addToCart, removeFromCart, checkout
    public DateTime Timestamp { get; set; }
    public int Quantity { get; set; }
}
