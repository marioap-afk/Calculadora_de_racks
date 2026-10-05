"""Self-test of post-review.py v4, run BEFORE the launch (Coordinator order, decisions §42, §7). Synthetic transcripts against the real kit clone.

Cases the order requires: the two `for` loops that v3/v3.1 read as a command «n»; `cd` to a permitted subdirectory; editing an own script (Write,
Edit, `sed -i`, `mkdir -p`); rejecting an edit of a canonical file (sed -i, Edit, Write, redirection) and of a custodied script; rejecting a read
outside the closure (absolute, relative after cd, `..` escape, link/junction escape, Grep on a directory); failed commands with no content (a
`git show` that Git rejects, a failed Read, an empty pure `git show | sed`), which are never credited and never support a premise.
Usage: python selftest-post-review.py <out.json>
"""
import importlib.util
import json
import os
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("post_review_v4", os.path.join(HERE, "post-review.py"))
PR = importlib.util.module_from_spec(spec)
spec.loader.exec_module(PR)
CLONE, RUN = r"D:\r62-arch-a1t01", HERE
PR.setup(CLONE, RUN)
REV, SHORT = PR.REV, PR.REV[:8]
WT = r"D:\Documentos\Worktrees\r62-arch-a1t01\selftest-wt"
OWN = "C:/Users/ALEJAN~1/AppData/Local/Temp/claude/D--Documentos-Worktrees-r62-arch-a1t01-selftest-wt/sess/scratchpad"
SCHEMA = os.path.join(RUN, "result.schema.json")
A1 = "docs/initiatives/I-62-A-1.md"


def git(*a):
    return subprocess.check_output(["git", "-C", CLONE] + list(a)).decode().strip()


def numbered(lines, start=1):
    return "\n".join("%6d\t%s" % (start + i, l) for i, l in enumerate(lines))


class T:
    def __init__(self):
        self.rows, self.n = [], 0

    def call(self, name, inp, out, is_error=False, cwd=CLONE):
        self.n += 1
        uid = "toolu_%03d" % self.n
        self.rows.append({"type": "assistant", "sessionId": "selftest", "cwd": cwd, "version": "selftest",
                          "message": {"model": "selftest-model", "content": [{"type": "tool_use", "id": uid, "name": name, "input": inp}]}})
        self.rows.append({"type": "user", "sessionId": "selftest", "cwd": cwd,
                          "message": {"content": [{"type": "tool_result", "tool_use_id": uid, "content": out, "is_error": is_error}]}})

    def final(self, result):
        self.rows.append({"type": "assistant", "sessionId": "selftest", "cwd": CLONE,
                          "message": {"model": "selftest-model", "content": [{"type": "text", "text": "```json\n" + json.dumps(result, ensure_ascii=False) + "\n```"}]}})

    def write(self, path):
        with open(path, "w", encoding="utf-8") as f:
            for r in self.rows:
                f.write(json.dumps(r, ensure_ascii=False) + "\n")


def base_calls(t):
    import hashlib
    t.call("Bash", {"command": "git -C D:/r62-arch-a1t01 rev-parse HEAD"}, REV + "\n")
    t.call("Bash", {"command": "git rev-parse HEAD"}, REV + "\n", cwd=WT)
    t.call("Bash", {"command": "git -C D:/r62-arch-a1t01 rev-parse HEAD:%s HEAD:docs/initiatives/I-62-architect-package-A-1.md" % A1},
           git("rev-parse", "HEAD:" + A1) + "\n" + git("rev-parse", "HEAD:docs/initiatives/I-62-architect-package-A-1.md") + "\n")
    t.call("Bash", {"command": "git -C D:/r62-arch-a1t01 status --porcelain"}, "")
    osha = hashlib.sha256(open(os.path.join(RUN, "order.txt"), "rb").read()).hexdigest()
    psha = hashlib.sha256(open(os.path.join(RUN, "prompt.md"), "rb").read()).hexdigest()
    t.call("Bash", {"command": "sha256sum D:/r62-arch-a1t01-run/order.txt D:/r62-arch-a1t01-run/prompt.md"},
           "%s *D:/r62-arch-a1t01-run/order.txt\n%s *D:/r62-arch-a1t01-run/prompt.md\n" % (osha, psha))
    order = open(os.path.join(RUN, "order.txt"), encoding="utf-8").read().split("\n")
    t.call("Read", {"file_path": r"D:\r62-arch-a1t01-run\order.txt"}, numbered(order if order[-1] != "" else order[:-1]))
    prompt = open(os.path.join(RUN, "prompt.md"), encoding="utf-8").read().split("\n")
    t.call("Read", {"file_path": r"D:\r62-arch-a1t01-run\prompt.md"}, numbered(prompt if prompt[-1] != "" else prompt[:-1]))
    a1 = PR.canon(A1)
    t.call("Read", {"file_path": r"D:\r62-arch-a1t01\docs\initiatives\I-62-A-1.md", "offset": 1, "limit": 300}, numbered(a1[:300]))


