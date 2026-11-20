"""CA-T4a: compare the dry contract of PB-T4a with the v4.2 package and with the PB-T3c contract (read-only).

Usage: python -I cat4a_compare.py <dry.json> <pbt3c.json> <package.md> <lstree.txt> <lstree-root.txt> <out.txt>
"""
import json
import sys

dry_p, t3c_p, pkg_p, tree_p, root_p, out_p = sys.argv[1:7]
dry = json.loads(open(dry_p, "rb").read().decode("utf-8"))
t3c = json.loads(open(t3c_p, "rb").read().decode("utf-8"))
pkg = open(pkg_p, "rb").read().decode("utf-8")
tree = [l for l in open(tree_p, encoding="utf-8").read().splitlines() if l]
root = [l for l in open(root_p, encoding="utf-8").read().splitlines() if l]
L = []
w = L.append

# ---------------------------------------------------------------- the ASCII block of 6.1.1
start = pkg.index("```text\nTaskId: F4B-PBT4-FULL-READ-PATH")
end = pkg.index("\n```", start + 8)
block = pkg[start + len("```text\n"):end]
w("block ascii=" + str(all(ord(ch) < 128 for ch in block)) + " len=" + str(len(block)))
parts = {}
cur = None
for line in block.split("\n"):
    if line.startswith("TaskId: "):
        parts["TaskId"] = line[len("TaskId: "):]
        continue
    if line in ("Objective:", "Invariants:", "StopConditions (propias):", "ExpectedEvidence:"):
        cur = line.rstrip(":").split(" ")[0]
        parts[cur] = []
        continue
    if cur and line.strip():
        parts[cur].append(line)

w("TaskId equal=" + str(parts["TaskId"] == dry["TaskId"]) + " len=" + str(len(dry["TaskId"])))
obj = "\n".join(parts["Objective"])
w("Objective equal=" + str(obj == dry["Objective"]))
if obj != dry["Objective"]:
    w("  pkg: " + obj)
    w("  dry: " + dry["Objective"])

inv = parts["Invariants"]
w("Invariants pkg=%d dry=%d" % (len(inv), len(dry["Invariants"])))
for i, (a, b) in enumerate(zip(inv, dry["Invariants"])):
    if a != b:
        w("  INV DIFF %d\n    pkg: %s\n    dry: %s" % (i, a, b))

stops_pkg = {}
for line in parts["StopConditions"]:
    k, _, v = line.partition(": ")
    stops_pkg[k] = v
own = [s for s in dry["StopConditions"] if s["Id"].startswith("PBT4A-")]
std = [s for s in dry["StopConditions"] if not s["Id"].startswith("PBT4A-")]
w("Stops dry=%d own=%d std=%d pkg-own=%d" % (len(dry["StopConditions"]), len(own), len(std), len(stops_pkg)))
for s in own:
    if stops_pkg.get(s["Id"]) != s["Text"]:
        w("  STOP DIFF " + s["Id"])
        w("    pkg: " + str(stops_pkg.get(s["Id"])))
        w("    dry: " + s["Text"])
w("  own ids: " + " ".join(s["Id"] for s in own))
t3c_std = [s for s in t3c["StopConditions"] if not s["Id"].startswith("PBT3C-")]
w("  std equal to PB-T3c std: " + str(std == t3c_std) + " (t3c std %d)" % len(t3c_std))
t3c_own = {s["Id"]: s["Text"] for s in t3c["StopConditions"] if s["Id"].startswith("PBT3C-")}
for n in ("C08", "C11", "C12", "C17", "C18", "C07", "C02", "C16"):
    a = t3c_own.get("PBT3C-" + n)
    b = next((s["Text"] for s in own if s["Id"] == "PBT4A-" + n), None)
    if a is None or b is None:
        w("  %s: t3c=%s t4a=%s" % (n, a is not None, b is not None))
        continue
    if n == "C11":
        a2 = a.replace("PBT3C", "PBT4").replace("PB-T3c", "PB-T4a")
        w("  C11 == t3c with PBT3C->PBT4, PB-T3c->PB-T4a: " + str(a2 == b))
        if a2 != b:
            import difflib
            for d in difflib.ndiff([a2], [b]):
                w("    " + d[:2000])
    else:
        w("  %s equal to t3c verbatim: %s" % (n, a == b))
        if a != b:
            a2 = a.replace("PBT3C", "PBT4A").replace("PB-T3c", "PB-T4a")
            w("    equal after rename: %s" % (a2 == b))

