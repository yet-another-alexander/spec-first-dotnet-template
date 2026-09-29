// Implements the open tasks of a change, one fresh agent per task, each verified by an independent agent.
//
// Run it from a Claude session with:  use the implement-change workflow for EXAMPLE-1-orders
// args: { change: "<folder under docs/specs/changes>", tasks?: [4, 5], maxRepairs?: 1 }
//
// The workflow is the rule of docs/workflow.md section 3 made mechanical: one task per branch and PR, implemented by a
// session that never saw the spec session, and checked by another that never saw the implementer. It stops at the
// first task whose verification fails after the allowed repairs, and reports what is left.

export const meta = {
  name: 'implement-change',
  description: 'Implement the open tasks of a change: a fresh implementer per task, an independent verifier, one repair round',
  whenToUse: 'After a spec PR is merged and tasks.md lists open tasks. Costs an implementer and a verifier per task.',
  phases: [
    { title: 'Scout', detail: 'read tasks.md and list the open tasks' },
    { title: 'Implement', detail: 'one fresh agent per task, in order, on its own branch' },
    { title: 'Verify', detail: 'checks, protected paths, review against the requirements' },
  ],
}

if (!args || typeof args.change !== 'string' || !args.change) {
  throw new Error('args.change is required: the folder name under docs/specs/changes/, e.g. "EXAMPLE-1-orders"')
}
const change = args.change
const maxRepairs = Number.isInteger(args.maxRepairs) ? args.maxRepairs : 1
const only = Array.isArray(args.tasks) ? args.tasks : null

const TASKS = {
  type: 'object',
  required: ['tasks'],
  properties: {
    tasks: {
      type: 'array',
      items: {
        type: 'object',
        required: ['number', 'title', 'open', 'requirementIds', 'wipMarkers'],
        properties: {
          number: { type: 'integer' },
          title: { type: 'string' },
          open: { type: 'boolean' },
          requirementIds: { type: 'array', items: { type: 'string' } },
          wipMarkers: { type: 'array', items: { type: 'string' }, description: 'file:line or file + scenario title of each wip marker the task deletes' },
          files: { type: 'array', items: { type: 'string' } },
        },
      },
    },
  },
}

const IMPLEMENTATION = {
  type: 'object',
  required: ['status', 'branch', 'summary'],
  properties: {
    status: { type: 'string', enum: ['done', 'blocked'] },
    branch: { type: 'string' },
    summary: { type: 'string' },
    filesChanged: { type: 'array', items: { type: 'string' } },
    wipMarkersRemoved: { type: 'array', items: { type: 'string' } },
    blockedBy: { type: 'string', description: 'when blocked: the spec or test problem that must go back to a spec PR' },
    pullRequest: { type: 'string' },
  },
}

const VERDICT = {
  type: 'object',
  required: ['passed', 'failures', 'notes'],
  properties: {
    passed: { type: 'boolean' },
    failures: { type: 'array', items: { type: 'string' }, description: 'each a concrete, reproducible defect: a failing check with its output, or a protected-path change, or a requirement not met' },
    notes: { type: 'array', items: { type: 'string' }, description: 'non-blocking review remarks' },
  },
}

phase('Scout')
const scouted = await agent(
  `Read docs/specs/changes/${change}/tasks.md and docs/specs/changes/${change}/spec.md. Return every task in ` +
    `tasks.md with its number, title, whether it is still open (not marked done), the requirement ids it takes from ` +
    `work in progress to covered, the wip markers it says to delete, and the files it names. Read only; change nothing.`,
  { label: `scout:${change}`, schema: TASKS, effort: 'low' }
)
if (!scouted) throw new Error('The scout returned nothing; is the change folder name right?')

const tasks = scouted.tasks
  .filter(task => task.open)
  .filter(task => !only || only.includes(task.number))
  .sort((a, b) => a.number - b.number)
log(`${tasks.length} open task(s) in ${change}${only ? ` (requested: ${only.join(', ')})` : ''}`)
if (tasks.length === 0) return { change, implemented: [], stoppedAt: null, reason: 'no open tasks' }

const implemented = []
let stoppedAt = null
let reason = null

