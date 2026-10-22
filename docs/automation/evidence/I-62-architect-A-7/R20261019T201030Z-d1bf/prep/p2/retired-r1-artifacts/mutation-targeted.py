"""Targeted mutation test of the read-fidelity correction (decisions §94, point 5; decisions §88, points 3-4) of the kit of the formal review of
amendment A-7: the judge of mutation-post-review.py (each rule site disabled in turn on a copy of its module; KILLED when at least one case fails
or raises), applied only to the rule sites that changed from the LAUNCHED kit (R20261019T010200Z-4a72; manifest c27db93e; full battery
mutation-result.json ac6a6eba, 181/181, run by the Coordinator on post-review.py 303321f2, transport_gate.py e326b5b9 and owner_gate.py 879d630a):
  * post-review.py (v5.6-a7): every site of the faithful-copies block of audit() (from «# ---------------- faithful copies» to «# ----------------
    run custody») and the direct-read site whose message changed («… ni en su copia fiel»);
  * fidelity.py (new): every site: each `mark(` of the detector (one per class) and each `probs.append(` of verify() and run_files_of();
  * transport_gate.py and owner_gate.py: none (byte for byte the launched modules; their kills in ac6a6eba are reused).
The other post-review.py sites are byte for byte those of v5.5-a7 (their kills in ac6a6eba are reused: the self-test keeps every case that
killed them, and its baseline passes on the auditor under test).
Judge (mutation-post-review.py): a post-review.py mutant, the FULL case list of selftest-post-review.py without L00 (fidelity.verify and layout
memoized by content, MemoFid); a fidelity.py mutant, judge_fidelity (the cases of selftest-fidelity.py first; only if they all pass, the full list
with the auditor's fidelity module replaced by the mutant; one failing case kills).
Every temporary file lives under <kit>/../.tmp/st and is removed at the end. Honours RACKCAD_A7_KITDATA like the self-test.
Usage: python -B mutation-targeted.py <out.json>
"""
import hashlib
import importlib.util
import json
import os
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("mutation_post_review", os.path.join(HERE, "mutation-post-review.py"))
MU = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(MU)
ST, SF = MU.ST, MU.SF
LAUNCHED = {"KitManifestSha256": "c27db93ef6834a3e29bf36bd66f586c75b9281c46508ee2296ba14e86eab6327",
            "MutationResultSha256": "ac6a6ebab163150a949b53478bab12a5e1c81f80cf2ef64dbff4550a3c26e3f6",
            "post-review.py": "303321f265152dc03a6fc7771bb69faed47f5167a80a7f03113e2839305d9653",
            "transport_gate.py": "e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf",
            "owner_gate.py": "879d630a08d9fab2466ff434e158ef1f199887d8eafe555e9922e81f040c36ac"}


def targeted(name, src, found):
    if name == "fidelity.py":
        return list(found)
    if name != "post-review.py":
        return []
    f0 = next(i for i, l in enumerate(src) if l.strip().startswith("# ---------------- faithful copies"))
    f1 = next(i for i, l in enumerate(src) if l.strip() == "# ---------------- run custody")
    dr = [i for i, l in enumerate(src) if "ni en su copia fiel" in l and "add(" in l]
    assert len(dr) == 1, dr
    return [x for x in found if f0 < x[0] < f1 or x[0] == dr[0]]


def main(out_path):
    mut_dir = os.path.join(ST.TMPROOT, "mut")
    os.makedirs(mut_dir, exist_ok=True)
    failing, n_cases = MU.judge(ST.load_auditor())
    fid_cases = SF.cases(SF.load())
    fid_failing = [c["Case"] for c in fid_cases if c["Result"] != "PASS"]
    doc = {"Mutation": "corrección de fidelidad de lectura (decisiones §94, punto 5): post-review.py v5.6-a7 y fidelity.py; mutación DIRIGIDA a los "
                       "sitios de regla que cambiaron frente al kit lanzado de A-7 (R20261019T010200Z-4a72); cada sitio desactivado debe hacer "
                       "fallar al menos un caso",
           "Scope": "TARGETED",
           "SelectionRule": "post-review.py: los sitios del bloque de copias fieles de audit() y el de la lectura directa cuyo mensaje cambió; "
                            "fidelity.py: todos (mark( del detector y probs.append( de verify y run_files_of); transport_gate.py y owner_gate.py: "
                            "ninguno (idénticos al kit lanzado)",
           "ReusedFromLaunchedKit": dict(LAUNCHED, Note="batería COMPLETA del Coordinator sobre el kit lanzado (decisiones §88, punto 4), 181/181: "
                                                      "los sitios no dirigidos de post-review.py y todos los de transport_gate.py y owner_gate.py "
                                                      "son byte a byte los de esa batería"),
           "Environment": "SYNTHETIC" if ST._CLOSURE.get("Synthetic") else "REAL-CONTENT (clon y run de RACKCAD_A7_KITDATA o del kit)",
           "KitData": ST.USER_RX.sub("%USERPROFILE%", ST.KITDATA),
           "Baseline": {"Cases": n_cases, "Failing": failing, "FidelityUnitCases": len(fid_cases), "FidelityUnitFailing": fid_failing,
                        "AllPass": not failing and not fid_failing},
           "Modules": {}, "Mutants": []}
    for name in ("post-review.py", "fidelity.py", "transport_gate.py", "owner_gate.py"):
        src, found = MU.sites(name)
        sel = targeted(name, src, found)
        doc["Modules"][name] = {"Sha256": hashlib.sha256(open(os.path.join(HERE, name), "rb").read()).hexdigest(), "Sites": len(found),
                                "Targeted": len(sel)}
        for k, (i, a, b) in enumerate(sel):
            path = os.path.join(mut_dir, "%s.t%03d.py" % (name.split(".")[0].replace("-", "_"), k))
            open(path, "w", encoding="utf-8", newline="\n").write(MU.mutant_source(src, i, a, b))
            stage = "selftest-post-review"
            try:
                if name == "post-review.py":
                    fails, _ = MU.judge(ST.load_auditor(path))
                else:
                    fails, stage = MU.judge_fidelity(path)
            except Exception as e:   # the mutant made a case raise: observable difference, recorded as a kill
                fails = ["EXCEPTION %s: %s" % (type(e).__name__, str(e)[:200])]
            doc["Mutants"].append({"Module": name, "Line": i + 1, "Code": src[i].strip()[:150], "Killed": bool(fails), "Stage": stage,
                                   "KilledBy": fails[:4], "KillingCases": len(fails)})
            print("%-18s %4d %-6s %-22s %s" % (name, i + 1, "KILLED" if fails else "ALIVE", stage, (fails[:2] if fails else src[i].strip()[:80])))
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
