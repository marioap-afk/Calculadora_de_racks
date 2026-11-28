import hashlib
import json
import re
import subprocess
import sys

# Uso: python -I cat3c_dry.py <carpeta pbt3> <checkout> <rev del cierre de PB-T3b>
# El seco de PB-T3c, campo a campo, frente al contrato reemitido de PB-T3b (en git, en la rev): lo comun con el prefijo cambiado,
# lo propio, y los recuentos. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
P = "docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json"
rb = subprocess.run(["git", "-C", REPO, "show", REV + ":" + P], capture_output=True, check=True).stdout
rc = open(HERE + "/contract-dry-pbt3c.json", "rb").read()
print("PB-T3b reemitido", hashlib.sha256(rb).hexdigest().upper()[:16], len(rb), "| seco PB-T3c", hashlib.sha256(rc).hexdigest().upper()[:16], len(rc),
      "| ASCII:", all(c < 128 for c in rc), "| CR:", b"\r" in rc)
b, c = json.loads(rb.decode("utf-8-sig")), json.loads(rc.decode("utf-8"))
print("claves PB-T3b:", list(b))
print("claves PB-T3c:", list(c))
for k in c:
    v = c[k]
    if isinstance(v, (str, int, float, bool)) or v is None:
        s = str(v)
        print("== %s: %s%s" % (k, s[:700], " ..." if len(s) > 700 else ""), "| igual a PB-T3b" if b.get(k) == v else "")
    else:
        print("== %s: %s, %d entradas (PB-T3b: %s)" % (k, type(v).__name__, len(v), len(b.get(k, [])) if isinstance(b.get(k), list) else "-"))


def norm(s):
    return s.replace("PBT3B", "PBT3X").replace("PBT3C", "PBT3X").replace("PB-T3b", "PB-T3x").replace("PB-T3c", "PB-T3x")


def ident(x):
    if isinstance(x, dict):
        for key in ("Id", "Filter", "Path"):
            if key in x:
                return str(x[key])
    if isinstance(x, str):
        m = re.match(r"([A-Z]+-[A-Z0-9]+-\d+|PBT3[A-Z]-C\d+)", x)
        if m:
            return m.group(1)
    return None


for k in ("Invariants", "StopConditions"):
    print("== %s" % k)
    bmap = {}
    for x in b.get(k, []):
        t = json.dumps(x, ensure_ascii=False, sort_keys=True)
        i = ident(x)
        bmap[norm(i) if i else None] = norm(t)
    for x in c[k]:
        t = json.dumps(x, ensure_ascii=False, sort_keys=True)
        i = ident(x)
        same = bmap.get(norm(i) if i else None) == norm(t)
        print("   %-14s %s %s" % (i, "COMUN-IGUAL" if same else ("propio/cambia" if norm(i) in bmap else "nuevo"), t[:260]))
print("== RequiredTests")
for x in c["RequiredTests"]:
    print("   ", json.dumps(x, ensure_ascii=False)[:230])
print("== ExpectedEvidence")
for n, x in enumerate(c["ExpectedEvidence"]):
    s = x if isinstance(x, str) else json.dumps(x, ensure_ascii=False)
    eq = [m for m, y in enumerate(b["ExpectedEvidence"]) if y == x]
    print("   [%d] %s%s" % (n, "(= PB-T3b[%s]) " % eq if eq else "", s[:420]))
print("== Authorities")
for n, x in enumerate(c["Authorities"]):
    inb = any(y == x for y in b["Authorities"])
    print("   [%02d] %s %s %s | %s" % (n, "=T3b" if inb else "new ", x.get("Class"), x.get("Path"), str(x.get("Section"))[:150]))
for k in ("AllowedWriteScope", "ForbiddenWriteScope"):
    v = c.get(k, [])
    print("== %s (%d): %s" % (k, len(v), "; ".join(str(e) for e in v[:6]) + (" ..." if len(v) > 6 else "")))
fb = set(map(str, b.get("ForbiddenWriteScope", []))); fc = set(map(str, c.get("ForbiddenWriteScope", [])))
print("   forbidden: en PB-T3c y no en PB-T3b:", sorted(fc - fb)[:40])
print("   forbidden: en PB-T3b y no en PB-T3c:", sorted(fb - fc)[:40])
mk = sorted(set(re.findall(r"<[^<>\n]{3,80}>", rc.decode("utf-8"))))
print("== marcadores <...> en el seco:", mk)
