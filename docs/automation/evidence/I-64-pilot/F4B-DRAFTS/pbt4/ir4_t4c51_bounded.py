"""CA-T4c recheck: run bounded_check and the slice function of make_pbt4c_contract.py (extracted, not importing the generator) on sections
22 and 23 of independent-review-v1.md, the package v5.1 and the full BOUNDED line with the tail of the recheck (read-only).

Usage: python -I ir4_t4c51_bounded.py <make_pbt4c_contract.py> <independent-review-v1.md> <package-v51.md> <out.txt>
"""
import re
import sys

gen_p, rep_p, pkg_p, out_p = sys.argv[1:5]
gen = open(gen_p, "rb").read().decode("ascii")
a = gen.index("# ---- bounded_check (inicio)")
b = gen.index("# ---- bounded_check (fin)")
ns = {}
exec(compile(gen[a:b], "bounded_check", "exec"), ns)
m = re.search(r"^BOUNDED_SECTIONS = (\(.*?\))", gen, re.M)
sections = eval(m.group(1), {})
name = "cat_report" if "def cat_report(text):" in gen else "cat4c_report"
c = gen.index("def %s(text):" % name)
d = gen.index("\n\n\n", c)
ns2 = {"re": re, "BOUNDED_SECTIONS": sections}
exec(compile(gen[c:d], name, "exec"), ns2)
RESULT = ("EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 3; las importantes corregidas en el paquete v5.1 (I-01, los dos archivos nuevos de "
          "Draft/ empiezan por #nullable enable y la pureza de CT-PBT4C-15 lo prohibe: sin la directiva y sin el ? de ProjectOf); sin "
          "decisiones nuevas del Owner; precondicion 6 sobre 6adff90d: pasos 2 a 7 cumplen, sin las CT; recomprobacion v5.1: EMITIBLE, "
          "B 0 / I 0 / M 1")
rep, found = ns2[name](open(rep_p, "rb").read().decode("utf-8"))
pkg_lines = open(pkg_p, "rb").read().decode("utf-8").replace("\r\n", "\n").split("\n")
err = ns["bounded_check"](rep, RESULT, pkg_lines, reviewer="CA-T4c")
L = ["BOUNDED (%d chars, ascii %s); sections %s; found %s" % (len(RESULT), all(ord(x) < 128 for x in RESULT), sections, found),
     "bounded_check on the v5.1: %d error(s)" % len(err)] + ["  - " + e for e in err]
open(out_p, "w", encoding="ascii", errors="backslashreplace").write("\n".join(L) + "\n")
print("\n".join(L))
