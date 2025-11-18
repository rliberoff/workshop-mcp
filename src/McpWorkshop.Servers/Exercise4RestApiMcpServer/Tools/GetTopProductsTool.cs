using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Exercise4RestApiMcpServer.Tools;

public class GetTopProductsTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "get_top_products",
            description = "Obtener productos más vendidos (simula API de analítica)",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["limit"] = new Dictionary<string, object>
                    {
                        ["type"] = "number",
                        ["description"] = "Número de productos a retornar",
                        ["default"] = 10
                    },
                    ["period"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Período de análisis",
                        ["enum"] = new[] { "day", "week", "month" },
                        ["default"] = "week"
                    }
                }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments)
    {
        var limit = 10;
        if (arguments.ContainsKey("limit") && arguments["limit"].ValueKind == JsonValueKind.Number)
        {
            limit = arguments["limit"].GetInt32();
        }

        var period = "week";
        if (arguments.ContainsKey("period") && arguments["period"].ValueKind == JsonValueKind.String)
        {
            period = arguments["period"].GetString() ?? "week";
        }

        // Simulate API call delay
        System.Threading.Thread.Sleep(120);

        // Simulate top products data
        var products = new[]
        {
            new { id = 101, name = "Portátil HP ProBook", sales = 245, revenue = 220497.55m },
            new { id = 105, name = "Silla Herman Miller", sales = 189, revenue = 245698.11m },
            new { id = 102, name = "Monitor Dell UltraSharp", sales = 178, revenue = 62298.22m },
            new { id = 103, name = "Teclado Logitech MX", sales = 156, revenue = 18718.44m },
            new { id = 104, name = "Ratón Logitech MX Master", sales = 134, revenue = 12058.66m },
            new { id = 106, name = "Webcam Logitech Brio", sales = 98, revenue = 19502.00m },
            new { id = 107, name = "Auriculares Sony WH-1000XM5", sales = 87, revenue = 30189.00m },
            new { id = 108, name = "Dock USB-C Dell", sales = 76, revenue = 15124.00m },
            new { id = 109, name = "SSD Samsung 2TB", sales = 65, revenue = 12935.00m },
            new { id = 110, name = "Router Asus RT-AX88U", sales = 54, revenue = 13446.00m }
        };

        var topProducts = products.Take(limit).ToList();
        var totalSales = topProducts.Sum(p => p.sales);
        var totalRevenue = topProducts.Sum(p => p.revenue);

        var periodText = period switch
        {
            "day" => "HOY",
            "week" => "ESTA SEMANA",
            "month" => "ESTE MES",
            _ => period.ToUpper()
        };

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"🏆 TOP {limit} PRODUCTOS - {periodText}\n\n" +
                           $"Total Ventas: {totalSales} unidades\n" +
                           $"Ingresos Totales: €{totalRevenue:N2}\n\n" +
                           $"Ranking:\n" +
                           string.Join("\n", topProducts.Select((p, i) =>
                               $"{i + 1}. {p.name}: {p.sales} ventas (€{p.revenue:N2})"))
                }
            }
        };
    }
}
