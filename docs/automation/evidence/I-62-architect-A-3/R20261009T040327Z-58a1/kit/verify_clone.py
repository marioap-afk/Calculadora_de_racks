"""Verificación mecánica del clon limpio y del run de la revisión formal de A-3 por claude-cli, antes del lanzamiento (la identidad la verifica el
invocador, porque el revisor no tiene herramientas de ejecución).

Comprueba: HEAD = el commit del recibo de publicación; una sola rama (main); sin remotos; árbol limpio (también sin ignorados); los blobs designados
(los de CanonicalInputs del cierre y los que fija el kit); sin enlaces simbólicos ni uniones (reparse points) en el árbol de trabajo ni entradas
120000 en el índice; core.autocrlf; commits presentes y ausentes (entre los ausentes, 3ef58efa, decisiones §58, posterior al recibo, que entró con
la rama y se eliminó del clon); el run (exactamente order.txt, prompt.md, order-s56.txt y order-s57.txt; order.txt con el SHA-256 de la disposición
§58; order-s56.txt y order-s57.txt con el SHA-256 y los bytes que declaran decisiones §56 y §57 en el commit; cada archivo idéntico a la copia del
kit y a RunFileHashes del cierre; prompt.md sin salto de línea final ni CR); el registro de caracterización que fija la compuerta (su SHA-256 en el
clon = el de transport-gate.json); que no exista todavía ningún directorio de proyecto de claude-cli para el clon o el run ni una transcripción con
el session id fijado; y una preflight de las guardas (`a3-guards.py self-test --a3 docs/initiatives/I-62-A-3.md`, evidencia del invocador: el
revisor no la ejecuta), con la salida y los temporales en <kit-a3>/.tmp/ex1, que se borra, y su G5 comparado con el de a3-selftest.json custodiado.
Escribe clone-verification.json junto a este script.
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
CLONE, RUN = r"D:\r62-arch-a3", r"D:\r62-arch-a3-run"
REV = "492885254b58203f0bc0099345db2998b25d2a2b"
OBJECT = "docs/initiatives/I-62-A-3.md"
SESSION_ID = "ae3590eb-b508-4949-84da-8aa7340df993"
KIT_BLOBS = {   # además de los de CanonicalInputs: los que el kit fija por su cuenta
    "docs/initiatives/I-62-architect-package-A-2.md": "5bd0fa609722d9c94aeab38b0d8b486b1d4f1606",
    "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json": "541956eb8f5d154414b91338c655919ae0ade697",
}
PRESENT = {"91e29886a4870dd673372c66453d76ea503a22b9": "commit de publicación de A-3", "76b48e785d87af5591f4363246751b7eb1182506": "A3_BASE (padre)",
           "b088421649204742ff682e1eb1ebdafc762823d0": "base de preparación de A-3", "322cf8a8a4b8f875cde0d3b564e600b20b1b06aa": "acuerdo de A-2",
           "bb0d5522e8411f66a51fdfb3f1f0d0514b737453": "base de main", "d2d74c14f5e661c7ce77a81aae964dfcb4abdf40": "caracterización de claude-cli",
           "3bbaabef0346d4146585dbf17ac49a1d3a495b0d": "publicación de la A-2 corregida"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)",
          "3ef58efa8922d184d5fe5059c540429d907d4aa4": "decisiones §58 (posterior al recibo; eliminado del clon)"}
ORDER_SHA = "3ed7e135eb914a7307bdc83bd9da6cf4dd4cf68aadc3e04d71a619e82b709a1c"
ORDER_TEXTS = {"order-s56.txt": ("56", "e8b003284843374ba1e8497434f375f3206da2cd9a9829f2f451ff73af06b081", 8562),
               "order-s57.txt": ("57", "00a13ae527863117472bc7356458c849a8ef6f6e6255b417ee736dc3428023a3", 3654)}
RUN_FILES = ["order.txt", "prompt.md", "order-s56.txt", "order-s57.txt"]
TMP = os.path.join(os.path.dirname(KIT), ".tmp", "ex1")


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
run_listing = sorted(os.listdir(RUN))
run = {}
for n in RUN_FILES:
    b = open(os.path.join(RUN, n), "rb").read()
    k = open(os.path.join(KIT, n), "rb").read()
    run[n] = {"Bytes": len(b), "Sha256": sha(b), "IdenticalToKitCopy": b == k,
              "MatchesClosure": sha(b) == closure["RunFileHashes"][n]["Sha256"] and len(b) == closure["RunFileHashes"][n]["Bytes"]}
run["order.txt"]["MatchesAuthority"] = run["order.txt"]["Sha256"] == ORDER_SHA
for n, (num, want_sha, size) in ORDER_TEXTS.items():
    text = dec_section(num)
    run[n]["MatchesDecisions"] = (run[n]["Sha256"] == want_sha and ("`%s`" % want_sha) in text and run[n]["Bytes"] == size and
                                  ("%d %03d bytes" % (size // 1000, size % 1000)) in text)
pb = open(os.path.join(RUN, "prompt.md"), "rb").read()
run["prompt.md"].update({"FinalNewline": pb.endswith(b"\n"), "CarriageReturns": pb.count(b"\r"), "Utf8": True})
pb.decode("utf-8")
char_path = gate["Characterization"]["Path"]
char_in_clone = os.path.normcase(os.path.abspath(char_path)).startswith(os.path.normcase(CLONE) + os.sep)
char_sha = sha(open(char_path, "rb").read())
projects = os.path.join(os.path.expanduser("~"), ".claude", "projects")
project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a3*")))
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
doc = {
    "Clone": CLONE,
    "Commands": [
        'git clone -q --no-local -c core.autocrlf=false --single-branch --branch architecture/portabilidad-coordinador-principal '
        '"%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal" D:\\r62-arch-a3',
        "git -C D:\\r62-arch-a3 checkout -q -B main " + REV,
        "git -C D:\\r62-arch-a3 branch -D architecture/portabilidad-coordinador-principal   (la rama estaba en 3ef58efa, decisiones §58, posterior al recibo)",
        "git -C D:\\r62-arch-a3 remote remove origin",
        "git -C D:\\r62-arch-a3 reflog expire --expire=now --all; git -C D:\\r62-arch-a3 gc -q --prune=now   (3ef58efa queda fuera del clon)",
        "copia byte a byte de kit/order.txt (= cuerpo §58), kit/order-s56.txt, kit/order-s57.txt y kit/prompt.md → D:\\r62-arch-a3-run\\",
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
    "Blobs": {p: {"Expected": want[p], "Got": blobs[p], "Match": blobs[p] == want[p]} for p in want},
    "CommitsPresent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "TransportCharacterization": {"Path": char_path, "InClone": char_in_clone, "Sha256": char_sha,
                                  "MatchesGate": char_sha == gate["Characterization"]["Sha256"], "GateStatus": gate.get("Status")},
    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a3*", "Found": project_dirs},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
}
os.makedirs(TMP, exist_ok=True)
out = os.path.join(TMP, "a3-selftest.json")
env = dict(os.environ, TEMP=TMP, TMP=TMP, TMPDIR=TMP)
r = subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", "docs/automation/evidence/I-62-A3/a3-guards.py", "self-test", "--a3", OBJECT, "--out", out],
                   cwd=CLONE, capture_output=True, text=True, encoding="utf-8", errors="replace", env=env)
pre = {"Command": "python -I -B -X utf8 docs/automation/evidence/I-62-A3/a3-guards.py self-test --a3 docs/initiatives/I-62-A-3.md --out <temporal "
                  "junto al kit> (cwd = el clon; evidencia del invocador)", "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:]}
try:
    d = json.load(open(out, encoding="utf-8"))
    custodied = json.loads(git("show", REV + ":docs/automation/evidence/I-62-A3/a3-selftest.json").stdout)
    g = d.get("G5") or {}
    pre.update({"Result": d.get("Result"), "A3Text": d.get("A3Text"), "A3TextBlob": d.get("A3TextBlob"), "Vectors": g.get("Total"),
                "VectorsPassed": g.get("Passed"), "VectorsPassedReasonBlind": g.get("PassedReasonBlind"), "Mutants": g.get("MutantsTotal"),
                "MutantsKilled": g.get("MutantsKilled"), "RulesMissing": g.get("RulesMissing"),
                "G5EqualsCustodied": g == custodied.get("G5"), "WholeEqualsCustodied": d == custodied,
                "OutputSha256": sha(open(out, "rb").read())})
except (OSError, ValueError) as e:
    pre["OutputError"] = str(e)
shutil.rmtree(TMP)
if not os.listdir(os.path.dirname(TMP)):
    os.rmdir(os.path.dirname(TMP))
doc["GuardsSelfTestPreflight"] = pre
doc["StatusAfterPreflight"] = git("status", "--porcelain", "--ignored").stdout
doc["AllChecks"] = (doc["Head"] == REV and doc["Branches"] == ["main"] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and all(v["Match"] for v in doc["Blobs"].values()) and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and run["order.txt"]["MatchesAuthority"] and
                    all(run[n]["MatchesDecisions"] for n in ORDER_TEXTS) and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    char_in_clone and doc["TransportCharacterization"]["MatchesGate"] and
                    not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    pre["ExitCode"] == 0 and pre.get("Result") == "PASS" and pre.get("A3TextBlob") == closure["ObjectBlob"] and
                    pre.get("G5EqualsCustodied") is True and doc["StatusAfterPreflight"] == "")
text = json.dumps(doc, ensure_ascii=False, indent=1)
text = re.sub(r'[A-Za-z]:(?:\\\\|/)Users(?:\\\\|/)[^\\/"]+', "%USERPROFILE%", text)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "Guards": {k: pre.get(k) for k in ("ExitCode", "Result", "Vectors", "VectorsPassed", "Mutants", "MutantsKilled", "G5EqualsCustodied",
                                                      "WholeEqualsCustodied")}}))
sys.exit(0 if doc["AllChecks"] else 1)
