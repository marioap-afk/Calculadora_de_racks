"""Paso del clon de la ÚNICA revisión formal de la ronda 2 de A-6 (decisiones §80, punto 6; fase 2): crea el clon de lectura D:\\r62-arch-a6r2 y el
run D:\\r62-arch-a6r2-run, y escribe kit/kit-params.json. Lo ejecuta finish_kit.py (o la sesión principal), una sola vez. Adaptado del
prepare_clone.py del kit de la ronda 1.

Argumentos: <CLONE_COMMIT> <PUBLICATION> [<GUARDS_RUN_COMMIT>], SHA completos. CLONE_COMMIT es el commit que nombra el mensaje de la fase 2 (la
publicación de la ronda 2 o un commit posterior que custodie el resultado `run` de sus guardas); PUBLICATION, el commit que publica la A-6 de la
ronda 2 (su padre tiene la de la ronda 1); GUARDS_RUN_COMMIT, el que custodia el resultado `run` (por defecto, CLONE_COMMIT).
Comprueba antes de escribir nada (solo lectura de git en el worktree de I-62, con GIT_OPTIONAL_LOCKS=0): los tres son commits; CLONE_COMMIT es
alcanzable desde la punta de la rama; PUBLICATION ≤ GUARDS_RUN_COMMIT ≤ CLONE_COMMIT; el padre de PUBLICATION tiene la A-6 de la ronda 1 (eac473c0);
PUBLICATION, GUARDS_RUN_COMMIT y CLONE_COMMIT tienen el mismo blob de A-6, distinto del de la ronda 1; en CLONE_COMMIT, decisiones tiene §78, §79 y §80
y r2/ tiene el resultado `run` de las guardas; kit/order.txt es el cuerpo §80 y kit/a6-r1.md el blob eac473c0; ni el clon, ni el run, ni
kit/kit-params.json existen todavía. Después, con el patrón de los kits anteriores (HEAD separado en el commit, sin ramas locales ni remoto):
  git clone -q --no-local -c core.autocrlf=false --single-branch --branch <rama> <worktree> D:\\r62-arch-a6r2
  git -C D:\\r62-arch-a6r2 checkout -q --detach <CLONE_COMMIT>
  git -C D:\\r62-arch-a6r2 branch -D <rama>
  git -C D:\\r62-arch-a6r2 remote remove origin
  git -C D:\\r62-arch-a6r2 reflog expire --expire=now --all ; git -C D:\\r62-arch-a6r2 gc -q --prune=now
  D:\\r62-arch-a6r2-run\\order.txt y a6-r1.md = copias byte a byte de kit/; prompt.md lo copia finish_kit.py tras renderizarlo
No escribe en el worktree.
Uso: python -B prepare_clone.py <CLONE_COMMIT> <PUBLICATION> [<GUARDS_RUN_COMMIT>]
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
CLONE, RUN = r"D:\r62-arch-a6r2", r"D:\r62-arch-a6r2-run"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
BRANCH = "architecture/portabilidad-coordinador-principal"
OBJECT, A6R1_BLOB = "docs/initiatives/I-62-A-6.md", "eac473c0d2e58cf1c4594f48a8816b3833af6509"
R2 = "docs/automation/evidence/I-62-A6/r2/"
RUN_RESULT = R2 + "a6r2-guards-result-run.json"
ORDER_SHA = "1342b5be1a5de104ac08a1115ddc999d2e6dfbe80100affaa16ea44439fa7f99"


def wt(*a):
    return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace",
                          env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode:
        raise SystemExit("falló: %s\n%s" % (" ".join(cmd).replace(os.environ["USERPROFILE"], "%USERPROFILE%"), r.stderr[-800:]))
    return r.stdout


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


def main(argv):
    if len(argv) not in (2, 3):
        print(__doc__)
        return 2
    clone_commit, pub = argv[0], argv[1]
    grc = argv[2] if len(argv) == 3 else clone_commit
    for c in (clone_commit, pub, grc):
        if not re.match(r"^[0-9a-f]{40}$", c):
            raise SystemExit("se exige un SHA completo de 40 hexadecimales: %s" % c)
    for p in (CLONE, RUN, os.path.join(KIT, "kit-params.json")):
        if os.path.exists(p):
            raise SystemExit("ya existe %s: no se rehace (una sola preparación por kit)" % p)
    if hashlib.sha256(open(os.path.join(KIT, "order.txt"), "rb").read()).hexdigest() != ORDER_SHA:
        raise SystemExit("kit/order.txt distinto del cuerpo §80")
    if git_blob(open(os.path.join(KIT, "a6-r1.md"), "rb").read()) != A6R1_BLOB:
        raise SystemExit("kit/a6-r1.md no es el blob eac473c0 de la ronda 1")
    tip = wt("rev-parse", "refs/heads/" + BRANCH).stdout.strip()
    parent = wt("rev-parse", pub + "^").stdout.strip()
    blob = wt("rev-parse", clone_commit + ":" + OBJECT).stdout.strip()
    dec = wt("show", clone_commit + ":docs/automation/decisions/I-62.md").stdout.split("\n")
    r2_pub = set(x for x in wt("ls-tree", "-r", "--name-only", pub, "--", R2).stdout.split("\n") if x)
    r2_now = set(x for x in wt("ls-tree", "-r", "--name-only", clone_commit, "--", R2).stdout.split("\n") if x)
    checks = {
        "CommitsExist": all(wt("cat-file", "-e", c + "^{commit}").returncode == 0 for c in (clone_commit, pub, grc)),
        "CloneCommitReachableFromBranchTip": wt("merge-base", "--is-ancestor", clone_commit, tip).returncode == 0,
        "PublicationAncestorOfGuardsRun": wt("merge-base", "--is-ancestor", pub, grc).returncode == 0,
        "GuardsRunAncestorOfCloneCommit": wt("merge-base", "--is-ancestor", grc, clone_commit).returncode == 0,
        "Round1AtPublicationParent": wt("rev-parse", parent + ":" + OBJECT).stdout.strip() == A6R1_BLOB,
        "SameRound2BlobAtPublicationGuardsRunAndClone": blob != A6R1_BLOB and re.match(r"^[0-9a-f]{40}$", blob or "") is not None and
        all(wt("rev-parse", c + ":" + OBJECT).stdout.strip() == blob for c in (pub, grc)),
        "Decisions78to80AtCloneCommit": all(any(l.startswith("## %d. " % n) for l in dec) for n in (78, 79, 80)),
        "GuardsRunResultAtCloneCommit": RUN_RESULT in r2_now,
        "R2UnchangedSincePublication": all(wt("rev-parse", pub + ":" + p).stdout.strip() == wt("rev-parse", clone_commit + ":" + p).stdout.strip()
                                           for p in r2_pub),
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
    for n in ("order.txt", "a6-r1.md"):
        shutil.copyfile(os.path.join(KIT, n), os.path.join(RUN, n))
    shown = [" ".join(c).replace(WORKTREE, WORKTREE_DISPLAY) for c in cmds] + [
        "copia byte a byte de kit/order.txt (= cuerpo §80) y kit/a6-r1.md (= blob eac473c0) → D:\\r62-arch-a6r2-run\\",
        "copia byte a byte de kit/prompt.md (renderizado por make_closure.py --render-prompt) → D:\\r62-arch-a6r2-run\\prompt.md (finish_kit.py)"]
    params = {"Schema": "rackcad-a6r2-review-kit-params/0", "Provisional": False, "CloneCommit": clone_commit, "Publication": pub,
              "PublicationParent": parent, "GuardsRunCommit": grc, "Object": {"Path": OBJECT, "Blob": blob},
              "R2AddedAfterPublication": sorted(r2_now - r2_pub), "Clone": CLONE, "Run": RUN, "Branch": BRANCH, "BranchTipAtClone": tip,
              "Worktree": WORKTREE_DISPLAY, "PreparedAt": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
              "PreconditionChecks": checks, "Commands": shown}
    with open(os.path.join(KIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": ["kit-params.json", CLONE, RUN + "\\order.txt", RUN + "\\a6-r1.md"], "CloneCommit": clone_commit,
                      "ObjectBlob": blob, "R2AddedAfterPublication": params["R2AddedAfterPublication"]}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
