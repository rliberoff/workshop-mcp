# Implementation Plan: Update ModelContextProtocol SDK to 0.9.0-preview.1

**Branch**: `002-update-mcp-sdk` | **Date**: 2026-02-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-update-mcp-sdk/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Update the `ModelContextProtocol` NuGet package from v0.4.0-preview.3 to v0.9.0-preview.1 across all four projects that reference it. Upgrade `McpWorkshop.Shared` target framework from `net8.0` to `net10.0`. Proactively update all `Microsoft.Extensions.*` packages to their latest versions. Ensure the solution compiles with zero errors/warnings and all tests pass.

**Critical finding from research**: The `ModelContextProtocol` SDK is referenced in 4 `.csproj` files but its API surface is **not consumed in any C# source file**. The codebase implements MCP Tools, Resources, and Prompts manually via custom JSON-RPC types in `McpWorkshop.Shared.Mcp`. The migration is primarily a package version bump + framework alignment, with minimal risk of breaking API changes in source code. Documentation code samples for Exercise 5 may need review for updated SDK namespaces.

## Technical Context

**Language/Version**: C# / .NET 10.0 (servers, tests), .NET 8.0 → 10.0 (Shared library)
**Primary Dependencies**: `ModelContextProtocol` 0.4.0-preview.3 → 0.9.0-preview.1, `Microsoft.Extensions.Logging.Abstractions` 10.0.3, `Microsoft.Extensions.Options` 10.0.3, `Microsoft.AspNetCore.Http.Abstractions` 2.3.0, `StyleCop.Analyzers` 1.2.0-beta.556
**Storage**: JSON files (local data), Azure SQL, Azure Cosmos DB (via servers)
**Testing**: xUnit 2.9.3 + Moq 4.20.72 via `dotnet test`
**Target Platform**: Windows/Linux (ASP.NET Core web servers)
**Project Type**: Multi-project solution (3 MCP servers + 1 shared library + 1 test project)
**Performance Goals**: N/A (dependency update, no behavioral changes)
**Constraints**: Zero compilation errors, zero new warnings, all existing tests must pass
**Scale/Scope**: 4 `.csproj` files to update, 1 target framework change, potential `Microsoft.Extensions.*` version bumps, 1 documentation file to review

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. MCP-First Design | PASS | Feature preserves MCP-first approach. Only updating the SDK dependency, not changing architecture. |
| II. Text & JSON Interfaces | PASS | No interface changes — all existing JSON-RPC endpoints preserved. |
| III. Test-First & Reproducibility | PASS | All existing tests must continue to pass. No new untested code introduced. |
| IV. Integration Over Isolation | PASS | Integration scenarios unaffected — dependency update only. |
| V. Simplicity & Observability | PASS | No complexity added. Logging and diagnostics unchanged. |
| Languages & Runtimes (C#/.NET) | PASS | Stays within C# (.NET 10.0). Framework alignment improves consistency. |
| Azure Services | PASS | No Azure service changes. |
| Exclusions (no Power Platform) | PASS | Not applicable. |
| Local & Cloud Environments | PASS | Exercises remain runnable locally and deployable to Azure. |
| Documentation-First Flow | PASS | Exercise 5 documentation code samples will be reviewed for SDK namespace accuracy. |
| Exercise Design (clear start/steps/outcome) | PASS | No exercise structure changes. |
| Verification & Testing | PASS | Existing verification scripts preserved. |

**Gate result: PASS** — No violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── McpWorkshop.Shared/                    # TFM: net8.0 → net10.0 (upgrade)
│   ├── McpWorkshop.Shared.csproj          # Update ModelContextProtocol + Microsoft.Extensions.*
│   ├── Configuration/
│   ├── Logging/
│   ├── Mcp/                               # Custom MCP types (no SDK usage — no changes)
│   ├── Monitoring/
│   └── Security/
├── McpWorkshop.Servers/
│   ├── SqlMcpServer/
│   │   ├── SqlMcpServer.csproj            # Update ModelContextProtocol version
│   │   ├── Program.cs                     # Manual MCP (no SDK usage — no changes)
│   │   └── Tools/                         # Static tool classes (no SDK attributes — no changes)
│   ├── RestApiMcpServer/
│   │   ├── RestApiMcpServer.csproj        # Update ModelContextProtocol version
│   │   ├── Program.cs                     # Manual MCP (no SDK usage — no changes)
│   │   └── Tools/
│   ├── CosmosMcpServer/
│   │   ├── CosmosMcpServer.csproj         # Update ModelContextProtocol version
│   │   ├── Program.cs                     # Manual MCP (no SDK usage — no changes)
│   │   └── Tools/
│   └── Exercise5Agent/                    # Placeholder (no source, bin/obj only)

tests/
└── McpWorkshop.Tests/
    ├── McpWorkshop.Tests.csproj           # Transitive MCP ref (no direct changes needed)
    └── *.cs                               # Tests use custom Shared.Mcp types (no SDK types)

docs/
└── modules/
    └── 09b-ejercicio-5-agente-maf.md      # Review for SDK namespace accuracy
```

**Structure Decision**: Existing multi-project structure is preserved. This feature modifies `.csproj` files only — no new files, no structural changes. The only source code risk is if the new SDK introduces assembly-level type conflicts with the custom `McpWorkshop.Shared.Mcp` namespace.

## Complexity Tracking

No constitution violations detected. This section is intentionally left empty.

## Constitution Re-Check (Post-Design)

*All principles re-evaluated after Phase 1 design artifacts were produced.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. MCP-First Design | PASS | No architectural changes — MCP-first approach preserved. |
| II. Text & JSON Interfaces | PASS | All JSON-RPC interfaces unchanged. |
| III. Test-First & Reproducibility | PASS | Existing tests and verification scripts preserved. |
| IV. Integration Over Isolation | PASS | Integration scenarios unaffected. |
| V. Simplicity & Observability | PASS | No complexity added. |
| Languages & Runtimes | PASS | C#/.NET 10.0 throughout. |
| Azure Services | PASS | No changes. |
| Exclusions | PASS | N/A. |
| Local & Cloud Environments | PASS | Both preserved. |
| Documentation-First Flow | PASS | Exercise 5 doc reviewed for SDK accuracy. |
| Exercise Design | PASS | No exercise structure changes. |
| Verification & Testing | PASS | All verification scripts preserved. |

**Post-design gate: PASS** — No violations. Ready for Phase 2 (`/speckit.tasks`).

## Phase 0 Artifacts

- [research.md](research.md) — SDK availability, breaking changes, source code analysis, dependency versions

## Phase 1 Artifacts

- [data-model.md](data-model.md) — Package reference state transitions per project
- [contracts/csproj-changes.md](contracts/csproj-changes.md) — Exact XML changes per `.csproj` file
- [quickstart.md](quickstart.md) — Step-by-step migration guide
