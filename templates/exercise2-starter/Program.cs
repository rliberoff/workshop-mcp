var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// TODO: Crear herramientas (tools):
// 1. SearchCustomersTool - buscar clientes por nombre/país
// 2. FilterProductsTool - filtrar productos por categoría/precio
// 3. AggregateSalesTool - agregar ventas por período

// TODO: Implementar endpoint /mcp
// TODO: Implementar handler tools/list
// TODO: Implementar handler tools/call

app.Run("http://localhost:5002");
