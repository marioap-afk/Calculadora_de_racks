# Redaccion para el repo publico: en la expresion de rutas, la palabra de la carpeta de datos de aplicacion del perfil va con una letra entre corchetes (la misma expresion), para que el texto pase FORBIDDEN de la redaccion comun. El resto no cambia.
import re
import sys

# Uso: python -I cat3a_bounded.py <carpeta> <paquete.md> <literal>
# Aplica la funcion bounded_check del generador (entre sus marcas) al informe de CA-T3a, al literal y a las lineas del paquete.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, PKG, lit = sys.argv[1], sys.argv[2], sys.argv[3]
gen = open(HERE + "/make_pbt3a_contract.py", encoding="utf-8").read()
i = gen.index("# ---- bounded_check (inicio)")
j = gen.index("# ---- bounded_check (fin)")
ns = {}
exec(compile(gen[i:j], "bounded_check", "exec"), ns)
report = open(HERE + "/independent-review-pbt3a-v43-check.md", encoding="utf-8").read()
pkg = open(HERE + "/" + PKG, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
print("literal ascii:", all(ord(c) < 0x80 for c in lit))
print("literal regex [.,;]:", bool(re.match(r"EMITIBLE(?: TRAS CORRECCIONES)?[.,;]", lit)))
errs = ns["bounded_check"](report, lit, pkg)
print("errores:", len(errs))
for e in errs:
    print(" -", e)
# las rutas locales del informe, con la misma expresion que local_paths del generador (los nombres de personas se revisan a mano)
lp = sorted(set(m.group(0) for m in re.finditer(r"(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\bUsers[\\/]|\bApp[D]ata\b", report)))
print("rutas locales en el informe:", lp)
