# Tasks: Update ModelContextProtocol SDK to 0.8.0-preview.1

**Input**: Design documents from `/specs/002-update-mcp-sdk/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: No new tests requested. Existing tests must continue to pass.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Baseline verification before any changes

- [X] T001 Verify current solution builds successfully by running `dotnet build McpWorkshop.sln`
- [X] T002 Verify current tests pass by running `dotnet test McpWorkshop.sln`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Target framework alignment — MUST be complete before package updates to avoid TFM conflicts

**⚠️ CRITICAL**: The TFM upgrade must happen first so all projects share `net10.0` before changing SDK versions

- [X] T003 [US1] Update `TargetFramework` from `net8.0` to `net10.0` in src/McpWorkshop.Shared/McpWorkshop.Shared.csproj (FR-009)
- [X] T004 [US1] Run `dotnet restore McpWorkshop.sln` and verify successful restoration after TFM change (FR-009)
- [X] T005 [US1] Run `dotnet build McpWorkshop.sln` and fix any errors or warnings introduced by the TFM change (FR-009)

**Checkpoint**: All projects target `net10.0`. Solution builds cleanly. Ready for package updates.

---

## Phase 3: User Story 1 — Package Version Update Across All Projects (Priority: P1) 🎯 MVP

**Goal**: Update `ModelContextProtocol` from v0.4.0-preview.3 to v0.8.0-preview.1 in all four projects, and update `Microsoft.Extensions.*` packages to v10.0.3 in McpWorkshop.Shared

**Independent Test**: Run `dotnet restore McpWorkshop.sln` — all packages resolve without version conflicts

### Implementation for User Story 1

- [X] T006 [P] [US1] Update `ModelContextProtocol` version from `0.4.0-preview.3` to `0.8.0-preview.1` in src/McpWorkshop.Shared/McpWorkshop.Shared.csproj
- [X] T007 [P] [US1] Update `Microsoft.Extensions.Logging.Abstractions` version from `10.0.0` to `10.0.3` in src/McpWorkshop.Shared/McpWorkshop.Shared.csproj
- [X] T008 [P] [US1] Update `Microsoft.Extensions.Options` version from `10.0.0` to `10.0.3` in src/McpWorkshop.Shared/McpWorkshop.Shared.csproj
- [X] T009 [P] [US1] Update `ModelContextProtocol` version from `0.4.0-preview.3` to `0.8.0-preview.1` in src/McpWorkshop.Servers/SqlMcpServer/SqlMcpServer.csproj
- [X] T010 [P] [US1] Update `ModelContextProtocol` version from `0.4.0-preview.3` to `0.8.0-preview.1` in src/McpWorkshop.Servers/RestApiMcpServer/RestApiMcpServer.csproj
- [X] T011 [P] [US1] Update `ModelContextProtocol` version from `0.4.0-preview.3` to `0.8.0-preview.1` in src/McpWorkshop.Servers/CosmosMcpServer/CosmosMcpServer.csproj
- [X] T012 [US1] Run `dotnet restore McpWorkshop.sln` and verify all packages resolve without version conflicts

**Checkpoint**: All four projects reference `ModelContextProtocol` v0.8.0-preview.1. `Microsoft.Extensions.*` updated to 10.0.3. NuGet restore succeeds.

---

## Phase 4: User Story 2 — Compilation Compatibility (Priority: P1)

**Goal**: Ensure the entire solution compiles with zero errors and zero new warnings after the package updates

**Independent Test**: Run `dotnet build McpWorkshop.sln` — zero errors, zero new warnings

### Implementation for User Story 2

- [X] T013 [US2] Run `dotnet build McpWorkshop.sln` and capture the full build output
- [X] T014 [US2] Fix any compilation errors in src/McpWorkshop.Shared/ caused by breaking API changes from the SDK update
- [X] T015 [P] [US2] Fix any compilation errors in src/McpWorkshop.Servers/SqlMcpServer/ caused by breaking API changes
- [X] T016 [P] [US2] Fix any compilation errors in src/McpWorkshop.Servers/RestApiMcpServer/ caused by breaking API changes
- [X] T017 [P] [US2] Fix any compilation errors in src/McpWorkshop.Servers/CosmosMcpServer/ caused by breaking API changes
- [X] T018 [US2] Fix any compilation errors in tests/McpWorkshop.Tests/ caused by transitive dependency changes
- [X] T019 [US2] Review and resolve any new compiler warnings across all projects (run `dotnet build McpWorkshop.sln --warnaserror` to verify)

**Checkpoint**: Solution builds with zero errors and zero new warnings. SC-002 verified.

---

## Phase 5: User Story 3 — Test Suite Integrity (Priority: P2)

**Goal**: All existing unit tests pass after the update

**Independent Test**: Run `dotnet test McpWorkshop.sln` — 100% pass rate

### Implementation for User Story 3

- [X] T020 [US3] Run `dotnet test McpWorkshop.sln` and capture test results
- [X] T021 [US3] Fix any test failures caused by SDK type changes or transitive dependency updates in tests/McpWorkshop.Tests/
- [X] T022 [US3] Verify all tests pass with `dotnet test McpWorkshop.sln` (final confirmation)

**Checkpoint**: 100% of existing tests pass. SC-003 verified.

---

## Phase 6: User Story 4 — Workshop Exercise Continuity (Priority: P2)

**Goal**: All exercise documentation and verification scripts remain accurate after the migration

**Independent Test**: Run exercise verification scripts; review Exercise 5 doc code samples for SDK accuracy

### Implementation for User Story 4

- [X] T023 [US4] Review code samples in docs/modules/09b-ejercicio-5-agente-maf.md for SDK v0.8.0-preview.1 API accuracy (check `ListToolsAsync`, `CallToolAsync`, `RequestOptions` signatures)
- [X] T024 [US4] Update any outdated SDK code samples in docs/modules/09b-ejercicio-5-agente-maf.md to match v0.8.0-preview.1 API
- [X] T025 [US4] Run scripts/run-all-tests.ps1 to verify all exercise verification scripts pass (covers exercises 1–4 via verify-exercise*.ps1; Exercise 5 is doc-only and validated by T023/T024)

**Checkpoint**: Exercise 5 documentation is accurate for v0.8.0-preview.1. All verification scripts pass. SC-004, SC-005 verified.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [X] T026 Run full solution clean build: `dotnet clean McpWorkshop.sln` then `dotnet build McpWorkshop.sln`
- [X] T027 Verify all `.csproj` files match expected state from contracts/csproj-changes.md
- [X] T028 Run quickstart.md validation — execute the quickstart steps end-to-end to confirm they are accurate

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — baseline verification
- **Foundational (Phase 2)**: Depends on Setup — TFM upgrade BLOCKS all package updates
- **US1 (Phase 3)**: Depends on Foundational — package version changes
- **US2 (Phase 4)**: Depends on US1 — can only compile after packages are updated
- **US3 (Phase 5)**: Depends on US2 — can only run tests after compilation succeeds
- **US4 (Phase 6)**: Depends on US2 — doc review can happen after compilation confirms API surface
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational (Phase 2) only — no cross-story dependencies
- **US2 (P1)**: Depends on US1 — must have updated packages before compiling
- **US3 (P2)**: Depends on US2 — must compile before testing
- **US4 (P2)**: Depends on US2 — doc review requires knowledge of final API surface; can run in parallel with US3

### Parallel Opportunities

- T006–T011 (all `.csproj` updates) can run in parallel — different files, no dependencies
- T015–T017 (server compilation fixes) can run in parallel — different projects
- US3 and US4 can run in parallel after US2 completes

---

## Parallel Example: User Story 1

```bash
# Launch all .csproj updates together (6 tasks in parallel):
Task T006: "Update ModelContextProtocol in McpWorkshop.Shared.csproj"
Task T007: "Update Microsoft.Extensions.Logging.Abstractions in McpWorkshop.Shared.csproj"
Task T008: "Update Microsoft.Extensions.Options in McpWorkshop.Shared.csproj"
Task T009: "Update ModelContextProtocol in SqlMcpServer.csproj"
Task T010: "Update ModelContextProtocol in RestApiMcpServer.csproj"
Task T011: "Update ModelContextProtocol in CosmosMcpServer.csproj"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (baseline)
2. Complete Phase 2: Foundational (TFM upgrade)
3. Complete Phase 3: US1 (package updates)
4. Complete Phase 4: US2 (compilation fixes)
5. **STOP and VALIDATE**: Solution builds with zero errors — MVP delivered
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → TFM aligned
2. Add US1 → Packages updated → NuGet restore succeeds
3. Add US2 → Solution compiles → Zero errors/warnings
4. Add US3 → Tests pass → Confidence in correctness
5. Add US4 → Documentation accurate → Workshop-ready
6. Polish → Final clean build → Ready for participants

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Research finding: SDK API is NOT consumed in source code — T014–T018 (compilation fix tasks) are likely no-ops but included for completeness
- No new tests requested — existing tests must pass post-migration
- Commit after each phase completion for clean rollback points