ev = parts["ExpectedEvidence"]
w("Evidence pkg=%d dry=%d" % (len(ev), len(dry["ExpectedEvidence"])))
for i, (a, b) in enumerate(zip(ev, dry["ExpectedEvidence"])):
    a2 = a.split("] ", 1)[1] if a.startswith("[") else a
    if a2 != b:
        w("  EVID DIFF %d\n    pkg: %s\n    dry: %s" % (i, a2, b))
w("  EVID[0] equal to t3c EVID[0]: " + str(dry["ExpectedEvidence"][0] == t3c["ExpectedEvidence"][0]))
w("  t3c evidence count %d" % len(t3c["ExpectedEvidence"]))

# ---------------------------------------------------------------- RequiredTests
t3c_rt = t3c["RequiredTests"]
w("RequiredTests dry=%d t3c=%d" % (len(dry["RequiredTests"]), len(t3c_rt)))
new = dry["RequiredTests"][:2]
w("  new: " + json.dumps(new))
rest = dry["RequiredTests"][2:]
for a, b in zip(t3c_rt, rest):
    diffs = [k for k in ("Project", "Filter", "MinSelected", "ExpectRed") if a.get(k) != b.get(k)]
    if diffs:
        w("  RT %s: %s" % (b["Filter"], ", ".join("%s %s->%s" % (k, a.get(k), b.get(k)) for k in diffs)))
w("  same order and filters: " + str([a["Filter"] for a in t3c_rt] == [b["Filter"] for b in rest]))
w("  extra keys t3c: " + str(sorted(set(k for r in t3c_rt for k in r))) + " dry: " + str(sorted(set(k for r in dry["RequiredTests"] for k in r))))
# package table
import re
rows = re.findall(r"^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| \**(\d+)\** \| (true|false) \|$", pkg, re.M)
w("  package table rows: %d" % len(rows))
ok = True
for (n, proj, filt, mins, er), r in zip(rows, dry["RequiredTests"]):
    if (proj, filt, int(mins), er == "true") != (r["Project"], r["Filter"], r["MinSelected"], r["ExpectRed"]):
        ok = False
        w("  TABLE DIFF row %s: %s" % (n, (proj, filt, mins, er)))
w("  package table == dry: " + str(ok and len(rows) == len(dry["RequiredTests"])))

# ---------------------------------------------------------------- scopes
allowed = dry["AllowedWriteScope"]
pkg_allowed = re.findall(r"^  - `((?:src|tests)/[^`]+)`", pkg[pkg.index("### 6.1 PB-T4a"):pkg.index("#### 6.1.1")], re.M)
pkg_allowed += re.findall(r"`(tests/RackCad\.Tests/PushBackFullRead\w+\.cs)`", pkg[pkg.index("### 6.1 PB-T4a"):pkg.index("#### 6.1.1")])
w("Allowed dry=%d order: %s" % (len(allowed), allowed))
w("  package 6.1 mentions (in order, deduped): %s" % list(dict.fromkeys(pkg_allowed)))

forb = dry["ForbiddenWriteScope"]
t3c_forb = t3c["ForbiddenWriteScope"]
w("Forbidden dry=%d t3c=%d" % (len(forb), len(t3c_forb)))
w("  dry dupes: %d" % (len(forb) - len(set(forb))))
w("  in t3c not in dry: %s" % [x for x in t3c_forb if x not in forb])
w("  in dry not in t3c: %s" % [x for x in forb if x not in t3c_forb])
w("  t3c allowed: %s" % t3c["AllowedWriteScope"])
# overlap allowed vs forbidden (prefix)
clash = [(a, f) for a in allowed for f in forb if a == f or (f.endswith("/") and a.startswith(f))]
w("  allowed covered by forbidden: %s" % clash)

