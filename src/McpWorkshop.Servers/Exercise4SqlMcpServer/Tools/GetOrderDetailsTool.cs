using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Exercise4SqlMcpServer.Models;

namespace Exercise4SqlMcpServer.Tools;

public static class GetOrderDetailsTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "get_order_details",
            description = "Obtener detalles de un pedido específico por su ID",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["orderId"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "ID del pedido a consultar"
                    }
                },
                ["required"] = new[] { "orderId" }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Order[] orders, Customer[] customers, Product[] products)
    {
        if (!arguments.ContainsKey("orderId"))
        {
            throw new ArgumentException("Se requiere el parámetro 'orderId'");
        }

        var orderId = arguments["orderId"].GetInt32();

        var order = orders.FirstOrDefault(o => o.Id == orderId);

        if (order == null)
        {
            return new
            {
                found = false,
                message = $"No se encontró el pedido con ID {orderId}"
            };
        }

        var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
        var product = products.FirstOrDefault(p => p.Id == order.ProductId);

        return new
        {
            found = true,
            order = new
            {
                id = order.Id,
                customerId = order.CustomerId,
                customerName = customer?.Name ?? "Unknown",
                customerEmail = customer?.Email,
                productId = order.ProductId,
                productName = product?.Name ?? "Unknown",
                quantity = order.Quantity,
                totalAmount = order.TotalAmount,
                orderDate = order.OrderDate,
                status = order.Status
            }
        };
    }
}
