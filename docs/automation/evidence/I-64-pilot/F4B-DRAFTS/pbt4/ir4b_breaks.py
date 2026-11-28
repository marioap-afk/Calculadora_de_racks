"""Review of PB-T4b: the assertions of the five guards that break outside the list, translated, on the base (the GREEN of PB-T4a,
v44/base) and on the GREEN of PB-T4b (v44/green_b) of WorkspaceHostCore.cs (read-only; reads the two dumped files only).

Usage: python -I ir4b_breaks.py <base WHC> <green WHC> <out.txt>
The helpers are those of tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs at 5aaacce3 (CodeOnly, BodyOf, Count).
"""
import re
import sys

base_p, green_p, out_p = sys.argv[1:4]


def code_only(t):
    return re.sub(r"//[^\r\n]*", "", re.sub(r"/\*.*?\*/", "", t, flags=re.S))


def body_of(code, sig):
    m = re.search(sig, code)
    if not m:
        return None
    start = m.start()
    open_ = code.find("{", m.end() - 1)
    arrow = code.find("=>", m.end() - 1)
    if arrow >= 0 and (open_ < 0 or arrow < open_):
        return code[start:code.find(";", arrow) + 1]
    depth = 0
    for i in range(open_, len(code)):
        if code[i] == "{":
            depth += 1
        elif code[i] == "}":
            depth -= 1
            if depth == 0:
                return code[start:i + 1]
    raise SystemExit("unbalanced " + sig)


def count(code, pat):
    return len(re.findall(pat, code))


def in_order(text, *needles):
    pos, out = -1, []
    for n in needles:
        i = text.find(n, pos + 1)
        out.append((n, i))
        if i < 0:
            return False, out
        pos = i
    return True, out


ADM = r"\bpilot\.Read\.IsAdmitted\s*\?\s*null\s*:\s*pilot\.Read\.Admission\.Diagnostic\b"
ANOTHER = "\nvar again = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic;"
L = []
for name, path in (("base (GREEN of PB-T4a)", base_p), ("GREEN of PB-T4b", green_p)):
    core = code_only(open(path, encoding="utf-8").read().replace("\r\n", "\n"))
    sig = {
        "check": r"public\s+EditCheckOutcome\s+TryCheckPilotEdit\s*\(",
        "confirm": r"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\(",
        "begin": r"public\s+EditIntentOutcome\s+TryBeginPilotEdit\s*\(",
        "keep": r"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\(",
        "discard": r"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\(",
        "reload": r"public\s+EditIntentOutcome\s+TryReloadPilotEdit\s*\(\s*EditIntentRequest\s+request\s*\)",
        "mark": r"private\s+void\s+MarkPilotDraftsPossiblyStale\s*\(\s*SessionId\s+\w+\s*\)",
        "update": r"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(",
    }
    b = {k: body_of(core, v) for k, v in sig.items()}
    L.append("== " + name)
    # 1. EIG:871
    r1 = [count(core, ADM), count(b["confirm"], ADM), count(b["check"], ADM)] + [count(b[k], ADM) for k in ("begin", "keep", "discard")]
    L.append("1 EIG:871 core=%d confirm=%d check=%d begin/keep/discard=%s -> %s" % (r1[0], r1[1], r1[2], r1[3:], "PASS" if r1 == [2, 1, 1, 0, 0, 0] else "FAIL"))
    # 2. EIG:883 (the first three assertions; the fourth does not depend on the count)
    r2 = [count(core + ANOTHER, ADM), count(b["check"] + ANOTHER, ADM), count(b["keep"] + ANOTHER, ADM)]
    L.append("2 EIG:883 core+1=%d check+1=%d keep+1=%d -> %s" % (r2[0], r2[1], r2[2], "PASS" if r2 == [3, 2, 1] else "FAIL"))
    # 3. WLG:627
    m = b["mark"]
    r3 = ["_drafts.RackIdsOf(session)" in m, "_drafts.Find<PushBackPilotDraftState>(session, rackId)" in m, ".MarkPossiblyStale()" in m]
    L.append("3 WLG:627 RackIdsOf=%s Find<Pilot>(session, rackId)=%s MarkPossiblyStale=%s -> %s" % (r3[0], r3[1], r3[2], "PASS" if all(r3) else "FAIL"))
    L.append("    mark body: " + " ".join(m.split())[:220])
    # 4. WLG:642
    r = b["reload"]
    ok, where = in_order(r, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(",
                         "_drafts.Find<PushBackPilotDraftState>(", ".NeedsLazyReload", "_editIntentReader!.ReadIntent(", ".DiscardAndReload(pilot.Read);")
    c4 = [count(r, r"\.ReadIntent\s*\("), count(r, r"_editIntentReader\s*[.!]"), count(r, r"\bpilot\.Read\b"),
          bool(re.search(r"\.DiscardAndReload\s*\(\s*pilot\.Read\s*\)\s*;", r)), count(r, r"\.DiscardAndReload\s*\(")]
    L.append("4 WLG:642 order=%s missing=%s ReadIntent=%d reader=%d pilot.Read=%d DiscardAndReload(pilot.Read)=%s DiscardAndReload=%d -> %s"
             % (ok, [n for n, i in where if i < 0], c4[0], c4[1], c4[2], c4[3], c4[4], "PASS" if ok and c4 == [1, 1, 1, True, 1] else "FAIL"))
    L.append("    reload: " + "; ".join(sorted(set(re.findall(r"var \w+ = read\.\w+;|\w+\.DiscardAndReload\([^)]*\)|_drafts\.Find<\w+>", r)))))
    # 5. WPUG:823
    u = b["update"]
    ok, where = in_order(u, "UpdateGate(", "string.IsNullOrWhiteSpace(request.RackId)", "_drafts.Find<PushBackPilotDraftState>(",
                         "PushBackPilotUpdateRun.Precheck(", ".TryBeginApply(", "PushBackPilotUpdateRun.Execute(", ".CompleteApply(",
                         "_knownClean.Remove(request.Session);")
    c5 = [count(u, r"PushBackPilotUpdateRun\.Execute\s*\("), count(u, r"PushBackPilotUpdateRun\.Precheck\s*\(")]
    L.append("5 WPUG:823 order=%s missing=%s Execute=%d Precheck=%d -> %s" % (ok, [n for n, i in where if i < 0], c5[0], c5[1],
                                                                            "PASS" if ok and c5 == [1, 1] else "FAIL"))
    L.append("    update: " + "; ".join(sorted(set(re.findall(r"PushBackPilotUpdateRun\.\w+\(|_drafts\.Find<\w+>", u)))))
    L.append("    var pilot = read.PilotRead; in the whole core: %d" % count(core, r"var\s+pilot\s*=\s*read\.PilotRead\s*;"))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
