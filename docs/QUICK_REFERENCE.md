# MCP Workshop - Quick Reference Card

Referencia rápida para asistentes durante ejercicios prácticos. Mantener esta página abierta en otra pantalla.

---

## 📡 MCP Protocol Cheat Sheet

### JSON-RPC Message Structure

```json
{
    "jsonrpc": "2.0",
    "method": "method_name",
    "params": {},
    "id": 1
}
```

### Core Methods

| Method           | Purpose                  | Required Params                                 | Response                       |
| ---------------- | ------------------------ | ----------------------------------------------- | ------------------------------ |
| `initialize`     | Client handshake         | `protocolVersion`, `capabilities`, `clientInfo` | Server capabilities            |
| `resources/list` | List available resources | None                                            | Array of resource descriptors  |
| `resources/read` | Get specific resource    | `uri`                                           | Resource content (text/binary) |
| `tools/list`     | List available tools     | None                                            | Array of tool schemas          |
| `tools/call`     | Execute tool             | `name`, `arguments`                             | Tool execution result          |
| `prompts/list`   | List prompt templates    | None                                            | Array of prompts               |
| `prompts/get`    | Get prompt by name       | `name`, `arguments`                             | Rendered prompt                |

### Standard Error Codes

```text
-32700  Parse error
-32600  Invalid request
-32601  Method not found
-32602  Invalid params
-32603  Internal error
-32000 to -32099  Server-defined errors
```

---

## 🔧 Common C# Patterns

### 1. Basic MCP Server (Exercise 1)

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = await reader.ReadToEndAsync();
    var request = JsonSerializer.Deserialize<JsonRpcRequest>(json);

    var response = request.Method switch
    {
        "initialize" => HandleInitialize(),
        "resources/list" => await HandleResourcesList(),
        "resources/read" => await HandleResourcesRead(request.Params),
        _ => JsonRpcError.MethodNotFound(request.Id)
    };

    await context.Response.WriteAsJsonAsync(response);
});

await app.RunAsync();
```

### 2. Tool with Input Schema (Exercise 2)

```csharp
public class GetCustomersTool
{
    public string Name => "get_customers";

    public object InputSchema => new
    {
        type = "object",
        properties = new
        {
            region = new { type = "string", description = "Filter by region" },
            limit = new { type = "integer", minimum = 1, maximum = 100 }
        },
        required = new[] { "region" }
    };

    public async Task<object> ExecuteAsync(JsonElement arguments)
    {
        var region = arguments.GetProperty("region").GetString();
        var limit = arguments.TryGetProperty("limit", out var l) ? l.GetInt32() : 10;

        var customers = await _repository.GetByRegionAsync(region, limit);
        return new { customers, count = customers.Count };
    }
}
```

### 3. JWT Authentication Middleware (Exercise 3)

```csharp
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (authHeader?.StartsWith("Bearer ") == true)
    {
        var token = authHeader["Bearer ".Length..];
        var principal = ValidateToken(token);
        context.User = principal;
    }
    await next();
});

ClaimsPrincipal ValidateToken(string token)
{
    var handler = new JwtSecurityTokenHandler();
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
        ValidateIssuer = true,
        ValidIssuer = "mcp-workshop",
        ValidateAudience = true,
        ValidAudience = "mcp-servers"
    };

    return handler.ValidateToken(token, validationParams, out _);
}
```

### 4. Rate Limiting (Exercise 3)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ByRole", context =>
    {
        var role = context.User?.FindFirst("role")?.Value ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(role, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = role switch
            {
                "admin" => int.MaxValue,
                "editor" => 50,
                "viewer" => 10,
                _ => 5
            },
            Window = TimeSpan.FromMinutes(1)
        });
    });
});

app.UseRateLimiter();
```

### 5. Multi-Server Orchestration (Exercise 4)

```csharp
public class VirtualAnalyst
{
    private readonly SqlMcpClient _sqlClient;
    private readonly CosmosMcpClient _cosmosClient;
    private readonly RestMcpClient _restClient;
    private readonly IMemoryCache _cache;

    public async Task<string> AnswerAsync(string naturalLanguageQuery)
    {
        // 1. Parse intent
        var intent = ParseQuery(naturalLanguageQuery);

        // 2. Determine which servers to call
        var tasks = new List<Task<object>>();
        if (intent.NeedsSqlData) tasks.Add(_sqlClient.QueryAsync(intent.SqlQuery));
        if (intent.NeedsCosmosData) tasks.Add(_cosmosClient.GetSessionAsync(intent.SessionId));
        if (intent.NeedsExternalApi) tasks.Add(_restClient.CallAsync(intent.ApiEndpoint));

        // 3. Execute in parallel
        var results = await Task.WhenAll(tasks);

        // 4. Aggregate and format
        return FormatResponse(results, intent);
    }
}
```

