import re
import sys

path = sys.argv[1]
with open(path, encoding="utf-8") as f:
    lines = f.read().split("\n")


def cells(line):
    s = line.strip()
    # Drop escaped pipes and pipes inside backticks.
    s = s.replace("\\|", "")
    s = re.sub(r"`[^`]*`", "", s)
    return s.count("|")


expected = None
problems = 0
for number, line in enumerate(lines, 1):
    stripped = line.strip()
    if stripped.startswith("|"):
        count = cells(line)
        if expected is None:
            expected = count
        elif count != expected:
            problems += 1
            print("line", number, "pipes", count, "expected", expected)
    else:
        expected = None

print("problems", problems)

# Local paths: a drive letter, a user profile folder or a hidden tool folder.
for number, line in enumerate(lines, 1):
    if re.search(r"\b[A-Za-z]:[\\/]|[\\/]Users[\\/]|[\\/]\.[a-z]+[\\/]worktrees", line):
        print("path-like", number, line[:120])
