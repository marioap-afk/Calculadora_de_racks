import difflib
import hashlib
import json
import sys

# Uso: python -I cat3b_r2.py <carpeta pbt3> <script nc2 v2>
# El seco original frente al r2, campo a campo y entrada a entrada, independiente de delta-r2.txt; y el SHA de C11 frente al script v2.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, V2 = sys.argv[1], sys.argv[2]
ra = open(HERE + "/contract-dry-pbt3b.json", "rb").read()
rb = open(HERE + "/contract-dry-pbt3b-r2.json", "rb").read()
print("original", hashlib.sha256(ra).hexdigest().upper()[:16], len(ra), "| r2", hashlib.sha256(rb).hexdigest().upper()[:16], len(rb), "| r2 ASCII:", all(c < 128 for c in rb))
a, b = json.loads(ra.decode("utf-8")), json.loads(rb.decode("utf-8"))
print("claves iguales:", list(a) == list(b))
for k in a:
    if a[k] == b.get(k):
        continue
    print("== cambia:", k)
    if isinstance(a[k], list):
        sa = [json.dumps(x, ensure_ascii=False, sort_keys=True) for x in a[k]]
        sb = [json.dumps(x, ensure_ascii=False, sort_keys=True) for x in b[k]]
        print("   entradas:", len(sa), "->", len(sb))
        for line in difflib.unified_diff(sa, sb, lineterm="", n=0):
            if line.startswith(("---", "+++")):
                continue
            print("   ", line[:500])
for k in b:
    if k not in a:
        print("== nueva clave:", k)
v2sha = hashlib.sha256(open(V2, "rb").read()).hexdigest()
c11 = next(s["Text"] for s in b["StopConditions"] if s["Id"] == "PBT3B-C11")
c11a = next(s["Text"] for s in a["StopConditions"] if s["Id"] == "PBT3B-C11")
print("sha del v2:", v2sha, "| en C11 r2:", v2sha in c11, "| marcador en C11 r2:", "por rellenar" in c11)
print("C11: solo cambia el marcador por el sha:", c11a.replace("<por rellenar: el SHA-256 de la v2 revisada>", v2sha) == c11)
auth = [x for x in b["Authorities"] if "nc2-substitute-f4b-v2" in x["Path"]]
for x in auth:
    print("autoridad:", x["Path"], "|", x["Class"])
    print("    ", x["Section"])
print("marcadores en r2:", [x["Path"] for x in b["Authorities"] if "por rellenar" in x["Section"] or "<" in x["Section"]],
      "| en paradas:", [s["Id"] for s in b["StopConditions"] if "por rellenar" in s["Text"]])
