# Spec-first workflow

The specification is the source of truth. Code conforms to it; CI checks that it did. A rule an agent can edit is
advice; only CI enforces. Everything in this document that matters is enforced by one of the checks in section 7.

## 1. Requirements

Every behaviour is one sentence in EARS (Easy Approach to Requirements Syntax): one `shall`, one testable behaviour.
The pattern tells the test author how to verify.

| Pattern | Form | Use it for | Verified by |
|---|---|---|---|
| Ubiquitous | The system shall … | Invariants, non-functionals | Property test |
| Event-driven | When X, the system shall … | Normal flow | Example test |
| State-driven | While X, the system shall … | Modes, statuses | Example test |
| Unwanted behaviour | If X, then the system shall … | Errors, timeouts, invalid input | Negative test |
| Optional feature | Where X, the system shall … | Feature flags, tenant settings | Feature-gated test |

Patterns combine in the order `Where` → `While` → `When` / `If`; the `EarsGrammarTests` guardrail checks the form,
the `spec-reviewer` agent checks the substance. An `and` between two actions means two requirements.
The subject is the system, the response is observable, numbers replace adjectives, and implementation decisions are
not requirements: "shall store in Redis" is an ADR; "shall respond within 200 ms" is a requirement.

**Identity.** `REQ-nnn` for behaviour observable by a client, `TECH-nnn` for behaviour observable by the system and
verifiable by a test (migrations at startup, idempotent handlers, error shapes). Decisions no test can fail are ADRs in
`docs/adr/`. The requirement text lives in exactly one place, `docs/specs/requirements/`; everything else refers to
the id. Ids are stable: rewording keeps the id, changing the meaning retires the id and creates a new one. Removed
requirements are deleted; git has the history. Ticket keys name the change, not the requirement.

**Hash.** Every test that proves a requirement carries the first eight hex characters of SHA-256 over the normalised
requirement text (trimmed, runs of whitespace collapsed to one space). When the text changes, the hash no longer
matches and CI fails until a human either confirms the test still proves the requirement and updates the hash, or puts
the test back to work in progress. The `requirement-hash` skill (`.claude/skills/`) computes, checks and rewrites the
hashes with the guardrail's algorithm.

**Format.** In `functional.md` and `technical.md`: a `### REQ-nnn` heading, optionally followed by tags such as
`@no-test`, then the requirement as the first paragraph. Anything after a blank line is commentary and is not hashed.

## 2. Repository layout

```
docs/adr/                                  decisions without a test
docs/specs/                                protected: spec PRs only
  requirements/functional.md               REQ-nnn, canonical text
  requirements/technical.md                TECH-nnn
  api/openapi.yaml                         the API contract
  db/schema.sql                            the database contract (normalised pg_dump)
  messages/asyncapi.yaml                   the message contract
  changes/<TICKET>-<slug>/                 one folder per change
    spec.md                                ticket link, ids added or changed, contracts touched, out of scope
    plan.md                                approach, files, sequencing
    tasks.md                               one task = one implementation PR
src/SpecFirst.Service.Api/                 the service: feature folders, persistence, setup
src/SpecFirst.Service.Models/              the HTTP contract as code: requests, responses, wire enums (packable)
src/SpecFirst.Service.Messages/            the message contract as code: events and commands with topics (packable)
tests/SpecFirst.Service.Tests.Spec/        protected: acceptance, integration, invariant and guardrail tests
tests/SpecFirst.Service.Tests.Unit/        free zone: the implementer's own tests
```

## 3. Two pull requests

**Spec PR** (label `spec`). Human, or agent under human review. Contains everything that defines what is correct:
requirement text with ids, contract changes, tests tagged work in progress and currently failing, hashes, the change
folder. Reviewed carefully by someone other than its author; this is where human attention goes.

**Implementation PRs**. Agent, one task from `tasks.md` per PR. May not touch `docs/specs/` or the spec test project.
The only permitted change there is deleting a line that is exactly `@wip` once the scenario passes. Reviewed mostly
by the guardrails.

**Roles.** The session that writes the tests is not the session that writes the implementation. A single session
doing both writes tests that suit the implementation rather than the spec. The `implement-change` workflow in
`.claude/workflows/` makes this mechanical: one fresh implementer per task, an independent verifier, one repair
round, and it stops at the first task that does not verify.