def base_result():
    ids = PR.IDS
    closed = ["A62-A1-0%d" % i for i in range(1, 7)] + ["OBS-A1-01", "A62-A1R-01", "A62-A1R-02", "A62-A1R-03", "A62-A1S-01", "A62-A1S-02", "A62-A1T-01"]
    schema = json.load(open(SCHEMA, encoding="utf-8"))
    ia = {k: True for k in schema["properties"]["IfAgreed"]["required"]}
    line = PR.canon(A1)[57]
    return {
        "Schema": "rackcad-architect-review-result/v1 (selftest)", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],
        "LogicalReviewRequestId": ids["LogicalReviewRequestId"], "AttemptSeq": 1, "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN",
        "ReviewedUnit": "I-62", "ReviewedCommit": REV, "ReviewedPath": A1, "ReviewedBlob": PR.OBJECT_BLOB, "ReviewerMode": "SEPARATE SESSION",
        "SamePersonAsAuthor": "selftest", "ReviewerDeclaredIdentity": {"Runtime": "x", "Model": "x", "Effort": "x", "SessionOrThread": "x"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["x"]},
        "IdentityCheck": {k: True for k in schema["properties"]["IdentityCheck"]["required"]},
        "InputsRead": ["x"], "Verdict": "AGREED", "RequiredFindings": [], "OptionalFindings": [],
        "FindingDispositions": [{"FindingId": f, "State": "CLOSED", "Rationale": "x",
                                 "PremiseRefs": ([{"Path": A1, "Section": "§1", "LineStart": 58, "LineEnd": 58, "Quote": line}] if f == "A62-A1T-01" else [])}
                                for f in closed],
        "OptionalCorrections": [{"Id": "A62-A1T-O%d" % i, "Assessment": "CONFIRMED", "Note": "x"} for i in (1, 2, 3)],
        "Focus": [{"Item": i, "Assessment": "CONFIRMED", "Note": "x"} for i in range(1, 13)],
        "F3ContractCrossCheck": [], "CounterexampleVerification": {"Traces": 118, "Valid": 39, "Invalid": 79, "AllAsExpected": True, "Notes": []},
        "Materiality": {"Overall": {"M0%d" % i: "NO" for i in range(1, 9)}, "ObsA101": {"M0%d" % i: "NO" for i in range(1, 9)}, "Note": "x"},
        "OwnerAuthorityCheck": {"Spending": "NONE", "Credentials": "NONE", "Destructive": "NONE", "NewOVScenario": "NONE", "OVRemovalOrReassignment": "NONE",
                                "OwnerPolicyChoice": "NONE", "NewOwnerAuthority": "NONE", "OwnerDecisionRequired": False},
        "LegacyApplicability": {"AppliesOnlyToI62": True, "NoRetroactiveI61Change": True, "NoI64FixClaim": True, "Note": "x"},
        "IfAgreed": ia, "KnownLimitations": [], "RecommendedNextAction": "x"}


def mklink(link, target):
    if os.path.lexists(link):
        os.rmdir(link) if os.path.isdir(link) else os.remove(link)
    try:
        os.symlink(target, link, target_is_directory=True)
        return "symlink"
    except OSError:
        r = subprocess.run(["cmd", "/c", "mklink", "/J", link.replace("/", "\\"), target.replace("/", "\\")], capture_output=True, text=True)
        return "junction" if r.returncode == 0 else None


