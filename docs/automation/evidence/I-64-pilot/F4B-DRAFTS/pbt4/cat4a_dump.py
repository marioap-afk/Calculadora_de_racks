"""CA-T4a: read-only dump of the dry contract of PB-T4a (contract-dry-pbt4a.json).

Usage: python -I cat4a_dump.py <contract.json> <out.txt>
Prints the structure, the counts and the literal fields needed by the bounded check.
"""
import hashlib
import json
import sys

src, out = sys.argv[1], sys.argv[2]
raw = open(src, "rb").read()
c = json.loads(raw.decode("utf-8"))
lines = []
w = lines.append
w("sha256 " + hashlib.sha256(raw).hexdigest() + " bytes " + str(len(raw)))
w("ascii-only " + str(all(b < 128 for b in raw)))
w("top-level keys: " + ", ".join(c.keys()))
for k, v in c.items():
    if isinstance(v, list):
        w("  %s: list(%d)" % (k, len(v)))
    elif isinstance(v, dict):
        w("  %s: dict(%d) keys=%s" % (k, len(v), list(v.keys())[:12]))
    else:
        s = json.dumps(v, ensure_ascii=True)
        w("  %s: %s" % (k, s if len(s) < 300 else s[:300] + "...(%d)" % len(s)))
w("")
for k, v in c.items():
    if isinstance(v, list):
        w("=== %s (%d)" % (k, len(v)))
        for i, item in enumerate(v):
            s = json.dumps(item, ensure_ascii=True)
            w("[%d] %s" % (i, s))
        w("")
    elif isinstance(v, dict):
        w("=== %s (dict)" % k)
        for kk, vv in v.items():
            s = json.dumps(vv, ensure_ascii=True)
            w("  %s = %s" % (kk, s))
        w("")
open(out, "w", encoding="ascii", errors="backslashreplace").write("\n".join(lines) + "\n")
print("ok", len(lines))
