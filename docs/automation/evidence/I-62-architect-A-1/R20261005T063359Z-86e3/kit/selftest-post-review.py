"""Self-test of post-review.py v3 with three synthetic transcripts:
  good   — a conforming review with the v2-style actions (→ ACCREDITED);
  good2  — a conforming review that uses the actions added or clarified in v3: cd to the clone, python heredoc over closure files, an own script
           written with Write and with `cat > own <<EOF`, dotnet from Bash through a shell variable, git -C <worktree>, `|| true`, /dev/null, a
           KNOWN long line truncated by Read (→ ACCREDITED, with one TruncatedKnownLongLines record);
  bad    — the typical deviations plus the v3 effect checks (→ NOT_ACCREDITED for exactly the expected reasons).
Usage: python selftest-post-review.py <out dir>"""
import hashlib
import json
import os
import subprocess
import sys

RUN = os.path.dirname(os.path.abspath(__file__))
CLONE = r"D:\r62-arch-a1r3"
OUTDIR = sys.argv[1]
os.makedirs(OUTDIR, exist_ok=True)
CL = json.load(open(os.path.join(RUN, "closure.json"), encoding="utf-8"))
REV, IDS = CL["AuthorityRevision"], CL["Ids"]
R8 = REV[:8]
BLOB = next(r["Blob"] for r in CL["CanonicalInputs"] if r["Path"] == "docs/initiatives/I-62-A-1.md")
WT = r"D:\Documentos\Worktrees\r62-arch-a1r3\test-wt"
WTF = WT.replace("\\", "/")
OWN = "C:/Users/u/AppData/Local/Temp/claude/D--Documentos-Worktrees-r62-arch-a1r3-test-wt/s1/scratchpad"
H = "docs/automation/evidence/I-62-A1/a1-counterexamples.py"


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


def numbered(lines, start=1):
    return "\n".join("%6d\t%s" % (i, l) for i, l in enumerate(lines, start))


def canon(rel):
    return subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")


def runlines(name):
    x = open(os.path.join(RUN, name), encoding="utf-8").read().replace("\r\n", "\n").split("\n")
    return x[:-1] if x[-1] == "" else x


prompt, order, prior = runlines("prompt.md"), runlines("order.txt"), runlines("prior-authorization.txt")
a1 = canon("docs/initiatives/I-62-A-1.md")
v14 = canon("docs/initiatives/I-62-proposal-v14.md")


class T:
    def __init__(self, cwd=WT):
        self.lines, self.k, self.cwd = [], 0, cwd
        self.lines.append({"type": "user", "sessionId": "s1", "cwd": cwd, "version": "test", "message": {"role": "user", "content": "chip"}})

    def call(self, name, inp, out, cwd=None, no_result=False):
        self.k += 1
        uid = "t%d" % self.k
        c = cwd or self.cwd
        self.lines.append({"type": "assistant", "sessionId": "s1", "cwd": c, "message": {"role": "assistant", "model": "claude-test",
                           "content": [{"type": "tool_use", "id": uid, "name": name, "input": inp}]}})
        if not no_result:
            self.lines.append({"type": "user", "sessionId": "s1", "cwd": c, "message": {"role": "user", "content": [{"type": "tool_result", "tool_use_id": uid, "content": out}]}})

    def identity(self, prior_too=True):
        self.call("Bash", {"command": "sha256sum D:/r62-arch-a1r3-run/prompt.md"}, sha(os.path.join(RUN, "prompt.md")) + " *D:/r62-arch-a1r3-run/prompt.md")
        self.call("Read", {"file_path": r"D:\r62-arch-a1r3-run\prompt.md"}, numbered(prompt))

    def run_files(self, prior_too=True):
        files = "D:/r62-arch-a1r3-run/order.txt" + (" D:/r62-arch-a1r3-run/prior-authorization.txt" if prior_too else "")
        out = sha(os.path.join(RUN, "order.txt")) + " *order.txt" + ("\n" + sha(os.path.join(RUN, "prior-authorization.txt")) + " *prior" if prior_too else "")
        self.call("Bash", {"command": "sha256sum " + files}, out)
        self.call("Read", {"file_path": r"D:\r62-arch-a1r3-run\order.txt"}, numbered(order))
        if prior_too:
            self.call("Read", {"file_path": r"D:\r62-arch-a1r3-run\prior-authorization.txt"}, numbered(prior))

    def final(self, obj):
        self.lines.append({"type": "assistant", "sessionId": "s1", "cwd": self.cwd, "message": {"role": "assistant", "model": "claude-test",
                           "content": [{"type": "text", "text": "```json\n" + json.dumps(obj, ensure_ascii=False) + "\n```"}]}})

    def save(self, p):
        with open(p, "w", encoding="utf-8") as f:
            for x in self.lines:
                f.write(json.dumps(x, ensure_ascii=False) + "\n")


