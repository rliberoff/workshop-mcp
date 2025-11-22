# Implementation Plan: MCP Workshop Course

**Branch**: `001-mcp-workshop-course` | **Date**: 2025-11-17 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-mcp-workshop-course/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Create a comprehensive 3-hour MCP workshop with 11 structured blocks covering theory, live coding demonstrations, and progressive hands-on exercises. The workshop teaches attendees to build MCP servers for data exploitation scenarios using C# .NET with the ModelContextProtocol library (prerelease from NuGet). All infrastructure will be defined using Terraform for Azure deployment, documentation in Markdown, and diagrams in Mermaid. The workshop progresses from foundational MCP concepts through practical exercises (static resources, parametric queries, security/governance, multi-source orchestration) culminating in a group integrative challenge building a virtual analyst agent.

## Technical Context

| **Language** | C# .NET 10.0 |
**Primary Dependencies**: `ModelContextProtocol` (--prerelease from NuGet), Azure SDK for .NET, ASP.NET Core for hosting, Microsoft.Extensions.\* for DI/logging/configuration  
**Storage**: Azure SQL Database (relational examples), Azure Cosmos DB (NoSQL examples), Azure Blob Storage (static resources), local JSON files (offline exercises)  
**Testing**: xUnit for unit tests, Microsoft.AspNetCore.Mvc.Testing for integration tests, MCP protocol validation scripts  
**Target Platform**: Cross-platform (Windows/macOS/Linux developer workstations), Azure App Service / Azure Container Apps for hosted MCP servers  
**Project Type**: Multi-project solution (workshop content + multiple exercise projects + sample MCP servers)  
**Infrastructure**: Terraform for Azure resource provisioning (App Service Plans, Container Apps, Storage Accounts, SQL Database, Cosmos DB, Log Analytics)  
**Documentation**: Markdown for all instructional content, Mermaid for architecture and flow diagrams  
**Performance Goals**: MCP servers respond to tool/resource requests within 500ms p95, exercises completable within specified time constraints (15-30 min each)  
**Constraints**: Exercises must be runnable locally without Azure dependencies for offline scenarios, all samples must use standard MCP protocol (stdin/stdout or HTTP transport), maximum 3-hour total workshop duration  
**Scale/Scope**: 10-30 attendees per session, 11 distinct content blocks, 4 hands-on exercises, 5-10 sample MCP servers, comprehensive documentation covering MCP fundamentals through enterprise patterns

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Evaluation Against Constitution

✅ **I. MCP-First Design**: All workshop exercises demonstrate wrapping data sources (Azure SQL, Cosmos DB, REST APIs) as MCP resources and tools. No ad-hoc scripts outside MCP patterns.

✅ **II. Text & JSON Interfaces**: All MCP servers use standard stdin/stdout or HTTP transport with JSON payloads. Exercise verification uses curl/HTTP requests or CLI tools with structured outputs.

✅ **III. Test-First & Reproducibility**: Each exercise includes verification steps and test scripts. Local execution with fixed inputs produces deterministic outputs. Examples are reproducible across environments.

✅ **IV. Integration Over Isolation**: Exercise 4 specifically covers multi-source orchestration combining multiple MCP providers. Security block addresses authentication and error handling patterns.

✅ **V. Simplicity & Observability**: C# samples use ASP.NET Core built-in logging. Each MCP server surfaces request/response logs. Code prioritizes clarity for workshop context.

✅ **Technology Constraints**: C# .NET with Azure services (App Service, Container Apps, Functions, Storage, SQL, Cosmos, API Management, Monitor). No Power Platform or Copilot Studio. Python allowed for utilities if needed.

✅ **Development Workflow**: Documentation-first with Markdown, clear module organization, step-by-step exercises with verification scripts, and contribution guidelines.

