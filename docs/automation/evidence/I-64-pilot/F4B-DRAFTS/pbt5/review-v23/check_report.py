"""Comprobacion acotada de PB-T5 (CA-T5): comprueba el propio informe con una copia de las reglas de check_bounded()
de emit_pbt5.py (v1 y v2), sin importar ni ejecutar ese guion.

Uso: python -I check_report.py <informe.md> [<paquete> <guia> <guion>]
Cuenta las lineas que empiezan por BOUNDED_RESULT: tras quitar espacios, exige que sea una sola y en ASCII, y dice
si la aceptarian las reglas de la v1 y, si se dan los tres archivos, las de la v2 (token EMITIBLE;, B 0 / I 0 y los
prefijos sha256 de 8 caracteres del paquete, la guia y el guion, una vez cada uno).
"""
import hashlib
import re
import sys

lines = [ln.rstrip("\n") for ln in open(sys.argv[1], encoding="utf-8")]
hits = [ln.strip() for ln in lines if ln.strip().startswith("BOUNDED_RESULT:")]
print("lineas BOUNDED_RESULT:", len(hits))
print("menciones de BOUNDED_RESULT en total:", sum(ln.count("BOUNDED_RESULT") for ln in lines))
for h in hits:
    print("ascii:", all(ord(c) < 128 for c in h), "| longitud:", len(h), "| comillas invertidas:", h.count("`"))
    v1 = "EMITIBLE" in h and re.search(r"\bB\s*0\s*/\s*I\s*0\b", h) and "NO EMITIBLE" not in h
    print("reglas v1 la aceptarian:", bool(v1))
    if len(sys.argv) == 5:
        want = {k: hashlib.sha256(open(p, "rb").read()).hexdigest() for k, p in zip(("paquete", "guia", "guion"), sys.argv[2:5])}
        ok = bool(re.match(r"BOUNDED_RESULT:\s*EMITIBLE;", h)) and bool(re.search(r";\s*B 0 / I 0(?=\s*(/|;|$))", h))
        words = {"paquete": r"paquete", "guia": r"gu[ií]a", "guion": r"gui[oó]n"}
        for k, full in want.items():
            found = re.findall(r"\b" + words[k] + r"\s+([0-9a-f]{8})\b", h)
            print(f"  {k}: en la linea {found}, esperado {full[:8]}")
            ok = ok and len(found) == 1 and found[0] == full[:8]
        print("reglas v2 la aceptarian:", ok)
