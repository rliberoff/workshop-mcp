# Quickstart: Update ModelContextProtocol SDK to 0.9.0-preview.1

**Date**: 2026-02-18  
**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Prerequisites

- .NET 10 SDK installed
- Access to nuget.org (or configured NuGet feed)
- Repository checked out on branch `002-update-mcp-sdk`

## Steps

### 1. Update Target Framework (McpWorkshop.Shared)

In `src/McpWorkshop.Shared/McpWorkshop.Shared.csproj`, change:

```xml
<TargetFramework>net8.0</TargetFramework>
```

to:

```xml
<TargetFramework>net10.0</TargetFramework>
```

### 2. Update ModelContextProtocol in All Projects

Update the version in these 4 files from `0.4.0-preview.3` to `0.9.0-preview.1`:

- `src/McpWorkshop.Shared/McpWorkshop.Shared.csproj`
- `src/McpWorkshop.Servers/SqlMcpServer/SqlMcpServer.csproj`
- `src/McpWorkshop.Servers/RestApiMcpServer/RestApiMcpServer.csproj`
- `src/McpWorkshop.Servers/CosmosMcpServer/CosmosMcpServer.csproj`

### 3. Update Microsoft.Extensions.* Packages

In `src/McpWorkshop.Shared/McpWorkshop.Shared.csproj`, update:

- `Microsoft.Extensions.Logging.Abstractions`: `10.0.0` → `10.0.3`
- `Microsoft.Extensions.Options`: `10.0.0` → `10.0.3`

### 4. Restore, Build, and Test

```bash
dotnet restore McpWorkshop.sln
dotnet build McpWorkshop.sln --no-restore
dotnet test McpWorkshop.sln --no-build
```

All three commands must complete with zero errors.

### 5. Review Exercise 5 Documentation

Check `docs/modules/09b-ejercicio-5-agente-maf.md` for SDK code samples that reference `ModelContextProtocol.*` types. Verify code samples are accurate for v0.9.0-preview.1 API.

## Verification

Run the verification scripts to confirm all exercises work:

```bash
scripts/run-all-tests.ps1
```

## Expected Outcome

- All 4 projects reference `ModelContextProtocol` v0.9.0-preview.1
- `McpWorkshop.Shared` targets `net10.0`
- Solution builds with zero errors and zero new warnings
- All tests pass
- All MCP servers start and respond correctly
