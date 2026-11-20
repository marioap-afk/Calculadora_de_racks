"""CA-T4a: run bounded_check of make_pbt4a_contract.py (extracted between its markers, not importing the generator) on the
report and the package, with the proposed BOUNDED_RESULT (read-only).

Usage: python -I cat4a_bounded.py <make_pbt4a_contract.py> <report.md> <package.md> <out.txt>
Two runs: the package as it is, and the package with one injected change-log row "| **V43-01** | **CA-T4a I-01** ... |"
(in memory only), to show what the literal needs once the package corrects I-01.
"""
import sys

gen_p, rep_p, pkg_p, out_p = sys.argv[1:5]
gen = open(gen_p, "rb").read().decode("ascii")
a = gen.index("# ---- bounded_check (inicio)")
b = gen.index("# ---- bounded_check (fin)")
ns = {}
exec(compile(gen[a:b], "bounded_check", "exec"), ns)
bounded_check = ns["bounded_check"]

RESULT = ("EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; las importantes corregidas en el paquete v4.3 (I-01, la anotacion string? de "
          "PushBackOriginByText.cs: aviso CS8632 que rompe build-plugin); sin decisiones nuevas del Owner; remedicion sobre 7f0d9aa3 sin "
          "cambios materiales")
assert all(ord(c) < 128 for c in RESULT)
report = open(rep_p, "rb").read().decode("utf-8")
pkg_lines = open(pkg_p, "rb").read().decode("utf-8").replace("\r\n", "\n").split("\n")
L = ["BOUNDED_RESULT (%d chars, ascii): %s" % (len(RESULT), RESULT)]
e1 = bounded_check(report, RESULT, pkg_lines, reviewer="CA-T4a")
L.append("v4.2 as is: %d error(s)" % len(e1))
L.extend("  - " + e for e in e1)
injected = pkg_lines + ["| **V43-01** | **CA-T4a I-01** | la anotacion de Choose y el molde de AtKey | D-PBT4-03; section 6.1 |"]
e2 = bounded_check(report, RESULT, injected, reviewer="CA-T4a")
L.append("with a V43-01 row naming CA-T4a I-01: %d error(s)" % len(e2))
L.extend("  - " + e for e in e2)
open(out_p, "w", encoding="ascii", errors="backslashreplace").write("\n".join(L) + "\n")
print("\n".join(L))