# recompute the src part by the expansion rule: expand every folder that contains an allowed path; list the rest
def children(prefix):
    seen = []
    for p in tree:
        if p.startswith(prefix):
            rest_ = p[len(prefix):]
            head = rest_.split("/", 1)
            item = prefix + head[0] + ("/" if len(head) > 1 else "")
            if item not in seen:
                seen.append(item)
    return seen

src_allowed = [a for a in allowed if a.startswith("src/")]
expand = set()
for a in src_allowed:
    parts_ = a.split("/")
    for i in range(1, len(parts_)):
        expand.add("/".join(parts_[:i]) + "/")

def walk(prefix):
    out = []
    for ch in children(prefix):
        if ch in expand:
            out.extend(walk(ch))
        elif ch in allowed:
            continue
        else:
            out.append(ch)
    return out

src_expected = walk("src/")
src_dry = [x for x in forb if x.startswith("src/")]
w("  src expected=%d dry=%d sameset=%s sameorder=%s" % (len(src_expected), len(src_dry), set(src_expected) == set(src_dry), src_expected == src_dry))
w("    expected-not-dry: %s" % sorted(set(src_expected) - set(src_dry)))
w("    dry-not-expected: %s" % sorted(set(src_dry) - set(src_expected)))
tests_dry = [x for x in forb if x.startswith("tests/")]
tests_t3c = [x for x in t3c_forb if x.startswith("tests/")]
w("  tests dry=%d t3c=%d; t3c-minus-allowed-equal=%s" % (len(tests_dry), len(tests_t3c), [x for x in tests_t3c if x not in allowed] == tests_dry))
missing_files = [x for x in tests_dry if not x.endswith("/") and x not in tree]
w("  tests forbidden entries that do not exist at the revision: %s" % missing_files)
rootf = [x for x in forb if not x.startswith("src/") and not x.startswith("tests/")]
w("  root dry=%d: %s" % (len(rootf), rootf))
w("  repo root entries: %s" % root)

# ---------------------------------------------------------------- other fields
for k in ("EligibleCells", "RoutingEnforcement", "CorrectionsAuthorized", "Schema", "Unit", "Gate", "MainSha"):
    w("%s equal to t3c: %s" % (k, dry.get(k) == t3c.get(k)))
w("t3c MainSha=%s AuthorityRevision=%s; dry MainSha=%s AuthorityRevision=%s" % (t3c.get("MainSha"), t3c.get("AuthorityRevision"), dry.get("MainSha"), dry.get("AuthorityRevision")))

# authorities
ap = [a["Path"] for a in dry["Authorities"]]
tp = [a["Path"] for a in t3c["Authorities"]]
w("Authorities dry=%d t3c=%d dupes=%d" % (len(ap), len(tp), len(ap) - len(set(ap))))
w("  in dry not in t3c: %s" % [p for p in ap if p not in tp])
w("  in t3c not in dry: %s" % [p for p in tp if p not in ap])
cls = {}
for a in dry["Authorities"]:
    cls[a["Class"]] = cls.get(a["Class"], 0) + 1
w("  classes: %s" % cls)
uc = [a["Path"] for a in dry["Authorities"] if a["Class"] == "UNIT_CHANGE"]
w("  UNIT_CHANGE: %s" % uc)
not_in_tree = [p for p in ap if p not in tree]
w("  authority paths not in the revision: %s" % not_in_tree)
for a in t3c["Authorities"]:
    if a["Path"] == "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md":
        w("  t3c f4b-dispositions section: " + a["Section"])
    if a["Class"] == "UNIT_CHANGE":
        w("  t3c UNIT_CHANGE: " + a["Path"])
w("IssuedBy dry: " + dry["IssuedBy"])
w("IssuedBy t3c: " + t3c["IssuedBy"])
open(out_p, "w", encoding="ascii", errors="backslashreplace").write("\n".join(L) + "\n")
print("ok", len(L))
