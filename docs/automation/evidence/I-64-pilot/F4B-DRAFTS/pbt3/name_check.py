"""Read-only: every string literal of every test file that reads product source, tried as a regex and as a substring
against the lines where a candidate name appears; reports the literals whose match overlaps the candidate name.

Usage: python -I name_check.py <main checkout> <rev> <name> [<name> ...]
Only calls `git ls-tree` and `git show`. Prints to stdout.
"""
import re
import subprocess
import sys

repo, rev = sys.argv[1:3]
names = sys.argv[3:]


def git(*args):
    return subprocess.run(["git", "-C", repo] + list(args), capture_output=True, check=True).stdout.decode("utf-8", "replace")


samples = []
for name in names:
    samples += [
        "public static PushBackFullWriteComposition " + name + "(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog)",
        "var composition = PushBackFullComposition." + name + "(initial, current, catalog);",
        "if (composition." + name + " == null) return;",
    ]

BROAD = re.compile(r"Src\.AllProduct\(|Src\.Files\(|GetFiles\(|EnumerateFiles\(|Src\.Code\(|ReadAllText\(|Src\.Text\(")
files = [p for p in git("ls-tree", "-r", "--name-only", rev, "tests").split("\n") if p.endswith(".cs")]
VERBATIM = re.compile(r'@"((?:[^"]|"")*)"')
REGULAR = re.compile(r'(?<!@)"((?:[^"\\\n]|\\.)*)"')
scanned = 0
tried = 0
found = []
for path in files:
    text = git("show", rev + ":" + path)
    if not BROAD.search(text):
        continue
    scanned += 1
    for number, line in enumerate(text.split("\n"), 1):
        if line.strip().startswith("//"):
            continue
        literals = [m.replace('""', '"') for m in VERBATIM.findall(line)]
        literals += [bytes(m, "utf-8").decode("unicode_escape", "replace") for m in REGULAR.findall(line.replace('@"', '@ "'))]
        for lit in literals:
            if len(lit) < 2:
                continue
            tried += 1
            try:
                pattern = re.compile(lit.replace("(?<", "(?P<").replace("(?P<=", "(?<=").replace("(?P<!", "(?<!"))
            except re.error:
                pattern = None
            for sample in samples:
                for name in names:
                    start = sample.find(name)
                    if start < 0:
                        continue
                    end = start + len(name)
                    spans = []
                    if pattern is not None:
                        spans += [(m.start(), m.end()) for m in pattern.finditer(sample) if m.end() > m.start()]
                    i = sample.find(lit)
                    while i >= 0:
                        spans.append((i, i + len(lit)))
                        i = sample.find(lit, i + 1)
                    for a, b in spans:
                        if a < end and b > start:
                            found.append("%s:%d | %r | %r" % (path, number, lit[:100], sample[a:b][:60]))
print("scanned", scanned, "literals tried", tried)
for row in sorted(set(found)):
    print(row)
