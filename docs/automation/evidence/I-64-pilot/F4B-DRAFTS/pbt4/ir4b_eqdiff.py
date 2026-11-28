"""Review of PB-T4b: unified diff between the original test method (git objects at <rev>) and its equivalent in
pbt4b-equivalents-review.txt, for the methods named on the command line (read-only).

Usage: python -I ir4b_eqdiff.py <repo> <rev> <equivalents-review.txt> <out.txt> <method> [<method> ...]
"""
import difflib
import re
import subprocess
import sys

repo, rev, review_p, out_p = sys.argv[1:5]
wanted = sys.argv[5:]
text = open(review_p, encoding="utf-8").read().replace("\r\n", "\n")
blocks = re.split(r"\n?=+\n(?=\S+ :: )", text)
eq = {}
for b in blocks:
    m = re.match(r"(\S+) :: (\S+)\n=+\n", b)
    if m:
        eq[m.group(2)] = (m.group(1), b[m.end():])


def method_text(src, name):
    m = re.search(r"(?m)^([ \t]*\[(?:Fact|Theory)[^\n]*\n(?:[ \t]*\[[^\n]*\n)*)[ \t]*public\s+(?:async\s+\w+|void)\s+" + re.escape(name) + r"\s*\(", src)
    if not m:
        return None
    start = m.start()
    i = src.find("{", m.end())
    depth = 0
    for j in range(i, len(src)):
        if src[j] == "{":
            depth += 1
        elif src[j] == "}":
            depth -= 1
            if depth == 0:
                return src[start:j + 1]


L = []
for name in wanted:
    if name not in eq:
        L.append("### %s: NOT IN THE REVIEW FILE" % name)
        continue
    path, new = eq[name]
    src = subprocess.run(["git", "-C", repo, "show", "%s:%s" % (rev, path)], capture_output=True).stdout.decode("utf-8").replace("\r\n", "\n")
    old = method_text(src, name)
    L.append("### %s (%s)" % (name, path))
    if old is None:
        L.append("   original not found")
        continue
    d = list(difflib.unified_diff(old.strip("\n").split("\n"), new.strip("\n").split("\n"), "original", "equivalente", n=1, lineterm=""))
    L.extend(d if d else ["   (identical)"])
    L.append("")
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("ok", len(L))
