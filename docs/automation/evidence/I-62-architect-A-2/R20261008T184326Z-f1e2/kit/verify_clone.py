"""Verificación mecánica del clon limpio y del run de la re-revisión de A-2 por claude-cli, antes del lanzamiento (la identidad la verifica el
invocador, porque el revisor no tiene herramientas de ejecución).

Comprueba: HEAD = el commit de publicación; una sola rama (main); sin remotos; árbol limpio (también sin ignorados); los blobs designados; sin enlaces
simbólicos ni uniones (reparse points) en el árbol de trabajo ni entradas 120000 en el índice; core.autocrlf; commits presentes y ausentes; el run
(exactamente order.txt, prompt.md, delta.diff y A-2.d47f71b6.md; order.txt con el SHA-256 de decisiones §56; delta.diff y A-2.d47f71b6.md
re-derivados del clon; cada archivo idéntico a la copia del kit y a RunFileHashes del cierre); que no exista todavía ningún directorio de proyecto
de claude-cli para el clon ni una transcripción con el session id fijado; y una preflight de las guardas (`a2-guards.py self-test`, evidencia del
invocador: el revisor no la ejecuta) con la salida en un directorio temporal junto al kit, que se borra. Escribe clone-verification.json junto a
este script.
Uso: python -B verify_clone.py
"""
import glob
import hashlib
import json
import os
import shutil
import stat
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
CLONE, RUN = r"D:\r62-arch-a2r", r"D:\r62-arch-a2r-run"
REV = "fd411b136f888ecf301f9edaa7fcc068da8dbf22"
DELTA_BASE = "b553608ccdac188c45cef0982be0f7da8b5ab2ec"
PREV_COMMIT, PREV_BLOB = "b280f7079e128113666e85a2f437e629d0f0d99e", "d47f71b66ba86c31f857f0d8c2d2437636947af0"
OBJECT = "docs/initiatives/I-62-A-2.md"
SESSION_ID = "26a89860-a107-4135-9981-a46f95816d16"
BLOBS = {
    OBJECT: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    "docs/initiatives/I-62-architect-package-A-2.md": "5bd0fa609722d9c94aeab38b0d8b486b1d4f1606",
    "docs/automation/evidence/I-62-A2/a2-guards.py": "b72d26ea6b30b5cfceaa8140a536a08086562b24",
    "docs/automation/evidence/I-62-A2/a2-guards-result.json": "d8f4845f853fcba8fe79e468312d4845b1c2e76e",
    "docs/initiatives/I-62-proposal-v14.md": "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    "docs/initiatives/I-62-A-1.md": "c01899a72b940503bb85a0fab42bc085c603fd0f",
    "docs/initiatives/I-62-consensus-freeze.md": "0f6b860837e2478db8525e1fb30a485bc4646816",
    "docs/INITIATIVE_LIFECYCLE.md": "f19896a8f1a7c82f74bac7231636f68462a87271",
}
PRESENT = {"3bbaabef0346d4146585dbf17ac49a1d3a495b0d": "commit de la corrección de A-2", DELTA_BASE: "base del delta corregido",
           PREV_COMMIT: "commit del objeto anterior", "c9f9419dc4aed32242c59de489a8e78a8e369f9b": "base de A-2"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)"}
ORDER_SHA = "e8b003284843374ba1e8497434f375f3206da2cd9a9829f2f451ff73af06b081"
RUN_FILES = ["order.txt", "prompt.md", "delta.diff", "A-2.d47f71b6.md"]


def git(*a, check=True, binary=False):
    r = subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=not binary)
    if check and r.returncode:
        raise SystemExit("git %s: %s" % (a, r.stderr))
    return r


def sha(b):
    return hashlib.sha256(b).hexdigest()


closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
reparse, files = [], 0
for root, dirs, fs in os.walk(CLONE, followlinks=False):
    for n in dirs + fs:
        p = os.path.join(root, n)
        st = os.lstat(p)
        if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
            reparse.append(p)
    files += len(fs)
blobs = {p: git("rev-parse", REV + ":" + p).stdout.strip() for p in BLOBS}
run_listing = sorted(os.listdir(RUN))
run = {}
for n in RUN_FILES:
    b = open(os.path.join(RUN, n), "rb").read()
    k = open(os.path.join(KIT, n), "rb").read()
    run[n] = {"Bytes": len(b), "Sha256": sha(b), "IdenticalToKitCopy": b == k,
              "MatchesClosure": sha(b) == closure["RunFileHashes"][n]["Sha256"] and len(b) == closure["RunFileHashes"][n]["Bytes"]}
