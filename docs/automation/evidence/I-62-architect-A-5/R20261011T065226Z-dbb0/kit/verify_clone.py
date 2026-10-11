"""Verificación mecánica del clon limpio y del run de la primera revisión formal de A-5 (blob ad94ceed) por claude-cli, antes del lanzamiento (la
identidad la verifica el invocador, porque el revisor no tiene herramientas de ejecución).

Comprueba: HEAD = el commit de decisiones §72 (b631c46f), separado (sin ninguna rama local); sin remotos; árbol limpio (también sin ignorados);
ningún objeto fuera de la historia de HEAD; los blobs designados (los de CanonicalInputs del cierre y los que fija el kit); la historia del
objeto (ausente en efcfcbc5, ad94ceed en la publicación bf2cf857, en la custodia de las guardas 1a0db7b0 y en b631c46f; a5-guards.py,
a5-selftest.json y preflight.md sin cambio desde bf2cf857); sin enlaces simbólicos ni uniones en el árbol de trabajo ni entradas 120000 en el
índice; core.autocrlf; commits presentes y ausentes; el run (exactamente order.txt, prompt.md y delta.diff; order.txt con el SHA-256 y los
bytes que declara decisiones §72 en el commit; delta.diff = salida literal de `git diff efcfcbc5 bf2cf857 -- <objeto>`; cada archivo idéntico a
la copia del kit y a RunFileHashes del cierre; prompt.md sin salto de línea final ni CR); el registro de caracterización que fija la compuerta (su
SHA-256 en el clon = el de transport-gate.json); que no exista ningún directorio de proyecto de claude-cli para el clon o el run (patrón
D--r62-arch-a5*) ni una transcripción con el session id fijado; y una preflight de las guardas de A-5 (evidencia del invocador: el revisor no la
ejecuta), con la salida y los temporales en <kit-a5>/.tmp/ex1, que se borra. Las guardas de A-5 (G1) comparan los blobs de V14 y del registro en
el commit del Freeze (b64a3b64), anterior al rebase, que un clon de una sola rama no tiene: como en el kit de A-4, la preflight ejecuta el
a5-guards.py del clon (blob 98d26405) con --repo = el worktree de la rama, solo lectura (show, cat-file, rev-parse, merge-base, diff entre commits
y config; ninguna escritura; tampoco `git status`), y con --a5 = el archivo del clon:
  * `a5-guards.py run --repo <worktree> --base efcfcbc5 --head bf2cf857`: su salida, entera, igual a a5-guards-result-run.json custodiado;
  * `a5-guards.py self-test --repo <worktree> --a5 <A-5 del clon>`: Result PASS, ToolBlob y A5TextBlob del commit, todos los mutantes muertos, e
    igual a a5-selftest.json custodiado salvo la clave `Repo` (el custodiado registra la HEAD de entonces, efcfcbc5; esta, la HEAD del worktree en
    la preflight, que se registra).
Las guardas escriben solo --out y su --tmp; se ejecutan con el intérprete aislado (-I -B) y el cwd en el clon, que se comprueba limpio al
terminar. Escribe clone-verification.json junto a este script.
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
CLONE, RUN = r"D:\r62-arch-a5", r"D:\r62-arch-a5-run"
REV = "b631c46f08fa403628250035bdb9c2f5c600cc81"
OBJECT = "docs/initiatives/I-62-A-5.md"
OBJECT_BLOB = "ad94ceedacbf3dbf70f0217a583978d0e32592fc"
SESSION_ID = "dcb1b798-b53a-4a31-9693-61cd4a2dfe51"
PUBLICATION, PREP_BASE = "bf2cf857b69ada371590cde2c8ddfbc8b3581fab", "efcfcbc5a9f89de0dc4f4f21a426b4723ef18245"
GUARDS_RUN, MAP_BASE = "1a0db7b03b1804145571a6733ca4eb079cbd1f08", "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"
EVID = "docs/automation/evidence/I-62-A5/"
GUARDS, SELFTEST, GUARDS_RUN_RESULT, PREFLIGHT = (EVID + n for n in ("a5-guards.py", "a5-selftest.json", "a5-guards-result-run.json", "preflight.md"))
KIT_BLOBS = {
    "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json": "541956eb8f5d154414b91338c655919ae0ade697",
}
AT_COMMIT = {(PUBLICATION, OBJECT): OBJECT_BLOB, (GUARDS_RUN, OBJECT): OBJECT_BLOB, (REV, OBJECT): OBJECT_BLOB,
             (PUBLICATION, GUARDS): "98d264056fe800477cf37fbfd5bde34877857c68", (REV, GUARDS): "98d264056fe800477cf37fbfd5bde34877857c68",
             (PUBLICATION, SELFTEST): "8d115759c2a5ef86a417da06a84cec81076ca25e", (REV, SELFTEST): "8d115759c2a5ef86a417da06a84cec81076ca25e",
             (PUBLICATION, PREFLIGHT): "2a2ee7997a0c6ec49c1786f33b7b58f1c5263e1d", (REV, PREFLIGHT): "2a2ee7997a0c6ec49c1786f33b7b58f1c5263e1d",
             (GUARDS_RUN, GUARDS_RUN_RESULT): "2a383d79aefb773363ba84bdca2d336d34f14159",
             (REV, GUARDS_RUN_RESULT): "2a383d79aefb773363ba84bdca2d336d34f14159"}
PRESENT = {REV: "decisiones §72 (revisión formal de A-5 autorizada)", GUARDS_RUN: "guardas `run` de A-5 sobre bf2cf857 = PASS (custodia)",
           PUBLICATION: "A-5 candidata publicada (ad94ceed)", PREP_BASE: "base de la publicación (decisiones §71)",
           MAP_BASE: "base del mapa de cláusulas (C-20b)", "d2d74c14f5e661c7ce77a81aae964dfcb4abdf40": "caracterización de claude-cli"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)"}
ORDER_TEXTS = {"order.txt": ("72", "0baac461aec463162df278220174ed5527af46c0bc242b8eac23bbcc623333fc", 2556)}
RUN_FILES = ["order.txt", "prompt.md", "delta.diff"]
GIT_RUN_FILES = {"delta.diff": ["diff", PREP_BASE, PUBLICATION, "--", OBJECT]}
TMP = os.path.join(os.path.dirname(KIT), ".tmp", "ex1")
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # solo lectura


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


def guards(args, out):
    env = dict(os.environ, TEMP=TMP, TMP=TMP, TMPDIR=TMP)
    r = subprocess.run([sys.executable, "-I", "-B", "-X", "utf8", GUARDS] + args + ["--out", out, "--tmp", os.path.join(TMP, "g")], cwd=CLONE,
                       capture_output=True, text=True, encoding="utf-8", errors="replace", env=env)
    return r


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
project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a5*")))
prior_sessions = glob.glob(os.path.join(projects, "*", SESSION_ID + ".jsonl"))
doc = {
    "Clone": CLONE,
    "Commands": [
        'git clone -q --no-local -c core.autocrlf=false --single-branch --branch architecture/portabilidad-coordinador-principal '
        '"%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal" D:\\r62-arch-a5',
        "git -C D:\\r62-arch-a5 checkout -q --detach " + REV,
        "git -C D:\\r62-arch-a5 branch -D architecture/portabilidad-coordinador-principal",
        "git -C D:\\r62-arch-a5 remote remove origin",
        "git -C D:\\r62-arch-a5 reflog expire --expire=now --all; git -C D:\\r62-arch-a5 gc -q --prune=now",
        "git -C D:\\r62-arch-a5 diff efcfcbc5 bf2cf857 -- docs/initiatives/I-62-A-5.md > kit/delta.diff   (bytes literales)",
        "copia byte a byte de kit/order.txt (= cuerpo §72), kit/delta.diff y kit/prompt.md → D:\\r62-arch-a5-run\\",
    ],
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
    "ObjectAbsentAtPublicationParent": git("cat-file", "-e", PREP_BASE + ":" + OBJECT, check=False).returncode != 0,
    "PublicationParent": git("rev-parse", PUBLICATION + "^").stdout.strip(),
    "GuardsRunParent": git("rev-parse", GUARDS_RUN + "^").stdout.strip(),
    "CommitsPresent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": run_listing, "Records": run},
    "TransportCharacterization": {"Path": char_path, "InClone": char_in_clone, "Sha256": char_sha,
                                  "MatchesGate": char_sha == gate["Characterization"]["Sha256"], "GateStatus": gate.get("Status")},
    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a5*", "Found": project_dirs},
    "PriorTranscriptsForSessionId": {"SessionId": SESSION_ID, "Found": len(prior_sessions)},
}
os.makedirs(TMP, exist_ok=True)
wt_head = lambda: subprocess.run(["git", "-C", WORKTREE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
pre = {"Worktree": "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal (solo lectura de git)",
       "WorktreeHeadBefore": wt_head()}
out_run, out_st = os.path.join(TMP, "run.json"), os.path.join(TMP, "st.json")
r = guards(["run", "--repo", WORKTREE, "--base", PREP_BASE, "--head", PUBLICATION], out_run)
pre["Run"] = {"Command": "python -I -B -X utf8 docs/automation/evidence/I-62-A5/a5-guards.py run --repo <worktree> --base efcfcbc5 --head bf2cf857 "
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
r = guards(["self-test", "--repo", WORKTREE, "--a5", os.path.join(CLONE, OBJECT)], out_st)
pre["SelfTest"] = {"Command": "python -I -B -X utf8 docs/automation/evidence/I-62-A5/a5-guards.py self-test --repo <worktree> --a5 <A-5 del clon> "
                              "--out <temporal> --tmp <temporal> (script del clon; cwd = el clon; solo lectura de git)",
                   "ExitCode": r.returncode, "Stdout": r.stdout.strip()[-400:], "StderrTail": r.stderr.strip()[-400:]}
try:
    d = json.load(open(out_st, encoding="utf-8"))
    custodied = json.loads(git("show", REV + ":" + SELFTEST).stdout)
    summ = d.get("Summary") or {}
    pre["SelfTest"].update({"Result": d.get("Result"), "ToolBlob": d.get("ToolBlob"), "A5TextBlob": d.get("A5TextBlob"), "Repo": d.get("Repo"),
                            "CustodiedRepo": custodied.get("Repo"), "Mutants": summ.get("Mutants"), "MutantsKilled": summ.get("Killed"),
                            "WholeEqualsCustodied": d == custodied,
                            "DifferingTopLevelKeys": sorted(k for k in set(d) | set(custodied) if d.get(k) != custodied.get(k)),
                            "EqualsCustodiedExceptRepo": {k: v for k, v in d.items() if k != "Repo"} == {k: v for k, v in custodied.items() if k != "Repo"},
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
                    doc["PublicationParent"] == PREP_BASE and doc["GuardsRunParent"] == PUBLICATION and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and run_listing == sorted(RUN_FILES) and
                    all(v["IdenticalToKitCopy"] and v["MatchesClosure"] for v in run.values()) and
                    all(run[n]["MatchesDecisions"] for n in ORDER_TEXTS) and all(run[n]["EqualsGitOutput"]["Equal"] for n in GIT_RUN_FILES) and
                    not run["prompt.md"]["FinalNewline"] and run["prompt.md"]["CarriageReturns"] == 0 and
                    char_in_clone and doc["TransportCharacterization"]["MatchesGate"] and
                    not project_dirs and doc["PriorTranscriptsForSessionId"]["Found"] == 0 and
                    gr["ExitCode"] == 0 and gr.get("Result") == "PASS" and gr.get("WholeEqualsCustodied") is True and
                    gs["ExitCode"] == 0 and gs.get("Result") == "PASS" and gs.get("A5TextBlob") == closure["ObjectBlob"] and
                    gs.get("ToolBlob") == AT_COMMIT[(REV, GUARDS)] and gs.get("Mutants") == gs.get("MutantsKilled") and
                    gs.get("EqualsCustodiedExceptRepo") is True and set(gs.get("DifferingTopLevelKeys") or []) <= {"Repo"} and
                    doc["StatusAfterPreflight"] == "")
text = json.dumps(doc, ensure_ascii=False, indent=1)
text = re.sub(r'(?i)[a-z]:(?:\\|/)+users(?:\\|/)+[^\\/"\s]+', "%USERPROFILE%", text)
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Run": run_listing,
                  "GuardsRun": {k: gr.get(k) for k in ("ExitCode", "Result", "WholeEqualsCustodied", "DifferingTopLevelKeys")},
                  "GuardsSelfTest": {k: gs.get(k) for k in ("ExitCode", "Result", "Mutants", "MutantsKilled", "EqualsCustodiedExceptRepo",
                                                            "DifferingTopLevelKeys")}}))
sys.exit(0 if doc["AllChecks"] else 1)
