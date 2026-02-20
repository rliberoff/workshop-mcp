# Workshop Continuity Checklist: Update ModelContextProtocol SDK to 0.9.0-preview.1

**Purpose**: Validate that the specification adequately covers participant experience, exercise integrity, and instructor readiness throughout the SDK migration  
**Created**: 2026-02-18  
**Feature**: [spec.md](../spec.md)  
**Depth**: Thorough  
**Audience**: Reviewer (pre-implementation gate)

## Requirement Completeness — Exercise Coverage

- [ ] CHK001 - Are requirements defined for each of the five exercises individually, or is exercise continuity only specified as a blanket statement? [Completeness, Spec §FR-007]
- [ ] CHK002 - Are requirements specified for Exercise 5 (Agent MAF) separately from the three MCP server exercises, given it uses a different architecture (client-side SDK consumption vs. server-side manual MCP)? [Completeness, Gap]
- [ ] CHK003 - Is the Exercise5Agent project's empty state (no source code, only bin/obj) addressed in the requirements — should it be excluded from migration scope or flagged as a known gap? [Completeness, Gap]
- [ ] CHK004 - Are requirements defined for updating the exercise HTTP request files (`.http` files) if the migration introduces changes to server response formats? [Completeness, Gap]
- [ ] CHK005 - Are requirements specified for each of the verification scripts (`verify-exercise1.ps1` through `verify-exercise5.ps1`) to confirm they still pass post-migration? [Completeness, Gap]

## Requirement Completeness — Documentation

- [ ] CHK006 - Are documentation update requirements specified for all exercise module files, or only for `09b-ejercicio-5-agente-maf.md`? [Completeness, Spec §FR-005]
- [ ] CHK007 - Are requirements defined for updating the QUICKSTART.md file if the setup prerequisites change (e.g., .NET SDK version requirements for participants)? [Completeness, Gap]
- [ ] CHK008 - Are requirements defined for reviewing/updating the TROUBLESHOOTING.md guide if the SDK version change introduces new potential failure modes for participants? [Completeness, Gap]
- [ ] CHK009 - Are requirements specified for updating the instructor materials (`*-instructor.md` files) if the migration changes any instructor talking points or demonstrations? [Completeness, Gap]
- [ ] CHK010 - Are requirements defined for updating the CHECKLIST.md and QUICK_REFERENCE.md files in `docs/` if they reference SDK version numbers or setup steps? [Completeness, Gap]

## Requirement Clarity — Migration Scope

- [ ] CHK011 - Is "functionally equivalent" (FR-007) defined with measurable criteria for each MCP server — what specific behaviors/outputs constitute equivalence? [Clarity, Spec §FR-007]
- [ ] CHK012 - Is "zero new compiler warnings" (FR-003) scoped precisely — does it include pre-existing warnings that may surface from the TFM upgrade (`net8.0` → `net10.0`), or only SDK-induced warnings? [Clarity, Spec §FR-003]
- [ ] CHK013 - Is the scope of "all existing unit tests MUST pass" (FR-004) clear about whether tests that reference SDK types transitively (through custom wrappers) are included? [Clarity, Spec §FR-004]
- [ ] CHK014 - Is "deprecated APIs" (FR-008) defined clearly — does it cover only compile-time `[Obsolete]` warnings or also runtime deprecation notices and behavioral changes? [Clarity, Spec §FR-008]

## Requirement Clarity — Participant Experience

- [ ] CHK015 - Are requirements specified for what version of the .NET SDK participants need installed — does the TFM upgrade from `net8.0` to `net10.0` change the minimum required tooling? [Clarity, Gap]
- [ ] CHK016 - Is the impact on participant setup time documented — will participants need to install additional SDKs or tools after this migration? [Clarity, Gap]
- [ ] CHK017 - Are error messages that participants might encounter during exercises defined as part of the "functionally equivalent" requirement, or only happy-path outputs? [Clarity, Spec §FR-007]

## Requirement Consistency

