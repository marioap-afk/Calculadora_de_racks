"""Review of PB-T4b, sampling of the 108 "still green": two guards that count literals in WHC, translated (EIG:1350-1355, the seven
answers of the flight; EIG:1281-1292, the texts of the update), on the base and on the GREEN of PB-T4b (read-only).

Usage: python -I ir4b_sample.py <base WHC> <green WHC> <out.txt>
"""
import re
import sys

sys.path.insert(0, __file__.rsplit("ir4b_sample.py", 1)[0])
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


FLIGHT_BODIES = [
    ("SetPendingText", r"PilotFieldResult\s+IWorkspacePilotEditPort\.SetPendingText\s*\("),
    ("CommitField", r"PilotFieldResult\s+IWorkspacePilotEditPort\.CommitField\s*\("),
    ("TryConfirmPilotBase", r"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\("),
    ("TryKeepPilotEdit", r"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\("),
    ("TryDiscardAndReloadPilotEdit", r"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\("),
    ("DiscardRack", r"EditDiscardOutcome\s+IWorkspacePilotEditPort\.DiscardRack\s*\("),
    ("DiscardDocument", r"EditDiscardOutcome\s+IWorkspacePilotEditPort\.DiscardDocument\s*\("),
]
FLIGHT = re.compile(r"\bUpdateInFlightFor(?:Request)?\s*\(|\b_tickets\b|\bIsAnyPilotDraftApplying\s*\(|\bIsPilotDraftApplying\s*\(")
BUSY = re.compile(r"\.(?:Busy|Waiting)\s*\(")
UPDATE_BODIES = [
    ("UpdateGate", r"private\s+EditGate\s+UpdateGate\s*\("),
    ("RequestUpdate", r"EditUpdateStart\s+IWorkspacePilotEditPort\.RequestUpdate\s*\("),
    ("TakeUpdateProgress", r"EditUpdateProgress\s+IWorkspacePilotEditPort\.TakeUpdateProgress\s*\("),
    ("ExpireStaleUpdate", r"private\s+UpdateTicket\?\s+ExpireStaleUpdate\s*\("),
    ("PilotUpdateRun", r"private\s+sealed\s+class\s+PilotUpdateRun\b"),
    ("TryUpdatePilot", r"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\("),
    ("AnswerOfAClosedThatWasNotAccepted", r"private\s+static\s+EditUpdateOutcome\s+AnswerOfAClosedThatWasNotAccepted\s*\("),
]
UPDATE_LIT = re.compile(r'\breason\s*=\s*"|\b(?:EditUpdateOutcome|EditUpdateStart)\.\w+\s*\(\s*"|\bEditUpdateOutcome\.Closed(?:InError)?\s*\(\s*[\w.]+\s*,\s*"|\.(?:Busy|Waiting)\s*\(\s*"')
L = []
for name, path in (("base", base_p), ("GREEN of PB-T4b", green_p)):
    core = code_only(open(path, encoding="utf-8").read().replace("\r\n", "\n"))
    v = []
    for n, s in FLIGHT_BODIES:
        body = body_of(core, s)
        if body is None:
            v.append(n + " missing")
            continue
        answers = [st for st in body.split(";") if FLIGHT.search(st) and BUSY.search(st)]
        if len(answers) != 1:
            v.append("%s has %d answers" % (n, len(answers)))
        for a in answers:
            if '"' in a:
                v.append(n + " literal")
            if "PushBackPilotHostTexts.UpdateInFlightReason" not in a:
                v.append(n + " not the constant")
    u = []
    for n, s in UPDATE_BODIES:
        body = body_of(core, s)
        if body is None:
            u.append(n + " missing")
        elif UPDATE_LIT.search(body):
            u.append(n + ": " + UPDATE_LIT.search(body).group(0))
    gate = body_of(core, UPDATE_BODIES[0][1]) or ""
    upd = body_of(core, UPDATE_BODIES[5][1]) or ""
    extra = ["PushBackPilotUpdateRun.NoActiveDocumentReason" in gate, "PushBackPilotUpdateRun.ModalOpenReason" in gate,
             not re.search(r"const\s+string\s+UpdateNotRunReason", core), "PushBackPilotUpdateRun.UpdateNotRunReason" in upd]
    L.append("== %s\n  EIG:1350 flight violations: %s -> %s\n  EIG:1286 update text violations: %s, constants %s -> %s"
             % (name, v, "PASS" if not v else "FAIL", u, extra, "PASS" if not u and all(extra) else "FAIL"))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
