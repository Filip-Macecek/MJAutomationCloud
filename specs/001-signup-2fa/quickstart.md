# Quickstart: Secure Sign-Up with Mandatory 2FA

## Goals
Demonstrate end-to-end registration → 2FA setup → activation → login with backoff handling.

## Prerequisites
- Feature branch `001-signup-2fa` checked out
- Database migrated (User table includes new fields if added)
- API running locally (e.g., https://localhost:5001)

## Flow Overview
1. Register user
2. Initiate 2FA setup (if not auto-initiated)
3. Activate 2FA with TOTP code
4. Login with 2FA
5. Trigger backoff (optional testing)

## Step 1: Register
POST /auth/register
```json
{ "email": "user@example.com", "password": "Str0ngPass!Word" }
```
Expected 201 response with `twoFactorPending: true`.

## Step 2: Setup 2FA
POST /auth/2fa/setup
```json
{ "userId": "<returned-user-id>" }
```
Response provides `qrPayload`, `secret`, `expiresAt`.
Scan using authenticator app.

## Step 3: Activate 2FA
Generate current TOTP code from app.
POST /auth/2fa/activate
```json
{ "userId": "<returned-user-id>", "code": "123456" }
```
Expect 200. If invalid, test retry and 429 after excessive failures.

## Step 4: Login
POST /auth/login
```json
{ "email": "user@example.com", "password": "Str0ngPass!Word", "code": "654321" }
```
Expect status `Authenticated`.

## Backoff Test (Optional)
Submit incorrect password or 2FA code repeatedly:
- After 5 failures: expect 423 with ~30s backoff
- After success: backoff cleared

## Status Check
GET /auth/status
Expect flags for `twoFactorActive` and no backoff once authenticated.

## Edge Case Validation
- Attempt login before activation → status `TwoFactorRequired` or `TwoFactorPending`
- Expired setup (wait >10m) then activation attempt → expect 400
- Restart setup after expiration: POST /auth/2fa/setup again

## Clean Up
Remove test user via admin tools (future feature) or DB manual deletion.

## Notes
All secrets must remain encrypted at rest; do not log raw `secret` or TOTP codes.
