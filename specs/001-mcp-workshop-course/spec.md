# Feature Specification: MCP Workshop Course

**Feature Branch**: `001-mcp-workshop-course`  
**Created**: November 17, 2025  
**Status**: Draft  
**Input**: User description: "Estoy creando un curso en forma de taller (workshop) para que los asistentes aprendan MCP dentro del contexto de una tecnología ideal para explotar datos desde diversas fuentes de datos."

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Instructor Delivers Foundational Training (Priority: P1)

As a workshop instructor, I need to deliver a structured 3-hour session that progressively teaches MCP fundamentals through theory, demonstrations, and hands-on exercises, so that attendees gain practical knowledge of building MCP servers for data exploitation.

**Why this priority**: This is the core deliverable - without the complete course structure and materials, the workshop cannot function. This represents the minimum viable product that delivers educational value.

**Independent Test**: Can be fully tested by conducting the workshop with a pilot group of attendees and measuring completion rates for each block, participant engagement metrics, and ability to complete practical exercises.

**Acceptance Scenarios**:

1. **Given** a group of 10-30 attendees with basic development knowledge, **When** the instructor follows the workshop agenda from opening to closure, **Then** all 11 blocks are delivered within 3 hours with smooth transitions between segments
2. **Given** an attendee with no prior MCP knowledge, **When** they complete the foundational block (25 minutes), **Then** they can articulate what MCP is, how it differs from plugins, and identify at least 2 use cases
3. **Given** an attendee watching the "Anatomía de un Proveedor" live coding session, **When** the instructor demonstrates server structure, **Then** they can identify the manifest, resources, and tool calls components in a sample MCP server
4. **Given** attendees starting Exercise 1, **When** they follow guided instructions for 15 minutes, **Then** at least 80% successfully create and expose a static resource
5. **Given** attendees completing Exercise 4 (Reto Integrador), **When** working in groups for 30 minutes on the virtual analyst case, **Then** each group produces a working agent that combines tools and resources from multiple providers

---

### User Story 2 - Attendees Build Progressive MCP Skills (Priority: P2)

As a workshop attendee, I need to progress through increasingly complex practical exercises that build on each other, so that I develop hands-on skills from basic resource creation to multi-source orchestration.

**Why this priority**: The practical exercises are what differentiate this from a standard lecture and ensure skill transfer. This is essential for workshop effectiveness but depends on the foundational training structure (P1).

**Independent Test**: Can be tested independently by providing attendees with exercise materials and success criteria, then measuring completion rates and quality of outputs for each exercise without requiring the full workshop context.

**Acceptance Scenarios**:

1. **Given** Exercise 1 instructions (static resource), **When** an attendee completes the exercise in 15 minutes, **Then** they have created a valid MCP resource that can be queried by a client
2. **Given** Exercise 2 materials (parametric queries), **When** an attendee completes it in 20 minutes, **Then** they have implemented a tool that accepts parameters and queries resources dynamically
3. **Given** Exercise 3 materials (security & governance), **When** an attendee completes it in 20 minutes, **Then** they have implemented basic permissions, logging, and rate limiting on their MCP server
4. **Given** Exercise 4 instructions (multi-source integration), **When** a group works together for 30 minutes, **Then** they deliver a virtual analyst agent that successfully combines data from at least 2 different MCP providers

---

### User Story 3 - Instructor Manages Security & Enterprise Context (Priority: P3)

As a workshop instructor, I need to deliver a focused 15-minute segment on authentication, scopes, rate limiting, and logging, followed by a 10-minute business cases session, so that attendees understand enterprise-grade MCP deployment considerations beyond basic functionality.

**Why this priority**: While important for complete understanding, this knowledge is supplementary to core MCP building skills. Attendees can build functional MCP servers without deep security knowledge initially, making this lower priority than hands-on exercises.

**Independent Test**: Can be tested by assessing attendee understanding through quiz questions or discussion participation specifically about security concepts and B2B use cases, independent of their ability to implement MCP servers.

**Acceptance Scenarios**:

1. **Given** the Security & Gobernanza micro-charla (15 minutes), **When** attendees complete this block, **Then** they can explain the purpose of authentication, scopes, rate limiting, and logging in MCP contexts
2. **Given** the Roadmap & Casos B2B session (10 minutes), **When** the instructor presents business scenarios, **Then** attendees can identify at least 3 B2B use cases (e.g., CRM enrichment, document auditing) where MCP provides value
3. **Given** the closing evaluation block, **When** attendees provide feedback, **Then** at least 70% report understanding enterprise considerations for MCP deployment

---

### Edge Cases

