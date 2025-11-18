using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Exercise4CosmosMcpServer.Models;

namespace Exercise4CosmosMcpServer.Tools;

public class GetAbandonedCartsTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "get_abandoned_carts",
            description = "Obtener carritos abandonados en las últimas N horas",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["hours"] = new Dictionary<string, object>
                    {
                        ["type"] = "number",
                        ["description"] = "Número de horas hacia atrás para buscar",
                        ["default"] = 24
                    }
                }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, CartEvent[] cartEvents)
    {
        var hours = 24;
        if (arguments.ContainsKey("hours") && arguments["hours"].ValueKind == JsonValueKind.Number)
        {
            hours = arguments["hours"].GetInt32();
        }

        var cutoffTime = DateTime.UtcNow.AddHours(-hours);

        // Find users who added items but never checked out
        var userActivity = cartEvents
            .Where(e => e.Timestamp >= cutoffTime)
            .GroupBy(e => e.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                HasAddToCart = g.Any(e => e.Action == "addToCart"),
                HasCheckout = g.Any(e => e.Action == "checkout"),
                LastActivity = g.Max(e => e.Timestamp),
                ProductIds = g.Where(e => e.Action == "addToCart").Select(e => e.ProductId).Distinct().ToList()
            })
            .Where(u => u.HasAddToCart && !u.HasCheckout)
            .ToList();

        object textContent = new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = $"🛒 CARRITOS ABANDONADOS (últimas {hours} horas)\n\n" +
                       $"Total usuarios: {userActivity.Count}\n\n" +
                       $"Detalles:\n" +
                       string.Join("\n", userActivity.Select(u =>
                           $"- {u.UserId}: {u.ProductIds.Count} producto(s), última actividad {u.LastActivity:yyyy-MM-dd HH:mm}"))
        };

        object resourceContent = new Dictionary<string, object>
        {
            ["type"] = "resource",
            ["resource"] = new Dictionary<string, object>
            {
                ["uri"] = $"cosmos://analytics/abandoned-carts?hours={hours}",
                ["mimeType"] = "application/json",
                ["text"] = JsonSerializer.Serialize(userActivity)
            }
        };

        return new Dictionary<string, object>
        {
            ["content"] = new[] { textContent, resourceContent }
        };
    }
}
