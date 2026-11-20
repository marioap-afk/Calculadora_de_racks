import hashlib
import re
import subprocess
import sys

# Uso: python -I cat3c_fill.py <carpeta pbt3> <checkout> <rev>
# Reproduce, sin el emisor, el registro declarado que escribiria X: el de la rev, con las cinco sustituciones de superseding-pbt3b-close-fill.md
# (leidas con una expresion propia) y la entrada de PB-T3c al final (superseding-pbt3c.md con FECHA y CLOSE8). Dice que lineas cambian, cuantas
# veces esta cada linea vieja y nueva, y el tamano final. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
P = "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md"
sup0 = subprocess.run(["git", "-C", REPO, "show", REV + ":" + P], capture_output=True, check=True).stdout.decode("utf-8")
fill = open(HERE + "/superseding-pbt3b-close-fill.md", encoding="utf-8").read()
print("relleno sha256", hashlib.sha256(fill.encode("utf-8")).hexdigest()[:16], "| CR en el relleno:", "\r" in fill)
blocks = re.findall(r"```text\n(.*?)\n```", fill, re.S)
pairs = list(zip(blocks[0::2], blocks[1::2]))
print("bloques de texto:", len(blocks), "-> pares:", len(pairs), "| cada bloque de una linea:", all("\n" not in b for b in blocks))
lines = sup0.split("\n")
sec = [i for i, l in enumerate(lines) if l.startswith("## PB-T3b ")]
end = next(j for j in range(sec[0] + 1, len(lines)) if lines[j].startswith("## ")) if any(l.startswith("## ") for l in lines[sec[0] + 1:]) else len(lines)
print("seccion de PB-T3b: lineas %d a %d" % (sec[0] + 1, end))
new = list(lines)
for old, nw in pairs:
    idx = [i for i, l in enumerate(lines) if l == old]
    print("  vieja x%d en %s | nueva ya en el cierre x%d | %s" % (len(idx), [i + 1 for i in idx], sum(l == nw for l in lines), old[:60]))
    assert len(idx) == 1 and sec[0] < idx[0] < end
    new[idx[0]] = nw
diff = [i + 1 for i in range(len(lines)) if lines[i] != new[i]]
print("lineas cambiadas:", diff)
tpl = open(HERE + "/superseding-pbt3c.md", encoding="utf-8").read().replace("{FECHA}", "2026-11-20").replace("{CLOSE8}", REV[:8])
out = "\n".join(new).rstrip("\n") + "\n" + tpl
print("marcas que quedan en la entrada:", sorted(set(re.findall(r"\{[A-Z0-9_]+\}", tpl))))
print("tamano: cierre %d bytes -> con las sustituciones y la entrada %d bytes" % (len(sup0.encode("utf-8")), len(out.encode("utf-8"))))
print("las nuevas, en orden:")
for i in diff:
    print("  %d: %s" % (i, new[i - 1][:150]))
