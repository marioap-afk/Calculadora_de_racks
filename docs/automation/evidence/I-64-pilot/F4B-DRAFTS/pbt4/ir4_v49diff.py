"""CA-T4b recheck: the v4.8 -> v4.9 diff of the package, hunk by hunk, with the fragments deleted and inserted inside each changed line
(word level), so that every change can be matched to a row of V49 (read-only).

Usage: python -I ir4_v49diff.py <package-v48.md> <package-v49.md> <out.txt>
"""
import difflib
import re
import sys

a_p, b_p, out_p = sys.argv[1:4]
a = open(a_p, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
b = open(b_p, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
L = []
sm = difflib.SequenceMatcher(None, a, b, autojunk=False)
for tag, i1, i2, j1, j2 in sm.get_opcodes():
    if tag == "equal":
        continue
    L.append("== %s v4.8:%d-%d -> v4.9:%d-%d" % (tag, i1 + 1, i2, j1 + 1, j2))
    if tag == "replace" and (i2 - i1) == (j2 - j1):
        for x, y in zip(a[i1:i2], b[j1:j2]):
            wa, wb = re.findall(r"\S+|\s+", x), re.findall(r"\S+|\s+", y)
            for t, p1, p2, q1, q2 in difflib.SequenceMatcher(None, wa, wb, autojunk=False).get_opcodes():
                if t == "equal":
                    continue
                da, db = "".join(wa[p1:p2]).strip(), "".join(wb[q1:q2]).strip()
                L.append("   - %s" % da[:400] if da else "   - (nada)")
                L.append("   + %s" % db[:600] if db else "   + (nada)")
    else:
        for x in a[i1:i2]:
            L.append("   < %s" % x[:300])
        for y in b[j1:j2]:
            L.append("   > %s" % y[:300])
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("hunks: %d; lines out: %d" % (sum(1 for l in L if l.startswith("==")), len(L)))
