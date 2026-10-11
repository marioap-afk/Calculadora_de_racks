"""Verificación mecánica del clon limpio y del run de la primera revisión formal de A-6 (decisiones §77, punto 4) por claude-cli, antes del
lanzamiento (la identidad la verifica el invocador, porque el revisor no tiene herramientas de ejecución), con la reproducción de las
comprobaciones estáticas de A-6 (§77, punto 4: «el kit de revisión independiente de A-6 y sus comprobaciones estáticas»). Adaptado del
verify_clone.py del kit de B-min (clon y run) y del de A-5 (preflight de guardas).

Comprueba: kit-params.json no provisional; HEAD = CloneCommit, separado (sin ninguna rama local); sin remotos; árbol limpio (también sin
ignorados); ningún objeto fuera de la historia de HEAD y ninguna etiqueta fuera de ella; los blobs designados (los de CanonicalInputs del cierre y
el del registro de caracterización que fija la compuerta); la historia del objeto (ausente en 108d4ca1; eac473c0 en la publicación 7b7aba46, en
la custodia de las guardas 0c817e1d y en el commit del clon; a6-guards.py, a6-selftest.json y preflight.md sin cambio desde 7b7aba46; el
resultado `run` sin cambio desde 0c817e1d); commits presentes y ausentes (los del Freeze y de V14, anteriores al rebase, no están en el clon); sin
enlaces simbólicos ni uniones en el árbol de trabajo ni entradas 120000 en el índice; core.autocrlf; el run (exactamente order.txt y prompt.md;
order.txt con el SHA-256 y los bytes que declara decisiones §77 en el commit; cada archivo idéntico a la copia del kit y a RunFileHashes del
cierre; prompt.md sin salto de línea final ni CR y en UTF-8); el registro de caracterización (su SHA-256 en el clon = el de transport-gate.json);
que no exista ningún directorio de proyecto de claude-cli para el clon o el run (patrón D--r62-arch-a6*) ni una transcripción con el session id
fijado; y una preflight de las guardas de A-6 (evidencia del invocador: el revisor no la ejecuta), con la salida y los temporales en
<kit-a6-review>/.tmp/ex1, que se borra. Las guardas de A-6 (G1) comparan blobs en el commit del Freeze (b64a3b64), anterior al rebase, que un clon
de una sola rama no tiene: como en los kits de A-4 y A-5, la preflight ejecuta el a6-guards.py del clon (blob c5cc6168) con --repo = el worktree de
la rama, solo lectura (show, cat-file, rev-parse, merge-base, diff entre commits y config, con GIT_OPTIONAL_LOCKS=0; ninguna escritura; tampoco
`git status`), y con --a6 = el archivo del clon:
  * `a6-guards.py run --repo <worktree> --base 108d4ca1 --head 7b7aba46`: su salida, entera, igual a a6-guards-result-run.json custodiado;
  * `a6-guards.py self-test --repo <worktree> --a6 <A-6 del clon> --base 108d4ca1`: Result PASS, ToolBlob y A6TextBlob del commit, todos los
    mutantes muertos, e igual entera a a6-selftest.json custodiado (misma base, así que también `Repo`).
Las guardas escriben solo --out y su --tmp; se ejecutan con el intérprete aislado (-I -B) y el cwd en el clon, que se comprueba limpio al
terminar. No ejecuta el paquete B-min ni sus pruebas. Escribe clone-verification.json junto a este script. No escribe en el clon ni en el worktree.
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
CLONE, RUN = r"D:\r62-arch-a6", r"D:\r62-arch-a6-run"
OBJECT, OBJECT_BLOB = "docs/initiatives/I-62-A-6.md", "eac473c0d2e58cf1c4594f48a8816b3833af6509"
SESSION_ID = "35cd6b11-5a2b-415a-8e87-a3b68fb0817d"
PUBLICATION, PUB_PARENT = "7b7aba4645ff2430d583c641d817630282a56fdf", "108d4ca1a395c64c43cfae4f17f5854fe2d01536"
GUARDS_RUN = "0c817e1d37409ae6dfb1246c20e08c4509ad86d6"
EVID = "docs/automation/evidence/I-62-A6/"
GUARDS, SELFTEST, GUARDS_RUN_RESULT, PREFLIGHT = (EVID + n for n in ("a6-guards.py", "a6-selftest.json", "a6-guards-result-run.json", "preflight.md"))
KIT_BLOBS = {
    "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json": "541956eb8f5d154414b91338c655919ae0ade697",
}
ORDER_TEXTS = {"order.txt": ("77", "e23faecbacc7ae33976f211ee381020b88dd08f38067eaa938c5b54ec36c4537", 2918)}
RUN_FILES = ["order.txt", "prompt.md"]
TMP = os.path.join(os.path.dirname(KIT), ".tmp", "ex1")
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # solo lectura


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


def guards(args, out):
    env = dict(os.environ, TEMP=TMP, TMP=TMP, TMPDIR=TMP, GIT_OPTIONAL_LOCKS="0")
    return subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", GUARDS] + args + ["--out", out, "--tmp", os.path.join(TMP, "g")], cwd=CLONE,
                          capture_output=True, text=True, encoding="utf-8", errors="replace", env=env)


params = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
assert params.get("Provisional") is False and params.get("Clone") == CLONE and params.get("Run") == RUN, "kit-params.json provisional o de otro clon"
REV = params["CloneCommit"]
closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
gate = json.load(open(os.path.join(KIT, "transport-gate.json"), encoding="utf-8"))
AT_COMMIT = {(PUBLICATION, OBJECT): OBJECT_BLOB, (GUARDS_RUN, OBJECT): OBJECT_BLOB, (REV, OBJECT): OBJECT_BLOB,
             (PUBLICATION, GUARDS): "c5cc6168d4a29f94623a8fab90a2d4f5f8df9ffa", (REV, GUARDS): "c5cc6168d4a29f94623a8fab90a2d4f5f8df9ffa",
             (PUBLICATION, SELFTEST): "8a61a8c48aeb007d05772cab53679b8f58a97fe4", (REV, SELFTEST): "8a61a8c48aeb007d05772cab53679b8f58a97fe4",
             (PUBLICATION, PREFLIGHT): "69c1d98752bcdbf0a2df0c0f2ca8812cc832f44f", (REV, PREFLIGHT): "69c1d98752bcdbf0a2df0c0f2ca8812cc832f44f",
             (GUARDS_RUN, GUARDS_RUN_RESULT): "a8c9ba8bba3047849cfb8bf583dd9d707ffea610",
             (REV, GUARDS_RUN_RESULT): "a8c9ba8bba3047849cfb8bf583dd9d707ffea610"}
PRESENT = {REV: "commit del clon (decisiones §77)", GUARDS_RUN: "guardas `run` de A-6 sobre 7b7aba46 = PASS (custodia)",
           PUBLICATION: "A-6 candidata publicada (eac473c0)", PUB_PARENT: "padre de la publicación (base de las guardas)",
           "9356edf141c795ea718a360e266334ded10d6a76": "commit del paquete B-min",
           "bb0d5522e8411f66a51fdfb3f1f0d0514b737453": "base del mapa de cláusulas (C-20b)",
           "d2d74c14f5e661c7ce77a81aae964dfcb4abdf40": "caracterización de claude-cli"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)"}
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
at_commit = {"%s:%s" % (c[:8], p): {"Expected": b, "Got": git("rev-parse", c + ":" + p, check=False).stdout.strip()} for (c, p), b in AT_COMMIT.items()}
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
project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a6*")))
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
doc = {
    "Clone": CLONE, "CloneCommit": REV, "Object": {"Path": OBJECT, "Blob": OBJECT_BLOB},
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
    "BlobsAtCommits": at_commit,
    "ObjectAbsentAtPublicationParent": not ok("cat-file", "-e", PUB_PARENT + ":" + OBJECT),
    "PublicationParent": git("rev-parse", PUBLICATION + "^").stdout.strip(),
    "GuardsRunParent": git("rev-parse", GUARDS_RUN + "^").stdout.strip(),
    "CommitsPresent": {c: {"What": w, "Present": ok("cat-file", "-e", c + "^{commit}")} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": ok("cat-file", "-e", c + "^{commit}")} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "TransportCharacterization": {"Path": char_path, "InClone": char_in_clone, "Sha256": char_sha,
                                  "MatchesGate": char_sha == gate["Characterization"]["Sha256"], "GateStatus": gate.get("Status")},
    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a6*", "Found": project_dirs},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
    "ClosureIdentity": {"AuthorityRevision": closure["AuthorityRevision"], "ObjectBlob": closure["ObjectBlob"], "Provisional": closure.get("Provisional")},
}
os.makedirs(TMP, exist_ok=True)
wt_head = lambda: subprocess.run(["git", "-C", WORKTREE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
pre = {"Worktree": "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal (solo lectura de git)",
       "WorktreeHeadBefore": wt_head()}
out_run, out_st = os.path.join(TMP, "run.json"), os.path.join(TMP, "st.json")
r = guards(["run", "--repo", WORKTREE, "--base", PUB_PARENT, "--head", PUBLICATION], out_run)
pre["Run"] = {"Command": "python -I -B -X utf8 docs/automation/evidence/I-62-A6/a6-guards.py run --repo <worktree> --base 108d4ca1 --head 7b7aba46 "
                         "--out <temporal> --tmp <temporal> (script del clon; cwd = el clon; solo lectura de git)",
              "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:], "StderrTail": r.stderr.strip()[-400:]}
try:
    d = json.load(open(out_run, encoding="utf-8"))
    custodied = json.loads(git("show", REV + ":" + GUARDS_RUN_RESULT).stdout)
    pre["Run"].update({"Result": d.get("Result"), "Mode": d.get("Mode"), "Base": d.get("Base"), "Head": d.get("Head"),
                       "WholeEqualsCustodied": d == custodied,
                       "DifferingTopLevelKeys": sorted(k for k in set(d) | set(custodied) if d.get(k) != custodied.get(k)),
                       "OutputSha256": sha(open(out_run, "rb").read())})
except (OSError, ValueError) as e:
    pre["Run"]["OutputError"] = str(e)
r = guards(["self-test", "--repo", WORKTREE, "--a6", os.path.join(CLONE, OBJECT), "--base", PUB_PARENT], out_st)
pre["SelfTest"] = {"Command": "python -I -B -X utf8 docs/automation/evidence/I-62-A6/a6-guards.py self-test --repo <worktree> --a6 <A-6 del clon> "
                              "--base 108d4ca1 --out <temporal> --tmp <temporal> (script del clon; cwd = el clon; solo lectura de git)",
                   "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:], "StderrTail": r.stderr.strip()[-400:]}
try:
    d = json.load(open(out_st, encoding="utf-8"))
    custodied = json.loads(git("show", REV + ":" + SELFTEST).stdout)
    summ = d.get("Summary") or {}
    pre["SelfTest"].update({"Result": d.get("Result"), "ToolBlob": d.get("ToolBlob"), "A6TextBlob": d.get("A6TextBlob"), "Repo": d.get("Repo"),
                            "CustodiedRepo": custodied.get("Repo"), "Mutants": summ.get("Mutants"), "MutantsKilled": summ.get("Killed"),
                            "WholeEqualsCustodied": d == custodied,
                            "DifferingTopLevelKeys": sorted(k for k in set(d) | set(custodied) if d.get(k) != custodied.get(k)),
                            "OutputSha256": sha(open(out_st, "rb").read())})
except (OSError, ValueError) as e:
    pre["SelfTest"]["OutputError"] = str(e)
pre["WorktreeHeadAfter"] = wt_head()
shutil.rmtree(TMP)
if not os.listdir(os.path.dirname(TMP)):
    os.rmdir(os.path.dirname(TMP))
doc["GuardsPreflight"] = pre
doc["StatusAfterPreflight"] = git("status", "--porcelain", "--ignored").stdout
gr, gs = pre["Run"], pre["SelfTest"]
doc["AllChecks"] = (doc["Head"] == REV and doc["Detached"] and doc["LocalBranches"] == [] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and doc["CommitsReachableFromAllRefs"] == doc["CommitsReachableFromHead"] and
                    not doc["TagsNotMergedIntoHead"] and all(v["Match"] for v in doc["Blobs"].values()) and
                    all(v["Expected"] == v["Got"] for v in at_commit.values()) and doc["ObjectAbsentAtPublicationParent"] and
                    doc["PublicationParent"] == PUB_PARENT and doc["GuardsRunParent"] == PUBLICATION and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and
                    all(run[n]["MatchesDecisions"] for n in ORDER_TEXTS) and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    char_in_clone and doc["TransportCharacterization"]["MatchesGate"] and gate.get("Status") in ("MEASURED", "ACCEPTED") and
                    not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    closure["AuthorityRevision"] == REV and closure["ObjectBlob"] == OBJECT_BLOB and closure.get("Provisional") is False and
                    gr["ExitCode"] == 0 and gr.get("Result") == "PASS" and gr.get("WholeEqualsCustodied") is True and
                    gs["ExitCode"] == 0 and gs.get("Result") == "PASS" and gs.get("A6TextBlob") == OBJECT_BLOB and
                    gs.get("ToolBlob") == AT_COMMIT[(REV, GUARDS)] and gs.get("Mutants") == gs.get("MutantsKilled") and
                    gs.get("WholeEqualsCustodied") is True and
                    pre["WorktreeHeadBefore"] == pre["WorktreeHeadAfter"] and
                    doc["StatusAfterPreflight"] == "")
text = json.dumps(doc, ensure_ascii=False, indent=1)
text = re.sub(r'(?i)[a-z]:(?:\\\\|/)+users(?:\\\\|/)+[^\\\\/"\s]+', "%USERPROFILE%", text)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "GuardsRun": {k: gr.get(k) for k in ("ExitCode", "Result", "WholeEqualsCustodied", "DifferingTopLevelKeys")},
                  "GuardsSelfTest": {k: gs.get(k) for k in ("ExitCode", "Result", "Mutants", "MutantsKilled", "WholeEqualsCustodied",
                                                            "DifferingTopLevelKeys")}}))
sys.exit(0 if doc["AllChecks"] else 1)
