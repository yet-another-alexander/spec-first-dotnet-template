---
description: Write the implementation plan for a change whose spec PR is merged or approved
argument-hint: <change folder name>
---

Change: $ARGUMENTS

Read `docs/specs/changes/$ARGUMENTS/spec.md`, the requirements it lists, the contracts it touches, the `@wip` tests
that prove it, and the code the change lands in (`src/SpecFirst.Service.Api/`). Then write
`docs/specs/changes/$ARGUMENTS/plan.md`:

- **Approach**: how the code will satisfy each requirement, one paragraph per feature folder touched. Reuse what
  exists; name the setup file, endpoint module or entity you extend.
- **Data**: entities, EF configuration and the migration name if the schema changes. The migration must produce exactly
  the lines already added to `docs/specs/db/schema.sql`.
- **Contract**: which operations in `openapi.yaml` the endpoints will expose, with their operation ids.
- **Sequencing**: what must land first so every intermediate PR keeps CI green.
- **Risks and open points**: anything that would need a spec change; the plan may not change the spec itself.

No code. The plan is the input to `/tasks`.
