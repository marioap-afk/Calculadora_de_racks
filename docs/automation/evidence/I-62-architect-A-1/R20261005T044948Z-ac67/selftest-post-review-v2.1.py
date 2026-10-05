"""Self-test of post-review.py v2 with two synthetic transcripts: a conforming review (→ ACCREDITED) and one with the typical deviations
(→ NOT_ACCREDITED for exactly the expected reasons). Usage: python selftest-post-review.py <out dir>"""
import hashlib
import json
import os
import subprocess
import sys

RUN = os.path.dirname(os.path.abspath(__file__))
CLONE = r"D:\r62-arch-a1r2"
OUTDIR = sys.argv[1]
os.makedirs(OUTDIR, exist_ok=True)
CL = json.load(open(os.path.join(RUN, "closure.json"), encoding="utf-8"))
REV, IDS = CL["AuthorityRevision"], CL["Ids"]
BLOB = next(r["Blob"] for r in CL["CanonicalInputs"] if r["Path"] == "docs/initiatives/I-62-A-1.md")
WT = r"D:\Documentos\Worktrees\r62-arch-a1r2\test-wt"
OWN = "C:/Users/u/AppData/Local/Temp/claude/D--Documentos-Worktrees-r62-arch-a1r2-test-wt/s1/scratchpad"


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


def numbered(lines, start=1):
    return "\n".join("%6d\t%s" % (i, l) for i, l in enumerate(lines, start))


def canon(rel):
    return subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")


prompt = open(os.path.join(RUN, "prompt.md"), encoding="utf-8").read().replace("\r\n", "\n").split("\n")
order = open(os.path.join(RUN, "order.txt"), encoding="utf-8").read().replace("\r\n", "\n").split("\n")
a1 = canon("docs/initiatives/I-62-A-1.md")
v14 = canon("docs/initiatives/I-62-proposal-v14.md")


class T:
    def __init__(self):
        self.lines, self.k = [], 0
        self.lines.append({"type": "user", "sessionId": "s1", "cwd": WT, "version": "test", "message": {"role": "user", "content": "chip"}})

    def call(self, name, inp, out):
        self.k += 1
        uid = "t%d" % self.k
        self.lines.append({"type": "assistant", "sessionId": "s1", "cwd": WT, "message": {"role": "assistant", "model": "claude-test",
                           "content": [{"type": "tool_use", "id": uid, "name": name, "input": inp}]}})
        self.lines.append({"type": "user", "sessionId": "s1", "cwd": WT, "message": {"role": "user", "content": [{"type": "tool_result", "tool_use_id": uid, "content": out}]}})

    def final(self, obj):
        self.lines.append({"type": "assistant", "sessionId": "s1", "cwd": WT, "message": {"role": "assistant", "model": "claude-test",
                           "content": [{"type": "text", "text": "```json\n" + json.dumps(obj, ensure_ascii=False) + "\n```"}]}})

    def save(self, p):
        with open(p, "w", encoding="utf-8") as f:
            for x in self.lines:
                f.write(json.dumps(x, ensure_ascii=False) + "\n")


