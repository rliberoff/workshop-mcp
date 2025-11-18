# 🚀 Quick Start - MCP Workshop

## Before the Workshop

### 1. Install Prerequisites

```powershell
# Check .NET version (required: 10.0 or higher)
dotnet --version

# If you need to install .NET 10
# Download from: https://dotnet.microsoft.com/download/dotnet/10.0
```

### 2. Clone Repository

```powershell
git clone <repository-url>
cd mcp-workshop
```

### 3. Verify Build

```powershell
# Build entire solution
dotnet build McpWorkshop.sln

# Expected output: "Build succeeded" with 8 projects
```

---

## During the Workshop

### Exercise 1: Static Resources (25 min)

**Goal**: Create your first MCP server with static JSON resources

```powershell
# Navigate to starter template
cd templates/exercise1-starter

# Run the reference server (for comparison)
cd ../../src/McpWorkshop.Servers/Exercise1StaticResources
dotnet run

# Verify (in another terminal)
cd ../../../scripts
.\verify-exercise1.ps1
```

**What you'll learn**: MCP protocol basics, resources/list, resources/read

---

### Exercise 2: Parametric Query (25 min)

**Goal**: Add tools with parameters and JSON Schema validation

```powershell
# Navigate to starter template
cd templates/exercise2-starter

# Run the reference server
cd ../../src/McpWorkshop.Servers/Exercise2ParametricQuery
dotnet run

# Verify
cd ../../../scripts
.\verify-exercise2.ps1
```

**What you'll learn**: Tools definition, inputSchema, parameter validation, error handling

---

### Exercise 3: Secure Server (30 min)

**Goal**: Add JWT authentication, authorization scopes, rate limiting, and logging

```powershell
# Navigate to starter template
cd templates/exercise3-starter

# Run the reference server
cd ../../src/McpWorkshop.Servers/Exercise3SecureServer
dotnet run

# Verify
cd ../../../scripts
.\verify-exercise3.ps1
```

**What you'll learn**: Authentication middleware, scope-based authorization, rate limiting (sliding window), structured logging with redaction

---

### Exercise 4: Virtual Analyst (35 min)

**Goal**: Build an orchestrator that coordinates multiple MCP servers with Spanish NLP

**⚠️ Important**: This exercise requires 4 servers running simultaneously

#### Step 1: Start Backend Servers

```powershell
# Terminal 1: SQL Server (port 5010)
cd src/McpWorkshop.Servers/Exercise4SqlMcpServer
dotnet run

# Terminal 2: Cosmos Server (port 5011)
cd src/McpWorkshop.Servers/Exercise4CosmosMcpServer
dotnet run

# Terminal 3: REST API Server (port 5012)
cd src/McpWorkshop.Servers/Exercise4RestApiMcpServer
dotnet run
```

#### Step 2: Start Orchestrator

```powershell
# Terminal 4: Virtual Analyst (port 5004)
cd src/McpWorkshop.Servers/Exercise4VirtualAnalyst
dotnet run
```

#### Step 3: Test with Spanish Queries

```powershell
# Terminal 5: Run verification
cd scripts
.\verify-exercise4.ps1

# Or test manually:
curl -X POST http://localhost:5004/query `
  -H "Content-Type: application/json" `
  -d '{"query": "¿Cuántos clientes nuevos hay en Madrid?"}'
```

**What you'll learn**: Orchestration patterns, parallel execution (Task.WhenAll), sequential execution (await), caching, natural language processing (Spanish), multi-server communication

---

## Quick Reference

### Port Allocation

-   `5001`: Exercise 1 - Static Resources
-   `5002`: Exercise 2 - Parametric Query
-   `5003`: Exercise 3 - Secure Server
-   `5004`: Exercise 4 - Virtual Analyst (orchestrator)
-   `5010`: Exercise 4 - SqlMcpServer
-   `5011`: Exercise 4 - CosmosMcpServer
-   `5012`: Exercise 4 - RestApiMcpServer

### MCP Protocol Endpoints

```
POST /mcp
Content-Type: application/json

# Initialize
{"jsonrpc": "2.0", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}}, "id": 1}

# List Resources
{"jsonrpc": "2.0", "method": "resources/list", "id": 2}

# Read Resource
{"jsonrpc": "2.0", "method": "resources/read", "params": {"uri": "workshop://data/customers"}, "id": 3}

# List Tools
{"jsonrpc": "2.0", "method": "tools/list", "id": 4}

# Call Tool
{"jsonrpc": "2.0", "method": "tools/call", "params": {"name": "search_customers", "arguments": {"query": "García"}}, "id": 5}
```

### Exercise 4 Spanish Queries

```json
// New customers
{"query": "¿Cuántos clientes nuevos hay en Madrid?"}

// Abandoned carts
{"query": "¿Usuarios que abandonaron carrito últimas 24 horas?"}

// Order status
{"query": "¿Estado del pedido 1001?"}

// Sales summary (parallel execution)
{"query": "Resumen de ventas de esta semana"}

// Top products
{"query": "Top 5 productos más vendidos"}
```

---

## Troubleshooting

### "Port already in use"

```powershell
# Find process using the port
netstat -ano | findstr :5001

# Kill process (replace PID)
taskkill /PID <PID> /F
```

### "Connection refused" in Exercise 4

Make sure all 3 backend servers are running:

-   SqlMcpServer (5010)
-   CosmosMcpServer (5011)
-   RestApiMcpServer (5012)

### Build Errors

```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Verification Script Fails

1. Ensure the server is running (`dotnet run`)
2. Wait 5 seconds for server startup
3. Check port is not blocked by firewall

---

## Help & Documentation

-   **Full Documentation**: [docs/README.md](docs/README.md)
-   **Agenda**: [docs/AGENDA.md](docs/AGENDA.md)
-   **Quick Reference**: [docs/QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md)
-   **Troubleshooting**: [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)
-   **Instructor Guide**: [docs/INSTRUCTOR_HANDBOOK.md](docs/INSTRUCTOR_HANDBOOK.md)

---

## After the Workshop

### Continue Learning

-   Explore Azure deployment: [docs/AZURE_DEPLOYMENT.md](docs/AZURE_DEPLOYMENT.md)
-   Review instructor notes for deeper insights
-   Experiment with custom tools and resources
-   Join the MCP community discussions

### Share Feedback

-   Report issues: GitHub Issues
-   Suggest improvements: Pull Requests
-   Share your MCP servers: Community showcase

---

**Happy Learning! 🎓🚀**
