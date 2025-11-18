using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Exercise4SqlMcpServer.Models;

namespace Exercise4SqlMcpServer.Tools;

public class QueryCustomersByCountryTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "query_customers_by_country",
            description = "Consultar clientes filtrados por país y opcionalmente por ciudad",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["country"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "País del cliente (ej: España, México)"
                    },
                    ["city"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Ciudad del cliente (opcional)"
                    }
                },
                ["required"] = new[] { "country" }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Customer[] customers)
    {
        var country = arguments.ContainsKey("country") ? arguments["country"].GetString() : null;
        var city = arguments.ContainsKey("city") ? arguments["city"].GetString() : null;

        if (string.IsNullOrEmpty(country))
            throw new ArgumentException("El parámetro 'country' es requerido");

        var filtered = customers
            .Where(c => c.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!string.IsNullOrEmpty(city))
        {
            filtered = filtered
                .Where(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var summary = city != null
            ? $"Encontrados {filtered.Count} cliente(s) en {city}, {country}"
            : $"Encontrados {filtered.Count} cliente(s) en {country}";

        object textContent = new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = $"{summary}\n\nClientes:\n{string.Join("\n", filtered.Select(c => $"- {c.Name} ({c.City}), registrado el {c.RegisteredAt:yyyy-MM-dd}"))}"
        };

        object resourceContent = new Dictionary<string, object>
        {
            ["type"] = "resource",
            ["resource"] = new Dictionary<string, object>
            {
                ["uri"] = $"sql://workshop/customers?country={country}" + (city != null ? $"&city={city}" : ""),
                ["mimeType"] = "application/json",
                ["text"] = JsonSerializer.Serialize(filtered)
            }
        };

        return new Dictionary<string, object>
        {
            ["content"] = new[] { textContent, resourceContent }
        };
    }
}
