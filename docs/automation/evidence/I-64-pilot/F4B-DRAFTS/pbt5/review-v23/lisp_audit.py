"""Comprobacion acotada de PB-T5 (CA-T5): mide, sin ejecutar nada, las lineas de los bloques ```lisp de la guia.

Uso: python -I lisp_audit.py <guia.md>
Por linea que empieza por '(': longitud, profundidad final de parentesis fuera de cadenas y comillas sin cerrar,
y el numero de trozos de 16 caracteres que hacen falta para teclearla.
"""
import sys

text = open(sys.argv[1], encoding="utf-8").read().split("\n")
inside = False
n = bad = 0
for no, ln in enumerate(text, 1):
    s = ln.strip()
    if s.startswith("```"):
        inside = (s == "```lisp") if not inside else False
        continue
    if not inside or not s.startswith("("):
        continue
    n += 1
    depth = 0
    in_str = esc = False
    neg = False
    for ch in s:
        if in_str:
            if esc:
                esc = False
            elif ch == "\\":
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
                neg = True
    ok = depth == 0 and not in_str and not neg and len(s) <= 1024
    bad += 0 if ok else 1
    print(f"linea {no}: len={len(s)} trozos16={-(-len(s) // 16)} profundidad={depth} cadena_abierta={in_str} {'OK' if ok else 'MAL'}")
print(f"lineas LISP: {n}; mal: {bad}")
