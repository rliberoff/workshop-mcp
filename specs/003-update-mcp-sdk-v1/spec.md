# Feature Specification: Update ModelContextProtocol SDK to v1.0.0

**Feature Branch**: `003-update-mcp-sdk-v1`  
**Created**: 2026-02-26  
**Status**: Draft  
**Input**: User description: "Update ModelContextProtocol library and its dependencies from v0.9.0-preview.1 to v1.0.0, ensuring all code compiles and any errors or warnings are corrected."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Package Version Update Across All Projects (Priority: P1)

As a workshop instructor, I need the `ModelContextProtocol` package updated from v0.9.0-preview.1 to v1.0.0 in all projects that reference it, and all `Microsoft.Extensions.*` packages proactively updated to their latest compatible versions, so that the workshop exercises use the stable SDK release and a consistent dependency set.

**Why this priority**: This is the foundation — without updating the package references, no other work can proceed. All four projects (McpWorkshop.Shared, SqlMcpServer, RestApiMcpServer, CosmosMcpServer) must reference the new version.

**Independent Test**: Test by restoring NuGet packages across the solution and verifying all four projects resolve the correct package version.

**Acceptance Scenarios**:

1. **Given** the solution with `ModelContextProtocol` v0.9.0-preview.1 referenced in four projects, **When** the package version is updated, **Then** all four `.csproj` files reference `ModelContextProtocol` v1.0.0
2. **Given** the updated package references, **When** NuGet restore is executed, **Then** all packages resolve successfully without version conflicts
3. **Given** the updated package references, **When** `.csproj` files are inspected, **Then** no references to the `--prerelease` flag or prerelease version strings remain

---

### User Story 2 - Compilation Compatibility (Priority: P1)

As a workshop instructor, I need the entire solution to compile without errors after the SDK update, so that workshop participants can build and run all exercises without issues.

**Why this priority**: A non-compiling solution renders the workshop unusable. Breaking API changes between v0.9.0-preview.1 and v1.0.0 must be identified and resolved. Specifically, the `RunSessionHandler` property on `HttpServerTransportOptions` is now marked as experimental (`MCPEXP002`) and will produce compile-time warnings or errors under `TreatWarningsAsErrors`.

**Independent Test**: Test by building the entire solution and verifying zero compilation errors across all projects.

**Acceptance Scenarios**:

1. **Given** the updated package references, **When** the solution is built, **Then** all projects compile successfully with zero errors
2. **Given** API changes in the new SDK version, **When** existing code uses deprecated or renamed APIs, **Then** the code is updated to use the new API equivalents
3. **Given** the experimental `MCPEXP002` annotation on `RunSessionHandler`, **When** any code references `RunSessionHandler`, **Then** it is either replaced with `ConfigureSessionOptions` (preferred) or suppressed with `#pragma warning disable MCPEXP002`
4. **Given** the updated solution, **When** the build output is reviewed, **Then** there are no new compiler warnings introduced by the migration

---

### User Story 3 - Test Suite Integrity (Priority: P2)

As a workshop instructor, I need all existing unit tests to pass after the SDK update, so that I can be confident the workshop exercises function correctly.

**Why this priority**: Passing tests provide confidence that the behavior observed by workshop participants matches expectations, even after the underlying SDK changes.

**Independent Test**: Test by running the full test suite and verifying all tests pass.

**Acceptance Scenarios**:

1. **Given** the updated and compiling solution, **When** the test suite is executed, **Then** all existing tests pass without modification, or are updated to reflect legitimate API changes
2. **Given** tests that reference SDK-specific types or behaviors, **When** those types or behaviors have changed, **Then** the tests are updated to align with the new SDK version while preserving test intent

---

### User Story 4 - Workshop Documentation Accuracy (Priority: P2)

As a workshop participant, I need all documentation and exercise instructions to reflect the v1.0.0 SDK APIs, so that code samples in the materials compile and run correctly.

