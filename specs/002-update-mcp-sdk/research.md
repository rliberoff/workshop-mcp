# Research: Update ModelContextProtocol SDK to 0.9.0-preview.1

**Date**: 2026-02-18  
**Feature**: [spec.md](spec.md)

## 1. SDK Availability & Target Framework Support

- **Decision**: `ModelContextProtocol` v0.9.0-preview.1 is available on nuget.org (published 2026-02-05)
- **Rationale**: Confirmed directly on nuget.org; supports `net8.0`, `net9.0`, `net10.0`, and `netstandard2.0`
- **Alternatives considered**: None — version was specified by the user

**TFM support confirmed**: The new SDK supports both `net8.0` and `net10.0`. The decision to upgrade `McpWorkshop.Shared` to `net10.0` is safe and not forced by the SDK.

**Package split**: Starting from recent versions, the SDK is split into three packages:

- `ModelContextProtocol` (main — what we reference)
- `ModelContextProtocol.Core` (minimal dependencies, pulled transitively)
- `ModelContextProtocol.AspNetCore` (HTTP server support — not currently used)

## 2. Breaking Changes Between v0.4.0-preview.3 and v0.9.0-preview.1

- **Decision**: Breaking changes exist in the SDK API but have **zero impact** on compiled source code (see Task 3)
- **Rationale**: The codebase does not import or use any `ModelContextProtocol.*` namespaces in C# source files. All MCP protocol handling is custom-built in `McpWorkshop.Shared.Mcp`.
- **Alternatives considered**: N/A — no code uses the SDK APIs

### Summary of Breaking Changes (for reference only)

| Version | Change | Impact on This Codebase |
|---------|--------|------------------------|
| v0.5.0 | `McpServerFactory`/`McpClientFactory` removed | None — not used |
| v0.5.0 | `IMcpEndpoint`/`IMcpClient`/`IMcpServer` interfaces removed | None — not used |
| v0.5.0 | `RequestOptions` bag replaces individual params | None in source; affects Exercise 5 doc samples |
| v0.5.0 | `Enumerate*Async` methods removed | None — not used |
| v0.8.0 | Protocol reference types sealed | None — not inherited |

### Exercise 5 Documentation Impact

The file `docs/modules/09b-ejercicio-5-agente-maf.md` contains code samples that reference SDK types:

- `McpClient.CreateAsync()`, `ListToolsAsync()`, `CallToolAsync()` — still valid in v0.9.0-preview.1
- `HttpClientTransport`, `StdioClientTransport` — still valid
- `CallToolResult`, `McpClientTool` — still valid
- `ListToolsAsync()` may now accept `RequestOptions?` parameter — minor signature change

**Decision**: Review documentation code samples during implementation to ensure accuracy with v0.9.0-preview.1 API, but the samples appear largely compatible.

## 3. SDK API Usage in Source Code

- **Decision**: The `ModelContextProtocol` SDK is referenced as a NuGet package but **not consumed** in any C# source file
- **Rationale**: Grep across all `.cs` files shows zero `using ModelContextProtocol.*` imports. All MCP protocol types are hand-rolled in `McpWorkshop.Shared.Mcp` namespace (10 custom types including `JsonRpcRequest`, `JsonRpcResponse`, `McpServerBase`, etc.)
- **Alternatives considered**: N/A — this is a factual finding

**Risk assessment**: The migration is a package version bump with minimal compiled code risk. The only potential issue is assembly-level type conflicts between SDK types and the custom `McpWorkshop.Shared.Mcp` types, but this is unlikely since they are in different namespaces and the SDK types are not directly imported.

## 4. Microsoft.Extensions.* Latest Versions

- **Decision**: Update all `Microsoft.Extensions.*` packages to v10.0.3 (latest stable)
- **Rationale**: User chose proactive update strategy; v10.0.3 is latest stable; SDK requires >= 10.0.2 for its transitive dependencies
- **Alternatives considered**: Keeping at 10.0.0 (rejected — user chose proactive update)

| Package | Current | Target |
|---------|---------|--------|
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.0 | 10.0.3 |
| `Microsoft.Extensions.Options` | 10.0.0 | 10.0.3 |
| `Microsoft.AspNetCore.Http.Abstractions` | 2.3.0 | 2.3.0 (keep — legacy package, no meaningful updates) |

**Note on `Microsoft.AspNetCore.Http.Abstractions`**: This legacy NuGet package (2.3.x) targets `netstandard2.0` only. For `net10.0` projects using `Microsoft.NET.Sdk.Web`, HTTP abstractions are included as a framework reference. Since `McpWorkshop.Shared` uses `Microsoft.NET.Sdk` (not `Web`), keeping this package as-is is the safest option. Version 2.3.9 is the latest but contains no significant changes over 2.3.0.

## 5. Transitive Dependencies from New SDK

- **Decision**: No additional package references need to be added; transitive dependencies are handled automatically
- **Rationale**: `ModelContextProtocol` v0.9.0-preview.1 pulls `ModelContextProtocol.Core`, `Microsoft.Extensions.Caching.Abstractions`, and `Microsoft.Extensions.Hosting.Abstractions` transitively
- **Alternatives considered**: Explicitly pinning transitive deps (rejected — unnecessary complexity)

## Summary: Migration Plan

| Step | Action | Risk |
|------|--------|------|
| 1 | Update `McpWorkshop.Shared` TFM from `net8.0` to `net10.0` | Low |
| 2 | Update `ModelContextProtocol` to `0.9.0-preview.1` in 4 `.csproj` files | Low |
| 3 | Update `Microsoft.Extensions.Logging.Abstractions` to `10.0.3` | Low |
| 4 | Update `Microsoft.Extensions.Options` to `10.0.3` | Low |
| 5 | Build solution, fix any errors/warnings | Low (no SDK API usage in source) |
| 6 | Run tests, fix any failures | Low |
| 7 | Review Exercise 5 doc code samples for SDK accuracy | Low |
