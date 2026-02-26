# Tasks: Update ModelContextProtocol SDK to v1.0.0

Legend: `[P]` = parallelizable task

## Phase 1: Setup

- [ ] T001 Create branch `003-update-mcp-sdk-v1` in repo — create a feature branch for the work
- [ ] T002 [P] Ensure all prerequisite scripts are available in `.specify/scripts/powershell/` — verify helper scripts exist

## Phase 2: Foundational

- [ ] T003 [P] Backup `.csproj` files in `src/McpWorkshop.Shared/` and servers — copy files for rollback
- [ ] T004 [P] Backup Exercise 5 documentation files (`09b` and `09a`) — preserve originals before edits

## Phase 3: User Story 1 - Package Version Update Across All Projects (P1)

- [ ] T005 [P] [US1] Update `ModelContextProtocol` to v1.0.0 in `src/McpWorkshop.Shared/McpWorkshop.Shared.csproj` — edit package reference
- [ ] T006 [P] [US1] Update `ModelContextProtocol` to v1.0.0 in `src/McpWorkshop.Servers/SqlMcpServer/SqlMcpServer.csproj` — edit package reference
- [ ] T007 [P] [US1] Update `ModelContextProtocol` to v1.0.0 in `src/McpWorkshop.Servers/RestApiMcpServer/RestApiMcpServer.csproj` — edit package reference
- [ ] T008 [P] [US1] Update `ModelContextProtocol` to v1.0.0 in `src/McpWorkshop.Servers/CosmosMcpServer/CosmosMcpServer.csproj` — edit package reference
- [ ] T009 [P] [US1] Update `Microsoft.Extensions.*` references to latest stable — align extension packages
- [ ] T010 [US1] Run `dotnet restore` and verify packages resolve — validate package restore

## Phase 4: User Story 2 - Compilation Compatibility (P1)

- [ ] T011 [US2] Build the solution and capture baseline warning count — run `dotnet build`
- [ ] T012 [US2] Fix compilation errors introduced by SDK changes — update code to new APIs
- [ ] T013 [US2] Locate usages of `RunSessionHandler`; migrate to `ConfigureSessionOptions` or suppress `MCPEXP002` — update or document
- [ ] T014 [US2] Review build warnings and resolve or document any new warnings — ensure no new warnings remain

## Phase 5: User Story 3 - Test Suite Integrity (P2)

- [ ] T015 [US3] Run all unit tests in `tests/McpWorkshop.Tests/` — execute `dotnet test`
- [ ] T016 [US3] Update failing tests to match SDK changes and re-run tests — maintain behavioral intent

## Phase 6: User Story 4 - Workshop Documentation Accuracy (P2)

- [ ] T017 [P] [US4] Update code samples in `docs/modules/09b-ejercicio-5-agente-maf.md` to v1.0.0 APIs — edit examples
- [ ] T018 [P] [US4] Update `docs/modules/09a-ejercicio-5-instructor.md` for v1.0.0 APIs and troubleshooting — edit instructor notes
- [ ] T019 [US4] Remove `--prerelease` from `dotnet add package` instructions in docs — update commands

## Phase 7: User Story 5 - Workshop Exercise Continuity (P2)

- [ ] T020 [US5] Start each MCP server and verify initialization — run servers locally
- [ ] T021 [US5] Execute exercise HTTP requests and verify expected responses — validate behavior

## Final Phase: Polish & Cross-Cutting Concerns

- [ ] T022 [P] Review all changes for consistency, style, and completeness — final review
- [ ] T023 [P] Update `README.md` and `QUICKSTART.md` if setup changed — update docs
- [ ] T024 [P] Commit changes with conventional commit messages — commit and push branch

---

## Dependencies

- User Story 1 (package update) must be completed before compilation, testing, and documentation update tasks
- User Story 2 (compilation) must be completed before test suite and exercise continuity tasks
- User Story 3, 4, 5 can be executed in parallel after compilation

## Parallel Execution Examples

- T005–T009 (package updates) can be run in parallel
- T017–T019 (documentation updates) can be run in parallel
- T020–T021 (exercise verification) can be run in parallel

## Implementation Strategy

- MVP: Complete User Story 1 (package update) and User Story 2 (compilation compatibility)
- Incremental delivery: Complete test suite, documentation, and exercise continuity tasks in parallel

---

Checklist format validated. All tasks are specific, actionable, and independently testable.
