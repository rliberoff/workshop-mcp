using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Exercise4SqlMcpServer.Models;

namespace Exercise4SqlMcpServer.Tools;

public class GetSalesSummaryTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "get_sales_summary",
            description = "Obtener resumen de ventas con estadísticas agregadas",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["startDate"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Fecha de inicio (opcional, formato: YYYY-MM-DD)"
                    },
                    ["endDate"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Fecha de fin (opcional, formato: YYYY-MM-DD)"
                    },
                    ["status"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Filtrar por estado del pedido (opcional)"
                    }
                }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Order[] orders)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;
        string? status = null;

        if (arguments.ContainsKey("startDate") && arguments["startDate"].ValueKind == JsonValueKind.String)
        {
            DateTime.TryParse(arguments["startDate"].GetString(), out var start);
            startDate = start;
        }

        if (arguments.ContainsKey("endDate") && arguments["endDate"].ValueKind == JsonValueKind.String)
        {
            DateTime.TryParse(arguments["endDate"].GetString(), out var end);
            endDate = end;
        }

        if (arguments.ContainsKey("status") && arguments["status"].ValueKind == JsonValueKind.String)
        {
            status = arguments["status"].GetString();
        }

        var filtered = orders.AsEnumerable();

        if (startDate.HasValue)
            filtered = filtered.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            filtered = filtered.Where(o => o.OrderDate <= endDate.Value);

        if (!string.IsNullOrEmpty(status))
            filtered = filtered.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        var ordersList = filtered.ToList();
        var totalSales = ordersList.Sum(o => o.TotalAmount);
        var totalOrders = ordersList.Count;
        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        var statusBreakdown = ordersList
            .GroupBy(o => o.Status)
            .Select(g => new { status = g.Key, count = g.Count(), total = g.Sum(o => o.TotalAmount) })
            .ToList();

        var periodText = startDate.HasValue && endDate.HasValue
            ? $"del {startDate:yyyy-MM-dd} al {endDate:yyyy-MM-dd}"
            : startDate.HasValue
                ? $"desde {startDate:yyyy-MM-dd}"
                : endDate.HasValue
                    ? $"hasta {endDate:yyyy-MM-dd}"
                    : "todo el período";

        object textContent = new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = $"📊 RESUMEN DE VENTAS {periodText}\n\n" +
                       $"Total Ventas: €{totalSales:F2}\n" +
                       $"Total Pedidos: {totalOrders}\n" +
                       $"Valor Promedio: €{averageOrderValue:F2}\n\n" +
                       $"Desglose por Estado:\n" +
                       string.Join("\n", statusBreakdown.Select(s => $"- {s.status}: {s.count} pedidos (€{s.total:F2})"))
        };

        object resourceContent = new Dictionary<string, object>
        {
            ["type"] = "resource",
            ["resource"] = new Dictionary<string, object>
            {
                ["uri"] = "sql://workshop/sales-summary",
                ["mimeType"] = "application/json",
                ["text"] = JsonSerializer.Serialize(new
                {
                    summary = new
                    {
                        totalSales,
                        totalOrders,
                        averageOrderValue,
                        period = periodText
                    },
                    breakdown = statusBreakdown
                })
            }
        };

        return new Dictionary<string, object>
        {
            ["content"] = new[] { textContent, resourceContent }
        };
    }
}
