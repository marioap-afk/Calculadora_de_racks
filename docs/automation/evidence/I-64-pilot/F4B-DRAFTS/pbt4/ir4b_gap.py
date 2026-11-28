"""Review of PB-T4b: the test files OUTSIDE the walk of PB-T4b (walk_guards_pbt4b.py, 34 classes) that name, as a literal, a folder that
holds a file the GREEN of PB-T4b touches or adds; and which of their literals match a line that the GREEN adds or changes (read-only).

Usage: python -I ir4b_gap.py <repo> <rev> <v44 dir> <walk.tsv> <out.txt>
The GREEN and its base are the dumps of v44/green_b and v44/base; the tests come from `git cat-file --batch` at <rev>.
"""
import difflib
import os
import re
import subprocess
import sys

repo, rev, v44, walk_tsv, out = sys.argv[1:6]
green_root, base_root = os.path.join(v44, "green_b"), os.path.join(v44, "base")
touched = []
for d, _, fs in os.walk(green_root):
    for f in fs:
        touched.append(os.path.relpath(os.path.join(d, f), green_root).replace("\\", "/"))
added = []
for p in touched:
    g = open(os.path.join(green_root, p), encoding="utf-8").read().replace("\r\n", "\n").split("\n")
    bp = os.path.join(base_root, p)
    if os.path.exists(bp):
        b = open(bp, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
        added += [(p, l[2:]) for l in difflib.ndiff(b, g) if l.startswith("+ ") and l[2:].strip()]
    else:
        added += [(p, l) for l in g if l.strip()]
anc = set()
for t in touched:
    parts = t.split("/")
    for i in range(1, len(parts)):
        anc.add("/".join(parts[:i]))
names = {t.rsplit("/", 1)[1] for t in touched} | {t.rsplit("/", 1)[1].rsplit(".", 1)[0] for t in touched}
walked = {l.split("\t", 1)[0] for l in open(walk_tsv, encoding="utf-8").read().splitlines()[1:]}
ls = subprocess.run(["git", "-C", repo, "ls-tree", "-r", "--name-only", rev, "--", "tests/"], capture_output=True).stdout.decode().split("\n")
paths = [p for p in ls if p.endswith(".cs") and p not in walked]
batch = subprocess.run(["git", "-C", repo, "cat-file", "--batch"], input="".join("%s:%s\n" % (rev, p) for p in paths).encode(), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b"\n", pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode("utf-8", "replace")
    pos = nl + 1 + size + 1
LIT = re.compile(r'@"(?:[^"]|"")*"|"(?:\\.|[^"\\\n])*"')
TEST = re.compile(r"\[(?:Fact|Theory)\b")
L = ["touched/new by the GREEN of PB-T4b: %d files; added or changed lines: %d; walked classes: %d; test files outside: %d"
     % (len(touched), len(added), len(walked), len(paths))]
flagged = 0
for p in paths:
    t = texts[p]
    lits = {m.group(0)[2:-1] if m.group(0).startswith("@") else m.group(0)[1:-1] for m in LIT.finditer(t)}
    norm = {l.replace("\\\\", "/").replace("\\", "/").rstrip("/") for l in lits}
    hits_f = sorted(n for n in norm if n in anc)
    hits_n = sorted(n for n in norm if n in names)
    if not (hits_f or hits_n):
        continue
    flagged += 1
    hits = []
    for lit in sorted(lits):
        if len(re.findall(r"[A-Za-z]", lit)) < 4:
            continue
        try:
            rx = re.compile(lit)
        except re.error:
            rx = None
        for f, line in added:
            if (rx is not None and rx.search(line)) or lit in line:
                hits.append("%r ~ %s: %s" % (lit[:60], f.rsplit("/", 1)[1], line.strip()[:80]))
                break
    L.append("== %s tests=%d folders=%s names=%s matches=%d" % (p, len(TEST.findall(t)), hits_f, hits_n, len(hits)))
    L.extend("   " + h for h in hits[:10])
L.insert(1, "outside files that name a touched folder or file: %d" % flagged)
open(out, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L[:2]))
