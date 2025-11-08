<!--
Sync Impact Report
Version: 1.0.0 -> 1.1.0
Modified Principles: None (existing principles unchanged)
Added Sections: Principle VI (Framework-First Development)
Removed Sections: None
Templates Updated: 
	- .specify/templates/plan-template.md ✅ (added Principle VI gate check)
	- .specify/templates/spec-template.md ✅ (no changes required - principle is implementation-focused)
	- .specify/templates/tasks-template.md ✅ (no changes required - tasks already reference plan gates)
	- .specify/templates/checklist-template.md ✅ (no changes required - generic quality checks)
	- .specify/templates/agent-file-template.md ✅ (no changes required - agent guidance unchanged)
Quality Gates Updated:
	- Added gate 6: Principle VI framework-first evaluation requirement
Deferred TODOs:
	- RATIFICATION_DATE (original adoption date unknown – needs confirmation)
Amendment Rationale:
	- MINOR version bump justified: New principle added without removing or redefining existing principles.
	- Principle VI codifies existing best practice (prefer .NET built-ins) as formal governance rule.
	- Addresses technical debt risk from custom authentication/session implementations.
-->

# MJAutomationCloud Constitution

## Core Principles

### I. Domain-Driven Design & Bounded Context Integrity (NON-NEGOTIABLE)
All code MUST honor clear layers: Domain, Application, Infrastructure, UI (Blazor). Aggregates, entities,
value objects, and domain events live ONLY in the Domain layer. Application layer orchestrates use cases
without containing business rules. Infrastructure concerns (EF Core, identity, messaging, file storage)
MUST remain replaceable. Each feature proposal MUST state its bounded context and explicitly reject scope
creep beyond that context.

Rationale: Enforces clarity, testability, and long-term evolvability as laser orchestration expands.

### II. Secure Authenticated Machine & User Communication (NON-NEGOTIABLE)
Every engraving machine and user interaction MUST be authenticated via .NET Identity (users) or signed
machine credentials (devices). WebSocket sessions MUST perform an auth handshake before any operational
message. All commands (e.g., engrave, status) MUST be authorization-checked per role/device scope. No
unauthenticated state mutations are permitted. Secrets (tokens, signing keys) MUST never be logged.

Rationale: Prevents unauthorized machine control and protects customer artwork data.

### III. Deterministic Job Orchestration & Queue Integrity
Task scheduling MUST be idempotent (replays do not duplicate physical engraving). Each job transitions
through a defined lifecycle (Queued → Preparing → Engraving → Completed | Failed | Canceled). Retries MUST
be bounded and logged with correlation IDs. Concurrency controls MUST prevent two jobs targeting the same
machine simultaneously. Any queue implementation MUST provide durability guarantees (no silent loss).

Rationale: Guarantees predictable production flow and reliable recovery from transient failures.

### IV. Image Processing & GCode Conversion Accuracy
Image-to-GCode conversion MUST be deterministic for identical inputs. Conversion parameters (resolution,
power, speed) MUST be validated against machine capability profiles. Generated GCode MUST include safety
prologue/epilogue (e.g., homing, laser off guard). Any transformation pipeline MUST surface metrics: avg
conversion time, failure rate. Test cases MUST cover edge inputs: high contrast, large dimensions, empty/
transparent regions.

Rationale: Ensures engraving fidelity and machine safety while enabling performance tuning.

### V. Observability, Auditability & Operational Safety
Structured logging (correlation IDs per job + machine) MUST exist for: auth events, job lifecycle
transitions, conversion steps, WebSocket connect/disconnect, error paths. Metrics MUST include queue depth,
job throughput, conversion latency, error counts. Audit records MUST capture who/what initiated state
changes (user ID or machine identity). Safety rules: Any unexpected disconnect while engraving MUST trigger
an emergency stop command; all such events are logged and surfaced in reporting.

Rationale: Provides traceability for production incidents and supports continuous optimization & safety.

### VI. Framework-First Development (NON-NEGOTIABLE)
Always leverage built-in .NET framework capabilities before implementing custom solutions. ASP.NET Core
Identity MUST be used for user authentication, session management, password hashing, and security token
generation—do not reinvent authentication mechanisms. Entity Framework Core conventions and built-in
features (migrations, change tracking, query optimization) MUST be preferred over custom data access
patterns. Blazor's component lifecycle, data binding, and built-in validation MUST be utilized before
creating custom abstractions. When framework features exist for a requirement (logging, configuration,
dependency injection, middleware, authorization policies), they MUST be evaluated and used unless a
documented, specific limitation prevents their use.

Rationale: Reduces maintenance burden, leverages battle-tested security implementations, ensures upgrade
path compatibility, and prevents accumulation of custom code debt that duplicates framework capabilities.

## Architecture Constraints & Technology Standards

Stack: .NET 8 (assumed) + Blazor Server/WebAssembly (front-end), EF Core for persistence, ASP.NET Identity
for authentication, WebSocket for machine communication, optional message queue/bus (TBD). All external
communication protocols MUST be versioned (e.g., WebSocket message schemas with semantic version header).
Persistent entities MUST avoid leaking infrastructure types (no DbContext in domain). Configuration MUST
be environment-driven (appsettings + overrides). Performance baseline targets (initial): <200ms p95 for
job scheduling API, <2s typical image→GCode conversion for standard 1080p grayscale image (adjust as data
collected). Security: Principle of least privilege for service accounts; TLS required for all machine
connections.

## Development Workflow & Quality Gates

Branch Naming: feature/<id>-<slug>. Every feature begins with a spec & plan referencing the principles.
Pull Requests MUST:
- Link to spec/plan tasks.
- Demonstrate tests for new job lifecycle branches or conversion changes.
- Include security & observability impact notes if behavior surface changes.

Quality Gates (PR MUST NOT MERGE if failing):
1. Principle I layer violations (detected via code review) resolved.
2. Principle II authentication enforced in new endpoints/messages.
3. Principle III idempotency & state transition clarity documented.
4. Principle IV conversion tests added/updated if GCode logic touched.
5. Principle V logging & metrics coverage for new operational paths.
6. Principle VI framework-first evaluation documented (custom solution justified if chosen).
7. No TODOs remaining except explicitly approved by maintainer.

Definition of Done (Feature): All user stories deliver individually testable value, docs updated (quickstart
or contracts), tasks.md reflects completion, and observability signals deployed.

## Governance

Authority: This constitution supersedes ad-hoc practices. Conflicts resolved by adhering to principles in
numeric order of criticality (I has priority over V when trade-offs occur).

Amendment Procedure:
1. Open governance issue describing proposed change + impact analysis.
2. Draft PR updating this file + affected templates (plan/spec/tasks).
3. Obtain at least one maintainer approval and one domain expert approval.
4. Assign version bump per semantic rules and update LAST_AMENDED_DATE.
5. Merge only after any required migration plans are documented.

Versioning Policy:
- MAJOR: Remove or redefine a principle, or introduce incompatible workflow changes.
- MINOR: Add a new principle or significantly expand governance scope.
- PATCH: Clarifications, typo fixes, non-semantic wording improvements.

Compliance Reviews: Every PR reviewer MUST check Principle Gate adherence. A periodic (monthly) audit MUST
sample job logs, conversion accuracy metrics, and security events for drift.

Enforcement: Non-compliant merges MUST trigger a remediation task within 48h. Critical security/safety
violations trigger immediate hotfix procedure.

**Version**: 1.1.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date unknown | **Last Amended**: 2025-11-08

