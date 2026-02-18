# Feature Specification: Update ModelContextProtocol SDK to 0.8.0-preview.1

**Feature Branch**: `002-update-mcp-sdk`  
**Created**: 2026-02-18  
**Status**: Draft  
**Input**: User description: "Update ModelContextProtocol library and its dependencies from v0.4.0-preview.3 to v0.8.0-preview.1, ensuring all code compiles and any errors or warnings are corrected."

## Clarifications

### Session 2026-02-18

- Q: If `ModelContextProtocol` v0.8.0-preview.1 drops `net8.0` support, should `McpWorkshop.Shared` be upgraded, multi-targeted, or kept as-is? → A: Upgrade `McpWorkshop.Shared` to `net10.0` to match all other projects.
- Q: Should other packages (e.g., `Microsoft.Extensions.*`) be proactively updated alongside the SDK, or only if binding conflicts arise? → A: Proactively update all `Microsoft.Extensions.*` packages to their latest versions.

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Package Version Update Across All Projects (Priority: P1)

As a workshop instructor, I need the `ModelContextProtocol` package updated from v0.4.0-preview.3 to v0.8.0-preview.1 in all projects that reference it, and all `Microsoft.Extensions.*` packages proactively updated to their latest compatible versions, so that the workshop exercises use the latest SDK capabilities and a consistent dependency set.

**Why this priority**: This is the core deliverable — without updating the package references, no other work can proceed. All four projects (McpWorkshop.Shared, SqlMcpServer, RestApiMcpServer, CosmosMcpServer) must reference the new version. The `Microsoft.Extensions.*` updates are included here to avoid binding conflicts and ensure consistency (per clarification decision).

**Independent Test**: Can be fully tested by restoring NuGet packages across the solution and verifying all four projects resolve the correct package version.

**Acceptance Scenarios**:

1. **Given** the solution with `ModelContextProtocol` v0.4.0-preview.3 referenced in four projects, **When** the package version is updated, **Then** all four `.csproj` files reference `ModelContextProtocol` v0.8.0-preview.1
2. **Given** the updated package references, **When** NuGet restore is executed, **Then** all packages resolve successfully without version conflicts

---

### User Story 2 - Compilation Compatibility (Priority: P1)

As a workshop instructor, I need the entire solution to compile without errors after the SDK update, so that workshop participants can build and run all exercises without issues.

**Why this priority**: A non-compiling solution renders the workshop unusable. Breaking API changes between v0.4.0-preview.3 and v0.8.0-preview.1 must be identified and resolved.

**Independent Test**: Can be fully tested by building the entire solution and verifying zero compilation errors across all projects.

**Acceptance Scenarios**:

1. **Given** the updated package references, **When** the solution is built, **Then** all projects compile successfully with zero errors
2. **Given** API changes in the new SDK version, **When** existing code uses deprecated or renamed APIs, **Then** the code is updated to use the new API equivalents
3. **Given** the updated solution, **When** the build output is reviewed, **Then** there are no new compiler warnings introduced by the migration

---

### User Story 3 - Test Suite Integrity (Priority: P2)

As a workshop instructor, I need all existing unit tests to pass after the SDK update, so that I can be confident the workshop exercises function correctly.

**Why this priority**: Passing tests provide confidence that the behavior observed by workshop participants matches expectations, even after the underlying SDK changes.

**Independent Test**: Can be fully tested by running the full test suite and verifying all tests pass.

**Acceptance Scenarios**:

1. **Given** the updated and compiling solution, **When** the test suite is executed, **Then** all existing tests pass without modification, or are updated to reflect legitimate API changes
2. **Given** tests that reference SDK-specific types or behaviors, **When** those types or behaviors have changed, **Then** the tests are updated to align with the new SDK version while preserving test intent

---

### User Story 4 - Workshop Exercise Continuity (Priority: P2)

As a workshop participant, I need all MCP server exercises to remain functional after the SDK update, so that I can follow the workshop modules without encountering runtime issues.

**Why this priority**: The workshop has five exercises that rely on the MCP servers. Any runtime regressions would disrupt the learning experience.

**Independent Test**: Can be fully tested by starting each MCP server and executing the corresponding exercise HTTP requests.

**Acceptance Scenarios**:

