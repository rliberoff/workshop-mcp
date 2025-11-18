using Exercise2ParametricQuery.Models;
using System.Text.Json;

namespace Exercise2ParametricQuery.Tools;

public static class AggregateSalesTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "CalculateTotal",
            description = "Calcular el total de pedidos específicos por sus IDs",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    orderIds = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Lista de IDs de pedidos para calcular el total"
                    }
                },
                required = new[] { "orderIds" }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Order[] allOrders)
    {
        if (!arguments.TryGetValue("orderIds", out var orderIdsElement))
        {
            // Si no hay orderIds, retornar total cero
            return new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = JsonSerializer.Serialize(new { total = 0m, currency = "USD", orderCount = 0 })
                    }
                }
            };
        }

        // Convertir orderIds de string[] a int[]
        var orderIds = new List<int>();
        foreach (var element in orderIdsElement.EnumerateArray())
        {
            var idStr = element.GetString();
            if (int.TryParse(idStr, out var id))
            {
                orderIds.Add(id);
            }
        }

        // Filtrar pedidos por IDs
        var selectedOrders = allOrders.Where(o => orderIds.Contains(o.Id)).ToArray();
        var total = selectedOrders.Sum(o => o.TotalAmount);

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = JsonSerializer.Serialize(new 
                    { 
                        total, 
                        currency = "USD", 
                        orderCount = selectedOrders.Length,
                        orderIds = selectedOrders.Select(o => o.Id).ToArray()
                    })
                }
            },
            data = new
            {
                total,
                currency = "USD",
                orderCount = selectedOrders.Length,
                orders = selectedOrders
            }
        };
    }
}
