# Feature Specification: Secure Sign-Up with Mandatory 2FA

**Feature Branch**: `001-signup-2fa`  
**Created**: 2025-11-02  
**Status**: Draft  
**Input**: User description: "We need a sign-up flow. We will have a sign-up flow that allows users to create their profile. It should force them to create a strong password (long, not with bizzare characters) and setup 2FA straightaway. Upon registration, users will be forced to log-in. For now, we will just show empty screen after login and nothing else. We will work on user's adminsitration later. We will only collect e-mail address of the user and all necessary data for 2FA to work.. nothing else. We do not require confirming the e-mail."

## Clarifications

### Session 2025-11-02
- Q: Account lockout strategy for repeated failed login attempts (password or 2FA) → A: Progressive backoff delays (30s → 2m → 10m) after thresholds (5, 8, 10 failures), reset on successful login.
- Q: 2FA setup session expiry duration → A: 10 minute expiry (session invalid after 10 minutes; user must restart setup to obtain new secret/QR).

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

### User Story 1 - Create Account & Enforce Strong Password (Priority: P1)

User visits the sign-up page, enters email and chooses a strong password meeting strength rules. System
validates password strength immediately, rejects weak ones with actionable feedback. Account is created
with minimal profile (email only) and user is seamlessly directed to 2FA setup.

**Why this priority**: Foundational security entry point; cannot proceed to 2FA or login without secure
credentials.

**Independent Test**: Attempt account creation using representative weak/strong passwords and verify only
strong passwords allow progression; successful creation leads to 2FA setup initiation.

**Acceptance Scenarios**:
1. **Given** a visitor without an account, **When** they submit email + weak password, **Then** system blocks
  creation and displays strength guidance.
2. **Given** a visitor without an account, **When** they submit email + strong password, **Then** system
  creates account and starts 2FA setup.

---

### User Story 2 - Mandatory 2FA Enrollment (Priority: P2)

Immediately after account creation user is shown 2FA setup flow (e.g., TOTP app pairing). User scans code
or receives secret, enters generated one-time code. System validates the code and marks 2FA active. User
cannot access application (even the empty landing screen) until 2FA is successful.

**Why this priority**: Ensures all accounts have strong second factor from inception reducing risk.

**Independent Test**: Attempt navigation after account creation before completing 2FA; verify access
blocked. Complete 2FA setup with valid code to gain access.

**Acceptance Scenarios**:
1. **Given** a newly created account pending 2FA, **When** user attempts to bypass 2FA to reach app, **Then**
  system prevents access and redirects to 2FA setup.
2. **Given** a pending 2FA setup, **When** user enters a correct one-time code, **Then** system activates
  2FA and grants login session.
3. **Given** a pending 2FA setup, **When** user enters an invalid one-time code multiple times, **Then**
  system limits attempts and provides retry guidance.
4. **Given** an account where initial 2FA setup was abandoned or failed, **When** user later submits valid
  email + password at login, **Then** system re-routes to 2FA setup flow and upon successful activation
  redirects back to login to complete full authentication.

---

### User Story 3 - First Login Post-Registration (Priority: P3)

After successful 2FA activation, user performs initial login (email + password + 2FA). System grants
session and presents an empty placeholder screen (intentionally minimal). No additional profile data is
requested or required.

**Why this priority**: Demonstrates end-to-end flow completion and verifies frictionless post-onboarding
access even without extended profile features.

**Independent Test**: Execute login using newly registered credentials + valid 2FA token; confirm empty
screen loads and no unauthorized redirects occur.

**Acceptance Scenarios**:
1. **Given** an activated 2FA account, **When** user submits valid credentials + token, **Then** system
  grants access and shows empty placeholder screen.
2. **Given** an activated 2FA account, **When** user submits invalid token, **Then** system blocks access
  and explains retry procedure.
3. **Given** an activated 2FA account, **When** user submits wrong password, **Then** system rejects login
  with non-revealing message (no indication whether email exists).

---

Story Count Justification: Three stories cover creation, mandatory security enrollment, and confirmation of
usable authenticated access. Future administration features intentionally excluded from scope.

### Edge Cases

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases.
-->