**GATE STATUS: PASS** - No constitutional violations. All principles satisfied.

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
docs/                                    # Workshop documentation (Markdown)
├── modules/                            # Learning modules (theory + exercises)
│   ├── 01b-apertura.md               # Block 1: Opening (10 min)
│   ├── 02b-fundamentos.md            # Block 2: MCP concepts, architecture (25 min)
│   ├── 03b-anatomia-proveedor.md     # Block 3: Server structure, live coding (20 min)
│   ├── 04b-ejercicio-1-recursos-estaticos.md # Block 4: Exercise 1 (15 min)
│   ├── 05b-ejercicio-2-consultas-parametricas.md # Block 5: Exercise 2 (20 min)
│   ├── 06b-ejercicio-3-seguridad.md  # Block 6: Exercise 3 instructions (20 min)
│   ├── 07b-seguridad-gobernanza.md   # Block 7: Security sesión (15 min)
│   ├── 08-ejercicio-4-analista-virtual.md # Block 8: Exercise 4 group challenge (30 min)
│   ├── 09-orquestacion-multifuente.md # Block 9: Multi-source orchestration (15 min)
│   ├── 10-roadmap-casos-b2b.md       # Block 10: Business scenarios (10 min)
│   ├── 11-cierre.md                  # Block 11: Retrospective, closure (10 min)
│   └── diagrams/                      # Mermaid diagrams (shared)
├── AGENDA.md                          # Master workshop agenda
└── instructor-notes/                  # Facilitation guides per block

src/                                    # Sample MCP servers and utilities
├── McpWorkshop.Servers/               # Solution folder
│   ├── Exercise1StaticResources/      # Exercise 1 sample
│   │   ├── Program.cs
│   │   ├── Models/
│   │   ├── Resources/
│   │   └── Exercise1StaticResources.csproj
│   ├── Exercise2ParametricQuery/      # Exercise 2 sample
│   │   ├── Program.cs
│   │   ├── Tools/
│   │   ├── Services/
│   │   └── ParametricQueryServer.csproj
│   ├── SecureServer/                  # Exercise 3 sample
│   │   ├── Program.cs
│   │   ├── Middleware/
│   │   ├── Authorization/
│   │   └── SecureServer.csproj
│   ├── DataSourceConnectors/          # Exercise 4 components
│   │   ├── SqlConnector/
│   │   ├── CosmosConnector/
│   │   ├── RestApiConnector/
│   │   └── DataSourceConnectors.csproj
│   ├── VirtualAnalyst/                # Exercise 4 integrator
│   │   ├── Program.cs
│   │   ├── Orchestration/
│   │   ├── AgentLogic/
│   │   └── VirtualAnalyst.csproj
│   └── McpWorkshop.Shared/            # Common utilities
│       ├── Logging/
│       ├── Configuration/
│       ├── Testing/
│       └── McpWorkshop.Shared.csproj

tests/                                  # Verification and testing
├── McpWorkshop.Tests/
│   ├── Unit/
│   ├── Integration/
│   └── Protocol/                      # MCP protocol validation
└── McpWorkshop.Tests.csproj

infrastructure/                         # Azure provisioning
├── terraform/
│   ├── modules/
│   │   ├── app-service/               # App Service module
│   │   ├── container-apps/            # Container Apps module
│   │   ├── storage/                   # Storage Account module
│   │   ├── sql-database/              # Azure SQL module
│   │   ├── cosmos-db/                 # Cosmos DB module
│   │   └── monitoring/                # Log Analytics module
│   ├── environments/
│   │   ├── dev/
│   │   └── prod/
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   └── terraform.tfvars.example
└── scripts/                           # Deployment automation
    ├── deploy.ps1
    └── teardown.ps1

scripts/                                # Workshop utilities
├── verify-setup.ps1                   # Pre-workshop environment check
├── create-sample-data.ps1             # Generate test datasets
└── run-all-tests.ps1                  # Execute full test suite

templates/                              # Exercise starter templates
├── exercise1-starter/                 # Empty project for Exercise 1
├── exercise2-starter/                 # Scaffold for Exercise 2
├── exercise3-starter/                 # Base for Exercise 3
└── exercise4-starter/                 # Group challenge template

.github/
├── copilot-instructions.md            # Repository instructions
└── instructions/
    └── conventional-commits.instructions.md
