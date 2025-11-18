using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Exercise4RestApiMcpServer.Tools;

public class CheckInventoryTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "check_inventory",
            description = "Verificar inventario disponible de un producto (simula llamada a API externa)",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["productId"] = new Dictionary<string, object>
                    {
                        ["type"] = "number",
                        ["description"] = "ID del producto a verificar"
                    }
                },
                ["required"] = new[] { "productId" }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments)
    {
        if (!arguments.ContainsKey("productId"))
            throw new ArgumentException("El parámetro 'productId' es requerido");

        var productId = arguments["productId"].GetInt32();

        // Simulate API call delay
        System.Threading.Thread.Sleep(100);

        // Simulate inventory data
        var random = new Random(productId);
        var inStock = random.Next(0, 100) > 20;
        var quantity = inStock ? random.Next(5, 50) : 0;
        var warehouse = new[] { "Madrid", "Barcelona", "Valencia" }[random.Next(0, 3)];

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"📦 INVENTARIO - Producto #{productId}\n\n" +
                           $"Estado: {(inStock ? "✅ DISPONIBLE" : "❌ AGOTADO")}\n" +
                           $"Cantidad: {quantity} unidades\n" +
                           $"Almacén: {warehouse}\n" +
                           $"Última actualización: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                }
            }
        };
    }
}