- What happens when an attendee falls behind during practical exercises? (Need support materials or pair programming approach)
- How does the instructor adapt if a live coding demonstration fails? (Need backup recordings or pre-built examples)
- What if attendees have varying technical skill levels? (May need tiered exercise difficulty or bonus challenges)
- How does Exercise 4 work if group sizes are uneven? (Need flexible grouping strategy and scalable case scenarios)
- What happens if the venue lacks adequate internet connectivity for accessing external data sources? (Need offline-capable exercises or local data setup)

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: Workshop MUST deliver 11 distinct content blocks covering opening, fundamentals, anatomy, 4 practical exercises, security, orchestration, business cases, and closing within 3 hours total duration
- **FR-002**: Course MUST provide progressive learning path from MCP concepts (25 min) to live coding anatomy (20 min) to guided exercises (15-30 min each) to integrative group challenge (30 min)
- **FR-003**: Each practical exercise MUST include clear instructions, success criteria, and time boundaries (Exercise 1: 15 min, Exercise 2: 20 min, Exercise 3: 20 min, Exercise 4: 30 min)
- **FR-004**: Workshop MUST include live coding demonstration of MCP server structure showing manifest, resources, and tool calls in "Anatomía de un Proveedor" block
- **FR-005**: Exercise 1 MUST guide attendees to create and expose a static resource through an MCP server
- **FR-006**: Exercise 2 MUST teach attendees to implement tools for parametric queries over resources
- **FR-007**: Exercise 3 MUST cover implementation of basic permissions, logging, and rate limiting
- **FR-008**: Exercise 4 MUST present an integrative group challenge where attendees build a "virtual analyst" agent combining tools and resources from multiple MCP providers
- **FR-009**: Security & Gobernanza block MUST cover authentication, scopes, rate limiting, and logging in 15-minute micro-charla format
- **FR-010**: Orquestación Multi-Fuente block MUST demonstrate consumption of multiple MCP providers and discuss merging/caching strategies through demo and discussion (15 min)
- **FR-011**: Roadmap & Casos B2B block MUST present business scenarios including CRM enrichment, document auditing, and other B2B applications (10 min)
- **FR-012**: Workshop MUST include opening (10 min) with welcome and MCP context, and closing (10 min) with retrospective, Q&A, next steps, and feedback collection
- **FR-013**: Course materials MUST support data exploitation scenarios from diverse data sources as the primary context for teaching MCP
- **FR-014**: All exercises MUST be designed for hands-on completion within specified time constraints while maintaining educational value
- **FR-015**: Workshop MUST alternate between presentation formats (plenario, charla + demo, live coding, práctico guiado, práctico, grupo) to maintain engagement

### Key Entities

- **Workshop Block**: Represents a discrete segment of the course with defined duration, format (plenario/charla/live coding/práctico), content focus, and learning objectives
- **Practical Exercise**: A hands-on activity with instructions, time limit, difficulty level (guided vs. independent), and measurable success criteria
- **MCP Server Component**: Educational element covering specific aspects of MCP architecture (manifest, resources, tools, authentication, rate limiting, logging)
- **Attendee Group**: Collection of participants working collaboratively on Exercise 4, with varying skill levels and roles
- **Course Materials**: Supporting documentation including exercise instructions, code templates, reference examples, and presentation slides
- **Data Source**: External or simulated data systems used in exercises to demonstrate MCP's data exploitation capabilities

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: Workshop delivers all 11 blocks within the 3-hour time allocation with maximum 5-minute variance per block
- **SC-002**: At least 80% of attendees successfully complete Exercise 1 (static resource) within the 15-minute timeframe
- **SC-003**: At least 70% of attendees successfully complete Exercise 2 (parametric queries) within the 20-minute timeframe
- **SC-004**: At least 90% of groups produce a working virtual analyst prototype in Exercise 4 within 30 minutes
- **SC-005**: Attendee satisfaction rating averages 4.0 or higher out of 5.0 in post-workshop feedback
- **SC-006**: At least 75% of attendees report confidence in building basic MCP servers after workshop completion
- **SC-007**: All attendees can articulate the difference between MCP and traditional plugins after the fundamentals block
- **SC-008**: Post-workshop assessment shows 80% of attendees can identify appropriate B2B use cases for MCP implementation
- **SC-009**: Live coding demonstration in "Anatomía de un Proveedor" block completes successfully without critical errors that disrupt learning flow
- **SC-010**: Exercise materials support independent completion without requiring extensive instructor intervention for more than 20% of attendees

## Assumptions

- Attendees have basic development knowledge and are familiar with at least one programming language
- Venue provides adequate computing resources (laptops, internet connectivity, power) for all attendees
- An appropriate technology stack/framework for teaching MCP within data exploitation context will be selected before workshop delivery (e.g., Python with data libraries, Node.js with data connectors)
- Attendees come prepared with necessary development environment pre-installed or can install quickly
- Workshop will be delivered in Spanish based on the original user input language
- Exercise complexity is calibrated for mixed skill levels, with fastest learners able to complete bonus challenges
- Instructor has expertise in both MCP architecture and data exploitation techniques
- Group sizes for Exercise 4 will be 3-5 people per group to enable effective collaboration
- Course materials will include fallback options (offline data, backup demos) for connectivity issues
