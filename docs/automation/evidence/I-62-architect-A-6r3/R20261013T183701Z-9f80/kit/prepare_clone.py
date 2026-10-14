"""Paso del clon de la ÚNICA revisión formal de la ronda 3 de A-6 (decisiones §84): crea el clon de lectura D:\\r62-arch-a6r3 y el run
D:\\r62-arch-a6r3-run, y escribe kit/kit-params.json. Lo ejecuta finish_kit.py (o quien prepara el kit), una sola vez. Adaptado del prepare_clone.py
del kit de la ronda 2.

Argumentos: <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT>, SHA completos. CLONE_COMMIT es el commit que nombra la orden del kit (contiene la
publicación de la ronda 3, la custodia del resultado `run` de sus guardas, decisiones §84 y las copias legibles de r3/); PUBLICATION, el commit
que publica la A-6 de la ronda 3 (su padre tiene la de la ronda 2); GUARDS_RUN_COMMIT, el que custodia el resultado `run`.
Comprueba antes de escribir nada (solo lectura de git en el worktree de I-62, con GIT_OPTIONAL_LOCKS=0): los tres son commits; CLONE_COMMIT es
alcanzable desde la punta de la rama; PUBLICATION ≤ GUARDS_RUN_COMMIT ≤ CLONE_COMMIT; el padre de PUBLICATION tiene la A-6 de la ronda 2
(86da0ae7); PUBLICATION, GUARDS_RUN_COMMIT y CLONE_COMMIT tienen el mismo blob de A-6, distinto de los de las rondas 1 y 2; en CLONE_COMMIT,
decisiones tiene §78, §83 y §84 (y §84 contiene el blob completo) y r3/ tiene el resultado `run` de las guardas; r3/ sin cambios desde la
publicación salvo añadidos; kit/order.txt es el cuerpo §84 y kit/a6-r2.md el blob 86da0ae7; ni el clon, ni el run, ni kit/kit-params.json existen
todavía. Después (HEAD separado en el commit, sin ramas locales ni remoto), clonando desde el directorio común de Git del worktree:
  git clone -q --no-local -c core.autocrlf=false --single-branch --branch <rama> <directorio común de git> D:\\r62-arch-a6r3
  git -C D:\\r62-arch-a6r3 checkout -q --detach <CLONE_COMMIT>
  git -C D:\\r62-arch-a6r3 branch -D <rama>
  git -C D:\\r62-arch-a6r3 remote remove origin
  git -C D:\\r62-arch-a6r3 reflog expire --expire=now --all ; git -C D:\\r62-arch-a6r3 gc -q --prune=now
  D:\\r62-arch-a6r3-run\\order.txt y a6-r2.md = copias byte a byte de kit/; prompt.md lo copia finish_kit.py tras renderizarlo
No escribe en el worktree ni en el repositorio de origen (git clone --no-local solo lee).
Uso: python -B prepare_clone.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT>
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
CLONE, RUN = r"D:\r62-arch-a6r3", r"D:\r62-arch-a6r3-run"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
BRANCH = "architecture/portabilidad-coordinador-principal"
OBJECT = "docs/initiatives/I-62-A-6.md"
A6R1_BLOB, A6R2_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509", "86da0ae770e683a647f2724860a4bd890061154d"
R3 = "docs/automation/evidence/I-62-A6/r3/"
RUN_RESULT = R3 + "a6r3-guards-result-run.json"
ORDER_SHA = "8a6b560f1da473ff152732e2e6bf2b82cd60f2df4ddfc7e44f8359edc7bef2e8"


def wt(*a):
    return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace",
                          env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    if r.returncode:
        raise SystemExit("falló: %s\n%s" % (" ".join(cmd).replace(os.environ["USERPROFILE"], "%USERPROFILE%"), r.stderr[-800:]))
    return r.stdout


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


def main(argv):
    if len(argv) != 3:
        print(__doc__)
        return 2
    clone_commit, pub, grc = argv
    for c in (clone_commit, pub, grc):
        if not re.match(r"^[0-9a-f]{40}$", c):
            raise SystemExit("se exige un SHA completo de 40 hexadecimales: %s" % c)
    for p in (CLONE, RUN, os.path.join(KIT, "kit-params.json")):
        if os.path.exists(p):
            raise SystemExit("ya existe %s: no se rehace (una sola preparación por kit)" % p)
    if hashlib.sha256(open(os.path.join(KIT, "order.txt"), "rb").read()).hexdigest() != ORDER_SHA:
        raise SystemExit("kit/order.txt distinto del cuerpo §84")
    if git_blob(open(os.path.join(KIT, "a6-r2.md"), "rb").read()) != A6R2_BLOB:
        raise SystemExit("kit/a6-r2.md no es el blob 86da0ae7 de la ronda 2")
    common = wt("rev-parse", "--path-format=absolute", "--git-common-dir").stdout.strip()
    tip = wt("rev-parse", "refs/heads/" + BRANCH).stdout.strip()
    parent = wt("rev-parse", pub + "^").stdout.strip()
    blob = wt("rev-parse", clone_commit + ":" + OBJECT).stdout.strip()
    dec = wt("show", clone_commit + ":docs/automation/decisions/I-62.md").stdout.split("\n")
    s84 = [i for i, l in enumerate(dec) if l.startswith("## 84. ")]
    s84_end = next((j for j in range(s84[0] + 1, len(dec)) if dec[j].startswith("## ")), len(dec)) if len(s84) == 1 else 0
    r3_pub = set(x for x in wt("ls-tree", "-r", "--name-only", pub, "--", R3).stdout.split("\n") if x)
    r3_now = set(x for x in wt("ls-tree", "-r", "--name-only", clone_commit, "--", R3).stdout.split("\n") if x)
    checks = {
        "CommonGitDirFound": bool(common) and os.path.isdir(common),
        "CommitsExist": all(wt("cat-file", "-e", c + "^{commit}").returncode == 0 for c in (clone_commit, pub, grc)),
        "CloneCommitReachableFromBranchTip": wt("merge-base", "--is-ancestor", clone_commit, tip).returncode == 0,
        "PublicationAncestorOfGuardsRun": wt("merge-base", "--is-ancestor", pub, grc).returncode == 0,
        "GuardsRunAncestorOfCloneCommit": wt("merge-base", "--is-ancestor", grc, clone_commit).returncode == 0,
        "Round2AtPublicationParent": wt("rev-parse", parent + ":" + OBJECT).stdout.strip() == A6R2_BLOB,
        "SameRound3BlobAtPublicationGuardsRunAndClone": blob not in (A6R1_BLOB, A6R2_BLOB) and re.match(r"^[0-9a-f]{40}$", blob or "") is not None and
        all(wt("rev-parse", c + ":" + OBJECT).stdout.strip() == blob for c in (pub, grc)),
        "Decisions78_83_84AtCloneCommit": all(any(l.startswith("## %d. " % n) for l in dec) for n in (78, 83, 84)),
        "Decisions84NamesTheBlob": len(s84) == 1 and bool(blob) and blob in "\n".join(dec[s84[0]:s84_end]),
        "GuardsRunResultAtCloneCommit": RUN_RESULT in r3_now,
        "R3UnchangedSincePublication": all(wt("rev-parse", pub + ":" + p).stdout.strip() == wt("rev-parse", clone_commit + ":" + p).stdout.strip()
                                           for p in r3_pub),
    }
    if not all(checks.values()):
        print(json.dumps({"Refused": "precondiciones no satisfechas: no se escribe nada", "Checks": checks, "BranchTip": tip}, indent=1))
        return 2
    cmds = [
        ["git", "clone", "-q", "--no-local", "-c", "core.autocrlf=false", "--single-branch", "--branch", BRANCH, common, CLONE],
        ["git", "-C", CLONE, "checkout", "-q", "--detach", clone_commit],
        ["git", "-C", CLONE, "branch", "-D", BRANCH],
        ["git", "-C", CLONE, "remote", "remove", "origin"],
        ["git", "-C", CLONE, "reflog", "expire", "--expire=now", "--all"],
        ["git", "-C", CLONE, "gc", "-q", "--prune=now"],
    ]
    for c in cmds:
        run(c)
    os.makedirs(RUN)
    for n in ("order.txt", "a6-r2.md"):
        shutil.copyfile(os.path.join(KIT, n), os.path.join(RUN, n))
    common_display = "<directorio común de git del worktree (el del repositorio de trabajo)>"
    shown = [" ".join(c).replace(common, common_display) for c in cmds] + [
        "copia byte a byte de kit/order.txt (= cuerpo §84) y kit/a6-r2.md (= blob 86da0ae7) → D:\\r62-arch-a6r3-run\\",
        "copia byte a byte de kit/prompt.md (renderizado por make_closure.py --render-prompt) → D:\\r62-arch-a6r3-run\\prompt.md (finish_kit.py)"]
    params = {"Schema": "rackcad-a6r3-review-kit-params/0", "Provisional": False, "CloneCommit": clone_commit, "Publication": pub,
              "PublicationParent": parent, "GuardsRunCommit": grc, "Object": {"Path": OBJECT, "Blob": blob},
              "R3AddedAfterPublication": sorted(r3_now - r3_pub), "Clone": CLONE, "Run": RUN, "Branch": BRANCH, "BranchTipAtClone": tip,
              "Worktree": WORKTREE_DISPLAY, "CloneSource": common_display,
              "PreparedAt": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
              "PreconditionChecks": checks, "Commands": shown}
    with open(os.path.join(KIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": ["kit-params.json", CLONE, RUN + "\\order.txt", RUN + "\\a6-r2.md"], "CloneCommit": clone_commit,
                      "ObjectBlob": blob, "R3AddedAfterPublication": params["R3AddedAfterPublication"], "Checks": checks}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