**Small changes and bugs.** Same rule, smaller PRs. A bug fix starts with a failing test tagged with the violated
requirement. If no requirement covers the case, the bug revealed a spec gap and the spec PR adds one.

## 4. Tests

| Layer | Where | Proves | Carries ids |
|---|---|---|---|
| Acceptance | `Tests.Spec/Acceptance/*.feature` (Reqnroll, over HTTP, real Postgres) | REQ | `@REQ-nnn:hash` tag |
| Invariant | `Tests.Spec/Invariants` (CsCheck property tests) | Ubiquitous REQ | `[Requirement]` |
| Integration | `Tests.Spec/Integration` (xUnit, real Postgres and RabbitMQ) | TECH, and the schema within the contract | `[Requirement]`, none for the contract check |
| Guardrails | `Tests.Spec/Guardrails` | Traceability, EARS grammar, architecture, the message contract | no |
| Unit | `Tests.Unit` | Internals | no |

Each layer is an xUnit collection (`acceptance`, `integration`, `guardrails`, `invariants`): tests inside a
collection run one at a time, the collections run in parallel. The acceptance scenarios and the integration tests each
start their own API with its own Postgres and RabbitMQ; the other two need no host and no Docker.

Work in progress: a scenario carries `@wip` alone on its own line; a fact carries `[Trait("Category", "wip")]` on its
own line. CI runs everything except `Category=wip` and reports wip tests that already pass. A wip marker may only be
removed in the PR that makes the test pass, which is automatic: once the marker is gone the test runs. A marker is
not a parking place: the nightly `stale-wip` workflow lists every marker with the age of the commit that added it and
fails when one is older than `WIP_MAX_DAYS` (14 by default). Implement the requirement or retire it.

Scenario tables hold example data taken from the ticket. Agents do not invent test data.

## 5. Contracts

The outside shape of the service is declared in `docs/specs/api/openapi.yaml`, `docs/specs/db/schema.sql` and
`docs/specs/messages/asyncapi.yaml`. All three are protected paths. On every PR the actual surface is regenerated from
code and compared with the contract in one direction only: everything present in code must be present in the contract.
The reverse, a contract element with no implementation yet, is pending work and passes. This makes the check independent
of PR order: the spec PR adds an endpoint to the contract and passes; the implementation PR adds it to code and passes;
an agent adding an endpoint nobody asked for fails.

- **API.** The Release build writes the generated document to `spec.json`. `oasdiff breaking spec.json openapi.yaml`
  reports what a client of the generated document would lose in the contract: exactly the elements code exposes and
  the contract does not declare. oasdiff rates changes by what they break for a client, so `.oasdiff-severity.txt`
  bends it into the one-way check: request-side checks that fire when the contract is stricter than the code (a
  tightened bound, a new required property) are demoted to info, because contract ahead of code is pending work;
  removals oasdiff rates informational (an enum value, an error status code, an operation id, an optional response
  property present in code but not in the contract) are promoted to error. The file allows no comments; the oasdiff
  image is pinned so the check ids stay valid. Spectral lints the contract itself, installed from `package.json` so the
  local run and CI use the same pinned version.
- **Database.** `DatabaseSchemaTests` applies the migrations to a fresh container, runs `pg_dump --schema-only`,
  normalises it into statements and checks every statement is in `schema.sql`. The unit is the statement, not the
  line, so a column counts as declared only inside its own `CREATE TABLE`. `UPDATE_SCHEMA=1 dotnet test` regenerates
  the file. CAP's own `cap` schema (outbox, inbox, lock) is library storage and excluded from the dump.
- **Messages.** `MessageContractTests` generates a JSON Schema from every type in the `Messages` project that carries
  `[MessageTopic]` and checks that its
  topic is a channel address in `asyncapi.yaml` and that each property, with its JSON type and required flag, is in
  the payload schema. Messages in the contract with no type in code are pending work. Spectral lints the contract with
  its AsyncAPI ruleset. Publishing goes through CAP's transactional outbox in the service's Postgres, relayed to
  RabbitMQ: a message commits with the business change or not at all.

## 6. Requirement lifecycle

