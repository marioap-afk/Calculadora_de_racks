"""Verificación mecánica del clon limpio y del run de la re-revisión formal de A-4 (corrección 2, blob 0d954376) por claude-cli, antes del
lanzamiento (la identidad la verifica el invocador, porque el revisor no tiene herramientas de ejecución).

Comprueba: HEAD = el commit del recibo (cca8c60e, decisiones §62); una sola rama (main); sin remotos; árbol limpio (también sin ignorados); ningún
objeto fuera de la historia de HEAD; los blobs designados (los de CanonicalInputs del cierre y los que fija el kit); la historia de la candidata
(27ffa26b en la primera publicación, 7d863219 en la corrección 1 y en la custodia de la revisión 1, 0d954376 en la corrección 2); sin enlaces
simbólicos ni uniones en el árbol de trabajo ni entradas 120000 en el índice; core.autocrlf; commits presentes y ausentes; el run (exactamente
order.txt, prompt.md, order-s61.txt, order-s60.txt, delta.diff y A-4.7d863219.md; los tres textos de las órdenes con el SHA-256 y los bytes que
declaran decisiones §62, §61 y §60 en el commit; delta.diff = salida literal de `git diff a5b50c68 a3332495 -- <objeto>` y A-4.7d863219.md =
salida literal de `git show a5b50c68:<objeto>` en el clon; cada archivo idéntico a la copia del kit y a RunFileHashes del cierre; prompt.md sin
salto de línea final ni CR); el registro de caracterización que fija la compuerta (su SHA-256 en el clon = el de transport-gate.json); que no exista
ningún directorio de proyecto de claude-cli para el clon o el run (patrón D--r62-arch-a4r*) ni una transcripción con el session id fijado; y una
preflight de las guardas (`a4-guards.py self-test`, evidencia del invocador: el revisor no la ejecuta), con la salida y los temporales en
<kit-a4r>/.tmp/ex1, que se borra, comparada con a4-selftest.json custodiado. Las guardas de A-4 necesitan `origin/main` y los commits del Freeze y
de V14 (b64a3b64, 4c617e82), anteriores al rebase, que un clon de una sola rama sin remoto no tiene: la preflight ejecuta el a4-guards.py del clon
(blob 6595d873) con --repo = el worktree de la rama, solo lectura (rev-parse, diff entre commits, merge-base, cat-file y config; ninguna escritura),
y con --a4 y --pkg = los archivos del clon, como se generó el resultado custodiado (TextSource = args). Escribe clone-verification.json junto a
este script.
Uso: python -B verify_clone.py
"""
import glob
import hashlib
import json
import os
import re
import shutil
import stat
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
CLONE, RUN = r"D:\r62-arch-a4r", r"D:\r62-arch-a4r-run"
REV = "cca8c60e4acfea498844c5d5f8ea14b807fbd4f3"
OBJECT = "docs/initiatives/I-62-A-4.md"
SESSION_ID = "9eb98aef-1bc0-44b9-9dc8-7e963f22bdf2"
PKG = "docs/initiatives/I-62-architect-package-A-4.md"
REVIEW1, CORRECTION2 = "a5b50c689ef33aa7b6fa7f7949fd72f58202e369", "a33324957f8778dd286a5a871881d403fd5a2d14"
KIT_BLOBS = {
    "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json": "541956eb8f5d154414b91338c655919ae0ade697",
}
AT_COMMIT = {("4710084b7589e27c511572d409b8742b75cfcbf4", OBJECT): "27ffa26b35ecacfae083bf9d360fe8460b8d5564",
             ("7f065a3a56102b8034247c7c64514e3f256f7b74", OBJECT): "7d863219a271b5ec427ef8669435826e19be8f11",
             (REVIEW1, OBJECT): "7d863219a271b5ec427ef8669435826e19be8f11",
             (REVIEW1, PKG): "085f30f602532a619734b47b6b8d60035d8fb278",
             (CORRECTION2, OBJECT): "0d9543761e3e3a45a94338776f7c1ba763224ab3",
             (CORRECTION2, PKG): "59052b847ccc13e8be539189ae1b77dc7f9d3623",
             (REVIEW1, "docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json"): "8477374495d809bed64283d8af2976e3ce2929e8"}
