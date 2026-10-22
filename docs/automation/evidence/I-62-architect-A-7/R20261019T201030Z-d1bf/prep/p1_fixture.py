"""Entorno de prueba de la FASE 1 (decisiones §94, punto 5): crea el clon de prueba <dst> desde el clon de r1 (D:\\r62-arch-a7, solo lectura:
`git clone --no-local` no escribe en el origen) en su commit 5158e4bc, y añade UN commit de fixture (autor y fecha fijos, sin datos del host) con
archivos que el detector debe marcar, uno por clase, más un control limpio. El commit de fixture solo existe en este clon de prueba: nunca en el
repositorio, en el worktree ni en el clon de r1. Después: HEAD separado en el commit de fixture, sin ramas, sin remoto, reflog expirado, gc.

Uso: python -I p1_fixture.py <origen> <destino>
"""
import os
import subprocess
import sys

SRC, DST = sys.argv[1], sys.argv[2]
BASE = "5158e4bc17ad48160d8573b7f48f3923e3197f06"
DIR = "zz-fidelity-fixture"
BOM = b"\xef\xbb\xbf"
FILES = {
    "bom.trx": BOM + b'<?xml version="1.0" encoding="utf-8"?>\n<TestRun id="fixture" name="p1">\n  <ResultSummary outcome="Completed" />\n</TestRun>',
    "bare-cr.txt": b"linea 1\nantes\rdespues del CR suelto\nlinea 3\n",
    "nul-byte.txt": b"linea 1\ncon un \x00 NUL en medio\nlinea 3\n",
    "invalid-utf8.txt": b"linea 1\nbyte \xff suelto y secuencia truncada \xc3\nlinea 3\n",
    "control.txt": b"a\tb (TAB)\nESC \x1b[0m\nDEL \x7f\n",
    "long-line.txt": b"corta\n" + b"x" * 1000 + (b"ab\\c de " * 300) + b"fin\n" + b"otra\n",
    "zwnbsp.txt": ("inicio\nmedio" + chr(0xFEFF) + "interior\n").encode("utf-8"),
    "line-separator.txt": ("a" + chr(0x2028) + "b\nfin\n").encode("utf-8"),
    "crlf-bom.txt": BOM + b"uno\r\ndos\r\n",
    "doc.pdf": b"no es un PDF: texto de prueba\n",
    "clean.txt": "texto limpio con acentos: línea, sí, ñ, « » — § → ≤\n".encode("utf-8"),
}
ENV = dict(os.environ, GIT_AUTHOR_NAME="RackCad p1 fixture", GIT_AUTHOR_EMAIL="fixture@invalid", GIT_COMMITTER_NAME="RackCad p1 fixture",
           GIT_COMMITTER_EMAIL="fixture@invalid", GIT_AUTHOR_DATE="2026-10-19T00:00:00Z", GIT_COMMITTER_DATE="2026-10-19T00:00:00Z")


def g(*a, cwd=None):
    r = subprocess.run(["git"] + list(a), capture_output=True, env=ENV, cwd=cwd)
    if r.returncode:
        raise SystemExit("git %s: %s" % (a, r.stderr.decode("utf-8", "replace")))
    return r.stdout.decode("utf-8", "replace").strip()


assert not os.path.exists(DST), "el destino ya existe"
assert g("-C", SRC, "rev-parse", "HEAD") == BASE, "el origen no está en 5158e4bc"
g("clone", "--no-local", "-q", "-c", "core.autocrlf=false", SRC, DST)
g("-C", DST, "checkout", "-q", "--detach", BASE)
for ref in [x for x in g("-C", DST, "for-each-ref", "--format=%(refname)", "refs/heads").split("\n") if x]:
    g("-C", DST, "update-ref", "-d", ref)
for r in [x for x in g("-C", DST, "remote").split("\n") if x]:
    g("-C", DST, "remote", "remove", r)
os.makedirs(os.path.join(DST, DIR))
for n, b in FILES.items():
    with open(os.path.join(DST, DIR, n), "wb") as f:
        f.write(b)
g("-C", DST, "add", "--", DIR)
g("-C", DST, "-c", "core.autocrlf=false", "-c", "commit.gpgsign=false", "commit", "-q", "--no-verify", "-m",
  "p1: fixture de fidelidad de lectura (solo en el clon de prueba de la fase 1; decisiones §94, punto 5)")
head = g("-C", DST, "rev-parse", "HEAD")
g("-C", DST, "checkout", "-q", "--detach", head)
for ref in [x for x in g("-C", DST, "for-each-ref", "--format=%(refname)", "refs/heads").split("\n") if x]:
    g("-C", DST, "update-ref", "-d", ref)
g("-C", DST, "reflog", "expire", "--expire=now", "--all")
g("-C", DST, "gc", "-q", "--prune=now")
blobs = {n: g("-C", DST, "rev-parse", "HEAD:%s/%s" % (DIR, n)) for n in FILES}
print({"FixtureCommit": head, "Parent": g("-C", DST, "rev-parse", "HEAD^"), "Status": g("-C", DST, "status", "--porcelain", "--ignored"),
       "Branches": g("-C", DST, "for-each-ref", "refs/heads"), "Remotes": g("-C", DST, "remote"), "Files": blobs})
