"""CA-T4b recheck: pbt4b-precondition6.json of the v4.9 against the one of the v4.8, key by key (read-only).

Usage: python -I ir4_json49.py <json-v48> <json-v49> <out.txt>
"""
import json
import sys

a_p, b_p, out_p = sys.argv[1:4]
a = json.load(open(a_p, encoding="utf-8"))
b = json.load(open(b_p, encoding="utf-8"))
L = ["keys v4.8: %s" % sorted(a), "keys v4.9: %s" % sorted(b)]
for k in sorted(set(a) | set(b)):
    if k == "CopySha":
        diff = sorted(p for p in set(a[k]) | set(b[k]) if a[k].get(p) != b[k].get(p))
        L.append("CopySha: %d and %d copies; different: %s" % (len(a[k]), len(b[k]), diff))
        for p in diff:
            L.append("   %s: %s -> %s" % (p, a[k].get(p, "")[:12], b[k].get(p, "")[:12]))
    elif k == "MinSelected":
        diff = sorted(p for p in set(a[k]) | set(b[k]) if a[k].get(p) != b[k].get(p))
        L.append("MinSelected: %d and %d classes (with tests: %d and %d); different: %s" % (
            len(a[k]), len(b[k]), sum(1 for v in a[k].values() if v), sum(1 for v in b[k].values() if v), diff))
    elif k in a and k in b:
        L.append("%s: %s" % (k, "same" if a[k] == b[k] else "DIFFERENT %r -> %r" % (a[k], b[k])))
    else:
        L.append("%s: only in %s" % (k, "v4.8" if k in a else "v4.9"))
if "Cost" in b:
    L.append("Cost of the v4.9:")
    L.append(json.dumps(b["Cost"], ensure_ascii=True, indent=2))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
