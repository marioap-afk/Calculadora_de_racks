# Solo lectura: el MISMO criterio de count_tests.py (recuento ESTATICO de casos por clase: Fact = 1; Theory = numero de
# InlineData; Theory con MemberData/ClassData = 'M'; los Skip aparte), pero leyendo los archivos de una REVISION por objetos
# git (ls-tree + cat-file) en vez de un arbol de trabajo. No escribe nada en el repositorio.
# Uso: python count_tests_git.py <checkout-que-comparte-el-almacen> <rev> <UI|Core> <prefijo> [<prefijo> ...]
#      con COUNT_DETAIL=--detail en el entorno, imprime tambien cada clase.
import os
import re
import subprocess
import sys

repo, rev, project = sys.argv[1], sys.argv[2], sys.argv[3]
prefixes = sys.argv[4:]
folder = "tests/RackCad.UI.Tests" if project == "UI" else "tests/RackCad.Tests"
ns_prefix = "RackCad.UI.Tests" if project == "UI" else "RackCad.Tests"

names = subprocess.run(["git", "-C", repo, "ls-tree", "-r", "--name-only", rev, "--", folder],
                       capture_output=True, check=True).stdout.decode("utf-8").split("\n")
names = [n for n in names if n.endswith(".cs") and "/bin/" not in n and "/obj/" not in n]

class_re = re.compile(r"^\s*(?:public|internal)\s+(?:sealed\s+|static\s+|partial\s+|abstract\s+)*class\s+(\w+)")
ns_re = re.compile(r"^\s*namespace\s+([\w\.]+)")


def blobs(paths):
    """Todos los blobs en una sola llamada a cat-file --batch (mismo contenido que git show <rev>:<ruta>)."""
    query = "".join(rev + ":" + p + "\n" for p in paths).encode("utf-8")
    out = subprocess.run(["git", "-C", repo, "cat-file", "--batch"], input=query, capture_output=True, check=True).stdout
    result, pos = {}, 0
    for p in paths:
        nl = out.index(b"\n", pos)
        header = out[pos:nl].decode("utf-8").split()
        size = int(header[2])
        data = out[nl + 1: nl + 1 + size]
        pos = nl + 1 + size + 1
        result[p] = data.decode("utf-8-sig")
    return result


totals = {}
contents = blobs(names)
for path in names:
    lines = contents[path].split("\n")
    ns = ""
    current = None
    pending = None
    inline = 0
    for line in lines:
        m = ns_re.match(line)
        if m:
            ns = m.group(1).rstrip(";")
        m = class_re.match(line)
        if m:
            current = m.group(1)
        s = line.strip()
        if s.startswith("[Fact"):
            pending = ("fact", "Skip" in s)
        elif s.startswith("[Theory"):
            pending = ("theory", "Skip" in s)
            inline = 0
        elif s.startswith("[InlineData") and pending and pending[0] == "theory":
            inline += 1
        elif (s.startswith("[MemberData") or s.startswith("[ClassData")) and pending and pending[0] == "theory":
            inline = -10**6
        elif re.match(r"(public|internal|private)\s+.*\(", s) and pending:
            key = (ns + "." + current) if current else ns
            kind, skip = pending
            n = 1 if kind == "fact" else inline
            entry = totals.setdefault(key, [0, 0, 0, path])
            if n < 0:
                entry[2] += 1
            elif skip:
                entry[1] += n
            else:
                entry[0] += n
            pending = None

for prefix in prefixes:
    full_prefix = ns_prefix + "." + prefix
    sel = {k: v for k, v in totals.items() if k.startswith(full_prefix)}
    run = sum(v[0] for v in sel.values())
    skipped = sum(v[1] for v in sel.values())
    member = sum(v[2] for v in sel.values())
    print(f"{project} {prefix}: clases={len(sel)} ejecutables={run} skip={skipped} theory-memberdata={member}")
    if "--detail" in os.environ.get("COUNT_DETAIL", ""):
        for k in sorted(sel):
            print("   ", k, sel[k][:3], sel[k][3])
