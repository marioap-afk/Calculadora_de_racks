"""Verificación mecánica del clon limpio y del run de la ÚNICA revisión formal de la enmienda A-7 (decisiones §93) por claude-cli, antes del
lanzamiento (la identidad la verifica el invocador, porque el revisor no tiene herramientas de ejecución), con la reproducción de las comprobaciones
estáticas de A-7 (guardas a7-guards.py). Derivado del verify_clone.py del kit acreditado de A-6 ronda 4 (R20261014T180556Z-5e02): las mismas
comprobaciones del clon, del run, del registro de caracterización y de la sesión nueva; cambian el objeto, los commits y las guardas reproducidas.

Comprueba: kit-params.json no provisional; HEAD = CloneCommit, separado (sin ninguna rama local); sin remotos; árbol limpio (también sin
ignorados); ningún objeto fuera de la historia de HEAD y ninguna etiqueta fuera de ella; los blobs designados (los de CanonicalInputs del cierre); la
historia del objeto (sin A-7 en el padre de la publicación; el blob de A-7 en la publicación, en la custodia de las guardas y en el commit del clon;
la A-6 acordada en los cuatro; a7-guards.py, p4-guards-selftest.json, summary.json y preflight.md sin cambio desde la publicación; el resultado
`run` sin cambio desde su custodia); commits presentes (los de A-7, §92, §91, la preparación de la medición, el acuerdo de A-6, la caracterización
de 2.1.299, el paquete B-min y la base del mapa) y ausentes (los del Freeze y de V14, anteriores al rebase); sin enlaces simbólicos ni uniones en el
árbol de trabajo ni entradas 120000 en el índice; core.autocrlf; el run (exactamente order.txt y prompt.md; order.txt con el SHA-256 y los bytes que
declara decisiones §93 en el commit; cada archivo idéntico a la copia del kit y a RunFileHashes del cierre; prompt.md sin salto de línea final ni CR
y en UTF-8); el registro de caracterización que fija la compuerta (decisiones §87: copia en kit/; su SHA-256 = el fijado = el de
transport-gate.json, y su blob = el del registro en el commit 1c960c0a, leído del worktree con git de solo lectura y del clon); que no exista ningún
directorio de proyecto de claude-cli para el clon o el run (patrón D--r62-arch-a7*) ni una transcripción con el session id fijado; y una preflight
de las guardas de A-7 (evidencia del invocador: el revisor no la ejecuta), con la salida y los temporales en <kit-review>/.tmp/ex1, que se borra.
Como en los kits de A-6, la preflight ejecuta el a7-guards.py del clon con --repo = el worktree de la rama, solo lectura (las guardas leen con
GIT_OPTIONAL_LOCKS=0 y --no-replace-objects; ninguna escritura en el worktree):
  * `a7-guards.py run --repo <worktree> --base <padre de la publicación> --head 6fee8fd6`: su salida, entera, igual al resultado `run` custodiado
    (PASS, bb439090..6fee8fd6; la cabeza abreviada, tal como la registra el resultado custodiado);
  * `a7-guards.py selftest --repo <worktree> --cand <A-7 del clon> --evid <copia de I-62-A7/ tal como estaba cuando se ejecutó la autoprueba>`:
    Result PASS, todos los mutantes muertos, e igual entera a la autoprueba custodiada (p4-guards-selftest.json). La copia de --evid se escribe
    desde el commit de publicación con los archivos que existían cuando run_a7.py ejecutó la autoprueba (fase p4, primera orden; antes de escribir
    p4-guards-selftest.json, p4-guards-preview.json, p4-guards.json, summary.json, preflight.md y MANIFEST.sha256): EVID_AT_SELFTEST, comprobado
    contra el número de archivos añadidos que registra la autoprueba custodiada.
Las guardas escriben solo --out y su --tmp; se ejecutan con el intérprete aislado (-I -B) y el cwd en el clon, que se comprueba limpio al terminar;
el worktree, sin cambio de HEAD ni de estado. No ejecuta el constructor ni el validador del cierre de A7-1, el paquete B-min ni el kit de la medición.
Escribe clone-verification.json junto a este script. No escribe en el clon ni en el worktree.
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
CLONE, RUN = r"D:\r62-arch-a7", r"D:\r62-arch-a7-run"
OBJECT = "docs/initiatives/I-62-A-7.md"
OBJECT_BLOB = "6c29747d61c5f689a195095ad3712e6d9dba62aa"
A6, A6_AGREED_BLOB = "docs/initiatives/I-62-A-6.md", "4bc919dd4768ab81abc74b70d3a4a3821c07feb5"
SESSION_ID = "4945013a-d80e-43dc-bc95-d34dff85fcfe"
EV = "docs/automation/evidence/I-62-A7/"
GUARDS, SELFTEST, GUARDS_RUN_RESULT, PREFLIGHT, SUMMARY = (EV + n for n in ("a7-guards.py", "p4-guards-selftest.json", "a7-guards-result-run.json",
                                                                             "preflight.md", "summary.json"))
GUARDS_RUN_HEAD = "6fee8fd6"   # la cabeza con que se ejecutó y registró el resultado `run` custodiado
EVID_NOT_YET_AT_SELFTEST = ["p4-guards-selftest.json", "p4-guards-preview.json", "p4-guards.json", "summary.json", "preflight.md", "MANIFEST.sha256"]
ORDER_TEXTS = {"order.txt": ("93", "a5c37f714364bc704c5f38218ae2b1a9209052c47834dcc5349684eb141be41b", 4057)}
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


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


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
assert params.get("Provisional") is False and not params.get("Synthetic") and params.get("Clone") == CLONE and params.get("Run") == RUN, \
    "kit-params.json provisional, sintético o de otro clon"
REV, PUBLICATION, PUB_PARENT, GUARDS_RUN = params["CloneCommit"], params["Publication"], params["PublicationParent"], params["GuardsRunCommit"]
assert params["Object"]["Blob"] == OBJECT_BLOB and PUBLICATION.startswith(GUARDS_RUN_HEAD), "kit-params.json con otro objeto u otra publicación"
closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
gate = json.load(open(os.path.join(KIT, "transport-gate.json"), encoding="utf-8"))
at = lambda c, p: git("rev-parse", c + ":" + p, check=False).stdout.strip()
AT_COMMIT = {(PUBLICATION, OBJECT): OBJECT_BLOB, (GUARDS_RUN, OBJECT): OBJECT_BLOB, (REV, OBJECT): OBJECT_BLOB,
             (PUB_PARENT, A6): A6_AGREED_BLOB, (PUBLICATION, A6): A6_AGREED_BLOB, (GUARDS_RUN, A6): A6_AGREED_BLOB, (REV, A6): A6_AGREED_BLOB,
             (REV, GUARDS): at(PUBLICATION, GUARDS), (REV, SELFTEST): at(PUBLICATION, SELFTEST), (REV, PREFLIGHT): at(PUBLICATION, PREFLIGHT),
             (REV, SUMMARY): at(PUBLICATION, SUMMARY), (REV, GUARDS_RUN_RESULT): at(GUARDS_RUN, GUARDS_RUN_RESULT)}
PRESENT = {REV: "commit del clon (decisiones §93)", GUARDS_RUN: "custodia del resultado `run` de las guardas de A-7 (y borradores de §92)",
           PUBLICATION: "publicación de A-7 (6c29747d)", PUB_PARENT: "padre de la publicación (base de las guardas; decisiones §92)",
           "703f1c3f9ee733ddfaa863d7b6c582625e3f7ac1": "análisis de §91 custodiados (determinación de I6, D-1, doce lecturas)",
           "718a7a5f196d595fe1032af8e7ca6a906a1be0f7": "kit a6m7-measure congelado y custodiado",
           "1e73c39b83159acfec588b2c3efbd2541a4b59ae": "preparación de la medición de A6-7 custodiada (banco, lecturas)",
           "400a72d8581d1dd472db59412fd485df8fae03f2": "decisiones §90 (A-6 AGREED)",
           "e604ba4fd4ffb684071f400c919f5b3e43e41662": "custodia de la revisión de A-6 ronda 4",
           "1c960c0a2bcf07b4a074db78179489ae4a9e4822": "caracterización de claude-cli 2.1.299",
           "9356edf141c795ea718a360e266334ded10d6a76": "commit del paquete B-min",
           "bb0d5522e8411f66a51fdfb3f1f0d0514b737453": "base del mapa de cláusulas (C-20b)"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)"}
reparse, files = [], 0
for root, dirs, fs in os.walk(CLONE, followlinks=False):
    for n in dirs + fs:
        p = os.path.join(root, n)
        st = os.lstat(p)
        if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
            reparse.append(p)
    files += len(fs)
want = {r["Path"]: r["Blob"] for r in closure["CanonicalInputs"]}
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
# decisiones §87: el registro de caracterización de 2.1.299 lo fija la compuerta en una copia en kit/ (la del kit acreditado de A-6 ronda 4): su
# SHA-256 = el fijado y su blob = el del registro en el commit de la caracterización (comprobado en el worktree, solo lectura, y en el clon).
CHAR_COMMIT = "1c960c0a2bcf07b4a074db78179489ae4a9e4822"
CHAR_REPO_PATH = "docs/automation/evidence/I-62-claude-cli/R20261016T185242Z-char/claude-cli-characterization.json"
CHAR_SHA, CHAR_BLOB = "4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c", "19b8a27e0de4c92749147a15c7cea5012289040b"
char_bytes = open(char_path, "rb").read()
char_sha = sha(char_bytes)
char_blob_at_commit = subprocess.run(["git", "-C", WORKTREE, "rev-parse", CHAR_COMMIT + ":" + CHAR_REPO_PATH], capture_output=True, text=True,
                                     env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout.strip()
char_blob_in_clone = git("rev-parse", REV + ":" + CHAR_REPO_PATH, check=False).stdout.strip()
char_in_kit = os.path.normcase(os.path.abspath(char_path)) == os.path.normcase(os.path.join(KIT, "claude-cli-characterization.json"))
char_external_ok = (char_in_kit and char_sha == CHAR_SHA and git_blob(char_bytes) == CHAR_BLOB and char_blob_at_commit == CHAR_BLOB and
                    char_blob_in_clone == CHAR_BLOB)
projects = os.path.join(os.path.expanduser("~"), ".claude", "projects")
project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a7*")))
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
ev_pub = set(n for n in git("ls-tree", "-r", "--name-only", PUBLICATION, "--", EV).stdout.split("\n") if n)
ev_now = set(n for n in git("ls-tree", "-r", "--name-only", REV, "--", EV).stdout.split("\n") if n)
doc = {
    "Clone": CLONE, "CloneCommit": REV, "Object": {"Path": OBJECT, "Blob": OBJECT_BLOB}, "Publication": PUBLICATION, "PublicationParent": PUB_PARENT,
    "GuardsRunCommit": GUARDS_RUN,
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
    "A7AbsentAtPublicationParent": not ok("cat-file", "-e", PUB_PARENT + ":" + OBJECT),
    "A7EvidenceAddedAfterPublication": sorted(ev_now - ev_pub), "A7EvidenceRemovedAfterPublication": sorted(ev_pub - ev_now),
    "PublicationParentOf": git("rev-parse", PUBLICATION + "^").stdout.strip(),
    "CommitsPresent": {c: {"What": w, "Present": ok("cat-file", "-e", c + "^{commit}")} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": ok("cat-file", "-e", c + "^{commit}")} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "TransportCharacterization": {"Path": char_path, "InKit": char_in_kit, "Sha256": char_sha, "PinnedSha256": CHAR_SHA,
                                  "Blob": git_blob(char_bytes), "PinnedBlob": CHAR_BLOB, "Commit": CHAR_COMMIT, "RepoPath": CHAR_REPO_PATH,
                                  "BlobAtCommit": char_blob_at_commit, "BlobInClone": char_blob_in_clone, "ExternalCopyOk": char_external_ok,
                                  "MatchesGate": char_sha == gate["Characterization"]["Sha256"], "GateStatus": gate.get("Status")},
    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a7*", "Found": project_dirs},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
    "ClosureIdentity": {"AuthorityRevision": closure["AuthorityRevision"], "ObjectBlob": closure["ObjectBlob"], "Provisional": closure.get("Provisional")},
}
os.makedirs(TMP, exist_ok=True)
wt_head = lambda: subprocess.run(["git", "-C", WORKTREE, "rev-parse", "HEAD"], capture_output=True, text=True,
                                 env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout.strip()
wt_status = lambda: subprocess.run(["git", "-C", WORKTREE, "status", "--porcelain"], capture_output=True, text=True, encoding="utf-8",
                                   errors="replace", env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout
pre = {"Worktree": "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal (solo lectura de git)",
       "WorktreeHeadBefore": wt_head(), "WorktreeStatusBefore": wt_status()}
out_run, out_st = os.path.join(TMP, "run.json"), os.path.join(TMP, "st.json")
r = guards(["run", "--repo", WORKTREE, "--base", PUB_PARENT, "--head", GUARDS_RUN_HEAD], out_run)
pre["Run"] = {"Command": "python -I -B -X utf8 %s run --repo <worktree> --base %s --head %s --out <temporal> --tmp <temporal> (script del clon; "
                         "cwd = el clon; solo lectura de git)" % (GUARDS, PUB_PARENT[:8], GUARDS_RUN_HEAD),
              "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:], "StderrTail": r.stderr.strip()[-400:]}
try:
    d = json.load(open(out_run, encoding="utf-8"))
    custodied = json.loads(git("show", REV + ":" + GUARDS_RUN_RESULT).stdout)
    pre["Run"].update({"Result": d.get("Result"), "Mode": d.get("Mode"), "Base": d.get("Base"), "Head": d.get("Head"), "A7Blob": d.get("A7Blob"),
                       "WholeEqualsCustodied": d == custodied,
                       "DifferingTopLevelKeys": sorted(k for k in set(d) | set(custodied) if d.get(k) != custodied.get(k)),
                       "OutputSha256": sha(open(out_run, "rb").read())})
except (OSError, ValueError) as e:
    pre["Run"]["OutputError"] = str(e)
custodied_st = json.loads(git("show", REV + ":" + SELFTEST).stdout)
evid_dir = os.path.join(TMP, "evid")
evid_names = sorted(n for n in ev_pub if n != OBJECT and n[len(EV):] not in EVID_NOT_YET_AT_SELFTEST)
for n in evid_names:
    dst = os.path.join(evid_dir, *n[len(EV):].split("/"))
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    open(dst, "wb").write(git("show", PUBLICATION + ":" + n, binary=True).stdout)
r = guards(["selftest", "--repo", WORKTREE, "--cand", os.path.join(CLONE, OBJECT), "--evid", evid_dir, "--base", PUB_PARENT], out_st)
pre["SelfTest"] = {"Command": "python -I -B -X utf8 %s selftest --repo <worktree> --cand <A-7 del clon> --evid <copia de I-62-A7/ en %s, %d "
                              "archivos> --base %s --out <temporal> --tmp <temporal> (script del clon; cwd = el clon; solo lectura de git)"
                              % (GUARDS, PUBLICATION[:8], len(evid_names), PUB_PARENT[:8]),
                   "EvidFiles": len(evid_names), "EvidExcluded": EVID_NOT_YET_AT_SELFTEST,
                   "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:], "StderrTail": r.stderr.strip()[-400:]}
try:
    d = json.load(open(out_st, encoding="utf-8"))
    pre["SelfTest"].update({"Result": d.get("Result"), "Base": d.get("Base"), "CandidateBlob": (d.get("Candidate") or {}).get("CandidateBlob"),
                            "CandidateAdded": (d.get("Candidate") or {}).get("Added"),
                            "CustodiedCandidateAdded": (custodied_st.get("Candidate") or {}).get("Added"),
                            "Mutants": d.get("MutantsTotal"), "MutantsKilled": d.get("Killed"),
                            "WholeEqualsCustodied": d == custodied_st,
                            "DifferingTopLevelKeys": sorted(k for k in set(d) | set(custodied_st) if d.get(k) != custodied_st.get(k)),
                            "OutputSha256": sha(open(out_st, "rb").read())})
except (OSError, ValueError) as e:
    pre["SelfTest"]["OutputError"] = str(e)
pre["WorktreeHeadAfter"], pre["WorktreeStatusAfter"] = wt_head(), wt_status()
shutil.rmtree(TMP, onexc=lambda func, path, exc: (os.chmod(path, stat.S_IWRITE), func(path)))   # objetos de git de solo lectura (Windows)
if not os.listdir(os.path.dirname(TMP)):
    os.rmdir(os.path.dirname(TMP))
doc["GuardsPreflight"] = pre
doc["StatusAfterPreflight"] = git("status", "--porcelain", "--ignored").stdout
gr, gs = pre["Run"], pre["SelfTest"]
doc["AllChecks"] = (doc["Head"] == REV and doc["Detached"] and doc["LocalBranches"] == [] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and doc["CommitsReachableFromAllRefs"] == doc["CommitsReachableFromHead"] and
                    not doc["TagsNotMergedIntoHead"] and all(v["Match"] for v in doc["Blobs"].values()) and
                    all(v["Expected"] == v["Got"] and v["Got"] for v in at_commit.values()) and doc["A7AbsentAtPublicationParent"] and
                    doc["PublicationParentOf"] == PUB_PARENT and doc["A7EvidenceAddedAfterPublication"] == [GUARDS_RUN_RESULT] and
                    not doc["A7EvidenceRemovedAfterPublication"] and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and
                    all(run[n]["MatchesDecisions"] for n in ORDER_TEXTS) and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    char_external_ok and doc["TransportCharacterization"]["MatchesGate"] and gate.get("Status") in ("MEASURED", "ACCEPTED") and
                    not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    closure["AuthorityRevision"] == REV and closure["ObjectBlob"] == OBJECT_BLOB and closure.get("Provisional") is False and
                    gr["ExitCode"] == 0 and gr.get("Result") == "PASS" and gr.get("WholeEqualsCustodied") is True and
                    gr.get("Base") == PUB_PARENT and gr.get("Head") == GUARDS_RUN_HEAD and gr.get("A7Blob") == OBJECT_BLOB and
                    gs["ExitCode"] == 0 and gs.get("Result") == "PASS" and gs.get("CandidateBlob") == OBJECT_BLOB and
                    gs.get("Mutants") == gs.get("MutantsKilled") and bool(gs.get("Mutants")) and gs.get("CandidateAdded") == len(evid_names) + 1 and
                    gs.get("WholeEqualsCustodied") is True and
                    pre["WorktreeHeadBefore"] == pre["WorktreeHeadAfter"] and pre["WorktreeStatusBefore"] == pre["WorktreeStatusAfter"] and
                    doc["StatusAfterPreflight"] == "")
text = json.dumps(doc, ensure_ascii=False, indent=1)
text = re.sub(r'(?i)[a-z]:(?:\\\\|/)+users(?:\\\\|/)+[^\\\\/"\s]+', "%USERPROFILE%", text)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "GuardsRun": {k: gr.get(k) for k in ("ExitCode", "Result", "WholeEqualsCustodied", "DifferingTopLevelKeys")},
                  "GuardsSelfTest": {k: gs.get(k) for k in ("ExitCode", "Result", "Mutants", "MutantsKilled", "CandidateAdded", "WholeEqualsCustodied",
                                                            "DifferingTopLevelKeys")}}))
sys.exit(0 if doc["AllChecks"] else 1)
