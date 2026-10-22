"""Resumen de kit/closure.json (fase 3, toma de relevo): claves, grupos, copias fieles por clase y las que solo marca UNMEASURED_CHAR.
Uso: python -I -B closure_summary.py <closure.json>"""
import collections
import json
import sys

c = json.load(open(sys.argv[1], encoding="utf-8"))
print("keys:", list(c.keys()))
ci = c["CanonicalInputs"]
print("CanonicalInputs:", len(ci), "bytes", sum(r.get("Bytes", 0) for r in ci), "lines", sum(r.get("Lines", 0) for r in ci))
fc = c.get("FaithfulCopies", [])
print("FaithfulCopies:", len(fc))
cls = collections.Counter()
only_unm = []
for e in fc:
    fl = e.get("Flags") or e.get("Marks") or {}
    for k in fl:
        cls[k] += 1
    if list(fl) == ["UNMEASURED_CHAR"]:
        only_unm.append(e.get("Original"))
print("by class:", dict(cls))
print("only UNMEASURED_CHAR:", len(only_unm))
for p in only_unm:
    print("   ", p)
print("FaithfulCopies[0] keys:", list(fc[0].keys()) if fc else None)
print("RunFiles:", len(c.get("RunFiles", [])), c.get("RunFiles", [])[:4], "PremiseRunFiles:", c.get("PremiseRunFiles"))
for k in ("ObjectPath", "ObjectBlob", "Rev", "RunDir", "AcceptorPath", "DirectRead", "DirectReads", "Provisional", "Ids"):
    if k in c:
        print(k, "=", json.dumps(c[k], ensure_ascii=True)[:300])
paths = [r["Path"] for r in ci]
tops = collections.Counter("/".join(p.split("/")[:4]) for p in paths)
for k, v in sorted(tops.items()):
    print("%4d %s" % (v, k))
