import sys

# Uso: python -I cat3b_title.py <carpeta> <paquete.md> <literal>
# El bounded_check de make_pbt3b_contract.py sobre el informe de CA-T3b con el titulo de la seccion 12 que pedia el encargo, cambiado solo en memoria.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, PKG, lit = sys.argv[1], sys.argv[2], sys.argv[3]
gen = open(HERE + "/make_pbt3b_contract.py", encoding="utf-8").read()
ns = {}
exec(compile(gen[gen.index("# ---- bounded_check (inicio)"):gen.index("# ---- bounded_check (fin)")], "bounded_check", "exec"), ns)
report = open(HERE + "/independent-review-pbt3b-check.md", encoding="utf-8").read()
old = "## " + chr(167) + "12. Recomprobaci" + chr(243) + "n sobre la v4.5: el seco r2"
new = "## " + chr(167) + "12. Recomprobaci" + chr(243) + "n sobre el seco r2"
assert report.count(old) == 1
pkg = open(HERE + "/" + PKG, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
for e in ns["bounded_check"](report.replace(old, new), lit, pkg, reviewer="CA-T3b"):
    print(" -", e)
