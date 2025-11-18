using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Exercise4VirtualAnalyst.Models;

namespace Exercise4VirtualAnalyst.Services;

public class OrchestratorService
{
    private readonly Dictionary<string, McpServerClient> _servers;
    private readonly ConcurrentDictionary<string, (string result, DateTime cachedAt)> _cache;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    public OrchestratorService()
    {
        _servers = new Dictionary<string, McpServerClient>
        {
            ["sql"] = new McpServerClient("http://localhost:5010"),
            ["cosmos"] = new McpServerClient("http://localhost:5011"),
            ["rest"] = new McpServerClient("http://localhost:5012")
        };
        _cache = new ConcurrentDictionary<string, (string, DateTime)>();
    }

    public async Task<string> ProcessQueryAsync(ParsedQuery parsedQuery)
    {
        // Check cache
        var cacheKey = $"{parsedQuery.Intent}:{string.Join(",", parsedQuery.Parameters.Select(p => $"{p.Key}={p.Value}"))}";
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow - cached.cachedAt < _cacheTtl)
            {
                return $"[CACHED] {cached.result}";
            }
            _cache.TryRemove(cacheKey, out _);
        }

        string result;
        try
        {
            result = parsedQuery.Intent switch
            {
                "new_customers" => await ProcessNewCustomersQuery(parsedQuery),
                "abandoned_carts" => await ProcessAbandonedCartsQuery(parsedQuery),
                "order_status" => await ProcessOrderStatusQuery(parsedQuery),
                "sales_summary" => await ProcessSalesSummaryQuery(parsedQuery),
                "top_products" => await ProcessTopProductsQuery(parsedQuery),
                _ => $"❌ No se pudo procesar la consulta. Intención detectada: {parsedQuery.Intent}"
            };

            // Store in cache
            _cache[cacheKey] = (result, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            result = $"❌ Error procesando consulta: {ex.Message}";
        }

        return result;
    }

    private async Task<string> ProcessNewCustomersQuery(ParsedQuery query)
    {
        var args = new Dictionary<string, object>();

        if (query.Parameters.ContainsKey("country"))
            args["country"] = query.Parameters["country"];
        else
            args["country"] = "España";

        if (query.Parameters.ContainsKey("city"))
            args["city"] = query.Parameters["city"];

        var result = await _servers["sql"].CallToolAsync("query_customers_by_country", args);

        if (result.TryGetProperty("content", out var content))
        {
            var textContent = content.EnumerateArray().FirstOrDefault();
            if (textContent.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? "Sin resultados";
            }
        }

        return "❌ Error obteniendo clientes";
    }

    private async Task<string> ProcessAbandonedCartsQuery(ParsedQuery query)
    {
        var args = new Dictionary<string, object>();

        if (query.Parameters.ContainsKey("hours"))
            args["hours"] = int.Parse(query.Parameters["hours"]);
        else
            args["hours"] = 24;

        var result = await _servers["cosmos"].CallToolAsync("get_abandoned_carts", args);

        if (result.TryGetProperty("content", out var content))
        {
            var textContent = content.EnumerateArray().FirstOrDefault();
            if (textContent.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? "Sin resultados";
            }
        }

        return "❌ Error obteniendo carritos abandonados";
    }

    private async Task<string> ProcessOrderStatusQuery(ParsedQuery query)
    {
        if (!query.Parameters.ContainsKey("orderId"))
            return "❌ Se requiere un ID de pedido. Ejemplo: '¿Estado del pedido 1001?'";

        var orderId = int.Parse(query.Parameters["orderId"]);

        // Sequential: first get order details, then shipping status
        var orderArgs = new Dictionary<string, object> { ["orderId"] = orderId };

        try
        {
            // Note: SqlMcpServer doesn't have get_order_details tool, 
            // so we'll just get shipping status for demonstration
            var shippingResult = await _servers["rest"].CallToolAsync("get_shipping_status", orderArgs);

            if (shippingResult.TryGetProperty("content", out var content))
            {
                var textContent = content.EnumerateArray().FirstOrDefault();
                if (textContent.TryGetProperty("text", out var text))
                {
                    return $"📦 ESTADO DEL PEDIDO #{orderId}\n\n{text.GetString()}";
                }
            }
        }
        catch
        {
            return $"❌ No se encontró información del pedido #{orderId}";
        }

        return "❌ Error obteniendo estado del pedido";
    }

    private async Task<string> ProcessSalesSummaryQuery(ParsedQuery query)
    {
        // PARALLEL execution: get sales summary and top products simultaneously
        var salesArgs = new Dictionary<string, object>();
        if (query.Parameters.ContainsKey("period"))
        {
            salesArgs["period"] = query.Parameters["period"];
        }

        var topProductsArgs = new Dictionary<string, object>
        {
            ["limit"] = 5,
            ["period"] = query.Parameters.ContainsKey("period") ? query.Parameters["period"] : "week"
        };

        // Execute in parallel with Task.WhenAll
        var salesTask = _servers["sql"].CallToolAsync("get_sales_summary", salesArgs);
        var topProductsTask = _servers["rest"].CallToolAsync("get_top_products", topProductsArgs);

        await Task.WhenAll(salesTask, topProductsTask);

        var salesResult = await salesTask;
        var topProductsResult = await topProductsTask;

        var salesText = ExtractTextFromResult(salesResult);
        var topProductsText = ExtractTextFromResult(topProductsResult);

        return $"{salesText}\n\n{topProductsText}";
    }

    private async Task<string> ProcessTopProductsQuery(ParsedQuery query)
    {
        var args = new Dictionary<string, object>
        {
            ["limit"] = 10,
            ["period"] = query.Parameters.ContainsKey("period") ? query.Parameters["period"] : "week"
        };

        var result = await _servers["rest"].CallToolAsync("get_top_products", args);
        return ExtractTextFromResult(result);
    }

    private string ExtractTextFromResult(JsonElement result)
    {
        if (result.TryGetProperty("content", out var content))
        {
            var textContent = content.EnumerateArray().FirstOrDefault();
            if (textContent.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? "Sin resultados";
            }
        }
        return "Sin resultados";
    }
}
