import json
import re
import subprocess
import sys

# Uso: python -I cat3c_auth.py <carpeta pbt3> <checkout> <rev>
# Las autoridades del seco de PB-T3c: cuales existen ya en la rev (por objetos git) y cuales trae la emision; las citas F4B-Enn de cada
# seccion frente a la ultima entrada de f4b-dispositions.md en la rev; y las anclas ruta:linea de las secciones, con su linea en la rev.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
c = json.load(open(HERE + "/contract-dry-pbt3c.json", encoding="utf-8"))


def exists(path):
    return subprocess.run(["git", "-C", REPO, "cat-file", "-e", REV + ":" + path], capture_output=True).returncode == 0


disp = subprocess.run(["git", "-C", REPO, "show", REV + ":docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md"],
                      capture_output=True, check=True).stdout.decode("utf-8", "replace")
last = max(int(n) for n in re.findall(r"^## F4B-E(\d+)\b", disp, re.M))
print("ultima entrada de f4b-dispositions.md en la rev: F4B-E%02d" % last)
for n, a in enumerate(c["Authorities"]):
    p = a["Path"]
    cites = sorted(set(int(x) for x in re.findall(r"F4B-E(\d+)", a.get("Section", ""))))
    later = [x for x in cites if x > last]
    print("[%02d] %-7s %s%s" % (n, "existe" if exists(p) else "NUEVA", p, ("  cita F4B-E%s, posterior al cierre" % later) if later else ""))
