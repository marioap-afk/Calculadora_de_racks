"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. I-62, A62-A1T-01, case T8 of the Coordinator order (successor reconstruction from a clean clone).

Real Git in a disposable directory, never in the I-62 repository:
  1. a branch with the reviewed object X (docs/U-proposal.md) and a state whose attempt L1/1 is in LAUNCHING with Target = {X, path, blob};
  2. main advances; rebase 1 (X -> X'), map M1 custodied in the reconciliation point n1 (rebase_history = [M1]); the attempt keeps Target X;
  3. main advances again; rebase 2 (X' -> X'', n1 -> n1'), map M2 built from the commits that rebase 2 really rewrote; BEFORE committing n2, the
     candidate history [M1, M2] resolves Target X against the rebased tip (no reference to the not-yet-existing commit of n2); n2 is then committed;
  4. a successor makes a clean clone (`git clone --no-local -c core.autocrlf=false --single-branch`), where X and X' are unreachable, and resolves the
     custodied Target only from the custodied state and maps (StateRef = {path, blob}), recomputing the PatchId of the image on its own machine.
Negatives (each with the rule it must trip): history without M1; M1 without the link of X; complete chain but another blob; only the last map.
ResolveBranchRef here follows A-1 D2-10 literally. Usage: python t8-clean-clone.py <empty work dir> <out.json>
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys

WORK, OUT = sys.argv[1:3]
if os.path.exists(WORK):
    shutil.rmtree(WORK, onexc=lambda f, p, e: (os.chmod(p, 0o700), f(p)))   # Git packs are read-only on Windows
os.makedirs(WORK)
SRC, CLONE = os.path.join(WORK, "src"), os.path.join(WORK, "clone")
ENV = dict(os.environ, GIT_AUTHOR_NAME="t8", GIT_AUTHOR_EMAIL="t8@example.invalid", GIT_COMMITTER_NAME="t8",
           GIT_COMMITTER_EMAIL="t8@example.invalid", GIT_CONFIG_NOSYSTEM="1", GIT_CONFIG_GLOBAL=os.devnull)
DOC, STATE, MAPS = "docs/U-proposal.md", "custody/state.json", "custody/rebase-maps/"


def git(repo, *a, check=True, inp=None):
    r = subprocess.run(["git", "-C", repo, "-c", "core.autocrlf=false"] + list(a), capture_output=True, env=ENV, input=inp)
    if check and r.returncode != 0:
        raise RuntimeError("git %s: %s" % (" ".join(a), r.stderr.decode(errors="replace")))
    return r


def out(repo, *a):
    return git(repo, *a).stdout.decode().strip()


def write(repo, rel, text):
    p = os.path.join(repo, rel)
    os.makedirs(os.path.dirname(p), exist_ok=True)
    open(p, "w", encoding="utf-8", newline="\n").write(text)


def commit(repo, msg, *paths):
    git(repo, "add", *paths)
    git(repo, "commit", "-q", "-m", msg)
    return out(repo, "rev-parse", "HEAD")


def blob_text(text):
    return hashlib.sha1(b"blob %d\0" % len(text.encode()) + text.encode()).hexdigest()


def patch_id(repo, c):
    show = git(repo, "show", c).stdout
    return git(repo, "patch-id", "--stable", inp=show).stdout.decode().split()[0]


def exists(repo, c):
    return git(repo, "cat-file", "-e", c + "^{commit}", check=False).returncode == 0


def is_ancestor(repo, a, b):
    return exists(repo, a) and git(repo, "merge-base", "--is-ancestor", a, b, check=False).returncode == 0


def blob_at(repo, c, path):
    r = git(repo, "rev-parse", "%s:%s" % (c, path), check=False)
    return r.stdout.decode().strip() if r.returncode == 0 else None


def rebase(repo, onto_main):
    """Rebase the branch onto main and return the RebaseMap of the commits that this rebase really rewrote."""
    main_before = out(repo, "merge-base", "main", "feature")
    branch_before = out(repo, "rev-parse", "feature")
    before = out(repo, "rev-list", "--reverse", "%s..%s" % (main_before, branch_before)).split()
    git(repo, "rebase", "-q", "main", "feature")
    main_after, branch_after = out(repo, "rev-parse", "main"), out(repo, "rev-parse", "feature")
    after = out(repo, "rev-list", "--reverse", "%s..%s" % (main_after, branch_after)).split()
    assert len(before) == len(after) and main_after == onto_main
    commits = [{"OriginalSha": o, "ImageSha": i, "PatchId": patch_id(repo, i), "PatchIdEqual": patch_id(repo, o) == patch_id(repo, i)}
               for o, i in zip(before, after)]
    return {"MainBeforeSha": main_before, "MainAfterSha": main_after, "BranchBeforeSha": branch_before, "BranchAfterSha": branch_after,
            "Commits": commits}


def resolve(repo, ref, H, head, recompute=True):
    """ResolveBranchRef of A-1 D2-10. H = ordered list of RebaseMap objects. Returns (result, trail)."""
    c, path, blob = ref["Commit"], ref["Path"], ref["Blob"]
    if is_ancestor(repo, c, head) and blob_at(repo, c, path) == blob:
        return "RESOLVED", [c]
    idx = next((k for k, m in enumerate(H) if any(e["OriginalSha"] == c for e in m["Commits"])), None)
    if idx is None:
        return "UNRESOLVED", ["%s no figura como OriginalSha en ningún mapa de H" % c[:8]]
    trail, last = [c], None
    for m in H[idx:]:
        e = next((e for e in m["Commits"] if e["OriginalSha"] == c), None)
        if e is not None:
            if not e["PatchIdEqual"]:
                return "UNRESOLVED", trail + ["PatchIdEqual falso"]
            c, last = e["ImageSha"], e
            trail.append(c)
        elif not is_ancestor(repo, c, m["MainBeforeSha"]):
            return "UNRESOLVED", trail + ["falta un paso de la cadena para %s" % c[:8]]
    if not is_ancestor(repo, c, head):
        return "UNRESOLVED", trail + ["%s no es ancestro de HEAD" % c[:8]]
    if blob_at(repo, c, path) != blob:
        return "UNRESOLVED", trail + ["el blob de %s en %s no es %s" % (path, c[:8], blob[:8])]
    if recompute and last is not None and patch_id(repo, c) != last["PatchId"]:
        return "UNRESOLVED", trail + ["PatchId recalculado distinto"]
    return "RESOLVED", trail


def state_doc(target, history, inv="I1", note=""):
    return json.dumps({"attempt": {"request": "L1", "seq": 1, "state": "LAUNCHING", "invocation": inv, "run_id": "R-L1", "reserved_at": 3,
                                   "budget_snapshot": {"review_rounds": 1, "logical_requests": 1, "architect_launches": 1},
                                   "target": target, "lineages": ["LIN-1"]},
                       "rebase_history": history, "note": note}, indent=1, sort_keys=True) + "\n"


steps = []
os.makedirs(SRC)
git(SRC, "init", "-q", "-b", "main")
write(SRC, "README.md", "base\n")
m0 = commit(SRC, "m0", "README.md")
git(SRC, "checkout", "-q", "-b", "feature")
X_TEXT = "# U proposal\n\nversión revisada X\n"
write(SRC, DOC, X_TEXT)
X = commit(SRC, "X: objeto revisado", DOC)
target = {"Commit": X, "Path": DOC, "Blob": blob_at(SRC, X, DOC)}
assert target["Blob"] == blob_text(X_TEXT)
write(SRC, STATE, state_doc(target, [], note="punto con L1/1 en LAUNCHING"))
commit(SRC, "punto: L1/1 LAUNCHING con Target X", STATE)

# rebase 1
git(SRC, "checkout", "-q", "main")
write(SRC, "main1.txt", "avance 1\n")
m1 = commit(SRC, "m1", "main1.txt")
M1 = rebase(SRC, m1)
M1_TEXT = json.dumps(M1, indent=1, sort_keys=True) + "\n"
write(SRC, MAPS + "M1.json", M1_TEXT)
H1 = [{"path": MAPS + "M1.json", "blob": blob_text(M1_TEXT)}]
r1 = resolve(SRC, target, [M1], out(SRC, "rev-parse", "feature"))
write(SRC, STATE, state_doc(target, H1, note="n1: REBASE_RECONCILIATION 1"))
n1 = commit(SRC, "n1: reconciliación del rebase 1", MAPS + "M1.json", STATE)
X1 = next(e["ImageSha"] for e in M1["Commits"] if e["OriginalSha"] == X)
steps.append({"Step": "rebase 1 + n1", "Resolve(Target X, [M1])": r1, "XImage1": X1})

# rebase 2: validate the candidate history BEFORE committing n2
git(SRC, "checkout", "-q", "main")
write(SRC, "main2.txt", "avance 2\n")
m2 = commit(SRC, "m2", "main2.txt")
M2 = rebase(SRC, m2)
M2_TEXT = json.dumps(M2, indent=1, sort_keys=True) + "\n"
branch_after = M2["BranchAfterSha"]
candidate = [M1, M2]
r2 = resolve(SRC, target, candidate, branch_after)
x_in_m2 = any(e["OriginalSha"] == X for e in M2["Commits"])
write(SRC, MAPS + "M2.json", M2_TEXT)
H2 = H1 + [{"path": MAPS + "M2.json", "blob": blob_text(M2_TEXT)}]
write(SRC, STATE, state_doc(target, H2, note="n2: REBASE_RECONCILIATION 2"))
n2 = commit(SRC, "n2: reconciliación del rebase 2", MAPS + "M2.json", STATE)
steps.append({"Step": "rebase 2, validación con la historia candidata antes de n2", "Resolve(Target X, [M1, M2], branch_after)": r2,
              "M2ContieneX": x_in_m2,
              "ReglaDe03dd822d": "STOP" if not (x_in_m2 or is_ancestor(SRC, X, M2["MainBeforeSha"])) else "PASA",
              "ValidadoSobre": "BranchAfterSha %s (n2 aún no existía)" % branch_after[:8], "n2": n2})

# successor: clean clone, only custodied artifacts
subprocess.run(["git", "clone", "-q", "--no-local", "-c", "core.autocrlf=false", "--single-branch", "--branch", "feature", SRC, CLONE],
               check=True, env=ENV, capture_output=True)
head = out(CLONE, "rev-parse", "HEAD")
st = json.loads(git(CLONE, "show", "HEAD:" + STATE).stdout)
maps = []
for ref in st["rebase_history"]:
    b = git(CLONE, "show", "HEAD:" + ref["path"]).stdout
    assert hashlib.sha1(b"blob %d\0" % len(b) + b).hexdigest() == ref["blob"], ref
    maps.append(json.loads(b))
t = st["attempt"]["target"]
X1_ = next(e["ImageSha"] for e in maps[0]["Commits"] if e["OriginalSha"] == t["Commit"])
pos = resolve(CLONE, t, maps, head)
unchanged = {k: st["attempt"][k] for k in ("state", "invocation", "run_id", "reserved_at", "budget_snapshot", "target", "lineages")}
cases = {
    "T8-sucesor-clon-limpio-cadena-completa": {"Expected": "RESOLVED", "Got": pos},
    "T8n-sin-M1-en-la-historia (A1-P08: falta el primer eslabón)": {"Expected": "UNRESOLVED", "Got": resolve(CLONE, t, maps[1:], head)},
    "T8n-M1-sin-el-eslabon-de-X (A1-P08)": {"Expected": "UNRESOLVED",
                                           "Got": resolve(CLONE, t, [dict(maps[0], Commits=[e for e in maps[0]["Commits"] if e["OriginalSha"] != t["Commit"]]), maps[1]], head)},
    "T8n-cadena-completa-con-otro-blob (identidad de contenido)": {"Expected": "UNRESOLVED", "Got": resolve(CLONE, dict(t, Blob=blob_text("otro\n")), maps, head)},
}
for v in cases.values():
    v["Result"] = "PASS" if v["Got"][0] == v["Expected"] else "FAIL"
steps.append({"Step": "sucesor en un clon limpio", "CloneHead": head, "XReachableInClone": exists(CLONE, t["Commit"]),
              "XImage1ReachableInClone": exists(CLONE, X1_), "AttemptFieldsReadFromState": unchanged, "Cases": cases})

ok = (r1[0] == "RESOLVED" and r2[0] == "RESOLVED" and not x_in_m2 and not exists(CLONE, t["Commit"]) and not exists(CLONE, X1_)
      and all(v["Result"] == "PASS" for v in cases.values()) and unchanged["state"] == "LAUNCHING" and unchanged["target"] == target)
doc = {"Label": "EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION", "Script": "docs/automation/evidence/I-62-A1/a1t01/t8-clean-clone.py",
       "Case": "T8 de la orden del Coordinator (A62-A1T-01)", "Git": subprocess.check_output(["git", "--version"], text=True).strip(),
       "Python": sys.version.split()[0], "Note": "Los SHAs son de esta ejecución (repositorio desechable); se repiten las relaciones, no los valores.",
       "Steps": steps, "AllAsExpected": ok}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(doc, f, ensure_ascii=False, indent=1)
    f.write("\n")
print(json.dumps(doc, ensure_ascii=False, indent=1)[:3000])
print("all", ok)
sys.exit(0 if ok else 1)
