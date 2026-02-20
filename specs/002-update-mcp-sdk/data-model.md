# Data Model: Update ModelContextProtocol SDK to 0.9.0-preview.1

**Date**: 2026-02-18  
**Feature**: [spec.md](spec.md)

## Overview

This feature modifies NuGet package references and a target framework — no new data entities are introduced. The "data model" for this feature describes the package dependency structure before and after the migration.

## Entities

### PackageReference (`.csproj` element)

Represents a NuGet package dependency declared in a project file.

| Field | Type | Description |
|-------|------|-------------|
| Include | string | Package name (e.g., `ModelContextProtocol`) |
| Version | string | SemVer version string |

### TargetFramework (`.csproj` element)

Represents the .NET target framework moniker for a project.

| Field | Type | Description |
|-------|------|-------------|
| Value | string | TFM (e.g., `net8.0`, `net10.0`) |

## State Transitions

### McpWorkshop.Shared.csproj

| Element | Before | After |
|---------|--------|-------|
| `TargetFramework` | `net8.0` | `net10.0` |
| `ModelContextProtocol` | `0.4.0-preview.3` | `0.9.0-preview.1` |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.0` | `10.0.3` |
| `Microsoft.Extensions.Options` | `10.0.0` | `10.0.3` |
| `Microsoft.AspNetCore.Http.Abstractions` | `2.3.0` | `2.3.0` (unchanged) |
| `StyleCop.Analyzers` | `1.2.0-beta.556` | `1.2.0-beta.556` (unchanged) |

### SqlMcpServer.csproj

| Element | Before | After |
|---------|--------|-------|
| `TargetFramework` | `net10.0` | `net10.0` (unchanged) |
| `ModelContextProtocol` | `0.4.0-preview.3` | `0.9.0-preview.1` |

### RestApiMcpServer.csproj

| Element | Before | After |
|---------|--------|-------|
| `TargetFramework` | `net10.0` | `net10.0` (unchanged) |
| `ModelContextProtocol` | `0.4.0-preview.3` | `0.9.0-preview.1` |

### CosmosMcpServer.csproj

| Element | Before | After |
|---------|--------|-------|
| `TargetFramework` | `net10.0` | `net10.0` (unchanged) |
| `ModelContextProtocol` | `0.4.0-preview.3` | `0.9.0-preview.1` |

### McpWorkshop.Tests.csproj

| Element | Before | After |
|---------|--------|-------|
| All packages | (current) | (unchanged — no direct MCP reference) |

## Relationships

```
McpWorkshop.Tests
  ├── references → McpWorkshop.Shared (gets MCP transitively)
  ├── references → SqlMcpServer (gets MCP transitively)
  ├── references → CosmosMcpServer (gets MCP transitively)
  └── references → RestApiMcpServer (gets MCP transitively)

SqlMcpServer       → references → McpWorkshop.Shared
RestApiMcpServer   → references → McpWorkshop.Shared
CosmosMcpServer    → references → McpWorkshop.Shared
```

## Validation Rules

- All `ModelContextProtocol` references MUST be exactly `0.9.0-preview.1`
- `McpWorkshop.Shared` TFM MUST be `net10.0`
- `Microsoft.Extensions.Logging.Abstractions` MUST be `10.0.3` (in `McpWorkshop.Shared`)
- `Microsoft.Extensions.Options` MUST be `10.0.3` (in `McpWorkshop.Shared`)
- No version conflicts between transitive dependencies across projects
