# Changes

One folder per ticket, named `<TICKET>-<slug>`, whether the change introduces behaviour or not. Technical changes
(a framework upgrade, a migration to a new library) use the same shape with no new requirement ids: everything stays
green.

- `spec.md`: the ticket link, the problem in two sentences, the requirement ids added or changed, the contracts
  touched, what is out of scope and where it went instead. Open questions are written as clarification markers and
  fail CI until answered.
- `plan.md`: the approach, the files, the data changes, the sequencing. No code.
- `tasks.md`: one task per implementation PR, each naming the ids it takes from work in progress to covered.

Requirement text never lives here. This folder names the change; `../requirements/` holds the truth.
