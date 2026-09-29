#!/usr/bin/env python3
"""Requirement hashes, with the same algorithm as the traceability guardrail.

  hash.py REQ-001 TECH-002   print id and hash for the given ids
  hash.py --all              print every requirement with its hash
  hash.py --check            compare the markers in tests/ with the requirements; exit 1 when any is missing or stale
  hash.py --fix              rewrite stale markers in the spec tests (feature tags and [Requirement] attributes)
"""
import hashlib
import os
import re
import sys

REQUIREMENTS_DIR = os.path.join("docs", "specs", "requirements")
TESTS_DIR = "tests"
HEADING = re.compile(r"^###\s+(?P<id>(?:REQ|TECH)-\d{3,})(?P<tags>(?:\s+@[\w-]+)*)\s*$")
FEATURE_TAG = re.compile(r"@(?P<id>(?:REQ|TECH)-\d+)(?::(?P<hash>[0-9a-f]{8}))?(?=\s|$)")
ATTRIBUTE = re.compile(r'\[Requirement\("(?P<id>(?:REQ|TECH)-\d+)",\s*"(?P<hash>[0-9a-f]{8})"\)\]')


def hash_of(text):
    normalised = re.sub(r"\s+", " ", text.strip())
    return hashlib.sha256(normalised.encode("utf-8")).hexdigest()[:8]


def requirements():
    """id -> (hash, location) for every requirement heading, first paragraph after it as the text."""
    found = {}
    for name in sorted(os.listdir(REQUIREMENTS_DIR)):
        if not name.endswith(".md"):
            continue
        path = os.path.join(REQUIREMENTS_DIR, name)
        lines = open(path, encoding="utf-8").read().split("\n")
        for i, line in enumerate(lines):
            heading = HEADING.match(line)
            if not heading:
                continue
            j = i + 1
            while j < len(lines) and not lines[j].strip():
                j += 1
            paragraph = []
            while j < len(lines) and lines[j].strip() and not lines[j].startswith("###"):
                paragraph.append(lines[j])
                j += 1
            found[heading.group("id")] = (hash_of("\n".join(paragraph)), f"{path}:{i + 1}")
    return found


def test_files():
    for root, dirs, files in os.walk(TESTS_DIR):
        dirs[:] = [d for d in dirs if d not in ("bin", "obj")]
        for name in files:
            if name.endswith(".feature") or (name.endswith(".cs") and not name.endswith(".feature.cs")):
                yield os.path.join(root, name)


def markers(path):
    """Yield (line number, id, hash or None, pattern) for every marker in a test file."""
    pattern = FEATURE_TAG if path.endswith(".feature") else ATTRIBUTE
    for number, line in enumerate(open(path, encoding="utf-8").read().split("\n"), start=1):
        for match in pattern.finditer(line):
            yield number, match.group("id"), match.group("hash"), pattern


def check(current):
    problems = []
    for path in test_files():
        for number, rid, digest, _ in markers(path):
            if rid not in current:
                problems.append(f"{path}:{number}: {rid} is not defined in {REQUIREMENTS_DIR}")
            elif digest != current[rid][0]:
                problems.append(f"{path}:{number}: {rid} has {digest or 'no hash'}, expected {current[rid][0]}")
    return problems


def fix(current):
    changed = []
    for path in test_files():
        original = open(path, encoding="utf-8").read()
        pattern = FEATURE_TAG if path.endswith(".feature") else ATTRIBUTE

        def replace(match):
            rid = match.group("id")
            if rid not in current or match.group("hash") == current[rid][0]:
                return match.group(0)
            changed.append(f"{path}: {rid} {match.group('hash') or 'no hash'} -> {current[rid][0]}")
            if pattern is FEATURE_TAG:
                return f"@{rid}:{current[rid][0]}"
            return f'[Requirement("{rid}", "{current[rid][0]}")]'

        updated = pattern.sub(replace, original)
        if updated != original:
            open(path, "w", encoding="utf-8").write(updated)
    return changed


def main(argv):
    if not os.path.isdir(REQUIREMENTS_DIR):
        sys.exit(f"run from the repository root: {REQUIREMENTS_DIR} not found")
    current = requirements()

    if argv == ["--all"]:
        for rid, (digest, location) in current.items():
            print(f"{rid} {digest}  {location}")
        return 0
    if argv == ["--check"]:
        problems = check(current)
        print("\n".join(problems) if problems else "all markers match")
        return 1 if problems else 0
    if argv == ["--fix"]:
        changed = fix(current)
        print("\n".join(changed) if changed else "nothing to fix")
        return 0
    if not argv or any(arg.startswith("-") for arg in argv):
        sys.exit(__doc__)
    missing = [rid for rid in argv if rid not in current]
    if missing:
        sys.exit(f"not defined in {REQUIREMENTS_DIR}: {', '.join(missing)}")
    for rid in argv:
        print(f"{rid} {current[rid][0]}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
