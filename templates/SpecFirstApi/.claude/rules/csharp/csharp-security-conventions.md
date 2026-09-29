---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Security Conventions

## Input and output

* Validate all external input at the entry point with allow-lists: request contracts carry DataAnnotations and the framework's validation rejects invalid requests with 400 before any handler runs.
* Encode output sent to untrusted sinks with framework encoders.

## Error handling

* Never expose internal errors, stack traces or sensitive information. Unhandled exceptions become a constant-title Problem Details response (TECH-003).

## Secrets and cryptography

* Never hard-code secrets. `appsettings.json` holds local defaults only; real values come from the environment or a secrets manager. The pre-commit hook and CI run gitleaks.
* Never implement cryptography yourself; use the platform's libraries.

## Authentication and authorization

* Prefer role- or scope-based access control.
* Service-to-service calls use signed, short-lived tokens; validate signature, issuer and expiry.
* If passwords must be stored, store salted hashes.
* The template ships without authentication on purpose; add it as TECH requirements in the first real change, never silently.

## Data protection

* Encrypt in transit and at rest with standard algorithms. Collect only what is needed; anonymise where possible. Rotate keys.

## Dependencies

* Only packages from the configured feed (`NuGet.Config`); central package management pins versions; Dependabot proposes updates.
* `SonarAnalyzer.CSharp` and `BannedApiAnalyzers` run on every project with warnings as errors; do not suppress a security rule to make a build pass.

## Secure defaults

* Deny until explicitly allowed. A failing component leaves the system in a safe state.

## Docker images

* See [`../common/dockerfile-conventions.md`](../common/dockerfile-conventions.md): pinned base-image tags, minimal multi-stage builds, no secrets in `ENV` or `ARG`, non-root user.