**Why this priority**: Exercise 5 documentation contains detailed code samples using SDK types (`McpClient`, `HttpClientTransport`, `HttpClientTransportOptions`, `StdioClientTransport`, `McpClientTool`, `CallToolResult`) and namespaces (`ModelContextProtocol.Client`, `ModelContextProtocol.Protocol`). If these APIs changed in v1.0.0, the documentation must be updated to match. Additionally, the `--prerelease` flag in `dotnet add package` instructions must be removed since v1.0.0 is a stable release.

**Independent Test**: Test by following the Exercise 5 instructions with the updated solution and verifying all code samples compile and produce expected results.

**Acceptance Scenarios**:

1. **Given** the Exercise 5 documentation references `dotnet add package ModelContextProtocol --prerelease`, **When** the package is now a stable v1.0.0 release, **Then** the `--prerelease` flag is removed from the documentation
2. **Given** SDK type names, namespaces, or method signatures changed between v0.9.0-preview.1 and v1.0.0, **When** those types appear in workshop documentation code samples, **Then** the documentation is updated to use the correct v1.0.0 API names
3. **Given** instructor materials reference SDK error troubleshooting (namespace issues, transport patterns), **When** the SDK version changes, **Then** the troubleshooting guides are updated to reflect v1.0.0 behavior

---

### User Story 5 - Workshop Exercise Continuity (Priority: P2)

As a workshop participant, I need all MCP server exercises to remain functional after the SDK update, so that I can follow the workshop modules without encountering runtime issues.

**Why this priority**: The workshop has five exercises that rely on the MCP servers. Any runtime regressions would disrupt the learning experience.

**Independent Test**: Test by starting each MCP server and executing the corresponding exercise HTTP requests.

**Acceptance Scenarios**:

1. **Given** the updated solution, **When** each MCP server (SQL, REST API, Cosmos) is started, **Then** it initializes and accepts connections without errors
2. **Given** the exercise HTTP request files, **When** requests are sent to the running servers, **Then** responses match the expected behavior defined in the exercise modules

---

### Edge Cases

- What happens if the v1.0.0 release introduces new required configuration or startup options that were optional in v0.9.0-preview.1?
- How does the system handle transitive dependency conflicts where v1.0.0 pulls in different versions of shared dependencies (e.g., `Microsoft.Extensions.*`)?
- What happens if tool or resource registration APIs have new required parameters or different method signatures?
- What if the target `Microsoft.Extensions.*` latest version introduces binding conflicts with v1.0.0? **Resolution**: Fall back to the highest stable version compatible with both the SDK and the solution's target frameworks. Verify via `dotnet restore`; if restore fails, decrement the patch version until restore succeeds.
- What if v1.0.0 changes client-side SDK types (`McpClient`, `HttpClientTransport`, `CallToolResult`) that are referenced in workshop documentation but not in compiled code? **Resolution**: Update documentation code samples to match the new API surface.
- What if experimental APIs marked with `MCPEXP002` are used in the codebase and `TreatWarningsAsErrors` is enabled? **Resolution**: Prefer migrating to `ConfigureSessionOptions`; only suppress if no stable alternative exists.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: All projects referencing `ModelContextProtocol` MUST be updated to version 1.0.0
- **FR-002**: The solution MUST compile successfully with zero errors after the update
- **FR-003**: The solution MUST compile with zero new compiler warnings introduced by the migration, measured against the baseline warning count captured in a pre-migration build. A warning is considered "new" if it does not appear in the baseline build output.
- **FR-004**: All existing unit tests MUST pass after the update (tests may be updated to reflect legitimate API changes)
- **FR-005**: Any breaking API changes between v0.9.0-preview.1 and v1.0.0 — including removed, renamed, or restructured types and methods — MUST be identified and resolved in the codebase
- **FR-006**: Any new or changed transitive dependencies MUST be compatible with the existing project target frameworks (`net10.0`)
- **FR-007**: The workshop exercise behavior MUST be preserved — all MCP tools, resources, and prompts MUST remain functionally equivalent after the update, as verified by exercise verification scripts producing identical results
- **FR-008**: Any deprecated APIs flagged by the new SDK version via compiler warnings MUST be replaced with their recommended alternatives (distinct from FR-005 which covers removals/renames that cause compilation errors)
- **FR-009**: Code referencing `HttpServerTransportOptions.RunSessionHandler` (now experimental `MCPEXP002`) MUST be migrated to use `ConfigureSessionOptions` where possible; if no stable alternative exists, the experimental warning MUST be suppressed with `#pragma warning disable MCPEXP002` and a code comment explaining the rationale
- **FR-010**: Workshop documentation code samples in Exercise 5 materials MUST be updated to reflect v1.0.0 API names, namespaces, and method signatures for: `McpClient`, `HttpClientTransport`, `HttpClientTransportOptions`, `StdioClientTransport`, `McpClientTool`, `CallToolResult`
- **FR-011**: The `dotnet add package ModelContextProtocol --prerelease` instruction in Exercise 5 documentation MUST be updated to `dotnet add package ModelContextProtocol` (removing the `--prerelease` flag) since v1.0.0 is a stable release
- **FR-012**: Instructor troubleshooting guides that reference SDK namespace patterns or transport behavior MUST be reviewed and updated for v1.0.0 accuracy

