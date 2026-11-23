"""Comprueba los entregables de PB-T5: sha256, CR, NUL, patrones prohibidos y el LISP de la guia.

Los patrones se leen de patterns.local.txt y los literales admitidos de allow.local.txt,
junto a este guion; ninguno de los dos se custodia. El guion se analiza tambien a si mismo.
En la guia, cada linea LISP de un bloque ```lisp tiene que medir 1024 caracteres o menos y tener
los parentesis equilibrados fuera de las cadenas.
Sale con 1 si hay alguna coincidencia no admitida, un NUL, una linea LISP mala o falta un archivo.
Los nombres de la lista que acaban en «(*)» se informan sin contar como fallo: llevan, a proposito,
rutas escritas como literales de Python que la redaccion de la emision no tapa, y van solo por sha256.
"""
import hashlib
import os
import re
import sys

here = os.path.dirname(os.path.abspath(__file__))
base = os.path.dirname(here)
names = [
    "I-64-f4b-pbt5-package-v2.4.md",
    "smoke-run-v2.md",
    "handoff-pbt5-v4.md",
    "_tools/emit_pbt5.py",
    "_tools/emit_pbt5-v1.py",
    "_tools/test_emit_pure.py",
    "_tools/harness_sr2.ps1",
    "_tools/scan_custody.py",
    "_tools/probe_selection_shape.py",
    "_tools/probe_selection_vs_s2.py",
    "I-64-f4b-pbt5-package-v2.3.md",
    "smoke-run-v1.md",
    "handoff-pbt5-v3.md",
    "_tools/make_pkg_v23.py",
    "_tools/fill_t4e_pkg_v23.py",
    "_tools/pkg_v23_head.md",
    "_tools/pkg_v23_tail.md",
    "_tools/sr_head.md",
    "_tools/sr_new4.md",
    "_tools/sr_56.md",
    "_tools/make_smoke_run_v1.py(*)",
    "_tools/list_ui_filters.py",
    "_tools/syntax_check.py",
    "_tools/ps_parse_any.ps1",
    "_tools/control_patterns.py",
]
guide = "smoke-run-v2.md"
result_file = "sha256-v4.txt"


def read_lines(name):
    return [ln.rstrip("\n") for ln in open(os.path.join(here, name), encoding="utf-8") if ln.strip()]


compiled = [re.compile(p) for p in read_lines("patterns.local.txt")]
allowed = read_lines("allow.local.txt")


def lisp_problems(text):
    probs = []
    inside = False
    n = 0
    for no, ln in enumerate(text.split(chr(10)), 1):
        s = ln.strip()
        if s.startswith("```"):
            inside = (s == "```lisp") if not inside else False
            continue
        if not inside or not s.startswith("("):
            continue
        n += 1
        if len(s) > 1024:
            probs.append(f"linea {no}: {len(s)} caracteres")
        depth = 0
        in_str = False
        esc = False
        for ch in s:
            if in_str:
                if esc:
                    esc = False
                elif ch == chr(92):
                    esc = True
                elif ch == '"':
                    in_str = False
                continue
            if ch == '"':
                in_str = True
            elif ch == "(":
                depth += 1
            elif ch == ")":
                depth -= 1
                if depth < 0:
                    break
        if depth != 0 or in_str:
            probs.append(f"linea {no}: parentesis o comillas sin cerrar")
    return n, probs


out = []
bad = 0
targets = [(n.replace("(*)", ""), n.endswith("(*)")) for n in names] + [("_tools/check_pbt5.py", False)]
for name, informative in targets:
    p = os.path.join(base, name)
    if not os.path.exists(p):
        out.append(f"MISSING  {name}")
        bad += 1
        continue
    data = open(p, "rb").read()
    text = data.decode("utf-8")
    masked = text
    n_allowed = 0
    for a in allowed:
        n_allowed += masked.count(a)
        masked = masked.replace(a, "X" * len(a))
    hits = []
    for rx in compiled:
        for m in rx.finditer(masked):
            hits.append((masked.count(chr(10), 0, m.start()) + 1, m.group(0)))
    h = hashlib.sha256(data).hexdigest()
    nuls = data.count(bytes([0]))
    out.append(f"{h}  {name}  bytes={len(data)} lines={text.count(chr(10))} cr={data.count(bytes([13]))} nul={nuls} hits={len(hits)} admitidos={n_allowed}{' (informativo)' if informative else ''}")
    for line, g in hits:
        out.append(f"    HIT {name}:{line}")  # sin el texto que casa, para que este resultado se pueda custodiar
    if not informative:
        bad += len(hits)
    bad += nuls
    if name == guide:
        nl, lp = lisp_problems(text)
        out.append(f"    LISP: {nl} lineas; {'OK' if not lp else '; '.join(lp)}")
        bad += len(lp)

res = "\n".join(out) + "\n"
open(os.path.join(base, result_file), "x", encoding="utf-8", newline="\n").write(res)
sys.stdout.write(res)
sys.exit(1 if bad else 0)
