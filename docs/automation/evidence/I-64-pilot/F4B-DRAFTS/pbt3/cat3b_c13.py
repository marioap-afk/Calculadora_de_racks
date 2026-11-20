import difflib
import json
import re
import sys

# Uso: python -I cat3b_c13.py <carpeta pbt3> <paquete.md> [<json del seco>]
# PBT3B-C13, C14 y C16 del seco frente al texto del paragrafo 6.2 del paquete (sin comillas invertidas ni vinetas), con el diff de lo que cambie.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, PKG = sys.argv[1], sys.argv[2]
DRY = sys.argv[3] if len(sys.argv) > 3 else "contract-dry-pbt3b.json"
c = json.loads(open(HERE + "/" + DRY, encoding="utf-8").read())
stops = {s["Id"]: s["Text"] for s in c["StopConditions"]}
lines = open(HERE + "/" + PKG, encoding="utf-8").read().replace("\r\n", "\n").split("\n")
i = next(k for k, l in enumerate(lines) if l.startswith("### 6.2"))
j = next(k for k, l in enumerate(lines) if l.startswith("### 6.3"))
sec = lines[i:j]


def para(start_marker, stop_pred):
    k = next(n for n, l in enumerate(sec) if start_marker in l)
    out = [sec[k]]
    for l in sec[k + 1:]:
        if stop_pred(l):
            break
        out.append(l)
    return out


def fold(ls):
    t = " ".join(re.sub(r"^\s*-\s+", "", x).strip() for x in ls if x.strip())
    t = t.replace("`", "").replace("**", "")
    t = t.replace("§", "section ")
    return re.sub(r"\s+", " ", t)


c13 = para("**PBT3B-C13**", lambda l: l.strip().startswith("- **Nota"))
pk = fold(c13)
pk = pk.split(": ", 1)[1] if ": " in pk else pk
x = stops["PBT3B-C13"]
print("C13 igual:", pk == x, len(pk), len(x))
if pk != x:
    sm = difflib.SequenceMatcher(None, pk, x, autojunk=False)
    for op, a1, a2, b1, b2 in sm.get_opcodes():
        if op != "equal":
            print("   ", op, repr(pk[a1:a2][:200]), "->", repr(x[b1:b2][:200]))
for cid, marker in (("PBT3B-C14", "**PBT3B-C14:**"), ("PBT3B-C16", "**PBT3B-C16:**")):
    t = fold([next(l for l in sec if marker in l)]).split(": ", 1)[1]
    print(cid, "igual:", t == stops[cid], "|", t[:160])
