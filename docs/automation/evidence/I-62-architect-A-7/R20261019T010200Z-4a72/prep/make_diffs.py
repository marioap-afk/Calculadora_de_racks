"""Escribe en prep/diffs/ el diff unificado de cada componente del kit de A-7 frente al del kit acreditado de A-6 ronda 4 (copia verificada en
prep/base-a6r4/, manifiesto b1342909…), y el de prep/gen_schema.py frente al de la preparación de A-6 ronda 4. Imprime la tabla «componente →
SHA-256 anterior → nuevo → líneas añadidas/quitadas». No escribe fuera de prep/diffs/.
Uso: python -I -B make_diffs.py <carpeta que contiene kit/ y prep/>
"""
import difflib
import hashlib
import json
import os
import sys

ROOT = sys.argv[1]
KIT, BASE, OUT = os.path.join(ROOT, "kit"), os.path.join(ROOT, "prep", "base-a6r4"), os.path.join(ROOT, "prep", "diffs")
GEN_BASE = r"D:\r62-a6r4\kit-a6r4-review\prep\gen_schema.py"
os.makedirs(OUT, exist_ok=True)


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest() if os.path.isfile(p) else None


rows = []
pairs = [(n, os.path.join(BASE, n), os.path.join(KIT, n)) for n in sorted(set(os.listdir(KIT)) | set(os.listdir(BASE)))
         if os.path.isfile(os.path.join(KIT, n)) or os.path.isfile(os.path.join(BASE, n))]
pairs.append(("prep/gen_schema.py", GEN_BASE, os.path.join(ROOT, "prep", "gen_schema.py")))
for name, old, new in pairs:
    so, sn = sha(old), sha(new)
    row = {"File": name, "OldSha256": so, "NewSha256": sn, "Status": "IDENTICAL" if so == sn else ("NEW" if so is None else ("REMOVED" if sn is None else "CHANGED"))}
    if row["Status"] == "CHANGED" and (name.endswith((".py", ".md", ".txt")) and name != "prompt.md"):
        a = open(old, encoding="utf-8").read().split("\n")
        b = open(new, encoding="utf-8").read().split("\n")
        d = list(difflib.unified_diff(a, b, "a6r4/" + name, "a7/" + name, lineterm=""))
        row.update({"Added": sum(1 for l in d if l.startswith("+") and not l.startswith("+++")),
                    "Removed": sum(1 for l in d if l.startswith("-") and not l.startswith("---"))})
        with open(os.path.join(OUT, name.replace("/", "_") + ".diff"), "w", encoding="utf-8", newline="\n") as f:
            f.write("\n".join(d) + "\n")
    rows.append(row)
for r in rows:
    print("%-34s %-9s %s -> %s %s" % (r["File"], r["Status"], (r["OldSha256"] or "-")[:12], (r["NewSha256"] or "-")[:12],
                                      ("+%d/-%d" % (r["Added"], r["Removed"])) if "Added" in r else ""))
with open(os.path.join(OUT, "diff-table.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(rows, ensure_ascii=False, indent=1) + "\n")
