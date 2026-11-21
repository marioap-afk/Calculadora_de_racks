"""CA-T4c: run bounded_check and cat4c_report of make_pbt4c_contract.py (extracted, not importing the generator) on section 22 of
independent-review-v1.md, the package v5.0 and the proposed BOUNDED line; and once more with one injected change-log row naming
CA-T4c I-01 (in memory), to show what the line needs once the package corrects I-01 (read-only).

Usage: python -I ir4_t4c_bounded.py <make_pbt4c_contract.py> <independent-review-v1.md> <package-v50.md> <out.txt>
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
m = re.search(r"^BOUNDED_SECTIONS = (\(.*?\))", gen, re.M)
sections = eval(m.group(1), {})
c = gen.index("def cat_report(text):") if "def cat_report(text):" in gen else gen.index("def cat4c_report(text):")
d = gen.index("\n\n\n", c)
ns2 = {"re": re, "BOUNDED_SECTIONS": sections}
exec(compile(gen[c:d], "cat4c_report", "exec"), ns2)
RESULT = ("EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 3; las importantes corregidas en el paquete v5.1 (I-01, los dos archivos nuevos de "
          "Draft/ empiezan por #nullable enable y la pureza de CT-PBT4C-15 lo prohibe: sin la directiva y sin el ? de ProjectOf); sin "
          "decisiones nuevas del Owner; precondicion 6 sobre 6adff90d: pasos 2 a 7 cumplen, sin las CT")
L = ["BOUNDED (%d chars, ascii %s); sections %s" % (len(RESULT), all(ord(x) < 128 for x in RESULT), sections)]
rep, found = (ns2.get("cat_report") or ns2["cat4c_report"])(open(rep_p, "rb").read().decode("utf-8"))
L.append("cat4c_report: sections found %s; slice %d lines" % (found, len((rep or "").split("\n"))))
pkg_lines = open(pkg_p, "rb").read().decode("utf-8").replace("\r\n", "\n").split("\n")
e1 = bounded_check(rep, RESULT, pkg_lines, reviewer="CA-T4c")
L.append("v5.0 as is: %d error(s)" % len(e1))
L.extend("  - " + e for e in e1)
e2 = bounded_check(rep, RESULT, pkg_lines + ["| **V51-01** | **CA-T4c I-01** | sin #nullable en Draft/ | anexo |"], reviewer="CA-T4c")
L.append("with a V51-01 row naming CA-T4c I-01: %d error(s)" % len(e2))
L.extend("  - " + e for e in e2)
open(out_p, "w", encoding="ascii", errors="backslashreplace").write("\n".join(L) + "\n")
print("\n".join(L))
