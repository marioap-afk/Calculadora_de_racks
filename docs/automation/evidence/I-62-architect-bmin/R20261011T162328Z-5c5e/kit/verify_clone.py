"""Verificación mecánica del clon limpio y del run del dictamen de materialidad de B-min (decisiones §75, punto 4) por claude-cli, antes del
lanzamiento (la identidad la verifica el invocador, porque el revisor no tiene herramientas de ejecución). Adaptado del verify_clone.py del kit
de A-5; sin preflight de guardas (el objeto no trae guardas propias que custodiar; las pruebas del paquete no las ejecuta el kit).

Comprueba: kit-params.json no provisional; HEAD = CloneCommit, separado (sin ninguna rama local); sin remotos; árbol limpio (también sin
ignorados); ningún objeto fuera de la historia de HEAD y ninguna etiqueta fuera de ella; los blobs designados (los de CanonicalInputs del cierre y
el del registro de caracterización que fija la compuerta); la historia del objeto (bmin/ ausente en la base 460665ab, la base antecesora estricta
del commit del paquete, el commit del paquete antecesor o igual del commit del clon, el mismo árbol de bmin/ en los dos y los blobs de
ObjectFiles en el commit del paquete); sin enlaces simbólicos ni uniones en el árbol de trabajo ni entradas 120000 en el índice; core.autocrlf;
el run (exactamente order.txt y prompt.md; order.txt con el SHA-256 y los bytes que declara decisiones §75 en el commit; cada archivo idéntico
a la copia del kit y a RunFileHashes del cierre; prompt.md sin salto de línea final ni CR y en UTF-8); el registro de caracterización (su
SHA-256 en el clon = el de transport-gate.json); que no exista ningún directorio de proyecto de claude-cli para el clon o el run (patrón
D--r62-arch-bmin*) ni una transcripción con el session id fijado. Escribe clone-verification.json junto a este script. No escribe en el clon.
Uso: python -B verify_clone.py
"""
import glob
import hashlib
import json
import os
import re
import stat
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
CLONE, RUN = r"D:\r62-arch-bmin", r"D:\r62-arch-bmin-run"
BASE_AT_PREP = "460665abd46e4f9f75c896eceecabe11689a6a40"
PKG_DIR = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SESSION_ID = "cd0a1e5c-605d-4363-9e49-57087fd26a47"
KIT_BLOBS = {
    "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json": "541956eb8f5d154414b91338c655919ae0ade697",
}
ORDER_TEXTS = {"order.txt": ("75", "21435cd1c0821d8c934117a10e850cb9fa124a06f8dd589719aff8f5b711d630", 3288)}
RUN_FILES = ["order.txt", "prompt.md"]
BRANCH = "architecture/portabilidad-coordinador-principal"


def git(*a, check=True, binary=False):
    r = subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=not binary, encoding=None if binary else "utf-8")
    if check and r.returncode:
        raise SystemExit("git %s: %s" % (a, r.stderr))
    return r


def ok(*a):
    return git(*a, check=False).returncode == 0


def sha(b):
    return hashlib.sha256(b).hexdigest()


def dec_section(rev, num):
    L = git("show", rev + ":docs/automation/decisions/I-62.md").stdout.split("\n")
    start = next(i for i, l in enumerate(L) if l.startswith("## %s. " % num))
    end = next((i for i in range(start + 1, len(L)) if L[i].startswith("## ")), len(L))
    return " ".join(L[start:end])


params = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
assert params.get("Provisional") is False and params.get("Clone") == CLONE and params.get("Run") == RUN, "kit-params.json provisional o de otro clon"
REV, PKG = params["CloneCommit"], params["PkgCommit"]
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
obj_at_pkg = {p: {"Expected": b, "Got": git("rev-parse", PKG + ":" + p, check=False).stdout.strip()} for p, b in closure["ObjectFiles"].items()}
pkg_names = sorted(n for n in git("ls-tree", "-r", "--name-only", PKG, "--", PKG_DIR + "/").stdout.split("\n") if n)
run_listing = sorted(os.listdir(RUN))
run = {}
for n in RUN_FILES:
    b = open(os.path.join(RUN, n), "rb").read()
    k = open(os.path.join(KIT, n), "rb").read()
    run[n] = {"Bytes": len(b), "Sha256": sha(b), "IdenticalToKitCopy": b == k,
              "MatchesClosure": sha(b) == closure["RunFileHashes"][n]["Sha256"] and len(b) == closure["RunFileHashes"][n]["Bytes"]}