```
written ──(spec PR adds a @wip test)──► work in progress ──(implementation PR deletes @wip)──► covered
   ▲                                                                                              │
   └── forbidden on main: traceability fails ──────── text changed, hash mismatch ◄───────────────┘
```

## 7. What CI checks

| Check | Where | Fails when |
|---|---|---|
| Formatting | `format` job | CSharpier would change a file |
| Protected paths | `protected-paths` job, PRs without label `spec` | `docs/specs/` or the spec tests change other than by deleting `@wip` lines or `[Trait("Category", "wip")]` lines; adding a wip marker fails too |
| Traceability | `TraceabilityTests` | a testable requirement has no test, a marker names an unknown id or a stale hash, a requirement has zero or several `shall`, an id is duplicated, `@wip` shares a line, or the spec contains an open clarification marker |
| EARS grammar | `EarsGrammarTests` | a requirement is not one capitalised sentence ending in a period, or does not follow one of the five patterns combined in the order Where, While, When/If; judgement (observable, precise, complete) is the `spec-reviewer` agent's job |
| Architecture | build (NsDepCop, BannedSymbols) and `ArchitectureTests` | a namespace dependency or API outside the rules; an unsealed contract or numeric enum |
| API contract | `contracts` job | Spectral finds an error in `openapi.yaml`, or oasdiff finds code exposing what the contract does not declare |
| Database contract | `DatabaseSchemaTests` | the migrated schema has a statement `schema.sql` does not |
| Message contract | `MessageContractTests`, and Spectral in the `contracts` job | a message type, topic or property in code is not in `asyncapi.yaml`, or the contract fails the AsyncAPI ruleset |
| Tests | `tests` job | any non-wip test fails |
| Coverage | `tests` job | line coverage of the merged report is below `MIN_LINE_COVERAGE` (90); the composition root and migrations are not measured, see `coverage.runsettings` |
| Stale wip | `stale-wip` workflow, nightly | a wip marker is older than `WIP_MAX_DAYS` (14) |
| Mutation | `mutation` workflow, weekly | the mutation score over both test projects is below `thresholds.break` in `stryker-config.json` |
| Secrets | `secrets` job | gitleaks finds a credential |
| Container | `docker` job on `main` | the image does not build, start, answer `/health` and accept an order |

## 8. Running the checks locally

```sh
dotnet tool restore
dotnet csharpier check .                                # formatting (format . to fix)
dotnet build -c Release                                 # analyzers, NsDepCop, banned APIs; writes spec.json
dotnet test --filter "Category!=wip"                    # everything CI runs, Docker required
dotnet test --filter "Category=wip"                     # the work in progress; expected to fail
dotnet test --filter "FullyQualifiedName~Guardrails"    # traceability and architecture only, no Docker
npm install && npm run lint:contracts                   # Spectral over both contracts, pinned in package.json
docker run --rm -v "$PWD:/w:ro" tufin/oasdiff:v1.32.1 breaking /w/spec.json /w/docs/specs/api/openapi.yaml \
  --fail-on WARN --severity-levels /w/.oasdiff-severity.txt
UPDATE_SCHEMA=1 dotnet test --filter "FullyQualifiedName~DatabaseSchema"   # regenerate schema.sql
dotnet stryker                                          # mutation report over both test projects, Docker required
gitleaks git --pre-commit --staged                      # what the pre-commit hook runs
```

## 9. Who enforces what

Husky owns the commit, Claude hooks own the session, CI owns the truth.

| Layer | Runs for | Does | Can be skipped |
|---|---|---|---|
| Claude hooks (`.claude/settings.json`, `.claude/hooks/`) | a Claude session only | blocks an implementation session editing a protected path (anything but deleting a wip marker on a non-`spec/` branch); blocks `--no-verify` and commits on the default branch | by any other tool, so never the only enforcement |
| Husky (`.husky/`) | anyone who commits | formats staged files, scans them for secrets, checks the commit message | with `--no-verify`, which the Claude hook blocks |
| CI (`.github/workflows/`) | every push and PR | everything in section 7 | no |

One owner per action: Husky formats, so the Claude hooks do not; the Claude hooks block or warn, never fix. A rule that
matters exists in CI; the other two layers only shorten the loop.
