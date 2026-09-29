---
paths:
  - "**/Dockerfile"
---
# Dockerfile Conventions

> **How to use this rule:** Applies whenever you author or modify a `Dockerfile` in any repo.

---

## Reproducible builds

Pin base images to a fully-qualified version tag. Never use floating tags.

- Do: `FROM mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.24`
- Don't: `FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine`

**Why:** floating tags (`:8.0-alpine`, `:latest`, `:lts`) silently drift each month as upstream images are patched. That makes builds non-reproducible and makes it impossible to tell whether a rebuild is needed to pick up a CVE fix.

When bumping a base image, update the full tag (including the alpine version suffix) in a single commit so the change is auditable. Dependabot's `docker` ecosystem opens that commit for you.

## Minimal base images

Shrink the attack surface of the final image.

- Prefer `alpine`-based images (or smaller — `distroless`, `scratch`) over full Debian/Ubuntu variants.
- Use **multi-stage builds** so SDKs, compilers, and dev headers stay in the build stage and never reach the runtime image.
- Install only what the runtime stage actually needs. Reuse what alpine already provides instead of pulling in extra packages — e.g. use `wget` for healthchecks instead of installing `curl`:

  ```dockerfile
  HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD wget -q -T 1 -O /dev/null http://localhost:8080/health || exit 1
  ```

## Secrets in builds

If the build needs access keys, licenses, or other credentials:

- **Never** pass them via `ENV` or `ARG` — `ENV` values persist in image layers and `ARG` values are visible in `docker history`, so both leak.
- Use [Docker Build Secrets](https://docs.docker.com/build/building/secrets/) (`--mount=type=secret=...`) so the value is available to a single `RUN` step and stays out of the image and build cache.

```dockerfile
RUN --mount=type=secret,id=ENV_NAME,env=ENV_NAME \
    --mount=type=secret,id=ENV2_NAME,env=ENV2_NAME \
    dotnet restore
```

## Rules

- Pinned base image tag (including OS suffix) in every `FROM`.
- Multi-stage build whenever the build needs tooling the runtime doesn't.
- No credentials in `ENV` or `ARG`. Use build secrets.
- Final image installs only runtime dependencies — review every `apk add` / `apt-get install` line before merging.