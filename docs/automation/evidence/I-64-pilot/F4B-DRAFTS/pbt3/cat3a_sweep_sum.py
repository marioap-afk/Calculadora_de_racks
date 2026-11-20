import re
import sys
from collections import defaultdict

# Uso: python -I cat3a_sweep_sum.py <salida de cat3a_sweep.py> [grupo]
# Resume los aciertos SOLO en el texto nuevo, por grupo y por prueba, con sus literales distintos.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
lines = open(sys.argv[1], encoding="utf-8").read().split("\n")
want = sys.argv[2] if len(sys.argv) > 2 else None
group = None
agg = defaultdict(lambda: defaultdict(set))
for l in lines:
    m = re.match(r"===== (.*?) \| (SOLO|tambien)", l)
    if m:
        group = m.group(1) if m.group(2) == "SOLO" else None
        continue
    m = re.match(r"  (tests/\S+):(\d+)  lit=(.*)$", l)
    if m and group:
        agg[group][m.group(1)].add((m.group(3), m.group(2)))
for g, per in agg.items():
    if want and not g.startswith(want):
        continue
    print("=====", g, "| pruebas:", len(per))
    for f, lits in sorted(per.items(), key=lambda x: -len(x[1])):
        uniq = sorted(set(x[0] for x in lits))
        print("  %-95s %3d  %s" % (f, len(lits), " ; ".join(u[:60] for u in uniq[:12]) + (" ..." if len(uniq) > 12 else "")))
