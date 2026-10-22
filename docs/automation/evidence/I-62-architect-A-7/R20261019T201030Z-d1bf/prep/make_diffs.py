"""Fase 1 (decisiones §94, punto 5): escribe en prep/diffs/ el diff unificado de cada componente de kit/ frente al del kit LANZADO de A-7
(copia verificada en base-launched/kit/, manifiesto c27db93e…, igual blob a blob a la custodia de e71400a2), e imprime la tabla «componente →
SHA-256 anterior → nuevo → estado → líneas añadidas/quitadas». No escribe fuera de prep/diffs/.
Uso: python -I -B make_diffs.py <kit-review2>
"""
import difflib
import hashlib
import json
import os
import sys

ROOT = sys.argv[1]
KIT, BASE, OUT = os.path.join(ROOT, "kit"), os.path.join(ROOT, "base-launched", "kit"), os.path.join(ROOT, "prep", "diffs")
os.makedirs(OUT, exist_ok=True)


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest() if os.path.isfile(p) else None


rows = []
for name in sorted(set(os.listdir(KIT)) | set(os.listdir(BASE))):
    old, new = os.path.join(BASE, name), os.path.join(KIT, name)
    if not (os.path.isfile(old) or os.path.isfile(new)):
        continue
    so, sn = sha(old), sha(new)
    row = {"File": name, "OldSha256": so, "NewSha256": sn,
           "Status": "IDENTICAL" if so == sn else ("NEW" if so is None else ("REMOVED" if sn is None else "CHANGED"))}
    if row["Status"] in ("CHANGED", "NEW") and name.endswith((".py", ".md")) and name != "prompt.md":
        a = open(old, encoding="utf-8").read().split("\n") if so else []
        b = open(new, encoding="utf-8").read().split("\n")
        d = list(difflib.unified_diff(a, b, "launched/" + name, "phase1/" + name, lineterm=""))
        row.update({"Added": sum(1 for l in d if l.startswith("+") and not l.startswith("+++")),
                    "Removed": sum(1 for l in d if l.startswith("-") and not l.startswith("---"))})
        with open(os.path.join(OUT, name + ".diff"), "w", encoding="utf-8", newline="\n") as f:
            f.write("\n".join(d) + "\n")
    rows.append(row)
for r in rows:
    print("%-30s %-9s %s -> %s %s" % (r["File"], r["Status"], (r["OldSha256"] or "-")[:12], (r["NewSha256"] or "-")[:12],
                                      ("+%d/-%d" % (r["Added"], r["Removed"])) if "Added" in r else ""))
with open(os.path.join(OUT, "diff-table.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(rows, ensure_ascii=False, indent=1) + "\n")
