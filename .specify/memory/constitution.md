# MCP Workshop Constitution

## Core Principles

### I. MCP-First Design

Every capability is exposed as a Model Context Protocol (MCP) server or tool. Data and API access must be modeled as MCP resources, not ad‑hoc scripts. All workshop examples demonstrate how to wrap existing systems (databases, REST APIs, SaaS) behind MCP in a reusable, composable way.

### II. Text & JSON Interfaces

All practical exercises use text and/or JSON interfaces: requests in via stdin/HTTP, responses out via stdout/HTTP. Errors must be explicit and structured. MCP tools and servers should support human-readable logs and, where appropriate, JSON payloads that can be easily tested without an AI model in the loop.

### III. Test-First & Reproducibility

Each MCP server or tool must be testable in isolation. Wherever feasible, introduce tests or verification scripts before full implementation. Workshop participants should be able to run examples locally and see deterministic outputs given fixed inputs, ensuring exercises are reproducible across machines and environments.

Each exersice must be fully validated.

### IV. Integration Over Isolation

Examples should cover not only isolated MCP tools but also how they are composed in a real AI workflow (e.g., an AI assistant using multiple MCP servers over organizational data and APIs). Integration scenarios must include authentication, error handling, and basic resilience patterns, mirroring realistic enterprise usage.

### V. Simplicity & Observability

Implementations must favor clarity over cleverness. Code samples should be minimal, well-structured, and easy to extend in a workshop setting. Logging and diagnostics are required: each MCP server should surface meaningful logs for requests, responses, and failures to aid troubleshooting during exercises.

## Scope & Technology Constraints

This workshop focuses on building and using MCP servers and tools to access organizational data and APIs using Microsoft and Azure technologies.

- **Languages & Runtimes**: Prefer C# (.NET) and Python for MCP servers and utilities.
- **Azure Services**: Use Azure services such as Azure App Service, Azure Container Apps, Azure Functions (for hosting MCP servers if needed), Azure Storage, Azure SQL Database, Azure Cosmos DB, Azure API Management, and Azure Monitor/Log Analytics for observability.
- **Access Patterns**: Examples should include common enterprise patterns such as querying relational data, calling internal REST/GraphQL APIs, and integrating with Azure-native APIs (e.g., Microsoft Graph via Azure AD).
- **Exclusions**: Power Platform (including Power Apps, Power Automate, Power Pages) and Copilot Studio are explicitly out of scope. All automation and integration patterns must be demonstrated without those platforms.
- **Local & Cloud Environments**: Every exercise must be runnable locally (developer workstation) and have clear guidance on how it could be deployed or adapted to Azure environments.

## Development Workflow & Workshop Structure

The workshop content combines Markdown documentation with practical coding exercises. All materials must be structured to support self-paced learning and instructor-led delivery.

- **Documentation-First Flow**: Each module starts with Markdown documentation that explains goals, architecture, and prerequisites. Hands-on steps follow, referencing code in the repository and any scripts or templates provided.
- **Module Organization**: Workshop modules are organized by capability: e.g., "Building a basic MCP server", "Wrapping a REST API with MCP", "Querying Azure-hosted data via MCP", "Securing MCP access to organizational APIs".
- **Exercise Design**: Every practical exercise has:
  - A clear starting point (folder, branch, or initial code).
  - Step-by-step instructions in Markdown.
  - An expected outcome (behavior, command output, or test passing).
- **Verification & Testing**: Where applicable, each module includes:
  - A minimal test or verification script (e.g., calling the MCP server with sample requests).
  - Instructions for running tests using standard tools (`dotnet test`, `pytest`, or equivalent).
- **Contribution & Extensions**: Any new examples or modules must:
  - Follow the same documentation and exercise structure.
  - Use the same technology constraints and naming conventions.
  - Include guidance on how they integrate with existing MCP components in the workshop.

## Governance

This constitution defines the non-negotiable constraints and principles for the MCP workshop.

- **Authority**: The constitution supersedes any conflicting practices or sample code patterns within this repository. If a sample conflicts with these principles, the sample must be updated or removed.
- **Compliance in Reviews**: All pull requests and content changes must be reviewed for:
  - Adherence to MCP-first design.
  - Consistency with Microsoft/Azure-only technology choices.
  - Clarity and simplicity of code and documentation.
- **Amendments**: Changes to this constitution require:
  - Explicit documentation of the rationale.
  - Agreement from the workshop maintainers.
  - A migration or update plan for affected modules, exercises, and documentation.
- **Versioning**: The constitution is versioned independently of code. Any significant workshop structural or technology changes must be accompanied by a constitution version bump and a brief changelog entry.

**Version**: 1.0.0 | **Ratified**: 2025-11-17 | **Last Amended**: 2025-11-17
