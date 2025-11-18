using Exercise2ParametricQuery.Models;
using System.Text.Json;

namespace Exercise2ParametricQuery.Tools;

public static class SearchCustomersTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "GetCustomers",
            description = "Buscar clientes por nombre o país con soporte de paginación",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    name = new
                    {
                        type = "string",
                        description = "Nombre del cliente (búsqueda parcial)"
                    },
                    country = new
                    {
                        type = "string",
                        description = "País del cliente"
                    },
                    page = new
                    {
                        type = "integer",
                        description = "Número de página (comenzando en 1)",
                        @default = 1
                    },
                    pageSize = new
                    {
                        type = "integer",
                        description = "Número de resultados por página",
                        @default = 10
                    }
                }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Customer[] allCustomers)
    {
        var customers = allCustomers.AsEnumerable();

        if (arguments.TryGetValue("name", out var nameElement))
        {
            var name = nameElement.GetString()?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(name))
            {
                customers = customers.Where(c => c.Name.ToLowerInvariant().Contains(name));
            }
        }

        if (arguments.TryGetValue("country", out var countryElement))
        {
            var country = countryElement.GetString();
            if (!string.IsNullOrEmpty(country))
            {
                customers = customers.Where(c => c.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
            }
        }

        var allResults = customers.ToArray();

        // Paginación
        int page = 1;
        int pageSize = 10;

        if (arguments.TryGetValue("page", out var pageElement) && pageElement.TryGetInt32(out var pageValue))
        {
            page = pageValue;
        }

        if (arguments.TryGetValue("pageSize", out var pageSizeElement) && pageSizeElement.TryGetInt32(out var pageSizeValue))
        {
            pageSize = pageSizeValue;
        }

        var results = allResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var responseData = new
        {
            customers = results,
            total = allResults.Length,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)allResults.Length / pageSize)
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
            data = results,
            metadata = new
            {
                total = allResults.Length,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)allResults.Length / pageSize)
            }
        };
    }
}
