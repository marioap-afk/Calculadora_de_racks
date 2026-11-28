"""CA-T4a: which string literals of the given test files match an ADDED or CHANGED line of the GREEN of PB-T4a (read-only).

Usage: python -I cat4a_added_lines.py <repo> <rev> <files.txt> <out.txt>
The added lines: difflib between the base and the GREEN of green_pbt4a.py for WHC, WEI, NH and RIA, plus every line of the two
new files. Each literal (>= 4 letters, or a regex with a metacharacter) is tried as a regex and as a substring on each line.
"""
import difflib
import importlib.util
import re
import subprocess
import sys

repo, rev, lst, out = sys.argv[1:5]
here = __file__.rsplit("cat4a_added_lines.py", 1)[0]
spec = importlib.util.spec_from_file_location("green_pbt4a", here + "green_pbt4a.py")
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green = gp.green(repo, rev)
added = []
for p in (gp.WHC_P, gp.WEI_P, gp.NH_P, gp.RIA_P):
    for l in difflib.ndiff(base[p].split("\n"), green[p].split("\n")):
        if l.startswith("+ ") and l[2:].strip():
            added.append((p.rsplit("/", 1)[1], l[2:]))
for p in (gp.IFACE_P, gp.DRAFT_P):
    for l in green[p].split("\n"):
        if l.strip():
            added.append((p.rsplit("/", 1)[1], l))
LIT = re.compile(r'@"(?:[^"]|"")*"|\$"(?:\\.|[^"\\\n])*"|"(?:\\.|[^"\\\n])*"')
ESC = {"\\\\": "\\", '\\"': '"', "\\n": "\n", "\\r": "\r", "\\t": "\t", "\\0": "\0", "\\'": "'"}
def decode(l):
    if l.startswith('@"'):
        return l[2:-1].replace('""', '"')
    inner = l[l.index('"') + 1:-1]
    inner = re.sub(r"\\[\\\"nrt0']", lambda m: ESC[m.group(0)], inner)
    if l.startswith("$"):
        inner = re.sub(r"\{[^{}]*\}", "", inner)
    return inner
paths = [l.strip() for l in open(lst, encoding="utf-8") if l.strip().endswith(".cs")]
batch = subprocess.run(["git", "-C", repo, "cat-file", "--batch"], input="".join("%s:%s\n" % (rev, p) for p in paths).encode(), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b"\n", pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode("utf-8", "replace")
    pos = nl + 1 + size + 1
L = ["added lines: %d" % len(added)]
for p in paths:
    lits = sorted({decode(l) for l in LIT.findall(texts[p])} - {""})
    hits = []
    for lit in lits:
        letters = len(re.findall(r"[A-Za-z]", lit))
        if letters < 4:
            continue
        try:
            rx = re.compile(lit)
        except re.error:
            rx = None
        for f, line in added:
            ok = (rx is not None and rx.search(line)) or lit in line
            if ok:
                hits.append("%r ~ %s: %s" % (lit[:70], f, line.strip()[:90]))
                break
    if hits:
        L.append("== %s (%d)" % (p, len(hits)))
        L.extend("   " + h for h in hits[:12])
open(out, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("ok")
