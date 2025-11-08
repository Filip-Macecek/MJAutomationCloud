# Implementation Plan: Secure Sign-Up with Mandatory 2FA

**Branch**: `001-signup-2fa` | **Date**: 2025-11-02 | **Spec**: `specs/001-signup-2fa/spec.md`
**Input**: Feature specification from `specs/001-signup-2fa/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement minimal registration + mandatory TOTP-based 2FA and initial login. Password strength enforced,
progressive backoff for repeated failed logins, minimal data stored (email + security artifacts). Users
cannot access application prior to activating 2FA. Recovery path for abandoned setup included.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 8 (C#)  
**Primary Dependencies**: ASP.NET Core Identity, Entity Framework Core, TOTP generation lib (NEEDS CLARIFICATION: library selection e.g., Otp.NET)  
**Storage**: SQL database via EF Core (existing ApplicationDbContext)  
**Testing**: xUnit (assumed) + integration tests via TestServer (NEEDS CLARIFICATION: confirm test framework)  
**Target Platform**: Server-side Blazor (web)  
**Project Type**: Web application (existing multi-project solution)  
**Performance Goals**: Registration + 2FA activation flow median completion < 5 minutes; password validation latency < 50ms; login endpoint p95 < 250ms  
**Constraints**: Minimal PII storage; secrets never logged; progressive backoff times enforced in memory + persisted for durability  
**Scale/Scope**: Initial rollout < 1k users; design must scale to 10k without architectural changes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

All gates expected to pass. No domain layering violations (feature limited to Infrastructure (Identity) + UI + minimal Application orchestrations). Principle II covered by enforced 2FA + password strength. Principle III not directly applicable (no queue jobs). Principle IV not applicable. Principle V logging additions planned.

Required Gates Snapshot (align with `MJAutomationCloud Constitution`):
- Principle I: No domain logic outside Domain project; infrastructure abstractions used.
- Principle II: Auth handshake defined for any new machine/user communication pathway.
- Principle III: Idempotent job/queue operations (explicit state transition table drafted if jobs involved).
- Principle IV: Deterministic conversion path documented when image/GCode features touched.
- Principle V: Logging + metrics additions listed (correlation IDs, lifecycle events).

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
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: Use existing solution projects:
Existing layout:
```
MJAutomationCloud.sln
MJAutomationCloud/                # Blazor Server UI (Components/Account/...)
MJAutomationCloud.Domain/         # Domain abstractions (no changes for this feature)
MJAutomationCloud.Application/    # Application services (extend with IAuthentication orchestration if needed)
MJAutomationCloud.Infrastructure/ # EF Core, Identity, Entities (ApplicationUser already present)
```
Feature implementation will:
- Extend `ApplicationUser` only if necessary (2FA secret & backoff fields) but prefer using IdentityUser's existing lockout fields when possible.
- Add Blazor pages/components under `MJAutomationCloud/Components/Account/` for Register, TwoFactorSetup, Login.
- Add infrastructure service(s) (e.g., `TwoFactorService`) if not covered by existing `IAuthenticationService`.
- Introduce test project `MJAutomationCloud.Tests/` (xUnit) at root.
Unneeded options removed. No new bounded context introduced.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

Post-Design Constitution Re-Check:
- Principle I: No domain logic added; all changes confined to Identity extension and Application orchestration.
- Principle II: Mandatory 2FA + password policy + backoff satisfy secure auth handshake scope (machine endpoints unaffected).
- Principle III: Not applicable (no job orchestration).
- Principle IV: Not in scope (no image/GCode changes).
- Principle V: Logging events enumerated; metrics plan includes registration/activation success ratio & backoff occurrences.

Complexity Justification:
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| Additional user fields (attempted) | Not added | Minimal data principle enforced |
| Separate LoginSecurityState entity | Deferred | Embedding reduces joins; scale small |