def result(verdict, req, disp_premise, ifagreed_value):
    findings = ["A62-A1-0%d" % i for i in range(1, 7)] + ["OBS-A1-01"]
    return {
        "Schema": "test", "RunId": IDS["RunId"], "InvocationId": IDS["InvocationId"], "LogicalReviewRequestId": IDS["LogicalReviewRequestId"], "AttemptSeq": 1,
        "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": REV, "ReviewedPath": "docs/initiatives/I-62-A-1.md",
        "ReviewedBlob": BLOB, "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "test",
        "ReviewerDeclaredIdentity": {"Runtime": "UNKNOWN", "Model": "UNKNOWN", "Effort": "UNKNOWN", "SessionOrThread": "UNKNOWN"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["test"]},
        "IdentityCheck": {"CloneHeadMatches": True, "WorktreeHeadMatches": True, "ObjectBlobMatches": True, "PackageBlobMatches": True, "CleanTree": True, "OrderSha256Matches": True},
        "InputsRead": ["test"], "Verdict": verdict, "RequiredFindings": req, "OptionalFindings": [],
        "FindingDispositions": [{"FindingId": f, "State": "CLOSED", "Rationale": "test", "PremiseRefs": [disp_premise] if f == "A62-A1-01" else []} for f in findings],
        "OptionalCorrections": [{"Id": "A62-A1-O%d" % i, "Assessment": "CONFIRMED", "Note": "t"} for i in range(1, 6)],
        "Focus": [{"Item": i, "Assessment": "CONFIRMED", "Note": "t"} for i in range(1, 13)],
        "F3ContractCrossCheck": [{"Contract": "t", "Assessment": "COMPATIBLE", "Note": "t"}],
        "CounterexampleVerification": {"Traces": 73, "Valid": 24, "Invalid": 49, "AllAsExpected": True, "Notes": []},
        "Materiality": {"Overall": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "YES", "M06": "NO", "M07": "NO", "M08": "NO"},
                        "ObsA101": {"M01": "NO", "M02": "YES", "M03": "YES", "M04": "YES", "M05": "NO", "M06": "NO", "M07": "NO", "M08": "NO"}, "Note": "t"},
        "OwnerAuthorityCheck": {"Spending": "NONE", "Credentials": "NONE", "Destructive": "NONE", "NewOVScenario": "NONE", "OVRemovalOrReassignment": "NONE",
                                "OwnerPolicyChoice": "NONE", "NewOwnerAuthority": "NONE", "OwnerDecisionRequired": False},
        "LegacyApplicability": {"AppliesOnlyToI62": True, "NoRetroactiveI61Change": True, "NoI64FixClaim": True, "Note": "t"},
        "IfAgreed": {k: ifagreed_value for k in ("A62_A1_01_CLOSED", "A62_A1_02_CLOSED", "A62_A1_03_CLOSED", "A62_A1_04_CLOSED", "A62_A1_05_CLOSED", "A62_A1_06_CLOSED",
                                                 "OBS_A1_01_CLOSED", "FC01Resolved", "FC02Resolved", "NoOwnerDecisionRequired", "SuitableForCoordinatorAgreement",
                                                 "F4MayProceedAfterCoordinatorAgreed")},
        "KnownLimitations": [], "RecommendedNextAction": "t"}


req = [{"FindingId": "A62-A1R-01", "AffectedDelta": "D1-8", "ViolatedInvariant": "t", "Counterexample": "t", "CorrectionRequired": "t", "WhyItMatters": "t",
        "PremiseRefs": [{"Path": "docs/initiatives/I-62-proposal-v14.md", "Section": "B.8.8 I-P13", "LineStart": 2216, "LineEnd": 2217, "Quote": v14[2215][:60]}]}]
disp = {"Path": "D:/r62-arch-a1r2/docs/initiatives/I-62-A-1.md", "Section": "header", "LineStart": 1, "LineEnd": 1, "Quote": a1[0]}

good = T()
good.call("Bash", {"command": "sha256sum D:/r62-arch-a1r2-run/prompt.md"}, sha(os.path.join(RUN, "prompt.md")) + " *D:/r62-arch-a1r2-run/prompt.md")
good.call("Read", {"file_path": r"D:\r62-arch-a1r2-run\prompt.md"}, numbered(prompt))
good.call("Bash", {"command": "git -C D:/r62-arch-a1r2 rev-parse HEAD; git -C D:/r62-arch-a1r2 status --porcelain"}, REV)
good.call("Bash", {"command": "git rev-parse HEAD && git status --porcelain"}, REV)
good.call("Bash", {"command": "sha256sum D:/r62-arch-a1r2-run/order.txt"}, sha(os.path.join(RUN, "order.txt")))
good.call("Read", {"file_path": r"D:\r62-arch-a1r2-run\order.txt"}, numbered(order[:-1] if order[-1] == "" else order))
good.call("Read", {"file_path": r"D:\r62-arch-a1r2\docs\initiatives\I-62-A-1.md", "offset": 1, "limit": 50}, numbered(a1[:50]))
good.call("Grep", {"pattern": "D1-1", "path": WT + r"\docs\initiatives\I-62-A-1.md", "output_mode": "content", "-n": True}, "\n".join("%d:%s" % (i + 1, l) for i, l in enumerate(a1) if "D1-1 " in l))
good.call("Bash", {"command": "wc -l D:/r62-arch-a1r2/docs/initiatives/I-62-A-1.md; git -C D:/r62-arch-a1r2 show 0ad410f8:docs/initiatives/I-62-proposal-v14.md | sed -n '2352p' | wc -m"}, "308 x\n2077")
good.call("Bash", {"command": "git -C D:/r62-arch-a1r2 show 0ad410f8:docs/initiatives/I-62-proposal-v14.md | sed -n '2216,2217p'"}, "\n".join(v14[2215:2217]))
good.call("Bash", {"command": "cd D:/r62-arch-a1r2 && python D:/r62-arch-a1r2/docs/automation/evidence/I-62-A1/a1-counterexamples.py " + OWN + "/out.json > " + OWN + "/run.log 2>&1; tail -2 " + OWN + "/run.log"}, "traces 73 all True missing []")
good.call("PowerShell", {"command": "$dn = Join-Path $env:LOCALAPPDATA 'Microsoft\\dotnet\\dotnet.exe'; Push-Location D:\\r62-arch-a1r2; & $dn test tests/RackCad.Tests/RackCad.Tests.csproj --nologo *> '" + OWN.replace("/", "\\") + "\\dotnet.log'; Pop-Location"}, "exit=0")
good.call("Read", {"file_path": OWN + "/dotnet.log"}, "1\tok")
good.final(result("CHANGES REQUIRED", req, disp, None))
good.save(os.path.join(OUTDIR, "good.jsonl"))

