import hashlib
import json
import subprocess
import sys

# Uso: python -I cat3c_r3.py <carpeta pbt3> <checkout> <rev>
# El seco r3 de PB-T3c frente al de antes (_ref/r3-before/): que solo cambien los RequiredTests, con las dos entradas de M-01 y nada mas; que el
# orden de lo heredado sea el del contrato reemitido de PB-T3b (en la rev); y los MinSelected frente a ese contrato. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
ra = open(HERE + "/_ref/r3-before/contract-dry-pbt3c.json", "rb").read()
rb = open(HERE + "/contract-dry-pbt3c.json", "rb").read()
for name, r in (("antes", ra), ("ahora", rb)):
    print(name, hashlib.sha256(r).hexdigest().upper()[:16], len(r), "ASCII:", all(c < 128 for c in r), "CR:", b"\r" in r)
a, b = json.loads(ra), json.loads(rb)
print("claves iguales y en orden:", list(a) == list(b))
print("campos que cambian:", [k for k in a if a[k] != b[k]])
ra_, rb_ = a["RequiredTests"], b["RequiredTests"]
print("RequiredTests:", len(ra_), "->", len(rb_))
added = [x for x in rb_ if x not in ra_]
removed = [x for x in ra_ if x not in rb_]
print("anadidas:", json.dumps(added, ensure_ascii=False))
print("quitadas:", removed)
print("lo de antes, en el mismo orden relativo:", [x for x in rb_ if x in ra_] == ra_)
print("posiciones de las anadidas:", [rb_.index(x) for x in added], "| vecinas:", [(rb_[rb_.index(x) - 1]["Filter"][-40:], rb_[rb_.index(x) + 1]["Filter"][-40:]) for x in added])
t3b = json.loads(subprocess.run(["git", "-C", REPO, "show", REV + ":docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json"],
                                capture_output=True, check=True).stdout.decode("utf-8-sig"))["RequiredTests"]
key = lambda x: (x["Project"], x["Filter"])
t3b_keys = [key(x) for x in t3b]
inh = [x for x in rb_ if key(x) in t3b_keys]
print("heredadas de PB-T3b presentes: %d de %d" % (len(inh), len(t3b)), "| faltan:", [k for k in t3b_keys if k not in [key(x) for x in rb_]])
print("orden de las heredadas = orden en PB-T3b:", [key(x) for x in inh] == [k for k in t3b_keys if k in [key(y) for y in inh]])
for x in inh:
    y = t3b[t3b_keys.index(key(x))]
    if x["MinSelected"] != y["MinSelected"] or x["ExpectRed"] is not False:
        print("   distinta de PB-T3b:", key(x)[1], "PB-T3c", x["MinSelected"], x["ExpectRed"], "| PB-T3b", y["MinSelected"], y["ExpectRed"])
early = [key(x) for x in rb_[:6] if key(x) in t3b_keys]
tail = [key(x) for x in rb_[6:]]
print("heredadas que PB-T3c pone en sus filas propias (las 6 primeras):", [k[1][-45:] for k in early])
print("el resto (desde la 7.a) = PB-T3b en su orden, sin esas:", tail == [k for k in t3b_keys if k not in early], "| fuera de PB-T3b:", [k for k in tail if k not in t3b_keys])
print("con ExpectRed true en PB-T3c:", [x["Filter"] for x in rb_ if x["ExpectRed"]])
print("con ExpectRed true en PB-T3b:", [(x["Filter"], x["MinSelected"]) for x in t3b if x["ExpectRed"]])
