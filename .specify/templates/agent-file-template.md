# [PROJECT NAME] Development Guidelines

Auto-generated from all feature plans. Last updated: [DATE]

## Active Technologies

[EXTRACTED FROM ALL PLAN.MD FILES]

## Project Structure

```text
[ACTUAL STRUCTURE FROM PLANS]
```

## Commands

[ONLY COMMANDS FOR ACTIVE TECHNOLOGIES]

## Code Style

[LANGUAGE-SPECIFIC, ONLY FOR LANGUAGES IN USE]

## Recent Changes

[LAST 3 FEATURES AND WHAT THEY ADDED]

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

## Principle Reference Quick Guide
I. DDD Integrity: Keep domain pure; avoid infra leakage.
II. Secure Communication: Auth handshake required for WebSocket/machine endpoints.
III. Deterministic Orchestration: Idempotent job operations; explicit lifecycle states.
IV. Conversion Accuracy: Deterministic image→GCode; validate parameters.
V. Observability & Safety: Structured logging, metrics, audit, emergency stop handling.
