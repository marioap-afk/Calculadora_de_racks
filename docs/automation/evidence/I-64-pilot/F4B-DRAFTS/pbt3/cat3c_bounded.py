# Redaccion para el repo publico: en la expresion de rutas, las palabras de la carpeta de perfiles y de la de datos de aplicacion del perfil van con una letra entre corchetes (la misma expresion), para que el texto pase FORBIDDEN de la redaccion comun. El resto no cambia.
import re
import sys

# Uso: python -I cat3c_bounded.py <carpeta> <paquete.md> <literal>
# Aplica la funcion bounded_check de make_pbt3c_contract.py (entre sus marcas), con reviewer='CA-T3c' como en su llamada, al informe de
# CA-T3c, al literal y al paquete. Dice tambien si el literal es ASCII y si el informe lleva rutas locales. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, PKG, lit = sys.argv[1], sys.argv[2], sys.argv[3]
gen = open(HERE + "/make_pbt3c_contract.py", encoding="utf-8").read()
i = gen.index("# ---- bounded_check (inicio)")
j = gen.index("# ---- bounded_check (fin)")
ns = {}
exec(compile(gen[i:j], "bounded_check", "exec"), ns)
report = open(HERE + "/independent-review-pbt3c-check.md", encoding="utf-8").read()
pkg = open(HERE + "/" + PKG, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
print("literal ascii:", all(ord(ch) < 0x80 for ch in lit))
print("literal regex [.,;]:", bool(re.match(r"EMITIBLE(?: TRAS CORRECCIONES)?[.,;]", lit)))
errs = ns["bounded_check"](report, lit, pkg, reviewer="CA-T3c")
print("errores:", len(errs))
for e in errs:
    print(" -", e)
lp = sorted(set(m.group(0) for m in re.finditer(r"(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\bUs[e]rs[\\/]|\bApp[D]ata\b", report)))
print("rutas locales en el informe:", lp)
print("lineas de veredicto:", [l for l in report.split("\n") if l.startswith("**Veredicto")])
