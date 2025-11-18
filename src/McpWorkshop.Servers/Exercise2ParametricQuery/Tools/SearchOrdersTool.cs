using Exercise2ParametricQuery.Models;
using System.Text.Json;

namespace Exercise2ParametricQuery.Tools;

public static class SearchOrdersTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "SearchOrders",
            description = "Buscar pedidos por cliente, producto, rango de fechas o total mínimo",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    customerId = new
                    {
                        type = "integer",
                        description = "ID del cliente"
                    },
                    productId = new
                    {
                        type = "integer",
                        description = "ID del producto"
                    },
                    minTotal = new
                    {
                        type = "number",
                        description = "Monto total mínimo"
                    },
                    startDate = new
                    {
                        type = "string",
                        description = "Fecha de inicio (formato: yyyy-MM-dd)"
                    },
                    endDate = new
                    {
                        type = "string",
                        description = "Fecha de fin (formato: yyyy-MM-dd)"
                    }
                }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Order[] allOrders)
    {
        var orders = allOrders.AsEnumerable();

        if (arguments.TryGetValue("customerId", out var customerIdElement))
        {
            int customerId;
            if (customerIdElement.ValueKind == JsonValueKind.String)
            {
                var customerIdStr = customerIdElement.GetString();
                if (!string.IsNullOrEmpty(customerIdStr) && int.TryParse(customerIdStr, out customerId))
                {
                    orders = orders.Where(o => o.CustomerId == customerId);
                }
            }
            else if (customerIdElement.ValueKind == JsonValueKind.Number && customerIdElement.TryGetInt32(out customerId))
            {
                orders = orders.Where(o => o.CustomerId == customerId);
            }
        }

        if (arguments.TryGetValue("productId", out var productIdElement))
        {
            int productId;
            if (productIdElement.ValueKind == JsonValueKind.String)
            {
                var productIdStr = productIdElement.GetString();
                if (!string.IsNullOrEmpty(productIdStr) && int.TryParse(productIdStr, out productId))
                {
                    orders = orders.Where(o => o.ProductId == productId);
                }
            }
            else if (productIdElement.ValueKind == JsonValueKind.Number && productIdElement.TryGetInt32(out productId))
            {
                orders = orders.Where(o => o.ProductId == productId);
            }
        }

        if (arguments.TryGetValue("minTotal", out var minTotalElement))
        {
            if (minTotalElement.TryGetDecimal(out var minTotal))
            {
                orders = orders.Where(o => o.TotalAmount >= minTotal);
            }
        }

        if (arguments.TryGetValue("startDate", out var startDateElement))
        {
            var startDateStr = startDateElement.GetString();
            if (!string.IsNullOrEmpty(startDateStr) && DateTime.TryParse(startDateStr, out var startDate))
            {
                orders = orders.Where(o => o.OrderDate >= startDate);
            }
        }

        if (arguments.TryGetValue("endDate", out var endDateElement))
        {
            var endDateStr = endDateElement.GetString();
            if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out var endDate))
            {
                orders = orders.Where(o => o.OrderDate <= endDate);
            }
        }

        var result = orders.ToArray();

        var responseData = new
        {
            orders = result,
            total = result.Length
        };

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = System.Text.Json.JsonSerializer.Serialize(responseData)
                }
            },
            data = result
        };
    }
}
