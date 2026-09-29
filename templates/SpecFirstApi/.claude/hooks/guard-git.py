#!/usr/bin/env python3
"""PreToolUse hook: git commands that dodge the hooks or the workflow.

Husky owns the commit (format, secrets, commit message) and CI owns the truth; this hook only stops an agent from
skipping them. Blocked: --no-verify (or -n on commit), which skips Husky; committing on the default branch, which the
workflow forbids. Exit 2 blocks the tool call and shows the message to Claude. The command in .claude/settings.json
only starts this script when the command mentions git.
"""
import json
import re
import subprocess
import sys

DEFAULT_BRANCHES = {"main", "master"}
NO_VERIFY = re.compile(r"\bgit\b[^|;&]*\b(commit|push|merge)\b[^|;&]*(\s--no-verify\b|\s-n\b)")
COMMIT = re.compile(r"\bgit\b[^|;&]*\bcommit\b")


def branch():
    try:
        return subprocess.run(
            ["git", "rev-parse", "--abbrev-ref", "HEAD"], capture_output=True, text=True, check=True
        ).stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        return ""


def main():
    try:
        command = json.load(sys.stdin).get("tool_input", {}).get("command", "")
    except ValueError:
        return 0  # not a tool call we understand: never block on our own bug

    if NO_VERIFY.search(command):
        sys.stderr.write(
            "--no-verify skips the Husky hooks (formatting, secret scan, commit message). Fix what the hook "
            "reports instead; CI runs the same checks and would reject the commit anyway.\n"
        )
        return 2

    if COMMIT.search(command) and branch() in DEFAULT_BRANCHES:
        sys.stderr.write(
            f"No commits on '{branch()}': create a branch (spec/... for a specification change, feature/... or "
            "bugfix/... for an implementation task) and open a pull request. See .claude/rules/common/git-conventions.md.\n"
        )
        return 2

    return 0


if __name__ == "__main__":
    sys.exit(main())
