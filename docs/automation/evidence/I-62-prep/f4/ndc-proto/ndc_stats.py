"""I-62 F4 preparation (C-42): NormativeDependencyClosure over the real B.11 manifest of the frozen surfaces, for EVERY unit, to measure how usable the
manifest is before F4 implements premise independence (Proposal V14 §20.3.3, points 5-7).

For each unit of the Proposal: breadth-first closure over the declared edges (visited set; cycles end there), its size, whether it reaches a unit with
Complete = false (→ independence UNKNOWN: such a premise can never be credited under DEGRADED_BOUNDED) and which incomplete units are the most frequent
blockers. Prototype, not production.

Usage: python ndc_stats.py <manifest.json> <out.json>
"""
import collections
import json
import sys

M = json.load(open(sys.argv[1], encoding="utf-8"))
key = lambda u: u["Document"] + "|" + u["UnitId"]
entries = {key(e["Source"]): e for e in M["Entries"]}
complete = {k: e["Complete"] for k, e in entries.items()}


def closure(start):
    seen, queue, incomplete, proposed = {start}, [start], set(), 0
    while queue:
        k = queue.pop(0)
        e = entries.get(k)
        if e is None or not e["Complete"]:
            incomplete.add(k)
        if e is None:
            continue
        for d in e["DependsOn"]:
            t = d["Target"]
            if t["Kind"] == "PROPOSED":
                proposed += 1
                continue
            tk = key(t["Unit"])
            if tk not in seen:
                seen.add(tk)
                queue.append(tk)
    return seen, incomplete, proposed


rows, blockers = [], collections.Counter()
for k, e in entries.items():
    if not e["Source"]["Document"].endswith("I-62-proposal-v14.md"):
        continue
    seen, inc, prop = closure(k)
    rows.append({"Unit": e["Source"]["UnitId"], "Size": len(seen), "Incomplete": len(inc), "Credible": not inc})
    for b in inc:
        blockers[b.split("|", 1)[1] + (" (externa)" if not b.startswith("docs/initiatives/I-62-proposal-v14.md") else "")] += 1
sizes = sorted(r["Size"] for r in rows)
credible = [r for r in rows if r["Credible"]]
q = lambda p: sizes[min(len(sizes) - 1, int(p * len(sizes)))]
ids = {r["Unit"]: r for r in rows}
sample = {u: ids[u] for u in ("C-03", "C-11", "P-15", "I-S15", "T19", "§20.6", "§20.5", "B.8.8") if u in ids}
out = {"Units": len(rows), "CredibleClosures": len(credible), "UnknownClosures": len(rows) - len(credible),
       "ClosureSize": {"min": sizes[0], "p50": q(0.5), "p90": q(0.9), "max": sizes[-1]},
       "TopBlockers": blockers.most_common(15), "Sample": sample}
json.dump(out, open(sys.argv[2], "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1)
print(json.dumps({k: out[k] for k in ("Units", "CredibleClosures", "UnknownClosures", "ClosureSize")}, ensure_ascii=False))
print(json.dumps(out["TopBlockers"][:8], ensure_ascii=False))
print(json.dumps(sample, ensure_ascii=False))
