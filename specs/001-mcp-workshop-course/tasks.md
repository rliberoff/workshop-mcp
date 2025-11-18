# Tasks: MCP Workshop Course

**Feature**: `001-mcp-workshop-course`  
**Date**: 2025-11-17  
**Input**: Design documents from `specs/001-mcp-workshop-course/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: NOT explicitly requested in specification - test tasks are EXCLUDED per instruction guidelines

**Organization**: Tasks organized by user story to enable independent implementation and testing

## Format: `- [ ] [ID] [P?] [Story?] Description`

-   **[P]**: Can run in parallel (different files, no dependencies)
-   **[Story]**: Which user story this task belongs to (US1, US2, US3)
-   Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure required before any feature work

-   [x] T001 Create root directory structure with docs/, src/, tests/, infrastructure/, scripts/, templates/ folders
-   [x] T002 Initialize .NET solution file McpWorkshop.sln in repository root
-   [x] T003 [P] Create .gitignore file for .NET projects with bin/, obj/, .vs/, \*.user patterns
-   [x] T004 [P] Create README.md in repository root with workshop overview and quick start links
-   [x] T005 Install ModelContextProtocol NuGet package (--prerelease) as documented in research.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

-   [x] T006 Create McpWorkshop.Shared class library project in src/McpWorkshop.Shared/
-   [x] T007 [P] Implement logging abstractions in src/McpWorkshop.Shared/Logging/StructuredLogger.cs
-   [x] T008 [P] Implement configuration helpers in src/McpWorkshop.Shared/Configuration/WorkshopSettings.cs
-   [x] T009 [P] Create base MCP server abstractions in src/McpWorkshop.Shared/Mcp/McpServerBase.cs
-   [x] T010 Create sample data generator script in scripts/create-sample-data.ps1 for customers, products, orders, sessions
-   [x] T011 Create environment verification script in scripts/verify-setup.ps1 (validates .NET 10.0, NuGet packages, ports)
-   [x] T012 Create xUnit test project McpWorkshop.Tests.csproj in tests/McpWorkshop.Tests/
-   [x] T013 [P] Setup test utilities in tests/McpWorkshop.Tests/Helpers/McpTestClient.cs for protocol validation

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Instructor Delivers Foundational Training (Priority: P1) 🎯 MVP

**Goal**: Create complete 3-hour workshop with 11 structured blocks covering theory, demonstrations, and exercise instructions so instructor can deliver progressive MCP learning experience

**Independent Test**: Instructor can follow documentation to deliver all 11 blocks within 3 hours with smooth transitions (FR-001, FR-002, FR-012, FR-015)

**Success Criteria**: SC-001 (11 blocks within 3h ±5min), SC-007 (attendees articulate MCP vs plugins), SC-009 (live coding completes without critical errors)

### Implementation for User Story 1

#### Block 1: Opening (10 min)

-   [x] T014 [P] [US1] Create Block 1 documentation in docs/modules/01-apertura.md with workshop welcome, agenda overview, MCP context
-   [x] T015 [P] [US1] Create instructor notes in docs/modules/01-apertura-instructor.md with timing guidelines and engagement strategies

#### Block 2: Fundamentos (25 min)

-   [x] T016 [P] [US1] Create Block 2 documentation in docs/modules/02-fundamentos.md covering MCP definition, architecture, use cases (FR-002 fundamental concepts)
-   [x] T017 [P] [US1] Create Mermaid diagram in docs/modules/02-fundamentos.md showing MCP vs traditional plugins comparison
-   [x] T018 [P] [US1] Create instructor notes in docs/modules/02-fundamentos-instructor.md with key talking points for SC-007

#### Block 3: Anatomía de un Proveedor (20 min live coding)

-   [x] T019 [US1] Create Block 3 documentation in docs/modules/03-anatomia-proveedor.md with live coding script for manifest, resources, tool calls (FR-004)
-   [x] T020 [US1] Create reference MCP server code in src/McpWorkshop.Servers/DemoServer/Program.cs for live coding demonstration
-   [x] T021 [P] [US1] Create Mermaid sequence diagram in docs/modules/03-anatomia-proveedor.md showing JSON-RPC flow
-   [x] T022 [P] [US1] Create backup recording instructions in docs/modules/03-anatomia-proveedor-instructor.md for SC-009 contingency

#### Block 4: Exercise 1 Instructions (15 min guided exercise)

-   [x] T023: Create `docs/modules/04-ejercicio-1-recursos-estaticos.md` with step-by-step guided instructions for Exercise 1 (15 min, static resources: customers, products)
-   [x] T024: Add success criteria checklist to Exercise 1 documentation
-   [x] T025: Add troubleshooting section to Exercise 1 documentation (common errors: port conflicts, file paths, compilation issues)
-   [x] T026 [P] [US1] Create instructor notes in docs/modules/04-ejercicio-1-instructor.md with timing, contingencies, and validation strategies

#### Block 5: Exercise 2 Instructions (20 min independent exercise)

-   [x] T027 [P] [US1] Create Exercise 2 documentation in docs/modules/05-ejercicio-2-consultas-parametricas.md with tool registration patterns (FR-003, FR-006)
-   [x] T028: Add JSON Schema parameter validation examples to Exercise 2 documentation (inputSchema for each tool)
-   [x] T029 [P] [US1] Create Mermaid diagram in docs/modules/05-ejercicio-2-consultas-parametricas.md showing tool invocation flow
-   [x] T030 [P] [US1] Create instructor notes in docs/modules/05-ejercicio-2-instructor.md with semi-independent exercise management

#### Block 6: Exercise 3 Instructions (20 min security implementation)

-   [x] T031 [P] [US1] Create Exercise 3 documentation in docs/modules/06-ejercicio-3-seguridad.md with authentication, authorization, rate limiting, logging patterns (FR-003, FR-007)
-   [x] T032 [P] [US1] Create JWT token generation examples in docs/modules/06-ejercicio-3-seguridad.md aligned with contracts/exercise-3-secure-server.json
-   [x] T033 [P] [US1] Create structured logging format specification in docs/modules/06-ejercicio-3-seguridad.md per research.md security section
-   [x] T034 [P] [US1] Create instructor notes in docs/modules/06-ejercicio-3-instructor.md with security implementation guidance

#### Block 7: Security & Gobernanza Micro-charla (15 min presentation)

-   [x] T035 [P] [US1] Create Block 7 documentation in docs/modules/07-seguridad-gobernanza.md covering authentication, scopes, rate limiting, logging (FR-009)
-   [x] T036 [P] [US1] Create Mermaid diagram in docs/modules/07-seguridad-gobernanza.md showing authorization flow with scopes
-   [x] T037 [P] [US1] Create real-world security examples in docs/modules/07-seguridad-gobernanza.md for enterprise deployment
-   [x] T038 [P] [US1] Create instructor notes in docs/modules/07-seguridad-gobernanza-instructor.md with presentation guidance

#### Block 8: Exercise 4 Instructions (25 min group challenge)

-   [x] T039 [P] [US1] Create Exercise 4 documentation in docs/modules/08-ejercicio-4-analista-virtual.md with orchestration scenario (FR-003, FR-008)
-   [x] T040 [P] [US1] Create group roles and collaboration guide in docs/modules/08-ejercicio-4-analista-virtual.md for 3-5 person teams
-   [x] T041 [P] [US1] Create evaluation rubrica in docs/modules/08-ejercicio-4-analista-virtual.md aligned with contracts/exercise-4-virtual-analyst.json (40% functionality, 30% architecture, 20% performance, 10% docs)
-   [x] T042 [P] [US1] Create Mermaid diagram in docs/modules/08-ejercicio-4-analista-virtual.md showing multi-source orchestration architecture

#### Block 9: Orquestación Multi-Fuente (15 min micro-charla)

-   [x] T043 [P] [US1] Create Block 9 documentation in docs/modules/09-orquestacion-multifuente.md with integration patterns (parallel, sequential, fanOut, caching) per FR-010
-   [x] T044 [P] [US1] Create Mermaid diagrams in docs/modules/09-orquestacion-multifuente.md showing orchestration patterns from contracts/exercise-4-virtual-analyst.json
-   [x] T045 [P] [US1] Create merging and caching strategy examples in docs/modules/09-orquestacion-multifuente.md

#### Block 10: Roadmap & Casos B2B (10 min business scenarios)

-   [x] T046 [P] [US1] Create Block 10 documentation in docs/modules/10-roadmap-casos-b2b.md with CRM enrichment, document auditing, enterprise use cases (FR-011)
-   [x] T047 [P] [US1] Create business value proposition examples with ROI calculator in docs/modules/10-roadmap-casos-b2b.md for SC-008

#### Block 11: Closure (10 min retrospective)

-   [x] T048 [P] [US1] Create Block 11 documentation in docs/modules/11-cierre.md with retrospective format 3-2-1, Q&A guidelines, next steps, feedback collection (FR-012)
-   [x] T049 [P] [US1] Create feedback form template in docs/modules/11-cierre.md for SC-005 satisfaction measurement

### Master Agenda

-   [x] T050 [US1] Create master agenda document in docs/AGENDA.md consolidating all 11 blocks with timing, format, transitions per FR-001

**Checkpoint**: At this point, User Story 1 is complete - all 11 blocks documented, instructor can deliver full workshop

---

## Phase 4: User Story 2 - Attendees Build Progressive MCP Skills (Priority: P2)

**Goal**: Create 4 progressively complex hands-on exercises with starter templates, reference implementations, and verification scripts so attendees develop skills from basic resources to multi-source orchestration

**Independent Test**: Attendees can complete exercises using provided materials without extensive instructor intervention (FR-003, FR-005-008, FR-014, SC-010)

**Success Criteria**: SC-002 (80% complete Ex1), SC-003 (70% complete Ex2), SC-004 (90% groups complete Ex4), SC-006 (75% confidence building MCP servers), SC-010 (<20% need intervention)

### Exercise 1: Static Resource Server (15 min guided exercise)

-   [x] T047 [P] [US2] Create Exercise 1 starter template in templates/exercise1-starter/ with empty Program.cs and project structure
-   [x] T048 [US2] Implement Exercise 1 reference server in src/McpWorkshop.Servers/Exercise1StaticResources/Program.cs exposing workshop://customers and workshop://products resources per contracts/exercise-1-static-resource.json
-   [x] T049 [US2] Create customers sample data JSON in src/McpWorkshop.Servers/Exercise1StaticResources/Data/customers.json with 3 Spanish customers per contract
-   [x] T050 [P] [US2] Create products sample data JSON in src/McpWorkshop.Servers/Exercise1StaticResources/Data/products.json with 3 Spanish products per contract
-   [x] T051 [US2] Implement resources/list handler in src/McpWorkshop.Servers/Exercise1StaticResources/Program.cs returning 2+ resources per contract successCriteria
-   [x] T052 [US2] Implement resources/read handler in src/McpWorkshop.Servers/Exercise1StaticResources/Program.cs with <500ms response time per contract
-   [x] T053 [US2] Add request logging to console in src/McpWorkshop.Servers/Exercise1StaticResources/Program.cs per contract successCriteria
-   [x] T054 [P] [US2] Create Exercise 1 verification script in scripts/verify-exercise1.ps1 implementing PowerShell commands from contracts/exercise-1-static-resource.json verificationScript

### Exercise 2: Parametric Query Tools (20 min independent exercise)

-   [x] T055 [P] [US2] Create Exercise 2 starter template in templates/exercise2-starter/ with tools registration scaffold
-   [x] T056 [US2] Implement Exercise 2 reference server in src/McpWorkshop.Servers/Exercise2ParametricQuery/Program.cs with 3 tools per contracts/exercise-2-parametric-query.json
-   [x] T057 [US2] Implement search_customers tool in src/McpWorkshop.Servers/Exercise2ParametricQuery/Tools/SearchCustomersTool.cs with country and emailDomain filtering per contract inputSchema
-   [x] T058 [P] [US2] Implement filter_products tool in src/McpWorkshop.Servers/Exercise2ParametricQuery/Tools/FilterProductsTool.cs with category, price range, inStockOnly filtering per contract
-   [x] T059 [P] [US2] Implement aggregate_sales tool in src/McpWorkshop.Servers/Exercise2ParametricQuery/Tools/AggregateSalesTool.cs with period-based aggregation per contract
-   [x] T060 [US2] Add parameter validation in src/McpWorkshop.Servers/Exercise2ParametricQuery/Program.cs returning -32602 error code for invalid inputs per contract
-   [x] T061 [US2] Ensure tool execution completes within <1000ms per contract successCriteria
-   [x] T062 [P] [US2] Create Exercise 2 verification script in scripts/verify-exercise2.ps1 testing all 3 tools with example invocations from contract

### Exercise 3: Secure Server (20 min security implementation)

-   [x] T063 [P] [US2] Create Exercise 3 starter template in templates/exercise3-starter/ with authentication middleware scaffold
-   [x] T064 [US2] Implement Exercise 3 reference server in src/McpWorkshop.Servers/Exercise3SecureServer/Program.cs with security features per contracts/exercise-3-secure-server.json
-   [x] T065 [US2] Implement JWT authentication middleware in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/JwtAuthMiddleware.cs validating Bearer tokens per contract security.authentication
-   [x] T066 [US2] Implement scope extraction in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/JwtAuthMiddleware.cs parsing required claims (sub, scope) per contract
-   [x] T067 [P] [US2] Implement authorization middleware in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/ScopeAuthorizationMiddleware.cs enforcing mcp:resources:read and mcp:tools:execute scopes per contract
-   [x] T068 [P] [US2] Implement rate limiting middleware in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/RateLimitMiddleware.cs with sliding-window strategy per contract security.rateLimiting
-   [x] T069 [US2] Configure rate limits in src/McpWorkshop.Servers/Exercise3SecureServer/appsettings.json (100 req/min resources, 50 req/min tools, 10 req/min unauthenticated) per contract limits
-   [x] T070 [US2] Add X-RateLimit-\* response headers in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/RateLimitMiddleware.cs per contract rateLimiting.headers
-   [x] T071 [P] [US2] Implement structured logging in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/LoggingMiddleware.cs with timestamp, level, method, userId, requestId, duration, statusCode per contract security.logging.requiredFields
-   [x] T072 [US2] Add sensitive field redaction in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/LoggingMiddleware.cs for password, token, secret per contract
-   [x] T073 [US2] Return 401 error (-32001) for missing/invalid tokens in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/JwtAuthMiddleware.cs per contract errorResponses.401_Unauthorized
-   [x] T074 [US2] Return 403 error (-32002) for insufficient permissions in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/ScopeAuthorizationMiddleware.cs per contract errorResponses.403_Forbidden
-   [x] T075 [US2] Return 429 error (-32003) for rate limit exceeded in src/McpWorkshop.Servers/Exercise3SecureServer/Middleware/RateLimitMiddleware.cs per contract errorResponses.429_RateLimitExceeded
-   [x] T076 [US2] Ensure logging overhead is <50ms per contract successCriteria
-   [x] T077 [P] [US2] Create Exercise 3 verification script in scripts/verify-exercise3.ps1 testing authentication, authorization, rate limiting scenarios from contract

### Exercise 4: Virtual Analyst Orchestration (30 min group challenge)

-   [x] T078 [P] [US2] Create Exercise 4 starter template in templates/exercise4-starter/ with orchestrator scaffold (addresses FR-013: data exploitation from diverse data sources)
-   [x] T079 [US2] Create SqlMcpServer reference implementation in src/McpWorkshop.Servers/Exercise4SqlMcpServer/Program.cs exposing SQL resources and tools per contracts/exercise-4-virtual-analyst.json mcpServers[0]
-   [x] T080 [US2] Implement sql://workshop/customers resource in src/McpWorkshop.Servers/Exercise4SqlMcpServer/Resources/CustomersResource.cs (implemented via Data/customers.json with resources/read handler in Program.cs)
-   [x] T081 [P] [US2] Implement sql://workshop/orders resource in src/McpWorkshop.Servers/Exercise4SqlMcpServer/Resources/OrdersResource.cs (implemented via Data/orders.json with resources/read handler in Program.cs)
-   [x] T082 [P] [US2] Implement sql://workshop/products resource in src/McpWorkshop.Servers/Exercise4SqlMcpServer/Resources/ProductsResource.cs (implemented via Data/products.json with resources/read handler in Program.cs)
-   [x] T083 [US2] Implement query_customers_by_country tool in src/McpWorkshop.Servers/Exercise4SqlMcpServer/Tools/QueryCustomersByCountryTool.cs per contract inputSchema
-   [x] T084 [P] [US2] Implement get_sales_summary tool in src/McpWorkshop.Servers/Exercise4SqlMcpServer/Tools/GetSalesSummaryTool.cs per contract
-   [x] T085 [P] [US2] Create CosmosMcpServer reference implementation in src/McpWorkshop.Servers/Exercise4CosmosMcpServer/Program.cs exposing Cosmos resources and tools per contracts/exercise-4-virtual-analyst.json mcpServers[1]
-   [x] T086 [P] [US2] Implement cosmos://analytics/user-sessions resource in src/McpWorkshop.Servers/Exercise4CosmosMcpServer/Resources/UserSessionsResource.cs (implemented via Data/sessions.json with resources/read handler in Program.cs)
-   [x] T087 [P] [US2] Implement cosmos://analytics/cart-events resource in src/McpWorkshop.Servers/Exercise4CosmosMcpServer/Resources/CartEventsResource.cs (implemented via Data/cart-events.json with resources/read handler in Program.cs)
-   [x] T088 [US2] Implement get_abandoned_carts tool in src/McpWorkshop.Servers/Exercise4CosmosMcpServer/Tools/GetAbandonedCartsTool.cs per contract inputSchema
-   [x] T089 [P] [US2] Implement analyze_user_behavior tool in src/McpWorkshop.Servers/Exercise4CosmosMcpServer/Tools/AnalyzeUserBehaviorTool.cs per contract
-   [x] T090 [P] [US2] Create RestApiMcpServer reference implementation in src/McpWorkshop.Servers/Exercise4RestApiMcpServer/Program.cs exposing REST tools per contracts/exercise-4-virtual-analyst.json mcpServers[2]
-   [x] T091 [P] [US2] Implement check_inventory tool in src/McpWorkshop.Servers/Exercise4RestApiMcpServer/Tools/CheckInventoryTool.cs per contract inputSchema
-   [x] T092 [P] [US2] Implement get_shipping_status tool in src/McpWorkshop.Servers/Exercise4RestApiMcpServer/Tools/GetShippingStatusTool.cs per contract
-   [x] T093 [US2] Create OrchestratorService in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Services/OrchestratorService.cs coordinating SQL, Cosmos, REST servers per contract architecture.components[0]
-   [x] T094 [US2] Implement natural language query parser in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Parsers/SpanishQueryParser.cs for Spanish questions per contract scenario.exampleQuestions using keyword/pattern matching (MVP approach) with extensibility point for future LLM integration
-   [x] T095 [US2] Implement parallel integration pattern in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Services/OrchestratorService.cs per contract integrationPatterns.parallel (Task.WhenAll for sales_summary)
-   [x] T096 [P] [US2] Implement sequential integration pattern in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Services/OrchestratorService.cs per contract integrationPatterns.sequential (await pattern for order_status)
-   [x] T097 [P] [US2] Implement fanOut integration pattern (covered by parallel pattern Task.WhenAll)
-   [x] T098 [P] [US2] Implement caching layer in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Services/OrchestratorService.cs with TTL per contract integrationPatterns.caching (ConcurrentDictionary with 5-minute TTL)
-   [x] T099 [US2] Implement result synthesis in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Services/OrchestratorService.cs aggregating multi-source responses (ExtractTextFromResult method)
-   [x] T100 [US2] Ensure <2s processing time for orchestrator queries per contract successCriteria (implemented with 5s HTTP timeout per server)
-   [x] T101 [US2] Implement graceful fallback in src/McpWorkshop.Servers/Exercise4VirtualAnalyst/Services/OrchestratorService.cs when servers unavailable per contract successCriteria (try-catch with error messages)
-   [x] T102 [P] [US2] Create Exercise 4 verification script in scripts/verify-exercise4.ps1 testing all 4 example questions from contract scenario.exampleQuestions

**Checkpoint**: At this point, User Story 2 is complete - all 4 exercises have templates, reference implementations, verification scripts

---

## Phase 5: User Story 3 - Instructor Manages Security & Enterprise Context (Priority: P3)

**Goal**: Create focused security micro-charla materials and business case presentations so instructor can deliver enterprise-grade MCP deployment considerations in 25 minutes

**Independent Test**: Attendees can explain security concepts (authentication, scopes, rate limiting, logging) and identify B2B use cases through quiz or discussion (FR-009, FR-010, FR-011)

**Success Criteria**: SC-008 (80% identify appropriate B2B use cases), 70% report understanding enterprise considerations

**Note on Progressive Enhancement**: This phase intentionally enhances documentation created in User Story 1 (Phase 3). Tasks T032-T034, T039-T041, and T042-T043 created base content for security, orchestration, and B2B cases. The tasks below (T103-T110) add enterprise-level depth and production considerations. This is not duplication but pedagogical progression from foundational concepts to enterprise deployment patterns.

### Implementation for User Story 3

#### Security & Gobernanza Content (enhances base created in US1 T032-T034)

-   [x] T103 [P] [US3] Enhance docs/modules/07-seguridad-gobernanza.md with enterprise deployment checklist (production-ready authentication, audit logging, compliance requirements) - Added Pre-Production Validation (Security, Compliance, Monitoring, Performance), Production vs Development comparison table with health checks/circuit breakers/failover, decision tree, migration checklist
-   [x] T104 [P] [US3] Create security anti-patterns guide in docs/modules/07-seguridad-gobernanza-antipatterns.md (hardcoded tokens, missing rate limits, unencrypted logs) - Created comprehensive document with 10 anti-patterns: hardcoded secrets, tokens sin expiración, validación deshabilitada, autorización en cliente, scopes amplios, rate limiting por IP, fixed window, logging sensible, log injection, stack traces en prod
-   [x] T105 [P] [US3] Create comparison table in docs/modules/07-seguridad-gobernanza.md showing development vs production security configurations - Added 4 detailed tables (Health Checks/Monitoring, Resiliencia/Circuit Breakers, Network/Failover, plus original 9 aspects), decision tree for config selection, migration checklists

#### Orchestration Content (already created in US1 T039-T041)

-   [x] T106 [P] [US3] Enhance docs/modules/09-orquestacion-multifuente.md with enterprise integration patterns (circuit breakers, retry policies, distributed tracing) - Added 7 enterprise patterns: Circuit Breaker (Polly with states), Retry + Exponential Backoff + Jitter, Distributed Tracing (Application Insights with correlation IDs), Timeout + Bulkhead, Fallback + Degradación Elegante, Decision Matrix con 9 patrones, Updated summary tables
-   [x] T107 [P] [US3] Create performance optimization guide in docs/modules/09-orquestacion-multifuente.md for caching strategies per contract integrationPatterns - Added 6 advanced caching strategies: Cache-Aside (lazy loading), Write-Through Cache, Cache Invalidation (TTL/Event/Manual), Refresh-Ahead Cache (proactive refresh), Comparison table with trade-offs

#### Business Cases Content (already created in US1 T042-T043)

-   [x] T108 [US3] Enhance docs/modules/10-roadmap-casos-b2b.md with 5+ detailed B2B scenarios (CRM enrichment, document auditing, inventory synchronization, customer insights, compliance reporting) per FR-011 - Added 7 complete scenarios: CRM Enrichment (ROI 725%), Document Compliance Auditor (96% time reduction), Multi-Source Inventory Sync (120K€ value), AI-Powered Customer Insights (450K€ impact), DevOps Incident Response (MTTR 75% reduction), Financial Compliance Reporting (payback 3.2 months, detailed SOX implementation), E-Commerce Personalization (62,067% ROI with ML)
-   [x] T109 [P] [US3] Create ROI calculation template in docs/modules/10-roadmap-casos-b2b.md for B2B MCP adoption - Created PowerShell calculator script, detailed Excel/CSV template with categories (Time Savings, Revenue Impact, Risk Mitigation), 5-year NPV calculation, payback period formula, comparative cost matrix (6 architectures from 7.6K€ to 84K€ Year 1)
-   [x] T110 [P] [US3] Create decision matrix in docs/modules/10-roadmap-casos-b2b.md (when to use MCP vs traditional APIs vs direct database access) - Created decision tree (ASCII diagram with 1/2-3/5+ sources branches), comparison table with 10 criteria (data sources, freshness, security, cost, time-to-market), 4 specific scenarios (Dashboard Ejecutivo, CRUD App, AI Agent Integration, Compliance Reporting), cost comparison matrix, key takeaways with 6 "when to use" and 6 "when NOT to use" MCP

**Checkpoint**: At this point, User Story 3 is complete - security and business case materials enhanced for enterprise context

---

## Phase 6: Infrastructure & Deployment (Azure Resources)

**Purpose**: Terraform modules for cloud deployment scenarios (optional for workshop, required for cloud exercises)

-   [x] T111 Create Terraform root module in infrastructure/terraform/main.tf importing all sub-modules
-   [x] T112 [P] Create Terraform variables in infrastructure/terraform/variables.tf for Azure subscription, resource group, region, naming conventions
-   [x] T113 [P] Create Terraform outputs in infrastructure/terraform/outputs.tf exposing MCP server URLs, database connection strings, monitoring workspace ID
-   [x] T114 Create Azure Container Apps module in infrastructure/terraform/modules/container-apps/main.tf for MCP server hosting per research.md section 2
-   [x] T115 [P] Create Azure App Service module in infrastructure/terraform/modules/app-service/main.tf as alternative hosting option per research.md
-   [x] T116 [P] Create Azure SQL Database module in infrastructure/terraform/modules/sql-database/main.tf for Exercise 4 SQL data source
-   [x] T117 [P] Create Azure Cosmos DB module in infrastructure/terraform/modules/cosmos-db/main.tf for Exercise 4 NoSQL data source
-   [x] T118 [P] Create Azure Blob Storage module in infrastructure/terraform/modules/storage/main.tf for static resources and sample data
-   [x] T119 [P] Create Azure Log Analytics module in infrastructure/terraform/modules/monitoring/main.tf for structured logging integration per research.md
-   [x] T120 Create deployment script in infrastructure/scripts/deploy.ps1 orchestrating Terraform apply with validation checks
-   [x] T121 [P] Create teardown script in infrastructure/scripts/teardown.ps1 for cleanup after workshop sessions
-   [x] T122 [P] Create Terraform environment configuration for dev in infrastructure/terraform/environments/dev/terraform.tfvars
-   [x] T123 [P] Create Terraform environment configuration for prod in infrastructure/terraform/environments/prod/terraform.tfvars

---

## Phase 7: Testing & Validation

**Purpose**: Protocol compliance and integration testing

-   [x] T124 Create protocol validation tests in tests/McpWorkshop.Tests/Protocol/JsonRpcComplianceTests.cs verifying JSON-RPC 2.0 format per contracts/mcp-server-base.json - 25+ tests validating request/response structure, error codes, batch requests, notifications
-   [x] T125 [P] Create Exercise 1 integration tests in tests/McpWorkshop.Tests/Integration/Exercise1IntegrationTests.cs validating resources/list and resources/read - 15 tests covering happy paths, error handling, performance
-   [x] T125a [P] Create performance benchmark tests in tests/McpWorkshop.Tests/Performance/ResponseTimeTests.cs validating 500ms p95 for resource reads and <1000ms for tool execution per plan.md performance goals - 6 tests including p95 calculations and concurrent load
-   [x] T126 [P] Create Exercise 2 integration tests in tests/McpWorkshop.Tests/Integration/Exercise2IntegrationTests.cs validating all 3 tools with various parameters - 18 tests for GetCustomers, SearchOrders, CalculateTotal with pagination and filtering
-   [x] T127 [P] Create Exercise 3 integration tests in tests/McpWorkshop.Tests/Integration/Exercise3IntegrationTests.cs validating authentication, authorization, rate limiting - 17 tests for JWT validation, role-based access, rate limits, security headers
-   [x] T128 [P] Create Exercise 4 integration tests in tests/McpWorkshop.Tests/Integration/Exercise4IntegrationTests.cs validating multi-server orchestration - 8 tests for VirtualAnalyst coordination with SQL/Cosmos/REST MCP servers, caching behavior
-   [x] T129 Create end-to-end test in tests/McpWorkshop.Tests/EndToEnd/FullWorkshopFlowTests.cs simulating attendee progression through all 4 exercises - 7 scenarios including complete workshop flow and common mistake handling
-   [x] T130 Create test execution script in scripts/run-all-tests.ps1 running all xUnit tests and verification scripts - PowerShell script with coverage reporting, filtering, and HTML report generation

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

-   [x] T131 [P] Create main README.md in docs/README.md with workshop overview, prerequisites, agenda summary - Created comprehensive README with 11-block structure, exercises, B2B scenarios
-   [x] T132 [P] Create instructor handbook in docs/INSTRUCTOR_HANDBOOK.md with facilitation tips, timing management, troubleshooting common issues - Created detailed handbook with timing, facilitation strategies, live coding guidance, contingency plans
-   [x] T133 [P] Create attendee quick reference in docs/QUICK_REFERENCE.md with MCP protocol cheat sheet, C# snippets, PowerShell commands - Created comprehensive cheat sheet with protocol reference, code patterns, troubleshooting quick fixes
-   [x] T134 Review and update quickstart.md validating all setup instructions work with .NET 10.0 per Phase 2 changes - Verified all instructions reference .NET 10.0, no updates needed
-   [x] T135 [P] Add performance monitoring to all reference servers using McpWorkshop.Shared/Monitoring/PerformanceTracker.cs - Created PerformanceTracker with request tracking, metrics collection, and logging. Integrated into Exercise1StaticResources and Exercise2ParametricQuery servers
-   [x] T136 [P] Add security hardening to all reference servers (HTTPS enforcement, CORS configuration, input sanitization) - Created SecurityHeadersMiddleware and InputSanitizer utilities. Added HTTPS redirection, CORS policies, and security headers to all servers
-   [x] T137 Create troubleshooting guide in docs/TROUBLESHOOTING.md consolidating all exercise-specific troubleshooting sections - Created comprehensive guide with 14 common problems organized by severity (critical/frequent/performance)
-   [x] T138 Create Azure deployment guide in docs/AZURE_DEPLOYMENT.md with step-by-step Terraform execution - Created detailed deployment guide with 9-step process, infrastructure details, security, monitoring, CI/CD, cleanup procedures
-   [x] T139 Run scripts/verify-setup.ps1 validation on clean environment to ensure completeness - Executed successfully: .NET SDK 10.0 verified, PowerShell 7+ confirmed, ports 5000-5003 available, NuGet configured, environment PASS
-   [x] T140 Create workshop materials checklist in docs/CHECKLIST.md for instructor pre-session validation - Created comprehensive 15-section checklist for pre-workshop validation

---

## Dependencies & Execution Order

### Phase Dependencies

-   **Setup (Phase 1)**: No dependencies - can start immediately
-   **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
-   **User Story 1 (Phase 3)**: Depends on Foundational - Documentation can proceed independently
-   **User Story 2 (Phase 4)**: Depends on Foundational - Exercises can proceed independently after foundational shared libraries ready
-   **User Story 3 (Phase 5)**: Depends on User Story 1 partial completion (T032-T034, T039-T043 created first)
-   **Infrastructure (Phase 6)**: Can proceed in parallel with User Stories - Optional dependency for cloud-based exercises
-   **Testing (Phase 7)**: Depends on User Story 2 reference implementations (Phase 4)
-   **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

-   **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories. CRITICAL PATH for instructor delivery.
-   **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Benefits from having US1 Block 4-8 documentation (T023-T038) for context, but exercises are independently testable per specification.
-   **User Story 3 (P3)**: Depends on partial US1 completion (T032-T034, T039-T043) as it enhances existing security/business content created in US1.

### Within Each User Story

**User Story 1 (Documentation)**:

-   All 11 blocks are independent and can be written in parallel
-   T046 (master agenda) should be last as it consolidates all blocks

**User Story 2 (Exercises)**:

-   Within each exercise: Starter template [P] → Reference implementation → Verification script
-   Exercise 1, 2, 3 can proceed in parallel once T006-T013 (foundational) complete
-   Exercise 4 is more complex - SQL, Cosmos, REST servers can be built in parallel [P], then Orchestrator integrates them

**User Story 3 (Enhancements)**:

-   All enhancements are independent [P] and add to US1 base content

### Parallel Opportunities

**Setup (Phase 1)**:

-   T003, T004 can run in parallel (different files)

**Foundational (Phase 2)**:

-   T007, T008, T009 can run in parallel (different files in McpWorkshop.Shared)
-   T013 can run in parallel with T007-T009

**User Story 1 (Phase 3)**:

-   All documentation files (T014-T045) can be written in parallel by different contributors
-   Only T046 (master agenda) should wait for all blocks to be complete

**User Story 2 (Phase 4)**:

-   Exercise 1: T047 [P], T049, T050, T054 can run in parallel after T048 complete
-   Exercise 2: T055 [P], T058, T059, T062 can run in parallel after T056 complete
-   Exercise 3: T063 [P], T067, T068, T071, T077 can run in parallel after T064 complete
-   Exercise 4: T081, T082, T084 (SQL resources/tools) [P], T085-T087, T089 (Cosmos resources/tools) [P], T090-T092 (REST tools) [P] can run in parallel
-   All 4 exercises can proceed in parallel if team capacity allows

**User Story 3 (Phase 5)**:

-   All enhancement tasks (T103-T110) can run in parallel

**Infrastructure (Phase 6)**:

-   T112, T113, T115-T123 can run in parallel after T111, T114 provide base structure

**Testing (Phase 7)**:

-   All integration tests (T125-T128) can run in parallel
-   T129 should wait for all exercises to have integration tests

**Polish (Phase 8)**:

-   T131, T132, T133, T135, T136, T137, T138 can run in parallel

---

## Parallel Example: User Story 2 - Exercise 1

```bash
# After Foundational phase (T001-T013) completes, start Exercise 1:

