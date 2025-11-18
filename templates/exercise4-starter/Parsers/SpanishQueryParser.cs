namespace Exercise4VirtualAnalyst.Parsers;

public class ParsedQuery
{
    public string Intent { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public List<string> RequiredServers { get; set; } = new();
}

public class SpanishQueryParser
{
    public ParsedQuery Parse(string query)
    {
        // TODO: Detectar intención de la consulta en español
        // Intenciones soportadas:
        //   - "clientes nuevos", "nuevos clientes" → new_customers
        //   - "carrito abandonado", "carritos abandonados" → abandoned_carts
        //   - "estado pedido", "estado del pedido" → order_status
        //   - "resumen ventas", "ventas totales" → sales_summary

        // TODO: Extraer parámetros (ciudad, fecha, orderId, etc.)

        // TODO: Mapear intención a servidores requeridos:
        //   - new_customers → ["sql"]
        //   - abandoned_carts → ["cosmos"]
        //   - order_status → ["sql", "rest"]
        //   - sales_summary → ["sql", "rest"]

        return new ParsedQuery
        {
            Intent = "unknown",
            Parameters = new Dictionary<string, string>(),
            RequiredServers = new List<string>()
        };
    }
}