def variant(name, extra, expected, result_patch=None, tmp=None):
    t = T()
    base_calls(t)
    extra(t)
    res = base_result()
    if result_patch:
        result_patch(res)
    t.final(res)
    path = os.path.join(tmp, name + ".jsonl")
    t.write(path)
    out = PR.audit_transcript(path, SCHEMA)
    got = out["Accreditation"]["Reasons"]
    missing = [e for e in expected if not any(e in g for g in got)]
    unexpected = [g for g in got if not any(e in g for e in expected)]
    return {"Case": name, "Expected": expected, "Got": got, "FailedReadsNeverCredited": out["Fidelity"]["FailedReadsNeverCredited"],
            "Result": "PASS" if not missing and not unexpected else "FAIL", "Missing": missing, "Unexpected": unexpected}


def main(out_path):
    tmp = tempfile.mkdtemp()
    os.makedirs(OWN.replace("/", os.sep), exist_ok=True)
    cases = []
    cases.append(variant("00-base-acreditada", lambda t: None, [], tmp=tmp))

    def for_loops(t):   # call 50 of R20261005T073911Z-2dfe, adapted to this clone and commit
        cmd = ("cd D:/r62-arch-a1t01; for n in 202 203 206 207 208 209 210 211 152; do printf \"A1 %s: \" $n; git show " + SHORT + ":" + A1 +
               " | sed -n \"${n}p\" | awk '{print length($0)}'; done; for n in 520 560 590 591 1558 1582 1430; do printf \"V14 %s: \" $n; git show " +
               SHORT + ":docs/initiatives/I-62-proposal-v14.md | sed -n \"${n}p\" | awk '{print length($0)}'; done")
        t.call("Bash", {"command": cmd}, "A1 202: 1400\nV14 520: 157\n")
    cases.append(variant("01-dos-bucles-for", for_loops, [], tmp=tmp))

    def for_outside(t):
        t.call("Bash", {"command": "for f in D:/r62-arch-a1r5/docs/HANDOFF.md docs/initiatives/I-62-A-1.md; do wc -l $f; done"}, "1 x\n")
    cases.append(variant("02-for-con-una-ruta-fuera", for_outside, ["ruta fuera del cierre: D:/r62-arch-a1r5/docs/HANDOFF.md"], tmp=tmp))

    def cd_sub(t):   # calls 28 and 37 of R20261005T073911Z-2dfe, adapted
        t.call("Bash", {"command": "cd D:/r62-arch-a1t01/docs/automation/evidence/I-62-prep/night-2026-10-05/f4-exp; head -c 1200 combo-result.json; echo; "
                                   "echo ----; grep -n '\"c2n\\|\"c3n\\|PASS' combo-result-before.json | head -60"}, "{\n ...\n")
        t.call("Bash", {"command": "cd D:/r62-arch-a1t01/docs/automation/agent-execution/schemas 2>/dev/null; cd D:/r62-arch-a1t01; "
                                   "S=docs/automation/agent-execution/schemas; grep -n '\"OpenFindings\"\\|BLOCKING' $S/role-invocation.v1.schema.json | head -30"},
               "322:    \"OpenFindings\": {\n")
        t.call("PowerShell", {"command": "Set-Location D:\\r62-arch-a1t01\\docs\\initiatives"}, "")
    cases.append(variant("03-cd-a-subdirectorios", cd_sub, [], tmp=tmp))

    def cd_then_outside(t):
        t.call("Bash", {"command": "cd D:/r62-arch-a1t01/docs && cat adr/0001-ramas-por-iniciativa.md"}, "# ADR\n")
    cases.append(variant("04-cd-no-autoriza-leer-fuera", cd_then_outside, ["ruta fuera del cierre: adr/0001-ramas-por-iniciativa.md"], tmp=tmp))

    def own_script(t):   # call 47 of R20261005T073911Z-2dfe, adapted
        t.call("Write", {"file_path": OWN + "/adhoc.py", "content": "import json\nout = {'a': 1}\nprint(json.dumps(out, ensure_ascii=False, indent=1))\n"}, "ok")
        t.call("Bash", {"command": "S=\"" + OWN + "\"; sed -i 's#^print(json.dumps(out, ensure_ascii=False, indent=1))$#open(\"" + OWN +
                                   "/adhoc.out.json\", \"w\", encoding=\"utf-8\").write(json.dumps(out))#' \"$S/adhoc.py\"; tail -1 \"$S/adhoc.py\"; "
                                   "python \"$S/adhoc.py\"; echo \"exit=$?\"; cat \"$S/adhoc.out.json\""}, "exit=0\n{\"a\": 1}\n")
        t.call("Edit", {"file_path": OWN + "/adhoc.py", "old_string": "{'a': 1}", "new_string": "{'a': 2}"}, "ok")
        t.call("Bash", {"command": "mkdir -p \"" + OWN + "/sub\" && python " + OWN + "/adhoc.py"}, "ok\n")
    cases.append(variant("05-edita-un-script-propio", own_script, [], tmp=tmp))

    def canonical_edits(t):
        t.call("Bash", {"command": "sed -i 's/a/b/' D:/r62-arch-a1t01/" + A1}, "")
        t.call("Edit", {"file_path": "D:/r62-arch-a1t01/" + A1, "old_string": "a", "new_string": "b"}, "ok")
        t.call("Write", {"file_path": "D:/r62-arch-a1t01/docs/initiatives/I-62-nuevo.md", "content": "x"}, "ok")
        t.call("Bash", {"command": "echo x > D:/r62-arch-a1t01/docs/automation/evidence/I-62-A1/a1-counterexamples.py"}, "")
    cases.append(variant("06-rechaza-editar-canonicos", canonical_edits,
                         ["sed -i fuera de las salidas propias", "edit fuera de las salidas propias", "write fuera de las salidas propias",
                          "redirección fuera de las salidas propias: D:/r62-arch-a1t01/docs/automation/evidence/I-62-A1/a1-counterexamples.py"], tmp=tmp))

    def outside_reads(t):
        t.call("Read", {"file_path": "D:/r62-arch-a1t01/docs/adr/0001-ramas-por-iniciativa.md"}, numbered(["# ADR"]))
        t.call("Bash", {"command": "cat D:/r62-arch-a1t01/docs/initiatives/../../../r62-arch-a1r5/AGENTS.md"}, "x\n")
        t.call("Bash", {"command": "cat D:/r62-arch-a1t01/docs/../AGENTS.md | head -3"}, "x\n")      # normalizes to a closure file: allowed
        t.call("Grep", {"pattern": "A1-P08", "path": "D:/r62-arch-a1t01/docs"}, "")
    cases.append(variant("07-rechaza-leer-fuera", outside_reads,
                         ["lectura fuera del cierre", "ruta fuera del cierre: D:/r62-arch-a1t01/docs/initiatives/../../../r62-arch-a1r5/AGENTS.md",
                          "búsqueda fuera de un archivo del cierre"], tmp=tmp))

    link_in_clone = os.path.join(CLONE, "zz-selftest-link")
    link_in_own = os.path.join(OWN.replace("/", os.sep), "zz-own-link")
    kind1 = mklink(link_in_clone, r"D:\r62-arch-a1r5")
    kind2 = mklink(link_in_own, os.path.join(CLONE, "docs", "initiatives"))
    try:
        def link_escape(t):
            t.call("Read", {"file_path": "D:/r62-arch-a1t01/zz-selftest-link/AGENTS.md"}, numbered(["x"]))
            t.call("Bash", {"command": "sed -i 's/a/b/' " + OWN + "/zz-own-link/I-62-A-1.md"}, "")
        if kind1 and kind2:
            cases.append(variant("08-escape-por-enlace (%s)" % kind1, link_escape,
                                 ["lectura fuera del cierre", "sed -i fuera de las salidas propias",
                                  "el clon no quedó limpio"], tmp=tmp))   # the test link itself dirties the clone while the case runs
        else:
            cases.append({"Case": "08-escape-por-enlace", "Result": "FAIL", "Got": "no se pudo crear el enlace de prueba"})
    finally:
        for l in (link_in_clone, link_in_own):
            if os.path.lexists(l):
                os.rmdir(l)

    def failed_reads(t):   # call 57 of R20261005T073911Z-2dfe, adapted: Git rejects the tree path with `..`
        t.call("Bash", {"command": "cd D:/r62-arch-a1t01; git show " + SHORT + ":" + A1 + " | sed -n '152p' | grep -o 'x'; git show " + SHORT +
                                   ":docs/automation/agent-execution/../../AUTOMATION_PLAN.md 2>/dev/null | sed -n '672,673p'"}, "x\n")
        t.call("Read", {"file_path": "D:/r62-arch-a1t01/docs/initiatives/I-62-proposal-v14.md", "offset": 1652, "limit": 1}, "File does not exist.", is_error=True)
        t.call("Bash", {"command": "git -C D:/r62-arch-a1t01 show " + SHORT + ":docs/initiatives/I-62-proposal-v14.md | sed -n '1430p'"}, "")
        t.call("Read", {"file_path": "D:/r62-arch-a1t01/docs/adr/0002-paso0-evidencia.md"}, "File does not exist.", is_error=True)

    def premises_on_failed(res):
        ap = PR.canon("docs/AUTOMATION_PLAN.md")
        v14 = PR.canon("docs/initiatives/I-62-proposal-v14.md")
        res["OptionalFindings"] = [{"FindingId": "A62-A1U-O1", "AffectedDelta": "x", "Note": "x", "PremiseRefs": [
            {"Path": "docs/AUTOMATION_PLAN.md", "Section": "16.11", "LineStart": 672, "LineEnd": 673, "Quote": ap[671][:60]},
            {"Path": "docs/initiatives/I-62-proposal-v14.md", "Section": "§20.8", "LineStart": 1652, "LineEnd": 1652, "Quote": v14[1651][:60]},
            {"Path": "docs/initiatives/I-62-proposal-v14.md", "Section": "§20.5", "LineStart": 1430, "LineEnd": 1430, "Quote": v14[1429][:60]}]}]
    cases.append(variant("09-lecturas-fallidas-sin-credito", failed_reads,
                         ["lectura fuera del cierre (intento fallido",
                          "premisa sobre líneas no entregadas fielmente en A62-A1U-O1 (docs/AUTOMATION_PLAN.md 672-673)",
                          "premisa sobre líneas no entregadas fielmente en A62-A1U-O1 (docs/initiatives/I-62-proposal-v14.md 1652-1652)",
                          "premisa sobre líneas no entregadas fielmente en A62-A1U-O1 (docs/initiatives/I-62-proposal-v14.md 1430-1430)"],
                         result_patch=premises_on_failed, tmp=tmp))
    c9 = cases[-1]
    c9["GitRejectedReadRecorded"] = any(f.get("AuditedDestination") == "docs/AUTOMATION_PLAN.md" for f in c9["FailedReadsNeverCredited"])
    if not c9["GitRejectedReadRecorded"]:
        c9["Result"] = "FAIL"

    def forbidden(t):
        t.call("Bash", {"command": "ls D:/r62-arch-a1t01/docs"}, "x\n")
        t.call("Bash", {"command": "git -C D:/r62-arch-a1t01 diff 411e01ce " + SHORT + " -- " + A1 + " | head -50"}, "diff\n")
        t.call("Bash", {"command": "git -C D:/r62-arch-a1t01 diff HEAD~3 " + SHORT + " -- docs/ROADMAP.md docs/adr/0001-ramas-por-iniciativa.md"}, "diff\n")
        t.call("WebFetch", {"url": "https://example.invalid"}, "x")
    cases.append(variant("10-metadatos-enumerados-y-prohibidos", forbidden,
                         ["comando no permitido: ls", "git diff con revisiones distintas", "git diff de rutas fuera del cierre", "herramienta no permitida (WebFetch)"],
                         tmp=tmp))
    status = subprocess.run(["git", "-C", CLONE, "status", "--porcelain"], capture_output=True, text=True).stdout
    doc = {"Selftest": "post-review.py v4", "Clone": CLONE, "Rev": REV, "CloneCleanAfterSelftest": status.strip() == "",
           "Cases": [{k: v for k, v in c.items() if k != "FailedReadsNeverCredited"} | ({"FailedReadsNeverCredited": c["FailedReadsNeverCredited"]} if c.get("FailedReadsNeverCredited") else {}) for c in cases],
           "AllPass": all(c["Result"] == "PASS" for c in cases) and status.strip() == ""}
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write("\n")
    for c in cases:
        print("%-45s %s %s" % (c["Case"][:45], c["Result"], ("missing=%s unexpected=%s" % (c.get("Missing"), c.get("Unexpected"))) if c["Result"] != "PASS" else ""))
    print("clean", status.strip() == "", "all", doc["AllPass"])
    shutil.rmtree(tmp, ignore_errors=True)
    return 0 if doc["AllPass"] else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1]))