for (const task of tasks) {
  const branch = `feature/${change.toLowerCase()}-task-${task.number}`
  const taskLabel = `task ${task.number}: ${task.title}`

  phase('Implement')
  let result = await agent(
    `You are a fresh implementation session for change ${change}, task ${task.number} ("${task.title}"). You have not ` +
      `seen the specification session and must not act as one. Follow the /implement procedure in ` +
      `.claude/commands/implement.md exactly, with these fixed inputs: create and work on branch ${branch} from the ` +
      `current HEAD; implement only this task; the tests it names define done. Do not change anything under ` +
      `docs/specs/ or tests/SpecFirst.Service.Tests.Spec/ except deleting these wip markers once their tests pass: ` +
      `${task.wipMarkers.join('; ') || 'none listed'}. Run dotnet csharpier format ., dotnet build -c Release and ` +
      `dotnet test --filter "Category!=wip" before finishing, and commit with a Conventional Commits message. If a ` +
      `test or the spec looks wrong, stop, leave the branch as it is, and return status "blocked" with the reason; ` +
      `never adapt a test to the code. Open a draft PR with gh only if gh is authenticated; otherwise leave the ` +
      `branch and say so. Return the branch, what you changed, and which wip markers you removed.`,
    { label: `implement:${taskLabel}`, phase: 'Implement', schema: IMPLEMENTATION }
  )

  if (!result || result.status === 'blocked') {
    stoppedAt = task.number
    reason = result ? `blocked: ${result.blockedBy || result.summary}` : 'the implementer returned nothing'
    break
  }

  let verdict = null
  for (let attempt = 0; attempt <= maxRepairs; attempt++) {
    phase('Verify')
    verdict = await agent(
      `You verify branch ${result.branch} for change ${change}, task ${task.number} ("${task.title}"). You did not ` +
        `write it. Check, in this order, and stop at the first hard failure only after collecting all check outputs: ` +
        `(1) run dotnet csharpier check ., dotnet build -c Release, dotnet test --filter "Category!=wip"; ` +
        `(2) run git diff main...HEAD -- docs/specs tests/SpecFirst.Service.Tests.Spec and confirm the only lines ` +
        `removed are wip markers (@wip, or the Trait("Category", "wip") line) and nothing was added; ` +
        `(3) confirm the requirements ${task.requirementIds.join(', ')} are now covered: their tests run without a ` +
        `wip marker and pass; (4) read the full diff against the requirements' text in docs/specs/requirements/ and ` +
        `the rules in .claude/rules/, and report anything that violates them or exposes behaviour no requirement ` +
        `asks for (an extra endpoint, field, status code or message is drift). Change nothing. Every failure must ` +
        `be concrete and reproducible: quote the command and the output, or the file and line.`,
      { label: `verify:${taskLabel}${attempt ? ` (after repair ${attempt})` : ''}`, phase: 'Verify', schema: VERDICT }
    )
    if (!verdict) { verdict = { passed: false, failures: ['the verifier returned nothing'], notes: [] }; break }
    if (verdict.passed || attempt === maxRepairs) break

    log(`task ${task.number}: ${verdict.failures.length} failure(s), repair ${attempt + 1}/${maxRepairs}`)
    phase('Implement')
    const repaired = await agent(
      `You are a fresh implementation session repairing branch ${result.branch} for change ${change}, task ` +
        `${task.number} ("${task.title}"). An independent verifier found these failures:\n- ` +
        `${verdict.failures.join('\n- ')}\nFix them under the same rules as .claude/commands/implement.md: only ` +
        `src/ and tests/SpecFirst.Service.Tests.Unit/ may change, plus deleting the task's wip markers; if a failure ` +
        `means the spec or a spec test is wrong, return status "blocked" and say which. Run the checks, commit on ` +
        `the same branch, and return what you changed.`,
      { label: `repair:${taskLabel}`, phase: 'Implement', schema: IMPLEMENTATION }
    )
    if (!repaired || repaired.status === 'blocked') {
      verdict = { passed: false, failures: [repaired ? `blocked: ${repaired.blockedBy || repaired.summary}` : 'the repair agent returned nothing'], notes: verdict.notes }
      break
    }
    result = repaired
  }

  implemented.push({ task: task.number, title: task.title, branch: result.branch, pullRequest: result.pullRequest || null, passed: verdict.passed, failures: verdict.failures, notes: verdict.notes })
  if (!verdict.passed) {
    stoppedAt = task.number
    reason = `verification failed after ${maxRepairs} repair(s): ${verdict.failures[0]}`
    break
  }
  log(`task ${task.number} verified on ${result.branch}`)
}

return { change, implemented, stoppedAt, reason }
