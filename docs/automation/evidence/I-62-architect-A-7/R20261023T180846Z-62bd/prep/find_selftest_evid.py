"""Determina qué archivos de I-62-A7/r3/ existían cuando run_a7r3.py ejecutó la autoprueba de las guardas (fase q7): reproduce el modo `preview`
del a7-guards.py del clon con cada conjunto candidato (los 57 archivos de la publicación menos los que se escriben después de la autoprueba) y
compara el commit hipotético (Head) y Added con los de Candidate en la autoprueba custodiada. Solo lectura del worktree (GIT_OPTIONAL_LOCKS=0);
temporales en <kit-review3>/.tmp/fse, que se borran. Uso: python -I -B find_selftest_evid.py
"""
import itertools
import json
import os
import shutil
import stat
import subprocess
import sys

CLONE = r"D:\r62-arch-a7r3"
PUB = "37ac6ca4a6f0a04268ac357764bb74f843eb3a08"
BASE = "5da2007f45160ced37b958a3a772c46ac64874ee"
EV = "docs/automation/evidence/I-62-A7/r3/"
GUARDS = os.path.join(CLONE, *(EV + "a7-guards.py").split("/"))
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
TMP = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), ".tmp", "fse")
LATE = ["q7-guards-selftest.json", "q7-guards-preview.json", "q7-guards.json", "summary.json", "preflight.md", "MANIFEST.sha256"]


def git(*a):
    return subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, check=True).stdout


cust = json.loads(git("show", "HEAD:" + EV + "q7-guards-selftest.json"))
want_head, want_added = cust["Candidate"]["Head"], cust["Candidate"]["Added"]
names = [n for n in git("ls-tree", "-r", "--name-only", PUB, "--", EV).decode().split("\n") if n]
os.makedirs(TMP, exist_ok=True)
env = dict(os.environ, TEMP=TMP, TMP=TMP, TMPDIR=TMP, GIT_OPTIONAL_LOCKS="0")
results = []
for k in (5, 6):
    for excl in itertools.combinations(LATE, k):
        d = os.path.join(TMP, "evid")
        if os.path.isdir(d):
            shutil.rmtree(d, onexc=lambda f, p, e: (os.chmod(p, stat.S_IWRITE), f(p)))
        for n in names:
            if n[len(EV):] in excl:
                continue
            dst = os.path.join(d, *n[len(EV):].split("/"))
            os.makedirs(os.path.dirname(dst), exist_ok=True)
            open(dst, "wb").write(git("show", PUB + ":" + n))
        out = os.path.join(TMP, "out.json")
        r = subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", GUARDS, "preview", "--repo", WORKTREE, "--base", BASE, "--cand",
                            os.path.join(CLONE, "docs", "initiatives", "I-62-A-7.md"), "--evid", d, "--out", out, "--tmp", os.path.join(TMP, "g")],
                           cwd=CLONE, capture_output=True, text=True, encoding="utf-8", errors="replace", env=env)
        got = json.load(open(out, encoding="utf-8")) if os.path.isfile(out) else {}
        present = sorted(set(LATE) - set(excl))
        results.append({"PresentAmongLate": present, "ExitCode": r.returncode, "Head": got.get("Head"), "Added": got.get("Added"),
                        "MatchesCustodied": got.get("Head") == want_head and got.get("Added") == want_added})
shutil.rmtree(TMP, onexc=lambda f, p, e: (os.chmod(p, stat.S_IWRITE), f(p)))
if not os.listdir(os.path.dirname(TMP)):
    os.rmdir(os.path.dirname(TMP))
print(json.dumps({"CustodiedHead": want_head, "CustodiedAdded": want_added, "Results": results}, indent=1))
