# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
import hashlib
import json
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO = "<checkout-principal>"
EV = "docs/automation/evidence/I-64-pilot/"
SUB = EV + "F4B-PBT2A-DRAFT-STATE-PORT-nc2-substitute/2/R20261119T125505Z-c2d3/nc2_exact_whitelist_substitute_f4b.py"
SNAPK = EV + "F4-ENVIRONMENT/snapshot-K/snapshot-k.json"
DECK = EV + "F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md"
DEC34 = EV + "F4B-DRAFTS/coordinator/owner-pbt2a-controls-decisions.md"
DEC35 = EV + "F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md"
DISP = EV + "F4B-DRAFTS/f4b-dispositions.md"


def git(*args, check=True):
    r = subprocess.run(["git", "-C", REPO, *args], capture_output=True)
    if check and r.returncode != 0:
        return None
    return r.stdout


def show(rev, path):
    return git("show", f"{rev}:{path}")


for rev in ("56cb026f", "05beabfd"):
    print("==", rev, git("rev-parse", rev).decode().strip())
    b = show(rev, SUB)
    print("substitute present:", b is not None, "sha256:", hashlib.sha256(b).hexdigest() if b else None, "bytes:", len(b) if b else None)
    if b:
        crlf = b.count(b"\r\n")
        print("  CRLF count:", crlf, "LF count:", b.count(b"\n"))
        for line in b.decode("utf-8", "replace").splitlines():
            if re.search(r"NC2-|SUBSTITUTE_ID|SCOPE|TaskId|TASK", line):
                print("   |", line[:200])
    s = show(rev, SNAPK)
    print("snapshot-k present:", s is not None)
    if s:
        j = json.loads(s.decode("utf-8-sig"))
        def walk(o, p=""):
            if isinstance(o, dict):
                for k, v in o.items():
                    walk(v, p + "." + k)
            elif isinstance(o, list):
                for i, v in enumerate(o[:5]):
                    walk(v, p + f"[{i}]")
            else:
                if re.search(r"Sha|sha|Probe|probe|Run|Result|Outcome|Snapshot|Id|Pass|Verdict|Decision", p):
                    print("   ", p, "=", str(o)[:120])
        walk(j)
    d = show(rev, DECK)
    if d:
        t = d.decode("utf-8", "replace")
        for line in t.splitlines():
            if re.search(r"Recomendado|Respuesta|respuesta|E1F69847|2a9a|F4B-E37|sustituye|J\b", line):
                print("  DECK |", line[:220])
    print()

rev = "05beabfd"
d34 = show(rev, DEC34)
if d34:
    t = d34.decode("utf-8", "replace")
    print("== F4B-E34 decision (question 2 and Alcance)")
    on = False
    for line in t.splitlines():
        if re.match(r"#+ .*[Pp]regunta 2", line):
            on = True
        elif re.match(r"#+ .*[Pp]regunta 3", line):
            on = False
        if on or re.search(r"Alcance|alcance", line):
            print("  |", line[:300])
d35 = show(rev, DEC35)
if d35:
    t = d35.decode("utf-8", "replace")
    print("== F4B-E35 decision: headings and answers")
    for line in t.splitlines():
        if re.match(r"#+ ", line) or re.search(r"Recomendado|RR-[1-7]|PB-T3a|Empieza", line):
            print("  |", line[:300])
disp = show(rev, DISP)
if disp:
    t = disp.decode("utf-8", "replace")
    print("== f4b-dispositions entries E34..E39")
    for line in t.splitlines():
        if re.search(r"F4B-E3[4-9]", line) and (line.startswith("#") or line.startswith("|") or line.startswith("**")):
            print("  |", line[:260])
