"""Paso del clon de la primera revisión formal de A-6 (decisiones §77, punto 4): crea el clon de lectura D:\\r62-arch-a6 y el run
D:\\r62-arch-a6-run, y escribe kit/kit-params.json. Lo ejecuta finish_kit.py (o la sesión principal), una sola vez. Adaptado del prepare_clone.py
del kit de B-min.

Comprueba antes de escribir nada (solo lectura de git en el worktree de I-62): CLONE_COMMIT (por defecto, el commit de preparación 7b5ffac9,
decisiones §77 registradas) es un SHA completo de un commit alcanzable desde la punta de la rama; el commit de preparación es antecesor o igual de
CLONE_COMMIT; en CLONE_COMMIT, A-6 tiene el blob eac473c0 y decisiones tiene §77; ni el clon, ni el run, ni kit/kit-params.json existen todavía.
Después, con el patrón de los kits de A-5 y de B-min (HEAD separado en el commit, sin ramas locales ni remoto):
  git clone -q --no-local -c core.autocrlf=false --single-branch --branch <rama> <worktree> D:\\r62-arch-a6
  git -C D:\\r62-arch-a6 checkout -q --detach <CLONE_COMMIT>
  git -C D:\\r62-arch-a6 branch -D <rama>
  git -C D:\\r62-arch-a6 remote remove origin
  git -C D:\\r62-arch-a6 reflog expire --expire=now --all ; git -C D:\\r62-arch-a6 gc -q --prune=now
  D:\\r62-arch-a6-run\\order.txt = copia byte a byte de kit/order.txt (cuerpo §77); prompt.md lo copia finish_kit.py tras renderizarlo
No escribe en el worktree.
Uso: python -B prepare_clone.py [<CLONE_COMMIT>]
"""
import datetime
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
CLONE, RUN = r"D:\r62-arch-a6", r"D:\r62-arch-a6-run"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
BRANCH = "architecture/portabilidad-coordinador-principal"
PREP_COMMIT = "7b5ffac9d9c04f90b9708a867089d32e885dc815"
OBJECT, OBJECT_BLOB = "docs/initiatives/I-62-A-6.md", "eac473c0d2e58cf1c4594f48a8816b3833af6509"
ORDER_SHA = "e23faecbacc7ae33976f211ee381020b88dd08f38067eaa938c5b54ec36c4537"


def wt(*a):
    return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace",
                          env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode:
        raise SystemExit("falló: %s\n%s" % (" ".join(cmd).replace(os.environ["USERPROFILE"], "%USERPROFILE%"), r.stderr[-800:]))
    return r.stdout


def main(argv):
    if len(argv) > 1:
        print(__doc__)
        return 2
    clone_commit = argv[0] if argv else PREP_COMMIT
    if not re.match(r"^[0-9a-f]{40}$", clone_commit):
        raise SystemExit("se exige un SHA completo de 40 hexadecimales: %s" % clone_commit)
    for p in (CLONE, RUN, os.path.join(KIT, "kit-params.json")):
        if os.path.exists(p):
            raise SystemExit("ya existe %s: no se rehace (una sola preparación por kit)" % p)
    if hashlib.sha256(open(os.path.join(KIT, "order.txt"), "rb").read()).hexdigest() != ORDER_SHA:
        raise SystemExit("kit/order.txt distinto del cuerpo §77")
    tip = wt("rev-parse", "refs/heads/" + BRANCH).stdout.strip()
    dec = wt("show", clone_commit + ":docs/automation/decisions/I-62.md").stdout
    checks = {
        "CloneCommitIsCommit": wt("cat-file", "-e", clone_commit + "^{commit}").returncode == 0,
        "PrepAncestorOrEqualOfCloneCommit": wt("merge-base", "--is-ancestor", PREP_COMMIT, clone_commit).returncode == 0,
        "CloneCommitReachableFromBranchTip": wt("merge-base", "--is-ancestor", clone_commit, tip).returncode == 0,
        "ObjectBlobAtCloneCommit": wt("rev-parse", clone_commit + ":" + OBJECT).stdout.strip() == OBJECT_BLOB,
        "Decisions77AtCloneCommit": any(l.startswith("## 77. ") for l in dec.split("\n")),
    }
    if not all(checks.values()):
        print(json.dumps({"Refused": "precondiciones no satisfechas: no se escribe nada", "Checks": checks, "BranchTip": tip}, indent=1))
        return 2
    cmds = [
        ["git", "clone", "-q", "--no-local", "-c", "core.autocrlf=false", "--single-branch", "--branch", BRANCH, WORKTREE, CLONE],
        ["git", "-C", CLONE, "checkout", "-q", "--detach", clone_commit],
        ["git", "-C", CLONE, "branch", "-D", BRANCH],
        ["git", "-C", CLONE, "remote", "remove", "origin"],
        ["git", "-C", CLONE, "reflog", "expire", "--expire=now", "--all"],
        ["git", "-C", CLONE, "gc", "-q", "--prune=now"],
    ]
    for c in cmds:
        run(c)
    os.makedirs(RUN)
    shutil.copyfile(os.path.join(KIT, "order.txt"), os.path.join(RUN, "order.txt"))
    shown = [" ".join(c).replace(WORKTREE, WORKTREE_DISPLAY) for c in cmds] + [
        "copia byte a byte de kit/order.txt (= cuerpo §77) → D:\\r62-arch-a6-run\\order.txt",
        "copia byte a byte de kit/prompt.md (renderizado por make_closure.py --render-prompt) → D:\\r62-arch-a6-run\\prompt.md (finish_kit.py)"]
    params = {"Schema": "rackcad-a6-review-kit-params/0", "Provisional": False, "CloneCommit": clone_commit, "PreparationCommit": PREP_COMMIT,
              "Object": {"Path": OBJECT, "Blob": OBJECT_BLOB}, "Clone": CLONE, "Run": RUN, "Branch": BRANCH, "BranchTipAtClone": tip,
              "Worktree": WORKTREE_DISPLAY, "PreparedAt": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
              "PreconditionChecks": checks, "Commands": shown}
    with open(os.path.join(KIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": ["kit-params.json", CLONE, RUN + "\\order.txt"], "CloneCommit": clone_commit}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