```

**Structure Decision**: Multi-project workshop structure separating concerns:

-   **docs/**: Instructor and attendee-facing Markdown documentation organized by workshop block sequence
-   **src/**: Complete C# solutions for all exercises (reference implementations)
-   **tests/**: Comprehensive test suite for verification and protocol validation
-   **infrastructure/**: Terraform IaC for Azure deployment scenarios
-   **templates/**: Exercise starter code for attendees to build upon
-   **scripts/**: Automation for setup verification, data generation, and testing

This structure supports self-paced learning, instructor-led delivery, local development, and cloud deployment scenarios.

**Structure Decision**: Multi-project workshop structure separating concerns:

-   **docs/**: Instructor and attendee-facing Markdown documentation organized by workshop block sequence
-   **src/**: Complete C# solutions for all exercises (reference implementations)
-   **tests/**: Comprehensive test suite for verification and protocol validation
-   **infrastructure/**: Terraform IaC for Azure deployment scenarios
-   **templates/**: Exercise starter code for attendees to build upon
-   **scripts/**: Automation for setup verification, data generation, and testing

This structure supports self-paced learning, instructor-led delivery, local development, and cloud deployment scenarios.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations identified. Constitution check passed completely - no complexity tracking required.

---

## Post-Phase-1 Constitution Re-Evaluation

**Date**: 2025-11-17  
**Status**: ✅ PASS - All principles maintained after design phase

### Design Artifacts Review

**Artifacts Created**:

-   ✅ research.md: Technology decisions and best practices
-   ✅ data-model.md: Domain entities and relationships
-   ✅ contracts/: MCP protocol specifications (5 files)
-   ✅ quickstart.md: Environment setup guide

### Compliance Verification

✅ **I. MCP-First Design**: All contracts specify JSON-RPC 2.0 over MCP protocol. Data model entities (WorkshopBlock, PracticalExercise, McpServerComponent) enforce MCP patterns. No escape hatches to non-MCP interfaces.

✅ **II. Text & JSON Interfaces**: Contract specifications mandate JSON payloads for all MCP messages. Verification scripts use HTTP POST with JSON bodies. Quickstart guide demonstrates curl-based testing.

✅ **III. Test-First & Reproducibility**: Each exercise contract includes verificationScript section with PowerShell commands. Quickstart includes verify-setup.ps1 for environment validation. Data model specifies deterministic sample data.

✅ **IV. Integration Over Isolation**: Exercise 4 contract (virtual-analyst.json) explicitly defines multi-source orchestration patterns (parallel, sequential, fanOut, caching). Architecture diagram in data-model.md shows integration flows.

✅ **V. Simplicity & Observability**: Contracts require structured logging with specific fields (timestamp, userId, requestId, duration). Exercise 3 focuses entirely on logging and observability patterns. Quickstart emphasizes clear troubleshooting steps.

✅ **Technology Constraints**: Research document confirms C# .NET 8.0, ModelContextProtocol library, Azure services only. No Power Platform references. Terraform for IaC as specified.

✅ **Development Workflow**: Documentation artifacts follow Markdown-first approach. Data model separates conceptual entities from implementation. Contracts use JSON Schema for validation. Quickstart provides step-by-step instructions.

### Changes Since Initial Check

**None** - Design phase maintained all constitutional principles. No new violations introduced.

### Recommendations for Implementation Phase

1. **Test Coverage**: Ensure xUnit test projects follow test-first principle for each MCP server component
2. **Protocol Validation**: Implement JSON Schema validators for all contract specifications in tests/Protocol/
3. **Logging Standards**: Create shared logging library (McpWorkshop.Shared/Logging/) enforcing structured format
4. **Documentation Review**: Before each module implementation, verify Markdown content follows template structure

**Final Status**: ✅ **CLEARED FOR PHASE 2 (TASKS)** - All gates passed, no blockers identified.

---

## Next Steps

This completes the `/speckit.plan` command execution. The following artifacts have been generated:

-   ✅ **plan.md**: Technical context, constitution checks, project structure
-   ✅ **research.md**: Technology decisions and alternatives analysis
-   ✅ **data-model.md**: Domain entities, relationships, and state diagrams
-   ✅ **contracts/**: MCP protocol specifications for all exercises
-   ✅ **quickstart.md**: Environment setup and first server deployment
-   ✅ **Agent context updated**: GitHub Copilot instructions refreshed

**Ready for**: `/speckit.tasks` command to break down implementation into actionable work items.

**Branch**: `001-mcp-workshop-course`  
**Spec Directory**: `specs/001-mcp-workshop-course/`

To begin implementation planning, run:

```bash
# Generate task breakdown (not part of /speckit.plan)
/speckit.tasks
```