DISP = ["A62-A1-0%d" % i for i in range(1, 7)] + ["OBS-A1-01"] + ["A62-A1R-0%d" % i for i in range(1, 4)]
OPT = ["A62-A1-O%d" % i for i in range(1, 6)] + ["A62-A1R-O%d" % i for i in range(1, 6)]
IA = ["A62_A1_01_CLOSED", "A62_A1_02_CLOSED", "A62_A1_03_CLOSED", "A62_A1_04_CLOSED", "A62_A1_05_CLOSED", "A62_A1_06_CLOSED", "OBS_A1_01_CLOSED",
      "A62_A1R_01_CLOSED", "A62_A1R_02_CLOSED", "A62_A1R_03_CLOSED", "FC01Resolved", "FC02Resolved", "NoOwnerDecisionRequired",
      "SuitableForCoordinatorAgreement", "F4MayProceedAfterCoordinatorAgreed"]


def result(verdict, req, disp_premise, ifagreed_value):
    return {
        "Schema": "test", "RunId": IDS["RunId"], "InvocationId": IDS["InvocationId"], "LogicalReviewRequestId": IDS["LogicalReviewRequestId"], "AttemptSeq": 1,
        "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": REV, "ReviewedPath": "docs/initiatives/I-62-A-1.md",
        "ReviewedBlob": BLOB, "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "test",
        "ReviewerDeclaredIdentity": {"Runtime": "UNKNOWN", "Model": "UNKNOWN", "Effort": "UNKNOWN", "SessionOrThread": "UNKNOWN"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["test"]},
        "IdentityCheck": {"CloneHeadMatches": True, "WorktreeHeadMatches": True, "ObjectBlobMatches": True, "PackageBlobMatches": True, "CleanTree": True,
                          "OrderSha256Matches": True, "PriorAuthorizationSha256Matches": True},
        "InputsRead": ["test"], "Verdict": verdict, "RequiredFindings": req, "OptionalFindings": [],
        "FindingDispositions": [{"FindingId": f, "State": "CLOSED", "Rationale": "test", "PremiseRefs": [disp_premise] if f == "A62-A1R-01" else []} for f in DISP],
        "OptionalCorrections": [{"Id": i, "Assessment": "CONFIRMED", "Note": "t"} for i in OPT],
        "Focus": [{"Item": i, "Assessment": "CONFIRMED", "Note": "t"} for i in range(1, 17)],
        "F3ContractCrossCheck": [{"Contract": "t", "Assessment": "COMPATIBLE", "Note": "t"}],
        "CounterexampleVerification": {"Traces": 94, "Valid": 32, "Invalid": 62, "AllAsExpected": True, "Notes": []},
        "Materiality": {"Overall": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"},
                        "ObsA101": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"}, "Note": "t"},
        "OwnerAuthorityCheck": {"Spending": "NONE", "Credentials": "NONE", "Destructive": "NONE", "NewOVScenario": "NONE", "OVRemovalOrReassignment": "NONE",
                                "OwnerPolicyChoice": "NONE", "NewOwnerAuthority": "NONE", "OwnerDecisionRequired": False},
        "LegacyApplicability": {"AppliesOnlyToI62": True, "NoRetroactiveI61Change": True, "NoI64FixClaim": True, "Note": "t"},
        "IfAgreed": {k: ifagreed_value for k in IA},
        "KnownLimitations": [], "RecommendedNextAction": "t"}


req = [{"FindingId": "A62-A1S-01", "AffectedDelta": "D1-18", "ViolatedInvariant": "t", "Counterexample": "t", "CorrectionRequired": "t", "WhyItMatters": "t",
        "PremiseRefs": [{"Path": "docs/initiatives/I-62-proposal-v14.md", "Section": "B.8.8 I-P13", "LineStart": 2216, "LineEnd": 2217, "Quote": v14[2215][:60]}]}]
disp = {"Path": "D:/r62-arch-a1r3/docs/initiatives/I-62-A-1.md", "Section": "header", "LineStart": 1, "LineEnd": 1, "Quote": a1[0]}

# ------------------------------------------------------------------ good (v2-style actions)
good = T()
good.identity()
good.call("Bash", {"command": "git -C D:/r62-arch-a1r3 rev-parse HEAD; git -C D:/r62-arch-a1r3 status --porcelain"}, REV)
good.call("Bash", {"command": "git rev-parse HEAD && git status --porcelain"}, REV)
good.run_files()
good.call("Read", {"file_path": r"D:\r62-arch-a1r3\docs\initiatives\I-62-A-1.md", "offset": 1, "limit": 50}, numbered(a1[:50]))
good.call("Grep", {"pattern": "D1-1", "path": WT + r"\docs\initiatives\I-62-A-1.md", "output_mode": "content", "-n": True}, "\n".join("%d:%s" % (i + 1, l) for i, l in enumerate(a1) if "D1-1 " in l))
good.call("Bash", {"command": "wc -l D:/r62-arch-a1r3/docs/initiatives/I-62-A-1.md; git -C D:/r62-arch-a1r3 show " + R8 + ":docs/initiatives/I-62-proposal-v14.md | sed -n '2352p' | wc -m"}, "363 x\n2077")
good.call("Bash", {"command": "git -C D:/r62-arch-a1r3 show " + R8 + ":docs/initiatives/I-62-proposal-v14.md | sed -n '2216,2217p'"}, "\n".join(v14[2215:2217]))
good.call("Bash", {"command": "cd D:/r62-arch-a1r3 && python D:/r62-arch-a1r3/" + H + " " + OWN + "/out.json > " + OWN + "/run.log 2>&1; tail -2 " + OWN + "/run.log"}, "traces 94 all True missing []")
good.call("PowerShell", {"command": "$dn = Join-Path $env:LOCALAPPDATA 'Microsoft\\dotnet\\dotnet.exe'; Push-Location D:\\r62-arch-a1r3; & $dn test tests/RackCad.Tests/RackCad.Tests.csproj --nologo *> '" + OWN.replace("/", "\\") + "\\dotnet.log'; Pop-Location"}, "exit=0")
good.call("Read", {"file_path": OWN + "/dotnet.log"}, "1\tok")
good.final(result("CHANGES REQUIRED", req, disp, None))
good.save(os.path.join(OUTDIR, "good.jsonl"))

# ------------------------------------------------------------------ good2 (v3 actions)
long_line = v14[2351]
good2 = T()
good2.identity()
good2.call("Bash", {"command": "cd D:/r62-arch-a1r3 && git rev-parse HEAD && git status --porcelain"}, REV)
good2.call("Bash", {"command": "git -C " + WTF + " rev-parse HEAD"}, REV)
good2.run_files()
good2.call("Read", {"file_path": r"D:\r62-arch-a1r3\docs\initiatives\I-62-proposal-v14.md", "offset": 2350, "limit": 3},
           numbered([v14[2349], v14[2350], long_line[:2000]], 2350))
good2.call("Bash", {"command": "git -C D:/r62-arch-a1r3 show " + R8 + ":docs/initiatives/I-62-proposal-v14.md | sed -n '2352p'"}, long_line)
good2.call("Bash", {"command": "cd D:/r62-arch-a1r3 && python - <<'EOF'\nimport json\nr = json.load(open('docs/automation/evidence/I-62-A1/a1-counterexamples-result.json', encoding='utf-8'))\n"
                               "print(len(r['Traces']))\nEOF"}, "94")
good2.call("Write", {"file_path": OWN + "/count.py", "content": "import json\nprint(len(json.load(open('D:/r62-arch-a1r3/docs/automation/evidence/I-62-A1/a1-counterexamples-result.json'))['Traces']))\n"}, "ok")
good2.call("Bash", {"command": "python " + OWN + "/count.py"}, "94")
good2.call("Bash", {"command": "cat > " + OWN + "/c2.py <<'EOF'\nprint(open('" + OWN + "/out.json').read()[:10])\nEOF\npython " + OWN + "/c2.py " + OWN + "/out.json"}, "{")
good2.call("Bash", {"command": "dn=\"$LOCALAPPDATA/Microsoft/dotnet/dotnet.exe\"; cd D:/r62-arch-a1r3 && \"$dn\" test tests/RackCad.Tests/RackCad.Tests.csproj --nologo --logger \"trx;LogFileName=" + OWN + "/t.trx\" > " + OWN + "/dn.log 2>&1 || true; tail -3 " + OWN + "/dn.log"}, "Passed!")
good2.call("Bash", {"command": "grep -c 'D1-2' D:/r62-arch-a1r3/docs/initiatives/I-62-A-1.md 2>/dev/null; echo done"}, "5\ndone")
good2.call("Read", {"file_path": WT + r"\docs\initiatives\I-62-A-1.md", "offset": 1, "limit": 2}, numbered(a1[:2]))
good2.call("Bash", {"command": "git -C D:/r62-arch-a1r3 show HEAD:docs/initiatives/I-62-proposal-v14.md | sed -n '2216,2217p'"}, "\n".join(v14[2215:2217]))
good2.call("TodoWrite", {"todos": []}, "ok")
good2.final(result("CHANGES REQUIRED", req, disp, None))
good2.save(os.path.join(OUTDIR, "good2.jsonl"))

# ------------------------------------------------------------------ bad
bad = T()
bad.identity()                                                                                     # 0, 1
bad.call("Bash", {"command": "git -C D:/r62-arch-a1r3 rev-parse HEAD"}, REV)                         # 2
bad.run_files(prior_too=False)                                                                     # 3, 4
bad.call("Grep", {"pattern": "ancestr", "path": r"D:\r62-arch-a1r3\docs\automation\agent-execution", "output_mode": "content"}, "x")   # 5
bad.call("Read", {"file_path": r"D:\r62-arch-a1r3\docs\automation\state\I-62.yml"}, "1\tx")          # 6
bad.call("Bash", {"command": "ls D:/r62-arch-a1r3/docs"}, "x")                                      # 7
bad.call("Bash", {"command": "git -C D:/r62-arch-a1r3 show " + R8 + " --stat"}, "x")                 # 8
bad.call("Bash", {"command": "cd D:/r62-arch-a1r3 && python - <<'EOF'\nimport os\nprint(os.listdir('docs/automation'))\nprint(open('src/RackCad.Domain/x.cs').read())\nEOF"}, "x")   # 9
bad.call("Bash", {"command": "cd D:/r62-arch-a1r3 && python " + H + " D:/r62-arch-a1r3/docs/automation/evidence/I-62-A1/a1-counterexamples-result.json"}, "x")   # 10
bad.call("Bash", {"command": "cd D:/r62-arch-a1r3 && dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --results-directory D:/elsewhere/tr"}, "x")   # 11
bad.call("Bash", {"command": "python " + OWN + "/ghost.py"}, "x")                                   # 12
bad.call("Bash", {"command": "tail -5 " + OWN + "/never.log"}, "", no_result=True)                  # 13
bad.final(result("CHANGES REQUIRED", req, disp, True))
bad.save(os.path.join(OUTDIR, "bad.jsonl"))

outcomes, audits = {}, {}
for name in ("good", "good2", "bad"):
    o = os.path.join(OUTDIR, name + "-audit.json")
    subprocess.run([sys.executable, os.path.join(RUN, "post-review.py"), os.path.join(OUTDIR, name + ".jsonl"), CLONE, RUN, os.path.join(RUN, "result.schema.json"), o],
                   check=True, capture_output=True)
    audits[name] = json.load(open(o, encoding="utf-8"))
    outcomes[name] = audits[name]["Accreditation"]
expected_bad = {
    "no se comprobó el SHA-256 de prior-authorization.txt",
    "prior-authorization.txt no se leyó entera de forma fiel",
    "HEAD del worktree propio no verificado antes de la primera lectura sustantiva",
    "llamada 5 (Grep): búsqueda fuera de un archivo del cierre (directorio, glob o ruta no listada)",
    "llamada 6 (Read): lectura fuera del cierre",
    "llamada 7 (Bash): comando no permitido: ls",
    "llamada 8 (Bash): git show sin <rev>:<ruta del cierre>",
    "llamada 9 (Bash): python con un efecto no verificable (os.listdir)",
    "llamada 9 (Bash): python: ruta fuera del cierre: docs/automation",
    "llamada 9 (Bash): python: ruta fuera del cierre: src/RackCad.Domain/x.cs",
    "llamada 10 (Bash): el arnés exige exactamente una salida propia como argumento",
    "llamada 11 (Bash): dotnet: ruta distinta del proyecto y de las salidas propias: D:/elsewhere/tr",
    "llamada 12 (Bash): script propio sin contenido auditable en la transcripción: " + OWN + "/ghost.py",
    "llamada 13 (Bash): llamada sin resultado (terminación incompleta)",
    "resultado: IfAgreed debe ir en null salvo con AGREED",
    "premisa sobre líneas no entregadas fielmente en A62-A1S-01 (docs/initiatives/I-62-proposal-v14.md 2216-2217)",
    "premisa sobre líneas no entregadas fielmente en A62-A1R-01 (docs/initiatives/I-62-A-1.md 1-1)",
}
ok_good = outcomes["good"]["Status"] == "ACCREDITED"
tk = audits["good2"]["Fidelity"]["TruncatedKnownLongLines"]
ok_good2 = outcomes["good2"]["Status"] == "ACCREDITED" and len(tk) == 1 and tk[0]["Line"] == 2352
ok_bad = outcomes["bad"]["Status"] == "NOT_ACCREDITED" and set(outcomes["bad"]["Reasons"]) == expected_bad
print(json.dumps({"good": outcomes["good"], "good2": outcomes["good2"], "good2_truncated_known": tk, "bad_status": outcomes["bad"]["Status"],
                  "bad_unexpected": sorted(set(outcomes["bad"]["Reasons"]) - expected_bad),
                  "bad_missing": sorted(expected_bad - set(outcomes["bad"]["Reasons"])), "PASS": ok_good and ok_good2 and ok_bad}, ensure_ascii=False, indent=1))
sys.exit(0 if ok_good and ok_good2 and ok_bad else 1)