---

## ⚡ PowerShell Commands

### Setup & Verification

```powershell
# Check all prerequisites
.\scripts\check-prerequisites.ps1 -Verbose

# Verify specific exercise
.\scripts\verify-exercise1.ps1
.\scripts\verify-exercise2.ps1
.\scripts\verify-exercise3.ps1 -Token "your-jwt-token"
.\scripts\verify-exercise4.ps1

# Run all tests
.\scripts\run-all-tests.ps1 -Coverage $true

# Start all Exercise 4 servers
.\scripts\start-exercise4-servers.ps1
```

### Development Workflow

```powershell
# Create new MCP server project
dotnet new web -n MyMcpServer
cd MyMcpServer
dotnet add package ModelContextProtocol --prerelease
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.0

# Build and run
dotnet build
dotnet run --urls "http://localhost:5000"

# Run with specific profile
dotnet run --launch-profile Development

# Watch mode (auto-rebuild)
dotnet watch run
```

### Testing Endpoints

```powershell
# Test initialize
$body = @{
    jsonrpc = "2.0"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{ name = "test-client"; version = "1.0.0" }
    }
    id = 1
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"

# Test resources/list
$body = @{ jsonrpc="2.0"; method="resources/list"; id=2 } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"

# Test tools/call
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "get_customers"
        arguments = @{ region = "Europe"; limit = 5 }
    }
    id = 3
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri http://localhost:5000 -Method Post -Body $body -ContentType "application/json"

# Test with authentication
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri http://localhost:5001 -Method Post -Body $body -ContentType "application/json" -Headers $headers
```

---

## 🐛 Troubleshooting Quick Fixes

### Port Conflicts

```powershell
# Change port
$env:ASPNETCORE_URLS="http://localhost:5010"
dotnet run

# Find and kill process
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### NuGet Issues

```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose logging
dotnet restore --verbosity detailed

# Use specific source
dotnet add package ModelContextProtocol --source https://api.nuget.org/v3/index.json --prerelease
```

### JWT Debugging

```csharp
// Log token claims
var handler = new JwtSecurityTokenHandler();
var token = handler.ReadJwtToken(tokenString);
Console.WriteLine(string.Join("\n", token.Claims.Select(c => $"{c.Type}: {c.Value}")));

// Decode token online: https://jwt.io
```

### JSON Serialization

```csharp
// Case-insensitive deserialization
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

### CORS Errors

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

---

## 📚 Useful Links

| Resource              | URL                                      |
| --------------------- | ---------------------------------------- |
| MCP Specification     | https://spec.modelcontextprotocol.io/    |
| .NET 10 Documentation | https://learn.microsoft.com/dotnet/core/ |
| JSON-RPC 2.0 Spec     | https://www.jsonrpc.org/specification    |
| JWT Debugger          | https://jwt.io                           |
| JSON Schema Validator | https://www.jsonschemavalidator.net/     |
| Workshop Repository   | https://github.com/your-org/mcp-workshop |

---

## 🎯 Success Criteria Summary

| Exercise | Criterion                   | Validation Command                             |
| -------- | --------------------------- | ---------------------------------------------- |
| 1        | 4 resources exposed         | `.\scripts\verify-exercise1.ps1`               |
| 2        | 3 tools with schemas        | `.\scripts\verify-exercise2.ps1`               |
| 3        | JWT auth + rate limiting    | `.\scripts\verify-exercise3.ps1 -Token $token` |
| 4        | Orchestration of 3+ servers | `.\scripts\verify-exercise4.ps1`               |

---

## 💡 Pro Tips

1. **Keep this page open** in a second monitor/tab during exercises
2. **Copy-paste cautiously**: Understand each line before running
3. **Read error messages completely**: They often contain the solution
4. **Test incrementally**: Validate after each method implementation
5. **Use debugger**: Set breakpoints in VS Code (F5) or VS (F5)
6. **Check samples**: Reference implementations in `src/McpWorkshop.Servers/`
7. **Ask for help**: Raise hand if blocked > 3 minutes

---

**Happy Coding!** 🚀
