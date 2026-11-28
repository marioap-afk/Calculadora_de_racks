"""CA-T4a: the test files OUTSIDE the guard walk of PB-T4a that name, as a string literal, a touched or new file of PB-T4a or one of
its folders (any ancestor), read from the git objects of the revision (read-only).

Usage: python -I cat4a_walk_gap.py <repo> <rev> <outside-list.txt> <out.txt>
Runs `git cat-file --batch` (read-only) and writes only <out.txt>.
"""
import re
import subprocess
import sys

repo, rev, lst, out = sys.argv[1:5]
paths = [l.strip() for l in open(lst, encoding="utf-8") if l.strip().endswith(".cs")]
TOUCHED = [
    "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs",
    "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
    "src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs",
    "src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs",
    "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs",
    "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs",
]
anc = set()
for t in TOUCHED:
    parts = t.split("/")
    for i in range(1, len(parts)):
        anc.add("/".join(parts[:i]))
names = {t.rsplit("/", 1)[1] for t in TOUCHED} | {t.rsplit("/", 1)[1][:-3] for t in TOUCHED}
batch = subprocess.run(["git", "-C", repo, "cat-file", "--batch"], input="".join("%s:%s\n" % (rev, p) for p in paths).encode(),
                       capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b"\n", pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode("utf-8", "replace")
    pos = nl + 1 + size + 1
LIT = re.compile(r'@"(?:[^"]|"")*"|"(?:\\.|[^"\\\n])*"')
TEST = re.compile(r"\[(?:Fact|Theory)\b")
L = []
for p in paths:
    t = texts[p]
    lits = {m.group(0)[2:-1] if m.group(0).startswith("@") else m.group(0)[1:-1] for m in LIT.finditer(t)}
    norm = {l.replace("\\\\", "/").replace("\\", "/").rstrip("/") for l in lits}
    hits_f = sorted(n for n in norm if n in anc)
    hits_n = sorted(n for n in norm if n in names or any(n.endswith("/" + x) for x in names))
    if hits_f or hits_n:
        L.append("%-90s tests=%-4d folders=%s names=%s" % (p, len(TEST.findall(t)), hits_f, hits_n))
open(out, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("files", len(paths), "flagged", len(L))
