# Research: Secure Sign-Up with Mandatory 2FA

## Decisions

### 2FA Implementation Approach
- **Decision**: Use ASP.NET Core Identity built-in authenticator (TokenOptions.DefaultAuthenticatorProvider).
- **Rationale**: Eliminates custom crypto/code; reduces maintenance; sufficient for mandatory TOTP requirement; leverages existing persistence (`AspNetUserTokens`).
- **Alternatives Considered**: Otp.NET (more control, required custom secret handling), custom TOTP implementation (higher risk), external MFA provider (out of scope phase 1).

### Test Framework Confirmation
- **Decision**: Use xUnit for unit/integration tests.
- **Rationale**: Common in .NET ecosystem, parallel test execution, existing community tooling; aligns with assumed project style.
- **Alternatives Considered**: NUnit (similar capability), MSTest (more verbose), SpecFlow (adds complexity not needed for initial feature).

### Test Project Pathing
- **Decision**: Create `MJAutomationCloud.Tests` project at repo root for feature tests.
- **Rationale**: Centralized test assembly keeps Identity + UI integration straightforward via TestServer; consistent naming pattern.
- **Alternatives Considered**: Separate domain/application/integration test projects (overhead for current scope), mixing tests inside existing projects (violates separation of concerns).

### Progressive Backoff Persistence Strategy
- **Decision**: Store failed attempt counters and next allowed timestamp in Identity user table extension or separate `LoginSecurityState` entity.
- **Rationale**: Ensures durability across app restarts; single write footprint; avoids race conditions.
- **Alternatives Considered**: In-memory cache only (lost on restart), distributed cache (overkill for initial scale).

### 2FA Secret Storage Format
- **Decision**: Rely on Identity authenticator key storage in `AspNetUserTokens` (no custom field/migration).
- **Rationale**: Simplifies schema; avoids premature optimization; accepted risk for MVP.
- **Alternatives Considered**: Encrypted custom column (deferred), external secret vault, HSM integration.

### Encrypted Secret Implementation Details (Removed)
- Reverted: Custom encrypted column dropped. Future enhancement: introduce encrypted storage if compliance requires.

### Progressive Backoff vs Identity Lockout
- **Decision**: Reuse existing `FailedLoginAttempts` + `LockedUntil` fields; defer custom `BackoffNextAllowedAt`.
- **Rationale**: Avoid duplicate mechanisms; Identity lockout meets initial security requirement; simplifies migrations.
- **Alternatives Considered**: Custom exponential schedule (fine-grained control), separate state entity (adds complexity), distributed rate limiter (premature optimization).
- **Notes**: If UX demands softer throttling without full lockout, introduce `BackoffNextAllowedAt` later.

## Resolved NEEDS CLARIFICATION Items
- 2FA implementation path (built-in Identity)
- Test framework confirmation
- Test project path

## No Further Clarifications Required for Plan
Feature ready for Phase 1 design artifacts.