- Password strength edge: extremely long (e.g., >128 chars) password submitted.
- Repeated failed 2FA attempts (rate limit threshold).
- User abandons 2FA mid-process then returns later (pending state re-entry).
- Duplicate email attempted (existing account protection without disclosure).
- 2FA time skew causing code near expiration.
- Session timeout during forced 2FA setup.
- Progressive backoff waiting period after repeated failed login attempts.

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST allow new users to register with only email + password (no other profile fields).
- **FR-002**: System MUST enforce password strength: minimum length 12, includes at least 3 of 4 categories
  (uppercase, lowercase, digit, common symbol), rejects overly complex sequences of random special chars
  (avoid unusable passwords) with feedback.
- **FR-003**: System MUST prevent account creation with an email already registered (generic error message).
- **FR-004**: System MUST initiate mandatory 2FA setup immediately after successful registration.
- **FR-005**: System MUST block any authenticated area access until 2FA activation is complete.
- **FR-006**: System MUST allow user to pair a standard TOTP authenticator app during setup.
- **FR-007**: System MUST validate entered one-time codes and limit invalid attempts (lockout after 10
  consecutive failures within 10 minutes).
- **FR-008**: System MUST require email + password + valid current 2FA token on first login post-setup.
- **FR-009**: System MUST present an empty placeholder screen after successful login (no additional data).
- **FR-010**: System MUST store only email, password hash, 2FA secret, and necessary audit timestamps
  (created, last login, 2FA activated). No other personal data.
- **FR-011**: System MUST log security events: registration attempt, registration success, 2FA activation,
  failed 2FA attempt count, login success/failure.
- **FR-012**: System MUST provide password strength feedback messages referencing unmet criteria.
- **FR-013**: System MUST allow user to restart 2FA setup if abandoned before activation.
- **FR-014**: System MUST ensure partial registration states (pending 2FA) cannot perform login.
- **FR-015**: System MUST implement progressive backoff for failed login attempts (password or 2FA token): After 5 consecutive failures impose 30s wait; after 8 impose 2m; after 10 impose 10m; counters reset upon successful authentication; waiting period communicated to user without exposing which credential failed.

*Clarifications decided via reasonable defaults (no markers required)*
Email verification intentionally omitted per feature description.

### Key Entities *(include if feature involves data)*

- **User Account**: Represents a registered individual. Attributes: Email, PasswordHash, TwoFactorSecret,
  TwoFactorActiveFlag, CreatedAt, LastLoginAt, Failed2FAAttemptCount (rolling window), Pending2FAStartAt.
- **TwoFactorSetupSession**: Temporary pairing context. Attributes: UserId, Secret (transient until stored),
  QRPayload, ExpiresAt (10 minutes from creation), AttemptCount.

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: 90% of new users successfully complete registration + 2FA activation within 5 minutes.
- **SC-002**: <2% of registration attempts fail due to password strength after first retry.
- **SC-003**: 95% of first logins post-2FA activation succeed on first try (excluding intentional invalid tests).
- **SC-004**: 100% of accounts have 2FA active immediately after creation (no bypass recorded).
- **SC-005**: Security event logs record 100% of registration, 2FA activation, and login outcomes.

### Edge Success Metrics
- **SC-006**: Abandoned 2FA sessions (started but not activated within 30 minutes) <10% of registrations.
  (Measured with 10 minute session expiry; abandonment defined as expiration without activation.)

### Assumptions
- TOTP chosen as 2FA mechanism; no SMS/email fallback in this feature scope.
- Password strength categories reflect common security baseline; future policies may refine.
- Rate limiting values (10 failures/10 minutes) adequate initial deterrent; can be tuned later.
- Empty screen placeholder considered acceptable MVP post-login experience.

### Out of Scope
- Email verification, password reset flows, account profile management, backup codes, device trust.
- Admin user management functions.

### Constitution Alignment (MANDATORY)
- Principle I: Identify affected bounded context & confirm no leakage across contexts.
- Principle II: List auth roles/devices introduced or modified.
- Principle III: Describe idempotency strategy for any job/queue operations.
- Principle IV: Define test cases for deterministic output if processing/conversion affected.
- Principle V: List new logs/metrics/audit events.

Principle I: Operates within Authentication bounded context; no domain business rules altered.
Principle II: Introduces mandatory 2FA for all user accounts at creation; roles unchanged.
Principle III: Not a job/queue feature; idempotency applies to registration (repeat attempt with same email
produces single account) ensured by uniqueness constraint.
Principle IV: Not related to image/GCode conversion.
Principle V: Adds logging events: RegistrationAttempt, RegistrationSuccess, TwoFactorSetupStart, TwoFactorActivation, LoginSuccess, LoginFailure, TwoFactorFailureLocked.
