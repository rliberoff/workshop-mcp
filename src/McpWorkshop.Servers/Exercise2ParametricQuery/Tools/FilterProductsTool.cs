using Exercise2ParametricQuery.Models;
using System.Text.Json;

namespace Exercise2ParametricQuery.Tools;

public static class FilterProductsTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "GetProducts",
            description = "Filtrar productos por categoría, rango de precio y disponibilidad",
            inputSchema = new
            {
                type = "object",
                properties = new
                {
                    category = new
                    {
                        type = "string",
                        description = "Categoría del producto"
                    },
                    minPrice = new
                    {
                        type = "number",
                        description = "Precio mínimo"
                    },
                    maxPrice = new
                    {
                        type = "number",
                        description = "Precio máximo"
                    },
                    inStockOnly = new Dictionary<string, object>
                    {
                        ["type"] = "boolean",
                        ["description"] = "Solo productos en stock",
                        ["default"] = false
                    }
                }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, Product[] allProducts)
    {
        var products = allProducts.AsEnumerable();

        if (arguments.TryGetValue("category", out var categoryElement))
        {
            var category = categoryElement.GetString();
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (arguments.TryGetValue("minPrice", out var minPriceElement))
        {
            if (minPriceElement.TryGetDecimal(out var minPrice))
            {
                products = products.Where(p => p.Price >= minPrice);
            }
        }

        if (arguments.TryGetValue("maxPrice", out var maxPriceElement))
        {
            if (maxPriceElement.TryGetDecimal(out var maxPrice))
            {
                products = products.Where(p => p.Price <= maxPrice);
            }
        }

        if (arguments.TryGetValue("inStockOnly", out var inStockOnlyElement))
        {
            if (inStockOnlyElement.GetBoolean())
            {
                products = products.Where(p => p.Stock > 0);
            }
        }

        var results = products.ToArray();

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = $"Found {results.Length} product(s): " + string.Join(", ", results.Select(p => p.Name))
                }
            },
            data = results
        };
    }
}
