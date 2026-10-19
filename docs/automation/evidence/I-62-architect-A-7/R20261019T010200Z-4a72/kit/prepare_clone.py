"""Paso del clon de la ÚNICA revisión formal de la enmienda A-7 (decisiones §93): crea el clon de lectura D:\\r62-arch-a7 y el run D:\\r62-arch-a7-run,
y escribe kit/kit-params.json. Lo ejecuta finish_kit.py (o quien prepara el kit), una sola vez. Derivado del prepare_clone.py del kit acreditado de
A-6 ronda 4 (R20261014T180556Z-5e02): el mismo clon (desde el directorio común de git, HEAD separado, sin ramas ni remoto) y las mismas
precondiciones de solo lectura, con el objeto, la evidencia y la sección de decisiones de A-7.

Argumentos: <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT>, SHA completos. CLONE_COMMIT es el commit que contiene decisiones §93 (la orden del
kit y la autorización), la publicación de A-7 y la custodia del resultado `run` de sus guardas; PUBLICATION, el commit que publica A-7 (su padre no
tiene A-7); GUARDS_RUN_COMMIT, el que custodia el resultado `run` (docs/automation/evidence/I-62-A7/a7-guards-result-run.json).
Comprueba antes de escribir nada (solo lectura de git en el worktree de I-62, con GIT_OPTIONAL_LOCKS=0): los tres son commits; CLONE_COMMIT es
alcanzable desde la punta de la rama; PUBLICATION ≤ GUARDS_RUN_COMMIT ≤ CLONE_COMMIT; el padre de PUBLICATION no tiene A-7; PUBLICATION,
GUARDS_RUN_COMMIT y CLONE_COMMIT tienen el blob de A-7 fijado (6c29747d…) y la A-6 acordada (4bc919dd…); en CLONE_COMMIT, decisiones tiene §78,
§92 y §93, y §93 es su última sección numerada, y I-62-A7/ tiene el resultado `run` de las guardas; I-62-A7/ sin cambios desde la publicación salvo
añadidos; kit/order.txt es el cuerpo §93; ni el clon, ni el run, ni kit/kit-params.json existen todavía. Después (HEAD separado en el commit, sin
ramas locales ni remoto), clonando desde el directorio común de Git del worktree:
  git clone -q --no-local -c core.autocrlf=false --single-branch --branch <rama> <directorio común de git> D:\\r62-arch-a7
  git -C D:\\r62-arch-a7 checkout -q --detach <CLONE_COMMIT>
  git -C D:\\r62-arch-a7 branch -D <rama>
  git -C D:\\r62-arch-a7 remote remove origin
  git -C D:\\r62-arch-a7 reflog expire --expire=now --all ; git -C D:\\r62-arch-a7 gc -q --prune=now
  D:\\r62-arch-a7-run\\order.txt = copia byte a byte de kit/; prompt.md lo copia quien termina el kit tras renderizarlo
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
CLONE, RUN = r"D:\r62-arch-a7", r"D:\r62-arch-a7-run"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
BRANCH = "architecture/portabilidad-coordinador-principal"
OBJECT = "docs/initiatives/I-62-A-7.md"
OBJECT_BLOB = "6c29747d61c5f689a195095ad3712e6d9dba62aa"
A6, A6_AGREED_BLOB = "docs/initiatives/I-62-A-6.md", "4bc919dd4768ab81abc74b70d3a4a3821c07feb5"
EVID = "docs/automation/evidence/I-62-A7/"
RUN_RESULT = EVID + "a7-guards-result-run.json"
ORDER_SHA = "a5c37f714364bc704c5f38218ae2b1a9209052c47834dcc5349684eb141be41b"
AUTH_SECTION = 93


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
        raise SystemExit("kit/order.txt distinto del cuerpo §93")
    common = wt("rev-parse", "--path-format=absolute", "--git-common-dir").stdout.strip()
    tip = wt("rev-parse", "refs/heads/" + BRANCH).stdout.strip()
    parent = wt("rev-parse", pub + "^").stdout.strip()
    blob = wt("rev-parse", clone_commit + ":" + OBJECT).stdout.strip()
    dec = wt("show", clone_commit + ":docs/automation/decisions/I-62.md").stdout.split("\n")
    s93 = [i for i, l in enumerate(dec) if l.startswith("## %d. " % AUTH_SECTION)]
    numbered = [int(m.group(1)) for m in (re.match(r"^## ([0-9]+)\. ", l) for l in dec) if m]
    ev_pub = set(x for x in wt("ls-tree", "-r", "--name-only", pub, "--", EVID).stdout.split("\n") if x)
    ev_now = set(x for x in wt("ls-tree", "-r", "--name-only", clone_commit, "--", EVID).stdout.split("\n") if x)
    checks = {
        "CommonGitDirFound": bool(common) and os.path.isdir(common),
        "CommitsExist": all(wt("cat-file", "-e", c + "^{commit}").returncode == 0 for c in (clone_commit, pub, grc)),
        "CloneCommitReachableFromBranchTip": wt("merge-base", "--is-ancestor", clone_commit, tip).returncode == 0,
        "PublicationAncestorOfGuardsRun": wt("merge-base", "--is-ancestor", pub, grc).returncode == 0,
        "GuardsRunAncestorOfCloneCommit": wt("merge-base", "--is-ancestor", grc, clone_commit).returncode == 0,
        "A7AbsentAtPublicationParent": wt("cat-file", "-e", parent + ":" + OBJECT).returncode != 0,
        "SameA7BlobAtPublicationGuardsRunAndClone": blob == OBJECT_BLOB and all(wt("rev-parse", c + ":" + OBJECT).stdout.strip() == blob
                                                                                for c in (pub, grc)),
        "AgreedA6BlobAtPublicationGuardsRunAndClone": all(wt("rev-parse", c + ":" + A6).stdout.strip() == A6_AGREED_BLOB
                                                          for c in (pub, grc, clone_commit)),
        "Decisions78_92_93AtCloneCommit": all(any(l.startswith("## %d. " % n) for l in dec) for n in (78, 92, 93)),
        "Decisions93IsTheLastSectionAtCloneCommit": len(s93) == 1 and bool(numbered) and max(numbered) == AUTH_SECTION,
        "GuardsRunResultAtCloneCommit": RUN_RESULT in ev_now,
        "A7EvidenceUnchangedSincePublication": all(wt("rev-parse", pub + ":" + p).stdout.strip() == wt("rev-parse", clone_commit + ":" + p).stdout.strip()
                                                   for p in ev_pub),
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
    shutil.copyfile(os.path.join(KIT, "order.txt"), os.path.join(RUN, "order.txt"))
    common_display = "<directorio común de git del worktree (el del repositorio de trabajo)>"
    shown = [" ".join(c).replace(common, common_display) for c in cmds] + [
        "copia byte a byte de kit/order.txt (= cuerpo §93) → D:\\r62-arch-a7-run\\",
        "copia byte a byte de kit/prompt.md (renderizado por make_closure.py --render-prompt) → D:\\r62-arch-a7-run\\prompt.md"]
    params = {"Schema": "rackcad-a7-review-kit-params/0", "Provisional": False, "CloneCommit": clone_commit, "Publication": pub,
              "PublicationParent": parent, "GuardsRunCommit": grc, "Object": {"Path": OBJECT, "Blob": blob},
              "A6Agreed": {"Path": A6, "Blob": A6_AGREED_BLOB},
              "A7EvidenceAddedAfterPublication": sorted(ev_now - ev_pub), "Clone": CLONE, "Run": RUN, "Branch": BRANCH, "BranchTipAtClone": tip,
              "Worktree": WORKTREE_DISPLAY, "CloneSource": common_display,
              "PreparedAt": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
              "PreconditionChecks": checks, "Commands": shown}
    with open(os.path.join(KIT, "kit-params.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(params, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": ["kit-params.json", CLONE, RUN + "\\order.txt"], "CloneCommit": clone_commit,
                      "ObjectBlob": blob, "A7EvidenceAddedAfterPublication": params["A7EvidenceAddedAfterPublication"], "Checks": checks},
                     ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
