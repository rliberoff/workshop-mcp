# Contract: .csproj Package Reference Updates

**Date**: 2026-02-18  
**Feature**: [spec.md](../spec.md)

## Overview

This contract defines the exact changes to be made to `.csproj` files. Since this feature involves no API endpoints or data schemas, the "contract" is the precise set of XML element modifications.

## Changes per File

### 1. `src/McpWorkshop.Shared/McpWorkshop.Shared.csproj`

```xml
<!-- BEFORE -->
<TargetFramework>net8.0</TargetFramework>

<!-- AFTER -->
<TargetFramework>net10.0</TargetFramework>
```

```xml
<!-- BEFORE -->
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0" />
<PackageReference Include="ModelContextProtocol" Version="0.4.0-preview.3" />

<!-- AFTER -->
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.3" />
<PackageReference Include="Microsoft.Extensions.Options" Version="10.0.3" />
<PackageReference Include="ModelContextProtocol" Version="0.8.0-preview.1" />
```

### 2. `src/McpWorkshop.Servers/SqlMcpServer/SqlMcpServer.csproj`

```xml
<!-- BEFORE -->
<PackageReference Include="ModelContextProtocol" Version="0.4.0-preview.3" />

<!-- AFTER -->
<PackageReference Include="ModelContextProtocol" Version="0.8.0-preview.1" />
```

### 3. `src/McpWorkshop.Servers/RestApiMcpServer/RestApiMcpServer.csproj`

```xml
<!-- BEFORE -->
<PackageReference Include="ModelContextProtocol" Version="0.4.0-preview.3" />

<!-- AFTER -->
<PackageReference Include="ModelContextProtocol" Version="0.8.0-preview.1" />
```

### 4. `src/McpWorkshop.Servers/CosmosMcpServer/CosmosMcpServer.csproj`

```xml
<!-- BEFORE -->
<PackageReference Include="ModelContextProtocol" Version="0.4.0-preview.3" />

<!-- AFTER -->
<PackageReference Include="ModelContextProtocol" Version="0.8.0-preview.1" />
```

### 5. `tests/McpWorkshop.Tests/McpWorkshop.Tests.csproj`

No direct changes required. Gets `ModelContextProtocol` transitively via project references.

## Verification

After applying all changes:

1. `dotnet restore McpWorkshop.sln` — must succeed with no errors
2. `dotnet build McpWorkshop.sln` — must succeed with zero errors and zero new warnings
3. `dotnet test McpWorkshop.sln` — all tests must pass
