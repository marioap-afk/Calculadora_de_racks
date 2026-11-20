"""Read-only sweep: the string literals of the test guards against the identifiers the package gives to each new or touched file.

Usage: python -I guard_sweep.py <main checkout> <rev> <package.md> <out.txt>
It only calls `git ls-tree` and `git show` and writes the report to <out.txt>.
"""
import re
import subprocess
import sys

repo, rev, package, out_path = sys.argv[1:5]


def git(*args):
    return subprocess.run(["git", "-C", repo] + list(args), capture_output=True, check=True).stdout.decode("utf-8", "replace")


with open(package, encoding="utf-8") as f:
    lines = f.read().split("\n")


def section(start_heading, end_heading):
    a = next(i for i, l in enumerate(lines) if l.startswith(start_heading))
    b = next(i for i, l in enumerate(lines) if i > a and l.startswith(end_heading))
    return "\n".join(lines[a:b])


def code_corpus(text):
    """The code blocks and the backticked spans of a design section: identifiers and code, not prose."""
    parts = re.findall(r"```(?:csharp)?\n(.*?)```", text, re.S)
    parts += re.findall(r"`([^`\n]+)`", text)
    return "\n".join(parts)


# The file -> the section of the package that describes it (the closed API, the skeleton and the prose identifiers).
SECTIONS = {
    "Draft/PushBackFullDraftState.cs + Comparer": ("### D-PBT3-02", "### D-PBT3-03"),
    "Draft/PushBackEditableStateSeat.cs + PushBackFullDraftMaterializer.cs": ("### D-PBT3-03", "### D-PBT3-04"),
    "Draft/PushBackFullDraftReader.cs": ("### D-PBT3-04", "### D-PBT3-05"),
    "Draft/PushBackFullComposition.cs": ("### D-PBT3-05", "### D-PBT3-06"),
    "Draft/PushBackViewDisposition.cs": ("### D-PBT3-06", "### D-PBT3-07"),
    "Draft/PushBackConflictClassifier.cs": ("### D-PBT3-07", "### D-PBT3-08"),
    "Pilot/PP + UR (lo nuevo de PB-T3b)": ("### D-PBT3-09.", "### D-PBT3-11"),
    "Plugin/WR + RackViewBlockEraser.cs (PB-T3c)": ("### D-PBT3-11", "### D-PBT3-12"),
}
corpora = {name: code_corpus(section(a, b)) for name, (a, b) in SECTIONS.items()}

# The guards: test files that read product source.
BROAD = re.compile(r"Src\.AllProduct\(|Src\.Files\(|GetFiles\(|EnumerateFiles\(|Src\.Code\(|ReadAllText\([^)]*src")
files = [p for p in git("ls-tree", "-r", "--name-only", rev, "tests").split("\n") if p.endswith(".cs")]
VERBATIM = re.compile(r'@"((?:[^"]|"")*)"')
REGULAR = re.compile(r'(?<!@)"((?:[^"\\\n]|\\.)*)"')

report = []
scanned = 0
for path in files:
    text = git("show", rev + ":" + path)
    if not BROAD.search(text):
        continue
    scanned += 1
    for number, line in enumerate(text.split("\n"), 1):
        literals = [m.replace('""', '"') for m in VERBATIM.findall(line)]
        literals += [bytes(m, "utf-8").decode("unicode_escape", "replace") for m in REGULAR.findall(line.replace('@"', '@ "'))
                     if not line.strip().startswith("//")]
        for lit in literals:
            if len(lit) < 4 or lit.startswith(("src/", "tests/", "RackCad.", "docs/")) or " " in lit.strip() and "\\" not in lit:
                continue
            try:
                pattern = re.compile(lit.replace("(?<", "(?P<").replace("(?P<=", "(?<=").replace("(?P<!", "(?<!"))
            except re.error:
                pattern = None
            for name, corpus in corpora.items():
                hit = pattern.search(corpus) if pattern else (lit in corpus)
                if hit:
                    value = hit.group(0) if pattern else lit
                    if len(value.strip()) >= 3:
                        report.append("%s:%d | %s | %s | %s" % (path, number, name, lit[:120], value[:60]))

with open(out_path, "w", encoding="utf-8") as f:
    f.write("scanned guard files: %d\n" % scanned)
    f.write("\n".join(report))
print("scanned", scanned, "hits", len(report))
