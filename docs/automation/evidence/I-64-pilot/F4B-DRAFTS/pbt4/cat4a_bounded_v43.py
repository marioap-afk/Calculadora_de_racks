"""CA-T4a (recheck on v4.3): run bounded_check of make_pbt4a_contract.py (extracted between its markers) on the report and
the v4.3 package with the final BOUNDED_RESULT (read-only).

Usage: python -I cat4a_bounded_v43.py <make_pbt4a_contract.py> <report.md> <package-v43.md> <out.txt>
"""
import sys

gen_p, rep_p, pkg_p, out_p = sys.argv[1:5]
gen = open(gen_p, "rb").read().decode("ascii")
a = gen.index("# ---- bounded_check (inicio)")
b = gen.index("# ---- bounded_check (fin)")
ns = {}
exec(compile(gen[a:b], "bounded_check", "exec"), ns)
RESULT = ("EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; las importantes corregidas en el paquete v4.3 (I-01, la anotacion string? de "
          "PushBackOriginByText.cs: aviso CS8632 que rompe build-plugin); sin decisiones nuevas del Owner; remedicion sobre 7f0d9aa3 sin "
          "cambios materiales; recomprobacion v4.3: EMITIBLE, B 0 / I 0 / M 2")
import re
assert all(ord(c) < 128 for c in RESULT) and re.match(r"EMITIBLE(?: TRAS CORRECCIONES)?[.,;]", RESULT)
report = open(rep_p, "rb").read().decode("utf-8")
pkg_lines = open(pkg_p, "rb").read().decode("utf-8").replace("\r\n", "\n").split("\n")
errors = ns["bounded_check"](report, RESULT, pkg_lines, reviewer="CA-T4a")
L = ["BOUNDED_RESULT (%d chars, ascii): %s" % (len(RESULT), RESULT), "v4.3: %d error(s)" % len(errors)] + ["  - " + e for e in errors]
open(out_p, "w", encoding="ascii", errors="backslashreplace").write("\n".join(L) + "\n")
print("\n".join(L))
