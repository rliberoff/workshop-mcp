using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Exercise4VirtualAnalyst.Models;

namespace Exercise4VirtualAnalyst.Parsers;

public class SpanishQueryParser
{
    public ParsedQuery Parse(string query)
    {
        var lowerQuery = query.ToLowerInvariant();
        var intent = DetectIntent(lowerQuery);
        var parameters = ExtractParameters(lowerQuery, intent);
        var requiredServers = MapServers(intent);

        return new ParsedQuery
        {
            Intent = intent,
            Parameters = parameters,
            RequiredServers = requiredServers,
            OriginalQuery = query
        };
    }

    private string DetectIntent(string query)
    {
        // new_customers intent
        if (query.Contains("clientes nuevos") || query.Contains("nuevos clientes") ||
            query.Contains("clientes registrados"))
            return "new_customers";

        // abandoned_carts intent
        if (query.Contains("carrito abandonado") || query.Contains("carritos abandonados") ||
            query.Contains("abandonaron carrito") || query.Contains("abandonaron el carrito"))
            return "abandoned_carts";

        // order_status intent
        if (query.Contains("estado pedido") || query.Contains("estado del pedido") ||
            query.Contains("rastrear pedido") || query.Contains("seguimiento pedido"))
            return "order_status";

        // sales_summary intent
        if (query.Contains("resumen ventas") || query.Contains("resumen de ventas") ||
            query.Contains("ventas totales") || query.Contains("ventas del") ||
            query.Contains("ventas de la"))
            return "sales_summary";

        // top_products intent
        if (query.Contains("productos más vendidos") || query.Contains("top productos") ||
            query.Contains("productos populares") || query.Contains("mejores productos"))
            return "top_products";

        return "unknown";
    }

    private Dictionary<string, string> ExtractParameters(string query, string intent)
    {
        var parameters = new Dictionary<string, string>();

        // Extract city
        var cities = new[] { "madrid", "barcelona", "sevilla", "valencia", "bilbao" };
        foreach (var city in cities)
        {
            if (query.Contains(city))
            {
                parameters["city"] = char.ToUpper(city[0]) + city.Substring(1);
                break;
            }
        }

        // Extract country
        if (query.Contains("españa") || query.Contains("spain"))
            parameters["country"] = "España";
        else if (query.Contains("méxico") || query.Contains("mexico"))
            parameters["country"] = "México";

        // Extract order ID
        var orderMatch = Regex.Match(query, @"pedido\s+#?(\d+)");
        if (orderMatch.Success)
            parameters["orderId"] = orderMatch.Groups[1].Value;

        // Extract hours for abandoned carts
        var hoursMatch = Regex.Match(query, @"últimas?\s+(\d+)\s+horas?");
        if (hoursMatch.Success)
            parameters["hours"] = hoursMatch.Groups[1].Value;
        else if (query.Contains("24 horas") || query.Contains("día"))
            parameters["hours"] = "24";
        else if (query.Contains("48 horas"))
            parameters["hours"] = "48";

        // Extract period for sales
        if (query.Contains("semana") || query.Contains("week"))
            parameters["period"] = "week";
        else if (query.Contains("mes") || query.Contains("month"))
            parameters["period"] = "month";
        else if (query.Contains("hoy") || query.Contains("día") || query.Contains("day"))
            parameters["period"] = "day";

        return parameters;
    }

    private List<string> MapServers(string intent)
    {
        return intent switch
        {
            "new_customers" => new List<string> { "sql" },
            "abandoned_carts" => new List<string> { "cosmos" },
            "order_status" => new List<string> { "sql", "rest" },
            "sales_summary" => new List<string> { "sql", "rest" },
            "top_products" => new List<string> { "rest" },
            _ => new List<string>()
        };
    }
}
