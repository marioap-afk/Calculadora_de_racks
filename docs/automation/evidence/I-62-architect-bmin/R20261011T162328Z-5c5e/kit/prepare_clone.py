"""Paso que depende del commit del paquete B-min (PKG_COMMIT): crea el clon de lectura D:\\r62-arch-bmin y el run D:\\r62-arch-bmin-run, y escribe
kit/kit-params.json. Lo ejecuta la sesión principal (o finish_kit.py) cuando el paquete ya está custodiado en el worktree de I-62; nunca antes.

Comprueba antes de escribir nada (solo lectura de git en el worktree): PKG_COMMIT y CLONE_COMMIT (por defecto = PKG_COMMIT) son SHA completos de
commits alcanzables desde la punta de la rama de I-62; la base de la preparación (460665ab, decisiones §75) es antecesora estricta de PKG_COMMIT;
PKG_COMMIT es antecesor o igual de CLONE_COMMIT; ni el clon, ni el run, ni kit/kit-params.json existen todavía.
Después, con el patrón del kit de A-5 (HEAD separado en el commit, sin ramas locales ni remoto):
  git clone -q --no-local -c core.autocrlf=false --single-branch --branch <rama> <worktree> D:\\r62-arch-bmin
  git -C D:\\r62-arch-bmin checkout -q --detach <CLONE_COMMIT>
  git -C D:\\r62-arch-bmin branch -D <rama>
  git -C D:\\r62-arch-bmin remote remove origin
  git -C D:\\r62-arch-bmin reflog expire --expire=now --all ; git -C D:\\r62-arch-bmin gc -q --prune=now
  D:\\r62-arch-bmin-run\\order.txt = copia byte a byte de kit/order.txt (cuerpo §75); prompt.md lo copia finish_kit.py tras renderizarlo
No escribe en el worktree.
Uso: python -B prepare_clone.py <PKG_COMMIT> [<CLONE_COMMIT>]
"""
import datetime
import json
import os
import re
import shutil
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
CLONE, RUN = r"D:\r62-arch-bmin", r"D:\r62-arch-bmin-run"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
BRANCH = "architecture/portabilidad-coordinador-principal"
BASE_AT_PREP = "460665abd46e4f9f75c896eceecabe11689a6a40"
ORDER_SHA = "21435cd1c0821d8c934117a10e850cb9fa124a06f8dd589719aff8f5b711d630"


def wt(*a):
    return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True)


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode:
        raise SystemExit("falló: %s\n%s" % (" ".join(cmd).replace(os.environ["USERPROFILE"], "%USERPROFILE%"), r.stderr[-800:]))
    return r.stdout


def main(argv):
    if not argv or len(argv) > 2:
        print(__doc__)
        return 2
    pkg = argv[0]
    clone_commit = argv[1] if len(argv) > 1 else pkg
    for c in (pkg, clone_commit):
        if not re.match(r"^[0-9a-f]{40}$", c):
            raise SystemExit("se exige un SHA completo de 40 hexadecimales: %s" % c)
    for p in (CLONE, RUN, os.path.join(KIT, "kit-params.json")):
        if os.path.exists(p):
            raise SystemExit("ya existe %s: no se rehace (una sola preparación por kit)" % p)
    import hashlib
    if hashlib.sha256(open(os.path.join(KIT, "order.txt"), "rb").read()).hexdigest() != ORDER_SHA:
        raise SystemExit("kit/order.txt distinto del cuerpo §75")
    tip = wt("rev-parse", "refs/heads/" + BRANCH).stdout.strip()
    checks = {
        "PkgIsCommit": wt("cat-file", "-e", pkg + "^{commit}").returncode == 0,
        "CloneCommitIsCommit": wt("cat-file", "-e", clone_commit + "^{commit}").returncode == 0,
        "BaseStrictAncestorOfPkg": wt("merge-base", "--is-ancestor", BASE_AT_PREP, pkg).returncode == 0 and pkg != BASE_AT_PREP,
        "PkgAncestorOrEqualOfCloneCommit": wt("merge-base", "--is-ancestor", pkg, clone_commit).returncode == 0,
        "CloneCommitReachableFromBranchTip": wt("merge-base", "--is-ancestor", clone_commit, tip).returncode == 0,
        "PackagePresentAtPkg": wt("cat-file", "-e", pkg + ":docs/automation/evidence/I-62-F6/FX-02/bmin/SPEC.md").returncode == 0,
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
        "copia byte a byte de kit/order.txt (= cuerpo §75) → D:\\r62-arch-bmin-run\\order.txt",
        "copia byte a byte de kit/prompt.md (renderizado por make_closure.py --render-prompt) → D:\\r62-arch-bmin-run\\prompt.md (finish_kit.py)"]
    params = {"Schema": "rackcad-bmin-kit-params/0", "Provisional": False, "PkgCommit": pkg, "CloneCommit": clone_commit, "Clone": CLONE, "Run": RUN,
              "Branch": BRANCH, "BranchTipAtClone": tip, "BaseAtPreparation": BASE_AT_PREP, "Worktree": WORKTREE_DISPLAY,
              "PreparedAt": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"), "PreconditionChecks": checks, "Commands": shown}
    with open(os.path.join(KIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": ["kit-params.json", CLONE, RUN + "\\order.txt"], "PkgCommit": pkg, "CloneCommit": clone_commit}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