1. **Given** the updated solution, **When** each MCP server (SQL, REST API, Cosmos) is started, **Then** it initializes and accepts connections without errors
2. **Given** the exercise HTTP request files, **When** requests are sent to the running servers, **Then** responses match the expected behavior defined in the exercise modules

---

### Edge Cases

- What happens if the new SDK version introduces new required configuration or startup options that were optional or non-existent in v0.4.0-preview.3?
- How does the system handle transitive dependency conflicts where the new SDK version pulls in different versions of shared dependencies (e.g., `Microsoft.Extensions.*`)?
- ~~What if the new SDK version drops support for the `net8.0` target framework used by `McpWorkshop.Shared`?~~ **Resolved**: `McpWorkshop.Shared` will be upgraded to `net10.0` to align with all other projects.
- What happens if tool or resource registration APIs have new required parameters or different method signatures?
- What if the target `Microsoft.Extensions.*` version (10.0.3) is unavailable or introduces binding conflicts? **Resolution**: Fall back to the highest stable version compatible with the solution's target frameworks. Verify via `dotnet restore`; if restore fails, decrement the patch version until restore succeeds.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: All projects referencing `ModelContextProtocol` MUST be updated to version 0.8.0-preview.1
- **FR-002**: The solution MUST compile successfully with zero errors after the update
- **FR-003**: The solution MUST compile with zero new compiler warnings introduced by the migration, measured against the baseline warning count captured in T001 (pre-migration build). A warning is considered "new" if it does not appear in the baseline build output.
- **FR-004**: All existing unit tests MUST pass after the update (tests may be updated to reflect legitimate API changes)
- **FR-005**: Any breaking API changes between v0.4.0-preview.3 and v0.8.0-preview.1 — including removed, renamed, or restructured types and methods — MUST be identified and resolved in the codebase
- **FR-006**: Any new or changed transitive dependencies MUST be compatible with the existing project target frameworks
- **FR-007**: The workshop exercise behavior MUST be preserved — all MCP tools, resources, and prompts MUST remain functionally equivalent after the update, as verified by exercise verification scripts producing identical results
- **FR-008**: Any deprecated APIs flagged by the new SDK version via compiler warnings MUST be replaced with their recommended alternatives (distinct from FR-005 which covers removals/renames that cause compilation errors)
- **FR-009**: The `McpWorkshop.Shared` project MUST be upgraded from `net8.0` to `net10.0` to align its target framework with all other projects in the solution

### Key Entities

- **Package Reference**: A NuGet package dependency declared in a `.csproj` file, with a name and version. Four projects reference `ModelContextProtocol` directly.
- **MCP Server**: An application that exposes tools, resources, and/or prompts via the Model Context Protocol. Three servers exist (SQL, REST API, Cosmos).
- **MCP Tool/Resource/Prompt**: Capabilities registered in each MCP server that may be affected by SDK API changes in registration, invocation, or type signatures.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four projects reference `ModelContextProtocol` v0.8.0-preview.1 (verifiable via `.csproj` inspection)
- **SC-002**: Solution builds with zero errors and zero new warnings compared to the T001 baseline (verifiable by diffing `dotnet build` output before and after migration)
- **SC-003**: 100% of existing tests pass after the update (verifiable via `dotnet test` output)
- **SC-004**: All three MCP servers start successfully and respond to requests (verifiable via exercise verification scripts)
- **SC-005**: No regression in workshop exercise functionality — participants can complete all exercises as documented

## Assumptions

- The `ModelContextProtocol` v0.8.0-preview.1 package is available on the NuGet feed (nuget.org or configured sources).
- `McpWorkshop.Shared` will be upgraded from `net8.0` to `net10.0` to align with all other projects, regardless of whether the new SDK requires it.
- Any API changes between v0.4.0-preview.3 and v0.8.0-preview.1 are documented in release notes or discoverable via compiler errors.
- The workshop's functional behavior (what each tool does, what data it returns) does not need to change — only the underlying SDK API calls may need adaptation.
- All `Microsoft.Extensions.*` packages in the solution will be proactively updated to their latest versions (currently 10.0.3) alongside the SDK update to ensure consistency and avoid binding conflicts. If the target version is unavailable or causes restore failures, the highest compatible stable version will be used instead.
