"""Self-test of post-review.py with two synthetic reviewer transcripts (no model involved).

GOOD: hash check, faithful Read of prompt.md and of A-1, a faithful `git show | sed` of a long line, and a coherent AGREED result → ACCREDITED.
BAD:  the same plus a Read outside the closure, a Read whose «≤» arrives as «=», and an AGREED with one REQUIRED → NOT_ACCREDITED for exactly those
      three reasons (and nothing else).

Usage: python selftest-post-review.py <clone> <run dir> <kit dir> <work dir>
"""
import hashlib
import json
import os
import subprocess
import sys

CLONE, RUN, KIT, WORK = sys.argv[1:5]
REV = "bf7b0d9c4ec79e38350bad55e0debcde55b3cec9"
os.makedirs(WORK, exist_ok=True)


def show(rel):
    return subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")


def numbered(lines, start):
    return "\n".join("%6d\t%s" % (start + i, l) for i, l in enumerate(lines))


def tool(uid, name, inp, out):
    return [{"type": "assistant", "sessionId": "s-review", "cwd": CLONE, "version": "synthetic", "message": {"model": "model-x", "content": [
                {"type": "tool_use", "id": uid, "name": name, "input": inp}]}},
            {"type": "user", "sessionId": "s-review", "message": {"content": [{"type": "tool_result", "tool_use_id": uid, "content": out}]}}]


prompt = open(os.path.join(RUN, "prompt.md"), encoding="utf-8").read().replace("\r\n", "\n").split("\n")
psha = hashlib.sha256(open(os.path.join(RUN, "prompt.md"), "rb").read()).hexdigest()
a1 = show("docs/initiatives/I-62-A-1.md")
dec = show("docs/automation/decisions/I-62.md")
v14 = show("docs/initiatives/I-62-proposal-v14.md")
le = next(i for i, l in enumerate(v14) if "≤" in l)
good_result = {
    "Schema": "x", "RunId": "R20261003T023945Z-cab7", "InvocationId": "I20261003T023945Z-cab7", "LogicalReviewRequestId": "L20261003T023945Z-cab7",
    "AttemptSeq": 1, "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": REV,
    "ReviewedPath": "docs/initiatives/I-62-A-1.md", "ReviewedBlob": "09ca93285975c4b7af6471d6ae91bfa12c94a1fc", "ReviewerMode": "SEPARATE SESSION",
    "SamePersonAsAuthor": "x", "ReviewerDeclaredIdentity": {"Runtime": "UNKNOWN", "Model": "UNKNOWN", "Effort": "UNKNOWN", "SessionOrThread": "UNKNOWN"},
    "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["CLAUDE.md"]},
    "IdentityCheck": {"HeadMatches": True, "ObjectBlobMatches": True, "PackageBlobMatches": True, "CleanTree": True}, "InputsRead": ["x"],
    "Verdict": "AGREED", "RequiredFindings": [], "OptionalFindings": [],
    "FC01": [{"Item": i, "Assessment": "CONFIRMED", "Note": "x"} for i in range(1, 11)],
    "FC02": [{"Item": i, "Assessment": "CONFIRMED", "Note": "x"} for i in range(1, 11)],
    "F3ContractCrossCheck": [], "CounterexampleVerification": [],
    "OwnerAuthorityCheck": {"Spending": "NONE", "Credentials": "NONE", "Destructive": "NONE", "NewOVScenario": "NONE", "OVRemovalOrReassignment": "NONE",
                            "OwnerPolicyChoice": "NONE", "OwnerDecisionRequired": False},
    "LegacyApplicability": {"AppliesOnlyToI62": True, "NoRetroactiveI61Change": True, "NoI64FixClaim": True, "Note": "x"},
    "IfAgreed": {k: True for k in ["FC01Resolved", "FC02Resolved", "NoOwnerDecisionRequired", "SuitableForCoordinatorAgreement",
                                   "F4MayUseFreezePlusA1AfterCoordinatorVerdict"]},
    "KnownLimitations": [], "RecommendedNextAction": "x"}


def transcript(bad):
    e = [{"type": "user", "sessionId": "s-review", "cwd": CLONE, "message": {"content": "prompt de lanzamiento"}}]
    e += tool("t1", "Bash", {"command": "sha256sum D:/r62-arch-a1-run/prompt.md"}, psha + "  D:/r62-arch-a1-run/prompt.md")
    e += tool("t2", "Read", {"file_path": os.path.join(RUN, "prompt.md")}, numbered(prompt, 1))
    e += tool("t3", "Read", {"file_path": os.path.join(CLONE, "docs", "initiatives", "I-62-A-1.md"), "offset": 60, "limit": 5}, numbered(a1[59:64], 60))
    e += tool("t4", "Bash", {"command": "git -C D:/r62-arch-a1 show bf7b0d9c:docs/automation/decisions/I-62.md | sed -n '657p'"}, dec[656])
    e += tool("t5", "Read", {"file_path": os.path.join(CLONE, "docs", "initiatives", "I-62-proposal-v14.md"), "offset": le + 1, "limit": 1},
              numbered([v14[le].replace("≤", "=") if bad else v14[le]], le + 1))
    r = json.loads(json.dumps(good_result))
    if bad:
        e += tool("t6", "Read", {"file_path": r"C:\Users\x\.claude\projects\D--Documentos-Codex-Calculadora-de-racks\memory\MEMORY.md"}, "1\tsecreto")
        r["RequiredFindings"] = [{"FindingId": "A62-A1-01", "AffectedDelta": "D1-5", "ViolatedInvariant": "I-P13", "Counterexample": "x",
                                  "CorrectionRequired": "x", "WhyItMatters": "x",
                                  "PremiseRefs": [{"Path": "docs/initiatives/I-62-A-1.md", "Section": "2.3", "LineStart": 60, "LineEnd": 64,
                                                   "Quote": a1[60].strip()[:60]}]}]
    e.append({"type": "assistant", "sessionId": "s-review", "message": {"model": "model-x", "content": [{"type": "text", "text": "```json\n" + json.dumps(r, ensure_ascii=False) + "\n```"}]}})
    return e


summary = {}
for name, bad, expected in (("good", False, "ACCREDITED"), ("bad", True, "NOT_ACCREDITED")):
    tp = os.path.join(WORK, name + ".jsonl")
    with open(tp, "w", encoding="utf-8") as f:
        for x in transcript(bad):
            f.write(json.dumps(x, ensure_ascii=False) + "\n")
    op = os.path.join(WORK, name + "-audit.json")
    subprocess.run([sys.executable, os.path.join(KIT, "post-review.py"), tp, CLONE, RUN, os.path.join(KIT, "result.schema.json"), op], check=True,
                   capture_output=True)
    a = json.load(open(op, encoding="utf-8"))
    reasons = a["Accreditation"]["Reasons"]
    ok = a["Accreditation"]["Status"] == expected
    if bad:
        want = ["lectura fuera del cierre", "fidelidad:", "resultado: AGREED exige"]
        ok = ok and all(any(r.startswith(w) for r in reasons) for w in want) and len(reasons) == 3
    summary[name] = {"Expected": expected, "Got": a["Accreditation"]["Status"], "Reasons": reasons, "Pass": ok}
json.dump(summary, open(os.path.join(WORK, "selftest-result.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=1)
print(json.dumps({k: v["Pass"] for k, v in summary.items()}))
sys.exit(0 if all(v["Pass"] for v in summary.values()) else 1)
