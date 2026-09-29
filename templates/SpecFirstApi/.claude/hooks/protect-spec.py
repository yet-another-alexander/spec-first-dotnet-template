#!/usr/bin/env python3
"""PreToolUse hook: an implementation session may not change the spec.

docs/workflow.md section 3: the specification (docs/specs/**) and the tests that prove it
(tests/SpecFirst.Service.Tests.Spec/**) change in spec PRs only, on a spec/ branch. On any other branch the only
permitted change is deleting a wip marker line. CI enforces the same rule on the pull request; this hook stops the
session before it gets that far. Exit 2 blocks the tool call and shows the message to Claude. The command in
.claude/settings.json only starts this script when the tool call mentions a protected path.
"""
import difflib
import json
import os
import re
import subprocess
import sys

PROTECTED = ("docs/specs/", "tests/SpecFirst.Service.Tests.Spec/")
WIP_LINE = re.compile(r'^\s*(@wip|\[Trait\(\s*"Category"\s*,\s*"wip"\s*\)\])\s*$')


def git(*args):
    try:
        return subprocess.run(["git", *args], capture_output=True, text=True, check=True).stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        return None


def only_wip_lines_removed(before, after):
    diff = difflib.ndiff(before.splitlines(), after.splitlines())
    for line in diff:
        if line.startswith("+ "):
            return False
        if line.startswith("- ") and not WIP_LINE.match(line[2:]):
            return False
    return True


def changes(tool, tool_input):
    """Yield (before, after) pairs for the edit about to happen."""
    path = tool_input.get("file_path", "")
    if tool == "Write":
        before = open(path, encoding="utf-8").read() if os.path.exists(path) else ""
        yield before, tool_input.get("content", "")
    elif tool == "Edit":
        yield tool_input.get("old_string", ""), tool_input.get("new_string", "")
    else:
        for edit in tool_input.get("edits", []):
            yield edit.get("old_string", ""), edit.get("new_string", "")


def main():
    try:
        payload = json.load(sys.stdin)
    except ValueError:
        return 0  # not a tool call we understand: never block on our own bug
    tool_input = payload.get("tool_input", {})
    path = tool_input.get("file_path")
    root = git("rev-parse", "--show-toplevel")
    if not path or not root:
        return 0  # not a git repository: nothing to protect against

    relative = os.path.relpath(os.path.abspath(path), root).replace(os.sep, "/")
    if not relative.startswith(PROTECTED):
        return 0

    branch = git("rev-parse", "--abbrev-ref", "HEAD") or ""
    if branch.startswith("spec/"):
        return 0

    if all(only_wip_lines_removed(before, after) for before, after in changes(payload.get("tool_name"), tool_input)):
        return 0

    sys.stderr.write(
        f"Protected path: {relative} changes in specification PRs only (branch spec/..., label spec), and this "
        f"branch is '{branch}'. An implementation session may only delete a line that is exactly @wip or the "
        f'[Trait("Category", "wip")] line of a fact, once the test passes. If the test or the spec looks wrong, '
        "stop and report it; do not adapt it to the code. See docs/workflow.md section 3.\n"
    )
    return 2


if __name__ == "__main__":
    sys.exit(main())
