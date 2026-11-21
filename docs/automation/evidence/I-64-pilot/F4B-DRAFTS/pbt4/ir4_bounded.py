"""CA-T4b recheck: run bounded_check and cat4b_report of make_pbt4b_contract.py (extracted, not importing the generator) on the
sections 20 and 21 of independent-review-v1.md, the package v4.9 and the proposed BOUNDED line (read-only).

Usage: python -I ir4_bounded.py <make_pbt4b_contract.py> <independent-review-v1.md> <package-v49.md> <out.txt>
"""
import re
import sys

gen_p, rep_p, pkg_p, out_p = sys.argv[1:5]
gen = open(gen_p, "rb").read().decode("ascii")
a = gen.index("# ---- bounded_check (inicio)")
b = gen.index("# ---- bounded_check (fin)")
ns = {}
exec(compile(gen[a:b], "bounded_check", "exec"), ns)
bounded_check = ns["bounded_check"]
# cat4b_report, as the generator has it, with its BOUNDED_SECTIONS
m = re.search(r"^BOUNDED_SECTIONS = (\(.*?\))$", gen, re.M)
sections = eval(m.group(1), {})
c = gen.index("def cat4b_report(text):")
d = gen.index("\n\n\n", c)
ns2 = {"re": re, "BOUNDED_SECTIONS": sections}
exec(compile(gen[c:d], "cat4b_report", "exec"), ns2)
RESULT = ("EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 8; las importantes corregidas en el paquete v4.9 (I-01, la autoprueba de la guarda de "
          "los textos del anfitrion pasa en vacio en el GREEN: el equivalente de NoneOfThe30 trae su autoprueba sobre la tabla nueva, como G-4); "
          "sin decisiones nuevas del Owner salvo el coste de la lectura (pb-20-ab x4, origen-es-Primary, primera corrida); precondicion 6 sobre "
          "78a7cef3: pasos 1 a 6 cumplen; recomprobacion v4.9: EMITIBLE, B 0 / I 0 / M 3")
L = ["BOUNDED (%d chars, ascii %s)" % (len(RESULT), all(ord(x) < 128 for x in RESULT)), "sections %s" % (sections,)]
report_all = open(rep_p, "rb").read().decode("utf-8")
rep, found = ns2["cat4b_report"](report_all)
L.append("cat4b_report: sections found %s; slice %d lines" % (found, len((rep or "").split("\n"))))
pkg_lines = open(pkg_p, "rb").read().decode("utf-8").replace("\r\n", "\n").split("\n")
err = bounded_check(rep, RESULT, pkg_lines, reviewer="CA-T4b")
L.append("bounded_check: %d error(s)" % len(err))
L.extend("  - " + e for e in err)
open(out_p, "w", encoding="ascii", errors="backslashreplace").write("\n".join(L) + "\n")
print("\n".join(L))
