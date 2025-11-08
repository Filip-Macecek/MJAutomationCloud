# MJAutomationCloud Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-02

## Active Technologies

- .NET 8 (C#) + ASP.NET Core Identity, Entity Framework Core, TOTP generation lib (NEEDS CLARIFICATION: library selection e.g., Otp.NET) (001-signup-2fa)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

# Add commands for .NET 8 (C#)

## Code Style

.NET 8 (C#): Follow standard conventions

## Recent Changes

- 001-signup-2fa: Added .NET 8 (C#) + ASP.NET Core Identity, Entity Framework Core, TOTP generation lib (NEEDS CLARIFICATION: library selection e.g., Otp.NET)

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

## Principle Reference Quick Guide
I. DDD Integrity: Keep domain pure; avoid infra leakage.
II. Secure Communication: Auth handshake required for WebSocket/machine endpoints.
III. Deterministic Orchestration: Idempotent job operations; explicit lifecycle states.
IV. Conversion Accuracy: Deterministic image→GCode; validate parameters.
V. Observability & Safety: Structured logging, metrics, audit, emergency stop handling.
