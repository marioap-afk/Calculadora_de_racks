"""I-62 A-6, preflight: commit candidato HIPOTÉTICO de A-6, construido solo en un clon temporal compartido bajo D:\\r62-a6-tmp.

Nunca escribe en el worktree de I-62 ni en su repositorio: `git clone --bare --shared` (alternates de solo lectura), índice temporal propio y
fontanería (hash-object, update-index, write-tree, commit-tree). El commit añade A-6 y el paquete de evidencia (docs/automation/evidence/I-62-A6/)
sobre la base indicada (por defecto, la HEAD del worktree) y comprueba que el diff de las superficies prohibidas es vacío. `--extra RUTA=ARCHIVO`
(repetible) añade o sustituye otra ruta: solo para los controles negativos. Ningún push; el clon lo borra quien lo crea (run_preflight_a6.py).
Uso: python -I -B make_candidate_commit_a6.py --dest D:/r62-a6-tmp/rc --out <json> [--base <sha>] [--extra RUTA=ARCHIVO ...]
"""
import argparse
import json
import os
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
A6_LOCAL = os.path.normpath(os.path.join(HERE, "..", "..", "..", "initiatives", "I-62-A-6.md"))
TMP_ROOT = "D:/r62-a6-tmp"
A6 = "docs/initiatives/I-62-A-6.md"
EVID = "docs/automation/evidence/I-62-A6/"
EXCLUDE = {"a6-guards-result-preview.json", "out/candidate-commit.json"}
FORBIDDEN = ["src", "tests", "eng", "tools", "deploy", "assets", "docs/adr", "docs/automation/agent-execution", "docs/AUTOMATION_PLAN.md",
             "docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/FOUNDATIONS.md", "AGENTS.md", "CLAUDE.md", "Directory.Build.props",
             "Directory.Build.targets", "RackCad.sln", "global.json", "NuGet.Config", "docs/initiatives/I-62-proposal-v14.md",
             "docs/initiatives/I-62-consensus-freeze.md", "docs/initiatives/I-62-A-1.md", "docs/initiatives/I-62-A-2.md",
             "docs/initiatives/I-62-A-3.md", "docs/initiatives/I-62-A-4.md", "docs/initiatives/I-62-A-5.md", "docs/initiatives/PROMPT_TEMPLATES.md",
             "docs/automation/evidence/I-62-F4", "docs/automation/evidence/I-62-F6/MAP-INVALID", "docs/automation/evidence/I-62-F6/FX-02/bmin",
             "docs/automation/evidence/I-62-F6/kits/FX-02", "docs/automation/evidence/I-62-architect-bmin", "docs/automation/evidence/I-62-A5"]


def rmtree_ro(path):
    def _writable(func, p, exc):  # los objetos de Git son de solo lectura en Windows
        os.chmod(p, 0o600)
        func(p)
    if os.path.exists(path):
        shutil.rmtree(path, onexc=_writable)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dest", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--base")
    ap.add_argument("--extra", action="append", default=[])
    a = ap.parse_args()
    dest = a.dest.replace("\\", "/")
    if not dest.lower().startswith(TMP_ROOT.lower() + "/") or "r62-fixture" in dest.lower():
        raise SystemExit("destino fuera de D:/r62-a6-tmp")

    def git(*args, data=None, env=None, ok=(0,)):
        r = subprocess.run(["git", "-C", dest] + list(args), input=data, capture_output=True, env=env)
        if r.returncode not in ok:
            raise SystemExit("git %s: código %d" % (args[0], r.returncode))
        return r.stdout.decode("utf-8")

    rmtree_ro(dest)
    common = subprocess.run(["git", "-C", WT, "rev-parse", "--path-format=absolute", "--git-common-dir"], capture_output=True,
                            encoding="utf-8").stdout.strip()
    base = a.base or subprocess.run(["git", "-C", WT, "rev-parse", "HEAD"], capture_output=True, encoding="utf-8").stdout.strip()
    r = subprocess.run(["git", "clone", "-q", "--bare", "--shared", "--no-tags", common, dest], capture_output=True)
    if r.returncode:
        raise SystemExit("clone: código %d" % r.returncode)
    git("remote", "remove", "origin")
    assert git("rev-parse", base + "^{commit}").strip() == base
    files = {A6: A6_LOCAL}
    for root, dirs, names in os.walk(HERE):
        dirs[:] = [d for d in dirs if not d.startswith("tmp") and d != "__pycache__"]
        for n in names:
            rel = os.path.relpath(os.path.join(root, n), HERE).replace("\\", "/")
            if rel in EXCLUDE:
                continue
            files[EVID + rel] = os.path.join(root, n)
    for ex in a.extra:
        path, src = ex.split("=", 1)
        files[path] = src
    idx = os.path.join(dest, "cand.index")
    env = {**os.environ, "GIT_INDEX_FILE": idx, "GIT_AUTHOR_NAME": "a6-preflight", "GIT_AUTHOR_EMAIL": "fixture@example.invalid",
           "GIT_COMMITTER_NAME": "a6-preflight", "GIT_COMMITTER_EMAIL": "fixture@example.invalid",
           "GIT_AUTHOR_DATE": "2026-10-11T22:00:00+00:00", "GIT_COMMITTER_DATE": "2026-10-11T22:00:00+00:00"}
    git("read-tree", base, env=env)
    for path, src in sorted(files.items()):
        with open(src, "rb") as fh:
            raw = fh.read().replace(b"\r\n", b"\n")   # como la conversión de entrada de core.autocrlf=true
        sha = git("hash-object", "-w", "--no-filters", "--stdin", data=raw).strip()
        git("update-index", "--add", "--cacheinfo", "100644,%s,%s" % (sha, path), env=env)
    tree = git("write-tree", env=env).strip()
    os.remove(idx)
    cand = git("commit-tree", tree, "-p", base, data="I-62 A-6 (HIPOTÉTICO, solo preflight): candidata y paquete de evidencia\n".encode("utf-8"),
               env=env).strip()
    status = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", base, cand).splitlines() if l]
    prod = git("diff", "--name-status", "--no-renames", base, cand, "--", *FORBIDDEN).strip()
    out = {"Base": base, "Candidate": cand, "Tree": tree, "Files": len(status), "Extra": sorted(p.split("=", 1)[0] for p in a.extra),
           "Status": [{"Status": s[0], "Path": s[-1]} for s in status],
           "AllAdded": all(s[0] == "A" for s in status),
           "OnlyA6AndEvidence": all(s[-1] == A6 or s[-1].startswith(EVID) for s in status),
           "ForbiddenDiff": prod.splitlines(), "ForbiddenDiffEmpty": prod == "",
           "Note": "clon --bare --shared temporal bajo D:/r62-a6-tmp; sin push; el worktree de I-62 no se escribe"}
    with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({k: out[k] for k in ("Candidate", "Files", "AllAdded", "OnlyA6AndEvidence", "ForbiddenDiffEmpty")}))
    sys.exit(0 if out["ForbiddenDiffEmpty"] and out["OnlyA6AndEvidence"] else 1)


if __name__ == "__main__":
    main()