for n, (num, want_sha, size) in ORDER_TEXTS.items():
    text = dec_section(REV, num)
    run[n]["MatchesDecisions"] = (run[n]["Sha256"] == want_sha and ("`%s`" % want_sha) in text and run[n]["Bytes"] == size and
                                  ("%d %03d bytes" % (size // 1000, size % 1000)) in text)
pb = open(os.path.join(RUN, "prompt.md"), "rb").read()
pb.decode("utf-8")
run["prompt.md"].update({"FinalNewline": pb.endswith(b"\n"), "CarriageReturns": pb.count(b"\r"), "Utf8": True})
char_path = gate["Characterization"]["Path"]
char_in_clone = os.path.normcase(os.path.abspath(char_path)).startswith(os.path.normcase(CLONE) + os.sep)
char_sha = sha(open(char_path, "rb").read())
projects = os.path.join(os.path.expanduser("~"), ".claude", "projects")
project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-bmin*")))
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
doc = {
    "Clone": CLONE, "CloneCommit": REV, "PkgCommit": PKG, "BaseAtPreparation": BASE_AT_PREP,
    "Commands": params.get("Commands"),
    "Head": git("rev-parse", "HEAD").stdout.strip(),
    "Detached": git("rev-parse", "--symbolic-full-name", "HEAD").stdout.strip() == "HEAD",
    "LocalBranches": git("for-each-ref", "--format=%(refname)", "refs/heads").stdout.split(),
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
    "Object": {"Dir": PKG_DIR, "FilesAtPkgCommit": pkg_names, "FilesMatchClosure": pkg_names == sorted(closure["ObjectFiles"]),
               "BlobsAtPkgCommit": obj_at_pkg,
               "AbsentAtBase": not ok("cat-file", "-e", BASE_AT_PREP + ":" + PKG_DIR),
               "BaseStrictAncestorOfPkg": ok("merge-base", "--is-ancestor", BASE_AT_PREP, PKG) and BASE_AT_PREP != PKG,
               "PkgAncestorOrEqualOfClone": ok("merge-base", "--is-ancestor", PKG, REV),
               "TreeAtPkg": git("rev-parse", PKG + ":" + PKG_DIR).stdout.strip(), "TreeAtClone": git("rev-parse", REV + ":" + PKG_DIR).stdout.strip()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "TransportCharacterization": {"Path": char_path, "InClone": char_in_clone, "Sha256": char_sha,
                                  "MatchesGate": char_sha == gate["Characterization"]["Sha256"], "GateStatus": gate.get("Status")},
    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-bmin*", "Found": project_dirs},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
    "ClosureIdentity": {"AuthorityRevision": closure["AuthorityRevision"], "PkgCommit": closure["PkgCommit"], "Provisional": closure.get("Provisional")},
}
o = doc["Object"]
doc["AllChecks"] = (doc["Head"] == REV and doc["Detached"] and doc["LocalBranches"] == [] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and doc["CommitsReachableFromAllRefs"] == doc["CommitsReachableFromHead"] and
                    not doc["TagsNotMergedIntoHead"] and all(v["Match"] for v in doc["Blobs"].values()) and
                    o["FilesMatchClosure"] and all(v["Expected"] == v["Got"] for v in obj_at_pkg.values()) and o["AbsentAtBase"] and
                    o["BaseStrictAncestorOfPkg"] and o["PkgAncestorOrEqualOfClone"] and o["TreeAtPkg"] == o["TreeAtClone"] and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and
                    all(run[n]["MatchesDecisions"] for n in ORDER_TEXTS) and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    char_in_clone and doc["TransportCharacterization"]["MatchesGate"] and gate.get("Status") in ("MEASURED", "ACCEPTED") and
                    not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    closure["AuthorityRevision"] == REV and closure["PkgCommit"] == PKG and closure.get("Provisional") is False)
text = json.dumps(doc, ensure_ascii=False, indent=1)
text = re.sub(r'(?i)[a-z]:(?:\\\\|/)+users(?:\\\\|/)+[^\\\\/"\s]+', "%USERPROFILE%", text)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "PkgCommit": PKG, "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "ObjectFiles": len(pkg_names)}))
sys.exit(0 if doc["AllChecks"] else 1)
