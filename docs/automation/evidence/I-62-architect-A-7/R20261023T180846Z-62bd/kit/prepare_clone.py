"""Paso del clon de la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7 (decisiones §96, puntos 6 y 7): crea el clon de lectura
D:\\r62-arch-a7r3 y el run D:\\r62-arch-a7r3-run, y escribe kit/kit-params.json. Lo ejecuta quien prepara el kit, una sola vez. Derivado del
prepare_clone.py del kit de A-7 ronda 2 (R20261019T201030Z-d1bf, a su vez del de la ronda 1): el mismo clon (desde el directorio común de git,
HEAD separado, sin ramas ni remoto) y las mismas precondiciones de solo lectura, con el objeto de la ronda 3, su evidencia (I-62-A7/r3/), la
evidencia de la ronda 2 (I-62-A7/r2/) y la revisión de la ronda 2 como evidencia previa, y la sección §96.

Argumentos: <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT>, SHA completos. CLONE_COMMIT es el commit que contiene decisiones §96 (la orden del
kit), la publicación de la ronda 3 y la custodia del resultado `run` de sus guardas (b9bfae50); PUBLICATION, el commit que publica la ronda 3
(37ac6ca4; su padre, 5da2007f, tiene la ronda 2, blob 8a528133…, y §96); GUARDS_RUN_COMMIT, el que custodia el resultado `run`
(docs/automation/evidence/I-62-A7/r3/a7-guards-result-run.json).
Comprueba antes de escribir nada (solo lectura de git en el worktree de I-62, con GIT_OPTIONAL_LOCKS=0): los tres son commits; CLONE_COMMIT es
alcanzable desde la punta de la rama; PUBLICATION ≤ GUARDS_RUN_COMMIT ≤ CLONE_COMMIT; el padre de PUBLICATION tiene la ronda 2 (8a528133…) y no
tiene I-62-A7/r3/; PUBLICATION, GUARDS_RUN_COMMIT y CLONE_COMMIT tienen el blob de la ronda 3 fijado (8bcccb2f…) y la A-6 acordada (4bc919dd…); en
CLONE_COMMIT, decisiones tiene §78, §95 y §96, y §96 es su última sección numerada; I-62-A7/r3/ tiene el resultado `run` y no cambia desde la
publicación salvo añadidos; la evidencia de la ronda 2 (I-62-A7/r2/) no cambia del padre de la publicación al commit del clon; la revisión de la
ronda 2 (docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/) está en el commit del clon; kit/order.txt es el cuerpo §96 y
kit/a7-r2.md el blob de la ronda 2; ni el clon, ni el run, ni kit/kit-params.json existen todavía. Después (HEAD separado en el commit, sin ramas
locales ni remoto), clonando desde el directorio común de Git del worktree:
  git clone -q --no-local -c core.autocrlf=false --single-branch --branch <rama> <directorio común de git> D:\\r62-arch-a7r3
  git -C D:\\r62-arch-a7r3 checkout -q --detach <CLONE_COMMIT>
  git -C D:\\r62-arch-a7r3 branch -D <rama>
  git -C D:\\r62-arch-a7r3 remote remove origin
  git -C D:\\r62-arch-a7r3 reflog expire --expire=now --all ; git -C D:\\r62-arch-a7r3 gc -q --prune=now
  D:\\r62-arch-a7r3-run\\order.txt y a7-r2.md = copias byte a byte de kit/; prompt.md y las copias fieles los copia quien termina el kit
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
CLONE, RUN = r"D:\r62-arch-a7r3", r"D:\r62-arch-a7r3-run"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
BRANCH = "architecture/portabilidad-coordinador-principal"
OBJECT = "docs/initiatives/I-62-A-7.md"
OBJECT_BLOB = "8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff"                 # A-7 ronda 3 (decisiones §96)
PREV_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"                   # A-7 ronda 2 (kit/a7-r2.md; padre de la publicación)
A6, A6_AGREED_BLOB = "docs/initiatives/I-62-A-6.md", "4bc919dd4768ab81abc74b70d3a4a3821c07feb5"
A7EVID = "docs/automation/evidence/I-62-A7/"
EVID = A7EVID + "r3/"
R2EVID = A7EVID + "r2/"
R2_REVIEW = "docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/"
RUN_RESULT = EVID + "a7-guards-result-run.json"
ORDER_SHA = "adb11535f24cac2874ddef13af712cc56eebece8905fe948db2f0077b3dd87b2"   # cuerpo pegado de §96 (3 198 bytes)
LAST_SECTION = 96


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


def wt(*a):
    return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace",
                          env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    if r.returncode:
        raise SystemExit("falló: %s\n%s" % (" ".join(cmd).replace(os.environ["USERPROFILE"], "%USERPROFILE%"), r.stderr[-800:]))
    return r.stdout


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
        raise SystemExit("kit/order.txt distinto del cuerpo §96")
    if git_blob(open(os.path.join(KIT, "a7-r2.md"), "rb").read()) != PREV_BLOB:
        raise SystemExit("kit/a7-r2.md no es el blob 8a528133 de la ronda 2")
    common = wt("rev-parse", "--path-format=absolute", "--git-common-dir").stdout.strip()
    tip = wt("rev-parse", "refs/heads/" + BRANCH).stdout.strip()
    parent = wt("rev-parse", pub + "^").stdout.strip()
    blob = wt("rev-parse", clone_commit + ":" + OBJECT).stdout.strip()
    dec = wt("show", clone_commit + ":docs/automation/decisions/I-62.md").stdout.split("\n")
    s_last = [i for i, l in enumerate(dec) if l.startswith("## %d. " % LAST_SECTION)]
    numbered = [int(m.group(1)) for m in (re.match(r"^## ([0-9]+)\. ", l) for l in dec) if m]
    ev_parent = set(x for x in wt("ls-tree", "-r", "--name-only", parent, "--", EVID).stdout.split("\n") if x)
    ev_pub = set(x for x in wt("ls-tree", "-r", "--name-only", pub, "--", EVID).stdout.split("\n") if x)
    ev_now = set(x for x in wt("ls-tree", "-r", "--name-only", clone_commit, "--", EVID).stdout.split("\n") if x)
    r2_now = sorted(x for x in wt("ls-tree", "-r", "--name-only", clone_commit, "--", R2EVID).stdout.split("\n") if x)
    r2_parent = sorted(x for x in wt("ls-tree", "-r", "--name-only", parent, "--", R2EVID).stdout.split("\n") if x)
    checks = {
        "CommonGitDirFound": bool(common) and os.path.isdir(common),
        "CommitsExist": all(wt("cat-file", "-e", c + "^{commit}").returncode == 0 for c in (clone_commit, pub, grc)),
        "CloneCommitReachableFromBranchTip": wt("merge-base", "--is-ancestor", clone_commit, tip).returncode == 0,
        "PublicationAncestorOfGuardsRun": wt("merge-base", "--is-ancestor", pub, grc).returncode == 0,
        "GuardsRunAncestorOfCloneCommit": wt("merge-base", "--is-ancestor", grc, clone_commit).returncode == 0,
        "Round2AtPublicationParent": wt("rev-parse", parent + ":" + OBJECT).stdout.strip() == PREV_BLOB,
        "R3EvidenceAbsentAtPublicationParent": not ev_parent,
        "SameA7BlobAtPublicationGuardsRunAndClone": blob == OBJECT_BLOB and all(wt("rev-parse", c + ":" + OBJECT).stdout.strip() == blob
                                                                                for c in (pub, grc)),
        "AgreedA6BlobAtParentPublicationGuardsRunAndClone": all(wt("rev-parse", c + ":" + A6).stdout.strip() == A6_AGREED_BLOB
                                                                for c in (parent, pub, grc, clone_commit)),
        "Decisions78_95_96AtCloneCommit": all(any(l.startswith("## %d. " % n) for l in dec) for n in (78, 95, 96)),
        "Decisions96IsTheLastSectionAtCloneCommit": len(s_last) == 1 and bool(numbered) and max(numbered) == LAST_SECTION,
        "GuardsRunResultAtCloneCommit": RUN_RESULT in ev_now and RUN_RESULT not in ev_pub,
        "R3EvidenceUnchangedSincePublication": bool(ev_pub) and all(wt("rev-parse", pub + ":" + p).stdout.strip() ==
                                                                    wt("rev-parse", clone_commit + ":" + p).stdout.strip() for p in ev_pub),
        "R2EvidenceUnchangedFromParentToClone": bool(r2_now) and r2_now == r2_parent and all(
            wt("rev-parse", parent + ":" + p).stdout.strip() == wt("rev-parse", clone_commit + ":" + p).stdout.strip() for p in r2_now),
        "Round2ReviewAtCloneCommit": all(wt("cat-file", "-e", clone_commit + ":" + R2_REVIEW + n).returncode == 0
                                         for n in ("output.json", "audit.json", "README.md")),
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
    for n in ("order.txt", "a7-r2.md"):
        shutil.copyfile(os.path.join(KIT, n), os.path.join(RUN, n))
    common_display = "<directorio común de git del worktree (el del repositorio de trabajo)>"
    shown = [" ".join(c).replace(common, common_display) for c in cmds] + [
        "copia byte a byte de kit/order.txt (= cuerpo §96) y kit/a7-r2.md (= blob 8a528133 de la ronda 2) → D:\\r62-arch-a7r3-run\\",
        "copia byte a byte de kit/prompt.md y de las copias fieles kit/fiel__*.txt (make_closure.py) → D:\\r62-arch-a7r3-run\\"]
    params = {"Schema": "rackcad-a7r3-review-kit-params/0", "Provisional": False, "CloneCommit": clone_commit, "Publication": pub,
              "PublicationParent": parent, "GuardsRunCommit": grc, "Object": {"Path": OBJECT, "Blob": blob},
              "PreviousObject": {"Path": OBJECT, "Blob": PREV_BLOB, "RunFile": "a7-r2.md"},
              "A6Agreed": {"Path": A6, "Blob": A6_AGREED_BLOB},
              "A7EvidenceAddedAfterPublication": sorted(ev_now - ev_pub), "Clone": CLONE, "Run": RUN, "Branch": BRANCH, "BranchTipAtClone": tip,
              "Worktree": WORKTREE_DISPLAY, "CloneSource": common_display,
              "PreparedAt": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
              "PreconditionChecks": checks, "Commands": shown}
    with open(os.path.join(KIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": ["kit-params.json", CLONE, RUN + "\\order.txt", RUN + "\\a7-r2.md"], "CloneCommit": clone_commit,
                      "ObjectBlob": blob, "A7EvidenceAddedAfterPublication": params["A7EvidenceAddedAfterPublication"], "Checks": checks},
                     ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