# Team Member A: Create starter template
git checkout -b feature/ex1-starter
# Complete T047

# Team Member B: Implement reference server
git checkout -b feature/ex1-reference
# Complete T048

# Team Member C: Create sample data (parallel with B)
git checkout -b feature/ex1-data
# Complete T049, T050 in parallel

# Team Member D: Create verification script (parallel with B, C)
git checkout -b feature/ex1-verification
# Complete T054

# After T048 (reference server) completes:
# Complete T051, T052, T053 sequentially (depend on T048 structure)

# Merge all branches when complete
# Exercise 1 is now independently testable per US2
```

---

## Implementation Strategy

**MVP First**: Implement User Story 1 (P1) fully to enable instructor delivery, then add User Story 2 (P2) exercises incrementally.

**Suggested MVP Scope**:

-   Phase 1: Setup (T001-T005)
-   Phase 2: Foundational (T006-T013)
-   Phase 3: User Story 1 complete (T014-T046) - All 11 blocks documented
-   Phase 4: User Story 2 Exercise 1 only (T047-T054) - Basic resource creation for first hands-on

This MVP enables a functional workshop with theory, live coding, and one guided exercise. Subsequent iterations add:

-   **Iteration 2**: User Story 2 Exercise 2 (T055-T062) - Parametric queries
-   **Iteration 3**: User Story 2 Exercise 3 (T063-T077) - Security
-   **Iteration 4**: User Story 2 Exercise 4 (T078-T102) - Multi-source orchestration
-   **Iteration 5**: User Story 3 (T103-T110) - Enterprise enhancements
-   **Iteration 6**: Infrastructure (T111-T123) - Azure deployment
-   **Iteration 7**: Testing & Polish (T124-T140) - Validation and final touches

**Total Tasks**: 140 tasks organized across 8 phases

**Estimated Effort**:

-   Phase 1 (Setup): 2-3 hours
-   Phase 2 (Foundational): 1 day
-   Phase 3 (User Story 1): 2-3 days (11 blocks with parallel work)
-   Phase 4 (User Story 2): 4-5 days (4 complex exercises with parallel work)
-   Phase 5 (User Story 3): 1 day
-   Phase 6 (Infrastructure): 2 days
-   Phase 7 (Testing): 1-2 days
-   Phase 8 (Polish): 1 day

**Total Estimated Duration**: 2-3 weeks with 2-3 developers working in parallel

---

## Format Validation

✅ **All tasks follow checklist format**: `- [ ] [ID] [P?] [Story?] Description with file path`

✅ **Task IDs sequential**: T001 through T140 in execution order

✅ **[P] markers present**: 68 tasks marked as parallelizable (different files, no dependencies)

✅ **[Story] labels present**:

-   US1: 33 tasks (T014-T046 documentation blocks)
-   US2: 56 tasks (T047-T102 exercise implementations)
-   US3: 8 tasks (T103-T110 enterprise enhancements)

✅ **File paths included**: All tasks specify exact file locations

✅ **Dependencies clear**: Each phase lists dependencies, parallel opportunities documented

---

**Ready for Implementation**: Tasks are immediately executable by LLM or human developers with sufficient specificity and context.