PRESENT = {CORRECTION2: "corrección 2 de A-4 (0d954376)", REVIEW1: "custodia de la revisión 1 (7d863219)",
           "02be0c34efc6125627ad4b31978fb349acc724c8": "recibo de la revisión 1",
           "7f065a3a56102b8034247c7c64514e3f256f7b74": "corrección 1 de A-4 (7d863219)", "4710084b7589e27c511572d409b8742b75cfcbf4": "primera publicación (27ffa26b)",
           "524b293e4667810f485c64d6cbb545d5b2dd8db5": "base de preparación (blobs del paquete §2)", "f504f6a9bf34847dd12e973c8d5545c1b6db9e52": "acuerdo de A-3",
           "c8d69fcb35e18ddc660152dca943684a351a7321": "base de A4-1 y A4-2", "d2d74c14f5e661c7ce77a81aae964dfcb4abdf40": "caracterización de claude-cli"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)"}
ORDER_TEXTS = {"order.txt": ("62", "560e043ddec6c64d5ab0ce5ef9025603cb2723b26b017f94924b9dd06479f952", 3035),
               "order-s61.txt": ("61", "c82456877990c172110d577d21cc592ed87ebc94dea20387eecb4061c17634a7", 3196),
               "order-s60.txt": ("60", "3a7257e16c955e586b2e6c300f42ba20079f0b29481dbb63f461c2a1c2217699", 2047)}
RUN_FILES = ["order.txt", "prompt.md", "order-s61.txt", "order-s60.txt", "delta.diff", "A-4.7d863219.md"]
GIT_RUN_FILES = {"delta.diff": ["diff", REVIEW1, CORRECTION2, "--", OBJECT], "A-4.7d863219.md": ["show", REVIEW1 + ":" + OBJECT]}
TMP = os.path.join(os.path.dirname(KIT), ".tmp", "ex1")
WORKTREE = os.path.join(os.path.expanduser("~"), ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # solo lectura


def git(*a, check=True, binary=False):
    r = subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=not binary, encoding=None if binary else "utf-8")
    if check and r.returncode:
        raise SystemExit("git %s: %s" % (a, r.stderr))
    return r


def sha(b):
    return hashlib.sha256(b).hexdigest()


def dec_section(num):
    L = git("show", REV + ":docs/automation/decisions/I-62.md").stdout.split("\n")
    start = next(i for i, l in enumerate(L) if l.startswith("## %s. " % num))
    end = next((i for i in range(start + 1, len(L)) if L[i].startswith("## ")), len(L))
    return "\n".join(L[start:end])


closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
gate = json.load(open(os.path.join(KIT, "transport-gate.json"), encoding="utf-8"))
reparse, files = [], 0
for root, dirs, fs in os.walk(CLONE, followlinks=False):
    for n in dirs + fs:
        p = os.path.join(root, n)
        st = os.lstat(p)
        if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
            reparse.append(p)
    files += len(fs)
want = dict({r["Path"]: r["Blob"] for r in closure["CanonicalInputs"]}, **KIT_BLOBS)
blobs = {p: git("rev-parse", REV + ":" + p).stdout.strip() for p in want}
at_commit = {"%s:%s" % (c[:8], p): {"Expected": b, "Got": git("rev-parse", c + ":" + p).stdout.strip()} for (c, p), b in AT_COMMIT.items()}
run_listing = sorted(os.listdir(RUN))
run = {}
for n in RUN_FILES:
    b = open(os.path.join(RUN, n), "rb").read()
    k = open(os.path.join(KIT, n), "rb").read()
    run[n] = {"Bytes": len(b), "Sha256": sha(b), "IdenticalToKitCopy": b == k,
              "MatchesClosure": sha(b) == closure["RunFileHashes"][n]["Sha256"] and len(b) == closure["RunFileHashes"][n]["Bytes"]}
