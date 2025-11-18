using Exercise4VirtualAnalyst.Parsers;

namespace Exercise4VirtualAnalyst.Services;

public class OrchestratorService
{
    private readonly Dictionary<string, McpServerClient> _servers;

    // TODO: Cache con TTL 5 minutos
    // private readonly ConcurrentDictionary<string, (object result, DateTime cachedAt)> _cache;

    public OrchestratorService()
    {
        _servers = new Dictionary<string, McpServerClient>
        {
            ["sql"] = new McpServerClient("http://localhost:5010"),
            ["cosmos"] = new McpServerClient("http://localhost:5011"),
            ["rest"] = new McpServerClient("http://localhost:5012")
        };
    }

    public async Task<object> ProcessQueryAsync(ParsedQuery parsedQuery)
    {
        // TODO: Verificar cache

        // TODO: Ejecutar según intención:
        //   - new_customers: llamar sql.query_customers_by_country (secuencial)
        //   - abandoned_carts: llamar cosmos.get_abandoned_carts (secuencial)
        //   - order_status: llamar sql + rest (secuencial - orden depende de producto)
        //   - sales_summary: llamar sql + rest (PARALELO - Task.WhenAll)

        // TODO: Almacenar en cache

        // TODO: Retornar resultado formateado

        return new { status = "TODO", message = "Implementar lógica de orquestación" };
    }
}
