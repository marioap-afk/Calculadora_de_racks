"""Targeted mutation test for the kit of the ONE formal review of amendment A-7 (decisions §93, point 2; decisions §88, points 3-4): the judge of
mutation-post-review.py (each rule site disabled in turn on a copy of its module, the FULL case list of selftest-post-review.py run against the
mutant, KILLED when at least one case fails or raises), applied only to the rule sites whose code changed from the accredited kit of A-6 round 4
(R20261014T180556Z-5e02; mutation-result.json 424c983e, 191/191):
  * post-review.py: every site of coherence() (rewritten for rackcad-a7-architect-review/0) and every site of the gate-11 record rule in audit()
    (the block from `og = checks.get(OG.CHECK)` to the FrozenCommandGuard rule);
  * owner_gate.py: every site (evaluate() changed: object, agreed A-6 blob, base §92);
  * transport_gate.py: none (byte for byte the A-6 round-4 module, e326b5b9; its 40/40 is reused).
The other post-review.py sites are byte for byte those of v5.4-a6r4 (their 133/133 in mutation-result.json 424c983e is reused; the self-test keeps
the cases that killed them). No full battery is run here: decisions §88, point 4, reserves the full repetition for changes in the auditor rules, the
schema and the gates, and decisions §93, point 2, asks to identify the exact clause before running one (README of the kit).
Usage: python -B mutation-targeted.py <out.json>
"""
import hashlib
import importlib.util
import json
import os
import re
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("mutation_post_review", os.path.join(HERE, "mutation-post-review.py"))
MU = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(MU)
ST = MU.ST


def targeted(name, src, found):
    """→ the sites of `found` (from MU.sites) selected by the rule of the docstring."""
    if name == "owner_gate.py":
        return list(found)
    if name != "post-review.py":
        return []
    coh = next(i for i, l in enumerate(src) if l.startswith("def coherence(result, c):"))
    main = next(i for i, l in enumerate(src) if l.startswith('if __name__ == "__main__":'))
    g0 = next(i for i, l in enumerate(src) if l.strip() == "og = checks.get(OG.CHECK) or {}")
    g1 = next(i for i, l in enumerate(src) if l.strip().startswith('fg = preflight.get("FrozenCommandGuard")'))
    return [x for x in found if coh < x[0] < main or g0 < x[0] < g1]


def main(out_path):
    mut_dir = os.path.join(ST.TMPROOT, "mut")
    os.makedirs(mut_dir, exist_ok=True)
    failing, n_cases = MU.judge(ST.load_auditor())
    doc = {"Mutation": "post-review.py v5.5-a7 + owner_gate.py (A-7): mutación DIRIGIDA a los sitios de regla que cambiaron frente al kit acreditado "
                       "de A-6 ronda 4; cada sitio desactivado debe hacer fallar al menos un caso del selftest",
           "Scope": "TARGETED",
           "SelectionRule": "post-review.py: los sitios de coherence() y los de la regla del registro de la compuerta 11 en audit(); owner_gate.py: todos; "
                            "transport_gate.py: ninguno (idéntico)",
           "ReusedFromA6R4": {"MutationResultSha256": "424c983eb68eaaa7052c491feafe98e5045df0d93a0fcc752dcab4cae926cf73",
                              "post-review.py": "133/133 sobre 4f46caa6 (los sitios no dirigidos son byte a byte los de v5.4-a6r4)",
                              "transport_gate.py": "40/40 sobre e326b5b9 (idéntico)"},
           "Environment": "SYNTHETIC" if ST._CLOSURE.get("Synthetic") else "REAL (clon y run del kit)",
           "Baseline": {"Cases": n_cases, "Failing": failing, "AllPass": not failing}, "Modules": {}, "Mutants": []}
    for name in ("post-review.py", "transport_gate.py", "owner_gate.py"):
        src, found = MU.sites(name)
        sel = targeted(name, src, found)
        doc["Modules"][name] = {"Sha256": hashlib.sha256(open(os.path.join(HERE, name), "rb").read()).hexdigest(), "Sites": len(found),
                                "Targeted": len(sel)}
        for k, (i, a, b) in enumerate(sel):
            path = os.path.join(mut_dir, "%s.t%03d.py" % (name.split(".")[0].replace("-", "_"), k))
            open(path, "w", encoding="utf-8", newline="\n").write(MU.mutant_source(src, i, a, b))
            if name == "post-review.py":
                pr = ST.load_auditor(path)
            else:
                pr = ST.load_auditor()
                pr.OG = ST.load("owner_gate_mutant", path)
            try:
                fails, _ = MU.judge(pr)
            except Exception as e:   # the mutant made a case raise: observable difference, recorded as a kill
                fails = ["EXCEPTION %s: %s" % (type(e).__name__, str(e)[:200])]
            doc["Mutants"].append({"Module": name, "Line": i + 1, "Code": src[i].strip()[:150], "Killed": bool(fails), "KilledBy": fails[:4],
                                   "KillingCases": len(fails)})
            print("%-18s %4d %-6s %s" % (name, i + 1, "KILLED" if fails else "ALIVE", (fails[:2] if fails else src[i].strip()[:80])))
            sys.stdout.flush()
            os.remove(path)
    for name in doc["Modules"]:
        doc["Modules"][name]["Killed"] = sum(1 for m in doc["Mutants"] if m["Module"] == name and m["Killed"])
    doc["Survivors"] = [m for m in doc["Mutants"] if not m["Killed"]]
    doc["Totals"] = {"Mutants": len(doc["Mutants"]), "Killed": sum(1 for m in doc["Mutants"] if m["Killed"]),
                     "KilledByException": sum(1 for m in doc["Mutants"] if any(str(k).startswith("EXCEPTION ") for k in m["KilledBy"]))}
    doc["AllKilled"] = doc["Baseline"]["AllPass"] and not doc["Survivors"]
    text = ST.USER_RX.sub("%USERPROFILE%", json.dumps(doc, ensure_ascii=False, indent=1))
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text + "\n")
    print(json.dumps({"Baseline": doc["Baseline"]["AllPass"], "Totals": doc["Totals"], "AllKilled": doc["AllKilled"],
                      "Targeted": {n: m["Targeted"] for n, m in doc["Modules"].items()}}))
    ST.safe_remove_tmproot()
    return 0 if doc["AllKilled"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1]))
