"""I-64 Smoke-NAV, RunId R20261110T064058Z-c883: compares HOST-EXTRAS BEFORE and AFTER (run by the session, read-only).

Rules from the Coordinator authorization:
- AssemblyMap: any creation or change of a RackCad mapping attributable to the occurrence -> STOP_COORDINATOR_REVIEW; any other value
  change -> REPORT_REQUIRED; a last-write change alone -> INFORMATIVE.
- Existing RackCad entries (CoreUser copies and the product key) were accepted as historical host state because they are unchanged: any
  difference -> REPORT_REQUIRED; a new or changed RackCad entry under Applications, Loaded or AssemblyMap (autoload registration) ->
  STOP_COORDINATOR_REVIEW.
- NETLOAD dialog history: a change is a normal UI-state update -> INFORMATIVE.
Usage: python -I compare_host_extras.py <before.json> <after.json> <out.json>
Exit code: 0 CLEAN or INFORMATIVE; 4 REPORT_REQUIRED; 6 STOP_COORDINATOR_REVIEW; 2 invalid input.
"""
import json
import sys

RANK = {"CLEAN": 0, "INFORMATIVE": 1, "REPORT_REQUIRED": 2, "STOP_COORDINATOR_REVIEW": 3}


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def values(entry):
    return {(v["Name"], v["Kind"], v["Data"]) for v in (entry.get("Values") or [])}


def index(entries, root):
    return {(root, e["Path"]): e for e in (entries or [])}


def is_registration(path):
    p = path.lower()
    return "\\applications\\" in p or "\\loaded\\" in p or p.endswith("\\assemblymap") or "\\assemblymap\\" in p


def compare_entries(before, after, area, findings):
    for key in sorted(set(before) | set(after)):
        b, a = before.get(key), after.get(key)
        path = key[1]
        dialog = (a or b).get("Why") == "NETLOAD_DIALOG_HISTORY"
        if b is None:
            change = "ADDED"
        elif a is None:
            change = "REMOVED"
        elif values(b) != values(a):
            change = "VALUES_CHANGED"
        elif b.get("LastWriteUtc") != a.get("LastWriteUtc"):
            change = "LASTWRITE_ONLY"
        else:
            continue
        if dialog:
            cls = "INFORMATIVE"
        elif change in ("ADDED", "VALUES_CHANGED") and is_registration(path):
            cls = "STOP_COORDINATOR_REVIEW"
        else:
            cls = "REPORT_REQUIRED"
        findings.append({"Area": area, "Root": key[0], "Path": path, "Change": change, "Class": cls,
                         "LastWriteBefore": (b or {}).get("LastWriteUtc"), "LastWriteAfter": (a or {}).get("LastWriteUtc"),
                         "Added": sorted(values(a or {}) - values(b or {})), "Removed": sorted(values(b or {}) - values(a or {}))})


def main(argv):
    if len(argv) != 4:
        print(__doc__)
        return 2
    b, a = load(argv[1]), load(argv[2])
    if b.get("Schema") != "rackcad-i64-host-extras/v1" or a.get("Schema") != b.get("Schema") or b.get("RunId") != a.get("RunId") \
            or b.get("Phase") != "BEFORE" or a.get("Phase") != "AFTER" or b.get("AcadCount") != 0 or a.get("AcadCount") != 0:
        print("INVALID: esquema, RunId, fases o acad.exe no son los esperados")
        return 2
    findings = []

    # AssemblyMap
    am_b, am_a = b["AssemblyMap"], a["AssemblyMap"]
    vb, va = values(am_b), values(am_a)
    rack_b = {v for v in vb if "rackcad" in (v[0] + v[2]).lower()}
    rack_a = {v for v in va if "rackcad" in (v[0] + v[2]).lower()}
    if am_b.get("Exists") != am_a.get("Exists") or vb != va:
        cls = "STOP_COORDINATOR_REVIEW" if rack_b != rack_a else "REPORT_REQUIRED"
        findings.append({"Area": "AssemblyMap", "Path": am_a.get("Path"), "Change": "VALUES_CHANGED", "Class": cls,
                         "LastWriteBefore": am_b.get("LastWriteUtc"), "LastWriteAfter": am_a.get("LastWriteUtc"),
                         "Added": sorted(va - vb), "Removed": sorted(vb - va)})
    elif am_b.get("LastWriteUtc") != am_a.get("LastWriteUtc"):
        findings.append({"Area": "AssemblyMap", "Path": am_a.get("Path"), "Change": "LASTWRITE_ONLY", "Class": "INFORMATIVE",
                         "LastWriteBefore": am_b.get("LastWriteUtc"), "LastWriteAfter": am_a.get("LastWriteUtc"), "Added": [], "Removed": []})

    # CoreUser copies
    roots_b = {c["Root"]: c for c in b["CoreUser"]}
    roots_a = {c["Root"]: c for c in a["CoreUser"]}
    for root in sorted(set(roots_b) | set(roots_a)):
        cb, ca = roots_b.get(root, {}), roots_a.get(root, {})
        if cb.get("Exists") != ca.get("Exists"):
            findings.append({"Area": "CoreUser", "Root": root, "Path": root, "Change": "EXISTENCE_CHANGED", "Class": "REPORT_REQUIRED",
                             "LastWriteBefore": cb.get("LastWriteUtc"), "LastWriteAfter": ca.get("LastWriteUtc"), "Added": [], "Removed": []})
        compare_entries(index(cb.get("Entries"), root), index(ca.get("Entries"), root), "CoreUser", findings)

    # Active product key
    pr = b["ProductKey"]["Root"]
    compare_entries(index(b["ProductKey"]["Entries"], pr), index(a["ProductKey"]["Entries"], pr), "ProductKey", findings)

    disposition = "CLEAN"
    for f in findings:
        if RANK[f["Class"]] > RANK[disposition]:
            disposition = f["Class"]
    summary = {
        "Schema": "rackcad-i64-host-extras-diff/v1", "RunId": b["RunId"], "Disposition": disposition,
        "AssemblyMap": {"Before": {"Exists": am_b.get("Exists"), "LastWriteUtc": am_b.get("LastWriteUtc"),
                                   "RackCadPluginPresent": am_b.get("RackCadPluginPresent", False), "Values": len(vb)},
                        "After": {"Exists": am_a.get("Exists"), "LastWriteUtc": am_a.get("LastWriteUtc"),
                                  "RackCadPluginPresent": am_a.get("RackCadPluginPresent", False), "Values": len(va)}},
        "CoreUserEntries": {"Before": sum(len(c.get("Entries") or []) for c in b["CoreUser"]),
                            "After": sum(len(c.get("Entries") or []) for c in a["CoreUser"])},
        "ProductKeyEntries": {"Before": len(b["ProductKey"]["Entries"] or []), "After": len(a["ProductKey"]["Entries"] or [])},
        "Findings": findings,
    }
    with open(argv[3], "w", encoding="utf-8", newline="\n") as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print("HOST-EXTRAS DIFF Disposition =", disposition)
    for f in findings:
        print(" ", f["Class"], f["Area"], f["Change"], f["Path"], f.get("LastWriteBefore"), "->", f.get("LastWriteAfter"))
    return {"CLEAN": 0, "INFORMATIVE": 0, "REPORT_REQUIRED": 4, "STOP_COORDINATOR_REVIEW": 6}[disposition]


if __name__ == "__main__":
    sys.exit(main(sys.argv))