- [ ] CHK018 - Does SC-004 ("All three MCP servers start successfully") align with the scope of FR-007 that mentions "all MCP tools, resources, and prompts" — are resources and prompts verified, not just server startup? [Consistency, Spec §SC-004 vs §FR-007]
- [ ] CHK019 - Is the scope of "four projects" in FR-001/SC-001 consistent with the "three servers" in SC-004 and the test project's transitive dependency — are all five projects accounted for? [Consistency, Spec §FR-001 vs §SC-004]
- [ ] CHK020 - Do the acceptance scenarios for User Story 4 ("exercise HTTP request files") align with the edge case about "tool or resource registration APIs have new required parameters" — if registration changes, do HTTP request formats change too? [Consistency, Spec §US4 vs Edge Cases]

## Scenario Coverage — Instructor Readiness

- [ ] CHK021 - Are requirements defined for instructor pre-workshop validation — a runbook or script that confirms all exercises work end-to-end after the migration? [Coverage, Gap]
- [ ] CHK022 - Are requirements defined for what the instructor should communicate to participants about the SDK version change (e.g., in opening module 01a/01b)? [Coverage, Gap]
- [ ] CHK023 - Are rollback requirements defined if the migration is applied but a critical exercise fails during the workshop — can the instructor revert quickly? [Coverage, Gap, Recovery Flow]

## Scenario Coverage — Participant Workflows

- [ ] CHK024 - Are requirements defined for the participant onboarding flow — does `verify-setup.ps1` need updating to check for the new SDK version? [Coverage, Gap]
- [ ] CHK025 - Are requirements defined for participants who may have cached NuGet packages from a previous version — will `dotnet restore` correctly resolve v0.9.0-preview.1 over cached v0.4.0-preview.3? [Coverage, Edge Case]
- [ ] CHK026 - Are requirements specified for the `create-sample-data.ps1` script — does it depend on any packages or TFMs being updated? [Coverage, Gap]
- [ ] CHK027 - Are requirements defined for partial completion scenarios — if a participant is mid-exercise when the SDK version matters, are there intermediate checkpoints? [Coverage, Alternate Flow]

## Edge Case Coverage — Runtime Behavior

- [ ] CHK028 - Are requirements defined for what happens if the new SDK's transitive dependency on `Microsoft.Extensions.Hosting.Abstractions` conflicts with the manually configured hosting in server `Program.cs` files? [Edge Case, Gap]
- [ ] CHK029 - Are requirements specified for assembly-level type collisions between `McpWorkshop.Shared.Mcp` custom types and new types introduced in `ModelContextProtocol` v0.9.0-preview.1? [Edge Case, Gap]
- [ ] CHK030 - Are requirements defined for the new `ModelContextProtocol.Core` transitive package — could it introduce new assembly bindings that affect the custom MCP implementation? [Edge Case, Gap]

## Acceptance Criteria Quality

- [ ] CHK031 - Can User Story 4 acceptance scenario 2 ("responses match the expected behavior") be objectively measured — are expected response payloads defined or referenced? [Measurability, Spec §US4]
- [ ] CHK032 - Can SC-005 ("No regression in workshop exercise functionality") be verified without running the entire workshop — is there an automated or scripted verification path? [Measurability, Spec §SC-005]
- [ ] CHK033 - Are acceptance criteria defined for the documentation review (Exercise 5 code samples) — what constitutes "accurate" for SDK v0.9.0-preview.1 API? [Measurability, Gap]

## Dependencies & Assumptions

- [ ] CHK034 - Is the assumption "SDK API not consumed in source code" validated with a requirement to re-verify at implementation time, or is it taken as final? [Assumption]
- [ ] CHK035 - Is the assumption that `Microsoft.AspNetCore.Http.Abstractions` stays at 2.3.0 validated against the `net10.0` TFM upgrade — does the upgrade surface new framework-included types that conflict? [Assumption]
- [ ] CHK036 - Is the dependency on the `run-all-tests.ps1` script documented as a verification requirement, and are its success criteria defined? [Dependency, Gap]

## Notes

- This checklist validates the **requirements quality** for workshop continuity — it does NOT test the implementation.
- Focus areas: participant experience (5 exercises), instructor readiness, documentation accuracy, and edge cases around the TFM upgrade.
- Depth: Thorough (~36 items across 9 categories).
- Each item references the spec section it validates or marks `[Gap]` for missing requirements.
