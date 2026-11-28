# Solo lectura: recuento ESTATICO de casos de prueba por clase (Fact = 1; Theory = numero de InlineData;
# Theory con MemberData/ClassData = 'M'). Cuenta tambien los Skip.
# Uso: python -I count_tests.py <raiz-del-worktree> <proyecto: UI|Core> <prefijo-de-clase> [<prefijo> ...]
import os
import re
import sys

root, project = sys.argv[1], sys.argv[2]
prefixes = sys.argv[3:]
folder = os.path.join(root, "tests", "RackCad.UI.Tests" if project == "UI" else "RackCad.Tests")
ns_prefix = "RackCad.UI.Tests" if project == "UI" else "RackCad.Tests"

class_re = re.compile(r"^\s*(?:public|internal)\s+(?:sealed\s+|static\s+|partial\s+|abstract\s+)*class\s+(\w+)")
ns_re = re.compile(r"^\s*namespace\s+([\w\.]+)")

totals = {}
for dirpath, _, files in os.walk(folder):
    if os.sep + "bin" in dirpath or os.sep + "obj" in dirpath:
        continue
    for name in files:
        if not name.endswith(".cs"):
            continue
        path = os.path.join(dirpath, name)
        with open(path, encoding="utf-8-sig") as f:
            lines = f.read().split("\n")
        ns = ""
        current = None
        pending = None  # ('fact'|'theory', skip)
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
                full = (ns + "." + current) if current else ns
                key = full
                kind, skip = pending
                n = 1 if kind == "fact" else inline
                entry = totals.setdefault(key, [0, 0, 0, os.path.relpath(path, root).replace(os.sep, "/")])
                if n < 0:
                    entry[2] += 1
                else:
                    if skip:
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
            print("   ", k, sel[k])