### Key Entities

- **Package Reference**: A NuGet package dependency declared in a `.csproj` file, with a name and version. Four projects reference `ModelContextProtocol` directly.
- **MCP Server**: An application that exposes tools, resources, and/or prompts via the Model Context Protocol. Three servers exist (SQL, REST API, Cosmos). Currently implemented using custom JSON-RPC handling rather than SDK server APIs.
- **MCP Tool/Resource/Prompt**: Capabilities registered in each MCP server that may be affected by SDK API changes in registration, invocation, or type signatures.
- **Workshop Documentation**: Markdown files in `docs/modules/` containing code samples, setup instructions, and troubleshooting guides that reference SDK types and APIs — primarily Exercise 5 (`09b-ejercicio-5-agente-maf.md`) and the instructor guide (`09a-ejercicio-5-instructor.md`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four projects reference `ModelContextProtocol` v1.0.0 (verifiable via `.csproj` inspection)
- **SC-002**: Solution builds with zero errors and zero new warnings compared to a pre-migration baseline (verifiable by diffing `dotnet build` output before and after migration)
- **SC-003**: 100% of existing tests pass after the update (verifiable via `dotnet test` output)
- **SC-004**: All three MCP servers start successfully and respond to requests (verifiable via exercise verification scripts)
- **SC-005**: No regression in workshop exercise functionality — participants can complete all exercises as documented
- **SC-006**: All workshop documentation code samples compile and execute correctly with v1.0.0 (verifiable by following Exercise 5 instructions end-to-end)
- **SC-007**: No references to prerelease version strings or `--prerelease` flags remain in documentation or project files

## Assumptions

- The `ModelContextProtocol` v1.0.0 package is available on the NuGet feed (nuget.org or configured sources).
- The codebase currently uses custom JSON-RPC protocol handling rather than SDK server-side APIs; therefore, the primary impact of server-side SDK breaking changes is limited to transitive dependencies rather than direct API usage.
- The workshop documentation (Exercise 5) uses SDK client-side types (`McpClient`, `HttpClientTransport`, etc.) in code samples that participants type during the workshop; these must be verified and updated for v1.0.0 compatibility.
- Any API changes between v0.9.0-preview.1 and v1.0.0 are documented in release notes or discoverable via compiler errors and warnings.
- The workshop's functional behavior (what each tool does, what data it returns) does not need to change — only the underlying SDK API calls may need adaptation.
- All `Microsoft.Extensions.*` packages in the solution will be proactively updated to their latest stable versions alongside the SDK update to ensure consistency and avoid binding conflicts.
- The `RunSessionHandler` experimental annotation (`MCPEXP002`) is a known breaking change; migration to `ConfigureSessionOptions` is the preferred approach.
- Since v1.0.0 is a stable release (not prerelease), the `--prerelease` NuGet flag used in Exercise 5 instructions is no longer needed.