for n, (num, want_sha, size) in ORDER_TEXTS.items():
    text = dec_section(num)
    run[n]["MatchesDecisions"] = (run[n]["Sha256"] == want_sha and ("`%s`" % want_sha) in text and run[n]["Bytes"] == size and
                                  ("%d %03d bytes" % (size // 1000, size % 1000)) in text)
for n, cmd in GIT_RUN_FILES.items():
    run[n]["EqualsGitOutput"] = {"Command": "git " + " ".join(cmd), "Equal": open(os.path.join(RUN, n), "rb").read() == git(*cmd, binary=True).stdout}
pb = open(os.path.join(RUN, "prompt.md"), "rb").read()
run["prompt.md"].update({"FinalNewline": pb.endswith(b"\n"), "CarriageReturns": pb.count(b"\r"), "Utf8": True})
pb.decode("utf-8")
char_path = gate["Characterization"]["Path"]
char_in_clone = os.path.normcase(os.path.abspath(char_path)).startswith(os.path.normcase(CLONE) + os.sep)
char_sha = sha(open(char_path, "rb").read())
projects = os.path.join(os.path.expanduser("~"), ".claude", "projects")
project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a4r*")))
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
doc = {
    "Clone": CLONE,
    "Commands": [
        'git clone -q --no-local -c core.autocrlf=false --single-branch --branch architecture/portabilidad-coordinador-principal '
        '"%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal" D:\\r62-arch-a4r',
        "git -C D:\\r62-arch-a4r checkout -q -B main " + REV,
        "git -C D:\\r62-arch-a4r branch -D architecture/portabilidad-coordinador-principal",
        "git -C D:\\r62-arch-a4r remote remove origin",
        "git -C D:\\r62-arch-a4r reflog expire --expire=now --all; git -C D:\\r62-arch-a4r gc -q --prune=now   (poda los commits posteriores al recibo)",
        "git -C D:\\r62-arch-a4r diff a5b50c68 a3332495 -- docs/initiatives/I-62-A-4.md > kit/delta.diff; "
        "git -C D:\\r62-arch-a4r show a5b50c68:docs/initiatives/I-62-A-4.md > kit/A-4.7d863219.md   (bytes literales)",
        "copia byte a byte de kit/order.txt (= cuerpo §62), kit/order-s61.txt (= cuerpo §61), kit/order-s60.txt (= cuerpo §60), kit/delta.diff, "
        "kit/A-4.7d863219.md y kit/prompt.md → D:\\r62-arch-a4r-run\\",
    ],
    "Head": git("rev-parse", "HEAD").stdout.strip(),
    "Branches": git("branch", "--list", "--format=%(refname:short)").stdout.split(),
    "Remotes": git("remote").stdout.split(),
    "StatusPorcelain": git("status", "--porcelain").stdout,
    "StatusIgnored": git("status", "--porcelain", "--ignored").stdout,
    "CoreAutocrlf": git("config", "--get", "core.autocrlf", check=False).stdout.strip(),
    "WorktreeList": git("worktree", "list").stdout.strip(),
    "Tags": len(git("tag", "--list").stdout.split()),
    "TagsNotMergedIntoHead": git("tag", "--no-merged", "HEAD").stdout.split(),
    "CommitsReachableFromAllRefs": int(git("rev-list", "--all", "--count").stdout.strip()),
    "CommitsReachableFromHead": int(git("rev-list", "HEAD", "--count").stdout.strip()),
    "Blobs": {p: {"Expected": want[p], "Got": blobs[p], "Match": blobs[p] == want[p]} for p in want},
    "BlobsAtCommits": at_commit,
    "CommitsPresent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "TransportCharacterization": {"Path": char_path, "InClone": char_in_clone, "Sha256": char_sha,
                                  "MatchesGate": char_sha == gate["Characterization"]["Sha256"], "GateStatus": gate.get("Status")},
    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a4r*", "Found": project_dirs},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
}
os.makedirs(TMP, exist_ok=True)
out = os.path.join(TMP, "a4-selftest.json")
env = dict(os.environ, TEMP=TMP, TMP=TMP, TMPDIR=TMP)
wt_status_before = subprocess.run(["git", "-C", WORKTREE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
r = subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", "docs/automation/evidence/I-62-A4/a4-guards.py", "self-test", "--repo", WORKTREE,
                    "--a4", os.path.join(CLONE, OBJECT), "--pkg", os.path.join(CLONE, "docs/initiatives/I-62-architect-package-A-4.md"),
                    "--out", out, "--tmp", TMP], cwd=CLONE, capture_output=True, text=True, encoding="utf-8", errors="replace", env=env)
pre = {"Command": "python -I -B -X utf8 docs/automation/evidence/I-62-A4/a4-guards.py self-test --repo <worktree> --a4 <clon> --pkg <clon> --out <temporal> "
                  "--tmp <temporal> (script del clon; --repo = %USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal, solo "
                  "lectura; --a4 y --pkg = los archivos del clon; evidencia del invocador)",
       "WorktreeHeadAtPreflight": wt_status_before, "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:], "StderrTail": r.stderr.strip()[-400:]}
try:
    d = json.load(open(out, encoding="utf-8"))
    custodied = json.loads(git("show", REV + ":docs/automation/evidence/I-62-A4/a4-selftest.json").stdout)
    tot = d.get("Total") or {}
    pre.update({"Result": d.get("Result"), "A4TextBlob": d.get("A4TextBlob"), "PkgTextBlob": d.get("PkgTextBlob"),
                "Mutants": tot.get("Mutants"), "MutantsKilled": tot.get("Killed"),
                "WholeEqualsCustodied": d == custodied,
                "DifferingTopLevelKeys": sorted(k for k in set(d) | set(custodied) if d.get(k) != custodied.get(k)),
                "OutputSha256": sha(open(out, "rb").read())})
except (OSError, ValueError) as e:
    pre["OutputError"] = str(e)
shutil.rmtree(TMP)
if not os.listdir(os.path.dirname(TMP)):
    os.rmdir(os.path.dirname(TMP))
doc["GuardsSelfTestPreflight"] = pre
doc["StatusAfterPreflight"] = git("status", "--porcelain", "--ignored").stdout
doc["AllChecks"] = (doc["Head"] == REV and doc["Branches"] == ["main"] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and doc["CommitsReachableFromAllRefs"] == doc["CommitsReachableFromHead"] and
                    not doc["TagsNotMergedIntoHead"] and all(v["Match"] for v in doc["Blobs"].values()) and
                    all(v["Expected"] == v["Got"] for v in at_commit.values()) and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and
                    all(run[n]["MatchesDecisions"] for n in ORDER_TEXTS) and all(run[n]["EqualsGitOutput"]["Equal"] for n in GIT_RUN_FILES) and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    char_in_clone and doc["TransportCharacterization"]["MatchesGate"] and
                    not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    pre["ExitCode"] == 0 and pre.get("Result") == "PASS" and pre.get("A4TextBlob") == closure["ObjectBlob"] and
                    pre.get("PkgTextBlob") == closure["PackageBlob"] and pre.get("WholeEqualsCustodied") is True and doc["StatusAfterPreflight"] == "")
text = json.dumps(doc, ensure_ascii=False, indent=1)
text = re.sub(r'(?i)[a-z]:(?:\\|/)+users(?:\\|/)+[^\\/"\s]+', "%USERPROFILE%", text)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "Guards": {k: pre.get(k) for k in ("ExitCode", "Result", "Mutants", "MutantsKilled", "WholeEqualsCustodied", "DifferingTopLevelKeys")}}))
sys.exit(0 if doc["AllChecks"] else 1)