run["order.txt"]["MatchesAuthority"] = run["order.txt"]["Sha256"] == ORDER_SHA
delta = git("diff", DELTA_BASE, REV, "--", OBJECT, binary=True).stdout
run["delta.diff"]["Rederived"] = open(os.path.join(RUN, "delta.diff"), "rb").read() == delta
prev = git("show", PREV_COMMIT + ":" + OBJECT, binary=True).stdout
run["A-2.d47f71b6.md"]["Rederived"] = open(os.path.join(RUN, "A-2.d47f71b6.md"), "rb").read() == prev
run["A-2.d47f71b6.md"]["Blob"] = git("hash-object", os.path.join(RUN, "A-2.d47f71b6.md")).stdout.strip()
pb = open(os.path.join(RUN, "prompt.md"), "rb").read()
run["prompt.md"].update({"FinalNewline": pb.endswith(b"\n"), "CarriageReturns": pb.count(b"\r"), "Utf8": True})
pb.decode("utf-8")
projects = os.path.join(os.path.expanduser("~"), ".claude", "projects")
project_dir = os.path.join(projects, "D--r62-arch-a2r")
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
doc = {
    "Clone": CLONE,
    "Commands": [
        'git clone --no-local -c core.autocrlf=false --single-branch --branch architecture/portabilidad-coordinador-principal "D:\\Documentos\\Codex\\Calculadora de racks" D:\\r62-arch-a2r',
        "git -C D:\\r62-arch-a2r checkout -q -B main " + REV,
        "git -C D:\\r62-arch-a2r branch -D architecture/portabilidad-coordinador-principal",
        "git -C D:\\r62-arch-a2r remote remove origin",
        "git -C D:\\r62-arch-a2r diff %s %s -- %s  > D:\\r62-arch-a2r-run\\delta.diff (bytes literales)" % (DELTA_BASE, REV, OBJECT),
        "git -C D:\\r62-arch-a2r show %s:%s  > D:\\r62-arch-a2r-run\\A-2.d47f71b6.md (bytes literales)" % (PREV_COMMIT[:8], OBJECT),
        "copia byte a byte de kit/prompt.md → D:\\r62-arch-a2r-run\\prompt.md; order.txt la dejó la sesión principal (cuerpo de decisiones §56)",
    ],
    "Head": git("rev-parse", "HEAD").stdout.strip(),
    "Branches": git("branch", "--list", "--format=%(refname:short)").stdout.split(),
    "Remotes": git("remote").stdout.split(),
    "StatusPorcelain": git("status", "--porcelain").stdout,
    "StatusIgnored": git("status", "--porcelain", "--ignored").stdout,
    "CoreAutocrlf": git("config", "--get", "core.autocrlf", check=False).stdout.strip(),
    "WorktreeList": git("worktree", "list").stdout.strip(),
    "Tags": len(git("tag", "--list").stdout.split()),
    "Blobs": {p: {"Expected": BLOBS[p], "Got": blobs[p], "Match": blobs[p] == BLOBS[p]} for p in BLOBS},
    "CommitsPresent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "ClaudeProjectDirForClone": {"Path": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a2r", "Exists": os.path.exists(project_dir)},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
}
tmp = os.path.join(os.path.dirname(KIT), ".ex1")
os.makedirs(tmp, exist_ok=True)
out = os.path.join(tmp, "a2-selftest.json")
env = dict(os.environ, TEMP=tmp, TMP=tmp, TMPDIR=tmp)
r = subprocess.run([sys.executable, "-I", "-B", "docs/automation/evidence/I-62-A2/a2-guards.py", "self-test", "--out", out], cwd=CLONE,
                   capture_output=True, text=True, env=env)
pre = {"Command": "python -I -B docs/automation/evidence/I-62-A2/a2-guards.py self-test --out <temporal junto al kit> (cwd = el clon; evidencia del invocador)",
       "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:]}
try:
    d = json.load(open(out, encoding="utf-8"))
    g = d.get("G5") or {}
    muts = g.get("Mutants") or []
    pre.update({"Result": d.get("Result"), "Vectors": g.get("Total"), "VectorsPassed": g.get("Passed"), "Mutants": len(muts),
                "MutantsKilled": sum(1 for m in muts if m.get("Result") == "PASS"), "RulesMissing": g.get("RulesMissing"),
                "OutputSha256": sha(open(out, "rb").read())})
except (OSError, ValueError) as e:
    pre["OutputError"] = str(e)
shutil.rmtree(tmp)
doc["GuardsSelfTestPreflight"] = pre
doc["StatusAfterPreflight"] = git("status", "--porcelain", "--ignored").stdout
doc["AllChecks"] = (doc["Head"] == REV and doc["Branches"] == ["main"] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and all(v["Match"] for v in doc["Blobs"].values()) and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and run["order.txt"]["MatchesAuthority"] and
                    run["delta.diff"]["Rederived"] and run["A-2.d47f71b6.md"]["Rederived"] and run["A-2.d47f71b6.md"]["Blob"] == PREV_BLOB and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    not doc["ClaudeProjectDirForClone"]["Exists"] and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    pre["ExitCode"] == 0 and pre.get("Result") == "PASS" and doc["StatusAfterPreflight"] == "")
text = json.dumps(doc, ensure_ascii=False, indent=1)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "Guards": {k: pre.get(k) for k in ("ExitCode", "Result", "Vectors", "VectorsPassed", "Mutants", "MutantsKilled")}}))
sys.exit(0 if doc["AllChecks"] else 1)
