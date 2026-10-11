"""I-62 A-5, preflight (f): commit candidato HIPOTÉTICO de A-5, construido solo en un clon temporal compartido (D:\\r62-fixture\\tmp-a5-rc).

Nunca escribe en el worktree de I-62 ni en su repositorio: `git clone --bare --shared` (alternates de solo lectura), índice temporal propio y
fontanería (hash-object, update-index, write-tree, commit-tree). El commit añade solo A-5 y el paquete de evidencia (docs/automation/evidence/I-62-A5/)
sobre la HEAD preparada (efcfcbc5) y comprueba que el diff de las superficies de producción es vacío. Ningún push; el clon se borra al final del preflight.
Uso: python -I -B make_candidate_commit.py [--out out/candidate-commit.json]
"""
import argparse
import json
import os
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
DEST = "D:/r62-fixture/tmp-a5-rc"
BASE = "efcfcbc5a9f89de0dc4f4f21a426b4723ef18245"
A5 = "docs/initiatives/I-62-A-5.md"
EVID = "docs/automation/evidence/I-62-A5/"
EXCLUDE = {"a5-guards-result-preview.json", "out/candidate-commit.json"}
PROD = ["src", "tests", "eng", "tools", "deploy", "assets", "docs/adr", "docs/automation/agent-execution", "docs/AUTOMATION_PLAN.md",
        "docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/FOUNDATIONS.md", "AGENTS.md", "CLAUDE.md", "Directory.Build.props",
        "Directory.Build.targets", "RackCad.sln", "global.json", "NuGet.Config", "docs/initiatives/I-62-proposal-v14.md",
        "docs/initiatives/I-62-consensus-freeze.md", "docs/initiatives/I-62-A-1.md", "docs/initiatives/I-62-A-2.md", "docs/initiatives/I-62-A-3.md",
        "docs/initiatives/I-62-A-4.md", "docs/initiatives/PROMPT_TEMPLATES.md", "docs/automation/evidence/I-62-F4",
        "docs/automation/evidence/I-62-F6/MAP-INVALID"]


def git(*args, data=None, env=None, ok=(0,)):
    r = subprocess.run(["git", "-C", DEST] + list(args), input=data, capture_output=True, env=env)
    if r.returncode not in ok:
        raise SystemExit("git %s: %s" % (args[0], r.stderr.decode("utf-8", "replace")[-200:]))
    return r.stdout.decode("utf-8")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=os.path.join(HERE, "out", "candidate-commit.json"))
    a = ap.parse_args()
    if os.path.exists(DEST):
        def _writable(func, path, exc):  # los objetos de Git son de solo lectura en Windows
            os.chmod(path, 0o600)
            func(path)
        shutil.rmtree(DEST, onexc=_writable)
    common = subprocess.run(["git", "-C", WT, "rev-parse", "--path-format=absolute", "--git-common-dir"], capture_output=True,
                            encoding="utf-8").stdout.strip()
    r = subprocess.run(["git", "clone", "-q", "--bare", "--shared", "--no-tags", common, DEST], capture_output=True)
    if r.returncode:
        raise SystemExit("clone: " + r.stderr.decode("utf-8", "replace")[-200:])
    git("remote", "remove", "origin")
    assert git("rev-parse", BASE + "^{commit}").strip() == BASE
    files = {A5: os.path.join(HERE, "I-62-A-5.md")}
    for root, dirs, names in os.walk(HERE):
        dirs[:] = [d for d in dirs if not d.startswith("tmp") and d != "__pycache__"]
        for n in names:
            rel = os.path.relpath(os.path.join(root, n), HERE).replace("\\", "/")
            if rel == "I-62-A-5.md" or rel in EXCLUDE:
                continue
            files[EVID + rel] = os.path.join(root, n)
    idx = os.path.join(DEST, "cand.index")
    env = {**os.environ, "GIT_INDEX_FILE": idx, "GIT_AUTHOR_NAME": "a5-preflight", "GIT_AUTHOR_EMAIL": "fixture@example.invalid",
           "GIT_COMMITTER_NAME": "a5-preflight", "GIT_COMMITTER_EMAIL": "fixture@example.invalid",
           "GIT_AUTHOR_DATE": "2026-10-11T12:00:00+00:00", "GIT_COMMITTER_DATE": "2026-10-11T12:00:00+00:00"}
    git("read-tree", BASE, env=env)
    for path, src in sorted(files.items()):
        with open(src, "rb") as fh:
            sha = git("hash-object", "-w", "--no-filters", "--stdin", data=fh.read()).strip()
        git("update-index", "--add", "--cacheinfo", "100644,%s,%s" % (sha, path), env=env)
    tree = git("write-tree", env=env).strip()
    os.remove(idx)
    cand = git("commit-tree", tree, "-p", BASE, data="I-62 A-5 (HIPOTÉTICO, solo preflight): candidata y paquete de evidencia\n".encode("utf-8"),
               env=env).strip()
    status = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", BASE, cand).splitlines() if l]
    prod = git("diff", "--name-status", "--no-renames", BASE, cand, "--", *PROD).strip()
    out = {"Base": BASE, "Candidate": cand, "Tree": tree, "Files": len(status),
           "Status": [{"Status": s[0], "Path": s[-1]} for s in status],
           "AllAdded": all(s[0] == "A" for s in status),
           "OnlyA5AndEvidence": all(s[-1] == A5 or s[-1].startswith(EVID) for s in status),
           "ProductionDiff": prod.splitlines(), "ProductionDiffEmpty": prod == "",
           "ProductionPaths": PROD, "Note": "clon --bare --shared temporal; sin push; el worktree de I-62 no se escribe"}
    with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({k: out[k] for k in ("Candidate", "Files", "AllAdded", "OnlyA5AndEvidence", "ProductionDiffEmpty")}))
    sys.exit(0 if out["ProductionDiffEmpty"] and out["OnlyA5AndEvidence"] else 1)


if __name__ == "__main__":
    main()
