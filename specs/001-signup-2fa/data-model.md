# Data Model: Secure Sign-Up with Mandatory 2FA

## Entities

### ApplicationUser (Existing Entity + Proposed Extensions)
Existing Fields (from code):
- Id (string, IdentityUser)
- UserName / Email (Identity-managed; unique enforced by Identity options)
- CreatedAt (UTC)
- LastLoginAt (UTC nullable)
- IsActive (bool)
- FailedLoginAttempts (int)
- LockedUntil (DateTime? lockout timestamp)
- PasswordResetTokens (collection)
Existing IdentityUser fields implicitly available: PasswordHash, EmailConfirmed, PhoneNumber, TwoFactorEnabled (will be used instead of custom TwoFactorActive where possible).

Proposed Additional Fields (evaluate necessity):
- TwoFactorSecret (encrypted Base32) OR reuse existing authenticator key store via Identity.
- Pending2FAStartAt (DateTime? track enrollment start)
- Failed2FAAttemptCount (int) – separate from FailedLoginAttempts for granular monitoring.
- BackoffNextAllowedAt (DateTime? replaces LockedUntil for progressive backoff without full lockout) – may reuse LockedUntil if semantics align.

Rationalization:
- Prefer leveraging built-in Identity lockout (FailedLoginAttempts + LockedUntil) for initial progressive backoff implementation; custom BackoffNextAllowedAt added only if granularity needed beyond Identity defaults.
- Use `TwoFactorEnabled` instead of new boolean; `TwoFactorSecret` only if Identity's authenticator storage insufficient for encryption requirement.

Constraints:
- Email unique
- LockedUntil null unless user under lock/backoff
- TwoFactorSecret only stored encrypted; cleared only if 2FA reset workflow (out of scope)

Validation Rules:
- Password policy enforced (min length via DomainConstants)
- Email format basic check (Identity already validates)
- TwoFactorSecret length >=16 chars base32 if stored

State Transitions:
- Registration: TwoFactorEnabled = false; Pending2FAStartAt set.
- 2FA Setup Start: TwoFactorSecret generated; Pending2FAStartAt set.
- 2FA Activation: TwoFactorEnabled = true; Pending2FAStartAt cleared; Failed2FAAttemptCount reset.
- Failed Login: FailedLoginAttempts++; if threshold reached set LockedUntil/backoff.
- Successful Login: FailedLoginAttempts reset; LockedUntil cleared; LastLoginAt updated.

### TwoFactorSetupSession (Transient / DB or Cache)
Fields:
- Id (GUID)
- UserId (foreign key to UserAccount)
- Secret (Base32 temporary, encrypted if persisted)
- QRPayload (string)
- CreatedAt (UTC)
- ExpiresAt (UTC - CreatedAt + 10m)
- AttemptCount (int)

Constraints:
- ExpiresAt > CreatedAt
- AttemptCount increments per code entry

Lifecycle:
- Created after successful registration
- Expires after 10 minutes if not activated
- On activation: data folded into UserAccount, session removed

### LoginSecurityState (Deferred Separate Entity)
Deferred; not created in initial implementation. Consider if audit history of backoff events or multi-factor risk scoring needed.

## Relationships
- UserAccount 1..1 TwoFactorSetupSession (temporary) during setup
- UserAccount 1..1 LoginSecurityState (optional design alternative)

## Derived Data
- BackoffActive = Now < NextLoginAllowedAt
- TwoFactorPending = !TwoFactorActive && Pending2FAStartAt != null

## Notes
- Align with existing `ApplicationUser` to avoid redundant fields (use TwoFactorEnabled, lockout where possible).
- Audit logs stored separately (not modeled here).
- Minimal additions reduce migration complexity.