bad = T()
bad.call("Bash", {"command": "sha256sum D:/r62-arch-a1r2-run/prompt.md"}, sha(os.path.join(RUN, "prompt.md")))
bad.call("Read", {"file_path": r"D:\r62-arch-a1r2-run\prompt.md"}, numbered(prompt))
bad.call("Bash", {"command": "git -C D:/r62-arch-a1r2 rev-parse HEAD"}, REV)
bad.call("Bash", {"command": "sha256sum D:/r62-arch-a1r2-run/order.txt"}, sha(os.path.join(RUN, "order.txt")))
bad.call("Read", {"file_path": r"D:\r62-arch-a1r2-run\order.txt"}, numbered(order[:-1] if order[-1] == "" else order))
bad.call("Grep", {"pattern": "ancestr", "path": r"D:\r62-arch-a1r2\docs\automation\agent-execution", "output_mode": "content"}, "x")
bad.call("Read", {"file_path": r"D:\r62-arch-a1r2\docs\automation\state\I-62.yml"}, "1\tx")
bad.call("Bash", {"command": "ls D:/r62-arch-a1r2/docs"}, "x")
bad.call("Bash", {"command": "git -C D:/r62-arch-a1r2 show 0ad410f8 --stat"}, "x")
bad.final(result("CHANGES REQUIRED", req, disp, True))
bad.save(os.path.join(OUTDIR, "bad.jsonl"))

outcomes = {}
for name in ("good", "bad"):
    o = os.path.join(OUTDIR, name + "-audit.json")
    subprocess.run([sys.executable, os.path.join(RUN, "post-review-v2.1.py"), os.path.join(OUTDIR, name + ".jsonl"), CLONE, RUN, os.path.join(RUN, "result.schema.json"), o],
                   check=True, capture_output=True)
    outcomes[name] = json.load(open(o, encoding="utf-8"))["Accreditation"]
expected_bad = {"HEAD del worktree propio no verificado antes de la primera lectura sustantiva",
                "llamada 5 (Grep): búsqueda fuera de un archivo del cierre (directorio, glob o ruta no listada)",
                "llamada 6 (Read): lectura fuera del cierre",
                "llamada 7 (Bash): comando no permitido: ls",
                "llamada 8 (Bash): git show sin <rev>:<ruta del cierre>",
                "resultado: IfAgreed debe ir en null salvo con AGREED",
                "premisa sobre líneas no entregadas fielmente en A62-A1R-01 (docs/initiatives/I-62-proposal-v14.md 2216-2217)",
                "premisa sobre líneas no entregadas fielmente en A62-A1-01 (docs/initiatives/I-62-A-1.md 1-1)"}
ok_good = outcomes["good"]["Status"] == "ACCREDITED"
ok_bad = outcomes["bad"]["Status"] == "NOT_ACCREDITED" and set(outcomes["bad"]["Reasons"]) == expected_bad
print(json.dumps({"good": outcomes["good"], "bad_status": outcomes["bad"]["Status"], "bad_unexpected": sorted(set(outcomes["bad"]["Reasons"]) - expected_bad),
                  "bad_missing": sorted(expected_bad - set(outcomes["bad"]["Reasons"])), "PASS": ok_good and ok_bad}, ensure_ascii=False, indent=1))
sys.exit(0 if ok_good and ok_bad else 1)
